// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// Payload type enum
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin/bitcoin/blob/f79a4a895279ba4efa43494270633f94f7d18342/src/protocol.h#L58-L263
    /// </remarks>
    public enum PayloadType
    {
        /// <summary>
        /// Contains connection information of one or more bitcoin nodes
        /// </summary>
        Addr,
        /// <summary>
        /// Version 2 of <see cref="Addr"/> message payloads as defined by BIP-155
        /// </summary>
        AddrV2,
        /// <summary>
        /// Alert messages (introduced in protocol version 311 and removed in 7013)
        /// <para/>Note: This message type is not used anymore.
        /// </summary>
        Alert,
        /// <summary>
        /// Contains a single block
        /// </summary>
        Block,
        /// <summary>
        /// Contains an array of transactions from a certain block and is sent in response to <see cref="GetBlockTxn"/> (BIP-152)
        /// </summary>
        BlockTxn,
        /// <summary>
        /// 
        /// </summary>
        CFCheckpt,
        /// <summary>
        /// The response to <see cref="GetCFHeaders"/> as defined by BIP-157 and BIP-158
        /// </summary>
        CFHeaders,
        /// <summary>
        /// The response to <see cref="GetCFilters"/> as defined by BIP-157 and BIP-158
        /// </summary>
        CFilter,
        /// <summary>
        /// Contains a compact block
        /// </summary>
        CmpctBlock,
        /// <summary>
        /// Contains a fee rate filter in Satoshi/kB
        /// </summary>
        FeeFilter,
        /// <summary>
        /// Adds a new element to the previously set bloom filter
        /// </summary>
        FilterAdd,
        /// <summary>
        /// Asks other node to remove previously set bloom filter
        /// </summary>
        FilterClear,
        /// <summary>
        /// Asks other node to set a bloom filter
        /// </summary>
        FilterLoad,
        /// <summary>
        /// Asks other node to send a list of known peers in a <see cref="Addr"/> message
        /// </summary>
        GetAddr,
        /// <summary>
        /// Asks other node to send a list of block header hashes
        /// </summary>
        GetBlocks,
        /// <summary>
        /// 
        /// </summary>
        GetBlockTxn,
        /// <summary>
        /// BIP-157 and BIP-158
        /// </summary>
        GetCFHeaders,
        /// <summary>
        /// 
        /// </summary>
        GetCFCheckpt,
        /// <summary>
        /// BIP-157 and BIP-158
        /// </summary>
        GetCFilters,
        /// <summary>
        /// Asks for one or more data (<see cref="Inventory"/>) objects
        /// </summary>
        GetData,
        /// <summary>
        /// Asks other node to send block headers starting from a particular block
        /// </summary>
        GetHeaders,
        /// <summary>
        /// Sends a list of block headers
        /// </summary>
        Headers,
        /// <summary>
        /// Sends one or more <see cref="Inventory"/> objects
        /// </summary>
        Inv,
        /// <summary>
        /// Asks other node to send all transaction hashes in its memory pool (the response can be different
        /// based on previously set bloom filter)
        /// </summary>
        MemPool,
        /// <summary>
        /// 
        /// </summary>
        MerkleBlock,
        /// <summary>
        /// Tells the other node that its requested "data" was not found.
        /// </summary>
        NotFound,
        /// <summary>
        /// Sends a message containing a random nonce to confirm the connection is still alive and measure latency.
        /// </summary>
        Ping,
        /// <summary>
        /// Replies to the received <see cref="Ping"/> message with the same received nonnce in confirmation that
        /// the connection is still alive.
        /// </summary>
        Pong,
        /// <summary>
        /// Indicates that the previous received message was rejected
        /// <para/>Note: This message type is not used anymore.
        /// </summary>
        Reject,
        /// <summary>
        /// Signals support for <see cref="AddrV2"/> messages as defined by BIP-155
        /// </summary>
        SendAddrV2,
        /// <summary>
        /// Requests blocks to be sent in compact form
        /// </summary>
        SendCmpct,
        /// <summary>
        /// Asks other node to only send block headers when sending new blocks
        /// </summary>
        SendHeaders,
        /// <summary>
        /// Sends a single transaction
        /// </summary>
        Tx,
        /// <summary>
        /// The one time reply sent as acknowledgement of receiving a <see cref="Version"/> message
        /// </summary>
        Verack,
        /// <summary>
        /// Provides information about the node during handshake process and is sent only once.
        /// </summary>
        Version,
        /// <summary>
        /// Indicates that a node prefers to relay transactions via wtxid, rather than txid.
        /// </summary>
        WTxIdRelay
    }
}
