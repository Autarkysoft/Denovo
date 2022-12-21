// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// The script included in transaction outputs which sets the conditions that must be fulfilled for those outputs to be spent. 
    /// Also known as public key script, scriptPub or locking script.
    /// <para/>Implements <see cref="IPubkeyScript"/> and inherits from <see cref="Script"/>.
    /// </summary>
    public class PubkeyScript : Script, IPubkeyScript
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PubkeyScript"/>.
        /// </summary>
        public PubkeyScript()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PubkeyScript"/> using the given byte array as the script.
        /// </summary>
        /// <remarks>
        /// Pubkey scripts are not evaluated in outputs until they are spent (become an input!) which means they can be
        /// anything including an invalid script. See tests for more information.
        /// Note that this can result in funds being lost forever if used incorrectly.
        /// </remarks>
        /// <param name="data">Data to use</param>
        public PubkeyScript(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PubkeyScript"/> using the given operation array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ops">An array of operations</param>
        public PubkeyScript(IOperation[] ops)
        {
            if (ops == null)
                throw new ArgumentNullException(nameof(ops), "Operation array can not be null.");

            SetData(ops);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PubkeyScript"/> using the given address.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="address">Bitcoin address to use</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/> Network type</param>
        public PubkeyScript(string address, NetworkType netType = NetworkType.MainNet)
        {
            AddressType adrt = Address.GetAddressType(address, netType, out byte[] data);
            switch (adrt)
            {
                case AddressType.Unknown:
                case AddressType.Invalid:
                    throw new ArgumentException("Invalid or unknown address.");
                case AddressType.P2PKH:
                    SetToP2PKH(data);
                    break;
                case AddressType.P2SH:
                    SetToP2SH(data);
                    break;
                case AddressType.P2WPKH:
                    SetToP2WPKH(data);
                    break;
                case AddressType.P2WSH:
                    SetToP2WSH(data);
                    break;
                case AddressType.P2TR:
                    SetToP2TR(data);
                    break;
                default:
                    throw new ArgumentException("Undefined address type");
            }
        }


        /// <inheritdoc/>
        public bool IsUnspendable() => (Data.Length > 0 && Data[0] == (byte)OP.RETURN) || Data.Length > Constants.MaxScriptLength;


        /// <inheritdoc/>
        public PubkeyScriptType GetPublicScriptType()
        {
            bool b = TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] OperationList, out _, out _);

            if (!b)
            {
                return PubkeyScriptType.Unknown;
            }
            else if (OperationList.Length == 0)
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
                if (push.data?.Length == Constants.CompressedPubkeyLen || push.data?.Length == Constants.UncompressedPubkeyLen)
                {
                    return PubkeyScriptType.P2PK;
                }
            }
            else if (OperationList.Length == 5 &&
                     OperationList[0] is DUPOp &&
                     OperationList[1] is Hash160Op &&
                     OperationList[2] is PushDataOp push2 && push2.data?.Length == Constants.Hash160Length &&
                     OperationList[3] is EqualVerifyOp &&
                     OperationList[4] is CheckSigOp)
            {
                return PubkeyScriptType.P2PKH;
            }
            else if (OperationList.Length == 3 &&
                     OperationList[0] is Hash160Op &&
                     OperationList[1] is PushDataOp push3 && push3.data?.Length == Constants.Hash160Length &&
                     OperationList[2] is EqualOp)
            {
                return PubkeyScriptType.P2SH;
            }
            else if (OperationList.Length == 2 &&
                     OperationList[0] is PushDataOp && OperationList[0].OpValue == OP._0 &&
                     OperationList[1] is PushDataOp push4 && push4.data?.Length == Constants.Hash160Length)
            {
                return PubkeyScriptType.P2WPKH;
            }
            else if (OperationList.Length == 2 &&
                     OperationList[0] is PushDataOp && OperationList[0].OpValue == OP._0 &&
                     OperationList[1] is PushDataOp push5 && push5.data?.Length == Constants.Sha256Length)
            {
                return PubkeyScriptType.P2WSH;
            }

            return PubkeyScriptType.Unknown;
        }

        /// <inheritdoc/>
        public PubkeyScriptSpecialType GetSpecialType(IConsensus consensus)
        {
            // DUP HASH160 0x14(data20) EqualVerify CheckSig
            if (Data.Length == 25 && Data[24] == (byte)OP.CheckSig && Data[23] == (byte)OP.EqualVerify &&
                Data[2] == 20 && Data[1] == (byte)OP.HASH160 && Data[0] == (byte)OP.DUP)
            {
                return PubkeyScriptSpecialType.P2PKH;
            }
            // HASH160 0x14(data20) EQUAL
            else if (Data.Length == 23 && consensus.IsBip16Enabled &&
                     Data[0] == (byte)OP.HASH160 && Data[1] == 20 && Data[^1] == (byte)OP.EQUAL)
            {
                return PubkeyScriptSpecialType.P2SH;
            }
            // https://github.com/bitcoin/bitcoin/blob/476436b2dec254bb988f8c7a6cbec1d7bb7cecfd/src/script/script.cpp#L215-L231
            else if (consensus.IsSegWitEnabled &&
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
                        return PubkeyScriptSpecialType.P2WPKH;
                    }
                    // OP_0 0x20(data32)
                    else if (Data.Length == 34)
                    {
                        return PubkeyScriptSpecialType.P2WSH;
                    }
                    else
                    {
                        return PubkeyScriptSpecialType.InvalidWitness;
                    }
                }
                // Version 1 witness program
                else if (consensus.IsTaprootEnabled && Data[0] == (byte)OP._1 && Data.Length == 34)
                {
                    return PubkeyScriptSpecialType.P2TR;
                }
                // OP_num PushData()
                else
                {
                    return PubkeyScriptSpecialType.UnknownWitness;
                }
            }

            return PubkeyScriptSpecialType.None;
        }

        /// <summary>
        /// Sets this script to a "pay to pubkey" script using the given <see cref="Point"/>.
        /// </summary>
        /// <param name="pubKey">Public key to use</param>
        /// <param name="useCompressed">Determines whether to use compressed or uncompressed <see cref="Point"/> format</param>
        public void SetToP2PK(in Point pubKey, bool useCompressed)
        {
            var ops = new IOperation[]
            {
                new PushDataOp(pubKey.ToByteArray(useCompressed)),
                new CheckSigOp()
            };
            SetData(ops);
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
            if (hash.Length != Constants.Hash160Length)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {Constants.Hash160Length} bytes long.");

            var ops = new IOperation[]
            {
                new DUPOp(),
                new Hash160Op(),
                new PushDataOp(hash),
                new EqualVerifyOp(),
                new CheckSigOp()
            };
            SetData(ops);
        }

        /// <summary>
        /// Sets this script to a "pay to pubkey hash" script using the given <see cref="Point"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="pubKey"><see cref="Point"/> to use</param>
        /// <param name="useCompressed">Indicates whether to use compressed or uncompressed <see cref="Point"/> format</param>
        public void SetToP2PKH(in Point pubKey, bool useCompressed)
        {
            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
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
            if (!Address.VerifyType(address, PubkeyScriptType.P2PKH, out byte[] hash))
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
            if (hash.Length != Constants.Hash160Length)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {Constants.Hash160Length} bytes long.");

            var ops = new IOperation[]
            {
                new Hash160Op(),
                new PushDataOp(hash),
                new EqualOp()
            };
            SetData(ops);
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
            if (!Address.VerifyType(address, PubkeyScriptType.P2SH, out byte[] hash))
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

            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
            byte[] hash = hashFunc.ComputeHash(redeem.Data);
            SetToP2SH(hash);
        }


        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash inside a pay to script hash" script 
        /// using the given <see cref="Point"/>.
        /// </summary>
        /// <param name="pubKey">Public key to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates whether to use compressed or uncompressed public key in the redeem script.
        /// <para/> * Note that uncompressed public keys are non-standard and can lead to funds being lost.
        /// </param>
        public void SetToP2SH_P2WPKH(in Point pubKey, bool useCompressed = true)
        {
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
        /// Sets this script to a "pay to witness pubkey hash" script for version 0 witness program
        /// using the given public key hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash of the public key to use</param>
        public void SetToP2WPKH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != Constants.Hash160Length)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {Constants.Hash160Length} bytes long.");

            var ops = new IOperation[]
            {
                // TODO: OP_0 is the version (?) and can be changed in future. 20 bytes hash size is also for version 0
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
            SetData(ops);
        }

        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash" script for version 0 witness program
        /// using the given <see cref="Point"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="pubKey">Public key to use</param>
        /// /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates whether to use compressed or uncompressed public key in the redeem script.
        /// <para/> * Note that uncompressed public keys are non-standard and can lead to funds being lost.
        /// </param>
        public void SetToP2WPKH(in Point pubKey, bool useCompressed = true)
        {
            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
            byte[] hash = hashFunc.ComputeHash(pubKey.ToByteArray(useCompressed));
            SetToP2WPKH(hash);
        }

        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash" script for version 0 witness program
        /// using the given address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">P2WPKH (native SegWit, Bech32 encoded) address to use</param>
        public void SetToP2WPKH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be empty or null.");
            if (!Address.VerifyType(address, PubkeyScriptType.P2WPKH, out byte[] hash))
                throw new FormatException("Invalid P2WPKH address.");

            SetToP2WPKH(hash);
        }


        /// <summary>
        /// Sets this script to a "pay to witness script hash" script for version 0 witness program 
        /// using the given hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash to use</param>
        public void SetToP2WSH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != Constants.Sha256Length)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {Constants.Sha256Length} bytes long.");

            var ops = new IOperation[]
            {
                // TODO: same as SetToP2WPKH about version and length
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
            SetData(ops);
        }

        /// <summary>
        /// Sets this script to a "pay to witness script hash" script for version 0 witness program
        /// using the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="scr">Script to use</param>
        public void SetToP2WSH(IScript scr)
        {
            if (scr == null)
                throw new ArgumentNullException(nameof(scr), "Witness script can not be null.");

            using Sha256 witHashFunc = new Sha256();
            byte[] hash = witHashFunc.ComputeHash(scr.Data);
            SetToP2WSH(hash);
        }

        /// <summary>
        /// Sets this script to a "pay to witness script hash" script for version 0 witness program 
        /// using the given address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">Address to use</param>
        public void SetToP2WSH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be null or empty.");
            if (!Address.VerifyType(address, PubkeyScriptType.P2WSH, out byte[] hash))
                throw new FormatException("Invalid P2WSH address.");

            SetToP2WSH(hash);
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

            SetData(new IOperation[] { new ReturnOp(data) });
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

            SetToReturn(scr.Data);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void SetToWitnessCommitment(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));
            if (hash.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(hash), "Hash should be 32 bytes");

            Data = new byte[38];
            Data[0] = 0x6a;
            Data[1] = 0x24;
            Data[2] = 0xaa;
            Data[3] = 0x21;
            Data[4] = 0xa9;
            Data[5] = 0xed;
            Buffer.BlockCopy(hash, 0, Data, 6, 32);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void SetToP2TR(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != Sha256.HashByteSize)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {Sha256.HashByteSize} bytes long.");

            var ops = new IOperation[]
            {
                new PushDataOp(OP._1),
                new PushDataOp(hash)
            };
            SetData(ops);
        }
    }
}
