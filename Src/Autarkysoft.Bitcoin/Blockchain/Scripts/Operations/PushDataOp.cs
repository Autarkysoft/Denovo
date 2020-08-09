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
    /// <remarks>
    /// Constructors of this class are stricter than consensus rules. For example, they will reject byte arrays that have 
    /// OP_numbers (short form) and reject any byte array that is bigger than <see cref="Constants.MaxScriptItemLength"/>
    /// even though it could be a valid <see cref="PushDataOp"/>. 
    /// See https://bitcoin.stackexchange.com/a/93664/87716 and test cases for more information.
    /// </remarks>
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
        /// Throws an <see cref="ArgumentException"/> if there is an OP_number available equal to the value of the byte array.
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
            if (ba.Length > Constants.MaxScriptItemLength)
            {
                throw new ArgumentOutOfRangeException(nameof(ba),
                    $"Data to be pushed to the stack can not be bigger than {Constants.MaxScriptItemLength} bytes.");
            }

            data = ba.CloneByteArray();
            StackInt size = new StackInt(ba.Length);
            _opVal = size.GetOpCode();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="script">Script to use</param>
        public PushDataOp(IScript script)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script), "Script can not be null.");
            if (script.Data.Length > Constants.MaxScriptItemLength)
            {
                throw new ArgumentOutOfRangeException(nameof(script),
                    $"Script byte size to be pushed to the stack can not be bigger than {Constants.MaxScriptItemLength} bytes.");
            }

            data = script.Data.CloneByteArray();
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
                throw new ArgumentException("Given OP code is not a number OP.");

            data = null;
            _opVal = numOp;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using the given integer.
        /// </summary>
        /// <param name="num">Integer value to use</param>
        public PushDataOp(long num)
        {
            if (num >= -1 && num <= 16) // We have OP for these
            {
                _ = TryConvertToOp((int)num, out _opVal);
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
        // Don't rename (reflection used by tests)
        internal byte[] data;
        private byte[] stackIntBytes = null;



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
                if (data.Length > Constants.MaxScriptItemLength)
                {
                    error = $"Item to be pushed to the stack can not be bigger than {Constants.MaxScriptItemLength} bytes.";
                    return false;
                }
                opData.Push(data);
            }

            return CheckItemCount(opData, out error);
        }


        /// <summary>
        /// If possible, will convert the data of this instance to a 64-bit signed integer. The return value indicates success. 
        /// The main usage is in:
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
                else
                {
                    result = (byte)OpValue - 0x50;
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
        /// Reads the push data from the given stream. The return value indicates success.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public bool TryRead(FastStreamReader stream, out string error)
        {
            // Since the first byte could be an OP_num or it could be the length we only peek at it
            if (!stream.TryPeekByte(out byte firstByte))
            {
                error = Err.EndOfStream;
                return false;
            }

            _opVal = (OP)firstByte;
            if (firstByte == (byte)OP._0 || firstByte == (byte)OP.Negative1 ||
                (firstByte >= (byte)OP._1 && firstByte <= (byte)OP._16))
            {
                stream.SkipOneByte();
                data = null;
            }
            else
            {
                if (!StackInt.TryRead(stream, out stackIntBytes, out StackInt size, out error))
                {
                    return false;
                }

                // Size check should only take place when "Running" the OP.
                // Only a quick check to prevent data loss while casting.
                if (size > int.MaxValue)
                {
                    error = "Data size is too big.";
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
        /// Reads the push data from the given stream as a witness item. 
        /// Expects the following encoding: [CompactInt Data.length][Data]
        /// The return value indicates success.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public bool TryReadWitness(FastStreamReader stream, out string error)
        {
            // Since the first byte could be an OP_num or it could be the length we only peek at it
            if (!stream.TryPeekByte(out byte firstByte))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (firstByte == (byte)OP._0 || firstByte == (byte)OP.Negative1 ||
                (firstByte >= (byte)OP._1 && firstByte <= (byte)OP._16))
            {
                stream.SkipOneByte();
                _opVal = (OP)firstByte;
                data = null;
            }
            else
            {
                if (!CompactInt.TryRead(stream, out CompactInt size, out error))
                {
                    return false;
                }

                // There is no size restriction for witnesses
                // https://github.com/bitcoin/bips/blob/cb071df902eafb7054635201a8b12e76f42774ad/bip-0141.mediawiki#new-script-system
                // Only a quick check to prevent data loss while casting
                if (size > int.MaxValue)
                {
                    error = "Data size is too big.";
                    return false;
                }

                if (!stream.TryReadByteArray((int)size, out data))
                {
                    error = Err.EndOfStream;
                    return false;
                }
                // Hack OpValue for equality comparisons
                _opVal = new StackInt((int)size).GetOpCode();
            }

            error = null;
            return true;
        }


        /// <inheritdoc/>
        public override void WriteToStream(FastStream stream)
        {
            if (OpValue == OP._0 || OpValue == OP.Negative1 || (OpValue >= OP._1 && OpValue <= OP._16))
            {
                stream.Write((byte)OpValue);
            }
            else
            {
                if (stackIntBytes != null)
                {
                    stream.Write(stackIntBytes);
                }
                else
                {
                    StackInt size = new StackInt(data.Length);
                    size.WriteToStream(stream);
                }
                stream.Write(data);
            }
        }

        /// <summary>
        /// Writes this operation's data to the given stream as a witness: [CompactInt Data.length][Data].
        /// Used by <see cref="IDeserializable.Serialize(FastStream)"/> methods 
        /// (not to be confused with what <see cref="Run(IOpData, out string)"/> does).
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void WriteToWitnessStream(FastStream stream)
        {
            if (OpValue == OP._0 || OpValue == OP.Negative1 || (OpValue >= OP._1 && OpValue <= OP._16))
            {
                stream.Write((byte)OpValue);
            }
            else
            {
                stream.WriteWithCompactIntLength(data);
            }
        }

        /// <inheritdoc/>
        public override void WriteToStreamForSigning(FastStream stream, ReadOnlySpan<byte> sig)
        {
            if (data == null)
            {
                stream.Write((byte)OpValue);
            }
            else if (stackIntBytes != null)
            {
                // This means the push used a wrong encoded (non-strict) StackInt hence the sig is not removed
                // even if the signature data is the same.
                stream.Write(stackIntBytes);
                stream.Write(data);
            }
            else if (!sig.SequenceEqual(data))
            {
                StackInt size = new StackInt(data.Length);
                size.WriteToStream(stream);
                stream.Write(data);
            }
        }


        /// <inheritdoc/>
        public override void WriteToStreamForSigning(FastStream stream, byte[][] sigs)
        {
            if (OpValue == OP._0 || OpValue == OP.Negative1 || (OpValue >= OP._1 && OpValue <= OP._16))
            {
                stream.Write((byte)OpValue);
            }
            else
            {
                if (stackIntBytes != null)
                {
                    stream.Write(stackIntBytes);
                }
                else
                {
                    foreach (ReadOnlySpan<byte> item in sigs)
                    {
                        if (item.SequenceEqual(data))
                        {
                            return;
                        }
                    }
                    StackInt size = new StackInt(data.Length);
                    size.WriteToStream(stream);
                }

                stream.Write(data);
            }
        }


        /// <inheritdoc/>
        public override void WriteToStreamForSigningSegWit(FastStream stream) => WriteToWitnessStream(stream);


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
                        return ((ReadOnlySpan<byte>)op.data).SequenceEqual(data) &&
                               ((ReadOnlySpan<byte>)op.stackIntBytes).SequenceEqual(stackIntBytes);
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
