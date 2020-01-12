// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using System;

namespace Autarkysoft.Bitcoin
{
    // TODO: 
    // https://bitcoin.stackexchange.com/questions/5914/how-is-locktime-enforced-in-the-standard-client
    // https://github.com/bitcoin/bips/blob/master/bip-0068.mediawiki
    // https://en.bitcoin.it/wiki/NLockTime

    /// <summary>
    /// The block number or timestamp at which the transaction is unlocked.
    /// <para/>All explicit operations convert values without throwing any exceptions
    /// (data may be lost, caller must consider this).
    /// </summary>
    /// <remarks>
    /// LockTime is interpreted based on sequences and BIP68
    /// </remarks>
    public readonly struct LockTime : IComparable, IComparable<LockTime>, IEquatable<LockTime>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LockTime"/> using a 32-bit signed integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use (must be >= 0)</param>
        public LockTime(int val)
        {
            if (val < 0)
                throw new ArgumentOutOfRangeException(nameof(val), "Locktime value can not be negative.");

            value = (uint)val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LockTime"/> using a 32-bit unsigned integer.
        /// </summary>
        /// <param name="val">Value to use</param>
        public LockTime(uint val)
        {
            value = val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LockTime"/> using a DateTime instance.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use</param>
        public LockTime(DateTime val)
        {
            long num = UnixTimeStamp.TimeToEpoch(val);
            if (num < Threshold || num >= uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(val), "DateTime value is not a valid Locktime.");
            }
            value = (uint)num;
        }



        // Don't rename (reflection used in tests).
        private readonly uint value;

        /// <summary>
        /// Any values bigger than this threshold will be interpretted as <see cref="DateTime"/> 
        /// otherwise as block height.
        /// </summary>
        public const uint Threshold = 500_000_000U;



        /// <summary>
        /// The minimum LockTime value (0).
        /// </summary>
        public static LockTime Minimum => new LockTime(0U);

        /// <summary>
        /// The maximum LockTime value (0xFFFFFFFF).
        /// </summary>
        public static LockTime Maximum => new LockTime(uint.MaxValue);



        /// <summary>
        /// Reads the <see cref="LockTime"/> value from the given <see cref="FastStreamReader"/>. 
        /// Return value indicates success.
        /// </summary>
        /// <param name="stream">Stream containing the <see cref="LockTime"/></param>
        /// <param name="result">The result</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public static bool TryRead(FastStreamReader stream, out LockTime result, out string error)
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

            result = new LockTime(val);
            error = null;
            return true;
        }


        /// <summary>
        /// Converts this value to its byte array representation in little-endian order 
        /// and writes the result to the given <see cref="FastStream"/>.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void WriteToStream(FastStream stream)
        {
            stream.Write(value);
        }



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator LockTime(uint val) => new LockTime(val);
        public static implicit operator LockTime(ushort val) => new LockTime(val);
        public static implicit operator LockTime(byte val) => new LockTime(val);
        public static explicit operator LockTime(int val) => new LockTime((uint)val);

        public static implicit operator uint(LockTime val) => val.value;
        public static explicit operator ushort(LockTime val) => (ushort)val.value;
        public static explicit operator byte(LockTime val) => (byte)val.value;
        public static explicit operator int(LockTime val) => (int)val.value;


        public static explicit operator LockTime(DateTime val)
        {
            long num = UnixTimeStamp.TimeToEpoch(val);
            if (num < Threshold || num >= uint.MaxValue)
            {
                return Maximum;
            }
            return new LockTime((uint)num);
        }
        public static explicit operator DateTime(LockTime val)
        {
            if (val.value < Threshold || val.value == uint.MaxValue)
            {
                return UnixTimeStamp.EpochToTime(0);
            }
            return UnixTimeStamp.EpochToTime(val.value);
        }


        public static bool operator >(LockTime left, LockTime right) => left.value > right.value;
        public static bool operator >(LockTime left, int right) => right < 0 || left.value > (ulong)right;
        public static bool operator >(int left, LockTime right) => left > 0 && (ulong)left > right.value;

        public static bool operator >=(LockTime left, LockTime right) => left.value >= right.value;
        public static bool operator >=(LockTime left, int right) => right < 0 || left.value >= (ulong)right;
        public static bool operator >=(int left, LockTime right) => left > 0 && (ulong)left >= right.value;

        public static bool operator <(LockTime left, LockTime right) => left.value < right.value;
        public static bool operator <(LockTime left, int right) => right > 0 && left.value < (ulong)right;
        public static bool operator <(int left, LockTime right) => left < 0 || (ulong)left < right.value;

        public static bool operator <=(LockTime left, LockTime right) => left.value <= right.value;
        public static bool operator <=(LockTime left, int right) => right > 0 && left.value <= (ulong)right;
        public static bool operator <=(int left, LockTime right) => left < 0 || (ulong)left <= right.value;

        public static bool operator ==(LockTime left, LockTime right) => left.value == right.value;
        public static bool operator ==(LockTime left, int right) => right > 0 && left.value == (ulong)right;
        public static bool operator ==(int left, LockTime right) => left > 0 && (ulong)left == right.value;

        public static bool operator !=(LockTime left, LockTime right) => left.value != right.value;
        public static bool operator !=(LockTime left, int right) => right < 0 || left.value != (ulong)right;
        public static bool operator !=(int left, LockTime right) => left < 0 || (ulong)left != right.value;

        public static LockTime operator ++(LockTime lt) => lt.value == uint.MaxValue ? Maximum : new LockTime(lt.value + 1);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        /// <summary>
        /// Returns a new instance of <see cref="LockTime"/> with its value increased by one up to its maximum value
        /// without changing this instance. If you want to change this instance's value use the ++ operator.
        /// </summary>
        public LockTime Increment()
        {
            return value == uint.MaxValue ? Maximum : new LockTime(value + 1);
        }



        #region Interfaces and overrides

        /// <summary>
        /// Compares the value of a given <see cref="LockTime"/> with the value of this instance and 
        /// Returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <param name="other">Other <see cref="LockTime"/> to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger.</returns>
        public int CompareTo(LockTime other)
        {
            return value.CompareTo(other.value);
        }

        /// <summary>
        /// Checks if the given object is of type <see cref="LockTime"/> and then compares its value with the value of this instance.
        /// And returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger</returns>
        public int CompareTo(object obj)
        {
            if (obj is null)
                return 1;
            if (!(obj is LockTime))
                throw new ArgumentException($"Object must be of type {nameof(LockTime)}");

            return CompareTo((LockTime)obj);
        }

        /// <summary>
        /// Checks if the value of the given <see cref="LockTime"/> is equal to the value of this instance.
        /// </summary>
        /// <param name="other">Other <see cref="LockTime"/> value to compare to this instance.</param>
        /// <returns>true if the value is equal to the value of this instance; otherwise, false.</returns>
        public bool Equals(LockTime other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Checks if the given object is of type <see cref="LockTime"/> 
        /// and if its value is equal to the value of this instance.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>
        /// true if value is an instance of <see cref="LockTime"/> 
        /// and equals the value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            else if (obj is LockTime lt)
            {
                return Equals(lt);
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
            return (value < Threshold || value == uint.MaxValue) ? $"{value}" : $"{UnixTimeStamp.EpochToTime(value)}";
        }

        #endregion

    }
}
