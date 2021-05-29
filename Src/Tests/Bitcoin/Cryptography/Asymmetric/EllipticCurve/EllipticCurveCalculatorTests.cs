//// Autarkysoft Tests
//// Copyright (c) 2020 Autarkysoft
//// Distributed under the MIT software license, see the accompanying
//// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

//using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
//using System.Collections.Generic;
//using Xunit;

//namespace Tests.Bitcoin.Cryptography.Asymmetric.EllipticCurve
//{
//    public class EllipticCurveCalculatorTests
//    {
//        private readonly EllipticCurveCalculator calc = new EllipticCurveCalculator();


//        // The following tests are from https://github.com/bitcoin/bips/blob/master/bip-0340/test-vectors.csv
//        // k values are calculated manually.
//        public static IEnumerable<object[]> GetSchnorrCases()
//        {
//            yield return new object[]
//            {
//                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000000"),
//                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001"),
//                // Helper.HexToBigInt("de2b58ff3b8b5d424a64529bfe10f87d35d5c3322a0c7f592cf30d4df9bbdd0c"), //=k
//                new Signature()
//                {
//                    R = Helper.HexToBigInt("528f745793e8472c0329742a463f59e58f3a3f1a4ac09c28f6f8514d4d0322a2"),
//                    S = Helper.HexToBigInt("58bd08398f82cf67b812ab2c7717ce566f877c2f8795c846146978e8f04782ae")
//                }
//            };
//            yield return new object[]
//            {
//                Helper.HexToBytes("243f6a8885a308d313198a2e03707344a4093822299f31d0082efa98ec4e6c89"),
//                Helper.HexToBytes("b7e151628aed2a6abf7158809cf4f3c762e7160f38b4da56a784d9045190cfef"),
//                //Helper.HexToBigInt("ff14fa3f8f7322afead018d96bea1a86ccb68f9972f9d21982465b0541a05960"),
//                new Signature()
//                {
//                    R = Helper.HexToBigInt("667c2f778e0616e611bd0c14b8a600c5884551701a949ef0ebfd72d452d64e84"),
//                    S = Helper.HexToBigInt("4160bcfc3f466ecb8facd19ade57d8699d74e7207d78c6aedc3799b52a8e0598")
//                }
//            };
//            yield return new object[]
//            {
//                Helper.HexToBytes("5e2d58d8b3bcdf1abadec7829054f90dda9805aab56c77333024b9d0a508b75c"),
//                Helper.HexToBytes("c90fdaa22168c234c4c6628b80dc1cd129024e088a67cc74020bbea63b14e5c9"),
//                //Helper.HexToBigInt("853535959a8f822991f34e0afd62e45e91a5d5d5a1c03b03135ecedf60df1faa"),
//                new Signature()
//                {
//                    R = Helper.HexToBigInt("2d941b38e32624bf0ac7669c0971b990994af6f9b18426bf4f4e7ec10e6cdf38"),
//                    S = Helper.HexToBigInt("6cf646c6ddafcfa7f1993eeb2e4d66416aead1ddae2f22d63cad901412d116c6")
//                }
//            };
//            yield return new object[]
//            {
//                Helper.HexToBytes("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"),
//                Helper.HexToBytes("0b432b2677937381aef05bb02a66ecd012773062cf3fa2549e44f58ed2401710"),
//                //Helper.HexToBigInt("4ae025f4eef2850342814f2aec0fd90e3f63793381ea3d2e1086f5206450d887"),
//                new Signature()
//                {
//                    R = Helper.HexToBigInt("8bd2c11604b0a87a443fcc2e5d90e5328f934161b18864fb48ce10cb59b45fb9"),
//                    S = Helper.HexToBigInt("b5b2a0f129bd88f5bdc05d5c21e5c57176b913002335784f9777a24bd317cd36")
//                }
//            };
//        }
//        [Theory]
//        [MemberData(nameof(GetSchnorrCases))]
//        public void SignSchnorrTest(byte[] hash, byte[] key, Signature expectedSig)
//        {
//            Signature actualSig = calc.SignSchnorr(hash, key);

//            Assert.Equal(expectedSig.R, actualSig.R);
//            Assert.Equal(expectedSig.S, actualSig.S);
//        }
//    }
//}
