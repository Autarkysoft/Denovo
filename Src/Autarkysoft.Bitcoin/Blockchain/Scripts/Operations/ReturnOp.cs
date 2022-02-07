// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation that is used to include an arbitrary data in transactions, it will fail on running.
    /// </summary>
    public class ReturnOp : NotRunableOps
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ReturnOp"/> with an empty data (only has 0x6a).
        /// </summary>
        public ReturnOp()
        {
            data = new byte[1] { (byte)OP.RETURN };
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ReturnOp"/> using the given data.
        /// </summary>
        /// <param name="ba">Data to use (can be null)</param>
        /// <param name="usePushOp">
        /// [Default value = true]
        /// If true, the data will be included after <see cref="OP.RETURN"/> using <see cref="PushDataOp"/> scheme.
        /// </param>
        public ReturnOp(byte[] ba, bool usePushOp = true)
        {
            if (ba == null || ba.Length == 0)
            {
                data = new byte[1] { (byte)OP.RETURN };
            }
            else if (usePushOp)
            {
                StackInt size = new StackInt(ba.Length);
                FastStream stream = new FastStream(ba.Length + 2);
                stream.Write((byte)OP.RETURN);
                size.WriteToStream(stream);
                stream.Write(ba);
                data = stream.ToByteArray();
            }
            else
            {
                data = new byte[ba.Length + 1];
                data[0] = (byte)OP.RETURN;
                Buffer.BlockCopy(ba, 0, data, 1, ba.Length);
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ReturnOp"/> using the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="scr">Script to use</param>
        /// <param name="usePushOp">
        /// [Default value = true]
        /// If true, the data will be included after <see cref="OP.RETURN"/> using <see cref="PushDataOp"/> scheme.
        /// </param>
        public ReturnOp(IScript scr, bool usePushOp = true)
        {
            if (scr == null)
                throw new ArgumentNullException(nameof(scr), "Script can not be null.");

            byte[] temp = scr.Data;

            if (usePushOp)
            {
                StackInt size = new StackInt(temp.Length);
                FastStream stream = new FastStream(temp.Length + 2);
                stream.Write((byte)OP.RETURN);
                size.WriteToStream(stream);
                stream.Write(temp);
                data = stream.ToByteArray();
            }
            else
            {
                data = new byte[temp.Length + 1];
                data[0] = (byte)OP.RETURN;
                Buffer.BlockCopy(temp, 0, data, 1, temp.Length);
            }
        }



        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.RETURN;

        // Don't rename (reflection used in tests)
        private byte[] data;



        /// <summary>
        /// Reads the <see cref="OP.RETURN"/> byte and the following specified data length from the specified offset. 
        /// The return value indicates success.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="length">
        /// Length of the data to read. Must be at least 1 byte, the <see cref="OP.RETURN"/> itself. 
        /// (note that <see cref="OP.RETURN"/> doesn't have any internal mechanism to tell us how much data it holds, 
        /// the length is instead specified before <see cref="OP.RETURN"/> as the length of the whole script).
        /// </param>
        /// <param name="error">Error message</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public bool TryRead(FastStreamReader stream, int length, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }

            // We need the first byte of "data" to be 0x6a so we only peek at that byte.
            if (!stream.TryPeekByte(out byte firstByte))
            {
                error = Errors.EndOfStream;
                return false;
            }
            if (firstByte != (byte)OP.RETURN)
            {
                error = Errors.WrongOpReturnByte;
                return false;
            }
            if (length < 1)
            {
                error = Errors.ShortOpReturn;
                return false;
            }

            if (!stream.TryReadByteArray(length, out data))
            {
                error = Errors.EndOfStream;
                return false;
            }

            error = Errors.None;
            return true;
        }

        /// <inheritdoc/>
        public override void WriteToStream(FastStream stream) => stream.Write(data); // "data" is at least 0x6a

        /// <inheritdoc/>
        public override void WriteToStreamForSigning(FastStream stream, ReadOnlySpan<byte> sig) => stream.Write(data);

        /// <inheritdoc/>
        public override void WriteToStreamForSigning(FastStream stream, byte[][] sigs) => stream.Write(data);

        /// <inheritdoc/>
        public override void WriteToStreamForSigningSegWit(FastStream stream) => stream.Write(data);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is ReturnOp op && ((ReadOnlySpan<byte>)op.data).SequenceEqual(data);

        /// <inheritdoc/>
        public override int GetHashCode()
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
