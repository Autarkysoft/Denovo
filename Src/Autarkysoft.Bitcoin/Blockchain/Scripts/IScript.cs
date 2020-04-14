// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Defines methods and properties that any script within transactions must have. 
    /// Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public interface IScript : IDeserializable
    {
        /// <summary>
        /// This script's content as an array of bytes (can be null for empty scripts)
        /// </summary>
        byte[] Data { get; set; }

        /// <summary>
        /// Returns number of <see cref="OP.CheckSig"/>, <see cref="OP.CheckSigVerify"/>, <see cref="OP.CheckMultiSig"/>
        /// and <see cref="OP.CheckMultiSigVerify"/> operations in this instance without a full script evaluation.
        /// </summary>
        /// <returns>Number of "SigOps"</returns>
        int CountSigOps();

        /// <summary>
        /// Converts <see cref="Data"/> to an array of <see cref="IOperation"/>s (result can be an empty array). 
        /// Return value indicates success.
        /// </summary>
        /// <param name="result">An array of <see cref="IOperation"/>s</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure)</param>
        /// <returns>True if evaluation was successful, false if otherwise.</returns>
        bool TryEvaluate(out IOperation[] result, out string error);
    }
}
