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
        public SignatureScript()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SignatureScript"/> using the given byte array.
        /// </summary>
        /// <param name="data">Data to use</param>
        public SignatureScript(byte[] data)
        {
            Data = data;
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
        /// (used for creating a coinbase transaction's script).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="height">Block height</param>
        /// <param name="extraData">An extra data to push in this coinbase script</param>
        public SignatureScript(int height, byte[] extraData)
        {
            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Block height can not be negative.");
            if (extraData.Length > Constants.MaxCoinbaseScriptLength - 4) // 1 byte push + 3 byte height byte
            {
                throw new ArgumentOutOfRangeException(nameof(extraData.Length),
                    $"Coinbase script can not be bigger than {Constants.MaxCoinbaseScriptLength}");
            }

            PushDataOp push = new PushDataOp(height);
            if (extraData != null)
            {
                SetData(new IOperation[] { push, new PushDataOp(extraData) });
            }
            else
            {
                SetData(new IOperation[] { push });
            }
        }



        /// <inheritdoc/>
        public bool VerifyCoinbase(int height, IConsensus consensus)
        {
            if (Data.Length < Constants.MinCoinbaseScriptLength || Data.Length > Constants.MaxCoinbaseScriptLength)
            {
                return false;
            }

            if (consensus.IsBip34Enabled(height))
            {
                PushDataOp op = new PushDataOp();
                return op.TryRead(new FastStreamReader(Data), out _) && op.TryGetNumber(out long h, out _, true, 4) && h == height;
            }
            else
            {
                return true;
            }
        }

        /// <inheritdoc/>
        public void SetToEmpty()
        {
            Data = new byte[0];
        }


        private byte[] ConvertToBytes(params PushDataOp[] pushOps)
        {
            FastStream stream = new FastStream();
            foreach (var op in pushOps)
            {
                op.WriteToStream(stream);
            }
            return stream.ToByteArray();
        }

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
        public void SetToMultiSig(Signature sig, PublicKey pub, IRedeemScript redeem, ITransaction tx, ITransaction prevTx, int inputIndex)
        {
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            if (redeem.Data.Length > Constants.MaxScriptItemLength)
                throw new ArgumentOutOfRangeException(nameof(redeem), "Redeem script is bigger than allowed length.");
            if (redeem.GetRedeemScriptType() != RedeemScriptType.MultiSig)
                throw new ArgumentException("Invalid redeem script type.");
            if (redeem.TryEvaluate(out IOperation[] rdmOps, out _, out string error))
                throw new ArgumentException($"Can not evaluate redeem script: {error}.");
            // OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
            if (!((PushDataOp)rdmOps[0]).TryGetNumber(out long m, out string err))
                throw new ArgumentException($"Invalid m ({err}).");
            if (m < 0)
                throw new ArgumentOutOfRangeException(nameof(m), "M can not be negative.");
            if (!((PushDataOp)rdmOps[^2]).TryGetNumber(out long n, out err))
                throw new ArgumentException($"Invalid n ({err}).");
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n), "N can not be negative.");
            if (m > n)
                throw new ArgumentOutOfRangeException(nameof(n), "M can not be bigger than N.");

            byte[] rdmBa = redeem.Data;


            if (m == 0)
            {
                var ops = new IOperation[2]
                {
                    // Due to a bug in bitcoin-core's implementation of OP_CheckMultiSig, there must be an extra item
                    // at the start, that item must be OP_0 in latest standard rules
                    new PushDataOp(OP._0),
                    new PushDataOp(rdmBa)
                };
                SetData(ops);
                return;
            }

            if (sig is null)
                throw new ArgumentNullException(nameof(sig), "Signature can not be null.");
            if (pub is null)
                throw new ArgumentNullException(nameof(pub), "Public key can not be null.");

            // Possible scenarios (1&3 are the normal most common cases):
            // 1) m-of-n redeem with no signature set yet and no duplicate public keys
            //    Simply instantiate the OP array with the given single sig
            // 2) m-of-n redeem with no signature set yet and duplicate public keys
            //    If the duplicate pub is the same as current pub instantiate the OP array with m sigs
            // 3) m-of-n redeem with one or more signatures set and no duplicate public keys
            //    Find index of current pub, validate each sig to figure out which pub they correspond to then set the given sig
            //    in an appropriate index among the already existing sigs (don't exceed m numbe of sigs)
            // 4) m-of-n redeem with one or more signatures set duplicate public keys
            //    Same as 3 but have to decide how many duplicate sigs should be provided while not exceeding m

            int pubIndex = -1;
            ReadOnlySpan<byte> compPub = pub.ToByteArray(true);
            ReadOnlySpan<byte> uncompPub = pub.ToByteArray(false);
            List<int> dupPubIndexes = new List<int>();
            PublicKey[] allPubs = new PublicKey[n];
            // RedeemScript = OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
            for (int i = 1; i < rdmOps.Length - 2; i++)
            {
                byte[] pushPubBa = ((PushDataOp)rdmOps[i]).data;
                if (compPub.SequenceEqual(pushPubBa) || uncompPub.SequenceEqual(pushPubBa))
                {
                    if (!PublicKey.TryRead(pushPubBa, out allPubs[i - 1]))
                    {
                        throw new ArgumentException("An invalid public key was found inside redeem script.");
                    }

                    if (pubIndex < 0)
                    {
                        pubIndex = i;
                    }
                    else
                    {
                        dupPubIndexes.Add(i);
                    }
                }
            }

            if (pubIndex < 0)
            {
                throw new ArgumentException("Public key doesn't exist in redeem script.");
            }

            // TODO: 
            throw new NotImplementedException("Implement this!");
            //// A signature script of this type that was set before must at least have 1 signature inside
            //// that means OP_0 <sig> <redeem> or 3 items
            //if (OperationList is null || OperationList.Length < 3 ||
            //    !(OperationList[0] is PushDataOp push0) || push0.OpValue != OP._0 ||
            //    !(OperationList[^1] is PushDataOp pushn) || ((ReadOnlySpan<byte>)rdmBa).SequenceEqual(pushn.data))
            //{
            //    OperationList = new IOperation[2 + (dupPubIndexes.Count + 1 > m ? m : dupPubIndexes.Count + 1)];
            //    OperationList[0] = new PushDataOp(OP._0);
            //    byte[] sigBa = sig.ToByteArray();
            //    for (int i = 0; i < dupPubIndexes.Count + 1 && i < m; i++)
            //    {
            //        OperationList[i + 1] = new PushDataOp(sigBa);
            //    }
            //    OperationList[^1] = new PushDataOp(rdmBa);
            //}
            //else
            //{
            //    Signature[] sigs = new Signature[OperationList.Length - 2];
            //    for (int i = 1; i < OperationList.Length - 1; i++)
            //    {
            //        if (OperationList[i] is PushDataOp push && push.data != null)
            //        {
            //            if (!Signature.TryRead(push.data, out sigs[i - 1], out string error))
            //            {
            //                throw new ArgumentException($"Invalid signature found inside existing SignatureScript {error}.");
            //            }
            //        }
            //        else
            //        {
            //            throw new ArgumentException("Invalid push operation found inside existing SignatureScript.");
            //        }
            //    }

            //    int currentSigCount = sigs.Length;
            //    int totalSigNeeded = (int)m;

            //    IOperation[] final;
            //    if (sigs.Length < m)
            //    {
            //        final = new IOperation[OperationList.Length + 1];
            //    }
            //    else // >= m
            //    {
            //        final = new IOperation[m + 2];
            //    }

            //    final[0] = new PushDataOp(OP._0);
            //    final[^1] = new PushDataOp(rdmBa);

            //    int insertIndex = 1;
            //    int insertRevIndex = final.Length - 1;

            //    var calc = new EllipticCurveCalculator();
            //    foreach (var item in sigs)
            //    {
            //        for (int i = 0; i < allPubs.Length; i++)
            //        {
            //            if (calc.Verify(tx.GetBytesToSign(prevTx, inputIndex, item.SigHash, redeem), item, allPubs[i]))
            //            {
            //                if (i < pubIndex)
            //                {
            //                    final[insertIndex++] = new PushDataOp(item.ToByteArray());
            //                }
            //                else
            //                {
            //                    final[insertRevIndex--] = new PushDataOp(item.ToByteArray());
            //                }
            //            }
            //        }
            //    }

            //    final[insertIndex] = new PushDataOp(sig.ToByteArray());
            //}
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
        public void SetToP2SH_P2WPKH(PublicKey pubKey)
        {
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Public key can not be null.");

            RedeemScript redeemBuilder = new RedeemScript();
            redeemBuilder.SetToP2SH_P2WPKH(pubKey);
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

            Data = ConvertToBytes(new PushDataOp(redeem.Data));
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

            var ops = new IOperation[]
            {
                new PushDataOp(sig.ToByteArray()),
                new PushDataOp(redeem.Data)
            };
            SetData(ops);
        }
    }
}
