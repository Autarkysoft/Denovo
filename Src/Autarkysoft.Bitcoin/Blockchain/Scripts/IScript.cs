// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    public interface IScript : IDeserializable
    {
        /// <summary>
        /// Returns whether the script instance is of witness type. It will affect (de)serialization methods.
        /// </summary>
        bool IsWitness { get; }

        /// <summary>
        /// Type of this script instance
        /// </summary>
        ScriptType ScriptType { get; }

        /// <summary>
        /// List of operations that the script contains.
        /// </summary>
        IOperation[] OperationList { get; set; }

        /// <summary>
        /// Converts this instance into its byte array representation only containing <see cref="IOperation"/>s as bytes 
        /// without the starting integer for length or count.
        /// </summary>
        /// <returns>An array of bytes</returns>
        byte[] ToByteArray();
    }
}
