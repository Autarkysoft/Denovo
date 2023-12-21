// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    public readonly struct ModInv32ModInfo
    {
        public ModInv32ModInfo(in ModInv32Signed30 mod, uint modinv30)
        {
            modulus = mod;
            modulus_inv30 = modinv30;
        }

        public readonly ModInv32Signed30 modulus;
        public readonly uint modulus_inv30;

        private static readonly ModInv32ModInfo _const = new ModInv32ModInfo(
            new ModInv32Signed30(0x10364141, 0x3F497A33, 0x348A03BB, 0x2BB739AB, -0x146, 0, 0, 0, 65536),
            0x2A774EC1U);
        // secp256k1_const_modinfo_scalar
        internal static ref readonly ModInv32ModInfo Constant => ref _const;
    }

    internal class secp256k1_modinv32_trans2x2
    {
        internal secp256k1_modinv32_trans2x2(uint u, uint v, uint q, uint r)
        {
            this.u = (int)u;
            this.v = (int)v;
            this.q = (int)q;
            this.r = (int)r;
        }
        internal secp256k1_modinv32_trans2x2(int u, int v, int q, int r)
        {
            this.u = u;
            this.v = v;
            this.q = q;
            this.r = r;
        }

        internal int u, v, q, r;
    }
}
