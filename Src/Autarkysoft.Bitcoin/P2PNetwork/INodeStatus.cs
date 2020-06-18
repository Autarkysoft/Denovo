// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    public interface INodeStatus
    {
        bool SendCompact { get; set; }
        bool ShouldDisconnect { get; }
        DateTime LastSeen { get; }


        void UpdateTime();
        void AddSmallViolation();
        void AddMediumViolation();
        void AddBigViolation();
    }
}
