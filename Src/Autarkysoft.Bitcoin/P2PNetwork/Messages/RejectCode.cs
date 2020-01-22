// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    public enum RejectCode : byte
    {
        FailedToDecodeMessage = 0x01,
        InvalidBlock = 0x10,
        InvalidTx = 0x10,
        InvalidBlockVersion = 0x11,
        InvalidProtocolVersion = 0x11,
        DoubleSpendTx = 0x12,
        MultiVersionMessageReceived = 0x12,
        NonStandardTx = 0x40,
        Dust = 0x41,
        LowFee = 0x42,
        InvalidBlock_CheckPoint = 0x43 
    }
}
