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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Blockchain.Transactions
{
    /// <summary>
    /// Bitcoin transaction. Note that this class stores the transaction sizes and transaction hash and if the transaction
    /// changes it will not re-compute them. There are methods available to manually force re-calculation.
    /// <para/>Implements <see cref="ITransaction"/>.
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

        /// <summary>
        /// Initializes a new instance of <see cref="Transaction"/> using hexadecimal string representation of the transaction.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="hex">Transaction data in hexadecimal format</param>
        public Transaction(string hex)
        {
            byte[] data = Base16.Decode(hex);
            var stream = new FastStreamReader(data);
            if (!TryDeserialize(stream, out string error))
            {
                throw new ArgumentException(error);
            }
        }



        private const int MaxTxSize = 4_000_000;

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

        private int _totalSize;
        /// <summary>
        /// Total transaction size in bytes
        /// </summary>
        public int TotalSize
        {
            get
            {
                if (_totalSize == 0)
                {
                    var counter = new SizeCounter();
                    ComputeTotalSize(counter);
                    _totalSize = counter.Size;
                }

                return _totalSize;
            }
        }

        private int _baseSize;
        /// <summary>
        /// Transaction size without witness
        /// </summary>
        public int BaseSize
        {
            get
            {
                if (_baseSize == 0)
                {
                    var counter = new SizeCounter();
                    ComputeBaseSize(counter);
                    _baseSize = counter.Size;
                }

                return _baseSize;
            }
        }

        /// <summary>
        /// Transaction weight (ie. 3x <see cref="BaseSize"/> + <see cref="TotalSize"/>)
        /// </summary>
        public int Weight => (BaseSize * 3) + TotalSize;

        /// <summary>
        /// Virtual transaction size (ie. 1/4 * <see cref="Weight"/>)
        /// </summary>
        public int VirtualSize => Weight / 4;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ComputeTotalSize(SizeCounter counter) => AddSerializedSize(counter);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ComputeBaseSize(SizeCounter counter) => AddSerializedSizeWithoutWitness(counter);

        /// <summary>
        /// Computes and sets the size values (useful to re-compute size when transaction changes)
        /// </summary>
        public void ComputeSizes()
        {
            var counter = new SizeCounter();
            ComputeTotalSize(counter);
            _totalSize = counter.Size;
            counter.Reset();

            ComputeBaseSize(counter);
            _baseSize = counter.Size;
        }

        /// <summary>
        /// Computes and sets transaction hash and witness transaction hash
        /// (useful to re-compute hashes when transaction changes)
        /// </summary>
        public void ComputeTransactionHashes()
        {
            using Sha256 sha = new Sha256();
            // Tx hash is always stripping witness
            byte[] bytesToHash = ToByteArrayWithoutWitness();
            _txHash = sha.ComputeHashTwice(bytesToHash);

            bytesToHash = ToByteArray();
            _txWitHash = sha.ComputeHashTwice(bytesToHash);
        }

        private byte[] _txHash, _txWitHash;
        /// <inheritdoc/>
        public byte[] GetTransactionHash()
        {
            if (_txHash is null)
            {
                Debug.Assert(_txWitHash is null);
                ComputeTransactionHashes();
            }

            Debug.Assert(_txHash != null);
            Debug.Assert(_txWitHash != null);

            return _txHash;
        }

        /// <inheritdoc/>
        public string GetTransactionId() => Base16.EncodeReverse(GetTransactionHash());

        /// <inheritdoc/>
        public byte[] GetWitnessTransactionHash()
        {
            if (_txWitHash is null)
            {
                Debug.Assert(_txHash is null);
                ComputeTransactionHashes();
            }

            Debug.Assert(_txHash != null);
            Debug.Assert(_txWitHash != null);

            return _txWitHash;
        }

        /// <inheritdoc/>
        public string GetWitnessTransactionId() => Base16.EncodeReverse(GetWitnessTransactionHash());


        /// <inheritdoc/>
        public void AddSerializedSize(SizeCounter counter)
        {
            counter.Add(4 + 4); // Version + Locktime
            bool hasWitness = !(WitnessList is null) && WitnessList.Length != 0;
            if (hasWitness)
            {
                // SegWit marker
                counter.Add(2);
                foreach (var wit in WitnessList)
                {
                    wit.AddSerializedSize(counter);
                }
            }

            counter.AddCompactIntCount(TxInList.Length);
            counter.AddCompactIntCount(TxOutList.Length);

            foreach (var tin in TxInList)
            {
                tin.AddSerializedSize(counter);
            }

            foreach (var tout in TxOutList)
            {
                tout.AddSerializedSize(counter);
            }
        }

        /// <inheritdoc/>
        public void AddSerializedSizeWithoutWitness(SizeCounter counter)
        {
            counter.Add(4 + 4); // Version + Locktime

            counter.AddCompactIntCount(TxInList.Length);
            counter.AddCompactIntCount(TxOutList.Length);

            foreach (var tin in TxInList)
            {
                tin.AddSerializedSize(counter);
            }

            foreach (var tout in TxOutList)
            {
                tout.AddSerializedSize(counter);
            }
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
        public byte[] SerializeForSigning(byte[] spendScript, int inputIndex, SigHashType sht)
        {
            bool isSingle = sht.IsSingle();
            if (isSingle && TxOutList.Length <= inputIndex)
            {
                byte[] res = new byte[32];
                res[0] = 1;
                return res;
            }
            bool isNone = sht.IsNone();
            bool isAnyone = sht.IsAnyoneCanPay();
            // TODO: change this into Sha256 itself with stream methods inside + benchmark
            FastStream stream = new FastStream();

            stream.Write(Version);

            var tinCount = new CompactInt(isAnyone ? 1 : TxInList.Length);
            tinCount.WriteToStream(stream);

            if (isAnyone)
            {
                TxInList[inputIndex].SerializeForSigning(stream, spendScript, false);
            }
            else
            {
                byte[] empty = Array.Empty<byte>();
                // Sequence of other inputs must be set to 0 if the SigHashType is Single or None
                bool changeSeq = isSingle || isNone;
                for (int i = 0; i < TxInList.Length; i++)
                {
                    if (i != inputIndex)
                    {
                        TxInList[i].SerializeForSigning(stream, empty, changeSeq);
                    }
                    else
                    {
                        TxInList[i].SerializeForSigning(stream, spendScript, false);
                    }
                }
            }

            CompactInt toutCount = new CompactInt(isNone ? 0 : isSingle ? inputIndex + 1 : TxOutList.Length);
            toutCount.WriteToStream(stream);

            if (isSingle)
            {
                for (int i = 0; i < inputIndex; i++)
                {
                    TxOutList[i].SerializeSigHashSingle(stream);
                }
                TxOutList[inputIndex].Serialize(stream);
            }
            else if (!isNone)
            {
                foreach (var tout in TxOutList)
                {
                    tout.Serialize(stream);
                }
            }

            LockTime.WriteToStream(stream);

            stream.Write((int)sht);

            using Sha256 sha = new Sha256();
            return sha.ComputeHashTwice(stream.ToByteArray()); ;
        }

        /// <inheritdoc/>
        public byte[] SerializeForSigningSegWit(byte[] spendScript, int inputIndex, ulong amount, SigHashType sht)
        {
            using Sha256 sha = new Sha256();

            // TODO: this needs to be optimized by storing hashPrevouts,... to be reused
            bool isAnyone = sht.IsAnyoneCanPay();
            bool isNone = sht.IsNone();
            bool isSingle = sht.IsSingle();

            byte[] hashPrevouts;
            if (isAnyone)
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

                hashPrevouts = sha.ComputeHashTwice(prvOutStream.ToByteArray());
            }

            byte[] hashSequence;
            if (isSingle || isNone || isAnyone)
            {
                hashSequence = new byte[32];
            }
            else
            {
                // Sequences are 4 bytes each
                FastStream seqStream = new FastStream(TxInList.Length * 4);
                foreach (var tin in TxInList)
                {
                    seqStream.Write(tin.Sequence);
                }

                hashSequence = sha.ComputeHashTwice(seqStream.ToByteArray());
            }

            byte[] hashOutputs;
            if (!isSingle && !isNone)
            {
                // 33 is the approximate size of most TxOuts
                FastStream outputStream = new FastStream(TxOutList.Length * 33);
                foreach (var tout in TxOutList)
                {
                    tout.Serialize(outputStream);
                }

                hashOutputs = sha.ComputeHashTwice(outputStream.ToByteArray());
            }
            else if (isSingle && inputIndex < TxOutList.Length)
            {
                FastStream outputStream = new FastStream(33);
                TxOutList[inputIndex].Serialize(outputStream);

                hashOutputs = sha.ComputeHashTwice(outputStream.ToByteArray());
            }
            else
            {
                hashOutputs = new byte[32];
            }

            // 4(Version) + 32(hashPrevouts) + 32(hashSequence) + 36 (outpoint) + ??(scriptCode.Length) + 8 (amount) +
            // 4(Sequence) + 32(hashOutputs) + 4(LockTime) + 4(SigHashType)
            // Note that the following total length is an approximation since +1 is only true if CompactInt length is 1 byte
            // which is true for majority of cases (<253 byte) and if not FastStream has to resize its array.
            FastStream finalStream = new FastStream(156 + spendScript.Length + 1);
            finalStream.Write(Version);
            finalStream.Write(hashPrevouts);
            finalStream.Write(hashSequence);
            finalStream.Write(TxInList[inputIndex].TxHash);
            finalStream.Write(TxInList[inputIndex].Index);
            finalStream.WriteWithCompactIntLength(spendScript);
            finalStream.Write(amount);
            finalStream.Write(TxInList[inputIndex].Sequence);
            finalStream.Write(hashOutputs);
            LockTime.WriteToStream(finalStream);
            finalStream.Write((int)sht);

            byte[] hashPreimage = sha.ComputeHashTwice(finalStream.ToByteArray());
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
        public byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht,
                                     IRedeemScript redeem = null, IRedeemScript witRedeem = null)
        {
            CheckSht(sht);
            if (prvTx is null)
                throw new ArgumentNullException(nameof(prvTx), "Previous transaction (to spend) can not be null.");
            if (inputIndex < 0 || inputIndex >= TxInList.Length)
                throw new ArgumentException("Invalid input index.", nameof(inputIndex));
            if (!((ReadOnlySpan<byte>)TxInList[inputIndex].TxHash).SequenceEqual(prvTx.GetTransactionHash()))
                throw new ArgumentException("Wrong previous transaction or index.");
            var prvScr = prvTx.TxOutList[TxInList[inputIndex].Index].PubScript;
            if (!prvScr.TryEvaluate(out IOperation[] prevPubOps, out int opCount, out string error))
                throw new ArgumentException($"Previous transaction pubkey script can not be evaluated: {error}.");
            if (opCount > Constants.MaxScriptOpCount)
                throw new ArgumentOutOfRangeException(nameof(opCount), Err.OpCountOverflow);

            using Ripemd160Sha256 ripSha = new Ripemd160Sha256();

            PubkeyScriptType pubScrType = prvScr.GetPublicScriptType();
            if (pubScrType == PubkeyScriptType.P2PKH || pubScrType == PubkeyScriptType.P2PK ||
                pubScrType == PubkeyScriptType.CheckLocktimeVerify)
            {
                return SerializeForSigning(prvScr.Data, inputIndex, sht);
            }
            else if (pubScrType == PubkeyScriptType.P2SH)
            {
                if (redeem is null)
                    throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null for signing P2SH outputs.");
                if (!redeem.TryEvaluate(out IOperation[] rdmOps, out opCount, out error))
                    throw new ArgumentException($"Redeem script could not be evaluated: {error}.");
                if (opCount > Constants.MaxScriptOpCount)
                    throw new ArgumentOutOfRangeException(nameof(opCount), Err.OpCountOverflow);

                ReadOnlySpan<byte> expHash = ripSha.ComputeHash(redeem.Data);
                // Previous is OP_HASH160 PushData(20) OP_EQUAL
                ReadOnlySpan<byte> actHash = ((ReadOnlySpan<byte>)prvScr.Data).Slice(2, 20);
                if (!expHash.SequenceEqual(actHash))
                {
                    throw new ArgumentException("Wrong previous transaction or index.");
                }

                RedeemScriptType rdmScrType = redeem.GetRedeemScriptType();
                if (rdmScrType == RedeemScriptType.MultiSig || rdmScrType == RedeemScriptType.CheckLocktimeVerify)
                {
                    return SerializeForSigning(redeem.Data, inputIndex, sht);
                }
                else if (rdmScrType == RedeemScriptType.P2SH_P2WPKH)
                {
                    ScriptSerializer scrSer = new ScriptSerializer();
                    byte[] spendScr = scrSer.ConvertP2wpkh(rdmOps);
                    return SerializeForSigningSegWit(spendScr, inputIndex, prvTx.TxOutList[TxInList[inputIndex].Index].Amount, sht);
                }
                else if (rdmScrType == RedeemScriptType.P2SH_P2WSH)
                {
                    if (witRedeem is null)
                    {
                        throw new ArgumentNullException(nameof(witRedeem), "To spend a P2SH-P2WSH output, a witness redeem script is needed.");
                    }
                    if (!witRedeem.TryEvaluate(out IOperation[] witRdmOps, out opCount, out error))
                        throw new ArgumentException($"Redeem script could not be evaluated: {error}.");
                    if (opCount > Constants.MaxScriptOpCount)
                        throw new ArgumentOutOfRangeException(nameof(opCount), Err.OpCountOverflow);

                    RedeemScriptType witRdmType = witRedeem.GetRedeemScriptType();
                    if (witRdmType == RedeemScriptType.MultiSig || witRdmType == RedeemScriptType.CheckLocktimeVerify)
                    {
                        ScriptSerializer scrSer = new ScriptSerializer();
                        byte[] spendScr = scrSer.ConvertWitness(witRdmOps);
                        return SerializeForSigningSegWit(spendScr, inputIndex, prvTx.TxOutList[TxInList[inputIndex].Index].Amount, sht);
                    }
                    else
                    {
                        throw new ArgumentException("Witness redeem script type is not defined.");
                    }
                }
                else
                {
                    throw new ArgumentException("Redeem script type is not defined.");
                }
            }
            else if (pubScrType == PubkeyScriptType.P2WPKH)
            {
                ScriptSerializer scrSer = new ScriptSerializer();
                byte[] spendScr = scrSer.ConvertP2wpkh(prevPubOps);
                return SerializeForSigningSegWit(spendScr, inputIndex, prvTx.TxOutList[TxInList[inputIndex].Index].Amount, sht);
            }
            else if (pubScrType == PubkeyScriptType.P2WSH)
            {
                if (witRedeem is null)
                {
                    throw new ArgumentNullException(nameof(witRedeem), "To spend a P2WSH output, a witness redeem script is needed.");
                }
                if (!witRedeem.TryEvaluate(out IOperation[] witRdmOps, out opCount, out error))
                    throw new ArgumentException($"Redeem script could not be evaluated: {error}.");
                if (opCount > Constants.MaxScriptOpCount)
                    throw new ArgumentOutOfRangeException(nameof(opCount), Err.OpCountOverflow);

                RedeemScriptType witRdmType = witRedeem.GetRedeemScriptType();
                if (witRdmType == RedeemScriptType.MultiSig || witRdmType == RedeemScriptType.CheckLocktimeVerify)
                {
                    ScriptSerializer scrSer = new ScriptSerializer();
                    byte[] spendScr = scrSer.ConvertWitness(witRdmOps);
                    return SerializeForSigningSegWit(spendScr, inputIndex, prvTx.TxOutList[TxInList[inputIndex].Index].Amount, sht);
                }
                else
                {
                    throw new ArgumentException("Witness redeem script type is not defined.");
                }
            }
            else if (pubScrType == PubkeyScriptType.Empty)
            {
                throw new ArgumentException($"Previous transaction's PubkeyScript at index {inputIndex} needs to be set first.");
            }
            else if (pubScrType == PubkeyScriptType.RETURN)
            {
                throw new ArgumentException("Can not spend OP_RETURN outputs.");
            }
            else
            {
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

            using Ripemd160Sha256 ripSha = new Ripemd160Sha256();

            PubkeyScriptType scrType = prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.GetPublicScriptType();
            switch (scrType)
            {
                case PubkeyScriptType.P2PK:
                    TxInList[inputIndex].SigScript.SetToP2PK(sig);
                    break;

                case PubkeyScriptType.P2PKH:
                    ReadOnlySpan<byte> expHash160 =
                        ((ReadOnlySpan<byte>)prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.Data).Slice(3, 20);
                    ReadOnlySpan<byte> actualHash160 = ripSha.ComputeHash(pubKey.ToByteArray(true));
                    bool compressed = true;
                    if (!actualHash160.SequenceEqual(expHash160))
                    {
                        actualHash160 = ripSha.ComputeHash(pubKey.ToByteArray(false));
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

                    ReadOnlySpan<byte> expHash = ripSha.ComputeHash(redeem.Data);
                    ReadOnlySpan<byte> actHash =
                        ((ReadOnlySpan<byte>)prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.Data).Slice(2, 20);
                    if (!expHash.SequenceEqual(actHash))
                    {
                        throw new ArgumentException("Wrong previous transaction or index.");
                    }

                    RedeemScriptType redeemType = redeem.GetRedeemScriptType();
                    switch (redeemType)
                    {
                        case RedeemScriptType.MultiSig:
                            TxInList[inputIndex].SigScript.SetToMultiSig(sig, redeem, this, inputIndex);
                            break;
                        case RedeemScriptType.CheckLocktimeVerify:
                            if (!redeem.TryEvaluate(out IOperation[] rdmOps, out _, out string error))
                            {
                                throw new ArgumentException($"Invalid redeem script. Error: {error}");
                            }
                            var stack = new OpData() { Tx = this, IsBip65Enabled = true };
                            // Only run PushData(locktime) and CheckLockTimeVerify
                            for (int i = 0; i < 2; i++)
                            {
                                if (!rdmOps[i].Run(stack, out error))
                                {
                                    throw new ArgumentException($"Invalid redeem script. Error: {error}");
                                }
                            }

                            TxInList[inputIndex].SigScript.SetToCheckLocktimeVerify(sig, redeem);
                            break;
                        case RedeemScriptType.P2SH_P2WPKH:
                            TxInList[inputIndex].SigScript.SetToP2SH_P2WPKH(redeem);

                            expHash160 = ((ReadOnlySpan<byte>)redeem.Data).Slice(2, 20);
                            actualHash160 = ripSha.ComputeHash(pubKey.ToByteArray(true));
                            compressed = true;
                            if (!actualHash160.SequenceEqual(expHash160))
                            {
                                actualHash160 = ripSha.ComputeHash(pubKey.ToByteArray(false));
                                if (!actualHash160.SequenceEqual(expHash160))
                                {
                                    throw new ArgumentException("Public key is invalid.");
                                }
                                else
                                {
                                    compressed = false;
                                }
                            }

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
                        case RedeemScriptType.P2SH_P2WSH:
                            TxInList[inputIndex].SigScript.SetToP2SH_P2WSH(redeem);
                            break;
                        case RedeemScriptType.P2WSH:
                        case RedeemScriptType.Empty:
                        case RedeemScriptType.Unknown:
                        default:
                            throw new ArgumentException("Not defined.");
                    }
                    break;

                case PubkeyScriptType.RETURN:
                    throw new ArgumentException("Can not spend an OP_Return output.");

                case PubkeyScriptType.P2WPKH:
                    expHash160 = ((ReadOnlySpan<byte>)prevTx.TxOutList[TxInList[inputIndex].Index].PubScript.Data).Slice(2, 20);
                    actualHash160 = ripSha.ComputeHash(pubKey.ToByteArray(true));
                    compressed = true;
                    if (!actualHash160.SequenceEqual(expHash160))
                    {
                        actualHash160 = ripSha.ComputeHash(pubKey.ToByteArray(false));
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
            if (tinCount == 0)
            {
                error = "TxOut count cann ot be zero.";
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
            if (toutCount == 0)
            {
                error = "TxOut count cannot be zero.";
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
