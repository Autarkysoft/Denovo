// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using Xunit;

namespace Tests.Bitcoin
{
#pragma warning disable CS0649 // Field is never assigned to
    class MockFileManager : IFileManager
    {
        private const string UnexpectedCall = "Unexpected call was made";


        public void AppendData(byte[] data, string fileName)
        {
            throw new NotImplementedException();
        }

        internal string expReadFN;
        internal byte[] returnReadData; // Can be null
        public byte[] ReadData(string fileName)
        {
            Assert.False(string.IsNullOrEmpty(expReadFN), UnexpectedCall);
            Assert.Equal(expReadFN, fileName);
            return returnReadData;
        }

        internal string expWriteFN;
        internal byte[] expWriteData;
        public void WriteData(byte[] data, string fileName)
        {
            Assert.False(string.IsNullOrEmpty(expWriteFN), UnexpectedCall);
            Assert.Equal(expWriteFN, fileName);
            Assert.Equal(expWriteData, data);
        }
    }
#pragma warning restore CS0649 // Field is never assigned to
}
