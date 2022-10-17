// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    internal class StraussState
    {
        public StraussState(int size)
        {
            aux = new UInt256_10x26[size];
            preA = new Point[size];
            ps = new StraussPointState[1] { new StraussPointState() };
        }

        public StraussState(UInt256_10x26[] aux, Point[] preA, StraussPointState[] ps)
        {
            this.aux = aux;
            this.preA = preA;
            this.ps = ps;
        }


        // aux is used to hold z-ratios, and then used to hold pre_a[i].x * BETA values.
        public UInt256_10x26[] aux;
        public Point[] preA;
        public StraussPointState[] ps;
    }


    internal class StraussPointState
    {
        public int[] wnafNa1 = new int[129];
        public int[] wnafNaLam = new int[129];
        public int bitsNa1;
        public int bitsNaLam;
    }
}
