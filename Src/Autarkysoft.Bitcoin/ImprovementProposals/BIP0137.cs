// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Message signing and verifcation
    /// <para/>https://github.com/bitcoin/bips/blob/master/bip-0137.mediawiki
    /// </summary>
    public class BIP0137
    {
        /// <summary>
        /// Type of the address derived from the key used for signing
        /// </summary>
        public enum AddressType
        {
            /// <summary>
            /// P2PKH address using uncompressed public key
            /// </summary>
            P2PKH_Uncompressed,
            /// <summary>
            /// P2PKH address using compressed public key
            /// </summary>
            P2PKH_Compressed,
            /// <summary>
            /// P2WPKH script inside a P2SH script (nested SegWit)
            /// </summary>
            P2SH_P2WPKH,
            /// <summary>
            /// Bech32 encoded native SegWit address for version 0 witness program
            /// </summary>
            P2WPKH
        }


        /// <summary>
        /// Returns the 256-bit result of double SHA-256 hash of the message with the added constant used in signing operation.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="message">
        /// UTF-8 encoded message to sign (will be normalized using full compatibility decomposition form).
        /// <para/>Note that trailing spaces, new line character,... will not be changed here. 
        /// Caller has to decide whether to change those
        /// </param>
        /// <returns>256-bit hash</returns>
        public byte[] GetBytesToSign(string message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "Message can not be null.");

            using Sha256 hash = new Sha256();
            FastStream stream = new FastStream();
            stream.Write((byte)Constants.MsgSignConst.Length);
            stream.Write(Encoding.UTF8.GetBytes(Constants.MsgSignConst));
            byte[] messageBytes = Encoding.UTF8.GetBytes(message.Normalize(NormalizationForm.FormKD));
            stream.WriteWithCompactIntLength(messageBytes);

            byte[] result = hash.ComputeHashTwice(stream.ToByteArray());
            return result;
        }

        /// <summary>
        /// Signs the given message and returns the <see cref="Signature"/> result with its recovery ID set to
        /// appropriate value according to the given <see cref="AddressType"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="key">Private key to use</param>
        /// <param name="message">UTF-8 encoded message to sign (null and white space will be rejected)</param>
        /// <param name="addrType">Type of the corresponding address (will affect the recovery ID)</param>
        /// <param name="ignoreSegwit">
        /// [Default value = false]
        /// If true and address type is a SegWit address, sets the additional recovery ID value to 31.
        /// This is useful to return a recovery ID that can be verified with popular Bitcoin implementations such as Electrum.
        /// </param>
        /// <returns>Signature with the recovery ID set to appropriate value</returns>
        public Signature Sign(PrivateKey key, string message, AddressType addrType, bool ignoreSegwit = false)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key), "Private key can not be null.");
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Signing an empty message is not allowed in this implementation.");

            byte[] toSign = GetBytesToSign(message);
            byte recId = addrType switch
            {
                AddressType.P2PKH_Uncompressed => 27,
                AddressType.P2PKH_Compressed => 31,
                AddressType.P2SH_P2WPKH => 35,
                AddressType.P2WPKH => 39,
                _ => throw new ArgumentException("Invalid address type.")
            };

            if (ignoreSegwit && (addrType == AddressType.P2SH_P2WPKH || addrType == AddressType.P2WPKH))
            {
                recId = 31;
            }

            var calc = new EllipticCurveCalculator();
            var sig = calc.Sign(toSign, key.ToBytes());
            sig.RecoveryId += recId;
            return sig;
        }

        /// <summary>
        /// Verifies the given signature against the message and for the given address.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="message">UTF-8 encoded message to sign</param>
        /// <param name="address">Address of the key used to create the given signature</param>
        /// <param name="signature">Fixed length (65 byte) signature with a starting recovery ID encoded using Base-64</param>
        /// <param name="ignoreSegwit">
        /// [Default value = false]
        /// If true and address type is a SegWit address, accepts both correct recovery ID (as defined by this BIP)
        /// and the incorrect one.
        /// This is useful to verify signature produced by popular Bitcoin implementations such as Electrum.
        /// </param>
        /// <returns>True if the verification succeeds; otherwise false.</returns>
        public bool Verify(string message, string address, string signature, bool ignoreSegwit = false)
        {
            byte[] toSign = GetBytesToSign(message);

            byte[] sigBa = Convert.FromBase64String(signature);
            if (!Signature.TryReadWithRecId(sigBa, out Signature sig, out string error))
            {
                throw new FormatException(error);
            }

            AddressType addrType = AddressType.P2PKH_Compressed;
            if (sig.RecoveryId < 27 || sig.RecoveryId > 43)
            {
                return false;
            }
            else if (sig.RecoveryId >= 27 && sig.RecoveryId < 35)
            {
                if (!Address.VerifyType(address, Blockchain.Scripts.PubkeyScriptType.P2PKH, out _))
                {
                    // Special case where this BIP is not used to create the signature
                    if (ignoreSegwit && Address.VerifyType(address, Blockchain.Scripts.PubkeyScriptType.P2SH, out _))
                    {
                        addrType = AddressType.P2SH_P2WPKH;
                    }
                    else if (ignoreSegwit && Address.VerifyType(address, Blockchain.Scripts.PubkeyScriptType.P2WPKH, out _))
                    {
                        addrType = AddressType.P2WPKH;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    addrType = sig.RecoveryId < 31 ? AddressType.P2PKH_Uncompressed : AddressType.P2PKH_Compressed;
                }
            }
            else if (sig.RecoveryId >= 35 && sig.RecoveryId < 39)
            {
                if (!Address.VerifyType(address, Blockchain.Scripts.PubkeyScriptType.P2SH, out _))
                {
                    return false;
                }
                addrType = AddressType.P2SH_P2WPKH;
            }
            else if (sig.RecoveryId >= 39 && sig.RecoveryId < 43)
            {
                if (!Address.VerifyType(address, Blockchain.Scripts.PubkeyScriptType.P2WPKH, out _))
                {
                    return false;
                }
                addrType = AddressType.P2WPKH;
            }

            EllipticCurveCalculator calc = new EllipticCurveCalculator();
            if (!calc.TryRecoverPublicKeys(toSign, sig, out EllipticCurvePoint[] points))
            {
                return false;
            }

            foreach (var item in points)
            {
                string actualAddr = addrType switch
                {
                    AddressType.P2PKH_Uncompressed => Address.GetP2pkh(new PublicKey(item), false),
                    AddressType.P2PKH_Compressed => Address.GetP2pkh(new PublicKey(item), true),
                    AddressType.P2SH_P2WPKH => Address.GetP2sh_P2wpkh(new PublicKey(item), 0, true),
                    AddressType.P2WPKH => Address.GetP2wpkh(new PublicKey(item), 0, true),
                    _ => throw new ArgumentException("Address type is not defined."),
                };

                if (actualAddr == address)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
