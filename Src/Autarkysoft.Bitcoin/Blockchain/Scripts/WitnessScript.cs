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
    public class WitnessScript : IWitnessScript
    {
        public WitnessScript()
        {
        }


        /// <inheritdoc/>
        public PushDataOp[] Items { get; set; }


        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            if (Items == null || Items.Length == 0)
            {
                stream.Write((byte)0);
            }
            else
            {
                CompactInt count = new CompactInt(Items.Length);
                count.WriteToStream(stream);
                foreach (var item in Items)
                {
                    item.WriteToStream(stream, true);
                }
            }
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (!CompactInt.TryRead(stream, out CompactInt count, out error))
            {
                return false;
            }

            // TODO: set a better value for comparison
            if (count > int.MaxValue)
            {
                error = "Item count is too big.";
                return false;
            }
            Items = new PushDataOp[(int)count];
            for (int i = 0; i < Items.Length; i++)
            {
                PushDataOp temp = new PushDataOp();
                if (!temp.TryRead(stream, out error, true))
                {
                    return false;
                }
                Items[i] = temp;
            }

            error = null;
            return true;
        }

        /// <inheritdoc/>
        public void SetToP2WPKH(Signature sig, PublicKey pubKey, bool useCompressed = true)
        {
            byte[] sigBa = sig.ToByteArray();
            byte[] pubkBa = pubKey.ToByteArray(useCompressed);

            Items = new PushDataOp[]
            {
                new PushDataOp(sigBa),
                new PushDataOp(pubkBa)
            };
        }

        /// <inheritdoc/>
        public void SetToP2WSH_MultiSig(Signature[] sigs, RedeemScript redeem)
        {
            Items = new PushDataOp[sigs.Length + 2]; // OP_0 | Sig1 | sig2 | .... | sig(n) | redeemScript

            Items[0] = new PushDataOp(OP._0);

            for (int i = 1; i <= sigs.Length; i++)
            {
                Items[i] = new PushDataOp(sigs[i].ToByteArray());
            }

            FastStream stream = new FastStream();
            PushDataOp temp = new PushDataOp(redeem.Data);
            temp.WriteToStream(stream, false);

            Items[^1] = new PushDataOp(stream.ToByteArray());
        }
    }
}
