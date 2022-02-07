// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// <see cref="BigInteger"/> extensions.
    /// </summary>
    public static class BigIntegerExtension
    {
        /// <summary>
        /// Calculates integer modulo n and always returns a positive result between 0 and n-1.
        /// </summary>
        /// <exception cref="ArithmeticException"/>
        /// <exception cref="DivideByZeroException"/>
        /// <param name="big">BigInteger value to use</param>
        /// <param name="n">A positive BigInteger used as divisor (must be bigger than 0).</param>
        /// <returns>Result of mod in [0, n-1] range</returns>
        public static BigInteger Mod(this BigInteger big, BigInteger n)
        {
            if (n == 0)
                throw new DivideByZeroException("Can't divide by zero!");
            if (n < 0)
                throw new ArithmeticException("Divisor can not be negative");


            BigInteger reminder = big % n;
            return reminder.Sign >= 0 ? reminder : reminder + n;
        }


        /// <summary>
        /// Finds modular multiplicative inverse of the integer a such that ax ≡ 1 (mod m) 
        /// using Extended Euclidean algorithm. If gcd(a,m) != 1 an <see cref="ArithmeticException"/> will be thrown.
        /// </summary>
        /// <exception cref="DivideByZeroException"/>
        /// <exception cref="ArithmeticException"/>
        /// <param name="a">The integer a in ax ≡ 1 (mod m)</param>
        /// <param name="m">The modulus m in ax ≡ 1 (mod m)</param>
        /// <returns>Modular multiplicative inverse result</returns>
        public static BigInteger ModInverse(this BigInteger a, BigInteger m)
        {
            if (a == 0)
                throw new DivideByZeroException("a can't be 0!");

            if (a == 1) return 1;
            if (a < 0) a = a.Mod(m);

            BigInteger s = 0;
            BigInteger oldS = 1;
            BigInteger r = m;
            BigInteger oldR = a % m;

            while (r != 0)
            {
                BigInteger quotient = oldR / r;

                BigInteger prov = r;
                r = oldR - quotient * prov;
                oldR = prov;

                prov = s;
                s = oldS - quotient * prov;
                oldS = prov;
            }

            // The resulting oldR is the Greatest Common Divisor of (a,m) and it needs to be 1.
            if (oldR != 1)
            {
                throw new ArithmeticException($"Modular multiplicative inverse doesn't exist because greatest common divisor " +
                    $"of {nameof(a)} and {nameof(m)} is not 1.");
            }

            return oldS.Mod(m);
        }
    }





    /// <summary>
    /// byte[] extensions.
    /// </summary>
    public static class ByteArrayExtension
    {
        /// <summary>
        /// Appends a new byte to the beginning of the given byte array and returns a new array with a bigger length.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="arr">Source array to append to.</param>
        /// <param name="newItem">The value to append to source.</param>
        /// <returns>The extended array of bytes.</returns>
        public static byte[] AppendToBeginning(this byte[] arr, byte newItem)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr), "Byte array can not be null!");


            byte[] result = new byte[arr.Length + 1];
            result[0] = newItem;
            Buffer.BlockCopy(arr, 0, result, 1, arr.Length);
            return result;
        }


        /// <summary>
        /// Appends a new byte to the end of the given byte array and returns a new array with a bigger length.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="arr">Source array to append to.</param>
        /// <param name="newItem">The value to append to source.</param>
        /// <returns>The extended array of bytes.</returns>
        public static byte[] AppendToEnd(this byte[] arr, byte newItem)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr), "Byte array can not be null!");


            byte[] result = new byte[arr.Length + 1];
            result[^1] = newItem;
            Buffer.BlockCopy(arr, 0, result, 0, arr.Length);
            return result;
        }


        /// <summary>
        /// Creates a copy (clone) of the given byte array, 
        /// will return null if the source was null instead of throwing an exception.
        /// </summary>
        /// <param name="ba">Byte array to clone</param>
        /// <returns>Copy (clone) of the given byte array</returns>
        public static byte[] CloneByteArray(this byte[] ba)
        {
            if (ba == null)
            {
                return null;
            }
            else
            {
                byte[] result = new byte[ba.Length];
                Buffer.BlockCopy(ba, 0, result, 0, ba.Length);
                return result;
            }
        }


        /// <summary>
        /// Concatinates two given byte arrays and returns a new byte array containing all the elements. 
        /// </summary>
        /// <remarks>
        /// This is a lot faster than Linq (~30 times)
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="firstArray">First set of bytes in the final array.</param>
        /// <param name="secondArray">Second set of bytes in the final array.</param>
        /// <returns>The concatinated array of bytes.</returns>
        public static byte[] ConcatFast(this byte[] firstArray, byte[] secondArray)
        {
            if (firstArray == null)
                throw new ArgumentNullException(nameof(firstArray), "First array can not be null!");
            if (secondArray == null)
                throw new ArgumentNullException(nameof(secondArray), "Second array can not be null!");


            byte[] result = new byte[firstArray.Length + secondArray.Length];
            Buffer.BlockCopy(firstArray, 0, result, 0, firstArray.Length);
            Buffer.BlockCopy(secondArray, 0, result, firstArray.Length, secondArray.Length);
            return result;
        }


        /// <summary>
        /// Creates a new array from the given array by taking a specified number of items starting from a given index.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="index">Starting index in <paramref name="sourceArray"/>.</param>
        /// <param name="count">Number of elements to take.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArray(this byte[] sourceArray, int index, int count)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray), "Input can not be null!");
            if (index < 0 || count < 0)
                throw new IndexOutOfRangeException("Index or count can not be negative.");
            if (sourceArray.Length != 0 && index > sourceArray.Length - 1 || sourceArray.Length == 0 && index != 0)
                throw new IndexOutOfRangeException("Index can not be bigger than array length.");
            if (count > sourceArray.Length - index)
                throw new IndexOutOfRangeException("Array is not long enough.");


            byte[] result = new byte[count];
            Buffer.BlockCopy(sourceArray, index, result, 0, count);
            return result;
        }


        /// <summary>
        /// Creates a new array from the given array by taking items starting from a given index.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="index">Starting index in <paramref name="sourceArray"/>.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArray(this byte[] sourceArray, int index)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray), "Input can not be null!");
            if (sourceArray.Length != 0 && index > sourceArray.Length - 1 || sourceArray.Length == 0 && index != 0)
                throw new IndexOutOfRangeException("Index can not be bigger than array length.");

            return SubArray(sourceArray, index, sourceArray.Length - index);
        }


        /// <summary>
        /// Creates a new array from the given array by taking the specified number of items from the end of the array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="count">Number of elements to take.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArrayFromEnd(this byte[] sourceArray, int count)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray), $"Input can not be null!");
            if (count < 0)
                throw new IndexOutOfRangeException("Count can not be negative.");
            if (count > sourceArray.Length)
                throw new IndexOutOfRangeException("Array is not long enough.");

            return (count == 0) ? Array.Empty<byte>() : sourceArray.SubArray(sourceArray.Length - count, count);
        }


        /// <summary>
        /// Converts the given byte array to base-16 (Hexadecimal) encoded string.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">The array of bytes to convert.</param>
        /// <returns>Base-16 (Hexadecimal) encoded string.</returns>
        public static string ToBase16(this byte[] ba) => Base16.Encode(ba);


        /// <summary>
        /// Converts a byte array to base-64 encoded string.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">The array of bytes to convert.</param>
        /// <returns>Base-64 encoded string.</returns>
        public static string ToBase64(this byte[] ba) => Convert.ToBase64String(ba);


        /// <summary>
        /// Converts the given arbitrary length bytes to a its equivalant <see cref="BigInteger"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">The array of bytes to convert.</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <param name="treatAsPositive">If true will treat the given bytes as always a positive integer.</param>
        /// <returns>A BigInteger.</returns>
        public static BigInteger ToBigInt(this byte[] ba, bool isBigEndian, bool treatAsPositive)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");


            if (ba.Length == 0)
            {
                return BigInteger.Zero;
            }

            // Make a copy of the array to avoid changing original array in case a reverse was needed.
            byte[] bytesToUse = new byte[ba.Length];
            Buffer.BlockCopy(ba, 0, bytesToUse, 0, ba.Length);

            // BigInteger constructor takes little-endian bytes
            if (isBigEndian)
            {
                Array.Reverse(bytesToUse);
            }

            if (treatAsPositive && (bytesToUse[^1] & 0x80) > 0)
            {
                bytesToUse = bytesToUse.AppendToEnd(0);
            }

            return new BigInteger(bytesToUse);
        }


        /// <summary>
        /// Removes zeros from the end of the given byte array.
        /// <para/>NOTE: If there is no zeros to trim, the same byte array will be return (careful about changing the reference)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Byte array to trim.</param>
        /// <returns>Trimmed bytes.</returns>
        public static byte[] TrimEnd(this byte[] ba)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null!");


            int index = ba.Length - 1;
            int count = 0;
            while (index >= 0 && ba[index] == 0)
            {
                index--;
                count++;
            }
            return (count == 0) ? ba : (count == ba.Length) ? Array.Empty<byte>() : ba.SubArray(0, ba.Length - count);
        }

        /// <summary>
        /// Removes zeros from the beginning of the given byte array.
        /// <para/>NOTE: If there is no zeros to trim, the same byte array will be return (careful about changing the reference)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Byte array to trim.</param>
        /// <returns>Trimmed bytes.</returns>
        public static byte[] TrimStart(this byte[] ba)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null!");


            int index = 0;// index acts both as "index" and "count"
            while (index != ba.Length && ba[index] == 0)
            {
                index++;
            }
            return (index == 0) ? ba : (index == ba.Length) ? Array.Empty<byte>() : ba.SubArray(index);
        }
    }





    /// <summary>
    /// All integer extensions for int, long, uint, ulong,...
    /// </summary>
    public static class IntegerExtension
    {
        /// <summary>
        /// Converts the given 32-bit signed integer to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="i">The 32-bit signed integer to convert.</param>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this int i, bool bigEndian)
        {
            unchecked
            {
                if (bigEndian)
                {
                    return new byte[]
                    {
                        (byte)(i >> 24),
                        (byte)(i >> 16),
                        (byte)(i >> 8),
                        (byte)i
                    };
                }
                else
                {
                    return new byte[]
                    {
                        (byte)i,
                        (byte)(i >> 8),
                        (byte)(i >> 16),
                        (byte)(i >> 24)
                    };
                }
            }
        }

        /// <summary>
        /// Changes endianness of the given 32-bit signed integer (from big to little endian and vice versa)
        /// </summary>
        /// <param name="i">The 32-bit signed integer to reverse</param>
        /// <returns>The 32-bit signed integer result with reverse endianness</returns>
        public static int SwapEndian(this int i) => (i >> 24) | (i << 24) | ((i >> 8) & 0xff00) | ((i << 8) & 0xff0000);

        /// <summary>
        /// Changes endianness of the given 32-bit unsigned integer (from big to little endian and vice versa)
        /// </summary>
        /// <param name="i">The 32-bit unsigned integer to reverse</param>
        /// <returns>The 32-bit signed uninteger result with reverse endianness</returns>
        public static uint SwapEndian(this uint i) => (i >> 24) | (i << 24) | ((i >> 8) & 0xff00) | ((i << 8) & 0xff0000);
    }





    /// <summary>
    /// <see cref="SigHashType"/> extention
    /// </summary>
    public static class SigHashTypeExtension
    {
        /// <summary>
        /// Returns if this <see cref="SigHashType"/> has <see cref="SigHashType.AnyoneCanPay"/> bit set.
        /// </summary>
        /// <param name="sht"><see cref="SigHashType"/> to check</param>
        /// <returns>True if the last bit is set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyoneCanPay(this SigHashType sht) => (sht & SigHashType.AnyoneCanPay) == SigHashType.AnyoneCanPay;

        /// <summary>
        /// Returns if this <see cref="SigHashType"/> is considered of type <see cref="SigHashType.None"/>.
        /// </summary>
        /// <param name="sht"><see cref="SigHashType"/> to check</param>
        /// <returns>True if the type is <see cref="SigHashType.None"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNone(this SigHashType sht) => ((int)sht & 0b0001_1111) == (int)SigHashType.None;

        /// <summary>
        /// Returns if this <see cref="SigHashType"/> is considered of type <see cref="SigHashType.Single"/>.
        /// </summary>
        /// <param name="sht"><see cref="SigHashType"/> to check</param>
        /// <returns>True if the type is <see cref="SigHashType.Single"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSingle(this SigHashType sht) => ((int)sht & 0b0001_1111) == (int)SigHashType.Single;

        /// <summary>
        /// Modifies the given <see cref="SigHashType"/> the way it is used in Taproot scripts.
        /// </summary>
        /// <param name="sht"><see cref="SigHashType"/> to change</param>
        /// <returns>the modified <see cref="SigHashType"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SigHashType ToOutputType(this SigHashType sht)
        {
            // https://github.com/bitcoin/bitcoin/blob/8b4d53e4d6e6a4b6e65a43a78b31d9d091be5b0e/src/script/interpreter.cpp#L1534
            // Note that this can return SigHashType.Default too!
            return sht == SigHashType.Default ? SigHashType.All : sht & SigHashType.Single;
        }
    }





    /// <summary>
    /// Errors extensions
    /// </summary>
    public static class ErrorsExtension
    {
        /// <summary>
        /// Converts the given <see cref="Errors"/> enum to a user friendly string.
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public static string Convert(this Errors err)
        {
            return err switch
            {
                Errors.None => string.Empty,
                Errors.NullStream => "Stream can not be null.",
                Errors.EndOfStream => "Reached end of stream.",

                Errors.ShortCompactInt2 => "First byte 253 needs to be followed by at least 2 byte.",
                Errors.SmallCompactInt2 => "For values less than 253, one byte format of CompactInt should be used.",
                Errors.ShortCompactInt4 => "First byte 254 needs to be followed by at least 4 byte.",
                Errors.SmallCompactInt4 => "For values less than 2 bytes, the [253, ushort] format should be used.",
                Errors.ShortCompactInt8 => "First byte 255 needs to be followed by at least 8 byte.",
                Errors.SmallCompactInt8 => "For values less than 4 bytes, the [254, uint] format should be used.",

                Errors.ShortOPPushData1 => "OP_PushData1 needs to be followed by at least one byte.",
                Errors.SmallOPPushData1 => $"For OP_PushData1 the data value must be bigger than {(byte)OP.PushData1 - 1}.",
                Errors.ShortOPPushData2 => "OP_PushData2 needs to be followed by at least two byte.",
                Errors.SmallOPPushData2 => $"For OP_PushData2 the data value must be bigger than {byte.MaxValue}.",
                Errors.ShortOPPushData4 => "OP_PushData4 needs to be followed by at least 4 byte.",
                Errors.SmallOPPushData4 => $"For OP_PushData4 the data value must be bigger than {ushort.MaxValue}.",
                Errors.UnknownOpPush => "Unknown OP_Push value.",

                Errors.NegativeTarget => "Target can not be negative.",
                Errors.TargetOverflow => "Target is defined as a 256-bit number (value overflow).",

                _ => $"Error message ({err}) is not defined."
            };
        }
    }





    /// <summary>
    /// String extentions
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Computes Levenshtein distance between the two strings using the Wagner–Fischer algorithm
        /// </summary>
        /// <remarks>
        /// <para/>https://en.wikipedia.org/wiki/Levenshtein_distance 
        /// <para/>https://en.wikipedia.org/wiki/Wagner%E2%80%93Fischer_algorithm
        /// </remarks>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>Levenshtein distance</returns>
        public static int LevenshteinDistance(this string s1, string s2)
        {
            var fast1 = s1.AsSpan();
            var fast2 = s2.AsSpan();

            int[,] distance = new int[s1.Length + 1, s2.Length + 1];

            for (var i = 0; i <= s1.Length; distance[i, 0] = i++) ;
            for (var j = 0; j <= s2.Length; distance[0, j] = j++) ;

            for (var i = 1; i <= fast1.Length; i++)
            {
                for (var j = 1; j <= fast2.Length; j++)
                {
                    var cost = (fast1[i - 1] == fast2[j - 1]) ? 0 : 1;
                    int deletion = distance[i - 1, j] + 1;
                    int insertion = distance[i, j - 1] + 1;
                    int substitution = distance[i - 1, j - 1] + cost;

                    distance[i, j] = Math.Min(Math.Min(deletion, insertion), substitution);
                }
            }

            return distance[s1.Length, s2.Length];
        }
    }
}
