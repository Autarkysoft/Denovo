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
        /// <param name="ver">Version</param>
        /// <param name="txIns">List of inputs</param>
        /// <param name="txOuts">List of outputs</param>
        /// <param name="lt">LockTime</param>
        /// <param name="witnesses">[Default value = null] List of witnesses (default is null).</param>
        public Transaction(int ver, TxIn[] txIns, TxOut[] txOuts, LockTime lt, Witness[] witnesses = null)
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
        public int Version
        {
            get => _version;
            set => _version = value;
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
        public IWitness[] WitnessList { get; set; }

        private LockTime _lockTime;
        /// <inheritdoc/>
        public LockTime LockTime
        {
            get => _lockTime;
            set => _lockTime = value;
        }

        /// <inheritdoc/>
        public bool IsVerified { get; set; } = false;

        /// <inheritdoc/>
        public int SigOpCount { get; set; }

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
        public byte[] GetWitnessTransactionHash()
        {
            // TODO: same as above (verify if signed)
            byte[] hashRes = hashFunc.ComputeHash(ToByteArray());
            return hashRes;
        }

        /// <inheritdoc/>
        public string GetWitnessTransactionId()
        {
            // TODO: same as above (verify if signed)
            byte[] hashRes = GetWitnessTransactionHash();
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


        /// <inheritdoc/>
        public byte[] SerializeForSigning(IOperation[] ops, int inputIndex, SigHashType sht, ReadOnlySpan<byte> sig)
        {
            // TODO: change this into Sha256 itself with stream methods inside + benchmark
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
                TxInList[inputIndex].SerializeForSigning(stream, ops, sig, false);
            }
            else
            {
                IOperation[] empty = new IOperation[0];
                // Sequence of other inputs must be set to 0 if the SigHashType is Single or None
                bool changeSeq = sht != SigHashType.All;
                for (int i = 0; i < TxInList.Length; i++)
                {
                    if (i != inputIndex)
                    {
                        TxInList[i].SerializeForSigning(stream, empty, sig, changeSeq);
                    }
                    else
                    {
                        TxInList[i].SerializeForSigning(stream, ops, sig, false);
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

        /// <inheritdoc/>
        public byte[] SerializeForSigningSegWit(byte[] prevOutScript, int inputIndex, ulong amount, SigHashType sht)
        {
            // TODO: like above prevOutScript needs to be of type IOperation[] instead of byte[] and we need to handle
            //       its conversion to bytep[] here.
            // TODO: this needs to be optimized by storing hashPrevouts,... to be reused
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

            // 4(Version) + 32(hashPrevouts) + 32(hashSequence) + 36 (outpoint) + ??(scriptCode.Length) + 8 (amount) +
            // 4(Sequence) + 32(hashOutputs) + 4(LockTime) + 4(SigHashType)
            FastStream finalStream = new FastStream(156 + prevOutScript.Length);
            finalStream.Write(Version);
            finalStream.Write(hashPrevouts);
            finalStream.Write(hashSequence);
            finalStream.Write(TxInList[inputIndex].TxHash);
            finalStream.Write(TxInList[inputIndex].Index);
            finalStream.Write(prevOutScript);
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
        public byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht, IRedeemScript redeem = null)
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
            if (!prvTx.TxOutList[TxInList[inputIndex].Index].PubScript.TryEvaluate(out IOperation[] prevOps, out int opCount, out string error))
                throw new ArgumentException($"Previous transaction pubkey script can not be evaluated: {error}.");
            if (opCount > Constants.MaxScriptOpCount)
                throw new ArgumentOutOfRangeException(nameof(opCount), "Number of OPs in this script exceeds the allowed number.");


            PubkeyScriptType scrType = prvTx.TxOutList[TxInList[inputIndex].Index].PubScript.GetPublicScriptType();
            if (scrType == PubkeyScriptType.P2PKH || scrType == PubkeyScriptType.P2PK)
            {
                return SerializeForSigning(prevOps, inputIndex, sht, null);
            }
            else if (scrType == PubkeyScriptType.P2SH)
            {
                if (redeem is null)
                    throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null for signing P2SH outputs.");
                if (!redeem.TryEvaluate(out IOperation[] rdmOps, out opCount, out error))
                    throw new ArgumentException($"Redeem script could not be evaluated: {error}.");
                if (opCount > Constants.MaxScriptOpCount)
                    throw new ArgumentOutOfRangeException(nameof(opCount), "Number of OPs in this script exceeds the allowed number.");

                ReadOnlySpan<byte> expHash = addrHashFunc.ComputeHash(redeem.Data);
                ReadOnlySpan<byte> actHash = ((ReadOnlySpan<byte>)prvTx.TxOutList[TxInList[inputIndex].Index].PubScript.Data).Slice(2, 20);
                if (!expHash.SequenceEqual(actHash))
                    throw new ArgumentException("Wrong previous transaction or index.");

                return SerializeForSigning(rdmOps, inputIndex, sht, null);
            }
            else if (scrType == PubkeyScriptType.P2WPKH)
            {
                // the prevOutScript is a P2WPKH (0014<hash160>) which should be turned into 1976a914<hash160>88ac and placed here
                byte[] temp = new byte[26];
                temp[0] = 25;
                temp[1] = (byte)OP.DUP;
                temp[2] = (byte)OP.HASH160;
                // Copy both push_size+hash from prev script
                Buffer.BlockCopy(prvTx.TxOutList[TxInList[inputIndex].Index].PubScript.Data, 1, temp, 3, 21);
                temp[^2] = (byte)OP.EqualVerify;
                temp[^1] = (byte)OP.CheckSig;
                return SerializeForSigningSegWit(temp, inputIndex, prvTx.TxOutList[TxInList[inputIndex].Index].Amount, sht);
            }
            else if (scrType == PubkeyScriptType.P2WSH)
            {
                throw new NotImplementedException(); // TODO: implement this!
            }
            else if (scrType == PubkeyScriptType.CheckLocktimeVerify)
            {
                throw new NotImplementedException(); // TODO: implement this!
            }

            switch (scrType)
            {
                case PubkeyScriptType.Empty:
                    throw new ArgumentException($"Previous transaction's PubkeyScript at index {inputIndex} needs to be set first.");
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
        public void WriteScriptSig(Signature sig, PublicKey pubKey, ITransaction prevTx, int inputIndex, IRedeemScript redeem)
        {
            if (sig is null)
                throw new ArgumentNullException(nameof(sig), "Signature can not be null.");
            if (pubKey is null)
                throw new ArgumentNullException(nameof(pubKey), "Public key can not be null.");
            if (prevTx is null)
                throw new ArgumentNullException(nameof(prevTx), "Previous transaction can not be null.");
            if (inputIndex < 0 || inputIndex >= TxInList.Length)
                throw new ArgumentOutOfRangeException(nameof(inputIndex), "Index can not be negative or bigger than input count.");

            PubkeyScriptType scrType = prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.GetPublicScriptType();
            switch (scrType)
            {
                case PubkeyScriptType.P2PK:
                    TxInList[inputIndex].SigScript.SetToP2PK(sig);
                    break;

                case PubkeyScriptType.P2PKH:
                    ReadOnlySpan<byte> expHash160 =
                        ((ReadOnlySpan<byte>)prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.Data).Slice(3, 20);
                    ReadOnlySpan<byte> actualHash160 = addrHashFunc.ComputeHash(pubKey.ToByteArray(true));
                    bool compressed = true;
                    if (!actualHash160.SequenceEqual(expHash160))
                    {
                        actualHash160 = addrHashFunc.ComputeHash(pubKey.ToByteArray(false));
                        if (!actualHash160.SequenceEqual(expHash160))
                        {
                            throw new ArgumentException("Public key is invalid.");
                        }
                        else
                        {
                            compressed = false;
                        }
                    }

                    TxInList[inputIndex].SigScript.SetToP2PKH(sig, pubKey, compressed);
                    break;

                case PubkeyScriptType.P2SH:
                    if (redeem is null)
                    {
                        throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null for signing P2SH outputs.");
                    }

                    ReadOnlySpan<byte> expHash = addrHashFunc.ComputeHash(redeem.Data);
                    ReadOnlySpan<byte> actHash =
                        ((ReadOnlySpan<byte>)prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.Data).Slice(2, 20);
                    if (!expHash.SequenceEqual(actHash))
                    {
                        throw new ArgumentException("Wrong previous transaction or index.");
                    }

                    TxInList[inputIndex].SigScript.SetToMultiSig(sig, pubKey, redeem, this, prevTx, inputIndex);
                    break;

                case PubkeyScriptType.CheckLocktimeVerify:
                    throw new NotImplementedException();

                case PubkeyScriptType.RETURN:
                    throw new ArgumentException("Can not spend an OP_Return output.");

                case PubkeyScriptType.P2WPKH:
                    expHash160 = ((ReadOnlySpan<byte>)prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.Data).Slice(2, 20);
                    actualHash160 = addrHashFunc.ComputeHash(pubKey.ToByteArray(true));
                    compressed = true;
                    if (!actualHash160.SequenceEqual(expHash160))
                    {
                        actualHash160 = addrHashFunc.ComputeHash(pubKey.ToByteArray(false));
                        if (!actualHash160.SequenceEqual(expHash160))
                        {
                            throw new ArgumentException("Public key is invalid.");
                        }
                        else
                        {
                            compressed = false;
                        }
                    }

                    // P2WPKH SignatureScript is always empty
                    TxInList[inputIndex].SigScript.SetToEmpty();

                    // only initialize witness list once
                    if (WitnessList == null || WitnessList.Length == 0)
                    {
                        WitnessList = new Witness[TxInList.Length];
                        for (int i = 0; i < WitnessList.Length; i++)
                        {
                            WitnessList[i] = new Witness();
                        }
                    }

                    WitnessList[inputIndex].SetToP2WPKH(sig, pubKey, compressed);
                    break;

                case PubkeyScriptType.P2WSH:
                    throw new NotImplementedException();

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
                case RedeemScriptType.CheckLocktimeVerify:
                    TxInList[index].SigScript.SetToCheckLocktimeVerify(sig, redeem);
                    break;
                case RedeemScriptType.P2SH_P2WPKH:
                    TxInList[index].SigScript.SetToP2SH_P2WPKH(redeem);
                    WitnessList[index].SetToP2WPKH(sig, pubKey);
                    break;
                case RedeemScriptType.P2SH_P2WSH:
                    TxInList[index].SigScript.SetToP2SH_P2WSH(redeem);
                    //((WitnessScript)WitnessList[index].WitnessItems).SetToP2WSH_MultiSig(sig, pubKey);
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
                WitnessList = new Witness[TxInList.Length];
                for (int i = 0; i < WitnessList.Length; i++)
                {
                    Witness temp = new Witness();
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
