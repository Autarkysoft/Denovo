// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Defines methods and properties that a block verifier implements.
    /// </summary>
    public interface IBlockVerifier
    {
        /// <summary>
        /// Verifies validity of thegiven <see cref="BlockHeader"/> by performing basic verifications (verion, target and PoW).
        /// Return value indicates succcess.
        /// <para/><see cref="IConsensus"/> dependency has to be updated by the caller before calling this method.
        /// </summary>
        /// <param name="header">Block header to verify</param>
        /// <param name="expectedTarget">
        /// The target that this header must have (calculated considering difficulty adjustment)
        /// </param>
        /// <returns>True if the given block header was valid; otherwise false.</returns>
        bool VerifyHeader(in BlockHeader header, Target expectedTarget);

        /// <summary>
        /// Verifies validity of the given block. Return value indicates succcess.
        /// <para/>Header has to be verified before using <see cref="VerifyHeader(in BlockHeader, Target)"/> method.
        /// <para/><see cref="IConsensus"/> dependency has to be updated by the caller before calling this method.
        /// </summary>
        /// <param name="block">Block to use</param>
        /// <param name="error">Error message (null if valid, otherwise contains information about the reason).</param>
        /// <returns>True if block was valid, otherwise false.</returns>
        bool Verify(IBlock block, out string error);

        /// <summary>
        /// Updates the UTXO database using the given (already verified) block.
        /// </summary>
        /// <param name="block">Block to use</param>
        void UpdateDB(IBlock block);
    }
}
