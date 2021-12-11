// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class MockOpData : IOpData
    {
#pragma warning disable CS0649 // Field is never assigned to

        public MockOpData(params FuncCallName[] calls)
        {
            funcOrder = calls;
            totalFuncCall = (calls == null) ? 0 : calls.Length;
            currentCallIndex = 0;
        }



        private readonly FuncCallName[] funcOrder;
        private readonly int totalFuncCall;
        private int currentCallIndex;


        public bool StrictNumberEncoding { get; set; }

        internal bool sigVerificationSuccess = true;

        internal Signature expectedSig;
        internal PublicKey expectedPubkey;
        internal byte[] expectedSigBa;
        public bool Verify(Signature sig, PublicKey pubKey, ReadOnlySpan<byte> sigBa)
        {
            if (expectedSig is null || expectedPubkey is null || expectedSigBa is null)
            {
                Assert.True(false, "Expected signature and/or public key must be set first.");
            }

            Assert.Equal(expectedSig.R, sig.R);
            Assert.Equal(expectedSig.S, sig.S);
            Assert.Equal(expectedSig.SigHash, sig.SigHash);
            Assert.Equal(expectedPubkey.ToByteArray(true), pubKey.ToByteArray(true));
            Assert.True(sigBa.SequenceEqual(expectedSigBa));

            return sigVerificationSuccess;
        }

        internal byte[][] expectedSigs;
        internal byte[][] expectedPubkeys;

        public bool Verify(byte[][] sigs, byte[][] pubKeys, int m, out string error)
        {
            if (expectedSigs is null || expectedPubkeys is null)
            {
                Assert.True(false, "Expected signature list and/or public key list must be set first.");
            }

            Assert.Equal(expectedSigs, sigs);
            Assert.Equal(expectedPubkeys, pubKeys);

            error = null;
            return sigVerificationSuccess;
        }

        internal byte[] expectedMultiSigGarbage;
        internal bool garbageCheckResult = true;
        public bool CheckMultiSigGarbage(byte[] garbage)
        {
            Assert.Equal(expectedMultiSigGarbage, garbage);
            return garbageCheckResult;
        }

        internal byte[] expectedConditionalBoolBytes;
        internal bool conditionalBoolCheckResult = true;
        public bool CheckConditionalOpBool(byte[] data)
        {
            Assert.Equal(expectedConditionalBoolBytes, data);
            return conditionalBoolCheckResult;
        }

        public bool IsStrictDerSig { get; set; } = true;


        internal bool bip65;
        public bool IsBip65Enabled
        {
            get => bip65;
            set => throw new Exception();
        }

        internal long expectedLocktime;
        internal bool locktimeVerificationSuccess;
        public bool CompareLocktimes(long other, out string error)
        {
            Assert.Equal(expectedLocktime, other);
            if (locktimeVerificationSuccess)
            {
                error = null;
                return true;
            }
            else
            {
                error = "Foo";
                return false;
            }
        }

        internal bool bip112;
        public bool IsBip112Enabled
        {
            get => bip112;
            set => throw new Exception();
        }

        internal long expectedSequence;
        internal bool SequenceVerificationSuccess;
        public bool CompareSequences(long other, out string error)
        {
            Assert.Equal(expectedSequence, other);
            if (SequenceVerificationSuccess)
            {
                error = null;
                return true;
            }
            else
            {
                error = "Foo";
                return false;
            }
        }

        internal int _opCountToReturn = -1;
        public int OpCount
        {
            get
            {
                if (_opCountToReturn < 0)
                {
                    Assert.True(false, $"Unexpected call to OpCount.Get() was detected " +
                                       $"({nameof(_opCountToReturn)} field is not set).");
                }
                return _opCountToReturn;
            }
            set
            {
                _opCountToReturn = value;
            }
        }


        private void CheckCall(FuncCallName funcName)
        {
            if (currentCallIndex + 1 > totalFuncCall)
            {
                Assert.True(false, $"An extra {funcName} call was made.");
            }
            if (funcOrder[currentCallIndex] != funcName)
            {
                Assert.True(false, $"{funcName} was called instead of {funcOrder[currentCallIndex]}.");
            }

            currentCallIndex++;
        }



        internal int _itemCount = -1;
        public int ItemCount
        {
            get
            {
                if (_itemCount == -1)
                {
                    Assert.True(false, "Item count was not set by test case but the method being tested called its get().");
                }

                return _itemCount;
            }
        }

        internal int _altItemCount = -1;
        public int AltItemCount
        {
            get
            {
                if (_altItemCount == -1)
                {
                    Assert.True(false, "Alt item count was not set by test case but the method being tested called its get().");
                }

                return _altItemCount;
            }
        }

        public byte[] AnnexHash { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte[] TapLeafHash { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int SigOpLimitLeft { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public uint CodeSeparatorPosition { get; set; }

        internal byte[][] altPopData;
        private int altPopDataIndex;
        public byte[] AltPop()
        {
            CheckCall(FuncCallName.AltPop);
            if (altPopData == null || altPopDataIndex > altPopData.Length - 1)
            {
                Assert.True(false, "Unexpected alt data pop request was made.");
            }

            return altPopData[altPopData.Length - ++altPopDataIndex];
        }


        internal byte[][] altPushData;
        private int altPushDataIndex;
        public void AltPush(byte[] data)
        {
            CheckCall(FuncCallName.AltPush);
            if (altPushData == null || altPushDataIndex > altPushData.Length - 1)
            {
                Assert.True(false, "Unexpected data was pushed to stack.");
            }
            else
            {
                Assert.Equal(altPushData[altPushDataIndex], data);
                altPushDataIndex++;
            }
        }


        internal Dictionary<int, byte[]> insertData;
        public void Insert(byte[] data, int index)
        {
            CheckCall(FuncCallName.Insert);
            if (insertData == null || !insertData.ContainsKey(index))
            {
                Assert.True(false, "Unexpected data was inserted in stack.");
            }
            else
            {
                Assert.Equal(insertData[index], data);
            }
        }

        internal Dictionary<int, byte[][]> insertMultiData;
        public void Insert(byte[][] data, int index)
        {
            CheckCall(FuncCallName.InsertMulti);
            if (insertMultiData == null || !insertMultiData.ContainsKey(index))
            {
                Assert.True(false, "Unexpected multiple data was inserted in stack.");
            }
            else
            {
                Assert.Equal(insertMultiData[index], data);
            }
        }


        internal byte[][] peekData;
        private int peekDataIndex;
        public byte[] Peek()
        {
            CheckCall(FuncCallName.Peek);
            if (peekData == null || peekDataIndex > peekData.Length - 1)
            {
                Assert.True(false, "Unexpected data peek request was made.");
            }

            return peekData[peekData.Length - ++peekDataIndex];
        }


        internal byte[][][] peekCountData;
        private int peekCountDataIndex;
        public byte[][] Peek(int count)
        {
            CheckCall(FuncCallName.PeekCount);
            if (peekCountData == null || peekCountDataIndex > peekCountData.Length - 1)
            {
                Assert.True(false, "Unexpected data peek count request was made.");
            }

            byte[][] result = peekCountData[peekCountDataIndex++];
            Assert.Equal(count, result.Length);
            return result;
        }


        internal Dictionary<int, byte[]> peekIndexData;
        public byte[] PeekAtIndex(int index)
        {
            CheckCall(FuncCallName.PeekIndex);
            if (peekIndexData == null || !peekIndexData.ContainsKey(index))
            {
                Assert.True(false, "Unexpected peek index data request was made.");
            }

            return peekIndexData[index];
        }


        internal byte[][] popData;
        private int popDataIndex;
        public byte[] Pop()
        {
            CheckCall(FuncCallName.Pop);
            if (popData == null || popDataIndex > popData.Length - 1)
            {
                Assert.True(false, "Unexpected data pop request was made.");
            }

            return popData[popData.Length - ++popDataIndex];
        }


        internal byte[][][] popCountData;
        private int popCountDataIndex;
        public byte[][] Pop(int count)
        {
            CheckCall(FuncCallName.PopCount);
            if (popCountData == null || popCountDataIndex > popCountData.Length - 1)
            {
                Assert.True(false, "Unexpected data pop count request was made.");
            }

            byte[][] result = popCountData[popCountDataIndex++];
            Assert.Equal(count, result.Length);
            return result;
        }


        internal Dictionary<int, byte[]> popIndexData;
        public byte[] PopAtIndex(int index)
        {
            CheckCall(FuncCallName.PopIndex);
            if (popIndexData == null || !popIndexData.ContainsKey(index))
            {
                Assert.True(false, "Unexpected data pop index request was made.");
            }

            return popIndexData[index];
        }


        internal bool? pushBool;
        public void Push(bool b)
        {
            CheckCall(FuncCallName.PushBool);
            if (pushBool == null)
            {
                Assert.True(false, "Unexpected bool was pushed to stack.");
            }
            else
            {
                Assert.Equal((bool)pushBool, b);
            }
        }


        internal byte[][] pushData;
        private int pushDataIndex;
        public void Push(byte[] data)
        {
            CheckCall(FuncCallName.Push);
            if (pushData == null || pushDataIndex > pushData.Length - 1)
            {
                Assert.True(false, "Unexpected data was pushed to stack.");
            }
            else
            {
                Assert.Equal(pushData[pushDataIndex], data);
                pushDataIndex++;
            }
        }


        internal byte[][][] pushMultiData;
        private int pushMultiDataIndex;
        public void Push(byte[][] data)
        {
            CheckCall(FuncCallName.PushMulti);
            if (pushMultiData == null || pushMultiDataIndex > pushMultiData.Length - 1)
            {
                Assert.True(false, "Unexpected multi data was pushed to stack.");
            }
            else
            {
                Assert.Equal(pushMultiData[pushMultiDataIndex], data);
                pushMultiDataIndex++;
            }
        }

#pragma warning restore CS0649 // Field is never assigned to
    }
}
