// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;

namespace Autarkysoft.Bitcoin.Encoders
{
    /// <summary>
    /// Provides methods to check and create bitcoin addresses from public keys.
    /// </summary>
    public class Address
    {
        private const byte P2pkhVerMainNet = 0;
        private const byte P2pkhVerTestNet = 111;
        private const byte P2pkhVerRegTest = 0;

        private const byte P2shVerMainNet = 5;
        private const byte P2shVerTestNet = 196;
        private const byte P2shVerRegTest = 5;

        private const string HrpMainNet = "bc";
        private const string HrpTestNet = "tb";
        private const string HrpRegTest = "bcrt";

        private readonly Base58 b58Encoder = new Base58();
        private readonly Bech32 b32Encoder = new Bech32();



        /// <summary>
        /// Address type, there are currently 4 defined types
        /// </summary>
        public enum AddressType
        {
            /// <summary>
            /// Unknown or invalid address
            /// </summary>
            Unknown,
            /// <summary>
            /// Pay to public key hash
            /// </summary>
            P2PKH,
            /// <summary>
            /// Pay to script hash
            /// </summary>
            P2SH,
            /// <summary>
            /// Pay to witness public key hash
            /// </summary>
            P2WPKH,
            /// <summary>
            /// Pay to witness script hash
            /// </summary>
            P2WSH
        }

        /// <summary>
        /// Returns the type of the given address
        /// </summary>
        /// <param name="address">Address string to check</param>
        /// <param name="netType">Network type</param>
        /// <returns>Address type</returns>
        public AddressType GetAddressType(string address, NetworkType netType)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return AddressType.Unknown;
            }
            else if (b58Encoder.IsValid(address))
            {
                byte[] decoded = b58Encoder.DecodeWithCheckSum(address);
                if (decoded.Length == 21)
                {
                    if ((netType == NetworkType.MainNet && decoded[0] == P2pkhVerMainNet) ||
                        (netType == NetworkType.TestNet && decoded[0] == P2pkhVerTestNet) ||
                        (netType == NetworkType.RegTest && decoded[0] == P2pkhVerRegTest))
                    {
                        return AddressType.P2PKH;
                    }
                    else if ((netType == NetworkType.MainNet && decoded[0] == P2shVerMainNet) ||
                             (netType == NetworkType.TestNet && decoded[0] == P2shVerTestNet) ||
                             (netType == NetworkType.RegTest && decoded[0] == P2shVerRegTest))
                    {
                        return AddressType.P2SH;
                    }
                }
            }
            else if (b32Encoder.IsValid(address))
            {
                byte[] decoded = b32Encoder.Decode(address, out byte witVer, out string hrp);
                if (witVer == 0)
                {
                    if (decoded.Length == 20 && (
                        (netType == NetworkType.MainNet && hrp == HrpMainNet) ||
                        (netType == NetworkType.TestNet && hrp == HrpTestNet) ||
                        (netType == NetworkType.RegTest && hrp == HrpRegTest)
                        ))
                    {
                        return AddressType.P2WPKH;
                    }
                    else if (decoded.Length == 32 && (
                             (netType == NetworkType.MainNet && hrp == HrpMainNet) ||
                             (netType == NetworkType.TestNet && hrp == HrpTestNet) ||
                             (netType == NetworkType.RegTest && hrp == HrpRegTest)
                             ))
                    {
                        return AddressType.P2WSH;
                    }
                }
            }

            return AddressType.Unknown;
        }


        /// <summary>
        /// Return the pay to public key hash address from the given <see cref="PublicKey"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="pubk">Public key to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates wheter to use compressed or compressed public key to generate the address
        /// </param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public string GetP2pkh(PublicKey pubk, bool useCompressed = true, NetworkType netType = NetworkType.MainNet)
        {
            if (pubk is null)
                throw new ArgumentNullException(nameof(pubk), "Public key can not be null.");

            byte ver = netType switch
            {
                NetworkType.MainNet => P2pkhVerMainNet,
                NetworkType.TestNet => P2pkhVerTestNet,
                NetworkType.RegTest => P2pkhVerRegTest,
                _ => throw new ArgumentException(Err.InvalidNetwork)
            };

            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
            byte[] data = hashFunc.ComputeHash(pubk.ToByteArray(useCompressed)).AppendToBeginning(ver);

            return b58Encoder.EncodeWithCheckSum(data);
        }


        /// <summary>
        /// Return the pay to script hash address from the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="redeem">Redeem script to use</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public string GetP2sh(IScript redeem, NetworkType netType = NetworkType.MainNet)
        {
            if (redeem is null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");

            byte ver = netType switch
            {
                NetworkType.MainNet => P2shVerMainNet,
                NetworkType.TestNet => P2shVerTestNet,
                NetworkType.RegTest => P2shVerRegTest,
                _ => throw new ArgumentException(Err.InvalidNetwork)
            };

            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
            byte[] data = hashFunc.ComputeHash(redeem.Data).AppendToBeginning(ver);

            return b58Encoder.EncodeWithCheckSum(data);
        }


        /// <summary>
        /// Return the pay to witness public key hash address from the given <see cref="PublicKey"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="pubk">Public key to use</param>
        /// <param name="witVer">Witness version to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates wheter to use compressed or compressed public key to generate the address
        /// <para/> Note: using uncompressed public keys makes the output non-standard and can lead to money loss.
        /// </param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public string GetP2wpkh(PublicKey pubk, byte witVer, bool useCompressed = true, NetworkType netType = NetworkType.MainNet)
        {
            if (pubk is null)
                throw new ArgumentNullException(nameof(pubk), "Public key can not be null.");
            if (witVer != 0)
                throw new ArgumentException("Currently only address version 0 is defined for P2WPKH.", nameof(witVer));

            string hrp = netType switch
            {
                NetworkType.MainNet => HrpMainNet,
                NetworkType.TestNet => HrpTestNet,
                NetworkType.RegTest => HrpRegTest,
                _ => throw new ArgumentException(Err.InvalidNetwork),
            };

            using Ripemd160Sha256 hashFunc = new Ripemd160Sha256();
            byte[] hash160 = hashFunc.ComputeHash(pubk.ToByteArray(useCompressed));

            return b32Encoder.Encode(hash160, witVer, hrp);
        }


        /// <summary>
        /// Return the pay to witness public key hash address from the given <see cref="PublicKey"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="pubk">Public key to use</param>
        /// <param name="witVer">Witness version to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates wheter to use compressed or compressed public key to generate the address
        /// <para/> Note: using uncompressed public keys makes the output non-standard and can lead to money loss.
        /// </param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public string GetP2sh_P2wpkh(PublicKey pubk, byte witVer, bool useCompressed = true,
                                     NetworkType netType = NetworkType.MainNet)
        {
            if (pubk is null)
                throw new ArgumentNullException(nameof(pubk), "Public key can not be null.");
            if (witVer != 0)
                throw new ArgumentException("Currently only address version 0 is defined for P2WPKH.", nameof(witVer));
            if (netType != NetworkType.MainNet && netType != NetworkType.TestNet && netType != NetworkType.RegTest)
                throw new ArgumentException(Err.InvalidNetwork);

            RedeemScript rdm = new RedeemScript();
            rdm.SetToP2SH_P2WPKH(pubk, useCompressed);
            return GetP2sh(rdm, netType);
        }


        /// <summary>
        /// Return the pay to witness script hash address from the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="script">Script to use</param>
        /// <param name="witVer">Witness version to use</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public string GetP2wsh(IScript script, byte witVer, NetworkType netType = NetworkType.MainNet)
        {
            if (script is null)
                throw new ArgumentNullException(nameof(script), "Script can not be null.");
            if (witVer != 0)
                throw new ArgumentException("Currently only address version 0 is defined for P2WSH.", nameof(witVer));

            string hrp = netType switch
            {
                NetworkType.MainNet => HrpMainNet,
                NetworkType.TestNet => HrpTestNet,
                NetworkType.RegTest => HrpRegTest,
                _ => throw new ArgumentException(Err.InvalidNetwork),
            };

            using Sha256 witHashFunc = new Sha256();
            byte[] hash = witHashFunc.ComputeHash(script.Data);

            return b32Encoder.Encode(hash, witVer, hrp);
        }


        /// <summary>
        /// Return the pay to witness script hash wrapped in a pay to script hash address from the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="script">Public key to use</param>
        /// <param name="witVer">Witness version to use</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>The resulting address</returns>
        public string GetP2sh_P2wsh(IScript script, byte witVer, NetworkType netType = NetworkType.MainNet)
        {
            if (script is null)
                throw new ArgumentNullException(nameof(script), "Script can not be null.");
            if (witVer != 0)
                throw new ArgumentException("Currently only address version 0 is defined for P2WSH-P2SH.", nameof(witVer));
            if (netType != NetworkType.MainNet && netType != NetworkType.TestNet && netType != NetworkType.RegTest)
                throw new ArgumentException(Err.InvalidNetwork);

            RedeemScript rdm = new RedeemScript();
            rdm.SetToP2SH_P2WSH(script);
            return GetP2sh(rdm, netType);
        }


        /// <summary>
        /// Checks if the given address string is of the given <see cref="PubkeyScriptType"/> type and returns the
        /// decoded hash from the address.
        /// </summary>
        /// <param name="address">Address to check</param>
        /// <param name="scrType">The public key script type to check against</param>
        /// <param name="hash">The hash used in creation of this address</param>
        /// <returns>True if the address is the same script type, otherwise false</returns>
        public bool VerifyType(string address, PubkeyScriptType scrType, out byte[] hash)
        {
            hash = null;
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }
            switch (scrType)
            {
                case PubkeyScriptType.P2PKH:
                    if (!b58Encoder.IsValid(address))
                    {
                        return false;
                    }
                    byte[] decoded = b58Encoder.DecodeWithCheckSum(address);
                    if (decoded[0] != P2pkhVerMainNet &&
                        decoded[0] != P2pkhVerTestNet &&
                        decoded[0] != P2pkhVerRegTest ||
                        decoded.Length != 21)
                    {
                        return false;
                    }
                    hash = decoded.SubArray(1);
                    return true;

                case PubkeyScriptType.P2SH:
                    if (!b58Encoder.IsValid(address))
                    {
                        return false;
                    }
                    decoded = b58Encoder.DecodeWithCheckSum(address);
                    if (decoded[0] != P2shVerMainNet &&
                        decoded[0] != P2shVerTestNet &&
                        decoded[0] != P2shVerRegTest ||
                        decoded.Length != 21)
                    {
                        return false;
                    }
                    hash = decoded.SubArray(1);
                    return true;

                case PubkeyScriptType.P2WPKH:
                    if (!b32Encoder.IsValid(address))
                    {
                        return false;
                    }
                    decoded = b32Encoder.Decode(address, out byte witVer, out string hrp);
                    if (witVer != 0 || decoded.Length != 20 ||
                        (hrp != HrpMainNet && hrp != HrpTestNet && hrp != HrpRegTest))
                    {
                        return false;
                    }
                    hash = decoded;
                    return true;

                case PubkeyScriptType.P2WSH:
                    if (!b32Encoder.IsValid(address))
                    {
                        return false;
                    }
                    decoded = b32Encoder.Decode(address, out witVer, out hrp);
                    if (witVer != 0 || decoded.Length != 32 ||
                        (hrp != HrpMainNet && hrp != HrpTestNet && hrp != HrpRegTest))
                    {
                        return false;
                    }
                    hash = decoded;
                    return true;
                default:
                    return false;
            }
        }
    }
}
