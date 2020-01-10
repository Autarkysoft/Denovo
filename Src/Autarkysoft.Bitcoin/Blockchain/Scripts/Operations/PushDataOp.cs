// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operations that push some data onto the stack. Covers number OPs from <see cref="OP._0"/> to <see cref="OP._16"/>, 
    /// push OPs from byte=0x01 to 0x4b and <see cref="OP.PushData1"/>, <see cref="OP.PushData2"/> and <see cref="OP.PushData4"/>.
    /// </summary>
    public class PushDataOp : BaseOperation
    {
        /// <summary>
        /// Initializes an empty new instance of <see cref="PushDataOp"/>. 
        /// Can be used for reading data from a stream.
        /// </summary>
        public PushDataOp()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using the given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <param name="ba">Byte array to use</param>
        public PushDataOp(byte[] ba)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            if (HasNumOp(ba))
                throw new ArgumentException("Short form of data exists with OP codes which should be used instead.");

            data = ba.CloneByteArray();
            StackInt size = new StackInt(ba.Length);
            _opVal = size.GetOpCode();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="script">
        /// Script to use (will be converted to byte array using the <see cref="IScript.ToByteArray()"/> method)
        /// </param>
        public PushDataOp(IScript script)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script), "Script can not be null.");

            data = script.ToByteArray();
            StackInt size = new StackInt(data.Length);
            _opVal = size.GetOpCode();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using one of the number OP codes.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="numOp">A number OP code (OP_0, OP_1, ..., OP_16 and OP_Negative1)</param>
        public PushDataOp(OP numOp)
        {
            if (!IsNumberOp(numOp))
                throw new ArgumentException("OP is not a number OP.");

            data = null;
            _opVal = numOp;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using the given integer.
        /// </summary>
        /// <param name="num">Integer value to use</param>
        public PushDataOp(int num)
        {
            if (num >= -1 && num <= 16) // We have OP for these
            {
                _ = TryConvertToOp(num, out _opVal);
                data = null;
            }
            else // There is no OP code, we have to use regular push
            {
                data = IntToByteArray(num);
                StackInt size = new StackInt(data.Length);
                _opVal = size.GetOpCode();
            }
        }



        private OP _opVal;
        /// <summary>
        /// A single byte inticating type of the opeartion.
        /// (Some push OP codes don't have a name)
        /// </summary>
        public override OP OpValue => _opVal;
        internal byte[] data;



        /// <summary>
        /// Pushes the specified data of this instance at the top of the stack.
        /// </summary>
        /// <param name="opData">Stack to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (OpValue == OP._0)
            {
                opData.Push(new byte[0]);
            }
            else if (OpValue == OP.Negative1)
            {
                opData.Push(new byte[1] { 0b1000_0001 });
            }
            else if (OpValue >= OP._1 && OpValue <= OP._16)
            {
                // OP_1=0x51, OP_2=0x52, ...
                opData.Push(new byte[] { (byte)(OpValue - 0x50) });
            }
            else
            {
                opData.Push(data);
            }

            error = null;
            return true;
        }


        /// <summary>
        /// If possible, will convert the data of this instance to a 64-bit signed integer. The return value indicates success. 
        /// The main usage is in 
        /// <para/> ArithmeticOps -> 4 byte max, value between (-2^31 +1) and(2^31 -1) (0xffffff7f)
        /// <para/> Multisig for m and n values -> 1 byte max, value between 0 and 20
        /// <para/> Locktime -> 5 bytes max, value between 0 and(2^39-1) (0xffffffff7f)
        /// </summary>
        /// <param name="result">The converted 64-bit signed integer or zero in case of failure</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <param name="isStrict">[Default value = true] Indicates whether to use strict rules</param>
        /// <param name="maxDataLength">[Default value = 4] Maximum number of bytes allowed to exist in the data</param>
        /// <returns>True if deserialization was successful, false if otherwise</returns>
        public bool TryGetNumber(out long result, out string error, bool isStrict = true, int maxDataLength = 4)
        {
            if (data == null)
            {
                if (OpValue == OP._0)
                {
                    result = 0;
                }
                else if (OpValue == OP.Negative1)
                {
                    result = -1;
                }
                else if (OpValue >= OP._1 && OpValue <= OP._16)
                {
                    result = (byte)OpValue - 0x50;
                }
                else
                {
                    result = 0;
                    error = "No data is available.";
                    return false;
                }
            }
            else
            {
                if (!TryConvertToLong(data, out result, isStrict, maxDataLength))
                {
                    error = "Invalid number format.";
                    return false;
                }
            }

            error = null;
            return true;
        }


        /// <summary>
        /// Reads the push data from the given byte array starting from the specified offset. The return value indicates success.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <param name="isWitness">Indicates whether this operation is inside a witness script</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public bool TryRead(FastStreamReader stream, out string error, bool isWitness = false)
        {
            if (stream is null || !stream.TryReadByte(out byte firstByte))
            {
                error = Err.EndOfStream;
                return false;
            }


            // TODO: set a bool for isStrict so that we can reject non-strict encoded lengths

            _opVal = (OP)firstByte;
            if (firstByte == (byte)OP._0 || firstByte == (byte)OP.Negative1 ||
                firstByte >= (byte)OP._1 && firstByte <= (byte)OP._16)
            {
                data = null;
                error = null;
                return true; // This has to return here since we set the data at the bottom after if block ends
            }
            else if (isWitness)
            {
                if (!CompactInt.TryRead(stream, out CompactInt size, out error))
                {
                    return false;
                }

                // TODO: change this with maximum allowed push data size
                if (size > int.MaxValue)
                {
                    error = "Push data size is too big.";
                    return false;
                }

                if (!stream.TryReadByteArray((int)size, out data))
                {
                    error = Err.EndOfStream;
                    return false;
                }
            }
            else
            {
                if (!StackInt.TryRead(stream, out StackInt size, out error))
                {
                    return false;
                }

                if (size > int.MaxValue)
                {
                    error = "Push data size is too big.";
                    return false;
                }

                if (!stream.TryReadByteArray((int)size, out data))
                {
                    error = Err.EndOfStream;
                    return false;
                }
            }

            error = null;
            return true;
        }


        /// <summary>
        /// Writes this operation's data to the given stream.
        /// Used by <see cref="IDeserializable.Serialize(FastStream)"/> methods 
        /// (not to be confused with what <see cref="Run(IOpData, out string)"/> does).
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="isWitness">[Default value = false] Indicates whether this operation is inside a witness script</param>
        public void WriteToStream(FastStream stream, bool isWitness = false)
        {
            if (OpValue == OP._0 || OpValue == OP.Negative1)
            {
                stream.Write((byte)OpValue);
            }
            else if (OpValue >= OP._1 && OpValue <= OP._16)
            {
                stream.Write((byte)OpValue);
            }
            else if (isWitness)
            {
                CompactInt size = new CompactInt(data.Length);
                size.WriteToStream(stream);
                stream.Write(data);
            }
            else
            {
                StackInt size = new StackInt(data.Length);
                size.WriteToStream(stream);
                stream.Write(data);
            }
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object, flase if otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is PushDataOp op)
            {
                if (op.OpValue == OpValue)
                {
                    if (op.data == null)
                    {
                        return data == null;
                    }
                    else
                    {
                        return ((ReadOnlySpan<byte>)op.data).SequenceEqual(data);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode()
        {
            if (data == null)
            {
                return OpValue.GetHashCode();
            }
            else
            {
                int hash = 17;
                foreach (var b in data)
                {
                    hash = hash * 31 + b;
                }
                return hash;
            }
        }

    }
}
