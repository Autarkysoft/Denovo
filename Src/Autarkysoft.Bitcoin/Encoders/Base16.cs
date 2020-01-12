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
    public class Base16
    {
        /// <summary>
        /// The prefix that could be appended to the beginning of Base-16 strings.
        /// </summary>
        public const string Prefix = "0x";
        private const string Base16Chars = "0123456789abcdef";


        /// <summary>
        /// Checks to see if a given string is a valid base-16 encoded string.
        /// </summary>
        /// <param name="hexToCheck">Hex string to check.</param>
        /// <returns>True if valid, fale if otherwise.</returns>
        public static bool IsValid(string hexToCheck)
        {
            if (hexToCheck is null || hexToCheck.Length % 2 != 0)
            {
                return false;
            }
            if (hexToCheck.StartsWith(Prefix))
            {
                hexToCheck = hexToCheck.Substring(2);
            }

            return hexToCheck.All(c => Base16Chars.Contains(char.ToLower(c)));
        }


        /// <summary>
        /// Converts the given base-16 encoded string to its byte array representation.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="hex">Hex to convert.</param>
        /// <returns>An array of bytes</returns>
        public static byte[] Decode(string hex)
        {
            if (!IsValid(hex))
                throw new ArgumentException($"Input is not a valid hex. <{hex}>");


            if (hex.StartsWith(Prefix))
            {
                hex = hex.Substring(2);
            }

            byte[] ba = new byte[hex.Length / 2];
            for (int i = 0; i < ba.Length; i++)
            {
                int hi = hex[i * 2] - 65;
                hi = hi + 10 + ((hi >> 31) & 7);

                int lo = hex[i * 2 + 1] - 65;
                lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;

                ba[i] = (byte)(lo | hi << 4);
            }
            return ba;
        }

        /// <summary>
        /// Converts the given byte array to base-16 (Hexadecimal) encoded string.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">The array of bytes to convert.</param>
        /// <returns>Base-16 (Hexadecimal) encoded string.</returns>
        public static string Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Byte array can not be null.");


            char[] ca = new char[data.Length * 2];
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

    }
}
