// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Globalization;
using System.IO;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class PrecomputeEcMultTests
    {
        private static void VerifyTable(PointStorage[] table, StreamReader reader)
        {
            Assert.Equal(PrecomputeEcMult.TableSize, table.Length);

            int i;
            for (i = 0; i < table.Length; i++)
            {
                ReadOnlySpan<char> line = reader.ReadLine();
                while (!line.StartsWith(" S(") && !line.StartsWith(",S("))
                {
                    line = reader.ReadLine();
                }
                line = line[3..];

                uint[] expected = new uint[16];
                for (int j = 0; j < expected.Length; j++)
                {
                    int count = line.IndexOf(j == expected.Length - 1 ? ')' : ',');
                    ReadOnlySpan<char> num = line.Slice(0, count);
                    expected[j] = uint.Parse(num, NumberStyles.HexNumber);
                    line = line[(count + 1)..];
                }

                //UInt256_4x64 exp = UInt256_4x64Tests.SECP256K1_FE_STORAGE_CONST(expected[0], expected[1], expected[2], expected[3], expected[4], expected[5], expected[6], expected[7]);
                Point point = PointTests.SECP256K1_GE_CONST(expected[0], expected[1], expected[2], expected[3], expected[4], expected[5], expected[6], expected[7], expected[8], expected[9], expected[10], expected[11], expected[12], expected[13], expected[14], expected[15]);

                PointStorage exp = point.ToStorage();

                Assert.Equal(exp.x.b0, table[i].x.b0);
                Assert.Equal(exp.x.b1, table[i].x.b1);
                Assert.Equal(exp.x.b2, table[i].x.b2);
                Assert.Equal(exp.x.b3, table[i].x.b3);

                Assert.Equal(exp.y.b0, table[i].y.b0);
                Assert.Equal(exp.y.b1, table[i].y.b1);
                Assert.Equal(exp.y.b2, table[i].y.b2);
                Assert.Equal(exp.y.b3, table[i].y.b3);
            }

            Assert.Equal(PrecomputeEcMult.TableSize, i);
        }

        [Fact]
        public void Verify()
        {
            PrecomputeEcMult.BuildTables(out PointStorage[] table, out PointStorage[] table128);
            // https://github.com/bitcoin-core/secp256k1/blob/694ce8fb2d1fd8a3d641d7c33705691d41a2a860/src/precomputed_ecmult.c
            using Stream stream = Helper.ReadResourceAsStream("precomputed_ecmult", "txt");
            using StreamReader reader = new(stream);
            VerifyTable(table, reader);
            VerifyTable(table128, reader);
        }
    }
}
