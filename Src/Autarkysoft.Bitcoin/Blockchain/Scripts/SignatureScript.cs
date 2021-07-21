// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// The script that is used in <see cref="TxIn"/> as the signature or unlocking script.
    /// Implements <see cref="ISignatureScript"/> and inherits from <see cref="Script"/>.
    /// </summary>
    public class SignatureScript : Script, ISignatureScript
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SignatureScript"/>.
        /// </summary>
        public SignatureScript()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SignatureScript"/> using the given byte array.
        /// </summary>
        /// <param name="data">Data to use</param>
        public SignatureScript(byte[] data)
        {
            Data = data.CloneByteArray();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SignatureScript"/> using the given operation array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ops">An array of operations</param>
        public SignatureScript(IOperation[] ops)
        {
            if (ops == null)
                throw new ArgumentNullException(nameof(ops), "Operation array can not be null.");

            SetData(ops);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SignatureScript"/> using the given block height
        /// (used for creating the signature script of the coinbase transactions).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="height">Block height</param>
        /// <param name="extraData">An extra data to push in this coinbase script</param>
        public SignatureScript(int height, byte[] extraData)
        {
            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Block height can not be negative.");

            PushDataOp hPush = new PushDataOp(height);
            if (extraData == null)
            {
                SetData(new IOperation[] { hPush });
                if (Data.Length < Constants.MinCoinbaseScriptLength)
                {
                    SetData(new IOperation[] { hPush, new PushDataOp(Encoding.UTF8.GetBytes("Bitcoin.Net")) });
                }
            }
            else
            {
                // No need for a min length check since with any height >= 1 it will be at least 2 bytes
                if (extraData.Length > Constants.MaxCoinbaseScriptLength - 4) // 1 byte push + 3 byte height byte
                {
                    throw new ArgumentOutOfRangeException(nameof(extraData.Length),
                          $"Coinbase script can not be bigger than {Constants.MaxCoinbaseScriptLength}");
                }
                SetData(new IOperation[] { hPush, new PushDataOp(extraData) });
            }
        }



        /// <inheritdoc/>
        public bool VerifyCoinbase(IConsensus consensus)
        {
            if (Data.Length < Constants.MinCoinbaseScriptLength || Data.Length > Constants.MaxCoinbaseScriptLength)
            {
                return false;
            }

            if (consensus.IsBip34Enabled)
            {
                var stream = new FastStream(4);
                new PushDataOp(consensus.BlockHeight).WriteToStream(stream);
                ReadOnlySpan<byte> expected = stream.ToByteArray();

                return Data.Length >= expected.Length && ((ReadOnlySpan<byte>)Data).Slice(0, expected.Length).SequenceEqual(expected);
            }
            else
            {
                return true;
            }
        }

        /// <inheritdoc/>
        public void SetToEmpty() => Data = Array.Empty<byte>();

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToP2PK(Signature sig)
        {
            if (sig is null)
                throw new ArgumentNullException(nameof(sig), "Signature can not be null.");

            SetData(new IOperation[] { new PushDataOp(sig.ToByteArray()) });
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToP2PKH(Signature sig, PublicKey pubKey, bool useCompressed)
        {
            if (sig is null)
                throw new ArgumentNullException(nameof(sig), "Signature can not be null.");
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Public key can not be null.");

            var ops = new IOperation[]
            {
                new PushDataOp(sig.ToByteArray()),
                new PushDataOp(pubKey.ToByteArray(useCompressed))
            };
            SetData(ops);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void SetToMultiSig(Signature sig, IRedeemScript redeem, ITransaction tx, int inputIndex)
        {
            if (sig is null)
                throw new ArgumentNullException(nameof(sig), "Signature can not be null.");
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            if (tx is null)
                throw new ArgumentNullException(nameof(tx), "Transaction can not be null.");
            if (inputIndex < 0 || inputIndex >= tx.TxInList.Length)
                throw new ArgumentOutOfRangeException(nameof(inputIndex), "Invalid input index.");
            if (redeem.Data.Length > Constants.MaxScriptItemLength)
                throw new ArgumentOutOfRangeException(nameof(redeem), "Redeem script is bigger than allowed length.");
            if (redeem.GetRedeemScriptType() != RedeemScriptType.MultiSig)
                throw new ArgumentException("Invalid redeem script type.");
            if (!redeem.TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] rdmOps, out _, out string error))
                throw new ArgumentException($"Can not evaluate redeem script: {error}");
            // OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
            if (!((PushDataOp)rdmOps[0]).TryGetNumber(out long m, out error))
                throw new ArgumentException($"Invalid m ({error}).");
            if (m < 0)
                throw new ArgumentOutOfRangeException(nameof(m), "M can not be negative.");
            if (m == 0)
                throw new ArgumentOutOfRangeException(nameof(m), "M value zero is not allowed to prevent funds being stolen.");
            if (!((PushDataOp)rdmOps[^2]).TryGetNumber(out long n, out error))
                throw new ArgumentException($"Invalid n ({error}).");
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n), "N can not be negative.");
            if (n == 0)
                throw new ArgumentOutOfRangeException(nameof(n), "N value zero is not allowed to prevent funds being stolen.");
            if (m > n)
                throw new ArgumentOutOfRangeException(nameof(n), "M can not be bigger than N.");

            bool reset = !TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] sigOps, out _, out _) ||
                         sigOps.Length == 0 ||
                         sigOps.Any(x => !(x is PushDataOp) /*Counting days till C# 9 and "is not" :P*/) ||
                         ((PushDataOp)sigOps[0]).OpValue != OP._0 ||
                         ((PushDataOp)sigOps[^1]).data == null ||
                         !((ReadOnlySpan<byte>)((PushDataOp)sigOps[^1]).data).SequenceEqual(redeem.Data);
            if (reset)
            {
                IOperation[] temp = new IOperation[]
                {
                    new PushDataOp(OP._0),
                    new PushDataOp(sig.ToByteArray()),
                    new PushDataOp(redeem),
                };
                SetData(temp);
                return;
            }

            var calc = new EllipticCurveCalculator();
            bool didSetSig = false;
            // OP_0 sig1 | sig2 | sig_m | redeem
            List<PushDataOp> pushOps = sigOps.Cast<PushDataOp>().ToList();
            int insertIndex = pushOps.Count - 1;
            // OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
            for (int i = rdmOps.Length - 3; i >= 1; i--)
            {
                if (!PublicKey.TryRead(((PushDataOp)rdmOps[i]).data, out PublicKey pubK))
                {
                    throw new ArgumentException("Invalid public key");
                }
                byte[] dataToSign = tx.SerializeForSigning(redeem.Data, inputIndex, sig.SigHash);

                if (calc.Verify(dataToSign, sig, pubK))
                {
                    pushOps.Insert(insertIndex, new PushDataOp(sig.ToByteArray()));
                    didSetSig = true;
                    break;
                }

                if (!Signature.TryReadStrict(pushOps[insertIndex - 1].data, out Signature tempSig, out _))
                {
                    throw new ArgumentException("Invalid signature found in this script.");
                }
                if (calc.Verify(dataToSign, tempSig, pubK))
                {
                    insertIndex--;
                }
            }

            if (!didSetSig)
            {
                throw new ArgumentException("Invalid signature was given.");
            }

            ReadOnlySpan<byte> sigBa = sig.ToByteArray();
            // Prevent infinite loop while keeping the signature that was set intact for only 1 round
            bool remove = false;
            while (pushOps.Count > m + 2)
            {
                for (int i = 1; i < pushOps.Count - 1; i++)
                {
                    if (remove || !sigBa.SequenceEqual(pushOps[i].data))
                    {
                        pushOps.Remove(pushOps[i]);
                        break;
                    }
                }
                remove = true;
            }

            SetData(pushOps.ToArray());
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

            SetData(new IOperation[] { new PushDataOp(redeem.Data) });
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToP2SH_P2WPKH(PublicKey pubKey, bool useCompressed)
        {
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Public key can not be null.");

            RedeemScript redeemBuilder = new RedeemScript();
            redeemBuilder.SetToP2SH_P2WPKH(pubKey, useCompressed);
            SetData(new IOperation[] { new PushDataOp(redeemBuilder.Data) });
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

            SetData(new IOperation[] { new PushDataOp(redeem.Data) });
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public void SetToCheckLocktimeVerify(Signature sig, IRedeemScript redeem)
        {
            if (sig is null)
                throw new ArgumentNullException(nameof(sig), "Signature can not be null.");
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            if (redeem.GetRedeemScriptType() != RedeemScriptType.CheckLocktimeVerify)
                throw new ArgumentException($"Redeem script must be of type {RedeemScriptType.CheckLocktimeVerify}.", nameof(redeem));

            var ops = new IOperation[]
            {
                new PushDataOp(sig.ToByteArray()),
                new PushDataOp(redeem.Data)
            };
            SetData(ops);
        }
    }
}
