// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Linq;

namespace Autarkysoft.Bitcoin.Encoders
{
    /// <summary>
    /// Base-16 or hexadecimal string encoder using only lower case letters.
    /// </summary>
    public static class Base16
    {
        /// <summary>
        /// The prefix that could be appended to the beginning of Base-16 strings to indicate their encoding
        /// </summary>
        public const string Prefix = "0x";
        /// <summary>
        /// The 16 characters used by this encoding
        /// </summary>
        public const string CharSet = "0123456789abcdef";


        /// <summary>
        /// Checks to see if a given string is a valid base-16 encoded string.
        /// </summary>
        /// <param name="hexToCheck">Hex string to check</param>
        /// <returns>True if the given string is a valid base-16 encoded string, otherwise fale.</returns>
        public static bool IsValid(string hexToCheck)
        {
            if (hexToCheck is null || hexToCheck.Length % 2 != 0)
            {
                return false;
            }
            if (hexToCheck.StartsWith(Prefix))
            {
                hexToCheck = hexToCheck[2..];
            }

            return hexToCheck.All(c => CharSet.Contains(char.ToLower(c)));
        }


        /// <summary>
        /// Converts the given base-16 encoded string to its byte array representation.
        /// Return value indicates success.
        /// </summary>
        /// <param name="hex">Hex to convert</param>
        /// <param name="result">Decoded result</param>
        /// <returns>True if the input is a valid base-16 encoded string; otherwise false.</returns>
        public static bool TryDecode(string hex, out byte[] result)
        {
            if (IsValid(hex))
            {
                int start = hex.StartsWith(Prefix) ? 2 : 0;
                ReadOnlySpan<char> vs = hex.AsSpan(start);

                result = new byte[vs.Length / 2];
                for (int i = 0; i < result.Length; i++)
                {
                    int hi = vs[i * 2] - 65;
                    hi = hi + 10 + ((hi >> 31) & 7);

                    int lo = vs[i * 2 + 1] - 65;
                    lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;

                    result[i] = (byte)(lo | hi << 4);
                }

                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Converts the given base-16 encoded string to its byte array representation.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="hex">Hex to convert</param>
        /// <returns>An array of bytes</returns>
        public static byte[] Decode(string hex)
        {
            if (TryDecode(hex, out byte[] result))
            {
                return result;
            }
            else
            {
                throw new ArgumentException($"Input is not a valid hex.");
            }
        }

        /// <summary>
        /// Converts the given base-16 encoded string to its byte array representation in reverse order.
        /// That is 0xabcd turns into { 0xcd, 0xab }.
        /// Return value indicates success.
        /// </summary>
        /// <param name="hex">Hex to convert</param>
        /// <param name="result">Decoded result</param>
        /// <returns>True if the input is a valid base-16 encoded string; otherwise false.</returns>
        public static bool TryDecodeReverse(string hex, out byte[] result)
        {
            if (IsValid(hex))
            {
                int start = hex.StartsWith(Prefix) ? 2 : 0;
                ReadOnlySpan<char> vs = hex.AsSpan(start);

                result = new byte[vs.Length / 2];
                for (int i = 0, j = result.Length - 1; i < result.Length; i++, j--)
                {
                    int hi = vs[i * 2] - 65;
                    hi = hi + 10 + ((hi >> 31) & 7);

                    int lo = vs[i * 2 + 1] - 65;
                    lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;

                    result[j] = (byte)(lo | hi << 4);
                }

                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Converts the given base-16 encoded string to its byte array representation in reverse order.
        /// That is 0xabcd turns into { 0xcd, 0xab }.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="hex">Hex to convert</param>
        /// <returns>An array of bytes</returns>
        public static byte[] DecodeReverse(string hex)
        {
            if (TryDecodeReverse(hex, out byte[] result))
            {
                return result;
            }
            else
            {
                throw new ArgumentException($"Input is not a valid hex.");
            }
        }

        /// <summary>
        /// Converts the given byte array to base-16 (Hexadecimal) encoded string.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">The array of bytes to convert</param>
        /// <returns>Base-16 (Hexadecimal) encoded string.</returns>
        public static string Encode(ReadOnlySpan<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Byte array can not be null.");


            Span<char> ca = new char[data.Length * 2];
            int b;
            for (int i = 0; i < data.Length; i++)
            {
                b = data[i] >> 4;
                ca[i * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));
                b = data[i] & 0xF;
                ca[i * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
            }
            return new string(ca);
        }

        /// <summary>
        /// Converts the given byte array to base-16 (Hexadecimal) encoded string in reverse order.
        /// That is 0xabcd turns into 0xcdab.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">The array of bytes to convert</param>
        /// <returns>Base-16 (Hexadecimal) encoded string in reverse order.</returns>
        public static string EncodeReverse(ReadOnlySpan<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Byte array can not be null.");


            Span<char> ca = new char[data.Length * 2];
            int b;
            for (int i = 0, j = data.Length - 1; i < data.Length && j >= 0; i++, j--)
            {
                b = data[i] >> 4;
                ca[j * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));
                b = data[i] & 0xF;
                ca[j * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
            }
            return new string(ca);
        }
    }
}
