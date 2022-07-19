// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using System;

namespace Tests.Bitcoin.Blockchain
{
    public class MockUtxo : IUtxo
    {
        public bool IsMempoolSpent { get; set; }
        public bool IsBlockSpent { get; set; }
        public uint Index { get; set; }
        public ulong Amount { get; set; }
        public IPubkeyScript PubScript { get; set; }

        public void AddSerializedSize(SizeCounter counter)
        {
            throw new NotImplementedException();
        }

        public void Serialize(FastStream stream)
        {
            throw new NotImplementedException();
        }

        public bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) => obj is MockUtxo other && Index == other.Index;
        public override int GetHashCode() => HashCode.Combine(Index);
    }
}
