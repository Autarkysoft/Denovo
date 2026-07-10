// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;

namespace Tests.Bitcoin
{
    public class SizeCounterTests
    {
        [Fact]
        public void ConstructorTest()
        {
            SizeCounter counter = new();
            Assert.Equal(0, counter.Size);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public void Constructor_WithInitialSizeTest(int size)
        {
            SizeCounter counter = new(size);
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
            SizeCounter counter = new();
            Assert.Equal(0, counter.Size);

            counter.Reset();
            Assert.Equal(0, counter.Size);

            counter.Add(10);
            Assert.Equal(10, counter.Size);

            counter.Reset();
            Assert.Equal(0, counter.Size);
        }

        [Fact]
        public void CumulativeTest()
        {
            SizeCounter counter = new();
            counter.Add(1);
            counter.AddUInt64();
            counter.AddByte();
            counter.AddHash160();

            Assert.Equal(30, counter.Size);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 2)]
        [InlineData(1, 5, 6)]
        public void Add_SizeTest(int init, int add, int expected)
        {
            SizeCounter counter = new(init);
            counter.Add(add);
            Assert.Equal(expected, counter.Size);
        }

        [Fact]
        public void Add_Size_ExceptionTest()
        {
            SizeCounter counter = new();
            Assert.Throws<ArgumentOutOfRangeException>(() => counter.Add(-1));
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
            SizeCounter counter = new(init);
            counter.AddWithStackIntLength(add);

            StackInt si = new(add);
            FastStream stream = new(5);
            si.WriteToStream(stream);
            int expected = stream.GetSize() + add + init;

            Assert.Equal(expected, counter.Size);
        }

        [Fact]
        public void AddWithStackIntLength_ExceptionTest()
        {
            SizeCounter counter = new();
            Assert.Throws<ArgumentOutOfRangeException>(() => counter.AddWithStackIntLength(-1));
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
            SizeCounter counter = new(init);
            counter.AddWithCompactIntLength(add);

            CompactInt ci = new(add);
            FastStream stream = new(9);
            ci.WriteToStream(stream);
            int expected = stream.GetSize() + add + init;

            Assert.Equal(expected, counter.Size);
        }

        [Fact]
        public void AddWithCompactIntLength_ExceptionTest()
        {
            SizeCounter counter = new();
            Assert.Throws<ArgumentOutOfRangeException>(() => counter.AddWithCompactIntLength(-1));
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
            SizeCounter counter = new(init);
            counter.AddCompactIntCount(add);

            CompactInt ci = new(add);
            FastStream stream = new(9);
            ci.WriteToStream(stream);
            int expected = stream.GetSize() + init;

            Assert.Equal(expected, counter.Size);
        }

        [Fact]
        public void AddCompactIntCount_ExceptionTest()
        {
            SizeCounter counter = new();
            Assert.Throws<ArgumentOutOfRangeException>(() => counter.AddCompactIntCount(-1));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(5, 6)]
        public void AddByteTest(int init, int expected)
        {
            SizeCounter counter = new(init);
            counter.AddByte();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(5, 7)]
        public void AddInt16Test(int init, int expected)
        {
            SizeCounter counter = new(init);
            counter.AddInt16();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 4)]
        [InlineData(5, 9)]
        public void AddInt32Test(int init, int expected)
        {
            SizeCounter counter = new(init);
            counter.AddInt32();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 8)]
        [InlineData(5, 13)]
        public void AddInt64Test(int init, int expected)
        {
            SizeCounter counter = new(init);
            counter.AddInt64();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(5, 7)]
        public void AddUInt16Test(int init, int expected)
        {
            SizeCounter counter = new(init);
            counter.AddUInt16();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 4)]
        [InlineData(5, 9)]
        public void AddUInt32Test(int init, int expected)
        {
            SizeCounter counter = new(init);
            counter.AddUInt32();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 8)]
        [InlineData(5, 13)]
        public void AddUInt64Test(int init, int expected)
        {
            SizeCounter counter = new(init);
            counter.AddUInt64();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 20)]
        [InlineData(5, 25)]
        public void AddHash160Test(int init, int expected)
        {
            SizeCounter counter = new(init);
            counter.AddHash160();
            Assert.Equal(expected, counter.Size);
        }

        [Theory]
        [InlineData(0, 32)]
        [InlineData(5, 37)]
        public void AddHash256Test(int init, int expected)
        {
            SizeCounter counter = new(init);
            counter.AddHash256();
            Assert.Equal(expected, counter.Size);
        }
    }
}
