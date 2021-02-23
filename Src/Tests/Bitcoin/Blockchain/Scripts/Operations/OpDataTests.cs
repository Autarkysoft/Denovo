// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class OpDataTests
    {
        private const int DefaultCapacity = 10;

        [Fact]
        public void Constructor_DefaultTest()
        {
            OpData opd = new OpData();
            byte[][] expected = new byte[DefaultCapacity][];

            Helper.ComparePrivateField(opd, "holder", expected);
            Assert.Equal(0, opd.ItemCount);
        }

        [Theory]
        [InlineData(-1, DefaultCapacity)]
        [InlineData(0, DefaultCapacity)]
        [InlineData(9, DefaultCapacity)]
        [InlineData(10, 10)]
        [InlineData(11, 11)]
        public void Constructor_WithCapTest(int capToUse, int expectedLen)
        {
            OpData opd = new OpData(capToUse);
            byte[][] expected = new byte[expectedLen][];

            Helper.ComparePrivateField(opd, "holder", expected);
            Assert.Equal(0, opd.ItemCount);
        }

        public static IEnumerable<object[]> GetConstructorCases()
        {
            yield return new object[] { null, new byte[DefaultCapacity][], 0 };
            yield return new object[] { new byte[0][], new byte[DefaultCapacity][], 0 };
            yield return new object[] { new byte[3][], new byte[DefaultCapacity][], 3 };
            yield return new object[] { new byte[10][], new byte[DefaultCapacity + 10][], 10 };
            yield return new object[] { new byte[11][], new byte[DefaultCapacity + 11][], 11 };
            yield return new object[]
            {
                new byte[1][] { new byte[3] { 1, 2, 3 } },
                new byte[10][] { new byte[3] { 1, 2, 3 }, null, null, null, null, null, null, null, null, null },
                1
            };
            yield return new object[]
            {
                new byte[3][] { new byte[3] {1,2,3}, new byte[2] {4,5}, new byte[5] {6,7,8,9,10} },
                new byte[10][]
                {
                    new byte[3] {1,2,3}, new byte[2] {4,5}, new byte[5] {6,7,8,9,10}, null, null, null, null, null, null, null
                },
                3
            };
        }
        [Theory]
        [MemberData(nameof(GetConstructorCases))]
        public void Constructor_WithDataTest(byte[][] data, byte[][] expected, int expCount)
        {
            OpData opd = new OpData(data);

            Helper.ComparePrivateField(opd, "holder", expected);
            Assert.Equal(expCount, opd.ItemCount);
        }


        [Theory]
        // If property is set to false (non-standard and not-tapscript) the check always returns true
        [InlineData(new byte[0], false, true)]
        [InlineData(new byte[] { 0 }, false, true)]
        [InlineData(new byte[] { 1 }, false, true)]
        [InlineData(new byte[] { 2 }, false, true)]
        [InlineData(new byte[] { 1, 2 }, false, true)]
        // Otherwise the stack item must be either empty array or array with 1 item equal to 1 (byte[0] or byte[1]{1})
        [InlineData(new byte[0], true, true)]
        [InlineData(new byte[] { 0 }, true, false)]
        [InlineData(new byte[] { 1 }, true, true)]
        [InlineData(new byte[] { 2 }, true, false)]
        [InlineData(new byte[] { 1, 2 }, true, false)]
        public void CheckConditionalOpBoolTest(byte[] data, bool standardRule, bool expected)
        {
            var stack = new OpData()
            {
                IsStrictConditionalOpBool = standardRule
            };
            bool actual = stack.CheckConditionalOpBool(data);
            Assert.Equal(expected, actual);
        }



        private const int TestItemCount = 5;
        private static readonly byte[][] testData5 = new byte[TestItemCount][]
        {
            new byte[] { 1, 2 },
            new byte[] { 3, 4, 5 },
            new byte[] { 6 },
            new byte[] { 7, 8, 9, 10 },
            new byte[] { 0, 0, 255, 200 }
        };
        private static readonly byte[][] testData9 = new byte[9][]
        {
            new byte[] { 7, 8, 9, 10 },
            new byte[] { 125 },
            new byte[] { 0, 0, 255, 200 },
            new byte[] { 1, 0, 5 },
            new byte[] { 100, 75, 32, 1, 0 },
            new byte[] { 90, 2 },
            new byte[] { 87 },
            new byte[] { 255, 12, 46, 12, 25, 143 },
            new byte[] { 1, 57, 194, 61 }
        };
        //private static readonly byte[][] testData10 = new byte[10][]
        //{
        //    new byte[] { 13 },
        //    new byte[] { 8, 9, 10 },
        //    new byte[] { 0, 0, 5, 200 },
        //    new byte[] { 1, 40, 5 },
        //    new byte[] { 75, 12, 0 },
        //    new byte[] { 90, 25, 55 },
        //    new byte[] { 66, 99 },
        //    new byte[] { 46, 12, 25, 143 },
        //    new byte[] { 57, 77, 61 },
        //    new byte[] { 255, 200 }
        //};


        [Fact]
        public void PeekTest()
        {
            OpData opd = new OpData(testData5);
            byte[] actual = opd.Peek();
            byte[] expected = testData5[^1];

            Assert.Equal(expected, actual);
            Assert.Equal(TestItemCount, opd.ItemCount);
        }

        [Fact]
        public void Peek_MultipleTest()
        {
            OpData opd = new OpData(testData5);
            byte[][] actual = opd.Peek(3);
            byte[][] expected = new byte[3][]
            {
                testData5[^3], testData5[^2], testData5[^1]
            };

            Assert.Equal(expected, actual);
            Assert.Equal(TestItemCount, opd.ItemCount);
        }

        [Fact]
        public void PeekAtIndexTest()
        {
            OpData opd = new OpData(testData5);
            // Index starts from zero and counts backwards from end (index=0 is last item)
            byte[] actual1 = opd.PeekAtIndex(0);
            byte[] expected1 = testData5[^1];

            byte[] actual2 = opd.PeekAtIndex(testData5.Length - 1);
            byte[] expected2 = testData5[0];

            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(TestItemCount, opd.ItemCount);
        }

        [Fact]
        public void PopTest()
        {
            OpData opd = new OpData(testData5);
            byte[] actual1 = opd.Pop();
            byte[] expected1 = testData5[^1];

            Assert.Equal(expected1, actual1);
            Assert.Equal(TestItemCount - 1, opd.ItemCount); // 1 item is removed

            byte[] actual2 = opd.Pop();
            byte[] expected2 = testData5[^2];

            Assert.Equal(expected2, actual2);
            Assert.Equal(TestItemCount - 2, opd.ItemCount); // 2 items are removed

            // Make sure the test array is not changed
            Assert.Equal(TestItemCount, testData5.Length);
        }

        [Fact]
        public void Pop_MultipleTest()
        {
            OpData opd = new OpData(testData5);
            byte[][] actual1 = opd.Pop(2);
            byte[][] expected1 = new byte[2][]
            {
                testData5[^2], testData5[^1]
            };

            Assert.Equal(expected1, actual1);
            Assert.Equal(TestItemCount - 2, opd.ItemCount); // 2 items are removed

            byte[][] actual2 = opd.Pop(3);
            byte[][] expected2 = new byte[3][]
            {
                testData5[^5], testData5[^4], testData5[^3]
            };

            Assert.Equal(expected2, actual2);
            Assert.Equal(0, opd.ItemCount); // 5 (all) items are removed

            // Make sure the test array is not changed
            Assert.Equal(TestItemCount, testData5.Length);
        }


        public static IEnumerable<object[]> GetPopAtIndexCases()
        {
            // (int index, byte[] expected, int expCount, byte[][] expData)
            yield return new object[]
            {
                0, // First item from end is popped
                testData5[4],
                TestItemCount - 1,
                new byte[10][]
                {
                    testData5[0], testData5[1], testData5[2], testData5[3], null, null, null, null, null, null
                }
            };
            yield return new object[]
            {
                1, // Second item from end is popped
                testData5[3],
                TestItemCount - 1,
                new byte[10][]
                {
                    testData5[0], testData5[1], testData5[2], testData5[4], null, null, null, null, null, null
                }
            };
            yield return new object[]
            {
                4, // Last item from end is popped
                testData5[0],
                TestItemCount - 1,
                new byte[10][]
                {
                    testData5[1], testData5[2], testData5[3], testData5[4], null, null, null, null, null, null
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetPopAtIndexCases))]
        public void PopAtIndexTest(int index, byte[] expPopped, int expCount, byte[][] expData)
        {
            OpData opd = new OpData(testData5);
            byte[] actual = opd.PopAtIndex(index);

            Assert.Equal(expPopped, actual);
            Assert.Equal(expCount, opd.ItemCount);
            Helper.ComparePrivateField(opd, "holder", expData);
        }


        public static IEnumerable<object[]> GetPushCases()
        {
            // (byte[][] dataToUse, byte[] dataToPush, byte[][] expData, int expCount)
            yield return new object[]
            {
                null,
                new byte[2] { 255, 255 },
                new byte[10][]
                {
                    new byte[2] { 255, 255 }, null, null, null, null, null, null, null, null, null
                },
                1
            };
            yield return new object[]
            {
                testData5,
                new byte[2] { 255, 255 },
                new byte[10][]
                {
                    testData5[0], testData5[1], testData5[2], testData5[3], testData5[4],
                    new byte[2] { 255, 255 }, null, null, null, null
                },
                TestItemCount + 1
            };
        }
        [Theory]
        [MemberData(nameof(GetPushCases))]
        public void PushTest(byte[][] dataToUse, byte[] dataToPush, byte[][] expData, int expCount)
        {
            OpData opd = new OpData(dataToUse);
            opd.Push(dataToPush);

            Helper.ComparePrivateField(opd, "holder", expData);
            Assert.Equal(expCount, opd.ItemCount);
        }

        [Fact]
        public void Push_SpecialCaseTest()
        {
            OpData opd = new OpData(testData9);

            // First push 1 item to fill all 10 items in holder
            byte[] tempToPush = new byte[3] { 12, 13, 14 };
            opd.Push(tempToPush);

            byte[][] expHolder = new byte[10][];
            Array.Copy(testData9, expHolder, 9);
            expHolder[9] = tempToPush;

            Assert.Equal(10, opd.ItemCount);
            Helper.ComparePrivateField(opd, "holder", expHolder);

            // Now push 1 more item to test correct resize
            byte[] toPush = new byte[2] { 1, 2 };
            opd.Push(toPush);

            byte[][] expHolder2 = new byte[20][];
            Array.Copy(expHolder, expHolder2, 10);
            expHolder2[10] = toPush;

            Assert.Equal(11, opd.ItemCount);
            Helper.ComparePrivateField(opd, "holder", expHolder2);
        }

        public static IEnumerable<object[]> GetPushMultipleCases()
        {
            // (byte[][] dataToUse, byte[][] dataToPush, byte[][] expData, int expCount)
            yield return new object[]
            {
                null,
                new byte[1][] { new byte[0] },
                new byte[10][]
                {
                    new byte[0], null, null, null, null, null, null, null, null, null
                },
                1
            };
            yield return new object[]
            {
                null,
                new byte[2][] { new byte[1] { 5 }, new byte[2] { 10, 20 } },
                new byte[10][]
                {
                    new byte[1] { 5 }, new byte[2] { 10, 20 }, null, null, null, null, null, null, null, null
                },
                2
            };
            yield return new object[]
            {
                new byte[3][]
                {
                    new byte[]{ 1, 2 }, new byte[] { 5, 6 }, new byte[] { 12 },
                },
                new byte[2][] { new byte[1] { 5 }, new byte[2] { 10, 20 } },
                new byte[10][]
                {
                    new byte[]{ 1, 2 }, new byte[] { 5, 6 }, new byte[] { 12 },
                    new byte[1] { 5 }, new byte[2] { 10, 20 }, null, null, null, null, null
                },
                5
            };
            yield return new object[]
            {
                testData9,
                new byte[2][] { new byte[1] { 5 }, new byte[2] { 10, 20 } },
                new byte[21][]
                {
                    testData9[0], testData9[1], testData9[2], testData9[3], testData9[4],
                    testData9[5], testData9[6], testData9[7], testData9[8],
                    new byte[1] { 5 }, new byte[2] { 10, 20 },
                    null, null, null, null, null, null, null, null, null, null
                },
                11
            };
            yield return new object[]
            {
                testData9,
                testData5,
                new byte[24][]
                {
                    testData9[0], testData9[1], testData9[2], testData9[3], testData9[4],
                    testData9[5], testData9[6], testData9[7], testData9[8],
                    testData5[0], testData5[1], testData5[2], testData5[3], testData5[4],
                    null, null, null, null, null, null, null, null, null, null
                },
                9 + TestItemCount
            };
        }
        [Theory]
        [MemberData(nameof(GetPushMultipleCases))]
        public void Push_MultipleTest(byte[][] dataToUse, byte[][] dataToPush, byte[][] expData, int expCount)
        {
            OpData opd = new OpData(dataToUse);
            opd.Push(dataToPush);

            Helper.ComparePrivateField(opd, "holder", expData);
            Assert.Equal(expCount, opd.ItemCount);
        }


        public static IEnumerable<object[]> GetInsertCases()
        {
            // (byte[][] dataToUse, byte[] dataToInsert, int index, byte[][] expData, int expCount)
            yield return new object[]
            {
                null,
                new byte[1]{ 5 },
                0,
                new byte[10][]
                {
                    new byte[1]{ 5 }, null, null, null, null, null, null, null, null, null
                },
                1
            };
            yield return new object[]
            {
                new byte[3][] { new byte[] { 7, 8 }, new byte[] { 123 }, new byte[] { 0, 255, 200 } },
                new byte[1]{ 5 },
                0,
                new byte[10][]
                {
                    new byte[] { 7, 8 }, new byte[] { 123 }, new byte[] { 0, 255, 200 },
                    new byte[1]{ 5 },
                    null, null, null, null, null, null
                },
                4
            };
            yield return new object[]
            {
                new byte[3][] { new byte[] { 7, 8 }, new byte[] { 123 }, new byte[] { 0, 255, 200 } },
                new byte[1]{ 5 },
                1,
                new byte[10][]
                {
                    new byte[] { 7, 8 }, new byte[] { 123 }, new byte[1]{ 5 }, new byte[] { 0, 255, 200 },
                    null, null, null, null, null, null
                },
                4
            };
            yield return new object[]
            {
                new byte[3][] { new byte[] { 7, 8 }, new byte[] { 123 }, new byte[] { 0, 255, 200 } },
                new byte[1]{ 5 },
                3,
                new byte[10][]
                {
                    new byte[1]{ 5 }, new byte[] { 7, 8 }, new byte[] { 123 }, new byte[] { 0, 255, 200 },
                    null, null, null, null, null, null
                },
                4
            };
        }
        [Theory]
        [MemberData(nameof(GetInsertCases))]
        public void InsertTest(byte[][] dataToUse, byte[] dataToInsert, int index, byte[][] expData, int expCount)
        {
            OpData opd = new OpData(dataToUse);
            opd.Insert(dataToInsert, index);

            Helper.ComparePrivateField(opd, "holder", expData);
            Assert.Equal(expCount, opd.ItemCount);
        }

        [Fact]
        public void Insert_SpecialCase_Index0Test()
        {
            OpData opd = new OpData(testData9);

            // First push 1 item to fill all 10 items in holder
            byte[] tempToPush = new byte[3] { 12, 13, 14 };
            opd.Push(tempToPush);

            byte[][] expHolder = new byte[10][];
            Array.Copy(testData9, expHolder, 9);
            expHolder[9] = tempToPush;

            Assert.Equal(10, opd.ItemCount);
            Helper.ComparePrivateField(opd, "holder", expHolder);

            // Now push 1 more item to test correct resize
            byte[] toPush = new byte[2] { 1, 2 };
            opd.Insert(toPush, 0);

            byte[][] expHolder2 = new byte[20][];
            Array.Copy(expHolder, expHolder2, 10);
            expHolder2[10] = toPush;

            Assert.Equal(11, opd.ItemCount);
            Helper.ComparePrivateField(opd, "holder", expHolder2);
        }

        [Fact]
        public void Insert_SpecialCase_IndexNTest()
        {
            OpData opd = new OpData(testData9);

            // First push 1 item to fill all 10 items in holder
            byte[] tempToPush = new byte[3] { 12, 13, 14 };
            opd.Push(tempToPush);

            byte[][] expHolder = new byte[10][];
            Array.Copy(testData9, expHolder, 9);
            expHolder[9] = tempToPush;

            Assert.Equal(10, opd.ItemCount);
            Helper.ComparePrivateField(opd, "holder", expHolder);

            // Now push 1 more item to test correct resize
            byte[] toInsert = new byte[2] { 1, 2 };
            opd.Insert(toInsert, 2);

            byte[][] expHolder2 = new byte[20][]
            {
                testData9[0], testData9[1], testData9[2], testData9[3], testData9[4],
                testData9[5], testData9[6], testData9[7], toInsert, testData9[8],
                tempToPush,
                null, null, null, null, null, null, null, null, null
            };

            Assert.Equal(11, opd.ItemCount);
            Helper.ComparePrivateField(opd, "holder", expHolder2);
        }


        public static IEnumerable<object[]> GetInsertMultipleCases()
        {
            // (byte[][] dataToUse, byte[][] dataToInsert, int index, byte[][] expData, int expCount)
            yield return new object[]
            {
                null,
                new byte[2][] { new byte[] { 3 }, new byte[] { 1, 2 } },
                0,
                new byte[10][]
                {
                    new byte[] { 3 }, new byte[] { 1, 2 }, null, null, null, null, null, null, null, null
                },
                2
            };
            yield return new object[]
            {
                new byte[3][] { new byte[] { 5 }, new byte[] { 7, 8 }, new byte[] { 255 } },
                new byte[2][] { new byte[] { 3 }, new byte[] { 1, 2 } },
                0,
                new byte[10][]
                {
                    new byte[] { 5 }, new byte[] { 7, 8 }, new byte[] { 255 },
                    new byte[] { 3 }, new byte[] { 1, 2 }, null, null, null, null, null
                },
                5
            };
            yield return new object[]
            {
                new byte[3][] { new byte[] { 5 }, new byte[] { 7, 8 }, new byte[] { 255 } },
                new byte[2][] { new byte[] { 3 }, new byte[] { 1, 2 } },
                1,
                new byte[10][]
                {
                    new byte[] { 5 }, new byte[] { 7, 8 },
                    new byte[] { 3 }, new byte[] { 1, 2 },
                    new byte[] { 255 }, null, null, null, null, null
                },
                5
            };
            yield return new object[]
            {
                new byte[3][] { new byte[] { 5 }, new byte[] { 7, 8 }, new byte[] { 255 } },
                new byte[2][] { new byte[] { 3 }, new byte[] { 1, 2 } },
                3,
                new byte[10][]
                {
                    new byte[] { 3 }, new byte[] { 1, 2 },
                    new byte[] { 5 }, new byte[] { 7, 8 }, new byte[] { 255 }, null, null, null, null, null
                },
                5
            };
            yield return new object[]
            {
                testData9,
                testData5,
                0,
                new byte[24][]
                {
                    testData9[0], testData9[1], testData9[2], testData9[3], testData9[4],
                    testData9[5], testData9[6], testData9[7], testData9[8],
                    testData5[0], testData5[1], testData5[2], testData5[3], testData5[4],
                    null, null, null, null, null, null, null, null, null, null
                },
                14
            };
            yield return new object[]
            {
                testData9,
                testData5,
                1,
                new byte[24][]
                {
                    testData9[0], testData9[1], testData9[2], testData9[3], testData9[4],
                    testData9[5], testData9[6], testData9[7],
                    testData5[0], testData5[1], testData5[2], testData5[3], testData5[4],
                    testData9[8],
                    null, null, null, null, null, null, null, null, null, null
                },
                14
            };
            yield return new object[]
            {
                testData9,
                testData5,
                8,
                new byte[24][]
                {
                    testData9[0],
                    testData5[0], testData5[1], testData5[2], testData5[3], testData5[4],
                    testData9[1], testData9[2], testData9[3], testData9[4],
                    testData9[5], testData9[6], testData9[7], testData9[8],
                    null, null, null, null, null, null, null, null, null, null
                },
                14
            };
            yield return new object[]
            {
                testData9,
                testData5,
                9,
                new byte[24][]
                {
                    testData5[0], testData5[1], testData5[2], testData5[3], testData5[4],
                    testData9[0], testData9[1], testData9[2], testData9[3], testData9[4],
                    testData9[5], testData9[6], testData9[7], testData9[8],
                    null, null, null, null, null, null, null, null, null, null
                },
                14
            };
        }
        [Theory]
        [MemberData(nameof(GetInsertMultipleCases))]
        public void Insert_MultipleTest(byte[][] dataToUse, byte[][] dataToInsert, int index, byte[][] expData, int expCount)
        {
            OpData opd = new OpData(dataToUse);
            opd.Insert(dataToInsert, index);

            Helper.ComparePrivateField(opd, "holder", expData);
            Assert.Equal(expCount, opd.ItemCount);
        }


        [Fact]
        public void AltPushPopTest()
        {
            OpData opd = new OpData(null);

            Assert.Equal(0, opd.AltItemCount);

            byte[] dataToPush = new byte[3] { 1, 2, 3 };
            byte[] dataToPush2 = new byte[2] { 4, 5 };
            byte[][] expectedHolder = new byte[10][]
            {
                new byte[3] { 1, 2, 3 }, null, null, null, null, null, null, null, null, null
            };
            opd.AltPush(dataToPush);

            Assert.Equal(1, opd.AltItemCount);
            Helper.ComparePrivateField(opd, "altHolder", expectedHolder);

            opd.AltPush(dataToPush2);
            expectedHolder[1] = new byte[2] { 4, 5 };

            Assert.Equal(2, opd.AltItemCount);
            Helper.ComparePrivateField(opd, "altHolder", expectedHolder);

            byte[] actualPopped = opd.AltPop();
            byte[] expectedPopped = new byte[2] { 4, 5 };

            Assert.Equal(1, opd.AltItemCount);
            Assert.Equal(expectedPopped, actualPopped);
        }

        [Fact]
        public void AltPush_OverflowTest()
        {
            OpData opd = new OpData(null);
            byte[] repeat = new byte[1] { 5 };
            for (int i = 0; i < DefaultCapacity; i++)
            {
                opd.AltPush(repeat);
            }

            byte[][] expectedHolder = new byte[10][]
            {
                repeat, repeat, repeat, repeat, repeat, repeat, repeat, repeat, repeat, repeat
            };

            Assert.Equal(10, opd.AltItemCount);
            Helper.ComparePrivateField(opd, "altHolder", expectedHolder);

            byte[] dataToPush = new byte[3] { 1, 2, 3 };
            opd.AltPush(dataToPush);

            byte[][] expectedHolder2 = new byte[20][]
            {
                repeat, repeat, repeat, repeat, repeat, repeat, repeat, repeat, repeat, repeat,
                new byte[3] { 1, 2, 3 },
                null, null, null, null, null, null, null, null, null
            };

            Assert.Equal(11, opd.AltItemCount);
            Helper.ComparePrivateField(opd, "altHolder", expectedHolder2);
        }
    }
}
