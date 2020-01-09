// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Defines methods that a class implements to have serialize capabilities.
    /// </summary>
    public interface IDeserializable
    {
        /// <summary>
        /// Converts this object to its byte array representation and writes those bytes to the given stream.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        void Serialize(FastStream stream);
    }
}
