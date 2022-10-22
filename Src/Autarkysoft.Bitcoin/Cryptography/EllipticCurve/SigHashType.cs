﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// A single byte added to bitcoin transaction signatures to indicate which parts of the transaction were signed. 
    /// This way the unsigned parts can be modified.
    /// <para/> Note: this is not a c# <see cref="FlagsAttribute"/> since values aren't powers of 2 and also 
    /// different types can not be combined with each other. 
    /// The only combination is <see cref="AnyoneCanPay"/> with any of the other 3 types.
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin/bitcoin/blob/907d636e5e76bd884a4ca1a1ebd8db68a3e64705/src/script/interpreter.h#L23-L34
    /// </remarks>
    [Flags]
    public enum SigHashType
    {
        /// <summary>
        /// Same as <see cref="All"/> but is used when SigHash byte is missing from signatures in Taproot
        /// </summary>
        Default = 0,

        /// <summary>
        /// Everything about the transaction is signed
        /// </summary>
        All = 0b0000_0001, // 0x01

        /// <summary>
        /// Sign all inputs but none of the outputs (outputs can be anything)
        /// </summary>
        None = 0b0000_0010, // 0x02

        /// <summary>
        /// Sign all inputs (without sequences) and only output at the same index as the input being signed 
        /// (if not present, sign value `1`)
        /// </summary>
        Single = 0b0000_0011, // 0x03

        /// <summary>
        /// Only the current input is signed. Added to other <see cref="SigHashType"/>s like a flag.
        /// </summary>
        AnyoneCanPay = 0b1000_0000 // 0x80
    }
}
