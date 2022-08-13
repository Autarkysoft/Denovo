// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
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
        /// Writes the given <see cref="Digest256"/> to this stream as bytes.
        /// </summary>
        /// <param name="digest">256-bit message digest to use</param>
        public void Write(in Digest256 digest)
        {
            CheckAndResize(Digest256.ByteSize);

            buffer[position] = (byte)digest.b0;
            buffer[position + 1] = (byte)(digest.b0 >> 8);
            buffer[position + 2] = (byte)(digest.b0 >> 16);
            buffer[position + 3] = (byte)(digest.b0 >> 24);
            buffer[position + 4] = (byte)digest.b1;
            buffer[position + 5] = (byte)(digest.b1 >> 8);
            buffer[position + 6] = (byte)(digest.b1 >> 16);
            buffer[position + 7] = (byte)(digest.b1 >> 24);
            buffer[position + 8] = (byte)digest.b2;
            buffer[position + 9] = (byte)(digest.b2 >> 8);
            buffer[position + 10] = (byte)(digest.b2 >> 16);
            buffer[position + 11] = (byte)(digest.b2 >> 24);
            buffer[position + 12] = (byte)digest.b3;
            buffer[position + 13] = (byte)(digest.b3 >> 8);
            buffer[position + 14] = (byte)(digest.b3 >> 16);
            buffer[position + 15] = (byte)(digest.b3 >> 24);
            buffer[position + 16] = (byte)digest.b4;
            buffer[position + 17] = (byte)(digest.b4 >> 8);
            buffer[position + 18] = (byte)(digest.b4 >> 16);
            buffer[position + 19] = (byte)(digest.b4 >> 24);
            buffer[position + 20] = (byte)digest.b5;
            buffer[position + 21] = (byte)(digest.b5 >> 8);
            buffer[position + 22] = (byte)(digest.b5 >> 16);
            buffer[position + 23] = (byte)(digest.b5 >> 24);
            buffer[position + 24] = (byte)digest.b6;
            buffer[position + 25] = (byte)(digest.b6 >> 8);
            buffer[position + 26] = (byte)(digest.b6 >> 16);
            buffer[position + 27] = (byte)(digest.b6 >> 24);
            buffer[position + 28] = (byte)digest.b7;
            buffer[position + 29] = (byte)(digest.b7 >> 8);
            buffer[position + 30] = (byte)(digest.b7 >> 16);
            buffer[position + 31] = (byte)(digest.b7 >> 24);

            position += Digest256.ByteSize;
        }


        /// <summary>
        /// Writes the given <see cref="Scalar8x32"/> to this stream as bytes.
        /// </summary>
        /// <param name="scalar">256-bit scalar</param>
        public void Write(in Scalar8x32 scalar)
        {
            CheckAndResize(Scalar8x32.ByteSize);

            buffer[position] = (byte)(scalar.b7 >> 24);
            buffer[position + 1] = (byte)(scalar.b7 >> 16);
            buffer[position + 2] = (byte)(scalar.b7 >> 8);
            buffer[position + 3] = (byte)scalar.b7;
            buffer[position + 4] = (byte)(scalar.b6 >> 24);
            buffer[position + 5] = (byte)(scalar.b6 >> 16);
            buffer[position + 6] = (byte)(scalar.b6 >> 8);
            buffer[position + 7] = (byte)scalar.b6;
            buffer[position + 8] = (byte)(scalar.b5 >> 24);
            buffer[position + 9] = (byte)(scalar.b5 >> 16);
            buffer[position + 10] = (byte)(scalar.b5 >> 8);
            buffer[position + 11] = (byte)scalar.b5;
            buffer[position + 12] = (byte)(scalar.b4 >> 24);
            buffer[position + 13] = (byte)(scalar.b4 >> 16);
            buffer[position + 14] = (byte)(scalar.b4 >> 8);
            buffer[position + 15] = (byte)scalar.b4;
            buffer[position + 16] = (byte)(scalar.b3 >> 24);
            buffer[position + 17] = (byte)(scalar.b3 >> 16);
            buffer[position + 18] = (byte)(scalar.b3 >> 8);
            buffer[position + 19] = (byte)scalar.b3;
            buffer[position + 20] = (byte)(scalar.b2 >> 24);
            buffer[position + 21] = (byte)(scalar.b2 >> 16);
            buffer[position + 22] = (byte)(scalar.b2 >> 8);
            buffer[position + 23] = (byte)scalar.b2;
            buffer[position + 24] = (byte)(scalar.b1 >> 24);
            buffer[position + 25] = (byte)(scalar.b1 >> 16);
            buffer[position + 26] = (byte)(scalar.b1 >> 8);
            buffer[position + 27] = (byte)scalar.b1;
            buffer[position + 28] = (byte)(scalar.b0 >> 24);
            buffer[position + 29] = (byte)(scalar.b0 >> 16);
            buffer[position + 30] = (byte)(scalar.b0 >> 8);
            buffer[position + 31] = (byte)scalar.b0;

            position += Scalar8x32.ByteSize;
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
