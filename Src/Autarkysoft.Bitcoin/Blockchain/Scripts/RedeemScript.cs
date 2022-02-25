// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// The script that is used inside <see cref="ISignatureScript"/>s when spending P2SH <see cref="IPubkeyScript"/> types.
    /// <para/>Implements <see cref="IRedeemScript"/> and inherits from <see cref="Script"/>.
    /// </summary>
    public class RedeemScript : Script, IRedeemScript
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RedeemScript"/>.
        /// </summary>
        public RedeemScript()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RedeemScript"/> using the given byte array.
        /// </summary>
        /// <param name="data">Script data to use</param>
        public RedeemScript(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RedeemScript"/> using the given operation array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ops">An array of operations</param>
        public RedeemScript(IOperation[] ops)
        {
            if (ops == null)
                throw new ArgumentNullException(nameof(ops), "Operation array can not be null.");

            SetData(ops);
        }



        /// <inheritdoc/>
        public int CountSigOps(IOperation[] ops)
        {
            int res = 0;
            for (int i = 0; i < ops.Length; i++)
            {
                if (ops[i] is CheckSigOp || ops[i] is CheckSigVerifyOp)
                {
                    res++;
                }
                else if (ops[i] is CheckMultiSigOp || ops[i] is CheckMultiSigVerifyOp)
                {
                    if (i > 0 && ops[i - 1] is PushDataOp push && (push.OpValue >= OP._1 && push.OpValue <= OP._16))
                    {
                        res += (int)push.OpValue - 0x50;
                    }
                    else
                    {
                        res += 20;
                    }
                }
                else if (ops[i] is IfElseOpsBase conditional)
                {
                    res += conditional.CountSigOps();
                }
            }
            return res;
        }

        /// <summary>
        /// Returns number of CheckSig operations but will not check if script is correctly evaluated.
        /// Evaluation and check must be done by caller and <see cref="CountSigOps(IOperation[])"/> method should be
        /// used instead.
        /// </summary>
        /// <returns></returns>
        public override int CountSigOps()
        {
            TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] ops, out _, out _);
            return CountSigOps(ops);
        }

        /// <inheritdoc/>
        public RedeemScriptType GetRedeemScriptType()
        {
            bool b = TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] OperationList, out _, out _);

            if (!b)
            {
                return RedeemScriptType.Unknown;
            }
            else if (OperationList.Length == 0)
            {
                return RedeemScriptType.Empty;
            }
            else if (OperationList.Length == 2 &&
                     OperationList[0] is PushDataOp op0 && op0.OpValue == OP._0 &&
                     OperationList[1] is PushDataOp push1)
            {
                if (push1.data?.Length == Constants.Hash160Length)
                {
                    return RedeemScriptType.P2SH_P2WPKH;
                }
                else if (push1.data?.Length == Constants.Sha256Length)
                {
                    return RedeemScriptType.P2SH_P2WSH;
                }
            }
            else if (OperationList.Length == 5 &&
                     OperationList[0] is PushDataOp push2 && push2.TryGetNumber(out _, out _, true, 5) &&
                     OperationList[1] is CheckLocktimeVerifyOp &&
                     OperationList[2] is DROPOp &&
                     OperationList[3] is PushDataOp push3 &&
                     (push3.data?.Length == Constants.CompressedPubkeyLen ||
                      push3.data?.Length == Constants.UncompressedPubkeyLen) &&
                     OperationList[4] is CheckSigOp)
            {
                return RedeemScriptType.CheckLocktimeVerify;
            }
            else if (OperationList[^1] is CheckMultiSigOp)
            {
                // Multi-sig redeem scripts can be as simple as a single CheckMultiSigOp OP (pointless script that anyone can spend)
                // to the usual multi-sig scripts with (OP_m <n*pubkeys> OP_n OP_CheckMultiSig) script up to 15 pubkeys
                for (int i = 0; i < OperationList.Length - 1; i++)
                {
                    if (!(OperationList[i] is PushDataOp))
                    {
                        return RedeemScriptType.Unknown;
                    }
                }
                return RedeemScriptType.MultiSig;
            }

            return RedeemScriptType.Unknown;
        }

        /// <inheritdoc/>
        public RedeemScriptSpecialType GetSpecialType(IConsensus consensus)
        {
            if (consensus.IsSegWitEnabled &&
                Data.Length >= 4 && Data.Length <= 42 &&
                Data.Length == Data[1] + 2 &&
                (Data[0] == 0 || (Data[0] >= (byte)OP._1 && Data[0] <= (byte)OP._16)))
            {
                // Version 0 witness program
                if (Data[0] == 0)
                {
                    // OP_0 0x14(data20)
                    if (Data.Length == 22)
                    {
                        return RedeemScriptSpecialType.P2SH_P2WPKH;
                    }
                    // OP_0 0x20(data32)
                    else if (Data.Length == 34)
                    {
                        return RedeemScriptSpecialType.P2SH_P2WSH;
                    }
                    else
                    {
                        return RedeemScriptSpecialType.InvalidWitness;
                    }
                }
                // OP_num PushData()
                else
                {
                    return RedeemScriptSpecialType.UnknownWitness;
                }
            }

            return RedeemScriptSpecialType.None;
        }


        /// <summary>
        /// Sets this script to a "m of n multi-signature" script using the given <see cref="PublicKey"/>s.
        /// </summary>
        /// <remarks>
        /// Since normally a m-of-n redeem script is created from all compressed or all uncompressed public keys
        /// this method has one boolean determining that. If a mixture is needed an <see cref="IOperation"/> array
        /// should be created manually and passed to constructor.
        /// Additionally m and n must be at least 1 (although 0 is valid) to prevent insecure redeem scripts (anyone can spend).
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="m">
        /// The minimum number of required signatures (must be smaller than <paramref name="pubKeys"/>.Length and be at least 1).
        /// </param>
        /// <param name="pubKeys">
        /// A list of public keys to use (must contain at least 1 public key up to 15 compressed or 7 uncompressed keys)
        /// </param>
        /// <param name="useCompressed">
        /// [Default value = true] If true compressed public keys are used, uncompressed otherwise
        /// </param>
        public void SetToMultiSig(int m, PublicKey[] pubKeys, bool useCompressed = true)
        {
            if (pubKeys == null || pubKeys.Length == 0)
                throw new ArgumentNullException(nameof(pubKeys), "Pubkey list can not be null or empty.");
            // Maximum allowed length of a redeem script is 520 bytes. That is:
            // Compressed 3 + 15*(1+33) = 513 or uncompressed 3 + 7*(1+65) = 465
            if (m < 1 || (useCompressed && m > 15) || (!useCompressed && m > 7) || m > pubKeys.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(m),
                    "M must be between 1 and (7 uncomprssed or 15 compressed) public keys and smaller than N.");
            }
            if ((useCompressed && pubKeys.Length > 15) || (!useCompressed && pubKeys.Length > 7))
            {
                throw new ArgumentOutOfRangeException(nameof(pubKeys),
                    "Pubkey list must contain at least 1 and at most 15 compressed or 7 uncompressed keys.");
            }

            // OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
            IOperation[] ops = new IOperation[pubKeys.Length + 3];
            ops[0] = new PushDataOp(m);
            ops[^2] = new PushDataOp(pubKeys.Length);
            ops[^1] = new CheckMultiSigOp();
            int i = 1;
            foreach (var item in pubKeys)
            {
                ops[i++] = new PushDataOp(item.ToByteArray(useCompressed));
            }
            SetData(ops);
        }


        /// <summary>
        /// Sets this script to a P2SH-P2WPKH redeem script using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="pubKey">Public key to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates whether to use compressed or uncompressed public key in the redeem script.
        /// <para/> * Note that uncompressed public keys are non-standard and can lead to funds being lost.
        /// </param>
        public void SetToP2SH_P2WPKH(PublicKey pubKey, bool useCompressed = true)
        {
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Public key can not be null.");

            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
            byte[] hash = hashFunc.ComputeHash(pubKey.ToByteArray(useCompressed));
            var ops = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
            SetData(ops);
        }


        /// <summary>
        /// Sets this script to a P2SH-P2WSH redeem script using the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="witnessScript">Script to use</param>
        public void SetToP2SH_P2WSH(IScript witnessScript)
        {
            if (witnessScript is null)
                throw new ArgumentNullException(nameof(witnessScript), "Witness script can not be null.");

            using Sha256 witHashFunc = new Sha256();
            byte[] hash = witHashFunc.ComputeHash(witnessScript.Data);
            var ops = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
            SetData(ops);
        }
    }
}
