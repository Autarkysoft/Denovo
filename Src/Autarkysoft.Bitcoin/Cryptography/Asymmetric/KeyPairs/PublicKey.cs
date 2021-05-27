// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs
{
    /// <summary>
    /// The public part of the key pair
    /// </summary>
    public class PublicKey
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PublicKey"/> with the given <see cref="EllipticCurvePoint"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="ellipticCurvePoint">The point to use.</param>
        public PublicKey(EllipticCurvePoint ellipticCurvePoint)
        {
            calc.CheckOnCurve(ellipticCurvePoint);
            point = ellipticCurvePoint;
        }



        private readonly EllipticCurvePoint point;
        private static readonly EllipticCurveCalculator calc = new EllipticCurveCalculator();

        /// <summary>
        /// Public key type in a Taproot script
        /// </summary>
        public enum PublicKeyType
        {
            /// <summary>
            /// Invalid pubkey type (length = 0)
            /// </summary>
            None,
            /// <summary>
            /// Unknown pubkey type (length != 32 &#38; !=0)
            /// </summary>
            Unknown,
            /// <summary>
            /// Public key used in version 1 witness (length = 32)
            /// </summary>
            Schnorr
        }


        /// <summary>
        /// Converts the given byte array to a <see cref="PublicKey"/>. Return value indicates success.
        /// </summary>
        /// <param name="pubBa">Byte array containing a public key</param>
        /// <param name="pubK">The result</param>
        /// <returns>True if the conversion was successful, otherwise false.</returns>
        public static bool TryRead(byte[] pubBa, out PublicKey pubK)
        {
            if (calc.TryGetPoint(pubBa, out EllipticCurvePoint pt))
            {
                pubK = new PublicKey(pt);
                return true;
            }
            else
            {
                pubK = null;
                return false;
            }
        }

        /// <summary>
        /// Converts the given byte array from a Taproot script to a <see cref="PublicKey"/>.
        /// Return value indicates success and type of the public key.
        /// </summary>
        /// <param name="pubBa">Byte array containing a public key</param>
        /// <param name="pubK">The result</param>
        /// <returns>Public key type.</returns>
        public static PublicKeyType TryReadTaproot(byte[] pubBa, out PublicKey pubK)
        {
            if (pubBa.Length == 0)
            {
                pubK = null;
                return PublicKeyType.None;
            }

            if (pubBa.Length != 32)
            {
                pubK = null;
                return PublicKeyType.Unknown;
            }

            if (calc.TryGetPoint(pubBa.AppendToBeginning(0x02), out EllipticCurvePoint pt))
            {
                pubK = new PublicKey(pt);
                return PublicKeyType.Schnorr;
            }
            else
            {
                pubK = null;
                return PublicKeyType.None;
            }
        }

        /// <summary>
        /// Converts this instance to its byte array representation.
        /// </summary>
        /// <param name="useCompressed">If true the 33 byte representation is returned, otherwise the 65 one.</param>
        /// <returns>An array of bytes with 33 or 65 length</returns>
        public byte[] ToByteArray(bool useCompressed)
        {
            byte[] xBytes = point.X.ToByteArray(true, true);
            // Public key's x and y bytes must always be 32 bytes so they may need padding
            if (useCompressed)
            {
                byte[] result = new byte[33];
                result[0] = point.Y.IsEven ? (byte)2 : (byte)3;
                Buffer.BlockCopy(xBytes, 0, result, 33 - xBytes.Length, xBytes.Length);
                return result;
            }
            else
            {
                byte[] result = new byte[65];
                result[0] = 4;
                byte[] yBytes = point.Y.ToByteArray(true, true);
                Buffer.BlockCopy(xBytes, 0, result, 33 - xBytes.Length, xBytes.Length);
                Buffer.BlockCopy(yBytes, 0, result, 65 - yBytes.Length, yBytes.Length);
                return result;
            }
        }

        /// <summary>
        /// Returns the <see cref="EllipticCurvePoint"/> of this public key.
        /// </summary>
        /// <returns></returns>
        public EllipticCurvePoint ToPoint()
        {
            return point;
        }

        /// <summary>
        /// Encrypts a message with this public key using Elliptic Curve Integrated Encryption Scheme (ECIES)
        /// with AES-128-CBC as cipher and HMAC-SHA256 as MAC.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="message">Message to encrypt</param>
        /// <param name="magic">
        /// [Default value = BIE1]
        /// A magic string added to encrypted result before computing its HMAC-SHA256
        /// </param>
        /// <returns>Encrypted result as a base-64 encoded string</returns>
        public string Encrypt(string message, string magic = "BIE1")
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "Mesage can not be null.");
            if (magic is null)
                throw new ArgumentNullException(nameof(magic), "Magic can not be null.");

            byte[] magicBytes = Encoding.UTF8.GetBytes(magic);

            // TODO: investigate if this can become deterministic (it can't be based on message or pubkey
            //       otherwise the ephemeral key is revealed)
            using SharpRandom rng = new SharpRandom();
            using PrivateKey ephemeral = new PrivateKey(rng);

            byte[] ecdhKey = new PublicKey(calc.Multiply(ephemeral.ToBigInt(), point)).ToByteArray(true);
            using Sha512 sha512 = new Sha512();
            byte[] key = sha512.ComputeHash(ecdhKey);

            using Aes aes = new AesManaged
            {
                KeySize = 128,
                Key = key.SubArray(16, 16),
                Mode = CipherMode.CBC,
                IV = key.SubArray(0, 16),
                Padding = PaddingMode.PKCS7
            };
            using ICryptoTransform encryptor = aes.CreateEncryptor();
            using MemoryStream encStream = new MemoryStream();
            using CryptoStream cryptStream = new CryptoStream(encStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new StreamWriter(cryptStream))
            {
                swEncrypt.Write(message);
            }

            byte[] encrypted = magicBytes.ConcatFast(ephemeral.ToPublicKey().ToByteArray(true)).ConcatFast(encStream.ToArray());
            using HmacSha256 hmac = new HmacSha256();
            byte[] mac = hmac.ComputeHash(encrypted, key.SubArray(32));

            return encrypted.ConcatFast(mac).ToBase64();
        }
    }
}
