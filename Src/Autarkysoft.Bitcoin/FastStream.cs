// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;

namespace Autarkysoft.Bitcoin
{
    // TODO: this is not yet fast, needs more optimization using span or maybe unsafe. needs benchmarking too.
    /// <summary>
    /// A custom stream mainly used in <see cref="IDeserializable"/> objects.
    /// </summary>
    public class FastStream
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FastStream"/> with the default capacity.
        /// </summary>
        public FastStream()
        {
            buffer = new byte[DefaultCapacity];
            position = 0;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FastStream"/> with the given capacity.
        /// </summary>
        /// <param name="size">
        /// Initial buffer size to use, sizes &#60;= 0 are changed to <see cref="DefaultCapacity"/> value without throwing
        /// </param>
        public FastStream(int size)
        {
            buffer = new byte[size <= 0 ? DefaultCapacity : size];
            position = 0;
        }


        /// <summary>
        /// Default capacity value used in default constructor and for resizing buffer
        /// </summary>
        public const int DefaultCapacity = 128;
        // Don't rename (used in test with reflection)
        private byte[] buffer;
        private int position;



        /// <summary>
        /// Returns the total size of the stream when converted to byte array.
        /// </summary>
        /// <returns>Stream size</returns>
        public int GetSize() => position;

        /// <summary>
        /// Returns the byte array buffer of this instance.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToByteArray()
        {
            byte[] result = new byte[position];
            Buffer.BlockCopy(buffer, 0, result, 0, position);
            return result;
        }


        internal void CheckAndResize(int extraSize)
        {
            Debug.Assert(extraSize >= 0);
            Debug.Assert(buffer.Length - position >= 0);

            int toAdd = extraSize - (buffer.Length - position);
            if (toAdd > 0)
            {
                if (toAdd < DefaultCapacity)
                {
                    toAdd = DefaultCapacity;
                }

                byte[] temp = new byte[buffer.Length + toAdd];
                Buffer.BlockCopy(buffer, 0, temp, 0, buffer.Length);
                buffer = temp;
            }
        }

        /// <summary>
        /// Writes byte array representation of the given <see cref="int"/> to stream in little-endian order.
        /// </summary>
        /// <param name="val">32-bit signed integer</param>
        public void Write(int val)
        {
            CheckAndResize(sizeof(int));

            buffer[position] = (byte)val;
            buffer[position + 1] = (byte)(val >> 8);
            buffer[position + 2] = (byte)(val >> 16);
            buffer[position + 3] = (byte)(val >> 24);

            position += sizeof(int);
        }

        /// <summary>
        /// Writes byte array representation of the given <see cref="long"/> to stream in little-endian order.
        /// </summary>
        /// <param name="val">64-bit signed integer</param>
        public void Write(long val)
        {
            CheckAndResize(sizeof(long));

            buffer[position] = (byte)val;
            buffer[position + 1] = (byte)(val >> 8);
            buffer[position + 2] = (byte)(val >> 16);
            buffer[position + 3] = (byte)(val >> 24);
            buffer[position + 4] = (byte)(val >> 32);
            buffer[position + 5] = (byte)(val >> 40);
            buffer[position + 6] = (byte)(val >> 48);
            buffer[position + 7] = (byte)(val >> 56);

            position += sizeof(long);
        }

        /// <summary>
        /// Writes the given <see cref="byte"/> to stream.
        /// </summary>
        /// <param name="b">8-bit unsigned integer</param>
        public void Write(byte b)
        {
            CheckAndResize(sizeof(byte));
            buffer[position] = b;
            position++;
        }

        /// <summary>
        /// Writes byte array representation of the given <see cref="ushort"/> to stream in little-endian order.
        /// </summary>
        /// <param name="val">16-bit usigned integer</param>
        public void Write(ushort val)
        {
            CheckAndResize(sizeof(ushort));

            buffer[position] = (byte)val;
            buffer[position + 1] = (byte)(val >> 8);

            position += sizeof(ushort);
        }

        /// <summary>
        /// Writes byte array representation of the given <see cref="uint"/> to stream in little-endian order.
        /// </summary>
        /// <param name="val">32-bit usigned integer</param>
        public void Write(uint val)
        {
            CheckAndResize(sizeof(uint));

            buffer[position] = (byte)val;
            buffer[position + 1] = (byte)(val >> 8);
            buffer[position + 2] = (byte)(val >> 16);
            buffer[position + 3] = (byte)(val >> 24);

            position += sizeof(uint);
        }

        /// <summary>
        /// Writes byte array representation of the given <see cref="ulong"/> to stream in little-endian order.
        /// </summary>
        /// <param name="val">64-bit usigned integer</param>
        public void Write(ulong val)
        {
            CheckAndResize(sizeof(ulong));

            buffer[position] = (byte)val;
            buffer[position + 1] = (byte)(val >> 8);
            buffer[position + 2] = (byte)(val >> 16);
            buffer[position + 3] = (byte)(val >> 24);
            buffer[position + 4] = (byte)(val >> 32);
            buffer[position + 5] = (byte)(val >> 40);
            buffer[position + 6] = (byte)(val >> 48);
            buffer[position + 7] = (byte)(val >> 56);

            position += sizeof(ulong);
        }

        /// <summary>
        /// Writes the given byte array to stream.
        /// </summary>
        /// <param name="data">The data to write</param>
        public void Write(byte[] data)
        {
            CheckAndResize(data.Length);
            Buffer.BlockCopy(data, 0, buffer, position, data.Length);
            position += data.Length;
        }

        /// <summary>
        /// Writes the given byte array to stream startring from the given index.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="data">The data to write</param>
        /// <param name="startIndex">The zero-based byte offset into data</param>
        /// <param name="count">The number of bytes to copy (writing is skipped if the value is negative)</param>
        public void Write(byte[] data, int startIndex, int count)
        {
            if (count <= 0)
                return;

            CheckAndResize(count);
            Buffer.BlockCopy(data, startIndex, buffer, position, count);
            position += count;
        }

        /// <summary>
        /// Writes the given byte array to stream with zero pads added after the data to reach the specified length 
        /// defined by <paramref name="sizeWithPad"/> parameter.
        /// <para/> eg. Write 1 byte=X with <paramref name="sizeWithPad"/>=1 => writes X
        /// <para/> eg. Write 1 byte=X with <paramref name="sizeWithPad"/>=2 => writes X0
        /// </summary>
        /// <param name="data">The data to write</param>
        /// <param name="sizeWithPad">
        /// The desired final length of the given data with padding (values &#60; data.Length are ignored)
        /// </param>
        public void Write(byte[] data, int sizeWithPad)
        {
            int finalSize = sizeWithPad >= data.Length ? sizeWithPad : data.Length;
            CheckAndResize(finalSize);
            Buffer.BlockCopy(data, 0, buffer, position, data.Length);
            position += finalSize;
        }

        /// <summary>
        /// Writes the given byte array to stream with zero pads added before the data to reach the specified length 
        /// defined by <paramref name="sizeWithPad"/> parameter.
        /// <para/> eg. Write 1 byte=X with <paramref name="sizeWithPad"/>=1 => writes X
        /// <para/> eg. Write 1 byte=X with <paramref name="sizeWithPad"/>=2 => writes 0X
        /// </summary>
        /// <param name="sizeWithPad">
        /// The desired final length of the given data with padding (values &#60; data.Length are ignored)
        /// </param>
        /// <param name="data">The data to write</param>
        public void Write(int sizeWithPad, byte[] data)
        {
            int finalSize = sizeWithPad >= data.Length ? sizeWithPad : data.Length;
            CheckAndResize(finalSize);
            position += finalSize - data.Length;
            Buffer.BlockCopy(data, 0, buffer, position, data.Length);
            position += data.Length;
        }

        /// <summary>
        /// Writes the given byte array to stream while adding the data length to the beginning as a <see cref="CompactInt"/>.
        /// </summary>
        /// <param name="data">The data to write</param>
        public void WriteWithCompactIntLength(byte[] data)
        {
            if (data.Length <= 252) // CompactInt is 1 Byte long
            {
                CheckAndResize(data.Length + 1);
                buffer[position] = (byte)data.Length;
                Buffer.BlockCopy(data, 0, buffer, position + 1, data.Length);
                position += data.Length + 1;
            }
            else if (data.Length <= 0xffff) // CompactInt is 1 + 2 Byte long
            {
                CheckAndResize(data.Length + 3);
                buffer[position] = 0xfd;
                buffer[position + 1] = (byte)data.Length;
                buffer[position + 2] = (byte)(data.Length >> 8);
                Buffer.BlockCopy(data, 0, buffer, position + 3, data.Length);
                position += data.Length + 3;
            }
            else
            {
                CompactInt len = new CompactInt(data.Length);
                len.WriteToStream(this);
                Write(data);
            }
        }

        /// <summary>
        /// Writes data from the given stream to this stream.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void Write(FastStream stream)
        {
            CheckAndResize(stream.position);
            Write(stream.buffer, 0, stream.position);
        }


        /// <summary>
        /// Writes byte array representation of the given <see cref="ushort"/> to stream in big-endian order.
        /// </summary>
        /// <param name="val">16-bit usigned integer</param>
        public void WriteBigEndian(ushort val)
        {
            CheckAndResize(sizeof(ushort));

            buffer[position + 1] = (byte)val;
            buffer[position] = (byte)(val >> 8);

            position += sizeof(ushort);
        }

        /// <summary>
        /// Writes byte array representation of the given <see cref="uint"/> to stream in big-endian order.
        /// </summary>
        /// <param name="val">32-bit usigned integer</param>
        public void WriteBigEndian(uint val)
        {
            CheckAndResize(sizeof(uint));

            buffer[position + 3] = (byte)val;
            buffer[position + 2] = (byte)(val >> 8);
            buffer[position + 1] = (byte)(val >> 16);
            buffer[position] = (byte)(val >> 24);

            position += sizeof(uint);
        }
    }
}
