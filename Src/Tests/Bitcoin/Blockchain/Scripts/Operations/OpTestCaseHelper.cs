﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public enum FuncCallName
    {
        Peek,
        PeekCount,
        PeekIndex,
        Pop,
        PopCount,
        PopIndex,
        Push,
        PushMulti,
        PushBool,
        Insert,
        InsertMulti,
        AltPop,
        AltPush
    }


    internal class OpTestCaseHelper
    {
        public static byte[] b1 = { 1, 10, 100 };
        public static byte[] b2 = { 2, 20, 200 };
        public static byte[] b3 = { 3, 31, 33 };
        public static byte[] b4 = { 4, 41, 44 };
        public static byte[] b5 = { 5, 51, 55 };
        public static byte[] b6 = { 6, 61, 66 };
        public static byte[] b7 = { 7, 71, 76 };
        public static byte[] numNeg1 = { 129 }; // 0b10000001
        public static byte[] num0 = Array.Empty<byte>();
        public static byte[] num1 = { 1 };
        public static byte[] num2 = { 2 };
        public static byte[] num3 = { 3 };
        public static byte[] num4 = { 4 };
        public static byte[] num5 = { 5 };
        public static byte[] num6 = { 6 };
        public static byte[] num7 = { 7 };
        public static byte[] num8 = { 8 };
        public static byte[] num16 = { 16 };
        public static byte[] num17 = { 17 };
        public static byte[] maxInt = { 255, 255, 255, 127 }; // int.MaxValue in little endian order
        public static byte[] maxIntPlus1 = { 0, 0, 0, 0x80, 0 }; // int.MaxValue + 1
        public static byte[] maxIntPlus2 = { 1, 0, 0, 0x80, 0 }; // int.MaxValue + 2
        public static byte[] maxNegInt = { 255, 255, 255, 255 }; // -int.MaxValue in little endian order

        public static byte[] TrueBytes = { 1 }; // OP_TRUE = 0x51
        public static byte[] FalseBytes = Array.Empty<byte>();
        public static byte[] FalseBytes_alt1 = { 0x80 };
        public static byte[] FalseBytes_alt2 = { 0x80, 0, 0 };


        internal static void RunTest<T>(MockOpData data, OP expOpVal) where T : IOperation, new()
        {
            T op = new();
            bool b = op.Run(data, out Errors error);
            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(expOpVal, op.OpValue);
        }

        internal static void RunFailTest<T>(MockOpData data, Errors expError) where T : IOperation, new()
        {
            T op = new();
            bool b = op.Run(data, out Errors error);
            Assert.False(b);
            Assert.Equal(expError, error);
        }
    }
}
