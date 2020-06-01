// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// A custom stream reader to be used in <see cref="IDeserializable"/> objects.
    /// <para/>All numbers are read as little-endian unless the method name explicitly says big-endian
    /// like <see cref="TryReadUInt16BigEndian(out ushort)"/>.
    /// <para/>Note: the main optimization is done by skipping size checks. 
    /// the <see cref="CheckRemaining(int)"/> method can be called by the user to check remaining bytes, then instead of calling
    /// TryRead* methods (that perform the same check for each individual object) the Read* method should be called that skips
    /// the size check.
    /// See <see cref="Blockchain.Blocks.Block.TryDeserializeHeader(FastStreamReader, out string)"/> for example of how
    /// this should be used.
    /// </summary>
    public class FastStreamReader
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FastStreamReader"/> using the given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Data to use</param>
        public FastStreamReader(byte[] ba)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Can not instantiate a stream with null bytes.");

            data = ba.CloneByteArray();
            position = 0;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FastStreamReader"/> using a sub-array of the given byte array
        /// from the starting index and the specified length.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <param name="ba">Data to use</param>
        /// <param name="start">Starting index</param>
        /// <param name="length">Length of the data to copy</param>
        public FastStreamReader(byte[] ba, int start, int length)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Can not instantiate a stream with null bytes.");

            data = ba.SubArray(start, length);
            position = 0;
        }



        // Don't rename either one of the following 2 fields (reflection used in tests).
        private readonly byte[] data;
        private int position;



        /// <summary>
        /// Checks if there is enough bytes remaining to read.
        /// </summary>
        /// <param name="length">Length of the data that should be read</param>
        /// <returns>True if there is enough bytes remaining; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckRemaining(int length) => data.Length - position >= length;

        /// <summary>
        /// Returns current index inside the buffer (number of read bytes so far)
        /// </summary>
        /// <returns>Current index or number of read bytes</returns>
        public int GetCurrentIndex() => position;

        /// <summary>
        /// Returns remaining number of bytes in the buffer to read.
        /// </summary>
        /// <returns>Number of bytes left</returns>
        public int GetRemainingBytesCount() => data.Length - position;

        /// <summary>
        /// Moves index forward by 1 (skips one byte).
        /// </summary>
        public void SkipOneByte() => position++;


        /// <summary>
        /// Compares the given byte array with a sub array of buffer from current position and equal to given bytes length.
        /// <para/>Buffer: {1,2,3,4,5} &#38; pos=1 &#38; other={2,3} => true
        /// <para/>Buffer: {1,2,3,4,5} &#38; pos=1 &#38; other={4,5} => false
        /// <para/>Buffer: {1,2,3,4,5} &#38; pos=2 &#38; other={4,5} => true
        /// </summary>
        /// <remarks>
        /// This method is useful for finding magic bytes inside a buffer without moving the index or actuall reading bytes.
        /// </remarks>
        /// <param name="other">Bytes to compare</param>
        /// <returns>True if equal; otherwise false.</returns>
        public bool CompareBytes(byte[] other)
        {
            return CheckRemaining(other.Length) && ((ReadOnlySpan<byte>)data).Slice(position, other.Length).SequenceEqual(other);
        }

        /// <summary>
        /// Search for the given byte array inside this stream while moving the position to the index where the other
        /// byte array starts. Useful for finding magic bytes inside a stream.
        /// </summary>
        /// <param name="other">The byte array to search for</param>
        /// <returns>True if the other byte array was found inside this stream; otherwise false.</returns>
        public bool FindAndSkip(byte[] other)
        {
            while (data.Length - position >= other.Length)
            {
                if (((ReadOnlySpan<byte>)data).Slice(position, other.Length).SequenceEqual(other))
                {
                    return true;
                }
                else
                {
                    position++;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads and returns 32 bytes from this stream.
        /// </summary>
        /// <returns>A 32 byte long array</returns>
        public byte[] ReadByteArray32Checked()
        {
            byte[] result = new byte[32];
            Buffer.BlockCopy(data, position, result, 0, 32);
            position += 32;
            return result;
        }

        /// <summary>
        /// Reads specified number of bytes from this stream if possible. Return value indicates success.
        /// </summary>
        /// <param name="len">Number of bytes to read</param>
        /// <param name="result">Result (null if failed)</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadByteArray(int len, out byte[] result)
        {
            if (CheckRemaining(len))
            {
                result = new byte[len];
                Buffer.BlockCopy(data, position, result, 0, len);
                position += len;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Reads the <see cref="CompactInt"/> length first and returns the respective length of bytes from the buffer.
        /// Return value indicates success.
        /// <para/>This method simplyfies reading data structures from the stream that start with a <see cref="CompactInt"/>
        /// and works for small lengths that fit in an <see cref="int"/> and will fail for anything bigger.
        /// </summary>
        /// <param name="result">Result (null if failed)</param>
        /// <returns>True if bytes were read; otherwise false.</returns>
        public bool TryReadByteArrayCompactInt(out byte[] result)
        {
            if (!TryReadByte(out byte firstByte))
            {
                result = null;
                return false;
            }

            if (firstByte <= 252)
            {
                return TryReadByteArray(firstByte, out result);
            }
            else if (firstByte == 253) // 0xfd-XX-XX
            {
                if (!CheckRemaining(sizeof(ushort)))
                {
                    result = null;
                    return false;
                }

                // Read ushort but there is no need for cast since it will be cast back to int soon
                int val = data[position] | (data[position + 1] << 8);
                position += sizeof(ushort);
                if (val <= 252)
                {
                    result = null;
                    return false;
                }

                return TryReadByteArray(val, out result);
            }
            else if (firstByte == 254) // 0xfe-XX-XX-XX-XX
            {
                if (!CheckRemaining(sizeof(uint)))
                {
                    result = null;
                    return false;
                }

                // Read uint but as int to avoid casting, bigger than max size will be negative and rejected 
                // (that's 2.1 billion/giga bytes).
                int val = data[position] | (data[position + 1] << 8) | (data[position + 2] << 16) | (data[position + 3] << 24);
                position += sizeof(uint);

                if (val <= ushort.MaxValue) // Also rejects negative (ie. too big a UInt32)
                {
                    result = null;
                    return false;
                }
                return TryReadByteArray(val, out result);
            }
            else // Also (firstByte == 255) that is length at least = uint.MaxValue which is rejected here
            {
                result = null;
                return false;
            }
        }


        /// <summary>
        /// Reads and returns a single byte without moving the index forward. Return value indicates success
        /// </summary>
        /// <param name="b">A signle byte</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryPeekByte(out byte b)
        {
            if (CheckRemaining(sizeof(byte)))
            {
                b = data[position];
                return true;
            }
            else
            {
                b = 0;
                return false;
            }
        }

        /// <summary>
        /// Reads and returns a single byte. Return value indicates success
        /// </summary>
        /// <param name="b">A signle byte</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadByte(out byte b)
        {
            if (CheckRemaining(sizeof(byte)))
            {
                b = data[position];
                position++;
                return true;
            }
            else
            {
                b = 0;
                return false;
            }
        }

        /// <summary>
        /// Reads and returns a 32-bit signed integer in little-endian format.
        /// </summary>
        /// <returns>A 32-bit signed integer</returns>
        public int ReadInt32Checked()
        {
            int res = data[position] | (data[position + 1] << 8) | (data[position + 2] << 16) | (data[position + 3] << 24);
            position += sizeof(int);
            return res;
        }

        /// <summary>
        /// Reads and returns a 32-bit signed integer in little-endian format. Return value indicates success.
        /// </summary>
        /// <param name="val">The 32-bit signed integer result</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadInt32(out int val)
        {
            if (CheckRemaining(sizeof(int)))
            {
                val = ReadInt32Checked();
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }

        /// <summary>
        /// Reads and returns a 64-bit signed integer in little-endian format.
        /// </summary>
        /// <returns>A 64-bit signed integer</returns>
        public long ReadInt64Checked()
        {
            long res = data[position]
                     | ((long)data[position + 1] << 8)
                     | ((long)data[position + 2] << 16)
                     | ((long)data[position + 3] << 24)
                     | ((long)data[position + 4] << 32)
                     | ((long)data[position + 5] << 40)
                     | ((long)data[position + 6] << 48)
                     | ((long)data[position + 7] << 56);
            position += sizeof(long);
            return res;
        }

        /// <summary>
        /// Reads and returns a 64-bit signed integer in little-endian format. Return value indicates success
        /// </summary>
        /// <param name="val">The 64-bit signed integer result</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadInt64(out long val)
        {
            if (CheckRemaining(sizeof(long)))
            {
                val = ReadInt64Checked();
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }

        /// <summary>
        /// Reads and returns a 16-bit unsigned integer in little-endian format.
        /// </summary>
        /// <returns>A 16-bit unsigned integer</returns>
        public ushort ReadUInt16Checked()
        {
            ushort res = (ushort)(data[position] | (data[position + 1] << 8));
            position += sizeof(ushort);
            return res;
        }

        /// <summary>
        /// Reads and returns a 16-bit unsigned integer in little-endian format. Return value indicates success
        /// </summary>
        /// <param name="val">The 16-bit unsigned integer result</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadUInt16(out ushort val)
        {
            if (CheckRemaining(sizeof(ushort)))
            {
                val = ReadUInt16Checked();
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }

        /// <summary>
        /// Reads and returns a 16-bit unsigned integer in little-endian format.
        /// </summary>
        /// <returns>A 16-bit unsigned integer</returns>
        public ushort ReadUInt16BigEndianChecked()
        {
            ushort res = (ushort)(data[position + 1] | (data[position] << 8));
            position += sizeof(ushort);
            return res;
        }

        /// <summary>
        /// Reads and returns a 16-bit unsigned integer in big-endian format. Return value indicates success
        /// </summary>
        /// <param name="val">The 16-bit unsigned integer result</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadUInt16BigEndian(out ushort val)
        {
            if (CheckRemaining(sizeof(ushort)))
            {
                val = ReadUInt16BigEndianChecked();
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }

        /// <summary>
        /// Reads and returns a 32-bit unsigned integer in little-endian format.
        /// </summary>
        /// <returns>A 32-bit unsigned integer</returns>
        public uint ReadUInt32Checked()
        {
            uint res = (uint)(data[position] | (data[position + 1] << 8) | (data[position + 2] << 16) | (data[position + 3] << 24));
            position += sizeof(uint);
            return res;
        }

        /// <summary>
        /// Reads and returns a 32-bit unsigned integer in little-endian format. Return value indicates success
        /// </summary>
        /// <param name="val">The 32-bit unsigned integer result</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadUInt32(out uint val)
        {
            if (CheckRemaining(sizeof(uint)))
            {
                val = ReadUInt32Checked();
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }

        /// <summary>
        /// Reads and returns a 32-bit unsigned integer in little-endian format.
        /// </summary>
        /// <returns>A 32-bit unsigned integer</returns>
        public ulong ReadUInt64Checked()
        {
            ulong res = data[position]
                     | ((ulong)data[position + 1] << 8)
                     | ((ulong)data[position + 2] << 16)
                     | ((ulong)data[position + 3] << 24)
                     | ((ulong)data[position + 4] << 32)
                     | ((ulong)data[position + 5] << 40)
                     | ((ulong)data[position + 6] << 48)
                     | ((ulong)data[position + 7] << 56);

            position += sizeof(ulong);
            return res;
        }

        /// <summary>
        /// Reads and returns a 64-bit unsigned integer in little-endian format. Return value indicates success
        /// </summary>
        /// <param name="val">The 64-bit unsigned integer result</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadUInt64(out ulong val)
        {
            if (CheckRemaining(sizeof(ulong)))
            {
                val = ReadUInt64Checked();
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }


        /// <summary>
        /// Reads and returns a DER length. Return value indicates success
        /// </summary>
        /// <param name="len">Der length</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadDerLength(out int len)
        {
            // DER length: if data.length<128 => 1 byte (the length itself)
            //             if data.length>=128 => first 7 bits is byte size of length followed by length bytes
            // https://docs.microsoft.com/en-us/windows/win32/seccertenroll/about-encoded-length-and-value-bytes
            if (TryReadByte(out byte b))
            {
                if (b > 128)
                {
                    if (TryReadByteArray(b & 0b0111_1111, out byte[] temp))
                    {
                        len = 0;
                        for (int i = temp.Length - 1, j = 0; i >= 0; i--, j += 8)
                        {
                            len |= temp[i] << j;
                        }
                        return true;
                    }
                    else
                    {
                        len = 0;
                        return false;
                    }
                }
                else
                {
                    len = b;
                    return true;
                }
            }
            else
            {
                len = 0;
                return false;
            }
        }
    }
}
