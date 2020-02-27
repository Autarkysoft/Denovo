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
    /// The script that is used in <see cref="ISignatureScript"/>s of pay to script hash type <see cref="IPubkeyScript"/>s.
    /// Implements <see cref="IRedeemScript"/> and inherits from <see cref="Script"/>.
    /// </summary>
    public class RedeemScript : Script, IRedeemScript
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RedeemScript"/>.
        /// </summary>
        public RedeemScript() : base(520)
        {
            ScriptType = ScriptType.ScriptRedeem;
            witHashFunc = new Sha256(false);
        }



        private readonly Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
        private readonly Sha256 witHashFunc = new Sha256(false);



        /// <inheritdoc/>
        public RedeemScriptType GetRedeemScriptType()
        {
            if (OperationList == null || OperationList.Length == 0)
            {
                return RedeemScriptType.Empty;
            }
            else if (OperationList.Length == 2 &&
                     OperationList[0] is PushDataOp op0 && op0.OpValue == OP._0 &&
                     OperationList[1] is PushDataOp push1)
            {
                if (push1.data?.Length == hashFunc.HashByteSize)
                {
                    return RedeemScriptType.P2SH_P2WPKH;
                }
                else if (push1.data?.Length == witHashFunc.HashByteSize)
                {
                    return RedeemScriptType.P2SH_P2WSH;
                }
            }
            else if (OperationList.Length == 5 &&
                     OperationList[0] is PushDataOp push2 && push2.TryGetNumber(out _, out _, true, 5) &&
                     OperationList[1] is CheckLocktimeVerifyOp &&
                     OperationList[2] is DROPOp &&
                     OperationList[3] is PushDataOp push3 &&
                     (push3.data?.Length == CompPubKeyLength || push3.data?.Length == UncompPubKeyLength) &&
                     OperationList[4] is CheckSigOp)
            {
                return RedeemScriptType.CheckLocktimeVerify;
            }
            else if (OperationList[^1] is CheckMultiSigOp)
            {
                // Multi-sig redeem scripts can be as simple as a single CheckMultiSigOp OP (pointless script that anyone can spend)
                // to the usual multi-sig scripts with (OP_m <n*pubkeys> OP_n OP_CheckMultiSig) script up to 15 pubkeys
                return RedeemScriptType.MultiSig;
            }

            return RedeemScriptType.Unknown;
        }


        /// <summary>
        /// Sets this script to a "m of n multi-signature" script using the given <see cref="PublicKey"/>s.
        /// </summary>
        /// <remarks>
        /// Since normally a m-of-n redeem script is created from all compressed or all uncompressed public keys
        /// this method has one boolean determining that. If a mixture is needed the <see cref="IScript.OperationList"/>
        /// should be set manually.
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
            OperationList = new IOperation[pubKeys.Length + 3];
            OperationList[0] = new PushDataOp(m);
            OperationList[^2] = new PushDataOp(pubKeys.Length);
            OperationList[^1] = new CheckMultiSigOp();
            int i = 1;
            foreach (var item in pubKeys)
            {
                OperationList[i++] = new PushDataOp(item.ToByteArray(useCompressed));
            }
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

            byte[] hash = hashFunc.ComputeHash(pubKey.ToByteArray(useCompressed));
            OperationList = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
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

            byte[] hash = witHashFunc.ComputeHash(witnessScript.ToByteArray());
            OperationList = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
        }


        // TODO: is this used anywhere?
        public PubkeyScript ConvertP2WPKH_to_P2PKH()
        {
            if (GetRedeemScriptType() != RedeemScriptType.P2SH_P2WPKH)
                throw new ArgumentException("This conversion only works for P2SH-P2WPKH redeem script types.");

            IOperation pushHash = OperationList[1];

            PubkeyScript res = new PubkeyScript()
            {
                OperationList = new IOperation[]
                {
                    new DUPOp(),
                    new Hash160Op(),
                    pushHash,
                    new EqualVerifyOp(),
                    new CheckSigOp()
                },
            };

            return res;
        }
    }
}
