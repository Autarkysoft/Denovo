// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;
using Xunit;

namespace Tests
{
    public static class Helper
    {
        internal static JsonSerializerSettings jSetting = new JsonSerializerSettings
        {
            Converters = { new ByteArrayHexConverter() },
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };


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


        private static readonly byte[] data =
        {
            191, 223, 147, 104, 106, 49, 205, 85, 252, 92, 27, 143, 210, 144, 254, 57, 164, 49, 225, 98, 106, 27, 65, 58, 254,
            89, 183, 16, 195, 150, 140, 217, 201, 8, 184, 159, 175, 184, 167, 26, 213, 213, 107, 123, 195, 224, 226, 215, 125, 225,
            254, 94, 147, 159, 39, 164, 157, 89, 106, 17, 122, 189, 146, 101, 208, 65, 198, 202, 215, 95, 138, 236, 137, 199, 141,
            148, 176, 198, 118, 29, 119, 223, 146, 225, 151, 45, 70, 42, 224, 20, 1, 85, 77, 150, 160, 24, 67, 5, 171, 130
        };

        /// <summary>
        /// Returns a predefined random bytes used for tests requiring a certain length byte array.
        /// </summary>
        /// <param name="size">Length of the bytes to return</param>
        /// <returns></returns>
        internal static byte[] GetBytes(int size)
        {
            byte[] res = new byte[size];

            int copied = 0;
            int toCopy = (size < data.Length) ? size : data.Length;
            while (copied < size)
            {
                Buffer.BlockCopy(data, 0, res, copied, toCopy);
                copied += toCopy;
                toCopy = (size - copied < data.Length) ? size - copied : data.Length;
            }
            return res;
        }


        /// <summary>
        /// Returns a predefined random hex using the same predefined random bytes
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        internal static string GetBytesHex(int size)
        {
            return BytesToHex(GetBytes(size));
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


        internal static BigInteger HexToBigInt(string hex, bool positive = true)
        {
            return positive ?
                BigInteger.Parse($"00{hex}", NumberStyles.HexNumber) :
                BigInteger.Parse(hex, NumberStyles.HexNumber);
        }


        /// <summary>
        /// Reads the embeded resource file located in "Tests.TestData" folder with the 
        /// given name and file extention type and returns the content without change. 
        /// If the file wasn't found, it fails using <see cref="Assert.True(bool)"/>.
        /// </summary>
        /// <param name="resourceName">Name of the resource file</param>
        /// <param name="fileExtention">[Default value = json] Type of the resource file</param>
        /// <returns>Contents of the read file as string</returns>
        public static string ReadResource(string resourceName, string fileExtention = "json")
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using Stream stream = asm.GetManifestResourceStream($"Tests.TestData.{resourceName}.{fileExtention}");
            if (!(stream is null))
            {
                using StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            else
            {
                Assert.True(false, "File was not found among resources!");
                return "";
            }
        }

        /// <summary>
        /// Reads the embeded resource file located in "Tests.TestData" folder with the 
        /// given name and file extention type and uses JSON deserializer to convert the content to the given
        /// type <typeparamref name="T"/>.
        /// If the file wasn't found, it fails using <see cref="Assert.True(bool)"/>.
        /// </summary>
        /// <typeparam name="T">the type to convert the file content to</typeparam>
        /// <param name="resourceName">Name of the resource file</param>
        /// <param name="fileExtention">[Default value = json] Type of the resource file</param>
        /// <returns>Contents of the read file converted to the given type</returns>
        public static T ReadResource<T>(string resourceName, string fileExtention = "json")
        {
            string read = ReadResource(resourceName, fileExtention);
            return JsonConvert.DeserializeObject<T>(read, jSetting);
        }
    }
}
