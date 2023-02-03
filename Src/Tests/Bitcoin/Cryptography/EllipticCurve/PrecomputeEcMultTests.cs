// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Globalization;
using System.IO;
using Xunit;

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

                Assert.Equal(expected[7], table[i].x.b0);
                Assert.Equal(expected[6], table[i].x.b1);
                Assert.Equal(expected[5], table[i].x.b2);
                Assert.Equal(expected[4], table[i].x.b3);
                Assert.Equal(expected[3], table[i].x.b4);
                Assert.Equal(expected[2], table[i].x.b5);
                Assert.Equal(expected[1], table[i].x.b6);
                Assert.Equal(expected[0], table[i].x.b7);

                Assert.Equal(expected[15], table[i].y.b0);
                Assert.Equal(expected[14], table[i].y.b1);
                Assert.Equal(expected[13], table[i].y.b2);
                Assert.Equal(expected[12], table[i].y.b3);
                Assert.Equal(expected[11], table[i].y.b4);
                Assert.Equal(expected[10], table[i].y.b5);
                Assert.Equal(expected[9], table[i].y.b6);
                Assert.Equal(expected[8], table[i].y.b7);
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
