// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing a single transaction.
    /// <para/> Sent: unsolicited, in response to <see cref="PayloadType.GetData"/>
    /// </summary>
    public class TxPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="TxPayload"/>.
        /// </summary>
        public TxPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TxPayload"/> using the given <see cref="ITransaction"/>.
        /// </summary>
        /// <param name="tx">Transaction to send</param>
        public TxPayload(ITransaction tx)
        {
            Tx = tx;
        }


        private ITransaction _tx = new Transaction();
        /// <summary>
        /// The transaction to send
        /// </summary>
        public ITransaction Tx
        {
            get => _tx;
            set => _tx = (value is null) ? new Transaction() : value;
        }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Tx;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter) => Tx.AddSerializedSize(counter);

        /// <inheritdoc/>
        public override void Serialize(FastStream stream) => Tx.Serialize(stream);

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out Errors error) => Tx.TryDeserialize(stream, out error);
    }
}
