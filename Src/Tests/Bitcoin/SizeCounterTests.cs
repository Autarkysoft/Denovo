// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using Xunit;

namespace Tests.Bitcoin
{
    public class SizeCounterTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var counter = new SizeCounter();
            Assert.Equal(0, counter.Size);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public void Constructor_WithInitialSizeTest(int size)
        {
            var counter = new SizeCounter(size);
            Assert.Equal(size, counter.Size);
        }

        [Fact]
        public void Constructor_WithInitialSize_ExceptionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SizeCounter(-1));
        }

        [Fact]
        public void ResetTest()
        {
            var counter = new SizeCounter();
            Assert.Equal(0, counter.Size);

            counter.Reset();
            Assert.Equal(0, counter.Size);

            counter.Add(10);
            Assert.Equal(10, counter.Size);

            counter.Reset();
            Assert.Equal(0, counter.Size);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 2)]
        [InlineData(1, 5, 6)]
        public void Add_SizeTest(int init, int add, int expected)
        {
            var counter = new SizeCounter(init);
            counter.Add(add);
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(3, 1)]
        [InlineData(3, 2)]
        [InlineData(3, 75)]
        [InlineData(3, 76)]
        [InlineData(3, 77)]
        [InlineData(3, 255)]
        [InlineData(3, 256)]
        [InlineData(3, ushort.MaxValue)]
        [InlineData(3, ushort.MaxValue + 1)]
        [InlineData(3, int.MaxValue)]
        public void AddWithStackIntLengthTest(int init, int add)
        {
            var counter = new SizeCounter(init);
            counter.AddWithStackIntLength(add);

            var si = new StackInt(add);
            var stream = new FastStream(5);
            si.WriteToStream(stream);
            int expected = stream.GetSize() + add + init;

            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(3, 1)]
        [InlineData(3, 252)]
        [InlineData(3, 253)]
        [InlineData(3, ushort.MaxValue)]
        [InlineData(3, ushort.MaxValue + 1)]
        [InlineData(3, int.MaxValue)]
        public void AddWithCompactIntLengthTest(int init, int add)
        {
            var counter = new SizeCounter(init);
            counter.AddWithCompactIntLength(add);

            var ci = new CompactInt(add);
            var stream = new FastStream(9);
            ci.WriteToStream(stream);
            int expected = stream.GetSize() + add + init;

            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(3, 1)]
        [InlineData(3, 252)]
        [InlineData(3, 253)]
        [InlineData(3, ushort.MaxValue)]
        [InlineData(3, ushort.MaxValue + 1)]
        [InlineData(3, int.MaxValue)]
        public void AddCompactIntCountTest(int init, int add)
        {
            var counter = new SizeCounter(init);
            counter.AddCompactIntCount(add);

            var ci = new CompactInt(add);
            var stream = new FastStream(9);
            ci.WriteToStream(stream);
            int expected = stream.GetSize() + init;

            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(5, 6)]
        public void AddByteTest(int init, int expected)
        {
            var counter = new SizeCounter(init);
            counter.AddByte();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(5, 7)]
        public void AddInt16Test(int init, int expected)
        {
            var counter = new SizeCounter(init);
            counter.AddInt16();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 4)]
        [InlineData(5, 9)]
        public void AddInt32Test(int init, int expected)
        {
            var counter = new SizeCounter(init);
            counter.AddInt32();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 8)]
        [InlineData(5, 13)]
        public void AddInt64Test(int init, int expected)
        {
            var counter = new SizeCounter(init);
            counter.AddInt64();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(5, 7)]
        public void AddUInt16Test(int init, int expected)
        {
            var counter = new SizeCounter(init);
            counter.AddUInt16();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 4)]
        [InlineData(5, 9)]
        public void AddUInt32Test(int init, int expected)
        {
            var counter = new SizeCounter(init);
            counter.AddUInt32();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 8)]
        [InlineData(5, 13)]
        public void AddUInt64Test(int init, int expected)
        {
            var counter = new SizeCounter(init);
            counter.AddUInt64();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 20)]
        [InlineData(5, 25)]
        public void AddHash160Test(int init, int expected)
        {
            var counter = new SizeCounter(init);
            counter.AddHash160();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 32)]
        [InlineData(5, 37)]
        public void AddHash256Test(int init, int expected)
        {
            var counter = new SizeCounter(init);
            counter.AddHash256();
            Assert.Equal(expected, counter.Size);
        }
    }
}
