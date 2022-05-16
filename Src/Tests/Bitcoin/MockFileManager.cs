// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;
using Xunit;

namespace Tests.Bitcoin
{
    public enum FileManCallName
    {
        AppendData_Block0,
        AppendData_Block1,
        AppendData_BlockInfo,
        AppendData_Headers,
        AppendData_Addrs,

        ReadData_BlockInfo,
        ReadData_Headers,
        ReadData_Addrs,

        WriteData_Headers,
        WriteData_Addr,

        ReadBlockInfo,
        WriteBlock
    }

    public class MockFileManager : IFileManager
    {
        public MockFileManager(FileManCallName[] cn, byte[][] data)
        {
            Assert.True((cn is null && data is null) || cn.Length == data.Length);
            callNames = cn;
            expected = data;
        }


        private int index = 0;
        internal FileManCallName[] callNames;
        internal byte[][] expected;

        internal void AssertIndex()
        {
            Assert.Equal(index, callNames.Length);
            Assert.Equal(blockIndex, expBlocks.Length);
        }

        internal void ResetIndex()
        {
            index = 0;
            blockIndex = 0;
        }

        private static string Convert(FileManCallName cn)
        {
            return cn switch
            {
                FileManCallName.AppendData_Block0 => "Block000000",
                FileManCallName.AppendData_Block1 => "Block000001",
                FileManCallName.AppendData_BlockInfo => "BlockInfo",
                FileManCallName.AppendData_Headers => "Headers",
                FileManCallName.AppendData_Addrs => "NodeAddrs",
                FileManCallName.ReadData_BlockInfo => "BlockInfo",
                FileManCallName.ReadData_Headers => "Headers",
                FileManCallName.ReadData_Addrs => "NodeAddrs",
                FileManCallName.WriteData_Headers => "Headers",
                FileManCallName.WriteData_Addr => "NodeAddrs",
                FileManCallName.ReadBlockInfo => throw new NotImplementedException(),
                FileManCallName.WriteBlock => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }


        public void AppendData(byte[] data, string fileName)
        {
            Assert.True(index < callNames.Length, Helper.UnexpectedCall);
            Assert.Equal(Convert(callNames[index]), fileName);
            Assert.Equal(expected[index], data);
            index++;
        }

        public byte[] ReadData(string fileName)
        {
            Assert.True(index < callNames.Length, Helper.UnexpectedCall);
            Assert.Equal(Convert(callNames[index]), fileName);
            return expected[index++];
        }

        public void WriteData(byte[] data, string fileName)
        {
            Assert.True(index < callNames.Length, Helper.UnexpectedCall);
            Assert.Equal(Convert(callNames[index]), fileName);
            Assert.Equal(expected[index], data);
            index++;
        }

        public byte[] ReadBlockInfo()
        {
            Assert.True(index < callNames.Length, Helper.UnexpectedCall);
            Assert.Equal(FileManCallName.ReadBlockInfo, callNames[index]);
            return expected[index++];
        }


        internal IBlock[] expBlocks = Array.Empty<IBlock>();
        internal int blockIndex = 0;
        public void WriteBlock(IBlock block)
        {
            Assert.True(index < callNames.Length, Helper.UnexpectedCall);
            Assert.True(blockIndex < expBlocks.Length, Helper.UnexpectedCall);
            Assert.Equal(FileManCallName.WriteBlock, callNames[index]);

            bool eq = ReferenceEquals(expBlocks[blockIndex], block) ||
                      expBlocks[blockIndex].Header.Hash.Equals(block.Header.Hash);
            Assert.True(eq, "Given block is not as expected.");

            index++;
            blockIndex++;
        }
    }
}
