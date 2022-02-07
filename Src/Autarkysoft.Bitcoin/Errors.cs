// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

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
        /// Script length bigger than <see cref="Constants.MaxScriptLength"/>
        /// </summary>
        ScriptOverflow,
        /// <summary>
        /// OP count in a script exceeded the allowed number
        /// </summary>
        OpCountOverflow,
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
        /// Negative <see cref="Target"/> value
        /// </summary>
        NegativeTarget,
        /// <summary>
        /// <see cref="Target"/> value overflow
        /// </summary>
        TargetOverflow,

        /// <summary>
        /// This is only used for testing
        /// </summary>
        ForTesting
    }
}
