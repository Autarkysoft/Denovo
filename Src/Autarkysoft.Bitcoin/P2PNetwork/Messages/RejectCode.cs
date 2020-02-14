// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    /// <summary>
    /// A signle byte used in <see cref="MessagePayloads.PayloadType.Reject"/> messages indicating reason of rejection.
    /// </summary>
    public enum RejectCode : byte
    {
        /// <summary>
        /// The message could not be decoded (deserialized)
        /// </summary>
        FailedToDecodeMessage = 0x01,
        /// <summary>
        /// Received block is invalid
        /// </summary>
        InvalidBlock = 0x10,
        /// <summary>
        /// Received transaction is invalid
        /// </summary>
        InvalidTx = 0x10,
        /// <summary>
        /// Received block has an unsupported version
        /// </summary>
        InvalidBlockVersion = 0x11,
        /// <summary>
        /// The protocol version is not supported or invalid
        /// </summary>
        InvalidProtocolVersion = 0x11,
        /// <summary>
        /// Double spend attempt
        /// </summary>
        DoubleSpendTx = 0x12,
        /// <summary>
        /// Duplicate version message received
        /// </summary>
        MultiVersionMessageReceived = 0x12,
        /// <summary>
        /// Non-standard transaction
        /// </summary>
        NonStandardTx = 0x40,
        /// <summary>
        /// One or more output amounts are below the 'dust' threshold
        /// </summary>
        Dust = 0x41,
        /// <summary>
        /// Transaction does not have enough fee/priority to be relayed or mined
        /// </summary>
        LowFee = 0x42,
        /// <summary>
        /// Inconsistent with a compiled-in checkpoint
        /// </summary>
        InvalidBlock_CheckPoint = 0x43 
    }
}
