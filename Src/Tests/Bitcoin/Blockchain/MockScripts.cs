// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;

namespace Tests.Bitcoin.Blockchain
{
    internal abstract class MockScriptBase : IScript
    {
        public virtual bool IsWitness => throw new NotImplementedException();
        public virtual ScriptType ScriptType => throw new NotImplementedException();
        public virtual IOperation[] OperationList 
        { 
            get => throw new NotImplementedException();
            set => throw new NotImplementedException(); 
        }

        public virtual void Serialize(FastStream stream)
        {
            throw new NotImplementedException();
        }

        public virtual void ToByteArray(FastStream stream)
        {
            throw new NotImplementedException();
        }

        public virtual byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }

        public virtual bool TryDeserialize(FastStreamReader stream, out string error)
        {
            throw new NotImplementedException();
        }
    }



    internal class MockSerializableScript : MockScriptBase
    {
        public MockSerializableScript(byte[] serializedResult)
        {
            ba = serializedResult;
        }

        private readonly byte[] ba;

        public override void Serialize(FastStream stream)
        {
            stream.Write(ba);
        }

        public override void ToByteArray(FastStream stream)
        {
            stream.Write(ba);
        }

        public override byte[] ToByteArray()
        {
            return ba;
        }
    }
}
