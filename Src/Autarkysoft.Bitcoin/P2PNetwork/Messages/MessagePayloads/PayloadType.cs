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
        /// 
        /// </summary>
        FilterClear,
        /// <summary>
        /// 
        /// </summary>
        FilterLoad,
        /// <summary>
        /// 
        /// </summary>
        GetAddr,
        /// <summary>
        /// 
        /// </summary>
        GetBlocks,
        /// <summary>
        /// 
        /// </summary>
        GetBlockTxn,
        /// <summary>
        /// 
        /// </summary>
        GetData,
        /// <summary>
        /// 
        /// </summary>
        GetHeaders,
        /// <summary>
        /// 
        /// </summary>
        Headers,
        /// <summary>
        /// 
        /// </summary>
        Inv,
        /// <summary>
        /// 
        /// </summary>
        MemPool,
        /// <summary>
        /// 
        /// </summary>
        MerkleBlock,
        /// <summary>
        /// 
        /// </summary>
        NotFound,
        /// <summary>
        /// 
        /// </summary>
        Ping,
        /// <summary>
        /// 
        /// </summary>
        Pong,
        /// <summary>
        /// 
        /// </summary>
        Reject,
        /// <summary>
        /// 
        /// </summary>
        SendCmpct,
        /// <summary>
        /// 
        /// </summary>
        SendHeaders,
        /// <summary>
        /// 
        /// </summary>
        Tx,
        /// <summary>
        /// 
        /// </summary>
        Verack,
        /// <summary>
        /// 
        /// </summary>
        Version
    }
}
