// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Collections.Generic;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// The script included in transaction outputs which sets the conditions that must be fulfilled for those outputs to be spent. 
    /// Also known as public key script, scriptPub or locking script.
    /// Implements <see cref="IPubkeyScript"/> and inherits from <see cref="Script"/>.
    /// </summary>
    public class PubkeyScript : Script, IPubkeyScript
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PubkeyScript"/>.
        /// </summary>
        public PubkeyScript() : base(Constants.MaxScriptLength)
        {
            ScriptType = ScriptType.ScriptPub;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PubkeyScript"/> using the given byte array as the script.
        /// </summary>
        /// <remarks>
        /// Pubkey scripts are not evaluated in outputs until they are spent (become an input!) which means they can be
        /// anything including an invalid script. See tests for more information.
        /// </remarks>
        /// <param name="data">Data to use</param>
        public PubkeyScript(byte[] data) : base(Constants.MaxScriptLength)
        {
            scrData = data;
        }



        private byte[] scrData;
        private const int MinMultiPubCount = 0;
        private const int MaxMultiPubCount = 20;

        private readonly Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
        private readonly Sha256 witHashFunc = new Sha256(false);
        private readonly Address addrManager = new Address();



        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            if (scrData == null)
            {
                base.Serialize(stream);
            }
            else
            {
                CompactInt len = new CompactInt(scrData.Length);
                len.WriteToStream(stream);
                stream.Write(scrData);
            }
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (!CompactInt.TryRead(stream, out CompactInt len, out error))
            {
                return false;
            }

            if (!stream.TryReadByteArray((int)len, out scrData))
            {
                error = Err.EndOfStream;
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public PubkeyScriptType GetPublicScriptType()
        {
            if ((OperationList == null || OperationList.Length == 0) && scrData != null)
            {
                FastStreamReader stream = new FastStreamReader(scrData);
                int offset = 0;
                List<IOperation> opList = new List<IOperation>();
                while (offset < scrData.Length)
                {
                    if (!TryRead(stream, opList, ref offset, out _))
                    {
                        break;
                    }
                }
                if (offset == scrData.Length)
                {
                    OperationList = opList.ToArray();
                }
            }

            if (OperationList == null || OperationList.Length == 0)
            {
                return PubkeyScriptType.Empty;
            }
            else if (OperationList.Length == 1 && OperationList[0] is ReturnOp)
            {
                return PubkeyScriptType.RETURN;
            }
            else if (OperationList.Length == 2 &&
                     OperationList[0] is PushDataOp push &&
                     OperationList[1] is CheckSigOp)
            {
                if (push.data?.Length == CompPubKeyLength || push.data?.Length == UncompPubKeyLength)
                {
                    return PubkeyScriptType.P2PK;
                }
            }
            else if (OperationList.Length == 5 &&
                     OperationList[0] is DUPOp &&
                     OperationList[1] is Hash160Op &&
                     OperationList[2] is PushDataOp push2 && push2.data?.Length == hashFunc.HashByteSize &&
                     OperationList[3] is EqualVerifyOp &&
                     OperationList[4] is CheckSigOp)
            {
                return PubkeyScriptType.P2PKH;
            }
            else if (OperationList.Length == 3 &&
                     OperationList[0] is Hash160Op &&
                     OperationList[1] is PushDataOp push3 && push3.data?.Length == hashFunc.HashByteSize &&
                     OperationList[2] is EqualOp)
            {
                return PubkeyScriptType.P2SH;
            }
            else if (OperationList.Length == 2 &&
                     OperationList[0] is PushDataOp && OperationList[0].OpValue == OP._0 &&
                     OperationList[1] is PushDataOp push4 && push4.data?.Length == hashFunc.HashByteSize)
            {
                return PubkeyScriptType.P2WPKH;
            }
            else if (OperationList.Length == 2 &&
                     OperationList[0] is PushDataOp && OperationList[0].OpValue == OP._0 &&
                     OperationList[1] is PushDataOp push5 && push5.data?.Length == witHashFunc.HashByteSize)
            {
                return PubkeyScriptType.P2WSH;
            }
            // OP_num n*<pubkey> OP_num OP_CheckMultiSig
            else if (OperationList.Length >= 4 &&
                     OperationList[0] is PushDataOp push6 &&
                     OperationList[^2] is PushDataOp push7 &&
                     OperationList[^1] is CheckMultiSigOp)
            {
                // TODO: check for values of PushData and count of pubkeys to be correct.
                if (push6.TryGetNumber(out long m, out string _) && m <= MinMultiPubCount &&
                    push7.TryGetNumber(out long n, out string _) && n <= MaxMultiPubCount &&
                    m <= n && OperationList.Length == n + 3)
                {
                    return PubkeyScriptType.P2MS;
                }
            }

            return PubkeyScriptType.Unknown;
        }


        /// <summary>
        /// Sets this script to a "pay to pubkey" script using the given <see cref="PublicKey"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="pubKey">Public key to use</param>
        /// <param name="useCompressed">Determines whether to use compressed or uncompressed <see cref="PublicKey"/> format</param>
        public void SetToP2PK(PublicKey pubKey, bool useCompressed)
        {
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Pubkey can not be null.");

            OperationList = new IOperation[]
            {
                new PushDataOp(pubKey.ToByteArray(useCompressed)),
                new CheckSigOp()
            };
        }


        /// <summary>
        /// Sets this script to a "pay to pubkey hash" script using the given hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash to use</param>
        public void SetToP2PKH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != hashFunc.HashByteSize)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {hashFunc.HashByteSize} bytes long.");

            OperationList = new IOperation[]
            {
                new DUPOp(),
                new Hash160Op(),
                new PushDataOp(hash),
                new EqualVerifyOp(),
                new CheckSigOp()
            };
        }

        /// <summary>
        /// Sets this script to a "pay to pubkey hash" script using the given <see cref="PublicKey"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="pubKey"><see cref="PublicKey"/> to use</param>
        /// <param name="useCompressed">Indicates whether to use compressed or uncompressed <see cref="PublicKey"/> format</param>
        public void SetToP2PKH(PublicKey pubKey, bool useCompressed)
        {
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Pubkey can not be null.");

            byte[] hash = hashFunc.ComputeHash(pubKey.ToByteArray(useCompressed));
            SetToP2PKH(hash);
        }

        /// <summary>
        /// Sets this script to a "pay to pubkey hash" script using the given base-58 encoded address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">P2PKH (Base-58 encoded) address to use</param>
        public void SetToP2PKH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be null or empty.");
            if (!addrManager.VerifyType(address, PubkeyScriptType.P2PKH, out byte[] hash))
                throw new FormatException("Invalid P2PKH address.");

            SetToP2PKH(hash);
        }


        /// <summary>
        /// Sets this script to a "pay to script hash" script using the given hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash to use</param>
        public void SetToP2SH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != hashFunc.HashByteSize)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {hashFunc.HashByteSize} bytes long.");

            OperationList = new IOperation[]
            {
                new Hash160Op(),
                new PushDataOp(hash),
                new EqualOp()
            };
        }

        /// <summary>
        /// Sets this script to a "pay to script hash" script using the given base-58 encoded P2SH address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">P2SH (Base-58 encoded) address to use</param>
        public void SetToP2SH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be empty or null.");
            if (!addrManager.VerifyType(address, PubkeyScriptType.P2SH, out byte[] hash))
                throw new FormatException("Invalid P2SH address.");

            SetToP2SH(hash);
        }

        /// <summary>
        /// Sets this script to a "pay to script hash" script using the given <see cref="IRedeemScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="redeem">Redeem script to use</param>
        public void SetToP2SH(IRedeemScript redeem)
        {
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");

            byte[] scrBytes = redeem.ToByteArray();
            byte[] hash = hashFunc.ComputeHash(scrBytes);
            SetToP2SH(hash);
        }


        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash inside a pay to script hash" script 
        /// using the given <see cref="PublicKey"/>.
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
                throw new ArgumentNullException(nameof(pubKey), "Pubkey can not be null.");

            RedeemScript redeemScrBuilder = new RedeemScript();
            redeemScrBuilder.SetToP2SH_P2WPKH(pubKey, useCompressed);
            SetToP2SH(redeemScrBuilder);
        }

        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash inside a pay to script hash" script 
        /// using the given <see cref="IRedeemScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="redeem">The <see cref="IRedeemScript"/> to use</param>
        public void SetToP2SH_P2WPKH(IRedeemScript redeem)
        {
            if (redeem == null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            // For SetToP2SH the IScript type doesn't matter. 
            // But it must be checked here to be explicit.
            if (redeem.GetRedeemScriptType() != RedeemScriptType.P2SH_P2WPKH)
                throw new ArgumentException("Invalid redeem script type.");

            SetToP2SH(redeem);
        }


        /// <summary>
        /// Sets this script to a "pay to witness script hash inside a pay to script hash" script 
        /// using the given <see cref="IRedeemScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="redeem">The <see cref="RedeemScript"/> to use</param>
        public void SetToP2SH_P2WSH(IRedeemScript redeem)
        {
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            // For SetToP2SH the IScript type doesn't matter. 
            // But it must be checked here to be explicit.
            if (redeem.GetRedeemScriptType() != RedeemScriptType.P2SH_P2WSH)
                throw new ArgumentException("Invalid redeem script type.");

            SetToP2SH(redeem);
        }


        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash" script using the given public key hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash of the public key to use</param>
        public void SetToP2WPKH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != hashFunc.HashByteSize)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {hashFunc.HashByteSize} bytes long.");

            OperationList = new IOperation[]
            {
                // TODO: OP_0 is the version (?) and can be changed in future. 20 bytes hash size is also for version 0
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
        }

        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash" script using the given <see cref="PublicKey"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="pubKey">Public key to use</param>
        /// /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates whether to use compressed or uncompressed public key in the redeem script.
        /// <para/> * Note that uncompressed public keys are non-standard and can lead to funds being lost.
        /// </param>
        public void SetToP2WPKH(PublicKey pubKey, bool useCompressed = true)
        {
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Pubkey can not be null.");

            byte[] hash = hashFunc.ComputeHash(pubKey.ToByteArray(useCompressed));
            SetToP2WPKH(hash);
        }

        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash" script using the given address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">P2WPKH (native SegWit, Bech32 encoded) address to use</param>
        public void SetToP2WPKH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be empty or null.");
            if (!addrManager.VerifyType(address, PubkeyScriptType.P2WPKH, out byte[] hash))
                throw new FormatException("Invalid P2WPKH address.");

            SetToP2WPKH(hash);
        }


        /// <summary>
        /// Sets this script to a "pay to witness script hash" script using the given hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash to use</param>
        public void SetToP2WSH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != witHashFunc.HashByteSize)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {witHashFunc.HashByteSize} bytes long.");

            OperationList = new IOperation[]
            {
                // TODO: same as SetToP2WPKH
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
        }

        /// <summary>
        /// Sets this script to a "pay to witness script hash" script using the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="scr">Script to use</param>
        public void SetToP2WSH(IScript scr)
        {
            if (scr == null)
                throw new ArgumentNullException(nameof(scr), "Witness script can not be null.");

            byte[] scrBa = scr.ToByteArray();
            byte[] hash = witHashFunc.ComputeHash(scrBa);
            SetToP2WSH(hash);
        }

        /// <summary>
        /// Sets this script to a "pay to witness script hash" script using the given address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">Address to use</param>
        public void SetToP2WSH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be null or empty.");
            if (!addrManager.VerifyType(address, PubkeyScriptType.P2WSH, out byte[] hash))
                throw new FormatException("Invalid P2WSH address.");

            SetToP2WSH(hash);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="pubKeyList"></param>
        public void SetToP2MS(int m, int n, Tuple<PublicKey, bool>[] pubKeyList)
        {
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

            // OP_m | [pub1|pub2|...|pub(n)] | OP_n | OP_CheckMultiSig
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
        /// Sets this script to an unspendable "OP_RETURN" script using the given data.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="data">Byte array to use</param>
        public void SetToReturn(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");

            OperationList = new IOperation[]
            {
                new ReturnOp(data)
            };
        }

        /// <summary>
        /// Sets this script to an unspendable "OP_RETURN" script using the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="scr">Script to use</param>
        public void SetToReturn(IScript scr)
        {
            if (scr is null)
                throw new ArgumentNullException(nameof(scr), "Script can not be null.");

            byte[] data = scr.ToByteArray();
            SetToReturn(data);
        }


        internal PubkeyScript ConvertP2WPKH_to_P2PKH()
        {
            if (GetPublicScriptType() != PubkeyScriptType.P2WPKH)
            {
                throw new ArgumentException("This conversion only works for P2WPKH script types.");
            }
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
