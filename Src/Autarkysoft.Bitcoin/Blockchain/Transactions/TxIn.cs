// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Transactions
{
    /// <summary>
    /// The input of transactions containing outpoint, <see cref="ISignatureScript"/> and sequence.
    /// Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public class TxIn : IDeserializable
    {
        /// <summary>
        /// Initializes a new and empty instance of <see cref="TxIn"/>.
        /// </summary>
        public TxIn()
        {
        }

        /// <summary>
        /// Initializes a new and empty instance of <see cref="TxIn"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">The outpoint's transaction hash</param>
        /// <param name="index">The outpoint's index</param>
        /// <param name="sigScript">Signature script</param>
        /// <param name="sequence">Sequence</param>
        public TxIn(byte[] hash, uint index, ISignatureScript sigScript, uint sequence)
        {
            TxHash = hash;
            Index = index;
            SigScript = sigScript;
            Sequence = sequence;
        }



        private byte[] _txHash;
        /// <summary>
        /// The transaction hash of the input used in this instance (Outpoint.Hash).
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] TxHash
        {
            get => _txHash;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(TxHash), "Transaction hash can not be null.");
                if (value.Length != 32)
                    throw new ArgumentOutOfRangeException(nameof(TxHash), "Transaction hash has to be 32 bytes long.");

                _txHash = value;
            }
        }

        private uint _index;
        /// <summary>
        /// The output index of the input used in this instance (Outpoint.Index).
        /// </summary>
        public uint Index
        {
            get => _index;
            set => _index = value;
        }

        private ISignatureScript _sigScr = new SignatureScript();
        /// <summary>
        /// The input's signature script
        /// </summary>
        public ISignatureScript SigScript
        {
            get => _sigScr;
            set => _sigScr = (value is null) ? new SignatureScript() : value;
        }

        // TODO: take a look at BIP68 and maybe implement a new "Sequence" struct
        // https://github.com/bitcoin/bips/blob/master/bip-0068.mediawiki
        private uint _sequence;
        /// <summary>
        /// Input's sequence
        /// </summary>
        public uint Sequence
        {
            get => _sequence;
            set => _sequence = value;
        }


        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            stream.Write(TxHash);
            stream.Write(Index);
            SigScript.Serialize(stream);
            stream.Write(Sequence);
        }


        /// <summary>
        /// Converts this instance into its byte array representation using the given <see cref="IOperation"/> list 
        /// from the executing script.
        /// This is used for signing where <see cref="SignatureScript"/> needs to be changed for the input being signed.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="spendScript">Serialization of the script being spent</param>
        /// <param name="setSeqToZero">
        /// Sequences are set to 0 for both <see cref="Cryptography.Asymmetric.EllipticCurve.SigHashType.None"/>
        /// and <see cref="Cryptography.Asymmetric.EllipticCurve.SigHashType.Single"/> but they are unchanged for others.
        ///</param>
        public void SerializeForSigning(FastStream stream, byte[] spendScript, bool setSeqToZero = false)
        {
            stream.Write(TxHash);
            stream.Write(Index);
            stream.WriteWithCompactIntLength(spendScript);

            if (setSeqToZero)
            {
                stream.Write(new byte[4]);
            }
            else
            {
                stream.Write(Sequence);
            }
        }


        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadByteArray(32, out _txHash))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!stream.TryReadUInt32(out _index))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!SigScript.TryDeserialize(stream, out error))
            {
                return false;
            }

            if (!stream.TryReadUInt32(out _sequence))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
