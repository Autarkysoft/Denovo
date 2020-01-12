// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Numerics;

namespace Autarkysoft.Bitcoin
{
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
            result[result.Length - 1] = newItem;
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

            return (count == 0) ? new byte[0] : sourceArray.SubArray(sourceArray.Length - count, count);
        }


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
    }
}
