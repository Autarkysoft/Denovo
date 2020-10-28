// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    /// <summary>
    /// 4 byte unsigned integer indicating type of the inventory
    /// </summary>
    public enum InventoryType : uint
    {
        /// <summary>
        /// (UNDEFINED) Data can be ignored
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// (MSG_TX) Inventory content is a 32 byte transaction hash
        /// </summary>
        Tx = 1,
        /// <summary>
        /// (MSG_BLOCK) Inventory content is a 32 byte block hash
        /// </summary>
        Block = 2,
        /// <summary>
        /// (MSG_FILTERED_BLOCK) 
        /// </summary>
        FilteredBlock = 3,
        /// <summary>
        /// (MSG_CMPCT_BLOCK) 
        /// </summary>
        CompactBlock = 4,
        /// <summary>
        /// (MSG_WTX) Inventory content is a 32 byte transaction witness hash
        /// </summary>
        WTx = 5,
        /// <summary>
        /// (MSG_WITNESS_BLOCK)
        /// </summary>
        WitnessBlock = Block | (1 << 30),
        /// <summary>
        /// (MSG_WITNESS_TX)
        /// </summary>
        WitnessTx = Tx | (1 << 30)
    }
}
