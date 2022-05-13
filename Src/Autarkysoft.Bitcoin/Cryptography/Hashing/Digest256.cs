// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Globalization;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// 256-bit message digest
    /// </summary>
    public readonly struct Digest256 : IComparable, IComparable<Digest256>, IEquatable<Digest256>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Digest256"/> using the given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ba32">Byte array to use</param>
        public Digest256(Span<byte> ba32)
        {
            if (ba32 == null)
                throw new ArgumentNullException(nameof(ba32));
            if (ba32.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(ba32), "Byte array must be 32 bytes.");

            b0 = (uint)(ba32[00] | (ba32[01] << 8) | (ba32[02] << 16) | (ba32[03] << 24));
            b1 = (uint)(ba32[04] | (ba32[05] << 8) | (ba32[06] << 16) | (ba32[07] << 24));
            b2 = (uint)(ba32[08] | (ba32[09] << 8) | (ba32[10] << 16) | (ba32[11] << 24));
            b3 = (uint)(ba32[12] | (ba32[13] << 8) | (ba32[14] << 16) | (ba32[15] << 24));
            b4 = (uint)(ba32[16] | (ba32[17] << 8) | (ba32[18] << 16) | (ba32[19] << 24));
            b5 = (uint)(ba32[20] | (ba32[21] << 8) | (ba32[22] << 16) | (ba32[23] << 24));
            b6 = (uint)(ba32[24] | (ba32[25] << 8) | (ba32[26] << 16) | (ba32[27] << 24));
            b7 = (uint)(ba32[28] | (ba32[29] << 8) | (ba32[30] << 16) | (ba32[31] << 24));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Digest256"/> using the given UInt32 array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="u8">UInt32 array</param>
        public Digest256(Span<uint> u8)
        {
            if (u8 == null)
                throw new ArgumentNullException(nameof(u8));
            if (u8.Length != 8)
                throw new ArgumentOutOfRangeException(nameof(u8), "Array length must be 8.");

            b0 = u8[0]; b1 = u8[1]; b2 = u8[2]; b3 = u8[3];
            b4 = u8[4]; b5 = u8[5]; b6 = u8[6]; b7 = u8[7];
        }

        /// <summary>
        /// Converts the hexadecimal representation of 256-bit hash to its <see cref="Digest256"/> equivalent.
        /// </summary>
        /// <param name="hex256">Base16 string to use</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <returns>256-bit digest</returns>
        public static Digest256 ParseHex(string hex256)
        {
            if (!Base16.IsValid(hex256))
                throw new ArgumentException("Invalid Base-16", nameof(hex256));
            if (hex256.Length != 64)
                throw new ArgumentOutOfRangeException(nameof(hex256), "String must contain 64 characters.");

            ReadOnlySpan<char> s = hex256.AsSpan();
            return new Digest256(uint.Parse(s.Slice(56, 8), NumberStyles.HexNumber),
                                 uint.Parse(s.Slice(48, 8), NumberStyles.HexNumber),
                                 uint.Parse(s.Slice(40, 8), NumberStyles.HexNumber),
                                 uint.Parse(s.Slice(32, 8), NumberStyles.HexNumber),
                                 uint.Parse(s.Slice(24, 8), NumberStyles.HexNumber),
                                 uint.Parse(s.Slice(16, 8), NumberStyles.HexNumber),
                                 uint.Parse(s.Slice(8, 8), NumberStyles.HexNumber),
                                 uint.Parse(s.Slice(0, 8), NumberStyles.HexNumber));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Digest256"/> using the given <see cref="Sha256.hashState"/>
        /// pointer.
        /// </summary>
        /// <param name="hPt">Hash-state pointer to use</param>
        public unsafe Digest256(uint* hPt)
        {
            b0 = hPt[0].SwapEndian();
            b1 = hPt[1].SwapEndian();
            b2 = hPt[2].SwapEndian();
            b3 = hPt[3].SwapEndian();
            b4 = hPt[4].SwapEndian();
            b5 = hPt[5].SwapEndian();
            b6 = hPt[6].SwapEndian();
            b7 = hPt[7].SwapEndian();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Digest256"/> using the given 32-bit unsigned integers.
        /// </summary>
        public Digest256(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7)
        {
            b0 = u0; b1 = u1; b2 = u2; b3 = u3;
            b4 = u4; b5 = u5; b6 = u6; b7 = u7;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Digest256"/> using the given 32-bit unsigned integer.
        /// </summary>
        /// <param name="u">Value to use</param>
        public Digest256(uint u)
        {
            b0 = u;
            b1 = b2 = b3 = b4 = b5 = b6 = b7 = 0;
        }


        /// <summary>
        /// Bit chunks
        /// </summary>
        public readonly uint b0, b1, b2, b3, b4, b5, b6, b7;

        /// <summary>
        /// Byte size of <see cref="Digest256"/>
        /// </summary>
        public const int ByteSize = 32;


        private static readonly Digest256 _zero = new Digest256(0, 0, 0, 0, 0, 0, 0, 0);
        private static readonly Digest256 _one = new Digest256(1, 0, 0, 0, 0, 0, 0, 0);

        /// <summary>
        /// Zero
        /// </summary>
        public static Digest256 Zero => _zero;
        /// <summary>
        /// One
        /// </summary>
        public static Digest256 One => _one;


        /// <summary>
        /// Converts this instance to its byte array representation with a fixed length of 32.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToByteArray()
        {
            return new byte[32]
            {
                (byte)b0, (byte)(b0 >> 8), (byte)(b0 >> 16), (byte)(b0 >> 24),
                (byte)b1, (byte)(b1 >> 8), (byte)(b1 >> 16), (byte)(b1 >> 24),
                (byte)b2, (byte)(b2 >> 8), (byte)(b2 >> 16), (byte)(b2 >> 24),
                (byte)b3, (byte)(b3 >> 8), (byte)(b3 >> 16), (byte)(b3 >> 24),
                (byte)b4, (byte)(b4 >> 8), (byte)(b4 >> 16), (byte)(b4 >> 24),
                (byte)b5, (byte)(b5 >> 8), (byte)(b5 >> 16), (byte)(b5 >> 24),
                (byte)b6, (byte)(b6 >> 8), (byte)(b6 >> 16), (byte)(b6 >> 24),
                (byte)b7, (byte)(b7 >> 8), (byte)(b7 >> 16), (byte)(b7 >> 24),
            };
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator >(Digest256 left, Digest256 right) => left.CompareTo(right) > 0;
        public static bool operator >=(Digest256 left, Digest256 right) => left.CompareTo(right) >= 0;
        public static bool operator <(Digest256 left, Digest256 right) => left.CompareTo(right) < 0;
        public static bool operator <=(Digest256 left, Digest256 right) => left.CompareTo(right) <= 0;


        public static bool operator ==(Digest256 left, Digest256 right)
        {
            return left.b0 == right.b0 && left.b1 == right.b1 && left.b2 == right.b2 && left.b3 == right.b3 &&
                   left.b4 == right.b4 && left.b5 == right.b5 && left.b6 == right.b6 && left.b7 == right.b7;
        }

        public static bool operator !=(Digest256 left, Digest256 right)
        {
            return left.b0 != right.b0 || left.b1 != right.b1 || left.b2 != right.b2 || left.b3 != right.b3 ||
                   left.b4 != right.b4 || left.b5 != right.b5 || left.b6 != right.b6 || left.b7 != right.b7;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        /// <inheritdoc/>
        public int CompareTo(Digest256 other)
        {
            if (b7 < other.b7) return -1; else if (b7 > other.b7) return 1;
            if (b6 < other.b6) return -1; else if (b6 > other.b6) return 1;
            if (b5 < other.b5) return -1; else if (b5 > other.b5) return 1;
            if (b4 < other.b4) return -1; else if (b4 > other.b4) return 1;
            if (b3 < other.b3) return -1; else if (b3 > other.b3) return 1;
            if (b2 < other.b2) return -1; else if (b2 > other.b2) return 1;
            if (b1 < other.b1) return -1; else if (b1 > other.b1) return 1;
            if (b0 < other.b0) return -1; else if (b0 > other.b0) return 1;

            return 0;
        }

        /// <inheritdoc/>
        public int CompareTo(object obj)
        {
            if (obj is null)
                return 1;
            if (!(obj is Digest256))
                throw new ArgumentException($"Object must be of type {nameof(Digest256)}");

            return CompareTo((Digest256)obj);
        }

        /// <inheritdoc/>
        public bool Equals(Digest256 other)
        {
            return b0 == other.b0 && b1 == other.b1 && b2 == other.b2 && b3 == other.b3 &&
                   b4 == other.b4 && b5 == other.b5 && b6 == other.b6 && b7 == other.b7;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => !(obj is null) && obj is Digest256 d && Equals(d);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            uint hash = 17;
            hash = ((hash << 7) | (hash >> 25)) ^ b0;
            hash = ((hash << 7) | (hash >> 25)) ^ b1;
            hash = ((hash << 7) | (hash >> 25)) ^ b2;
            hash = ((hash << 7) | (hash >> 25)) ^ b3;
            hash = ((hash << 7) | (hash >> 25)) ^ b4;
            hash = ((hash << 7) | (hash >> 25)) ^ b5;
            hash = ((hash << 7) | (hash >> 25)) ^ b6;
            hash = ((hash << 7) | (hash >> 25)) ^ b7;
            return (int)hash;
        }

        /// <inheritdoc/>
        public override string ToString() => $"0x{Base16.Encode(ToByteArray())}";
    }
}
