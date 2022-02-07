// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Linq;

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
            if (dataItems.Any(x => x is null))
                throw new ArgumentNullException(nameof(dataItems), "Data items can not be null.");

            Items = dataItems;
        }


        private byte[][] _items = Array.Empty<byte[]>();
        /// <inheritdoc/>
        public byte[][] Items
        {
            get => _items;
            set => _items = value ?? Array.Empty<byte[]>();
        }


        /// <inheritdoc/>
        public void AddSerializedSize(SizeCounter counter)
        {
            if (Items.Length == 0)
            {
                counter.AddByte();
            }
            else
            {
                counter.AddCompactIntCount(Items.Length);
                foreach (var item in Items)
                {
                    counter.AddWithCompactIntLength(item.Length);
                }
            }
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
                var count = new CompactInt(Items.Length);
                count.WriteToStream(stream);
                foreach (var item in Items)
                {
                    stream.WriteWithCompactIntLength(item);
                }
            }
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt count, out Errors err))
            {
                error = err.Convert();
                return false;
            }

            // A quick check to avoid data loss during cast below
            if (count > int.MaxValue)
            {
                error = "Item count is too big.";
                return false;
            }
            Items = new byte[(int)count][];
            for (int i = 0; i < Items.Length; i++)
            {
                if (!stream.TryReadByteArrayCompactInt(out byte[] temp))
                {
                    error = Err.EndOfStream;
                    return false;
                }
                Items[i] = temp;
            }

            error = null;
            return true;
        }

        /// <inheritdoc/>
        /// <exception cref="NullReferenceException"/>
        public void SetToP2WPKH(Signature sig, PublicKey pubKey, bool useCompressed = true)
        {
            Items = new byte[2][]
            {
                sig.ToByteArray(),
                pubKey.ToByteArray(useCompressed)
            };
        }

        /// <inheritdoc/>
        public void SetToP2WSH_MultiSig(Signature[] sigs, RedeemScript redeem)
        {
            Items = new byte[sigs.Length + 2][]; // OP_0 | Sig1 | sig2 | .... | sig(n) | redeemScript
            Items[0] = Array.Empty<byte>();
            for (int i = 1; i <= sigs.Length; i++)
            {
                Items[i] = sigs[i].ToByteArray();
            }
            Items[^1] = redeem.Data;
        }
    }
}
