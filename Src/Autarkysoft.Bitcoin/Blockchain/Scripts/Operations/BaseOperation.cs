// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base (abstract) class for all operations. Has basic methods shared among <see cref="IOperation"/>s 
    /// and Implements overrides for Equals() and GetHashCode() methods.
    /// </summary>
    public abstract class BaseOperation : IOperation
    {
        /// <summary>
        /// When overriden in child classes, inticates the type of the opeartion.
        /// </summary>
        public abstract OP OpValue { get; }

        /// <summary>
        /// When overriden, performs the action defined by the operation instance on the given stack. 
        /// Return value indicates success.
        /// </summary>
        /// <param name="opData">
        /// An advanced form of <see cref="System.Collections.Stack"/> that holds the required data 
        /// used by the <see cref="IOperation"/>s.
        /// </param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public abstract bool Run(IOpData opData, out string error);


        /// <summary>
        /// Checks if the total number of items in the stack (and alt stack) is below the allowed limit.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if total number of items in the stack is below the limit, false if otherwise</returns>
        protected bool CheckItemCount(IOpData opData, out string error)
        {
            // Note that there is no need to perform this check after all of the operations.
            // It is only needed when the OP _only_ adds items or adds more items than it removes.
            // Example: DupOp -> Adds 1 new item, CheckSigOp -> removes 2 then add 1, ArithmeticOps -> remove 1/2/3 then add 1

            if (opData.ItemCount + opData.AltItemCount <= 1000)
            {
                error = null;
                return true;
            }
            else
            {
                error = Err.OpStackItemOverflow;
                return false;
            }
        }


        /// <summary>
        /// Determines whether a given byte array could be encoded with minimal number of bytes
        /// using a number OP code.
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if the given byte array could be encoded using an OP code instead</returns>
        protected bool HasNumOp(byte[] data)
        {
            if (data.Length == 0)
            {
                return true;
            }
            else if (data.Length == 1)
            {
                if (data[0] == 0b10000001 /*-1*/ || data[0] >= 0 && data[0] <= 16)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Checks whether a given byte array is zero (...,0,0,0,0) or negative zero (...,0,0,0x80)
        /// <para/> This is the same as IsTrue()
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if given bytes represented zero or negative zero; False if otherwise.</returns>
        protected bool IsNotZero(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    // Can be negative zero
                    if (i == data.Length - 1 && data[i] == 0x80)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Converts the given integer to its byte array representation in little-endian and in shortest form for scripts.
        /// </summary>
        /// <param name="val">Integer to use</param>
        /// <returns>An array of bytes</returns>
        protected byte[] IntToByteArray(long val)
        {
            if (val == 0)
            {
                return new byte[0];
            }

            bool isNeg = val < 0;
            if (isNeg)
            {
                val = -val;
            }

            byte[] data = new byte[sizeof(long) + 1];
            int i = 0;
            while (val > 0)
            {
                data[i++] = (byte)val;
                val >>= 8;
            }

            if (isNeg)
            {
                // The highest bit (if wasn't set) must be set to 1.
                if ((data[i - 1] & 0b1000_0000) == 0)
                {
                    data[i - 1] |= 0b1000_0000;
                }
                else
                {
                    data[i++] = 0b1000_0000;
                }
            }
            else if ((data[i - 1] & 0b1000_0000) != 0) // && isPositive
            {
                // An additional 0 is needed if the highest bit is set.
                i++;
            }

            byte[] result = new byte[i];
            Buffer.BlockCopy(data, 0, result, 0, i);
            return result;
        }

        /// <summary>
        /// Determines whether the given <see cref="OP"/> code is one of the number OPs.
        /// </summary>
        /// <param name="val">OP code to check</param>
        /// <returns>
        /// True if the value is one of <see cref="OP._0"/>, <see cref="OP.Negative1"/> or from 
        /// <see cref="OP._1"/> to <see cref="OP._16"/>, false if otherwise.
        /// </returns>
        protected bool IsNumberOp(OP val)
        {
            return !(val != OP._0 && val != OP.Negative1 && val < OP._1 || val > OP._16);
        }


        /// <summary>
        /// Converts the given byte array (data from stack not scripts) to a 64-bit integer.
        /// The main usage is in 
        /// <para/> ArithmeticOps -> 4 byte max, value between (-2^31 +1) and(2^31 -1) (0xffffff7f)
        /// <para/> Multisig for m and n values -> 1 byte max, value between 0 and 20
        /// <para/> Locktime -> 5 bytes max, value between 0 and(2^39-1) (0xffffffff7f)
        /// </summary>
        /// <remarks>
        /// https://github.com/bitcoin/bitcoin/blob/e8e79958a7b2a0bf1b02adcce9f4d811eac37dfc/src/script/script.h#L221-L248
        /// </remarks>
        /// <param name="data">Byte array to use</param>
        /// <param name="result">The resulting integer or zero in case of failure</param>
        /// <param name="isStrict">
        /// [Default value = true] Enforces strict rules for shortest encoding (1 = {1} instead of {1,0,0})
        /// </param>
        /// <param name="maxDataLength">[Default value = 4] Maximum number of bytes allowed to exist in the data</param>
        /// <returns>True if conversion was successful, false if otherwise</returns>
        protected bool TryConvertToLong(byte[] data, out long result, bool isStrict = true, int maxDataLength = 4)
        {
            result = 0;
            if (data == null || data.Length > maxDataLength)
            {
                return false;
            }

            if (data.Length == 0)
            {
                return true;
            }

            if (isStrict && (data[^1] & 0b0111_1111) == 0)
            {
                if (data.Length <= 1 || (data[^2] & 0b1000_0000) == 0)
                {
                    return false;
                }
            }

            // If the most significant bit of the most significant byte of data was set, the result should be negative
            // and that bit should be removed here.
            bool isNeg = false;
            if ((data[^1] & 0b1000_0000) != 0)
            {
                isNeg = true;
                result = (long)(data[^1] & 0b0111_1111) << ((data.Length - 1) * 8);
            }
            else
            {
                result = (long)data[^1] << ((data.Length - 1) * 8);
            }

            for (int i = 0; i < data.Length - 1; i++)
            {
                result |= (long)data[i] << (i * 8);
            }

            if (isNeg)
            {
                result = -result;
            }

            return true;
        }

        /// <summary>
        /// Converts the given integer (between -1 and 16) to its equal <see cref="OP"/> code. 
        /// Return value indicates success.
        /// </summary>
        /// <param name="val">Integer to convert (only works for values from -1 to 16)</param>
        /// <param name="op">Resulting OP code.</param>
        /// <returns>True if conversion was successful, false if otherwise</returns>
        protected bool TryConvertToOp(int val, out OP op)
        {
            if (val == 0)
            {
                op = OP._0;
            }
            else if (val == -1)
            {
                op = OP.Negative1;
            }
            else if (val >= 1 && val <= 16)
            {
                // OP_1 = 0x51, OP_2 = 0x52,...
                op = (OP)(val + 0x50);
            }
            else
            {
                op = OP._0;
                return false;
            }

            return true;
        }


        /// <inheritdoc/>
        public virtual void WriteToStream(FastStream stream) => stream.Write((byte)OpValue);

        /// <inheritdoc/>
        public virtual void WriteToStreamForSigning(FastStream stream) => stream.Write((byte)OpValue);


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object, flase if otherwise.</returns>
        public override bool Equals(object obj) => obj is IOperation op && op.OpValue == OpValue;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode() => HashCode.Combine(OpValue);
    }
}
