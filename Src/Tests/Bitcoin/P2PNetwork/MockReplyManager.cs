// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class MockReplyManager : IReplyManager
    {
        public PayloadType expRejectType;

        public Message GetReject(PayloadType plt, string error)
        {
            Assert.Equal(expRejectType, plt);
            return new Message(new RejectPayload(), NetworkType.MainNet);
        }


        private int index;
        public PayloadType[] toReceive;
        public Message[] toReply;

        public Message GetReply(Message msg)
        {
            if (toReceive == null || index >= toReceive.Length)
            {
                Assert.True(false, "Unexpected message was received.");
            }

            if (msg.Payload.PayloadType != toReceive[index])
            {
                Assert.True(false, "A different message was received.");
            }

            Message reply = toReply?[index];
            index++;
            return reply;
        }
    }
}
