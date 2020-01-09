// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;

namespace Autarkysoft.Bitcoin.Blockchain.Transactions
{
    public interface ITransaction
    {
        byte[] GetBytesToSign(ITransaction prvTx, int txInIndex, SigHashType sht);
    }
}
