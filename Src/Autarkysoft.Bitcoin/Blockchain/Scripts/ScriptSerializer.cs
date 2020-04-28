// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Contains methods used for serializing scripts for signing operations.
    /// Handles <see cref="OP.CodeSeparator"/>, removes signatures,...
    /// </summary>
    public class ScriptSerializer
    {
        private int FindLastExecutedCodeSeparator(IOperation[] ops)
        {
            int start = 0;
            for (int i = ops.Length - 1; i >= 0; i--)
            {
                if (ops[i] is CodeSeparatorOp cs && cs.IsExecuted)
                {
                    start = i;
                    break;
                }
                else if (ops[i] is IfElseOpsBase conditional && conditional.HasExecutedCodeSeparator())
                {
                    start = i;
                    break;
                }
            }
            return start;
        }

        /// <summary>
        /// Converts the given script operations to byte array to be used in serialization
        /// while removing the given signature if found.
        /// <para/>Note: does not include result length
        /// </summary>
        /// <param name="ops">An array of <see cref="IOperation"/>s in the script</param>
        /// <param name="sig">Signatrue to remove</param>
        /// <returns>An array of bytes</returns>
        public byte[] Convert(IOperation[] ops, ReadOnlySpan<byte> sig)
        {
            int start = FindLastExecutedCodeSeparator(ops);

            FastStream temp = new FastStream(100);
            for (int i = start; i < ops.Length; i++)
            {
                ops[i].WriteToStreamForSigning(temp, sig);
            }

            return temp.ToByteArray();
        }

        /// <summary>
        /// Converts the given script operations to byte array to be used in serialization
        /// while removing the given signatures if found.
        /// <para/>Note: does not include result length
        /// </summary>
        /// <param name="ops">An array of <see cref="IOperation"/>s in the script</param>
        /// <param name="sigs">An array of signatrue to remove</param>
        /// <returns>An array of bytes</returns>
        public byte[] ConvertMulti(IOperation[] ops, byte[][] sigs)
        {
            int start = FindLastExecutedCodeSeparator(ops);

            FastStream temp = new FastStream(100);
            for (int i = start; i < ops.Length; i++)
            {
                ops[i].WriteToStreamForSigning(temp, sigs);
            }

            return temp.ToByteArray();
        }

        /// <summary>
        /// Converts the given P2WPKH script operation to the byte array used in serialization for signing.
        /// <para/>Note: Will not validate (assumes the validation is done by the caller)
        /// <para/>Note: does not include result length
        /// </summary>
        /// <param name="ops">
        /// Must contain 2 <see cref="PushDataOp"/> with first one being <see cref="OP._0"/> and second being a 20 byte push.
        /// </param>
        /// <returns>An array of bytes</returns>
        public byte[] ConvertP2wpkh(IOperation[] ops)
        {
            // Convert P2WPKH (0014<hash160>) to (19)76a914<hash160>88ac without data length (0x19)
            byte[] result = new byte[25];
            result[0] = (byte)OP.DUP;
            result[1] = (byte)OP.HASH160;
            result[2] = 20;
            Buffer.BlockCopy(((PushDataOp)ops[1]).data, 0, result, 3, 20);
            result[^2] = (byte)OP.EqualVerify;
            result[^1] = (byte)OP.CheckSig;

            return result;
        }
    }
}
