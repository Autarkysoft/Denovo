// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Cryptography.KeyDerivationFunctions;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Passphrase-protected private key. Implements <see cref="IDisposable"/>.
    /// <para/>https://github.com/bitcoin/bips/blob/master/bip-0038.mediawiki
    /// </summary>
    public class BIP0038 : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BIP0038"/>.
        /// </summary>
        public BIP0038()
        {
            scrypt = new Scrypt(16384, 8, 8);
            hash = new Sha256();
            aes = new AesManaged
            {
                KeySize = 256, // AES-256
                Mode = CipherMode.ECB, // This mode encrypts each block individually
                IV = new byte[16], // No initialization vector is used
                Padding = PaddingMode.None // No padding
            };
            curveOrder = new SecP256k1().N;
        }

        // TODO: add code for EC multiply encryption mode (decryption is already implemented)


        private const int EncodedLength = 39;
        private readonly byte[] prefix = { 0x01, 0x42 };
        private readonly byte[] prefix_ECMultiplied = { 0x01, 0x43 };
        private static readonly byte[] magicWithLot = new byte[8] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2, 0x51 };
        private static readonly byte[] magicNoLot = new byte[8] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2, 0x53 };
        private Scrypt scrypt;
        private Aes aes;
        private Sha256 hash;
        private readonly BigInteger curveOrder;



        /// <summary>
        /// Decrypts the given base-58 encoded encrypted key using the given password string and first 4 bytes of hash of
        /// (un)compressed P2PKH address as <see cref="Scrypt"/> salt.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="encrypted">Base-58 encrypted key (it will be normalized using Unicode Normalization Form C (NFC))</param>
        /// <param name="password">Password to use</param>
        /// <param name="isCompressed">Indicates whether to use compressed or uncompressed public key to build P2PKH address</param>
        /// <returns>The private key</returns>
        public PrivateKey Decrypt(string encrypted, string password, out bool isCompressed)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0038), "Instance was disposed.");
            if (string.IsNullOrWhiteSpace(encrypted))
                throw new ArgumentNullException(nameof(encrypted), "Encrypted string can not be null.");
            // Note that empty password string (ie: "") should not be rejected for decryption
            if (password == null)
                throw new ArgumentNullException(nameof(password), "Password can not be null.");

            byte[] pass = Encoding.UTF8.GetBytes(password.Normalize(NormalizationForm.FormC));
            return Decrypt(encrypted, pass, out isCompressed);
        }

        /// <summary>
        /// Decrypts the given base-58 encoded encrypted key using the given password bytes and first 4 bytes of hash of
        /// (un)compressed P2PKH address as <see cref="Scrypt"/> salt.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="encrypted">Base-58 encrypted key (it will be normalized using Unicode Normalization Form C (NFC))</param>
        /// <param name="password">Password to use</param>
        /// <param name="isCompressed">Indicates whether to use compressed or uncompressed public key to build P2PKH address</param>
        /// <returns>The private key</returns>
        public PrivateKey Decrypt(string encrypted, byte[] password, out bool isCompressed)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0038), "Instance was disposed.");
            if (string.IsNullOrWhiteSpace(encrypted))
                throw new ArgumentNullException(nameof(encrypted), "Invalid (null) encrypted key.");
            if (password == null)
                throw new ArgumentNullException(nameof(password), "Password can not be null.");


            ReadOnlySpan<byte> encryptedBytes = Base58.DecodeWithChecksum(encrypted);
            if (encryptedBytes.Length != EncodedLength)
            {
                throw new FormatException("Invalid encrypted bytes length.");
            }

            ReadOnlySpan<byte> actualPrefix = encryptedBytes.Slice(0, 2);

            bool isEcMultMode;
            if (actualPrefix.SequenceEqual(prefix))
            {
                isEcMultMode = false;
            }
            else if (actualPrefix.SequenceEqual(prefix_ECMultiplied))
            {
                isEcMultMode = true;
            }
            else
            {
                throw new FormatException("Invalid (unknown) prefix.");
            }

            isCompressed = IsCompressed(encryptedBytes[2]);
            ReadOnlySpan<byte> salt = encryptedBytes.Slice(3, 4);

            PrivateKey result = isEcMultMode ? DecryptECMult(encryptedBytes, password, salt.ToArray()) :
                                               DecryptNormal(encryptedBytes, password, salt.ToArray());

            string address = Address.GetP2pkh(result.ToPublicKey(), isCompressed, NetworkType.MainNet);
            Span<byte> computedHash = hash.ComputeHashTwice(Encoding.ASCII.GetBytes(address)).SubArray(0, 4);
            if (!computedHash.SequenceEqual(salt))
            {
                throw new FormatException("Wrong password (derived address hash is not the same).");
            }

            return result;
        }

        private PrivateKey DecryptNormal(ReadOnlySpan<byte> encryptedBytes, byte[] password, byte[] salt)
        {
            byte[] dk = scrypt.GetBytes(password, salt, 64);
            byte[] decryptedResult = new byte[32];

            aes.Key = dk.SubArray(32, 32); // AES key is derivedhalf2
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            decryptor.TransformBlock(encryptedBytes.ToArray(), 7, 16, decryptedResult, 0);
            decryptor.TransformBlock(encryptedBytes.ToArray(), 23, 16, decryptedResult, 16);

            // XOR method will only work on first item's length (32 byte here) so it doesn't matter of dk.Legth is 64
            return new PrivateKey(XOR(decryptedResult, dk));
        }

        private PrivateKey DecryptECMult(ReadOnlySpan<byte> data, byte[] password, byte[] addressHash)
        {
            bool hasLot = HasLotsequence(data[2]);

            ReadOnlySpan<byte> ownerEntropy = data.Slice(7, 8);
            ReadOnlySpan<byte> ownerSalt = hasLot ? ownerEntropy.Slice(0, 4) : ownerEntropy;

            ReadOnlySpan<byte> encryptedPart1Half = data.Slice(15, 8);
            ReadOnlySpan<byte> encryptedPart2 = data.Slice(23, 16);

            byte[] preFactor = scrypt.GetBytes(password, ownerSalt, 32);
            byte[] passFactor = hasLot ? hash.ComputeHashTwice(preFactor.ConcatFast(ownerEntropy.ToArray())) : preFactor;

            using PrivateKey tempKey = new PrivateKey(passFactor);
            PublicKey passPoint = tempKey.ToPublicKey();

            using Scrypt smallScrypt = new Scrypt(1024, 1, 1);
            byte[] dk = smallScrypt.GetBytes(passPoint.ToByteArray(true), addressHash.ConcatFast(ownerEntropy.ToArray()), 64);

            aes.Key = dk.SubArray(32, 32);
            ICryptoTransform decryptor = aes.CreateDecryptor();

            // last 8 byte encryptedpart1 | last 8 bytes of seedb
            byte[] decryptedResult = new byte[16];
            decryptor.TransformBlock(encryptedPart2.ToArray(), 0, 16, decryptedResult, 0);
            for (int i = 0; i < decryptedResult.Length; i++)
            {
                decryptedResult[i] ^= dk[i + 16];
            }

            byte[] encryptedPart1 = new byte[16];
            Buffer.BlockCopy(encryptedPart1Half.ToArray(), 0, encryptedPart1, 0, 8);
            Buffer.BlockCopy(decryptedResult, 0, encryptedPart1, 8, 8);

            byte[] decryptedPart1 = new byte[16];
            decryptor.TransformBlock(encryptedPart1, 0, 16, decryptedPart1, 0);
            for (int i = 0; i < 16; i++)
            {
                decryptedPart1[i] ^= dk[i];
            }

            byte[] seedb = new byte[24];
            Array.Copy(decryptedPart1, 0, seedb, 0, 16);
            Array.Copy(decryptedResult, 8, seedb, 16, 8);

            byte[] factorb = hash.ComputeHashTwice(seedb);

            BigInteger keyValue = (new BigInteger(passFactor, true, true) * new BigInteger(factorb, true, true)) % curveOrder;
            return new PrivateKey(keyValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsCompressed(byte b) => (b & 0b0010_0000) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasLotsequence(byte b) => (b & 0b0000_0100) != 0;


        /// <summary>
        /// Encrypts the given private key using the password string and first 4 bytes of hash of (un)compressed P2PKH address
        /// as <see cref="Scrypt"/> salt.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="key">Private key to encrypt</param>
        /// <param name="password">Password to use (it will be normalized using Unicode Normalization Form C (NFC))</param>
        /// <param name="isCompressed">Indicates whether to use compressed or uncompressed public key to build P2PKH address</param>
        /// <returns>Base-58 encoded encrypted result with a checksum</returns>
        public string Encrypt(PrivateKey key, string password, bool isCompressed)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0038), "Instance was disposed.");
            if (key is null)
                throw new ArgumentNullException(nameof(key), "Private key can not be null.");
            // Note that empty password string (ie: "") is rejected for encryption
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), "Password can not be null or empty.");

            return Encrypt(key, Encoding.UTF8.GetBytes(password.Normalize(NormalizationForm.FormC)), isCompressed);
        }

        /// <summary>
        /// Encrypts the given private key using the password bytes and first 4 bytes of hash of (un)compressed P2PKH address
        /// as <see cref="Scrypt"/> salt.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="key">Private key to encrypt</param>
        /// <param name="password">Password to use</param>
        /// <param name="isCompressed">Indicates whether to use compressed or uncompressed public key to build P2PKH address</param>
        /// <returns>Base-58 encoded encrypted result with a checksum</returns>
        public string Encrypt(PrivateKey key, ReadOnlySpan<byte> password, bool isCompressed)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0038), "Instance was disposed.");
            if (key is null)
                throw new ArgumentNullException(nameof(key), "Private key can not be null.");
            // Note that empty password is rejected for encryption
            if (password == null || password.Length == 0)
                throw new ArgumentNullException(nameof(password), "Password can not be null or empty.");

            string address = Address.GetP2pkh(key.ToPublicKey(), isCompressed, NetworkType.MainNet);
            byte[] salt = hash.ComputeHashTwice(Encoding.ASCII.GetBytes(address)).SubArray(0, 4);

            byte[] dk = scrypt.GetBytes(password, salt, 64);
            aes.Key = dk.SubArray(32, 32); // AES key is derivedhalf2
            byte[] encryptedResult = new byte[32];
            // XOR method will only work on first item's length (32 byte here) so it doesn't matter of dk.Legth is 64
            byte[] block = XOR(key.ToBytes(), dk);

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            encryptor.TransformBlock(block, 0, 16, encryptedResult, 0);
            encryptor.TransformBlock(block, 16, 16, encryptedResult, 16);

            FastStream stream = new FastStream(EncodedLength);
            stream.Write(prefix);
            stream.Write(GetFlagByte(isCompressed, false));
            stream.Write(salt);
            stream.Write(encryptedResult);

            return Base58.EncodeWithChecksum(stream.ToByteArray());
        }

        private byte[] XOR(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length];
            for (int i = 0; i < first.Length; i++)
            {
                result[i] = (byte)(first[i] ^ second[i]);
            }
            return result;
        }

        private byte GetFlagByte(bool isCompressed, bool isECMultiplied)
        {
            int flag = 0;
            if (!isECMultiplied)
                flag |= 0b1100_0000; // two most significat bits

            if (isCompressed)
                flag |= 0b0010_0000; // bit with value 0x20 indicating compressed public key usage

            // bits with values 0x10 (=0b0001_0000) and 0x08 (=0b0000_1000) are reserved for using multisig

            if (isECMultiplied)
                flag |= 0b0000_0100; // bit with value 0x04 indicating usage of lot and sequence number (for EC-multiplied keys)

            // Remaining bits are zero and reserved for future versions.

            return (byte)flag;
        }



        private bool isDisposed = false;

        /// <summary>
        /// Releases the resources used by the <see cref="BIP0038"/> class.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (!(scrypt is null))
                        scrypt.Dispose();
                    scrypt = null;

                    if (!(aes is null))
                        aes.Dispose();
                    aes = null;

                    if (!(hash is null))
                        hash.Dispose();
                    hash = null;
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="BIP0038"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
