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
    /// The script that is used in <see cref="Blockchain.Transactions.TxIn"/> as the signature or unlocking script.
    /// Implements <see cref="ISignatureScript"/> and inherits from <see cref="Script"/>.
    /// </summary>
    public class SignatureScript : Script, ISignatureScript
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SignatureScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public SignatureScript() : base(Constants.MaxScriptLength)
        {
            ScriptType = ScriptType.ScriptSig;
        }



        /// <inheritdoc/>
        public void SetToEmpty()
        {
            OperationList = new IOperation[0];
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToP2PK(Signature sig)
        {
            if (sig is null)
                throw new ArgumentNullException(nameof(sig), "Signature can not be null.");

            OperationList = new IOperation[]
            {
                new PushDataOp(sig.ToByteArray()),
            };
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToP2PKH(Signature sig, PublicKey pubKey, bool useCompressed)
        {
            if (sig is null)
                throw new ArgumentNullException(nameof(sig), "Signature can not be null.");
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Public key can not be null.");

            OperationList = new IOperation[]
            {
                new PushDataOp(sig.ToByteArray()),
                new PushDataOp(pubKey.ToByteArray(useCompressed))
            };
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToMultiSig(Signature sig, PublicKey pub, IRedeemScript redeem)
        {
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            if (redeem.GetRedeemScriptType() != RedeemScriptType.MultiSig)
                throw new ArgumentException("Invalid redeem script type.");
            // OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
            if (!((PushDataOp)redeem.OperationList[0]).TryGetNumber(out long m, out string err))
                throw new ArgumentException($"Invalid m ({err}).");
            if (!((PushDataOp)redeem.OperationList[^2]).TryGetNumber(out long n, out err))
                throw new ArgumentException($"Invalid n ({err}).");

            // We assume redeem script has already checked m and n to be valid values

            // Only initialize if it was not done before (perform minimal amount of test)
            if (OperationList.Length != m + 2)
            {
                // OP_0 | [Sig1|sig2|...|sig(m)] | redeemScript
                OperationList = new IOperation[m + 2];
                // Due to a bug in bitcoin-core's implementation of OP_CheckMultiSig, there must be an extra item
                // at the start, that item must be OP_0 in latest consensus rules
                OperationList[0] = new PushDataOp(OP._0);
                OperationList[^1] = new PushDataOp(redeem);
            }

            int index = -1;
            ReadOnlySpan<byte> compPub = pub.ToByteArray(true);
            ReadOnlySpan<byte> uncompPub = pub.ToByteArray(false);
            // RedeemScript = OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
            for (int i = 1; i < redeem.OperationList.Length - 2; i++)
            {
                if (compPub.SequenceEqual(((PushDataOp)redeem.OperationList[i]).data) ||
                    uncompPub.SequenceEqual(((PushDataOp)redeem.OperationList[i]).data))
                {
                    index = i;
                    break;
                }
            }
            if (index < 0)
            {
                throw new ArgumentException("Public key doesn't exist in redeem script.");
            }

            OperationList[index] = new PushDataOp(sig.ToByteArray());
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToP2SH_P2WPKH(IRedeemScript redeem)
        {
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            if (redeem.GetRedeemScriptType() != RedeemScriptType.P2SH_P2WPKH)
                throw new ArgumentException("Invalid redeem script type.");

            OperationList = new IOperation[]
            {
                new PushDataOp(redeem.ToByteArray())
            };
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToP2SH_P2WPKH(PublicKey pubKey)
        {
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Public key can not be null.");

            RedeemScript redeemBuilder = new RedeemScript();
            redeemBuilder.SetToP2SH_P2WPKH(pubKey);
            OperationList = new IOperation[]
            {
                new PushDataOp(redeemBuilder.ToByteArray())
            };
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToP2SH_P2WSH(IRedeemScript redeem)
        {
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            if (redeem.GetRedeemScriptType() != RedeemScriptType.P2SH_P2WSH)
                throw new ArgumentException("Invalid redeem script type.");

            OperationList = new IOperation[]
            {
                new PushDataOp(redeem.ToByteArray())
            };
        }

        /// <inheritdoc/>
        public void SetToCheckLocktimeVerify(Signature sig, IRedeemScript redeem)
        {
            if (sig is null)
                throw new ArgumentNullException(nameof(sig), "Signature can not be null.");
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            if (redeem.GetRedeemScriptType() != RedeemScriptType.CheckLocktimeVerify)
                throw new ArgumentException($"Redeem script must be of type {RedeemScriptType.CheckLocktimeVerify}.", nameof(redeem));

            OperationList = new IOperation[]
            {
                new PushDataOp(sig.ToByteArray()),
                new PushDataOp(redeem.ToByteArray())
            };
        }
    }
}
