// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using System;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Represents values up to 32-bit, used in bitcoin scripts indicating length of the data to be push to the stack.
    /// <para/>All explicit operations convert values without throwing any exceptions (data may be lost, caller must consider this).
    /// </summary>
    public readonly struct StackInt : IComparable, IComparable<StackInt>, IEquatable<StackInt>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="StackInt"/> using a 32-bit signed integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use (must be >= 0)</param>
        public StackInt(int val)
        {
            if (val < 0)
                throw new ArgumentOutOfRangeException(nameof(val), "StackInt value can not be negative!");

            value = (uint)val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StackInt"/> using a 32-bit unsigned integer.
        /// </summary>
        /// <param name="val">Value to use</param>
        public StackInt(uint val)
        {
            value = val;
        }



        // Don't rename (reflection used in tests).
        private readonly uint value;



        /// <summary>
        /// Returns the OP code used as the first byte when converting to a byte array
        /// </summary>
        /// <returns><see cref="OP"/> code</returns>
        public OP GetOpCode()
        {
            if (value > ushort.MaxValue)
            {
                return OP.PushData4;
            }
            else if (value > byte.MaxValue)
            {
                return OP.PushData2;
            }
            else if (value > (byte)OP.PushData1)
            {
                return OP.PushData1;
            }
            else
            {
                return (OP)value;
            }
        }


        /// <summary>
        /// Reads the <see cref="StackInt"/> value from the given <see cref="FastStreamReader"/>. 
        /// Return value indicates success.
        /// </summary>
        /// <param name="stream">Stream containing the <see cref="StackInt"/></param>
        /// <param name="isStrict">Determines if strict rules (shortest encoding possible) should be enforced</param>
        /// <param name="result">The result</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public static bool TryRead(FastStreamReader stream, bool isStrict, out StackInt result, out string error)
        {
            if (stream is null)
            {
                result = 0;
                error = "Stream can not be null.";
                return false;
            }
            if (!stream.TryReadByte(out byte firstByte))
            {
                result = 0;
                error = Err.EndOfStream;
                return false;
            }

            if (firstByte < (byte)OP.PushData1)
            {
                result = new StackInt((uint)firstByte);
            }
            else if (firstByte == (byte)OP.PushData1)
            {
                if (!stream.TryReadByte(out byte val))
                {
                    error = $"OP_{OP.PushData1} needs to be followed by at least one byte.";
                    result = 0;
                    return false;
                }

                if (isStrict && val < (byte)OP.PushData1)
                {
                    error = $"For OP_{OP.PushData1} the data value must be bigger than {(byte)OP.PushData1 - 1}.";
                    result = 0;
                    return false;
                }
                result = new StackInt((uint)val);
            }
            else if (firstByte == (byte)OP.PushData2)
            {
                if (!stream.TryReadUInt16(out ushort val))
                {
                    error = $"OP_{OP.PushData2} needs to be followed by at least two byte.";
                    result = 0;
                    return false;
                }

                if (isStrict && val <= byte.MaxValue)
                {
                    error = $"For OP_{OP.PushData2} the data value must be bigger than {byte.MaxValue}.";
                    result = 0;
                    return false;
                }
                result = new StackInt((uint)val);
            }
            else if (firstByte == (byte)OP.PushData4)
            {
                if (!stream.TryReadUInt32(out uint val))
                {
                    error = $"OP_{OP.PushData4} needs to be followed by at least 4 byte.";
                    result = 0;
                    return false;
                }

                if (isStrict && val <= ushort.MaxValue)
                {
                    error = $"For OP_{OP.PushData4} the data value must be bigger than {ushort.MaxValue}.";
                    result = 0;
                    return false;
                }
                result = new StackInt(val);
            }
            else
            {
                error = "Unknown OP_Push value.";
                result = 0;
                return false;
            }

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
            if (value < (byte)OP.PushData1)
            {
                stream.Write((byte)value);
            }
            else if (value <= byte.MaxValue)
            {
                stream.Write((byte)OP.PushData1);
                stream.Write((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                stream.Write((byte)OP.PushData2);
                stream.Write((ushort)value);
            }
            else // Value <= uint.MaxValue
            {
                stream.Write((byte)OP.PushData4);
                stream.Write(value);
            }
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator StackInt(uint val) => new StackInt(val);
        public static implicit operator StackInt(ushort val) => new StackInt(val);
        public static implicit operator StackInt(byte val) => new StackInt(val);
        public static explicit operator StackInt(int val) => new StackInt((uint)val);

        public static implicit operator uint(StackInt val) => val.value;
        public static explicit operator ushort(StackInt val) => (ushort)val.value;
        public static explicit operator byte(StackInt val) => (byte)val.value;
        public static explicit operator int(StackInt val) => (int)val.value;


        public static bool operator >(StackInt left, StackInt right) => left.value > right.value;
        public static bool operator >(StackInt left, int right) => right < 0 || left.value > (ulong)right;
        public static bool operator >(int left, StackInt right) => left > 0 && (ulong)left > right.value;

        public static bool operator >=(StackInt left, StackInt right) => left.value >= right.value;
        public static bool operator >=(StackInt left, int right) => right < 0 || left.value >= (ulong)right;
        public static bool operator >=(int left, StackInt right) => left > 0 && (ulong)left >= right.value;

        public static bool operator <(StackInt left, StackInt right) => left.value < right.value;
        public static bool operator <(StackInt left, int right) => right > 0 && left.value < (ulong)right;
        public static bool operator <(int left, StackInt right) => left < 0 || (ulong)left < right.value;

        public static bool operator <=(StackInt left, StackInt right) => left.value <= right.value;
        public static bool operator <=(StackInt left, int right) => right > 0 && left.value <= (ulong)right;
        public static bool operator <=(int left, StackInt right) => left < 0 || (ulong)left <= right.value;

        public static bool operator ==(StackInt left, StackInt right) => left.value == right.value;
        public static bool operator ==(StackInt left, int right) => right > 0 && left.value == (ulong)right;
        public static bool operator ==(int left, StackInt right) => left > 0 && (ulong)left == right.value;

        public static bool operator !=(StackInt left, StackInt right) => left.value != right.value;
        public static bool operator !=(StackInt left, int right) => right < 0 || left.value != (ulong)right;
        public static bool operator !=(int left, StackInt right) => left < 0 || (ulong)left != right.value;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        #region Interfaces and overrides

        /// <summary>
        /// Compares the value of a given <see cref="StackInt"/> with the value of this instance 
        /// and returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <param name="other">Other <see cref="StackInt"/> to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger.</returns>
        public int CompareTo(StackInt other)
        {
            return value.CompareTo(other.value);
        }

        /// <summary>
        /// Checks if the given object is of type <see cref="StackInt"/> and then compares its value with 
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
            if (!(obj is StackInt))
                throw new ArgumentException($"Object must be of type {nameof(StackInt)}");

            return CompareTo((StackInt)obj);
        }

        /// <summary>
        /// Checks if the value of the given <see cref="StackInt"/> is equal to the value of this instance.
        /// </summary>
        /// <param name="other">Other <see cref="StackInt"/> value to compare to this instance.</param>
        /// <returns>true if the value is equal to the value of this instance; otherwise, false.</returns>
        public bool Equals(StackInt other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Checks if the given object is of type <see cref="StackInt"/> and if its value is equal to the value of this instance.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>
        /// true if value is an instance of <see cref="StackInt"/> 
        /// and equals the value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            else if (obj is StackInt si)
            {
                return Equals(si);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        /// <summary>
        /// Converts the value of the current instance to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the value of the current instance.</returns>
        public override string ToString()
        {
            return value.ToString();
        }

        #endregion

    }
}
