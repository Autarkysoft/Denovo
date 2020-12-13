// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Target is the compact (32-bit) form of the 256-bit number used in bitcoin for proof-of-work.
    /// <para/>The default struct constructor must never be used since Target can not be 0.
    /// </summary>
    /// <remarks> https://en.bitcoin.it/wiki/Target and https://en.bitcoin.it/wiki/Difficulty </remarks>
    public readonly struct Target : IComparable, IComparable<Target>, IEquatable<Target>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Target"/> using a 32-bit signed integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use (must be >= 0)</param>
        public Target(int val)
        {
            if (val < 0)
                throw new ArgumentOutOfRangeException(nameof(val), "Target value can not be negative.");

            int firstByte = val >> 24;
            uint masked = (uint)(val & 0x007fffff);
            if (firstByte <= 3)
            {
                masked >>= 8 * (3 - firstByte);
            }

            if (IsNegative(masked, (uint)val))
                throw new ArgumentOutOfRangeException(nameof(val), "Target value can not be negative.");
            if (WillOverflow(firstByte, masked))
                throw new ArgumentOutOfRangeException(nameof(val), "Target is defined as a 256-bit number (value overflow).");

            value = (uint)val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Target"/> using a 32-bit unsigned integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use</param>
        public Target(uint val)
        {
            int firstByte = (int)(val >> 24);
            uint masked = val & 0x007fffff;
            if (firstByte <= 3)
            {
                masked >>= 8 * (3 - firstByte);
            }

            if (IsNegative(masked, val))
                throw new ArgumentOutOfRangeException(nameof(val), "Target value can not be negative.");
            if (WillOverflow(firstByte, masked))
                throw new ArgumentOutOfRangeException(nameof(val), "Target is defined as a 256-bit number (value overflow).");

            value = val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Target"/> using a 256-bit integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use (must be at most 256 bits)</param>
        public Target(BigInteger val)
        {
            byte[] ba = val.ToByteArray(true, false);
            if (ba.Length > 32)
                throw new ArgumentOutOfRangeException(nameof(val), "Value can not be bigger than 256 bits.");

            int shift = val == 0 ? 0 : ba.Length;
            int rem = 0;
            if (ba.Length >= 3)
            {
                if ((ba[^1] & 0x80) != 0)
                {
                    shift++;
                    rem = ba[^1] << 8 | ba[^2];
                }
                else
                {
                    rem = ba[^1] << 16 | ba[^2] << 8 | ba[^3];
                }
            }
            else if (ba.Length == 2)
            {
                if ((ba[^1] & 0x80) != 0)
                {
                    shift++;
                    rem = ba[^1] << 8 | ba[^2];
                }
                else
                {
                    rem = ba[^1] << 16 | ba[^2] << 8;
                }
            }
            else if (ba.Length == 1)
            {
                if ((ba[^1] & 0x80) != 0)
                {
                    shift++;
                    rem = ba[^1] << 8;
                }
                else
                {
                    rem = ba[^1] << 16;
                }
            }

            value = (uint)((shift << 24) | rem);
        }

        private Target(uint val, bool ignore)
        {
            if (ignore)
            {
                value = val;
            }
            else
            {
                throw new ArgumentException();
            }
        }



        // Don't rename (reflection used in tests).
        private readonly uint value;

        // Difficulty formula: 0xAABBCCDD -> 0xBBCCDD * 2^(8*(0xAA-3))
        // 2^n is shift left by n
        // Long form is 32 bytes or 256 bits.
        // If (BB!=0) then 3 byte or 24 bits is already set
        //   so we can't shift more than 256-24=232 bits
        //   so n has to be 232 tops -> 8*(0xAA-3)=232 -> 0xAA=32
        // If (BB==0 but CC!=0) then 2 bytes or 16 bits is already set
        //   256-16=240 => 8*(0xAA-3)=240 -> 0xAA=33
        // If (BB==0 & CC==0 but DD!=0) then 1 byte or 8 bits is already set
        //   256-8=248 => 8*(0xAA-3)=248 -> 0xAA=34
        //
        // If (0xAA-3 <= 0) the shift is opposite (shift right) and will never overflow
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool WillOverflow(int firstByte, uint masked) => masked != 0 &&
                                                                        ((firstByte > 34) ||
                                                                         (firstByte > 33 && masked > 0x000000ff) ||
                                                                         (firstByte > 32 && masked > 0x0000ffff));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNegative(uint masked, uint val) => (masked != 0 && (val & 0x00800000) != 0);


        /// <summary>
        /// Reads the <see cref="Target"/> value from the given <see cref="FastStreamReader"/>. 
        /// Return value indicates success.
        /// </summary>
        /// <param name="stream">Stream containing the <see cref="Target"/></param>
        /// <param name="result">The result</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public static bool TryRead(FastStreamReader stream, out Target result, out string error)
        {
            if (stream is null)
            {
                result = 0;
                error = "Stream can not be null.";
                return false;
            }
            if (!stream.TryReadUInt32(out uint val))
            {
                result = 0;
                error = Err.EndOfStream;
                return false;
            }

            int firstByte = (int)(val >> 24);
            uint masked = val & 0x007fffff;
            if (firstByte <= 3)
            {
                masked >>= 8 * (3 - firstByte);
            }

            if (IsNegative(masked, val))
            {
                error = "Target can not be negative.";
                result = 0;
                return false;
            }

            if (WillOverflow(firstByte, masked))
            {
                error = "Target is defined as a 256-bit number (value overflow).";
                result = 0;
                return false;
            }

            result = new Target(val, true);
            error = null;
            return true;
        }


        /// <summary>
        /// Converts this value to its byte array representation in little-endian order 
        /// and writes the result to the given <see cref="FastStream"/>.
        /// </summary>
        /// <param name="stream">Stream to use.</param>
        public void WriteToStream(FastStream stream)
        {
            stream.Write(value);
        }


        /// <summary>
        /// Converts this value to its BigInteger representation.
        /// </summary>
        /// <returns>BigInteger result</returns>
        public BigInteger ToBigInt()
        {
            int shift = (int)(value >> 24);
            uint masked = value & 0x007fffff;
            if (shift <= 3)
            {
                // https://github.com/bitcoin/bitcoin/blob/b18978066d875707af8e15edf225d3e52b5ade05/src/arith_uint256.cpp#L207-L209
                return masked >> 8 * (3 - shift);
            }
            else
            {
                return masked * BigInteger.Pow(2, 8 * (shift - 3));
            }
        }

        /// <summary>
        /// Converts this value to its byte array representation.
        /// </summary>
        /// <returns>An array of bytes in little-endian order</returns>
        public byte[] ToByteArray()
        {
            return new byte[4]
            {
                (byte)value,
                (byte)(value >> 8),
                (byte)(value >> 16),
                (byte)(value >> 24),
            };
        }

        /// <summary>
        /// Converts this value to its UInt32 array representation.
        /// </summary>
        /// <returns>An array of 32-bit unsigned integers</returns>
        public uint[] ToUInt32Array()
        {
            uint[] result = new uint[32 / 4];
            int firstByte = (int)(value >> 24);
            if (firstByte <= 3)
            {
                uint masked = value & 0x007fffff;
                result[^1] = masked >> 8 * (3 - firstByte);
                return result;
            }

            /*** Target ***/
            // if bits = XXYYYYYY then target = YYYYYY * 2^(8*(XX-3))
            // a * 2^k is the same as a << k
            int shift = 8 * (firstByte - 3);
            // We have 3 bytes that we need to shift left and since we are using UInt32, 3 bytes (24 bit) can fall in 1 item or 2 max.
            // Each 32 bit shift moves to next index from the end. Each remainder is the shift of the remaining 3 bytes.
            // if the remainder is bigger than 8 bits the shifted 24 bits will go in next item.
            // 00000000_XXXXXXXX_XXXXXXXX_XXXXXXXX << 9 => 00000000_00000000_00000000_0000000X XXXXXXXX_XXXXXXXX_XXXXXXX0_00000000
            int index = shift / 32;
            int remShift = shift % 32;
            result[result.Length - 1 - index] = (value & 0x007fffff) << remShift;
            if (remShift > 8)
            {
                result[result.Length - 2 - index] = (value & 0x007fffff) >> (32 - remShift);
            }

            return result;
        }

        /// <summary>
        /// Converts this value to its equivalant difficulty.
        /// </summary>
        /// <param name="max">Maximum target value</param>
        /// <returns>Difficulty value</returns>
        public BigInteger ToDifficulty(Target max)
        {
            BigInteger big = ToBigInt();
            return big == 0 ? 0 : max.ToBigInt() / ToBigInt();
        }

        /// <summary>
        /// Converts this value to its equivalant hashrate (hash/second).
        /// </summary>
        /// <param name="max">Maximum target value</param>
        /// <returns>Hashrate value as hash/second</returns>
        public BigInteger ToHashrate(Target max)
        {
            return ToDifficulty(max) * BigInteger.Pow(2, 32) / 600;
        }



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator Target(uint val) => new Target(val);
        public static implicit operator Target(ushort val) => new Target(val);
        public static implicit operator Target(byte val) => new Target(val);
        public static explicit operator Target(int val) => new Target((uint)val);

        public static implicit operator uint(Target val) => val.value;
        public static explicit operator ushort(Target val) => (ushort)val.value;
        public static explicit operator byte(Target val) => (byte)val.value;
        public static explicit operator int(Target val) => (int)val.value;


        public static bool operator >(Target left, Target right) => left.value > right.value;
        public static bool operator >(Target left, int right) => right < 0 || left.value > (ulong)right;
        public static bool operator >(int left, Target right) => left > 0 && (ulong)left > right.value;

        public static bool operator >=(Target left, Target right) => left.value >= right.value;
        public static bool operator >=(Target left, int right) => right < 0 || left.value >= (ulong)right;
        public static bool operator >=(int left, Target right) => left >= 0 && (ulong)left >= right.value;

        public static bool operator <(Target left, Target right) => left.value < right.value;
        public static bool operator <(Target left, int right) => right >= 0 && left.value < (ulong)right;
        public static bool operator <(int left, Target right) => left < 0 || (ulong)left < right.value;

        public static bool operator <=(Target left, Target right) => left.value <= right.value;
        public static bool operator <=(Target left, int right) => right >= 0 && left.value <= (ulong)right;
        public static bool operator <=(int left, Target right) => left < 0 || (ulong)left <= right.value;

        public static bool operator ==(Target left, Target right) => left.value == right.value;
        public static bool operator ==(Target left, int right) => right >= 0 && left.value == (ulong)right;
        public static bool operator ==(int left, Target right) => left >= 0 && (ulong)left == right.value;

        public static bool operator !=(Target left, Target right) => left.value != right.value;
        public static bool operator !=(Target left, int right) => right < 0 || left.value != (ulong)right;
        public static bool operator !=(int left, Target right) => left < 0 || (ulong)left != right.value;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        /// <summary>
        /// Compares the value of a given <see cref="Target"/> with the value of this instance and 
        /// And returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <param name="other">Other <see cref="Target"/> to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger.</returns>
        public int CompareTo(Target other) => value.CompareTo(other.value);

        /// <summary>
        /// Checks if the given object is of type <see cref="Target"/> and then compares its value with the value of this instance.
        /// Returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger</returns>
        public int CompareTo(object obj)
        {
            if (obj is null)
                return 1;
            if (!(obj is Target))
                throw new ArgumentException($"Object must be of type {nameof(Target)}");

            return CompareTo((Target)obj);
        }

        /// <summary>
        /// Checks if the value of the given <see cref="Target"/> is equal to the value of this instance.
        /// </summary>
        /// <param name="other">Other <see cref="Target"/> value to compare to this instance.</param>
        /// <returns>true if the value is equal to the value of this instance; otherwise, false.</returns>
        public bool Equals(Target other) => value == other.value;

        /// <summary>
        /// Checks if the given object is of type <see cref="Target"/> and if its value is equal to the value of this instance.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>
        /// true if value is an instance of <see cref="Target"/> 
        /// and equals the value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj) => !(obj is null) && obj is Target tar && value == tar.value;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => value.GetHashCode();

        /// <summary>
        /// Converts the value of the current instance to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the value of the current instance.</returns>
        public override string ToString() => value.ToString();
    }
}
