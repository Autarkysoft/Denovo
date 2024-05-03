﻿// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Denovo.Models
{
    public enum ClientType
    {
        Full,
        FullPruned,
        Spv,
        SpvElectrum
    }

    public enum MessageBoxResult
    {
        Ok,
        Cancel,
        Yes,
        No
    }

    public enum MessageBoxType
    {
        Ok,
        OkCancel,
        YesNo,
    }

    public enum PeerDiscoveryOption
    {
        DNS,
        CustomIP
    }
}
