﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autarkysoft.Bitcoin.Encoders
{
    /// <summary>
    /// A checksummed base32 format for native v0-16 witness outputs.
    /// <para/>https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki
    /// <para/>https://github.com/bitcoin/bips/blob/master/bip-0350.mediawiki
    /// </summary>
    public static class Bech32
    {
        /// <summary>
        /// Encoding mode (affects checksum)
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// BIP-173 or Bech32 mode. This mode has a weakness when encoding non-strict data lengths (witness version 1+).
            /// </summary>
            B32,
            /// <summary>
            /// BIP-350 or Bech32m mode. This mode fixes the BIP-173 weakness and should be used for witness version 1+.
            /// </summary>
            B32m
        }

        /// <summary>
        /// The 32 characters used by this encoding
        /// </summary>
        public const string CharSet = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
        /// <summary>
        /// Maximum length of the whole Bech32 string (hrp + separator + data)
        /// </summary>
        public const int StringMaxLength = 90;
        private const int CheckSumSize = 6;
        private const int HrpMinLength = 1;
        private const int HrpMaxLength = 83;
        private const int HrpMinValue = 33;
        private const int HrpMaxValue = 126;
        private const char Separator = '1';
        private static readonly uint[] generator = { 0x3b6a57b2u, 0x26508e6du, 0x1ea119fau, 0x3d4233ddu, 0x2a1462b3u };
        private const uint V0CS = 1;
        private const uint V1CS = 0x2bc830a3;



        /// <summary>
        /// Checks to see if a given string is a valid bech-32 encoded string with a valid checksum.
        /// </summary>
        /// <param name="bech32EncodedString">Input string to check</param>
        /// <param name="mode">Encoding mode</param>
        /// <returns>True if input was a valid bech-32 encoded string with checksum, otherwise false.</returns>
        public static bool IsValid(string bech32EncodedString, Mode mode)
        {
            if (!HasValidChars(bech32EncodedString))
            {
                return false;
            }

            byte[] b32Arr = Bech32Decode(bech32EncodedString, out string hrp);
            if (b32Arr.Length < CheckSumSize)
            {
                return false;
            }

            return VerifyChecksum(hrp, b32Arr, mode);
        }

        /// <summary>
        /// Checks to see if a given string is a valid bech-32 encoded string.
        /// <para/>* Doesn't verify checksum.
        /// <para/>* Doesn't verify data length (since it requires decoding first).
        /// </summary>
        /// <param name="bech32EncodedString">Input string to check.</param>
        /// <returns>True if input was a valid bech-32 encoded string (without verifying checksum).</returns>
        public static bool HasValidChars(string bech32EncodedString)
        {
            if (string.IsNullOrEmpty(bech32EncodedString) || bech32EncodedString.Length > StringMaxLength)
            {
                return false;
            }

            // Reject mixed upper and lower characters.
            if (bech32EncodedString.ToLower() != bech32EncodedString && bech32EncodedString.ToUpper() != bech32EncodedString)
            {
                return false;
            }

            // Check if it has a separator
            int sepIndex = bech32EncodedString.LastIndexOf(Separator);
            if (sepIndex == -1)
            {
                return false;
            }

            // Validate human readable part
            string hrp = bech32EncodedString.Substring(0, sepIndex);
            if (!IsValidHrp(hrp))
            {
                return false;
            }

            // Validate data part
            string data = bech32EncodedString.Substring(sepIndex + 1);
            if (data.Length < CheckSumSize || !data.All(x => CharSet.Contains(char.ToLower(x))))
            {
                return false;
            }

            return true;
        }

        private static bool IsValidHrp(string hrp)
        {
            return !string.IsNullOrWhiteSpace(hrp) &&
                    hrp.Length >= HrpMinLength &&
                    hrp.Length <= HrpMaxLength &&
                    hrp.All(x => (byte)x >= HrpMinValue && (byte)x <= HrpMaxValue);
        }


        private static uint Polymod(ReadOnlySpan<byte> data)
        {
            uint chk = 1;
            foreach (byte b in data)
            {
                uint temp = chk >> 25;
                chk = ((chk & 0x1ffffff) << 5) ^ b;
                for (int i = 0; i < 5; i++)
                {
                    if (((temp >> i) & 1) == 1)
                    {
                        chk ^= generator[i];
                    }
                }
            }
            return chk;
        }

        private static byte[] ExpandHrp(string hrp)
        {
            byte[] result = new byte[(2 * hrp.Length) + 1];
            for (int i = 0; i < hrp.Length; i++)
            {
                result[i] = (byte)(hrp[i] >> 5);
                result[i + hrp.Length + 1] = (byte)(hrp[i] & 0b0001_1111 /*=31*/);
            }
            return result;
        }

        private static byte[] CalculateCheckSum(string hrp, byte[] data, Mode mode)
        {
            // expand hrp, append data to it, and then add 6 zero bytes at the end.
            byte[] bytes = ExpandHrp(hrp).ConcatFast(data).ConcatFast(new byte[CheckSumSize]);

            // get polymod of the whole data and then flip the least significant bit.
            uint pm = Polymod(bytes) ^ (mode == Mode.B32 ? V0CS : V1CS);

            byte[] result = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                result[i] = (byte)((pm >> 5 * (5 - i)) & 0b0001_1111 /*=31*/);
            }
            return result;
        }

        private static bool VerifyChecksum(string hrp, byte[] data, Mode mode)
        {
            byte[] temp = ExpandHrp(hrp).ConcatFast(data);
            return Polymod(temp) == (mode == Mode.B32 ? V0CS : V1CS);
        }

        private static byte[] ConvertBits(ReadOnlySpan<byte> data, int fromBits, int toBits, bool pad = true)
        {
            // TODO: 
            // We can use a method similar to BIP39 here to avoid the nested loop, usage of List, increase the speed,
            // and shorten this function to 3 lines.
            // Or convert to ulong[], loop through it (3 times) take 5 bits at a time or 8 bits at a time...
            int acc = 0;
            int bits = 0;
            int maxv = (1 << toBits) - 1;
            int maxacc = (1 << (fromBits + toBits - 1)) - 1;

            List<byte> result = new List<byte>();
            foreach (var b in data)
            {
                // Speed doesn't matter for this class but we can skip this check for 8 to 5 conversion.
                if ((b >> fromBits) > 0)
                {
                    return null;
                }
                acc = ((acc << fromBits) | b) & maxacc;
                bits += fromBits;
                while (bits >= toBits)
                {
                    bits -= toBits;
                    result.Add((byte)((acc >> bits) & maxv));
                }
            }
            if (pad)
            {
                if (bits > 0)
                {
                    result.Add((byte)((acc << (toBits - bits)) & maxv));
                }
            }
            else if (bits >= fromBits || (byte)((acc << (toBits - bits)) & maxv) != 0)
            {
                return null;
            }
            return result.ToArray();
        }



        private static byte[] Bech32Decode(string bech32EncodedString, out string hrp)
        {
            bech32EncodedString = bech32EncodedString.ToLower();

            int sepIndex = bech32EncodedString.LastIndexOf(Separator);
            hrp = bech32EncodedString.Substring(0, sepIndex);
            string data = bech32EncodedString.Substring(sepIndex + 1);

            byte[] b32Arr = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                b32Arr[i] = (byte)CharSet.IndexOf(data[i]);
            }

            return b32Arr;
        }


        /// <summary>
        /// Converts a bech-32 encoded string back to its byte array representation.
        /// </summary>
        /// <param name="bech32EncodedString">Bech-32 encoded string.</param>
        /// <param name="mode">Encoding mode</param>
        /// <param name="result">Byte array of the given string</param>
        /// <param name="witVer">Witness version</param>
        /// <param name="hrp">Human readable part</param>
        /// <returns>True if the coversion was successful; otherwise false.</returns>
        public static bool TryDecode(string bech32EncodedString, Mode mode, out byte[] result, out byte witVer, out string hrp)
        {
            result = null;
            witVer = 0;
            hrp = null;

            if (!HasValidChars(bech32EncodedString))
                return false;
            if (!Enum.IsDefined(typeof(Mode), mode))
                return false;

            byte[] b32Arr = Bech32Decode(bech32EncodedString, out hrp);
            if (b32Arr.Length <= CheckSumSize)
            {
                return false;
            }
            if (!VerifyChecksum(hrp, b32Arr, mode))
            {
                return false;
            }

            result = ConvertBits(b32Arr.SubArray(1, b32Arr.Length - CheckSumSize - 1), 5, 8, false);
            if (result == null)
            {
                return false;
            }

            witVer = b32Arr[0];
            return true;
        }

        /// <summary>
        /// Converts a bech-32 encoded string back to its byte array representation.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="FormatException"/>
        /// <param name="bech32EncodedString">Bech-32 encoded string.</param>
        /// <param name="mode">Encoding mode</param>
        /// <param name="witVer">Witness version</param>
        /// <param name="hrp">Human readable part</param>
        /// <returns>Byte array of the given string.</returns>
        public static byte[] Decode(string bech32EncodedString, Mode mode, out byte witVer, out string hrp)
        {
            if (!HasValidChars(bech32EncodedString))
                throw new FormatException("Input is not a valid bech32 encoded string.");
            if (!Enum.IsDefined(typeof(Mode), mode))
                throw new ArgumentException("Undefined encoding mode.");

            byte[] b32Arr = Bech32Decode(bech32EncodedString, out hrp);
            if (b32Arr.Length < CheckSumSize)
            {
                throw new FormatException("Invalid data length.");
            }
            if (!VerifyChecksum(hrp, b32Arr, mode))
            {
                throw new FormatException("Invalid checksum.");
            }

            byte[] b256Arr = ConvertBits(b32Arr.SubArray(1, b32Arr.Length - CheckSumSize - 1), 5, 8, false);
            if (b256Arr == null)
            {
                throw new FormatException("Invalid data format.");
            }

            witVer = b32Arr[0];
            return b256Arr;
        }


        /// <summary>
        /// Converts the given byte array to its equivalent string representation that is encoded with bech-32 digits,
        /// with 6 byte appended checksum.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="data">Byte array to encode</param>
        /// <param name="mode">Encoding mode</param>
        /// <param name="witVer">Witness version</param>
        /// <param name="hrp">Human readable part</param>
        /// <returns>The string representation in bech-32 with a checksum.</returns>
        public static string Encode(ReadOnlySpan<byte> data, Mode mode, byte witVer, string hrp)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Data can not be null or empty.");
            if (!Enum.IsDefined(typeof(Mode), mode))
                throw new ArgumentException("Undefined encoding mode.");
            if (!IsValidHrp(hrp))
                throw new FormatException("Invalid HRP.");
            if (witVer >= CharSet.Length)
                throw new ArgumentOutOfRangeException($"Witness version can not be bigger than {CharSet.Length - 1}.");


            byte[] b32Arr = ConvertBits(data, 8, 5, true).AppendToBeginning(witVer);
            byte[] checksum = CalculateCheckSum(hrp, b32Arr, mode);
            b32Arr = b32Arr.ConcatFast(checksum);

            StringBuilder result = new StringBuilder(b32Arr.Length + 1 + hrp.Length);
            result.Append($"{hrp}{Separator}");
            foreach (var b in b32Arr)
            {
                result.Append(CharSet[b]);
            }

            return result.ToString();
        }
    }
}
