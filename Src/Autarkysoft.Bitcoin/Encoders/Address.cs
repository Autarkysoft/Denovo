// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;

namespace Autarkysoft.Bitcoin.Encoders
{
    /// <summary>
    /// Provides methods to check and create bitcoin addresses from public keys.
    /// </summary>
    public static class Address
    {
        private const byte P2pkhVerMainNet = 0;
        private const byte P2pkhVerTestNet = 111;
        private const byte P2pkhVerTestNet4 = 111;
        private const byte P2pkhVerRegTest = 111;

        private const byte P2shVerMainNet = 5;
        private const byte P2shVerTestNet = 196;
        private const byte P2shVerTestNet4 = 196;
        private const byte P2shVerRegTest = 196;

        private const string HrpMainNet = "bc";
        private const string HrpTestNet = "tb";
        private const string HrpTestNet4 = "tb";
        private const string HrpRegTest = "bcrt";


        /// <summary>
        /// Returns the type of the given address
        /// </summary>
        /// <param name="address">Address string to check</param>
        /// <param name="netType">Network type</param>
        /// <param name="data">
        /// Decoded data or hash extracted from the given address 
        /// (null for <see cref="AddressType.Unknown"/> and <see cref="AddressType.Invalid"/> tyepes)
        /// </param>
        /// <returns>Address type</returns>
        public static AddressType GetAddressType(string address, NetworkType netType, out byte[] data)
        {
            data = null;
            if (string.IsNullOrWhiteSpace(address))
            {
                return AddressType.Invalid;
            }

            // Address has to be checked by all encodings so that in case TryDecode passed for an unintended encoding
            // (eg. Bech32 address was valid Base58Check!) the evaluation still returns the correct type
            if (Base58.TryDecodeWithChecksum(address, out byte[] decoded))
            {
                if (decoded.Length == 21)
                {
                    if ((netType == NetworkType.MainNet && decoded[0] == P2pkhVerMainNet) ||
                        (netType == NetworkType.TestNet && decoded[0] == P2pkhVerTestNet) ||
                        (netType == NetworkType.TestNet4 && decoded[0] == P2pkhVerTestNet4) ||
                        (netType == NetworkType.RegTest && decoded[0] == P2pkhVerRegTest))
                    {
                        data = decoded.SubArray(1);
                        return AddressType.P2PKH;
                    }
                    else if ((netType == NetworkType.MainNet && decoded[0] == P2shVerMainNet) ||
                             (netType == NetworkType.TestNet && decoded[0] == P2shVerTestNet) ||
                             (netType == NetworkType.TestNet4 && decoded[0] == P2shVerTestNet4) ||
                             (netType == NetworkType.RegTest && decoded[0] == P2shVerRegTest))
                    {
                        data = decoded.SubArray(1);
                        return AddressType.P2SH;
                    }
                }
            }

            if (Bech32.TryDecode(address, Bech32.Mode.B32, out decoded, out byte witVer, out string hrp))
            {
                // Bech32 (BIP173) addresses can only be P2WPKH (byte[20]) or P2WSH (byte[32]) with witness version 0
                if (witVer == 0)
                {
                    if (decoded.Length == 20 &&
                        TransactionVerifier.IsNotZero20(decoded) &&
                        ((netType == NetworkType.MainNet && hrp == HrpMainNet) ||
                         (netType == NetworkType.TestNet && hrp == HrpTestNet) ||
                         (netType == NetworkType.TestNet4 && hrp == HrpTestNet4) ||
                         (netType == NetworkType.RegTest && hrp == HrpRegTest)))
                    {
                        data = decoded;
                        return AddressType.P2WPKH;
                    }
                    else if (decoded.Length == 32 &&
                            TransactionVerifier.IsNotZero32(decoded) &&
                            ((netType == NetworkType.MainNet && hrp == HrpMainNet) ||
                             (netType == NetworkType.TestNet && hrp == HrpTestNet) ||
                             (netType == NetworkType.TestNet4 && hrp == HrpTestNet4) ||
                             (netType == NetworkType.RegTest && hrp == HrpRegTest)))
                    {
                        data = decoded;
                        return AddressType.P2WSH;
                    }
                    else
                    {
                        return AddressType.Invalid;
                    }
                }
                else // witVer != 0
                {
                    // We mandate version 1+ SegWit addresses to be encoded using BIP-350 (mode m) not BIP-173.
                    return AddressType.Invalid;
                }
            }

            if (Bech32.TryDecode(address, Bech32.Mode.B32m, out decoded, out witVer, out hrp))
            {
                if (decoded.Length < 2 || decoded.Length > 40 || witVer > 16)
                {
                    return AddressType.Invalid;
                }

                if (witVer == 0)
                {
                    // We also mandate version 0 SegWit addresses to be encoded using BIP-173 not BIP-350 (mode m).
                    // TODO: this may change in the future (let all SegWit addresses use mode m).
                    return AddressType.Invalid;
                }
                else if (witVer == 1)
                {
                    // If data length is not 32 bytes it is an unknown witness version 1 address 
                    // (not supported yet and may be added through a soft fork in the future)
                    if (decoded.Length == 32)
                    {
                        if (TransactionVerifier.IsNotZero32(decoded) &&
                            ((netType == NetworkType.MainNet && hrp == HrpMainNet) ||
                             (netType == NetworkType.TestNet && hrp == HrpTestNet) ||
                             (netType == NetworkType.TestNet4 && hrp == HrpTestNet4) ||
                             (netType == NetworkType.RegTest && hrp == HrpRegTest)))
                        {
                            data = decoded;
                            return AddressType.P2TR;
                        }
                        else
                        {
                            return AddressType.Invalid;
                        }
                    }
                }

                if (!TransactionVerifier.IsNotZero(decoded))
                {
                    return AddressType.Invalid;
                }
            }

            return AddressType.Unknown;
        }

        /// <summary>
        /// Returns the type of the given address
        /// </summary>
        /// <param name="address">Address string to check</param>
        /// <param name="netType">Network type</param>
        /// <returns>Address type</returns>
        public static AddressType GetAddressType(string address, NetworkType netType) => GetAddressType(address, netType, out _);


        /// <summary>
        /// Return the pay to public key hash address from the given <see cref="Point"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="pubk">Public key to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates wheter to use compressed or uncompressed public key to generate the address
        /// </param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public static string GetP2pkh(in Point pubk, bool useCompressed = true, NetworkType netType = NetworkType.MainNet)
        {
            byte ver = netType switch
            {
                NetworkType.MainNet => P2pkhVerMainNet,
                NetworkType.TestNet => P2pkhVerTestNet,
                NetworkType.TestNet4 => P2pkhVerTestNet4,
                NetworkType.RegTest => P2pkhVerRegTest,
                _ => throw new ArgumentException(Errors.InvalidNetwork.Convert())
            };

            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
            byte[] data = hashFunc.ComputeHash(pubk.ToByteArray(useCompressed)).AppendToBeginning(ver);

            return Base58.EncodeWithChecksum(data);
        }


        /// <summary>
        /// Return the pay to script hash address from the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="redeem">Redeem script to use</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public static string GetP2sh(IScript redeem, NetworkType netType = NetworkType.MainNet)
        {
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");

            byte ver = netType switch
            {
                NetworkType.MainNet => P2shVerMainNet,
                NetworkType.TestNet => P2shVerTestNet,
                NetworkType.TestNet4 => P2shVerTestNet4,
                NetworkType.RegTest => P2shVerRegTest,
                _ => throw new ArgumentException(Errors.InvalidNetwork.Convert())
            };

            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
            byte[] data = hashFunc.ComputeHash(redeem.Data).AppendToBeginning(ver);

            return Base58.EncodeWithChecksum(data);
        }


        /// <summary>
        /// Return the pay to witness public key hash address from the given <see cref="Point"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="pubk">Public key to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates wheter to use compressed or compressed public key to generate the address
        /// <para/>Note: using uncompressed public keys makes the output non-standard and can lead to money loss.
        /// </param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public static string GetP2wpkh(in Point pubk, bool useCompressed = true, NetworkType netType = NetworkType.MainNet)
        {
            string hrp = netType switch
            {
                NetworkType.MainNet => HrpMainNet,
                NetworkType.TestNet => HrpTestNet,
                NetworkType.TestNet4 => HrpTestNet4,
                NetworkType.RegTest => HrpRegTest,
                _ => throw new ArgumentException(Errors.InvalidNetwork.Convert()),
            };

            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
            byte[] hash160 = hashFunc.ComputeHash(pubk.ToByteArray(useCompressed));

            if (!TransactionVerifier.IsNotZero20(hash160))
                throw new ArgumentException(Err.ZeroByteWitness, nameof(hash160));

            return Bech32.Encode(hash160, Bech32.Mode.B32, 0, hrp);
        }


        /// <summary>
        /// Return the pay to witness public key hash address from the given <see cref="Point"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="pubk">Public key to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates wheter to use compressed or compressed public key to generate the address
        /// <para/>Note: using uncompressed public keys makes the output non-standard and can lead to money loss.
        /// </param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public static string GetP2sh_P2wpkh(in Point pubk, bool useCompressed = true, NetworkType netType = NetworkType.MainNet)
        {
            if (netType != NetworkType.MainNet && netType != NetworkType.TestNet &&
                netType != NetworkType.TestNet4 && netType != NetworkType.RegTest)
            {
                throw new ArgumentException(Errors.InvalidNetwork.Convert());
            }

            var rdm = new RedeemScript();
            rdm.SetToP2SH_P2WPKH(pubk, useCompressed);
            return GetP2sh(rdm, netType);
        }


        /// <summary>
        /// Return the pay to witness script hash address from the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="script">Script to use</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public static string GetP2wsh(IScript script, NetworkType netType = NetworkType.MainNet)
        {
            if (script is null)
                throw new ArgumentNullException(nameof(script), "Script can not be null.");

            string hrp = netType switch
            {
                NetworkType.MainNet => HrpMainNet,
                NetworkType.TestNet => HrpTestNet,
                NetworkType.TestNet4 => HrpTestNet4,
                NetworkType.RegTest => HrpRegTest,
                _ => throw new ArgumentException(Errors.InvalidNetwork.Convert()),
            };

            using Sha256 witHashFunc = new Sha256();
            byte[] hash = witHashFunc.ComputeHash(script.Data);

            if (!TransactionVerifier.IsNotZero32(hash))
                throw new ArgumentException(Err.ZeroByteWitness, nameof(hash));

            return Bech32.Encode(hash, Bech32.Mode.B32, 0, hrp);
        }


        /// <summary>
        /// Return the pay to witness script hash wrapped in a pay to script hash address from the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="script">Public key to use</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public static string GetP2sh_P2wsh(IScript script, NetworkType netType = NetworkType.MainNet)
        {
            if (script is null)
                throw new ArgumentNullException(nameof(script), "Script can not be null.");
            if (netType != NetworkType.MainNet && netType != NetworkType.TestNet &&
                netType != NetworkType.TestNet4 && netType != NetworkType.RegTest)
            {
                throw new ArgumentException(Errors.InvalidNetwork.Convert());
            }

            RedeemScript rdm = new RedeemScript();
            rdm.SetToP2SH_P2WSH(script);
            return GetP2sh(rdm, netType);
        }


        /// <summary>
        /// Return the pay to taproot address from the given bytes.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="data32">32 byte data to use</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public static string GetP2tr(byte[] data32, NetworkType netType = NetworkType.MainNet)
        {
            if (data32 is null)
                throw new ArgumentNullException(nameof(data32), "Data can not be null.");
            if (data32.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(data32), "Only 32 byte data is accepted.");
            if (!TransactionVerifier.IsNotZero32(data32))
                throw new ArgumentException(Err.ZeroByteWitness, nameof(data32));

            string hrp = netType switch
            {
                NetworkType.MainNet => HrpMainNet,
                NetworkType.TestNet => HrpTestNet,
                NetworkType.TestNet4 => HrpTestNet4,
                NetworkType.RegTest => HrpRegTest,
                _ => throw new ArgumentException(Errors.InvalidNetwork.Convert()),
            };

            return Bech32.Encode(data32, Bech32.Mode.B32m, 1, hrp);
        }

        ///// <summary>
        ///// Return the pay to taproot address from the given public key.
        ///// </summary>
        ///// <exception cref="ArgumentException"/>
        ///// <exception cref="ArgumentNullException"/>
        ///// <exception cref="ArgumentOutOfRangeException"/>
        ///// <param name="pub">Public key to use</param>
        ///// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        ///// <returns>The resulting address</returns>
        //public static string GetP2tr(in Point pub, NetworkType netType = NetworkType.MainNet)
        //{
        //    byte[] tweaked = pub.ToTweaked(out _);
        //    return GetP2tr(tweaked, netType);
        //}


        /// <summary>
        /// Checks if the given address string is of the given <see cref="PubkeyScriptType"/> type and returns the
        /// decoded hash from the address.
        /// </summary>
        /// <param name="address">Address to check</param>
        /// <param name="scrType">The public key script type to check against</param>
        /// <param name="hash">The hash used in creation of this address</param>
        /// <returns>True if the address is the same script type, otherwise false</returns>
        public static bool VerifyType(string address, PubkeyScriptType scrType, out byte[] hash)
        {
            hash = null;
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }
            switch (scrType)
            {
                case PubkeyScriptType.P2PKH:
                    if (Base58.TryDecodeWithChecksum(address, out byte[] decoded))
                    {
                        if (decoded.Length == 21 &&
                            (decoded[0] == P2pkhVerMainNet ||
                             decoded[0] == P2pkhVerTestNet ||
                             decoded[0] == P2pkhVerTestNet4 ||
                             decoded[0] == P2pkhVerRegTest))
                        {
                            hash = decoded.SubArray(1);
                            return true;
                        }
                    }
                    break;

                case PubkeyScriptType.P2SH:
                    if (Base58.TryDecodeWithChecksum(address, out decoded))
                    {
                        if (decoded.Length == 21 &&
                            (decoded[0] == P2shVerMainNet ||
                             decoded[0] == P2shVerTestNet ||
                             decoded[0] == P2shVerTestNet4 ||
                             decoded[0] == P2shVerRegTest))
                        {
                            hash = decoded.SubArray(1);
                            return true;
                        }
                    }
                    break;

                case PubkeyScriptType.P2WPKH:
                    if (Bech32.TryDecode(address, Bech32.Mode.B32, out decoded, out byte witVer, out string hrp))
                    {
                        if (witVer == 0 && decoded.Length == 20 &&
                            (hrp == HrpMainNet || hrp == HrpTestNet || hrp == HrpTestNet4 || hrp == HrpRegTest))
                        {
                            if (!TransactionVerifier.IsNotZero20(decoded))
                            {
                                return false;
                            }

                            hash = decoded;
                            return true;
                        }
                    }
                    break;

                case PubkeyScriptType.P2WSH:
                    if (Bech32.TryDecode(address, Bech32.Mode.B32, out decoded, out witVer, out hrp))
                    {
                        if (witVer == 0 && decoded.Length == 32 &&
                            (hrp == HrpMainNet || hrp == HrpTestNet || hrp == HrpTestNet4 || hrp == HrpRegTest))
                        {
                            if (!TransactionVerifier.IsNotZero32(decoded))
                            {
                                return false;
                            }

                            hash = decoded;
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }
    }
}
