// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class MockReplyManager : IReplyManager
    {
        private int index;
        public PayloadType[] toReceive;
        public Message[][] toReply;

        public Message[] GetReply(Message msg)
        {
            if (toReceive == null || index >= toReceive.Length)
            {
                Assert.True(false, "Unexpected message was received.");
            }

            if (!Enum.TryParse(Encoding.ASCII.GetString(msg.PayloadName.TrimEnd()), ignoreCase: true, out PayloadType plt) ||
                plt != toReceive[index])
            {
                Assert.True(false, "A different message was received.");
            }

            Message[] reply = toReply?[index];
            index++;
            return reply;
        }
    }
}
