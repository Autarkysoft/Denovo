// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class TxPayload : PayloadBase
    {
        public Transaction TxData { get; set; }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Tx;


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            TxData.Serialize(stream);
        }


        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            TxData = new Transaction();
            return TxData.TryDeserialize(stream, out error);
        }

    }
}
