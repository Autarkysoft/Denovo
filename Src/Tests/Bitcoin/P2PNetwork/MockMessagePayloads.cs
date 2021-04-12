// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;

namespace Tests.Bitcoin.P2PNetwork
{
    public abstract class MockMessagePayloadBase : IMessagePayload
    {
        public virtual PayloadType PayloadType => throw new NotImplementedException();
        public virtual byte[] GetChecksum() => throw new NotImplementedException();
        public virtual void AddSerializedSize(SizeCounter counter) => throw new NotImplementedException();
        public virtual void Serialize(FastStream stream) => throw new NotImplementedException();
        public virtual bool TryDeserialize(FastStreamReader stream, out string error) => throw new NotImplementedException();
    }


    public class MockSerializableMessagePayload : MockMessagePayloadBase
    {
        public MockSerializableMessagePayload(PayloadType plt, byte[] serializedResult)
        {
            this.plt = plt;
            serBa = serializedResult;
        }

        private readonly byte[] serBa;
        private readonly PayloadType plt;

        public override PayloadType PayloadType => plt;
        public override void Serialize(FastStream stream) => stream.Write(serBa);
    }
}
