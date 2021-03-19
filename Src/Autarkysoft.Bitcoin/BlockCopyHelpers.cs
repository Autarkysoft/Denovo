// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Runtime.InteropServices;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// A 16 byte block used for copying memory in unsafe mode
    /// <para/>Usage: *(Block16*)des = *(Block16*)src;
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct Block16 { }
    /// <summary>
    /// A 32 byte block used for copying memory in unsafe mode
    /// <para/>Usage: *(Block32*)des = *(Block32*)src;
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public struct Block32 { }
    /// <summary>
    /// A 64 byte block used for copying memory in unsafe mode
    /// <para/>Usage: *(Block64*)des = *(Block64*)src;
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 64)]
    public struct Block64 { }
}
