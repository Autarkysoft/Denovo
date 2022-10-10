// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// Contains odd multiples of the base point G in table and odd multiples of 2^128* G in table128
    /// for accelerating the computation of a*P + b*G.
    /// </summary>
    public static class PrecomputeEcMult
    {
        /// <summary>
        /// 
        /// </summary>
        public const int WindowA = 5;
        /// <summary>
        /// Window size
        /// </summary>
        public const int WindowSize = 15;
        /// <summary>
        /// The number of entries a table with precomputed multiples needs to have (8191)
        /// </summary>
        public const int TableSize = 1 << (WindowSize - 2);


        private static void ComputeTable(PointStorage[] table, in PointJacobian gen)
        {
            PointJacobian gj = gen;
            Point ge = gj.ToPointVar();
            table[0] = ge.ToStorage();

            gj = gen.DoubleVar(out _);
            Point dgen = gj.ToPointVar();

            for (int j = 1; j < table.Length; j++)
            {
                gj = ge.ToPointJacobian();
                gj = gj.AddVar(dgen, out _);
                ge = gj.ToPointVar();
                table[j] = ge.ToStorage();
            }
        }

        /// <summary>
        /// Builds arrays containing odd multiples of the base point G in <paramref name="table"/>
        /// and odd multiples of 2^128* G in <paramref name="table128"/> for accelerating the computation of a*P + b*G.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="table128"></param>
        public static void BuildTables(out PointStorage[] table, out PointStorage[] table128)
        {
            table = new PointStorage[TableSize];
            table128 = new PointStorage[TableSize];

            PointJacobian gj = Point.G.ToPointJacobian();

            ComputeTable(table, gj);
            for (int i = 0; i < 128; ++i)
            {
                gj = gj.DoubleVar(out _);
            }
            ComputeTable(table128, gj);
        }
    }
}
