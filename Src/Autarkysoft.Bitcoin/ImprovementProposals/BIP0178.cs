// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Encoders;
using System;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Version Extended Wallet Import Formats (WIFs)
    /// <para/> Note that this is a draft BIP and Electrum has already abandoned its format.
    /// </summary>
    /// <remarks>
    /// <para/>https://github.com/bitcoin/bips/blob/master/bip-0178.mediawiki
    /// <para/>https://github.com/spesmilo/electrum/blob/8d400d69d895d495550b01cb9a37bb8a30ae2f5a/RELEASE-NOTES#L42-L51
    /// <para/>https://github.com/spesmilo/electrum/blob/e971bd849868122135012b836ece86aeb591efb7/RELEASE-NOTES#L58-L61
    /// </remarks>
    public class BIP0178
    {
        private const byte MainNetByte = 128;
        private const byte TestNetByte = 239;
        private const byte RegTestByte = 239;
        private const byte CompressedByte = 1;

        /// <summary>
        /// The suffix appended to the end of the private key bytes to indicate the type of address that should be
        /// created from it. (key | suffix)
        /// </summary>
        public enum VersionSuffix : byte
        {
            /// <summary>
            /// Pay to pubkey hash (aka legacy) address format using compressed public key 
            /// (versioned WIF is not defined for addresses using uncompressed public keys)
            /// </summary>
            P2PKH = 0x10,
            /// <summary>
            /// Pay to witness pubkey hash (aka native SegWit) address format
            /// </summary>
            P2WPKH = 0x11,
            /// <summary>
            /// Pay to witness pubkey hash inside a pay to script hash (aka nested SegWit) address format
            /// </summary>
            P2WPKH_P2SH = 0x12
        }

        /// <summary>
        /// The prefix added to the first byte of the private key bytes to indicate the type of address that should be
        /// created from it. (key[0] + prefix)
        /// </summary>
        public enum ElectrumVersionPrefix : byte
        {
            /// <summary>
            /// Pay to witness pubkey hash (aka native SegWit) address format
            /// </summary>
            P2WPKH = 1,
            /// <summary>
            /// Pay to witness pubkey hash inside a pay to script hash (aka nested SegWit) address format
            /// </summary>
            P2WPKH_P2SH = 2,
            /// <summary>
            /// Pay to script hash address format
            /// </summary>
            P2SH = 5,
            /// <summary>
            /// Pay to witness script hash address format
            /// </summary>
            P2WSH = 6,
            /// <summary>
            /// Pay to witness script hash inside a pay to script hash address format
            /// </summary>
            P2WSH_P2SH = 7
        }



        /// <exception cref="ArgumentException"/>
        protected byte GetWifFirstByte(NetworkType netType)
        {
            return netType switch
            {
                NetworkType.MainNet => MainNetByte,
                NetworkType.TestNet => TestNetByte,
                NetworkType.RegTest => RegTestByte,
                _ => throw new ArgumentException("Network type is not defined."),
            };
        }

        /// <summary>
        /// Converts a base-58 encoded private key with <see cref="BIP0178"/> version suffix to an instance of
        /// <see cref="PrivateKey"/> to be used for general purposes (convert to normal WIF, sign transactions,...).
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="FormatException"/>
        /// <param name="versionedWif">Base-58 encoded private key that is extended with a version byte.</param>
        /// <param name="netType">
        /// [Default value = <see cref="NetworkType.MainNet"/>
        /// Defines the network type that this WIF works for (affects string's initial character).
        /// </param>
        /// <returns>A new instance of <see cref="PrivateKey"/> with the version byte removed.</returns>
        public PrivateKey Decode(string versionedWif, NetworkType netType = NetworkType.MainNet)
        {
            if (string.IsNullOrEmpty(versionedWif))
                throw new ArgumentNullException(nameof(versionedWif), "Input WIF can not be null or empty.");

            byte[] ba = Base58.DecodeWithChecksum(versionedWif);
            if (ba[0] != GetWifFirstByte(netType))
            {
                throw new FormatException("Invalid first byte.");
            }

            // Uncompressed with no byte appended to the end.
            if (ba.Length == 32 + 1)
            {
                throw new FormatException("Given WIF is uncompressed and is not extended with a version byte.");
            }
            // Has an appended byte to the end
            else if (ba.Length == 32 + 2)
            {
                // The appended byte is the compressed byte, not version byte.
                if (ba[^1] == CompressedByte)
                {
                    throw new FormatException("Given WIF is normal, and not extended with a version byte.");
                }
                if (Enum.IsDefined(typeof(VersionSuffix), ba[^1]))
                {
                    return new PrivateKey(ba.SubArray(1, ba.Length - 2));
                }
                else
                {
                    throw new FormatException("Wrong byte was used to extend this private key.");
                }
            }
            else
            {
                throw new FormatException("Invalid WIF bytes length.");
            }
        }

        /// <summary>
        /// Converts a base-58 encoded private key with Electrum specific version preffix to an instance of <see cref="PrivateKey"/>
        /// while removing the version byte.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="FormatException"/>
        /// <param name="versionedWif">Base-58 encoded private key that is extended with a version byte.</param>
        /// <param name="netType">
        /// [Default value = <see cref="NetworkType.MainNet"/>
        /// Defines the network type that this WIF works for (affects string's initial character).
        /// </param>
        /// <returns>A new instance of <see cref="PrivateKey"/> with version byte removed.</returns>
        public PrivateKey DecodeElectrumVersionedWif(string versionedWif, NetworkType netType = NetworkType.MainNet)
        {
            if (string.IsNullOrEmpty(versionedWif))
                throw new ArgumentNullException(nameof(versionedWif), "Input WIF can not be null or empty.");

            byte[] ba = Base58.DecodeWithChecksum(versionedWif);
            // Uncompressed with no appended byte to the end
            if (ba.Length == 32 + 1)
            {
                throw new FormatException("Given WIF is uncompressed and is not extended with a version byte.");
            }
            else if (ba.Length == 32 + 2)
            {
                if (ba[^1] != CompressedByte)
                {
                    throw new FormatException($"Invalid compressed byte (0x{ba[^1]:x2}).");
                }

                byte firstByte = GetWifFirstByte(netType);
                if (ba[0] == firstByte)
                {
                    throw new FormatException("Given WIF is normal, and not extended with a version byte.");
                }
                else if (ba[0] != firstByte + (byte)ElectrumVersionPrefix.P2WPKH &&
                         ba[0] != firstByte + (byte)ElectrumVersionPrefix.P2WPKH_P2SH &&
                         ba[0] != firstByte + (byte)ElectrumVersionPrefix.P2SH &&
                         ba[0] != firstByte + (byte)ElectrumVersionPrefix.P2WSH &&
                         ba[0] != firstByte + (byte)ElectrumVersionPrefix.P2WSH_P2SH)
                {
                    throw new FormatException("Wrong byte was used to extend this private key.");
                }
                else
                {
                    return new PrivateKey(ba.SubArray(1, ba.Length - 2));
                }
            }
            else
            {
                throw new FormatException("Invalid WIF bytes length.");
            }
        }


        /// <summary>
        /// Converts the given <see cref="PrivateKey"/> to <see cref="Base58"/> encoded string with a checksum 
        /// and a version suffix indicating the type of address the private key should be converted to.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="key">The private key to use</param>
        /// <param name="ver">Version suffix used for encoding the private key</param>
        /// <param name="netType">
        /// [Default value = <see cref="NetworkType.MainNet"/>
        /// Defines the network type that this WIF works for (affects string's initial character).
        /// </param>
        /// <returns>Versioned wallet imported format (WIF)</returns>
        public string Encode(PrivateKey key, VersionSuffix ver, NetworkType netType = NetworkType.MainNet)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key), "Key can not be null.");
            if (!Enum.IsDefined(typeof(VersionSuffix), ver))
                throw new ArgumentException("Version suffix type is not defined.", nameof(ver));

            byte firstByte = GetWifFirstByte(netType);
            byte[] ba = key.ToBytes().AppendToBeginning(firstByte).AppendToEnd((byte)ver);

            return Base58.EncodeWithChecksum(ba);
        }

        /// <summary>
        /// Converts the given <see cref="PrivateKey"/> to <see cref="Base58"/> encoded string with a checksum 
        /// and an Electrum specific version prefix indicating the type of address the private key should be converted to.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="key">The private key to use</param>
        /// <param name="ver">Version prefix used for encoding the private key</param>
        /// <param name="netType">
        /// [Default value = <see cref="NetworkType.MainNet"/>
        /// Defines the network type that this WIF works for (affects string's initial character).
        /// </param>
        /// <returns>Versioned wallet imported format (WIF)</returns>
        public string EncodeElectrumVersionedWif(PrivateKey key, ElectrumVersionPrefix ver, NetworkType netType = NetworkType.MainNet)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key), "Key can not be null.");
            if (!Enum.IsDefined(typeof(ElectrumVersionPrefix), ver))
                throw new ArgumentException("Version suffix type is not defined.", nameof(ver));

            byte firstByte = (byte)(GetWifFirstByte(netType) + (byte)ver);
            byte[] ba = key.ToBytes().AppendToBeginning(firstByte).AppendToEnd(CompressedByte);

            return Base58.EncodeWithChecksum(ba);
        }

        /// <summary>
        /// Converts the given <see cref="PrivateKey"/> to <see cref="Base58"/> representation with a checksum.
        /// To indicate the type of corresponding address a type string will be added to the start of the WIF.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="key">key to use</param>
        /// <param name="ver">Version prefix is used to add the starting string</param>
        /// <param name="netType">
        /// [Default value = <see cref="NetworkType.MainNet"/>
        /// Defines the network type that this WIF works for (affects string's initial character).
        /// </param>
        /// <returns>Versioned wallet imported format (WIF).</returns>
        public string EncodeWithScriptType(PrivateKey key, ElectrumVersionPrefix ver, NetworkType netType = NetworkType.MainNet)
        {
            if (!Enum.IsDefined(typeof(ElectrumVersionPrefix), ver))
                throw new ArgumentException("Version suffix type is not defined.", nameof(ver));

            string wif = key.ToWif(true, netType);
            string scrType = ver.ToString().Replace("_", "-").ToLower();
            return $"{scrType}:{wif}";
        }

    }
}
