// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Autarkysoft.Bitcoin.Encoders
{
    /// <summary>
    /// Base-58 string encoder used for addresses, private keys, extended (BIP-32) keys, ...
    /// This encoding can use a 4 byte checksum using double SHA-256 hash of the data.
    /// <para/> https://en.bitcoin.it/wiki/Base58Check_encoding
    /// </summary>
    public static class Base58
    {
        /// <summary>
        /// The 32 characters used by this encoding
        /// </summary>
        /// <remarks>All letters excluding 0OIl</remarks>
        public const string CharSet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        /// <summary>
        /// Number of bytes in the checksum (first 4 bytes of double SHA256 of data)
        /// </summary>
        public const int ChecksumSize = 4;

        internal enum Mode
        {
            B58,
            B43
        }

        // The rounded up result of [log(baseValue) / log(256)] * 1000
        private const int Log58 = 733;
        private const int Log43 = 679;


        /// <summary>
        /// Checks to see if a given string is a valid base-58 encoded string (without checking the checksum).
        /// </summary>
        /// <param name="encoded">String to check</param>
        /// <returns>True if input was a valid base-58 encoded string; otherwise false.</returns>
        public static bool IsValid(string encoded) => HasValidChars(encoded, Mode.B58);

        /// <summary>
        /// Checks to see if a given string is a valid base-58 encoded string with a valid checksum.
        /// </summary>
        /// <param name="encoded">String to check</param>
        /// <returns>True if input was a valid base-58 encoded string with checksum, false if otherwise.</returns>
        public static bool IsValidWithChecksum(string encoded) =>
            HasValidChars(encoded, Mode.B58) && HasValidChecksum(encoded, Mode.B58);


        internal static bool HasValidChars(string val, Mode mode)
        {
            Debug.Assert(Enum.IsDefined(typeof(Mode), mode));

            // Empty string is considered valid here
            if (val is null)
            {
                return false;
            }

            if (mode == Mode.B58 && !val.All(c => Base58.CharSet.Contains(c)))
            {
                return false;
            }
            else if (mode == Mode.B43 && !val.All(c => Base43.CharSet.Contains(c)))
            {
                return false;
            }

            return true;
        }

        internal static bool HasValidChecksum(string val, Mode mode)
        {
            byte[] data = DecodeWithoutValidation(val, mode);
            if (data.Length < ChecksumSize)
            {
                return false;
            }

            byte[] dataWithoutChecksum = data.SubArray(0, data.Length - ChecksumSize);
            ReadOnlySpan<byte> checksum = data.SubArrayFromEnd(ChecksumSize);
            ReadOnlySpan<byte> calculatedChecksum = CalculateChecksum(dataWithoutChecksum);

            return checksum.SequenceEqual(calculatedChecksum);
        }

        internal static byte[] CalculateChecksum(ReadOnlySpan<byte> data)
        {
            using Sha256 hash = new Sha256();
            return hash.ComputeChecksum(data);
        }


        /// <summary>
        /// Converts the given base-58 encoded string back to its byte array representation.
        /// Return value indicates success.
        /// </summary>
        /// <param name="encoded">Base-58 encoded string</param>
        /// <param name="result">Byte array of the given string</param>
        /// <returns>True if the conversion was successful; otherwise false.</returns>
        public static bool TryDecode(string encoded, out byte[] result)
        {
            if (HasValidChars(encoded, Mode.B58))
            {
                result = DecodeWithoutValidation(encoded, Mode.B58);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Converts the given base-58 encoded string back to its byte array representation.
        /// </summary>
        /// <exception cref="FormatException"/>
        /// <param name="encoded">Base-58 encoded string</param>
        /// <returns>Byte array of the given string.</returns>
        public static byte[] Decode(string encoded)
        {
            if (!HasValidChars(encoded, Mode.B58))
                throw new FormatException("Input is not a valid Base-58 encoded string.");

            return DecodeWithoutValidation(encoded, Mode.B58);
        }


        internal static byte[] DecodeWithoutValidation(string validB58EncodedString, Mode mode)
        {
            Debug.Assert(validB58EncodedString != null);
            Debug.Assert(Enum.IsDefined(typeof(Mode), mode));

            int baseValue = mode == Mode.B58 ? 58 : 43;
            int logBaseValue = mode == Mode.B58 ? Log58 : Log43;
            ReadOnlySpan<char> chars = mode == Mode.B58 ? Base58.CharSet.AsSpan() : Base43.CharSet.AsSpan();

            int index = 0;
            int leadingZeroCount = 0;
            while (index < validB58EncodedString.Length && validB58EncodedString[index] == '1')
            {
                leadingZeroCount++;
                index++;
            }

            // This is a basic base conversion based on a simple principle that the total value is calculated like this:
            // charIndex0 * 58^0 + charIndex1 * 58^1 + charIndex2 * 58^2 = charIndex0 + 58*(charIndex1 + 58*(charIndex2)) ...

            // Base-256 (byte array) in big-endian order with length = log(58)/log(256) rounded up.
            byte[] b256 = new byte[(validB58EncodedString.Length - index) * logBaseValue / 1000 + 1];
            for (; index < validB58EncodedString.Length; index++)
            {
                int carry = chars.IndexOf(validB58EncodedString[index]);
                for (int i = b256.Length - 1; i >= 0; i--)
                {
                    carry += baseValue * b256[i];
                    b256[i] = (byte)(carry % 256);
                    carry /= 256;
                }
            }

            // Skip leading zeroes in Base-256.
            int zeros = 0;
            while (zeros < b256.Length && b256[zeros] == 0)
            {
                zeros++;
            }

            byte[] result = new byte[leadingZeroCount + (b256.Length - zeros)];
            for (int i = leadingZeroCount; i < result.Length; i++)
            {
                result[i] = b256[zeros++];
            }
            return result;
        }


        /// <summary>
        /// Converts a base-58 encoded string back to its byte array representation
        /// while also validating and removing checksum bytes.
        /// Return value indicates success.
        /// </summary>
        /// <param name="b58EncodedStringWithChecksum">Base-58 encoded string with checksum</param>
        /// <param name="result">Byte array of the given string</param>
        /// <returns>True if the conversion was successful; otherwise false.</returns>
        public static bool TryDecodeWithChecksum(string b58EncodedStringWithChecksum, out byte[] result)
        {
            if (HasValidChars(b58EncodedStringWithChecksum, Mode.B58))
            {
                byte[] data = DecodeWithoutValidation(b58EncodedStringWithChecksum, Mode.B58);
                if (data.Length >= ChecksumSize)
                {
                    result = data.SubArray(0, data.Length - ChecksumSize);
                    ReadOnlySpan<byte> checksum = data.SubArrayFromEnd(ChecksumSize);
                    ReadOnlySpan<byte> calculatedChecksum = CalculateChecksum(result);

                    if (checksum.SequenceEqual(calculatedChecksum))
                    {
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Converts a base-58 encoded string back to its byte array representation
        /// while also validating and removing checksum bytes.
        /// </summary>
        /// <exception cref="FormatException"/>
        /// <param name="b58EncodedStringWithChecksum">Base-58 encoded string with checksum</param>
        /// <returns>Byte array of the given string.</returns>
        public static byte[] DecodeWithChecksum(string b58EncodedStringWithChecksum)
        {
            if (!HasValidChars(b58EncodedStringWithChecksum, Mode.B58))
            {
                throw new FormatException("Input is not a valid base-58 encoded string.");
            }

            byte[] data = DecodeWithoutValidation(b58EncodedStringWithChecksum, Mode.B58);
            if (data.Length < ChecksumSize)
            {
                throw new FormatException("Input is not a valid base-58 encoded string.");
            }

            byte[] dataWithoutChecksum = data.SubArray(0, data.Length - ChecksumSize);
            byte[] checksum = data.SubArrayFromEnd(ChecksumSize);
            byte[] calculatedChecksum = CalculateChecksum(dataWithoutChecksum);

            if (!((ReadOnlySpan<byte>)checksum).SequenceEqual(calculatedChecksum))
            {
                throw new FormatException("Invalid checksum.");
            }

            return dataWithoutChecksum;
        }


        internal static string Encode(ReadOnlySpan<byte> data, Mode mode)
        {
            Debug.Assert(data != null);
            Debug.Assert(Enum.IsDefined(typeof(Mode), mode));

            int baseValue = mode == Mode.B58 ? 58 : 43;
            ReadOnlySpan<char> chars = mode == Mode.B58 ? Base58.CharSet.AsSpan() : Base43.CharSet.AsSpan();

            var big = new BigInteger(data, true, true);
            var result = new StringBuilder();
            while (big > 0)
            {
                big = BigInteger.DivRem(big, baseValue, out BigInteger remainder);
                result.Insert(0, chars[(int)remainder]);
            }

            // Append `1` for each leading 0 byte
            for (var i = 0; i < data.Length && data[i] == 0; i++)
            {
                result.Insert(0, '1');
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts the given byte array to its equivalent string representation that is encoded with base-58 digits.
        /// </summary>
        /// <remarks>
        /// Unlike Decode functions, using BigInteger here makes things slightly faster. 
        /// The difference will be more noticeable with larger byte arrays such as extended keys (BIP32).
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">Byte array to encode.</param>
        /// <returns>The string representation in base-58.</returns>
        public static string Encode(ReadOnlySpan<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Input can not be null.");

            return Encode(data, Mode.B58);
        }

        /// <summary>
        /// Converts the given byte array to its equivalent string representation that is encoded with base-58 digits,
        /// with 4 byte appended checksum.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">Byte array to encode.</param>
        /// <returns>The string representation in base-58 with a checksum.</returns>
        public static string EncodeWithChecksum(ReadOnlySpan<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Input can not be null!");

            Span<byte> checksum = CalculateChecksum(data);
            Span<byte> buffer = new byte[checksum.Length + data.Length];
            data.CopyTo(buffer);
            checksum.CopyTo(buffer.Slice(data.Length));

            return Encode(buffer, Mode.B58);
        }
    }
}
