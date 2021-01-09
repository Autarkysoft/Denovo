// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class GetDataPayloadTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var invs = new Inventory[2];
            var pl = new GetDataPayload(invs);

            Assert.Equal(PayloadType.GetData, pl.PayloadType);
            Assert.Same(invs, pl.InventoryList);
        }
    }
}
