// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using System;

namespace Autarkysoft.Bitcoin.Encoders
{
    public class Address
    {
        internal bool VerifyType(string address, PubkeyScriptType scrType, out byte[] hash)
        {
            throw new NotImplementedException();
        }
    }
}
