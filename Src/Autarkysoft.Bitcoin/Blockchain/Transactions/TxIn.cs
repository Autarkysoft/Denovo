// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;

namespace Autarkysoft.Bitcoin.Blockchain.Transactions
{
    public class TxIn
    {
        public byte[] TxHash { get; set; }
        public uint Index { get; set; }
        public ISignatureScript SigScript { get; set; }
        // TODO: read this about sequence and probably create a new variable type for it:
        // https://github.com/bitcoin/bips/blob/master/bip-0068.mediawiki
        public uint Sequence { get; set; }


        public void Serialize(FastStream stream)
        {
            stream.Write(TxHash);
            stream.Write(Index);
            SigScript.Serialize(stream);
            stream.Write(Sequence);
        }

    }
}
