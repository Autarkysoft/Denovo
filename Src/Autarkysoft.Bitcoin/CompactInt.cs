// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Compact representation of up to 64-bit integers also known as "variable length integer" as defined by bitcoin.
    /// <para/>All explicit operations convert values without throwing any exceptions (data may be lost, caller must consider this).
    /// </summary>
    /// <remarks>
    /// Ref: https://en.bitcoin.it/wiki/Protocol_documentation#Variable_length_integer
    /// </remarks>
    public readonly struct CompactInt : IComparable, IComparable<CompactInt>, IEquatable<CompactInt>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> using a 64-bit signed integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use (must be >= 0)</param>
        public CompactInt(long val)
        {
            if (val < 0)
                throw new ArgumentOutOfRangeException(nameof(val), "CompactInt value can not be negative.");

            value = (ulong)val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> using a 32-bit signed integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use (must be >= 0)</param>
        public CompactInt(int val)
        {
            if (val < 0)
                throw new ArgumentOutOfRangeException(nameof(val), "CompactInt value can not be negative.");

            value = (ulong)val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> using a 32-bit unsigned integer.
        /// </summary>
        /// <param name="val">Value to use</param>
        public CompactInt(uint val)
        {
            value = val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> using a 64-bit unsigned integer.
        /// </summary>
        /// <param name="val">Value to use</param>
        public CompactInt(ulong val)
        {
            value = val;
        }



        // Don't rename.
        // Variable name is hard-coded in tests with its value being fetched using reflection.
        private readonly ulong value;



        /// <summary>
        /// Reads the <see cref="CompactInt"/> value from the given<see cref="FastStreamReader"/>. 
        /// The return value indicates success.
        /// </summary>
        /// <param name="stream">Stream containing the <see cref="CompactInt"/></param>
        /// <param name="result">The result</param>
        /// <param name="error">Error message</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public static bool TryRead(FastStreamReader stream, out CompactInt result, out Errors error)
        {
            result = 0;
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }
            if (!stream.TryReadByte(out byte firstByte))
            {
                error = Errors.EndOfStream;
                return false;
            }

            if (firstByte <= 252)
            {
                result = new CompactInt((ulong)firstByte);
            }
            else if (firstByte == 253) // 0xfd-XX-XX
            {
                if (!stream.TryReadUInt16(out ushort val))
                {
                    error = Errors.ShortCompactInt2;
                    result = 0;
                    return false;
                }

                if (val <= 252)
                {
                    error = Errors.SmallCompactInt2;
                    result = 0;
                    return false;
                }
                result = new CompactInt((ulong)val);
            }
            else if (firstByte == 254) // 0xfe-XX-XX-XX-XX
            {
                if (!stream.TryReadUInt32(out uint val))
                {
                    error = Errors.ShortCompactInt4;
                    result = 0;
                    return false;
                }

                if (val <= ushort.MaxValue)
                {
                    error = Errors.SmallCompactInt4;
                    result = 0;
                    return false;
                }
                result = new CompactInt((ulong)val);
            }
            else if (firstByte == 255) //0xff-XX-XX-XX-XX-XX-XX-XX-XX
            {
                if (!stream.TryReadUInt64(out ulong val))
                {
                    error = Errors.ShortCompactInt8;
                    result = 0;
                    return false;
                }

                if (val <= uint.MaxValue)
                {
                    error = Errors.SmallCompactInt8;
                    result = 0;
                    return false;
                }
                result = new CompactInt(val);
            }

            error = Errors.None;
            return true;
        }


        /// <summary>
        /// Adds the serialized size of this instance to the given counter.
        /// </summary>
        /// <param name="counter">Size counter to use</param>
        public void AddSerializedSize(SizeCounter counter)
        {
            if (value <= 252) // 1 Byte
            {
                counter.AddByte();
            }
            else if (value <= 0xffff) // 1 + 2 Byte
            {
                counter.Add(3);
            }
            else if (value <= 0xffffffff) // 1 + 4 Byte
            {
                counter.Add(5);
            }
            else // < 0xffffffffffffffff // 1 + 8 Byte
            {
                counter.Add(9);
            }
        }

        /// <summary>
        /// Converts this value to its byte array representation in little-endian order 
        /// and writes the result to the given <see cref="FastStream"/>.
        /// </summary>
        /// <param name="stream">Stream to use.</param>
        public void WriteToStream(FastStream stream)
        {
            if (value <= 252) // 1 Byte
            {
                stream.Write((byte)value);
            }
            else if (value <= 0xffff) // 1 + 2 Byte
            {
                stream.Write((byte)0xfd);
                stream.Write((ushort)value);
            }
            else if (value <= 0xffffffff) // 1 + 4 Byte
            {
                stream.Write((byte)0xfe);
                stream.Write((uint)value);
            }
            else // < 0xffffffffffffffff // 1 + 8 Byte
            {
                stream.Write((byte)0xff);
                stream.Write(value);
            }
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator CompactInt(ulong val) => new CompactInt(val);
        public static implicit operator CompactInt(uint val) => new CompactInt((ulong)val);
        public static implicit operator CompactInt(ushort val) => new CompactInt((ulong)val);
        public static implicit operator CompactInt(byte val) => new CompactInt((ulong)val);
        public static explicit operator CompactInt(long val) => new CompactInt((ulong)val);
        public static explicit operator CompactInt(int val) => new CompactInt((ulong)val);

        public static implicit operator ulong(CompactInt val) => val.value;
        public static explicit operator uint(CompactInt val) => (uint)val.value;
        public static explicit operator ushort(CompactInt val) => (ushort)val.value;
        public static explicit operator byte(CompactInt val) => (byte)val.value;
        public static explicit operator long(CompactInt val) => (long)val.value;
        public static explicit operator int(CompactInt val) => (int)val.value;


        public static bool operator >(CompactInt left, CompactInt right) => left.value > right.value;
        public static bool operator >(CompactInt left, long right) => right < 0 || left.value > (ulong)right;
        public static bool operator >(CompactInt left, int right) => right < 0 || left.value > (ulong)right;
        public static bool operator >(long left, CompactInt right) => left > 0 && (ulong)left > right.value;
        public static bool operator >(int left, CompactInt right) => left > 0 && (ulong)left > right.value;

        public static bool operator >=(CompactInt left, CompactInt right) => left.value >= right.value;
        public static bool operator >=(CompactInt left, long right) => right < 0 || left.value >= (ulong)right;
        public static bool operator >=(CompactInt left, int right) => right < 0 || left.value >= (ulong)right;
        public static bool operator >=(long left, CompactInt right) => left >= 0 && (ulong)left >= right.value;
        public static bool operator >=(int left, CompactInt right) => left >= 0 && (ulong)left >= right.value;

        public static bool operator <(CompactInt left, CompactInt right) => left.value < right.value;
        public static bool operator <(CompactInt left, long right) => right >= 0 && left.value < (ulong)right;
        public static bool operator <(CompactInt left, int right) => right >= 0 && left.value < (ulong)right;
        public static bool operator <(long left, CompactInt right) => left < 0 || (ulong)left < right.value;
        public static bool operator <(int left, CompactInt right) => left < 0 || (ulong)left < right.value;

        public static bool operator <=(CompactInt left, CompactInt right) => left.value <= right.value;
        public static bool operator <=(CompactInt left, long right) => right >= 0 && left.value <= (ulong)right;
        public static bool operator <=(CompactInt left, int right) => right >= 0 && left.value <= (ulong)right;
        public static bool operator <=(long left, CompactInt right) => left < 0 || (ulong)left <= right.value;
        public static bool operator <=(int left, CompactInt right) => left < 0 || (ulong)left <= right.value;

        public static bool operator ==(CompactInt left, CompactInt right) => left.value == right.value;
        public static bool operator ==(CompactInt left, long right) => right >= 0 && left.value == (ulong)right;
        public static bool operator ==(CompactInt left, int right) => right >= 0 && left.value == (ulong)right;
        public static bool operator ==(long left, CompactInt right) => left >= 0 && (ulong)left == right.value;
        public static bool operator ==(int left, CompactInt right) => left >= 0 && (ulong)left == right.value;

        public static bool operator !=(CompactInt left, CompactInt right) => left.value != right.value;
        public static bool operator !=(CompactInt left, long right) => right < 0 || left.value != (ulong)right;
        public static bool operator !=(CompactInt left, int right) => right < 0 || left.value != (ulong)right;
        public static bool operator !=(long left, CompactInt right) => left < 0 || (ulong)left != right.value;
        public static bool operator !=(int left, CompactInt right) => left < 0 || (ulong)left != right.value;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        /// <summary>
        /// Compares the value of a given <see cref="CompactInt"/> with the value of this instance 
        /// and returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <param name="other">Other <see cref="CompactInt"/> to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger.</returns>
        public int CompareTo(CompactInt other) => value.CompareTo(other.value);

        /// <summary>
        /// Checks if the given object is of type <see cref="CompactInt"/> and then compares its value with 
        /// the value of this instance.
        /// Returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger</returns>
        public int CompareTo(object obj)
        {
            if (obj is null)
                return 1;
            if (!(obj is CompactInt))
                throw new ArgumentException($"Object must be of type {nameof(CompactInt)}");

            return CompareTo((CompactInt)obj);
        }

        /// <summary>
        /// Checks if the value of the given <see cref="CompactInt"/> is equal to the value of this instance.
        /// </summary>
        /// <param name="other">Other <see cref="CompactInt"/> value to compare to this instance.</param>
        /// <returns>true if the value is equal to the value of this instance; otherwise, false.</returns>
        public bool Equals(CompactInt other) => value == other.value;

        /// <summary>
        /// Checks if the given object is of type <see cref="CompactInt"/> and if its value is equal to the value of this instance.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>
        /// true if value is an instance of <see cref="CompactInt"/> 
        /// and equals the value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj) => !(obj is null) && obj is CompactInt ci && value == ci.value;

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
