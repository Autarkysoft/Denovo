// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Numerics;
using System.Text;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs
{
    /// <summary>
    /// Implementation of bitcoin private keys.
    /// Implements <see cref="IDisposable"/>.
    /// </summary>
    public class PrivateKey : IDisposable
    {
        /// <summary>
        /// This constructor is only used by <see cref="MiniPrivateKey"/> as the derived class. 
        /// Derived classes have to call <see cref="SetBytes(byte[])"/> method with the appropriate bytes.
        /// </summary>
        protected PrivateKey()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PrivateKey"/> using the given <see cref="BigInteger"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="big">Integer to use</param>
        public PrivateKey(BigInteger big)
        {
            if (big.Sign < 0)
                throw new ArgumentOutOfRangeException("Number value can not be negative.");

            SetBytes(big.ToByteArray(true, true));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PrivateKey"/> using the given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ba">Byte array to use</param>
        public PrivateKey(byte[] ba)
        {
            SetBytes(ba);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PrivateKey"/> using the given <see cref="IRandomNumberGenerator"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="rng">The <see cref="IRandomNumberGenerator"/> instance to use</param>
        public PrivateKey(IRandomNumberGenerator rng)
        {
            if (rng is null)
                throw new ArgumentNullException(nameof(rng), "Random number generator can not be null.");

            byte[] temp = new byte[KeyByteSize];
            int count = 0;
            while (count <= Constants.RngRetryCount)
            {
                try
                {
                    rng.GetBytes(temp);
                    SetBytes(temp);
                    break;
                }
                catch (Exception)
                {
                    count++;
                    // This should never happen:
                    if (count == Constants.RngRetryCount)
                        throw new ArgumentException(Err.BadRNG);
                    // TODO: maybe use a mod Curve.N in case bytes were bigger than N (out of range)
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PrivateKey"/> using the given WIF (Base-58 encoded private key
        /// with a checksum)
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="FormatException"/>
        /// <param name="wif">Wallet import format</param>
        /// <param name="netType">
        /// [Default value = <see cref="NetworkType.MainNet"/>]
        /// Network type to check against (affects WIF's starting characters)
        /// </param>
        public PrivateKey(string wif, NetworkType netType = NetworkType.MainNet)
        {
            if (string.IsNullOrWhiteSpace(wif))
                throw new ArgumentNullException(nameof(wif), "Input WIF can not be null or empty.");


            byte[] ba = b58Encoder.DecodeWithCheckSum(wif);
            if (ba[0] != GetWifFirstByte(netType))
            {
                throw new FormatException("Invalid first byte.");
            }

            if (ba.Length == KeyByteSize + 1) // Uncompressed
            {
                SetBytes(ba.SubArray(1));
            }
            else if (ba.Length == KeyByteSize + 2) // Compressed
            {
                if (ba[^1] != CompressedByte)
                {
                    throw new FormatException("Invalid compressed byte.");
                }

                SetBytes(ba.SubArray(1, ba.Length - 2));
            }
            else
            {
                throw new FormatException("Invalid bytes length");
            }
        }



        /// <summary>
        /// Base-58 encoder to convert byte arrays to WIF.
        /// </summary>
        protected readonly Base58 b58Encoder = new Base58();
        private readonly EllipticCurveCalculator calc = new EllipticCurveCalculator();
        private readonly BigInteger minValue = BigInteger.One;
        // Curve.N - 1
        private readonly BigInteger maxValue =
            BigInteger.Parse("115792089237316195423570985008687907852837564279074904382605163141518161494336");
        private const int KeyByteSize = 32;
        private const byte MainNetByte = 128;
        private const byte TestNetByte = 239;
        private const byte RegTestByte = 239;
        private const byte CompressedByte = 1;
        // Don't rename, used by tests with reflection.
        private byte[] keyBytes;



        /// <summary>
        /// Checks the given byte array and sets the keyBytes or throws an exception.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="key">Key bytes to use</param>
        protected void SetBytes(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key bytes can not be null.");

            key = key.TrimStart(); // bytes are treated as big-endian
            if (key.Length > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(key), $"Given key value is bigger than 256 bits.");
            }

            BigInteger num = key.ToBigInt(true, true);
            if (num < minValue || num > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(key), "Given key value is outside the defined range by the curve.");
            }

            keyBytes = new byte[32];
            // due to big-endianness byte array must be padded with initial zeros hence the dstOffset below
            Buffer.BlockCopy(key, 0, keyBytes, dstOffset: 32 - key.Length, count: key.Length);
        }

        /// <exception cref="ArgumentException"/>
        protected byte GetWifFirstByte(NetworkType netType)
        {
            return netType switch
            {
                NetworkType.MainNet => MainNetByte,
                NetworkType.TestNet => TestNetByte,
                NetworkType.RegTest => RegTestByte,
                _ => throw new ArgumentException("Network type is not defined!"),
            };
        }

        /// <exception cref="ObjectDisposedException"/>
        protected virtual void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException("Private key instance was disposed.");
        }


        /// <summary>
        /// Returns the byte sequence of this instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <returns>An array of bytes.</returns>
        public byte[] ToBytes()
        {
            CheckDisposed();

            byte[] ba = new byte[keyBytes.Length];
            Buffer.BlockCopy(keyBytes, 0, ba, 0, keyBytes.Length);
            return ba;
        }


        /// <summary>
        /// Converts the bytes sequence of this instance to its equivalent <see cref="BigInteger"/> representation.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <returns>The <see cref="BigInteger"/> representation of this instance.</returns>
        public BigInteger ToBigInt()
        {
            CheckDisposed();
            return keyBytes.ToBigInt(true, true);
        }


        /// <summary>
        /// Converts this instance to its equivalent <see cref="PublicKey"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <returns>The derived <see cref="PublicKey"/>.</returns>
        public PublicKey ToPublicKey()
        {
            CheckDisposed();
            return new PublicKey(calc.MultiplyByG(ToBigInt()));
        }


        /// <summary>
        /// Converts the bytes sequence of this instance to its <see cref="Base58"/> representation with a checksum.
        /// Also known as Wallet Import Format (WIF).
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>
        /// For versioned WIFs check out <see cref="ImprovementProposals.BIP0178"/>
        /// </remarks>
        /// <param name="compressed">Indicating public key type</param>
        /// <param name="netType">
        /// [Default value = <see cref="NetworkType.MainNet"/>]
        /// Network type to use when encoding the key (affects WIF's starting characters)
        /// </param>
        /// <returns>A <see cref="Base58"/> encoded string (WIF).</returns>
        public string ToWif(bool compressed, NetworkType netType = NetworkType.MainNet)
        {
            CheckDisposed();

            byte[] data = new byte[compressed ? keyBytes.Length + 2 : keyBytes.Length + 1];
            Buffer.BlockCopy(keyBytes, 0, data, 1, keyBytes.Length);

            data[0] = GetWifFirstByte(netType);
            if (compressed)
            {
                data[^1] = CompressedByte;
            }

            return b58Encoder.EncodeWithCheckSum(data);
        }


        /// <summary>
        /// Signs the given UTF-8 encoded message with this key.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="message">Message to sign</param>
        /// <returns>Signature</returns>
        public Signature Sign(string message)
        {
            CheckDisposed();
            // Empty string is OK but we don't accept it
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message), "Message can not be null or empty.");
            // TODO: check out https://github.com/bitcoin/bips/blob/master/bip-0137.mediawiki + 322

            // TODO: convert Sha into a straem itself and directly write to it instead of another stream (similar to tx)
            using Sha256 hash = new Sha256(true);
            FastStream stream = new FastStream();
            stream.Write((byte)Constants.MsgSignConst.Length);
            stream.Write(Encoding.UTF8.GetBytes(Constants.MsgSignConst));
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            new CompactInt(messageBytes.Length).WriteToStream(stream);
            stream.Write(messageBytes);

            byte[] toSign = hash.ComputeHash(stream.ToByteArray());

            return calc.Sign(toSign, keyBytes);
        }


        /// <summary>
        /// Signs the given transaction's input at the given index with the given <see cref="SigHashType"/> and
        /// sets its signature.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="tx">The transaction to sign</param>
        /// <param name="txToSpend">The previous tx that the transaction to sign is spending</param>
        /// <param name="index">Index of the input to sign</param>
        /// <param name="sht">Signature hash type</param>
        public void Sign(ITransaction tx, ITransaction txToSpend, int index, SigHashType sht = SigHashType.All)
        {
            CheckDisposed();
            if (tx == null)
                throw new ArgumentNullException(nameof(tx), "Transaction can not be null!");
            if (txToSpend == null)
                throw new ArgumentNullException(nameof(txToSpend), "Previous out script can not be null!");

            // TODO: change ITransaction.Sign so that it takes public key and does the initial checks 
            // such as correct txout being spent.
            Signature sig = calc.Sign(tx.GetBytesToSign(txToSpend, index, sht), keyBytes);
            sig.SigHash = sht;
            //tx.WriteScriptSig(sig, pubKey, txToSpend, index);
        }



        private bool isDisposed = false;

        /// <summary>
        /// Releases the resources used by the <see cref="PrivateKey"/> class.
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
                    if (keyBytes != null)
                        Array.Clear(keyBytes, 0, keyBytes.Length);
                    keyBytes = null;
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="PrivateKey"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
