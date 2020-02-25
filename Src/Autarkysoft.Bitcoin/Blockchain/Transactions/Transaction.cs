// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Transactions
{
    /// <summary>
    /// Bitcoin transaction!
    /// Implements <see cref="ITransaction"/>.
    /// </summary>
    public class Transaction : ITransaction
    {
        /// <summary>
        /// Initializes a new empty instance of <see cref="Transaction"/>.
        /// </summary>
        public Transaction()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Transaction"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ver">Version</param>
        /// <param name="txIns">List of inputs</param>
        /// <param name="txOuts">List of outputs</param>
        /// <param name="lt">LockTime</param>
        /// <param name="witnesses">[Default value = null] List of witnesses (default is null).</param>
        public Transaction(int ver, TxIn[] txIns, TxOut[] txOuts, LockTime lt, WitnessScript[] witnesses = null)
        {
            Version = ver;
            TxInList = txIns;
            TxOutList = txOuts;
            LockTime = lt;

            WitnessList = witnesses;
        }



        private const int MaxTxSize = 4_000_000;
        private readonly Sha256 hashFunc = new Sha256(true);
        private readonly Ripemd160Sha256 addrHashFunc = new Ripemd160Sha256();

        private int _version;
        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public int Version
        {
            get => _version;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Version), "Version can not be negative.");

                _version = value;
            }
        }

        private TxIn[] _tins = new TxIn[1];
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public TxIn[] TxInList
        {
            get => _tins;
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(TxInList), "List of transaction inputs can not be null or empty.");

                _tins = value;
            }
        }

        private TxOut[] _tout = new TxOut[1];
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public TxOut[] TxOutList
        {
            get => _tout;
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(TxOutList), "List of transaction outputs can not be null or empty.");

                _tout = value;
            }
        }

        /// <inheritdoc/>
        public IWitnessScript[] WitnessList { get; set; }

        private LockTime _lockTime;
        /// <inheritdoc/>
        public LockTime LockTime
        {
            get => _lockTime;
            set => _lockTime = value;
        }


        /// <summary>
        /// Total transaction size in bytes
        /// </summary>
        public int TotalSize => ToByteArray().Length;

        /// <summary>
        /// Transaction size without witness
        /// </summary>
        public int BaseSize => ToByteArrayWithoutWitness().Length;

        /// <summary>
        /// Transaction weight (ie. 3x <see cref="BaseSize"/> + <see cref="TotalSize"/>)
        /// </summary>
        public int Weight => (BaseSize * 3) + TotalSize;

        /// <summary>
        /// Virtual transaction size (ie. 1/4 * <see cref="Weight"/>)
        /// </summary>
        public int VirtualSize => Weight / 4;


        /// <inheritdoc/>
        public byte[] GetTransactionHash()
        {
            // Tx hash is always stripping witness
            byte[] bytesToHash = ToByteArrayWithoutWitness();
            return hashFunc.ComputeHash(bytesToHash);
        }

        /// <inheritdoc/>
        public string GetTransactionId()
        {
            // TODO: verify if transaction is signed then give TX ID. an unsigned tx doesn't have a TX ID.
            byte[] hashRes = GetTransactionHash();
            Array.Reverse(hashRes);
            return Base16.Encode(hashRes);
        }

        /// <inheritdoc/>
        public string GetWitnessTransactionId()
        {
            // TODO: same as above (verify if signed)
            byte[] hashRes = hashFunc.ComputeHash(ToByteArray());
            Array.Reverse(hashRes);
            return Base16.Encode(hashRes);
        }


        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            stream.Write(Version);

            bool hasWitness = !(WitnessList is null) && WitnessList.Length != 0;
            if (hasWitness)
            {
                // Add SegWit marker
                stream.Write(new byte[2] { 0x00, 0x01 });
            }

            CompactInt tinCount = new CompactInt(TxInList.Length);
            tinCount.WriteToStream(stream);

            foreach (var tin in TxInList)
            {
                tin.Serialize(stream);
            }

            CompactInt toutCount = new CompactInt(TxOutList.Length);
            toutCount.WriteToStream(stream);

            foreach (var tout in TxOutList)
            {
                tout.Serialize(stream);
            }

            if (hasWitness)
            {
                foreach (var wit in WitnessList)
                {
                    wit.Serialize(stream);
                }
            }

            LockTime.WriteToStream(stream);
        }

        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToByteArray()
        {
            FastStream stream = new FastStream();
            Serialize(stream);
            return stream.ToByteArray();
        }

        /// <summary>
        /// Converts this transaction to its byte array representation skipping witness flag and any witnesses that may be
        /// present and writes the result to the given stream.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void SerializeWithoutWitness(FastStream stream)
        {
            stream.Write(Version);

            CompactInt tinCount = new CompactInt(TxInList.Length);
            tinCount.WriteToStream(stream);

            foreach (var tin in TxInList)
            {
                tin.Serialize(stream);
            }

            CompactInt toutCount = new CompactInt(TxOutList.Length);
            toutCount.WriteToStream(stream);

            foreach (var tout in TxOutList)
            {
                tout.Serialize(stream);
            }

            LockTime.WriteToStream(stream);
        }

        /// <summary>
        /// Converts this instance into its byte array representation while skipping witnesses.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToByteArrayWithoutWitness()
        {
            FastStream stream = new FastStream();
            SerializeWithoutWitness(stream);
            return stream.ToByteArray();
        }


        /// <summary>
        /// A special serialization done with the given <see cref="IScript"/> and based on the <see cref="SigHashType"/>
        /// used in signing operations. Return result is the hash result.
        /// </summary>
        /// <param name="scr">
        /// The locking script (<see cref="PubkeyScript"/> or <see cref="RedeemScript"/>) to be used in serialization.
        /// </param>
        /// <param name="inputIndex">Index of the input being signed</param>
        /// <param name="sht">Signature hash type</param>
        /// <returns>32 byte hash</returns>
        private byte[] SerializeForSigning(IScript scr, int inputIndex, SigHashType sht)
        {
            // TODO: change this into Sha256 itself with stream methods inside
            FastStream stream = new FastStream();

            bool anyone = (sht & SigHashType.AnyoneCanPay) == SigHashType.AnyoneCanPay;
            if (anyone)
            {
                sht ^= SigHashType.AnyoneCanPay;
            }

            if (sht == SigHashType.Single)
            {
                if (TxOutList.Length <= inputIndex)
                {
                    byte[] res = new byte[32];
                    res[0] = 1;
                    return res;
                }
            }

            stream.Write(Version);

            var tinCount = new CompactInt(anyone ? 1 : TxInList.Length);
            tinCount.WriteToStream(stream);

            if (anyone)
            {
                TxInList[inputIndex].Serialize(stream, scr, false);
            }
            else
            {
                PubkeyScript empty = new PubkeyScript();
                // Sequence of other inputs must be set to 0 if the SigHashType is Single or None
                bool changeSeq = sht != SigHashType.All;
                for (int i = 0; i < TxInList.Length; i++)
                {
                    if (i != inputIndex)
                    {
                        TxInList[i].Serialize(stream, empty, changeSeq);
                    }
                    else
                    {
                        TxInList[i].Serialize(stream, scr, false);
                    }
                }
            }

            // The switch expression does not handle all possible values of its input type (it is not exhaustive).
#pragma warning disable CS8509 
            CompactInt toutCount = sht switch
#pragma warning restore CS8509
            {
                SigHashType.All => new CompactInt((ulong)TxOutList.Length),
                SigHashType.None => new CompactInt(),
                SigHashType.Single => new CompactInt(inputIndex + 1),
            };
            toutCount.WriteToStream(stream);

            if (sht == SigHashType.All)
            {
                foreach (var tout in TxOutList)
                {
                    tout.Serialize(stream);
                }
            }
            else if (sht == SigHashType.Single)
            {
                for (int i = 0; i < inputIndex; i++)
                {
                    TxOutList[i].SerializeSigHashSingle(stream);
                }
                TxOutList[inputIndex].Serialize(stream);
            }

            LockTime.WriteToStream(stream);

            if (anyone)
            {
                sht |= SigHashType.AnyoneCanPay;
            }

            stream.Write((uint)sht);

            byte[] hash = hashFunc.ComputeHash(stream.ToByteArray());
            return hash;
        }

        /// <summary>
        /// A special serialization done with the given <see cref="IScript"/> and based on the <see cref="SigHashType"/>
        /// used in signing operations for SegWit transactions. Return result is the hash result.
        /// </summary>
        /// <param name="scr">
        /// The locking script (<see cref="PubkeyScript"/> or <see cref="RedeemScript"/>) to be used in serialization.
        /// </param>
        /// <param name="inputIndex">Index of the input being signed</param>
        /// <param name="amount">The amount in satoshi that is being spent</param>
        /// <param name="sht">Signature hash type</param>
        /// <returns>32 byte hash</returns>
        private byte[] SerializeForSigningSegWit(IScript scr, int inputIndex, ulong amount, SigHashType sht)
        {
            bool anyone = (sht & SigHashType.AnyoneCanPay) == SigHashType.AnyoneCanPay;
            if (anyone)
            {
                sht ^= SigHashType.AnyoneCanPay;
            }

            byte[] hashPrevouts;
            if (anyone)
            {
                hashPrevouts = new byte[32];
            }
            else
            {
                // Outpoints are 32 byte tx hash + 4 byte index
                FastStream prvOutStream = new FastStream(TxInList.Length * 36);
                foreach (var tin in TxInList)
                {
                    prvOutStream.Write(tin.TxHash);
                    prvOutStream.Write(tin.Index);
                }

                hashPrevouts = hashFunc.ComputeHash(prvOutStream.ToByteArray());
            }

            byte[] hashSequence;
            if (!anyone && sht == SigHashType.All)
            {
                // Sequences are 4 bytes each
                FastStream seqStream = new FastStream(TxInList.Length * 4);
                foreach (var tin in TxInList)
                {
                    seqStream.Write(tin.Sequence);
                }

                hashSequence = hashFunc.ComputeHash(seqStream.ToByteArray());
            }
            else
            {
                hashSequence = new byte[32];
            }

            byte[] hashOutputs;
            if (sht == SigHashType.All)
            {
                // 33 is the approximate size of most TxOuts
                FastStream outputStream = new FastStream(TxOutList.Length * 33);
                foreach (var tout in TxOutList)
                {
                    tout.Serialize(outputStream);
                }

                hashOutputs = hashFunc.ComputeHash(outputStream.ToByteArray());
            }
            else if (sht == SigHashType.Single && inputIndex < TxOutList.Length)
            {
                FastStream outputStream = new FastStream(33);
                TxOutList[inputIndex].Serialize(outputStream);

                hashOutputs = hashFunc.ComputeHash(outputStream.ToByteArray());
            }
            else
            {
                hashOutputs = new byte[32];
            }

            if (anyone)
            {
                sht |= SigHashType.AnyoneCanPay;
            }

            FastStream finalStream = new FastStream(182);
            finalStream.Write(Version);
            finalStream.Write(hashPrevouts);
            finalStream.Write(hashSequence);
            finalStream.Write(TxInList[inputIndex].TxHash);
            finalStream.Write(TxInList[inputIndex].Index);
            // the prevOutScript is a P2WPKH (0014<hash160>) which should be turned into 1976a914<hash160>88ac and placed here
            ((PubkeyScript)scr).ConvertP2WPKH_to_P2PKH().Serialize(finalStream);
            finalStream.Write(amount);
            finalStream.Write(TxInList[inputIndex].Sequence);
            finalStream.Write(hashOutputs);
            LockTime.WriteToStream(finalStream);
            finalStream.Write((uint)sht);

            byte[] hashPreimage = hashFunc.ComputeHash(finalStream.ToByteArray());
            return hashPreimage;
        }


        /// <exception cref="ArgumentException"/>
        private void CheckSht(SigHashType sht)
        {
            if ((sht & SigHashType.AnyoneCanPay) == SigHashType.AnyoneCanPay)
            {
                sht ^= SigHashType.AnyoneCanPay;
            }
            if (!Enum.IsDefined(typeof(SigHashType), sht))
                throw new ArgumentException("Undefined SigHashType.");
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht)
        {
            CheckSht(sht);
            if (prvTx is null)
                throw new ArgumentNullException(nameof(prvTx), "Previous transaction (to spend) can not be null.");
            if (inputIndex < 0)
                throw new ArgumentException("Index can not be negative.");
            if (inputIndex >= TxInList.Length)
                throw new ArgumentOutOfRangeException(nameof(inputIndex), "Not enough TxIns.");
            if (!((ReadOnlySpan<byte>)TxInList[inputIndex].TxHash).SequenceEqual(prvTx.GetTransactionHash()))
                throw new ArgumentException("Wrong previous transaction or index.");

            PubkeyScriptType scrType = prvTx.TxOutList[TxInList[inputIndex].Index].PubScript.GetPublicScriptType();
            if (scrType == PubkeyScriptType.P2PKH || scrType == PubkeyScriptType.P2PK)
            {
                return SerializeForSigning(prvTx.TxOutList[TxInList[inputIndex].Index].PubScript, inputIndex, sht);
            }
            else if (scrType == PubkeyScriptType.P2WPKH)
            {
                return SerializeForSigningSegWit(prvTx.TxOutList[TxInList[inputIndex].Index].PubScript, inputIndex,
                    prvTx.TxOutList[TxInList[inputIndex].Index].Amount, sht);
            }
            else if (scrType == PubkeyScriptType.CheckLocktimeVerify)
            {
                throw new NotImplementedException(); // TODO: implement this!
            }

            switch (scrType)
            {
                case PubkeyScriptType.Empty:
                    throw new ArgumentException($"Previous transaction's PubkeyScript at index {inputIndex} needs to be set first.");
                case PubkeyScriptType.P2SH:
                case PubkeyScriptType.P2WSH:
                    throw new ArgumentException("Can not sign a pay-to-scripthash output without a RedeemScript, use the correct method.");
                case PubkeyScriptType.RETURN:
                    throw new ArgumentException("Can not spend OP_RETURN outputs.");
                case PubkeyScriptType.Unknown:
                default:
                    throw new ArgumentException("Previous transaction's PubkeyScript type is not defined.");
            }
        }


        /// <inheritdoc/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht, IRedeemScript redeemScript)
        {
            CheckSht(sht);
            if (prvTx is null)
                throw new ArgumentNullException(nameof(prvTx), "Previous transaction (to spend) can not be null.");
            if (redeemScript is null)
                throw new ArgumentNullException(nameof(redeemScript), "Redeem script can not be null.");
            if (inputIndex < 0)
                throw new ArgumentException("Index can not be negative.");
            if (redeemScript == null)
                throw new ArgumentNullException(nameof(redeemScript), "Redeem script can not be null.");
            if (!((ReadOnlySpan<byte>)TxInList[inputIndex].TxHash).SequenceEqual(prvTx.GetTransactionHash()))
                throw new ArgumentException("Wrong previous transaction or index.");
            if (prvTx.TxOutList[TxInList[inputIndex].Index].PubScript.GetPublicScriptType() != PubkeyScriptType.P2SH)
                throw new ArgumentException("Invalid script type.");


            RedeemScriptType scrType = redeemScript.GetRedeemScriptType();

            if (scrType == RedeemScriptType.MultiSig)
            {
                return SerializeForSigning(redeemScript, inputIndex, sht);
            }
            else if (scrType == RedeemScriptType.P2SH_P2WPKH)
            {
                throw new NotImplementedException(); // TODO: implement this!
            }
            else if (scrType == RedeemScriptType.CheckLocktimeVerify)
            {
                throw new NotImplementedException(); // TODO: implement this!
            }

            switch (scrType)
            {
                case RedeemScriptType.Empty:
                    throw new ArgumentException($"Previous transaction's PubkeyScript at index {inputIndex} needs to be set first.");
                case RedeemScriptType.Unknown:
                default:
                    throw new ArgumentException("Previous transaction's RedeemScript type is not defined.");
            }
        }


        /// <inheritdoc/>
        public void WriteScriptSig(Signature sig, PublicKey pubKey, ITransaction prevTx, int inputIndex)
        {
            PubkeyScriptType scrType = prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.GetPublicScriptType();
            switch (scrType)
            {
                // TODO: add methods to interface to void casting
                case PubkeyScriptType.P2PK:
                    ((SignatureScript)TxInList[inputIndex].SigScript).SetToP2PK(sig);
                    break;

                case PubkeyScriptType.P2PKH:
                    byte[] expHash160 = ((PushDataOp)prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.OperationList[2]).data;
                    byte[] actualHash160 = addrHashFunc.ComputeHash(pubKey.ToByteArray(true));
                    bool compressed = true;
                    if (!((ReadOnlySpan<byte>)actualHash160).SequenceEqual(expHash160))
                    {
                        actualHash160 = addrHashFunc.ComputeHash(pubKey.ToByteArray(false));
                        if (!((ReadOnlySpan<byte>)actualHash160).SequenceEqual(expHash160))
                        {
                            throw new ArgumentException("Public key is invalid.");
                        }
                        else
                        {
                            compressed = false;
                        }
                    }

                    ((SignatureScript)TxInList[inputIndex].SigScript).SetToP2PKH(sig, pubKey, compressed);
                    break;

                case PubkeyScriptType.P2SH:
                    throw new Exception("Not defined!");

                case PubkeyScriptType.P2WPKH:
                    byte[] expHash160_2 = ((PushDataOp)prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.OperationList[1]).data;
                    byte[] actualHash160_2 = addrHashFunc.ComputeHash(pubKey.ToByteArray(true)); // pubkey is always compressed
                    if (!((ReadOnlySpan<byte>)actualHash160_2).SequenceEqual(expHash160_2))
                    {
                        throw new ArgumentException("Public key is invalid.");
                    }
                    // P2WPKH SignatureScript is always empty
                    TxInList[inputIndex].SigScript = new SignatureScript();

                    // only initialize witness list once
                    if (WitnessList == null || WitnessList.Length == 0)
                    {
                        WitnessList = new WitnessScript[TxInList.Length];
                        for (int i = 0; i < WitnessList.Length; i++)
                        {
                            WitnessList[i] = new WitnessScript();
                        }
                    }

                    ((WitnessScript)WitnessList[inputIndex]).SetToP2WPKH(sig, pubKey);

                    break;

                default:
                    throw new Exception("Not defined!");
            }
        }

        /// <inheritdoc/>
        public void WriteScriptSig(Signature sig, PublicKey pubKey, RedeemScript redeem, ITransaction prevTx, int index)
        {
            if (prevTx.TxOutList[TxInList[index].Index].PubScript.GetPublicScriptType() != PubkeyScriptType.P2SH)
                throw new ArgumentException();

            RedeemScriptType scrType = redeem.GetRedeemScriptType();

            switch (scrType)
            {
                case RedeemScriptType.Empty:
                    break;
                case RedeemScriptType.Unknown:
                    break;
                case RedeemScriptType.MultiSig:
                    break;
                case RedeemScriptType.CheckLocktimeVerify:
                    ((SignatureScript)TxInList[index].SigScript).SetToCheckLocktimeVerify(sig, redeem);
                    break;
                case RedeemScriptType.P2SH_P2WPKH:
                    ((SignatureScript)TxInList[index].SigScript).SetToP2SH_P2WPKH(redeem);
                    ((WitnessScript)WitnessList[index]).SetToP2WPKH(sig, pubKey);
                    break;
                case RedeemScriptType.P2SH_P2WSH:
                    ((SignatureScript)TxInList[index].SigScript).SetToP2SH_P2WSH(redeem);
                    //((WitnessScript)WitnessList[index].WitnessItems).SetToP2WSH_MultiSig(sig, pubKey);
                    break;
                case RedeemScriptType.P2WSH:
                    break;
                default:
                    break;
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

            int start = stream.GetCurrentIndex();

            if (!stream.TryReadInt32(out _version))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!stream.TryPeekByte(out byte marker))
            {
                error = Err.EndOfStream;
                return false;
            }

            bool hasWitness = marker == 0;
            if (hasWitness)
            {
                _ = stream.TryReadByte(out byte _);
                if (!stream.TryReadByte(out marker))
                {
                    error = Err.EndOfStream;
                    return false;
                }
                if (marker != 1)
                {
                    error = "The SegWit marker has to be 0x0001";
                    return false;
                }
            }

            if (!CompactInt.TryRead(stream, out CompactInt tinCount, out error))
            {
                return false;
            }
            if (tinCount > int.MaxValue) // TODO: set a better value to check against.
            {
                error = "TxIn count is too big.";
                return false;
            }
            // TODO: Add a check for when (tinCount * eachTinSize) overflows size of our data

            TxInList = new TxIn[(int)tinCount];
            for (int i = 0; i < TxInList.Length; i++)
            {
                TxIn temp = new TxIn();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                TxInList[i] = temp;
            }

            if (!CompactInt.TryRead(stream, out CompactInt toutCount, out error))
            {
                return false;
            }
            if (toutCount > int.MaxValue) // TODO: set a better value to check against.
            {
                error = "TxOut count is too big.";
                return false;
            }
            // TODO: Add a check for when (toutCount * eachToutSize) overflows size of our data

            TxOutList = new TxOut[toutCount];
            for (int i = 0; i < TxOutList.Length; i++)
            {
                TxOut temp = new TxOut();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                TxOutList[i] = temp;
            }

            if (hasWitness)
            {
                WitnessList = new WitnessScript[TxInList.Length];
                for (int i = 0; i < WitnessList.Length; i++)
                {
                    WitnessScript temp = new WitnessScript();
                    if (!temp.TryDeserialize(stream, out error))
                    {
                        return false;
                    }
                    WitnessList[i] = temp;
                }
            }
            else
            {
                WitnessList = null;
            }

            if (!LockTime.TryRead(stream, out _lockTime, out error))
            {
                return false;
            }

            int end = stream.GetCurrentIndex();

            if (end - start > MaxTxSize)
            {
                error = "Transaction length is too big.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
