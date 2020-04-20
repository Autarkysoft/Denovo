// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

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



        public int GetCurrentIndex() => position;

        public int GetRemainingBytesCount() => data.Length - position;

        public byte[] GetReadBytes(int startIndex)
        {
            byte[] result = new byte[position - startIndex];
            Buffer.BlockCopy(data, startIndex, result, 0, position - startIndex);
            return result;
        }

        /// <summary>
        /// Checks if there is enough bytes remaining to read.
        /// </summary>
        /// <param name="length">Length of the data that should be read</param>
        /// <returns>True if there is enough bytes remaining; otherwise false.</returns>
        public bool CheckRemaining(int length) => data.Length - position >= length;


        public bool CompareBytes(byte[] other)
        {
            if (CheckRemaining(other.Length))
            {
                return ((ReadOnlySpan<byte>)data).Slice(position, other.Length).SequenceEqual(other);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Reads and returns 32 bytes from this stream.
        /// </summary>
        /// <returns>A 32 byte long array</returns>
        public byte[] ReadByteArray32()
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
        /// Reads and returns a 32-bit signed integer in little-endian format. Return value indicates success
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
        /// Reads and returns a 64-bit signed integer in little-endian format. Return value indicates success
        /// </summary>
        /// <param name="val">The 64-bit signed integer result</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadInt64(out long val)
        {
            if (CheckRemaining(sizeof(long)))
            {
                val = data[position]
                    | ((long)data[position + 1] << 8)
                    | ((long)data[position + 2] << 16)
                    | ((long)data[position + 3] << 24)
                    | ((long)data[position + 4] << 32)
                    | ((long)data[position + 5] << 40)
                    | ((long)data[position + 6] << 48)
                    | ((long)data[position + 7] << 56);

                position += sizeof(long);
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
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
                val = (ushort)(data[position] | (data[position + 1] << 8));
                position += sizeof(ushort);
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
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
                val = (ushort)(data[position + 1] | (data[position] << 8));
                position += sizeof(ushort);
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
        /// Reads and returns a 64-bit unsigned integer in little-endian format. Return value indicates success
        /// </summary>
        /// <param name="val">The 64-bit unsigned integer result</param>
        /// <returns>True if there were enough bytes remaining to read; otherwise false.</returns>
        public bool TryReadUInt64(out ulong val)
        {
            if (CheckRemaining(sizeof(ulong)))
            {
                val = data[position]
                    | ((ulong)data[position + 1] << 8)
                    | ((ulong)data[position + 2] << 16)
                    | ((ulong)data[position + 3] << 24)
                    | ((ulong)data[position + 4] << 32)
                    | ((ulong)data[position + 5] << 40)
                    | ((ulong)data[position + 6] << 48)
                    | ((ulong)data[position + 7] << 56);

                position += sizeof(ulong);
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }
    }
}
