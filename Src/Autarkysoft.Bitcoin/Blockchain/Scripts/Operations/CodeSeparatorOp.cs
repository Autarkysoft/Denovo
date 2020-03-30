// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Does nothing (but affects how hash for signatures are produced).
    /// </summary>
    public class CodeSeparatorOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CodeSeparator;

        /// <summary>
        /// Does nothing. 
        /// Under the hood, it increments number of executed <see cref="OP.CodeSeparator"/>s so that CheckSig ops can know
        /// how to serialize scripts for signing.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Always null</param>
        /// <returns>Always true</returns>
        public override bool Run(IOpData opData, out string error)
        {
            opData.CodeSeparatorCount++;
            error = null;
            return true;
        }
    }
}
