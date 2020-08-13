// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// The script that is used in witness part of the transaction as the signature or unlocking script.
    /// Implements <see cref="IWitness"/> and inherits from <see cref="Script"/>.
    /// <para/> Witnesses are more like stack items rather than scripts.
    /// </summary>
    public class Witness : IWitness
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Witness"/>.
        /// </summary>
        public Witness()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Witness"/> using the given data array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="dataItems">An array of byte arrays to use as witness items</param>
        public Witness(byte[][] dataItems)
        {
            if (dataItems == null)
                throw new ArgumentNullException(nameof(dataItems), "Data items can not be null.");

            Items = new PushDataOp[dataItems.Length];
            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = new PushDataOp(dataItems[i]);
            }
        }


        private PushDataOp[] _items = new PushDataOp[0];
        /// <inheritdoc/>
        public PushDataOp[] Items
        {
            get => _items;
            set => _items = value ?? new PushDataOp[0];
        }


        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            if (Items.Length == 0)
            {
                stream.Write((byte)0);
            }
            else
            {
                CompactInt count = new CompactInt(Items.Length);
                count.WriteToStream(stream);
                foreach (var item in Items)
                {
                    item.WriteToWitnessStream(stream);
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

            // A quick check to avoid data loss during cast below
            if (count > int.MaxValue)
            {
                error = "Item count is too big.";
                return false;
            }
            Items = new PushDataOp[(int)count];
            for (int i = 0; i < Items.Length; i++)
            {
                PushDataOp temp = new PushDataOp();
                if (!temp.TryReadWitness(stream, out error))
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
            temp.WriteToStream(stream);

            Items[^1] = new PushDataOp(stream.ToByteArray());
        }
    }
}
