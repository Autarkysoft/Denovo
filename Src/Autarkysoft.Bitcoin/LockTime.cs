// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin
{
    public readonly struct LockTime
    {
        public bool TryRead(FastStreamReader stream, out LockTime result, out string error)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(FastStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
