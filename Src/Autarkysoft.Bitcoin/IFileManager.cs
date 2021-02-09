// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Defines methods used by a file manager responsible for reading/writing data from/to disk.
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Appends the given byte array to the end of the file.
        /// </summary>
        /// <param name="data">Data to add</param>
        /// <param name="fileName">Name of the file</param>
        void AppendData(byte[] data, string fileName);
        /// <summary>
        /// Reads a file specified by its name and returns the read bytes.
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <returns>File bytes</returns>
        byte[] ReadData(string fileName);
        /// <summary>
        /// Writes the given byte array to disk as a new file (replacing any existing one) using the given name.
        /// </summary>
        /// <param name="data">Data to write</param>
        /// <param name="fileName">Name of the file</param>
        void WriteData(byte[] data, string fileName);
        /// <summary>
        /// Writes the given block to disk
        /// </summary>
        /// <param name="block">Block to store</param>
        void WriteBlock(IBlock block);
    }
}
