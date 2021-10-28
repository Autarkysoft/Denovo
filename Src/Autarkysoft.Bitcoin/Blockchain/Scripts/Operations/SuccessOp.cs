// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operations representing a OP_SuccessX used in Taproot scripts
    /// </summary>
    public class SuccessOp : SimpleRunableOpsBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SuccessOp"/> with the given OP value
        /// </summary>
        /// <param name="b">OP value (will not be validated)</param>
        public SuccessOp(byte b)
        {
            OpValue = (OP)b;
        }

        /// <inheritdoc/>
        public override OP OpValue { get; }
    }
}
