// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using Tests.Bitcoin.Blockchain;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class BlockTxnPayloadTests
    {
        [Fact]
        public void Constructor_ExceptionTest()
        {
            ITransaction[] txs = new ITransaction[] { new MockSerializableTx(new byte[32]) };
            Assert.Throws<ArgumentNullException>(() => new BlockTxnPayload(null, txs));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BlockTxnPayload(new byte[10], txs));
            Assert.Throws<ArgumentNullException>(() => new BlockTxnPayload(new byte[32], null));
            Assert.Throws<ArgumentNullException>(() => new BlockTxnPayload(new byte[32], Array.Empty<ITransaction>()));
        }

        [Fact]
        public void SerializeTest()
        {
            byte[] mockBlkHash = Helper.GetBytes(32);
            MockSerializableTx tx1 = new(new byte[] { 5, 6 });
            MockSerializableTx tx2 = new(new byte[] { 7, 8, 9, 10 });
            BlockTxnPayload pl = new(mockBlkHash, new ITransaction[] { tx1, tx2 });
            FastStream stream = new(32 + 1 + 2 + 4);
            pl.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes($"{Helper.GetBytesHex(32)}0205060708090a");

            Assert.Equal(expected, actual);

            Assert.Equal(PayloadType.BlockTxn, pl.PayloadType);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            byte[] mockBlkHash = Helper.GetBytes(32);
            // 2 random small size tx taken from a block explorer:
            string tx1 = "0100000001defccf0ab6f1ce363820fd8ffc59e1a455e520ad37ed2b4b781fddbffbe0b1fd00000000025200ffffffff01151605000000000017a914de2b27afd4498dc5688f5b511a8be5aad26820338700000000";
            string tx2 = "0100000001310f060f19fd067aed414c3902ac70693c67f753dcc34e6bddcfe4fabe6aa32000000000025100ffffffff01ee340200000000001976a9145c0189a6094fe13177cd47b9a9ee0ec92509365388ac00000000";
            FastStreamReader stream = new(Helper.HexToBytes($"{Helper.GetBytesHex(32)}02{tx1}{tx2}"));
            BlockTxnPayload pl = new();
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(mockBlkHash, pl.BlockHash);
            Assert.Equal(2, pl.Transactions.Length);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, Errors.NullStream };
            yield return new object[] { new FastStreamReader(new byte[31]), Errors.EndOfStream };

            byte[] ba1 = new byte[33];
            ba1[^1] = 255;
            yield return new object[] { new FastStreamReader(ba1), Errors.ShortCompactInt8 };

            byte[] ba2 = new byte[32 + 5];
            ba2[^1] = 255;
            ba2[^2] = 255;
            ba2[^3] = 255;
            ba2[^4] = 255;
            ba2[^5] = 254;
            yield return new object[] { new FastStreamReader(ba2), Errors.MsgTxCountOverflow };

            byte[] ba3 = new byte[32 + 2];
            ba3[^2] = 1;
            yield return new object[] { new FastStreamReader(ba3), Errors.EndOfStream };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailtTest(FastStreamReader stream, Errors expErr)
        {
            BlockTxnPayload pl = new();
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
