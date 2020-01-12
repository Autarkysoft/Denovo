// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Reflection;
using Xunit;

namespace Tests
{
    public static class Helper
    {
        public static void ComparePrivateField<InstanceType, FieldType>(InstanceType instance, string fieldName, FieldType expected)
        {
            FieldInfo fi = typeof(InstanceType).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi is null)
            {
                Assert.True(false, "The private field was not found.");
            }

            object fieldVal = fi.GetValue(instance);
            if (fieldVal is null)
            {
                Assert.True(false, "The private field value was null.");
            }
            else if (fieldVal is FieldType actual)
            {
                Assert.Equal(expected, actual);
            }
            else
            {
                Assert.True(false, $"Field value is not the same type as expected.{Environment.NewLine}" +
                    $"Actual type: {fieldVal.GetType()}{Environment.NewLine}" +
                    $"Expected type: {expected.GetType()}");
            }
        }


        /// <summary>
        /// This is used internally by unit tests so checks are skipped.
        /// Use <see cref="CryptoCurrency.Net.Encoders.Base16.ToByteArray(string)"/> for complete functionality.
        /// </summary>
        /// <param name="hex">Hex to convert.</param>
        /// <returns></returns>
        internal static byte[] HexToBytes(string hex, bool reverse = false)
        {
            if (string.IsNullOrEmpty(hex))
            {
                return new byte[0];
            }
            if (hex.Length % 2 != 0)
            {
                throw new FormatException("Invalid hex");
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
            if (reverse)
            {
                Array.Reverse(ba);
            }
            return ba;
        }

        /// <summary>
        /// This is used internally by unit tests so checks are skipped.
        /// Use <see cref="CryptoCurrency.Net.Extensions.ByteArrayExtension.ToBase16(byte[])"/> for complete functionality.
        /// </summary>
        /// <param name="ba">Bytes to convert.</param>
        /// <returns></returns>
        internal static string BytesToHex(byte[] ba)
        {
            char[] ca = new char[ba.Length * 2];
            int b;
            for (int i = 0; i < ba.Length; i++)
            {
                b = ba[i] >> 4;
                ca[i * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));
                b = ba[i] & 0xF;
                ca[i * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
            }
            return new string(ca);
        }

    }
}
