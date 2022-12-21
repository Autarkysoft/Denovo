// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CheckSigOpTests
    {
        public static IEnumerable<object[]> GetRunCases()
        {
            yield return new object[]
            {
                // Both sig and pubkey are correct and ECDSA verification (performed by dependancy) passes
                Helper.ShortSig1, // Parsed signature
                KeyHelper.Pub1, // Parsed pubkey
                Helper.ShortSig1Bytes, // Same signature's byte array to be deleted from the executing script
                true, // ECDSA verification result
                true, // BIP-66
                new byte[][] { Helper.ShortSig1Bytes, KeyHelper.Pub1CompBytes }, // PopMulti data
                true // Push result
            };
            yield return new object[]
            {
                // Same as above but ECDSA verification fails
                Helper.ShortSig1,
                KeyHelper.Pub1,
                Helper.ShortSig1Bytes,
                false,
                true,
                new byte[][] { Helper.ShortSig1Bytes, KeyHelper.Pub1CompBytes },
                false
            };
            yield return new object[]
            {
                // Signature bytes are invalid (empty bytes) but the execution must not fail
                // Also the IOpData.Verify() should not be called since it is pointless (the 3 first nulls make sure of that).
                // Instead the result (OP_False) should be pushed to the stack.
                null,
                null,
                null,
                true,
                false, // pre BIP-66
                new byte[][] { Array.Empty<byte>(), KeyHelper.Pub1CompBytes },
                false
            };
            yield return new object[]
            {
                // Same as above but with strict DER encoding enforcement
                null,
                null,
                null,
                true,
                true, // after BIP-66
                new byte[][] { Array.Empty<byte>(), KeyHelper.Pub1CompBytes },
                false
            };
            yield return new object[]
            {
                // Same as above but this time public key is invalid
                null,
                null,
                null,
                true,
                false,
                new byte[][] { Helper.ShortSig1Bytes, Array.Empty<byte>() },
                false
            };
            yield return new object[]
            {
                // Same as above but this time public key is invalid
                null,
                null,
                null,
                true,
                true,
                new byte[][] { Helper.ShortSig1Bytes, Array.Empty<byte>() },
                false
            };
            yield return new object[]
            {
                // Same as above but this time public key is invalid
                null,
                null,
                null,
                true,
                false,
                new byte[][] { Helper.ShortSig1Bytes, new byte[] { 1, 2, 3 } },
                false
            };
            yield return new object[]
            {
                // Same as above but this time public key is invalid
                null,
                null,
                null,
                true,
                true,
                new byte[][] { Helper.ShortSig1Bytes, new byte[] { 1, 2, 3 } },
                false
            };
        }
        [Theory]
        [MemberData(nameof(GetRunCases))]
        public void RunTest(Signature expSig, in Point expPub, byte[] expSigBa, bool success, bool der, byte[][] pop, bool expBool)
        {
            MockOpData data = new(FuncCallName.PopCount, FuncCallName.PushBool)
            {
                _itemCount = 2,
                expectedSig = expSig,
                expectedPubkey = expPub,
                expectedSigBa = expSigBa,
                sigVerificationSuccess = success,
                IsStrictDerSig = der,
                popCountData = new byte[][][] { pop },
                pushBool = expBool
            };

            OpTestCaseHelper.RunTest<CheckSigOp>(data, OP.CheckSig);
        }


        public static IEnumerable<object[]> GetErrorCases()
        {
            yield return new object[] { null, 1, false, null, Errors.NotEnoughStackItems };
            yield return new object[] { null, 1, true, null, Errors.NotEnoughStackItems };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                2,
                true,
                new byte[][][] { new byte[][] { new byte[] { 1, 2, 3 }, Array.Empty<byte>() } },
                Errors.InvalidDerEncodingLength
            };
        }
        [Theory]
        [MemberData(nameof(GetErrorCases))]
        public void Run_ErrorTest(FuncCallName[] expFuncCalls, int count, bool strict, byte[][][] expPopData, Errors expErr)
        {
            MockOpData data = new(expFuncCalls)
            {
                _itemCount = count,
                popCountData = expPopData,
                IsStrictDerSig = strict
            };

            OpTestCaseHelper.RunFailTest<CheckSigOp>(data, expErr);
        }
    }
}
