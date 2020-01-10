using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
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
        public RedeemScript() : base(10000)
        {
            IsWitness = false;
            ScriptType = ScriptType.ScriptRedeem;
            witHashFunc = new Sha256(false);
        }



        private const int MinMultiPubCount = 0;
        private const int MaxMultiPubCount = 20;

        private readonly Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
        private readonly Sha256 witHashFunc = new Sha256(false);



        /// <summary>
        /// Returns <see cref="RedeemScriptType"/> type of this instance if it was defined.
        /// </summary>
        /// <returns>The <see cref="RedeemScriptType"/></returns>
        public RedeemScriptType GetRedeemScriptType()
        {
            if (OperationList == null || OperationList.Length == 0)
            {
                return RedeemScriptType.Empty;
            }
            else if (OperationList.Length == 2 &&
                OperationList[0] is PushDataOp && OperationList[0].OpValue == OP._0 &&
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
                    OperationList[0] is PushDataOp push2 && push2.data?.Length > 1 && push2.data.Length < 5 &&
                    OperationList[1] is CheckLocktimeVerifyOp &&
                    OperationList[2] is DROPOp &&
                    OperationList[3] is PushDataOp push3 &&
                    (push3.data?.Length == CompPubKeyLength || push3.data?.Length == UncompPubKeyLength) &&
                    OperationList[4] is CheckSigOp)
            {
                return RedeemScriptType.CheckLocktimeVerify;
            }

            return RedeemScriptType.Unknown;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="pubKeyList"></param>
        public void SetToMultiSig(int m, int n, Tuple<PublicKey, bool>[] pubKeyList)
        {
            // TODO: read
            // https://github.com/bitcoin/bitcoin/blob/871d3ae45b642c903ed9c54235c09d62f6cf9a16/src/script/script.h#L22-L35
            // https://bitcoin.stackexchange.com/questions/23893/what-are-the-limits-of-m-and-n-in-m-of-n-multisig-addresses

            if (m < MinMultiPubCount || m > MaxMultiPubCount || m > n)
            {
                throw new ArgumentOutOfRangeException(nameof(m),
                    $"M must be between {MinMultiPubCount} and {MaxMultiPubCount} and smaller than N.");
            }
            if (n < MinMultiPubCount || n > MaxMultiPubCount)
                throw new ArgumentOutOfRangeException(nameof(n), $"N must be between {MinMultiPubCount} and {MaxMultiPubCount}.");
            if (pubKeyList == null || pubKeyList.Length == 0)
                throw new ArgumentNullException(nameof(pubKeyList), "Pubkey list can not be null or empty.");
            if (pubKeyList.Length != n)
                throw new ArgumentOutOfRangeException(nameof(pubKeyList), $"Pubkey list must contain N (={n}) items.");

            // OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
            OperationList = new IOperation[n + 3];
            OperationList[0] = new PushDataOp(m);
            OperationList[n + 1] = new PushDataOp(n);
            OperationList[n + 2] = new CheckMultiSigOp();
            int i = 1;
            foreach (var item in pubKeyList)
            {
                OperationList[i++] = new PushDataOp(item.Item1.ToByteArray(item.Item2));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pubKey"></param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates whether to use compressed or uncompressed public key in the redeem script.
        /// <para/> * Note that uncompressed public keys are non-standard and can lead to funds being lost.
        /// </param>
        public void SetToP2SH_P2WPKH(PublicKey pubKey, bool useCompressed = true)
        {
            byte[] hash = hashFunc.ComputeHash(pubKey.ToByteArray(true)); // Always use compressed
            OperationList = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
        }

        public void SetToP2SH_P2WSH(IScript witnessScript)
        {
            byte[] hash = witHashFunc.ComputeHash(witnessScript.ToByteArray());
            OperationList = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
        }



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
