// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin
{
    public class FastStream
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FastStream"/> with the default capacity.
        /// </summary>
        public FastStream()
        {
            buffer = new byte[Capacity];
            position = 0;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FastStream"/> with the given capacity.
        /// </summary>
        /// <param name="size">
        /// Size of the buffer to use, small sizes are changed to default <see cref="Capacity"/> value
        /// </param>
        public FastStream(int size)
        {
            buffer = new byte[size < Capacity ? Capacity : size];
            position = 0;
        }



        private const int Capacity = 100;
        private byte[] buffer;
        private int position;



        /// <summary>
        /// Returns the total size of the stream when converted to byte array.
        /// </summary>
        /// <returns></returns>
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


        private void CheckAndResize(int extraSize)
        {
            if (position + extraSize > buffer.Length)
            {
                byte[] temp = extraSize < Capacity ?
                            (new byte[buffer.Length + Capacity]) :
                            (new byte[buffer.Length + extraSize + Capacity]);
                Buffer.BlockCopy(buffer, 0, temp, 0, buffer.Length);
                buffer = temp;
            }
        }

        public void Write(int i)
        {
            CheckAndResize(sizeof(int));

            buffer[position] = (byte)i;
            buffer[position + 1] = (byte)(i >> 8);
            buffer[position + 2] = (byte)(i >> 16);
            buffer[position + 3] = (byte)(i >> 24);

            position += sizeof(int);
        }

        public void Write(long val)
        {
            CheckAndResize(sizeof(long));

            buffer[position] = (byte)val;
            buffer[position + 1] = (byte)(val >> 8);
            buffer[position + 2] = (byte)(val >> 16);
            buffer[position + 4] = (byte)(val >> 24);
            buffer[position + 5] = (byte)(val >> 32);
            buffer[position + 6] = (byte)(val >> 40);
            buffer[position + 7] = (byte)(val >> 48);
            buffer[position + 8] = (byte)(val >> 56);

            position += sizeof(long);
        }

        public void Write(byte b)
        {
            CheckAndResize(sizeof(byte));
            buffer[position] = b;
            position++;
        }

        public void Write(ushort val)
        {
            CheckAndResize(sizeof(ushort));

            buffer[position] = (byte)val;
            buffer[position + 1] = (byte)(val >> 8);

            position += sizeof(ushort);
        }

        public void Write(uint val)
        {
            CheckAndResize(sizeof(uint));

            buffer[position] = (byte)val;
            buffer[position + 1] = (byte)(val >> 8);
            buffer[position + 2] = (byte)(val >> 16);
            buffer[position + 3] = (byte)(val >> 24);

            position += sizeof(uint);
        }

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

        public void Write(byte[] data)
        {
            CheckAndResize(data.Length);
            Buffer.BlockCopy(data, 0, buffer, position, data.Length);
            position += data.Length;
        }

        public void Write(byte[] data, int totalSize)
        {
            CheckAndResize(totalSize);
            Buffer.BlockCopy(data, 0, buffer, position, data.Length);
            position += totalSize;
        }

        public void Write(FastStream stream)
        {
            CheckAndResize(stream.GetSize());
            Write(stream.ToByteArray());
        }


        public void WriteBigEndian(ushort val)
        {
            CheckAndResize(sizeof(ushort));

            buffer[position + 1] = (byte)val;
            buffer[position] = (byte)(val >> 8);

            position += sizeof(ushort);
        }

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
