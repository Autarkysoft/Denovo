﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Text;

namespace Tests.Bitcoin.P2PNetwork
{
    public class MockReplyManager : IReplyManager
    {
        internal Message? pingMsg;
        public Message GetPingMsg()
        {
            if (pingMsg is null)
            {
                Assert.Fail("Ping message to return must be set first.");
            }
            return pingMsg;
        }


        internal Message? verMessage;
        public Message GetVersionMsg()
        {
            if (verMessage is null)
            {
                Assert.Fail("Version message to return must be set first.");
            }
            return verMessage;
        }


        private int index;
        public PayloadType[]? toReceive;
        public byte[][]? toReceiveBytes;
        public Message[][]? toReply;

        public Message[]? GetReply(Message msg)
        {
            if (toReceive == null || toReceiveBytes == null || index >= toReceive.Length)
            {
                Assert.Fail("Unexpected message was received.");
            }
            if (toReceive.Length != toReceiveBytes.Length)
            {
                Assert.Fail("Expected payload type and bytes are incorrectly set.");
            }
            
            byte[] expPl = new byte[12];
            byte[] plBa = Encoding.ASCII.GetBytes(toReceive[index].ToString().ToLower());
            Buffer.BlockCopy(plBa, 0, expPl, 0, plBa.Length);
            Assert.Equal(expPl, msg.PayloadName);
            Assert.Equal(toReceiveBytes[index], msg.PayloadData);

            Message[]? reply = toReply?[index];
            index++;
            return reply;
        }
    }
}
