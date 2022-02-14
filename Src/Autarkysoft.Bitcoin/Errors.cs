// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Error messages returned from methods such as TryDeserialize
    /// </summary>
    public enum Errors
    {
        /// <summary>
        /// No error
        /// </summary>
        None,
        /// <summary>
        /// Invalid or undefined <see cref="NetworkType"/>
        /// </summary>
        InvalidNetwork,
        /// <summary>
        /// Byte array can not be null
        /// </summary>
        NullBytes,
        /// <summary>
        /// Byte array can not be null or empty
        /// </summary>
        NullOrEmptyBytes,
        /// <summary>
        /// Null stream
        /// </summary>
        NullStream,
        /// <summary>
        /// Reached end of stream
        /// </summary>
        EndOfStream,
        /// <summary>
        /// Data size is bigger than <see cref="int"/>
        /// </summary>
        DataTooBig,
        /// <summary>
        /// Could not read CompactInt from stream.
        /// </summary>
        InvalidCompactInt,

        /// <summary>
        /// Invalid DER encoding length.
        /// </summary>
        InvalidDerEncodingLength,
        /// <summary>
        /// Missing DER sequence tag
        /// </summary>
        MissingDerSeqTag,
        /// <summary>
        /// Missint first DER int tag
        /// </summary>
        MissingDerIntTag1,
        /// <summary>
        /// Missint second DER int tag
        /// </summary>
        MissingDerIntTag2,
        /// <summary>
        /// Invalid DER sequence length
        /// </summary>
        InvalidDerSeqLength,
        /// <summary>
        /// Invalid DER r length
        /// </summary>
        InvalidDerRLength,
        /// <summary>
        /// Invalid DER s length
        /// </summary>
        InvalidDerSLength,
        /// <summary>
        /// Invalid first DER int length
        /// </summary>
        InvalidDerIntLength1,
        /// <summary>
        /// Invalid second DER int length
        /// </summary>
        InvalidDerIntLength2,
        /// <summary>
        /// Invalid DER r format
        /// </summary>
        InvalidDerRFormat,
        /// <summary>
        /// Invalid DER s format
        /// </summary>
        InvalidDerSFormat,
        /// <summary>
        /// SigHashType byte can not be zero
        /// </summary>
        SigHashTypeZero,
        /// <summary>
        /// Invalid SigHashType
        /// </summary>
        InvalidSigHashType,
        /// <summary>
        /// No corresponding output for the input with SigHash_Signle
        /// </summary>
        OutOfRangeSigHashSingle,
        /// <summary>
        /// Schnorr signature length must be 64 or 65 bytes
        /// </summary>
        InvalidSchnorrSigLength,

        /// <summary>
        /// Script length bigger than <see cref="Constants.MaxScriptLength"/>
        /// </summary>
        ScriptOverflow,
        /// <summary>
        /// OP count in a script exceeded the allowed number
        /// </summary>
        OpCountOverflow,
        /// <summary>
        /// Stack item count exceeded the allowed number
        /// </summary>
        StackItemCountOverflow,
        /// <summary>
        /// Item to be pushed to the stack cannot be bigger than <see cref="Constants.MaxScriptItemLength"/>
        /// </summary>
        StackPushSizeOverflow,
        /// <summary>
        /// There isn't enough items left on the stack
        /// </summary>
        NotEnoughStackItems,
        /// <summary>
        /// There isn't enough items left on the alt-stack
        /// </summary>
        NotEnoughAltStackItems,
        /// <summary>
        /// Invalid number format for a stack item
        /// </summary>
        InvalidStackNumberFormat,
        /// <summary>
        /// Invalid (negative) number on the stack
        /// </summary>
        NegativeStackInteger,
        /// <summary>
        /// Numbers on the stack are not equal
        /// </summary>
        UnequalStackNumbers,
        /// <summary>
        /// Invalid OP code
        /// </summary>
        InvalidOP,
        /// <summary>
        /// Disabled OP code
        /// </summary>
        DisabledOP,
        /// <summary>
        /// Undefined OP code
        /// </summary>
        UndefinedOp,
        /// <summary>
        /// Missing OP_EndIf
        /// </summary>
        MissingOpEndIf,
        /// <summary>
        /// OP_ELSE without OP_(NOT)IF
        /// </summary>
        OpElseNoOpIf,
        /// <summary>
        /// OP_EndIf without OP_(NOT)IF
        /// </summary>
        OpEndIfNoOpIf,
        /// <summary>
        /// OP_CheckMultiSig should not be used in Taproot scripts
        /// </summary>
        OpCheckMultiSigTaproot,
        /// <summary>
        /// OP_CheckMultiSigVerify should not be used in Taproot scripts
        /// </summary>
        OpCheckMultiSigVerifyTaproot,
        /// <summary>
        /// OP_CheckSigAdd should not be used in legacy and SegWit v0 scripts
        /// </summary>
        OpCheckSigAddPreTaproot,
        /// <summary>
        /// A non runable OP was executed
        /// </summary>
        NotRunableOp,
        /// <summary>
        /// Signature verification failed
        /// </summary>
        FailedSignatureVerification,
        /// <summary>
        /// Invalid number of public keys in multi-sig script
        /// </summary>
        InvalidMultiSigPubkeyCount,
        /// <summary>
        /// Invalid number of signatures in multi-sig script
        /// </summary>
        InvalidMultiSigSignatureCount,
        /// <summary>
        /// Invalid multi-sig dummy item (it has to be OP_0)
        /// </summary>
        InvalidMultiSigDummy,
        /// <summary>
        /// Too much signature validation relative to witness weight
        /// </summary>
        TaprootSigOpOverflow,
        /// <summary>
        /// Invalid public key
        /// </summary>
        InvalidPublicKey,
        /// <summary>
        /// Boolean item popped by conditional OPs must be strict
        /// </summary>
        InvalidConditionalBool,
        /// <summary>
        /// Top 2 stack items are not equal
        /// </summary>
        UnequalStackItems,
        /// <summary>
        /// Top stack item is false
        /// </summary>
        FalseTopStackItem,

        /// <summary>
        /// Stream doesn't start with 0x6a
        /// </summary>
        WrongOpReturnByte,
        /// <summary>
        /// OP_RETURN stream must contain at least one byte
        /// </summary>
        ShortOpReturn,

        /// <summary>
        /// First byte 253 needs to read at least 2 bytes
        /// </summary>
        ShortCompactInt2,
        /// <summary>
        /// Values smaller than 253 should use one byte
        /// </summary>
        SmallCompactInt2,
        /// <summary>
        /// First byte 254 needs to read at least 4 bytes
        /// </summary>
        ShortCompactInt4,
        /// <summary>
        /// Values smaller than 2 bytes should use [253, ushort] format
        /// </summary>
        SmallCompactInt4,
        /// <summary>
        /// First byte 255 needs to read at least 8 bytes
        /// </summary>
        ShortCompactInt8,
        /// <summary>
        /// Values smaller than 253 should use [254, uint]
        /// </summary>
        SmallCompactInt8,

        /// <summary>
        /// OP_PushData1 needs to read at least 1 byte
        /// </summary>
        ShortOPPushData1,
        /// <summary>
        /// OP_PushData1 value should be bigger than (OP_PushData1 - 1)
        /// </summary>
        SmallOPPushData1,
        /// <summary>
        /// OP_PushData2 needs to read at least 2 bytes
        /// </summary>
        ShortOPPushData2,
        /// <summary>
        /// OP_PushData2 value should be bigger than byte.MaxValue
        /// </summary>
        SmallOPPushData2,
        /// <summary>
        /// OP_PushData4 needs to read at least 4 bytes
        /// </summary>
        ShortOPPushData4,
        /// <summary>
        /// OP_PushData4 value should be bigger than ushort.MaxValue
        /// </summary>
        SmallOPPushData4,
        /// <summary>
        /// Unknown OP_Push value (in <see cref="StackInt"/>)
        /// </summary>
        UnknownOpPush,

        /// <summary>
        /// When spending <see cref="OP.CheckLocktimeVerify"/> sequence of all inputs should be less than maximum
        /// </summary>
        MaxTxSequence,
        /// <summary>
        /// Lock time from stack and transaction are not of the same type
        /// </summary>
        UnequalLocktimeType,
        /// <summary>
        /// Transaction is unspendable since the locktime is in the future
        /// </summary>
        UnspendableLocktime,
        /// <summary>
        /// Locktime can not be negative
        /// </summary>
        NegativeLocktime,
        /// <summary>
        /// Sequence popped from stack can not be negative
        /// </summary>
        NegativeSequence,
        /// <summary>
        /// Highest bit in input's sequence should be 0
        /// </summary>
        InvalidSequenceHighBit,
        /// <summary>
        /// Sequence from stack and transaction are not of the same type
        /// </summary>
        UnequalSequenceType,
        /// <summary>
        /// Transaction version when spending <see cref="OP.CheckSequenceVerify"/> should be bigger than 1
        /// </summary>
        InvalidTxVersion,
        /// <summary>
        /// Negative <see cref="Target"/> value
        /// </summary>
        NegativeTarget,
        /// <summary>
        /// <see cref="Target"/> value overflow
        /// </summary>
        TargetOverflow,

        /// <summary>
        /// Number of items in the array is bigger than <see cref="int"/>.
        /// </summary>
        ItemCountOverflow,
        /// <summary>
        /// Number of transactions in the block is bigger than <see cref="int"/>.
        /// </summary>
        TxCountOverflow,
        /// <summary>
        /// The SegWit marker has to be 0x0001
        /// </summary>
        WrongSegWitMarker,
        /// <summary>
        /// Number of transaction inputs is bigger than <see cref="int"/>.
        /// </summary>
        TxInCountOverflow,
        /// <summary>
        /// Number of transaction inputs can not be zero.
        /// </summary>
        TxInCountZero,
        /// <summary>
        /// Amount is bigger than total bitcoin supply.
        /// </summary>
        TxAmountOverflow,
        /// <summary>
        /// Number of transaction outputs is bigger than <see cref="int"/>.
        /// </summary>
        TxOutCountOverflow,
        /// <summary>
        /// Number of transaction outputs can not be zero.
        /// </summary>
        TxOutCountZero,
        /// <summary>
        /// Number of transaction witnesses is bigger than <see cref="int"/>.
        /// </summary>
        WitnessCountOverflow,
        /// <summary>
        /// Transaction total size is bigger than <see cref="Constants.MaxBlockWeight"/>.
        /// </summary>
        TxSizeOverflow,

        /// <summary>
        ///  Message payload size is bigger than allowed size (<see cref="Constants.MaxPayloadSize"/>).
        /// </summary>
        MessagePayloadOverflow,
        /// <summary>
        /// The received message is from another network.
        /// </summary>
        InvalidMessageNetwork,
        /// <summary>
        /// The received message has an invalid checksum.
        /// </summary>
        InvalidMessageChecksum,
        /// <summary>
        /// AddressCount can not be bigger than <see cref="Constants.MaxAddrCount"/>."
        /// </summary>
        MsgAddrCountOverflow,
        /// <summary>
        /// Number of items in BlockTxn message is bigger than <see cref="int"/>.
        /// </summary>
        MsgTxCountOverflow,
        /// <summary>
        /// Number of short IDs in CmpctBlock message is bigger than <see cref="int"/>.
        /// </summary>
        MsgShortIdCountOverflow,
        /// <summary>
        /// Fee rate filter is too big.
        /// </summary>
        MsgFeeRateFilterOverflow,
        /// <summary>
        /// Number of elements in FilterAdd message is bigger than 
        /// <see cref="P2PNetwork.Messages.MessagePayloads.FilterAddPayload.MaxElementLength"/>
        /// </summary>
        MsgElementLenOverflow,
        /// <summary>
        /// Filter length in FilterLoad message is bigger than
        /// <see cref="P2PNetwork.Messages.MessagePayloads.FilterLoadPayload.MaxFilterLength"/>
        /// </summary>
        MsgFilterLenOverflow,
        /// <summary>
        /// Number of hashes in FilterLoad message is bigger than
        /// <see cref="P2PNetwork.Messages.MessagePayloads.FilterLoadPayload.MaxHashFuncs"/>
        /// </summary>
        MsgFilterHashOverflow,
        /// <summary>
        /// GetBlocks payload version is invalid
        /// </summary>
        InvalidBlocksPayloadVersion,
        /// <summary>
        /// Number of hashes in GetBlocks payload is bigger than
        /// <see cref="P2PNetwork.Messages.MessagePayloads.GetBlocksPayload.MaximumHashes"/>.
        /// </summary>
        MsgBlocksHashCountOverflow,
        /// <summary>
        /// Number of txns in GetBlockTxn message is bigger than <see cref="int"/>.
        /// </summary>
        MsgBlockTxnCountOverflow,
        /// <summary>
        /// Number of headers in a Headers message is bigger than
        /// <see cref="P2PNetwork.Messages.MessagePayloads.HeadersPayload.MaxCount"/>.
        /// </summary>
        MsgHeaderCountOverflow,
        /// <summary>
        /// Number of inventories in an Inv message is bigger than
        /// <see cref="P2PNetwork.Messages.MessagePayloads.InvPayload.MaxInvCount"/>.
        /// </summary>
        MsgInvCountOverflow,
        /// <summary>
        /// Number of hashes in MerkleBlock message is bigger than <see cref="int"/>.
        /// </summary>
        MsgMerkleBlockHashCountOverflow,
        /// <summary>
        /// Length of flag in MerkleBlock message is bigger than <see cref="int"/>.
        /// </summary>
        MsgMerkleBlockFlagLenOverflow,
        /// <summary>
        /// Announce bool in SendCmpct messasge should be 0 or 1.
        /// </summary>
        MsgSendCmpctInvalidAnn,
        /// <summary>
        /// Size of User-Agent in bytes is bigger than
        /// <see cref="P2PNetwork.Messages.MessagePayloads.VersionPayload.UserAgentMaxSize"/>.
        /// </summary>
        MsgUserAgentOverflow,
        /// <summary>
        /// Relay byte in Version message can only be 0 or 1.
        /// </summary>
        MsgVersionInvalidRelay,


        /// <summary>
        /// This is only used for testing
        /// </summary>
        ForTesting
    }
}
