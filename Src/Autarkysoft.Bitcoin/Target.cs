// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin
{
    public readonly struct Target
    {
        /// <summary>
        /// Converts this value to its byte array representation in little-endian order 
        /// and writes the result to the given <see cref="FastStream"/>.
        /// </summary>
        /// <param name="stream">Stream to use.</param>
        public void WriteToStream(FastStream stream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the <see cref="Target"/> value from the given <see cref="FastStreamReader"/>. 
        /// Return value indicates success.
        /// </summary>
        /// <param name="stream">Stream containing the <see cref="StackInt"/></param>
        /// <param name="result">The result</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public static bool TryRead(FastStreamReader stream, out Target result, out string error)
        {
            throw new NotImplementedException();
        }
    }
}
