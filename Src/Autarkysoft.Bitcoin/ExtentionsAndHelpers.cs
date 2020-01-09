// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// byte[] extensions.
    /// </summary>
    public static class ByteArrayExtension
    {
        /// <summary>
        /// Creates a copy (clone) of the given byte array, 
        /// will return null if the source was null instead of throwing an exception.
        /// </summary>
        /// <param name="ba">Byte array to clone</param>
        /// <returns>Copy (clone) of the given byte array</returns>
        public static byte[] CloneByteArray(this byte[] ba)
        {
            if (ba == null)
            {
                return null;
            }
            else
            {
                byte[] result = new byte[ba.Length];
                Buffer.BlockCopy(ba, 0, result, 0, ba.Length);
                return result;
            }
        }
    }
}
