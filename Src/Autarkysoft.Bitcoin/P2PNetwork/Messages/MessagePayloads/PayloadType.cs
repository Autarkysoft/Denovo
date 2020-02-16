// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// Payload type enum
    /// </summary>
    public enum PayloadType
    {
        /// <summary>
        /// Contains connection information of one or more bitcoin nodes
        /// </summary>
        Addr,
        /// <summary>
        /// Alert messages (introduced in protocol version 311 and removed in 7013)
        /// </summary>
        Alert,
        /// <summary>
        /// Contains a single block
        /// </summary>
        Block,
        /// <summary>
        /// Contains an array of transactions from a certain block
        /// </summary>
        BlockTxn,
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
        /// Asks other node to remove previously set bool filter
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
        /// Confirms the connection is still alive
        /// </summary>
        Ping,
        /// <summary>
        /// Confirms the connection is still alive
        /// </summary>
        Pong,
        /// <summary>
        /// Indicates that the previous received message was rejected
        /// </summary>
        Reject,
        /// <summary>
        /// 
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
        /// Provides information about the node during handshake process
        /// </summary>
        Version
    }
}
