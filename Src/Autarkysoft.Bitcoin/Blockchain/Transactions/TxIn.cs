// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Hashing;

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
        /// <param name="hash">The outpoint's transaction hash</param>
        /// <param name="index">The outpoint's index</param>
        /// <param name="sigScript">Signature script</param>
        /// <param name="sequence">Sequence</param>
        public TxIn(Digest256 hash, uint index, ISignatureScript sigScript, uint sequence)
        {
            TxHash = hash;
            Index = index;
            SigScript = sigScript;
            Sequence = sequence;
        }



        /// <summary>
        /// The transaction hash of the input used in this instance (Outpoint.Hash).
        /// </summary>
        public Digest256 TxHash { get; set; }

        /// <summary>
        /// The output index of the input used in this instance (Outpoint.Index).
        /// </summary>
        public uint Index { get; set; }

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
        public void AddSerializedSize(SizeCounter counter)
        {
            // Hash + Index + Sequence
            counter.Add(32 + 4 + 4);
            SigScript.AddSerializedSize(counter);
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
        public bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }

            if (!stream.CheckRemaining(32 + 4))
            {
                error = Errors.EndOfStream;
                return false;
            }

            TxHash = stream.ReadDigest256Checked();
            Index = stream.ReadUInt32Checked();

            if (!SigScript.TryDeserialize(stream, out error))
            {
                return false;
            }

            if (!stream.TryReadUInt32(out _sequence))
            {
                error = Errors.EndOfStream;
                return false;
            }

            return true;
        }
    }
}
