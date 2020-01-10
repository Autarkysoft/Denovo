// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// The script that is used in witness part of the transaction as the signature or unlocking script.
    /// Implements <see cref="IWitnessScript"/> and inherits from <see cref="Script"/>.
    /// <para/> Witnesses are more like stack items rather than scripts.
    /// </summary>
    public class WitnessScript : Script, IWitnessScript
    {
        public WitnessScript() : base(100)
        {
            IsWitness = true;
            ScriptType = ScriptType.ScriptWitness;
        }



        public void SetToP2WPKH(Signature sig, PublicKey pubKey, bool useCompressed = true)
        {
            byte[] sigBa = sig.ToByteArray();
            byte[] pubkBa = pubKey.ToByteArray(useCompressed);

            OperationList = new IOperation[]
            {
                new PushDataOp(sigBa),
                new PushDataOp(pubkBa)
            };
        }

        public void SetToP2WSH_MultiSig(Signature[] sigs, RedeemScript redeem)
        {
            OperationList = new IOperation[sigs.Length + 2]; // OP_0 | Sig1 | sig2 | .... | sig(n) | redeemScript

            OperationList[0] = new PushDataOp(OP._0);

            for (int i = 1; i <= sigs.Length; i++)
            {
                OperationList[i] = new PushDataOp(sigs[i].ToByteArray());
            }

            FastStream stream = new FastStream();
            PushDataOp temp = new PushDataOp(redeem.ToByteArray());
            temp.WriteToStream(stream, false);

            OperationList[^1] = new PushDataOp(stream.ToByteArray());
        }

    }
}
