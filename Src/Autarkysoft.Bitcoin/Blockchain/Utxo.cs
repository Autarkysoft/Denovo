// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Implements <see cref="IUtxo"/> and <see cref="IDeserializable"/>.
    /// </summary>
    public class Utxo : IUtxo, IDeserializable
    {
        /// <summary>
        /// Initializes a new empty instance of <see cref="Utxo"/>.
        /// </summary>
        public Utxo()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Utxo"/> using the given parameters.
        /// </summary>
        /// <param name="index">Index of the output</param>
        /// <param name="amount">Amount value of the output</param>
        /// <param name="pubScr">Locking script of the output</param>
        public Utxo(uint index, ulong amount, IPubkeyScript pubScr)
        {
            Index = index;
            Amount = amount;
            PubScript = pubScr;
        }


        /// <inheritdoc/>
        public bool IsMempoolSpent { get; set; }
        /// <inheritdoc/>
        public bool IsBlockSpent { get; set; }

        /// <inheritdoc/>
        public uint Index { get; set; }
        /// <inheritdoc/>
        public ulong Amount { get; set; }

        private IPubkeyScript _pubScr = new PubkeyScript();
        /// <inheritdoc/>
        public IPubkeyScript PubScript
        {
            get => _pubScr;
            set => _pubScr = value ?? new PubkeyScript();
        }

        /// <inheritdoc/>
        public void AddSerializedSize(SizeCounter counter)
        {
            counter.Add(4 + 8); // Index + Amount
            PubScript.AddSerializedSize(counter);
        }

        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            stream.Write(Index);
            stream.Write(Amount);
            PubScript.Serialize(stream);
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }

            if (!stream.CheckRemaining(sizeof(uint) + sizeof(ulong)))
            {
                error = Errors.EndOfStream;
                return false;
            }

            Index = stream.ReadUInt32Checked();
            Amount = stream.ReadUInt64Checked();

            return PubScript.TryDeserialize(stream, out error);
        }
    }
}
