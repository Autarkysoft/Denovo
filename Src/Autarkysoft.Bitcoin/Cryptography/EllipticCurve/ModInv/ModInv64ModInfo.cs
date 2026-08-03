// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve.ModInv
{
    public readonly struct ModInv64ModInfo
    {
        public ModInv64ModInfo(in ModInv64Signed62 mod, ulong modinv62)
        {
            modulus = mod;
            modulus_inv62 = modinv62;
        }

        /// <summary>
        /// The modulus in signed62 notation, must be odd and in [3, 2^256].
        /// </summary>
        public readonly ModInv64Signed62 modulus;
        /// <summary>
        /// modulus^{-1} mod 2^62
        /// </summary>
        public readonly ulong modulus_inv62;


        private static readonly ModInv64ModInfo _scConst = new ModInv64ModInfo(
            new ModInv64Signed62(0x3FD25E8CD0364141L, 0x2ABB739ABD2280EEL, -0x15L, 0, 256),
            0x34F20099AA774EC1L);
        /// <summary>
        /// secp256k1_const_modinfo_scalar
        /// </summary>
        internal static ref readonly ModInv64ModInfo ScalarConstant => ref _scConst;

        private static readonly ModInv64ModInfo _feConst = new ModInv64ModInfo(
            new ModInv64Signed62(-0x1000003D1L, 0, 0, 0, 256),
            0x27C7F6E22DDACACFL);
        /// <summary>
        /// secp256k1_const_modinfo_fe
        /// </summary>
        internal static ref readonly ModInv64ModInfo FeConstant => ref _feConst;
    }

    /// <summary>
    /// Data type for transition matrices (see section 3 of explanation).
    /// t = [ u  v ]
    ///     [ q  r ]
    /// </summary>
    internal readonly struct ModInv64Trans2x2
    {
        internal ModInv64Trans2x2(ulong u, ulong v, ulong q, ulong r)
        {
            this.u = (long)u;
            this.v = (long)v;
            this.q = (long)q;
            this.r = (long)r;
        }
        internal ModInv64Trans2x2(long u, long v, long q, long r)
        {
            this.u = u;
            this.v = v;
            this.q = q;
            this.r = r;
        }

        internal readonly long u, v, q, r;
    }
}
