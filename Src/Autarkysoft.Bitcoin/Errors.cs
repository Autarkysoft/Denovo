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
    }
}
