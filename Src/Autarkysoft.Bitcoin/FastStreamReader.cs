// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin
{
    public class FastStreamReader
    {
        public FastStreamReader(byte[] ba)
        {
            data = ba.CloneByteArray();
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

        private bool Check(int length)
        {
            return data.Length - position >= length;
        }


        public bool TryReadByteArray(int len, out byte[] result)
        {
            if (Check(len))
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
            if (Check(sizeof(byte)))
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
            if (Check(sizeof(byte)))
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


        public bool TryReadInt32(out int val)
        {
            if (Check(sizeof(int)))
            {
                val = data[position] | (data[position + 1] << 8) | (data[position + 2] << 16) | (data[position + 3] << 24);
                position += sizeof(int);
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }


        public bool TryReadInt64(out long val)
        {
            if (Check(sizeof(long)))
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


        public bool TryReadUInt16(out ushort val)
        {
            if (Check(sizeof(ushort)))
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

        public bool TryReadUInt16BigEndian(out ushort val)
        {
            if (Check(sizeof(ushort)))
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

        public bool TryReadUInt32(out uint val)
        {
            if (Check(sizeof(uint)))
            {
                val = (uint)(data[position] | (data[position + 1] << 8) | (data[position + 2] << 16) | (data[position + 3] << 24));
                position += sizeof(uint);
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }

        public bool TryReadUInt64(out ulong val)
        {
            if (Check(sizeof(ulong)))
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
