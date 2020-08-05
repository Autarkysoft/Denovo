// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Hierarchical Deterministic Wallets. Implements <see cref="IDisposable"/>.
    /// <para/>https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki
    /// </summary>
    public class BIP0032 : IDisposable
    {
        /// <summary>
        /// An empty constructor to let this class be inherited.
        /// </summary>
        protected BIP0032() { }

        /// <summary>
        /// Initializes a new instance of <see cref="BIP0032"/> with the given entropy.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="entropy">Entropy to use (must be between 128 and 512 bits; ie. 16 and 64 bytes)</param>
        public BIP0032(byte[] entropy)
        {
            if (entropy == null)
                throw new ArgumentNullException(nameof(entropy), "Entropy can not be null.");
            if (entropy.Length < MinEntropyLength || entropy.Length > MaxEntropyLength)
                throw new ArgumentOutOfRangeException(nameof(entropy), "Entropy must be between 16 and 64 bytes.");
            SetEntropy(entropy);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BIP0032"/> with an entropy derived from the given
        /// <see cref="IRandomNumberGenerator"/> and the entropy size.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="rng">RNG to use</param>
        /// <param name="entropySize">
        /// [Default value = 32]
        /// Size of the entropy (must be between 128 and 512 bits; ie. 16 and 64 bytes).
        /// </param>
        public BIP0032(IRandomNumberGenerator rng, int entropySize = 32)
        {
            if (rng is null)
                throw new ArgumentNullException(nameof(rng), "Random number generator can not be null.");
            if (entropySize < MinEntropyLength || entropySize > MaxEntropyLength)
                throw new ArgumentOutOfRangeException(nameof(entropySize), "Entropy size must be between 16 and 64 bytes.");


            byte[] entropy = new byte[entropySize];
            int count = 0;
            while (count <= Constants.RngRetryCount)
            {
                try
                {
                    rng.GetBytes(entropy);
                    SetEntropy(entropy);
                    break;
                }
                catch (Exception)
                {
                    // We should never land here for curves like secp256k1 because of how close N and P are.
                    count++;
                    // This will only throw if the RNG is broken.
                    if (count == Constants.RngRetryCount)
                        throw new ArgumentException(Err.BadRNG);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BIP0032"/> using the given base-58 encoded 
        /// extended public or private key string.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException">If given private key is outside of curve range.</exception>
        /// <exception cref="FormatException"/>
        /// <param name="extendedKey">Base-58 encoded extended key to use</param>
        /// <param name="netType">
        /// [Default value = <see cref="NetworkType.MainNet"/>]
        /// The expected network that this extended key belongs to.
        /// </param>
        public BIP0032(string extendedKey, NetworkType netType = NetworkType.MainNet)
        {
            if (string.IsNullOrWhiteSpace(extendedKey))
                throw new ArgumentNullException(nameof(extendedKey), "Extended key can not be null or empty.");


            byte[] decoded = b58Encoder.DecodeWithCheckSum(extendedKey);
            if (decoded.Length != ExtendedKeyLength)
            {
                throw new FormatException($"Extended key length should be {ExtendedKeyLength} bytes " +
                    $"but it is {decoded.Length} bytes.");
            }

            Span<byte> ver = decoded.SubArray(0, 4);
            bool isPublic;
            if (netType == NetworkType.MainNet)
            {
                if (!ver.SequenceEqual(prvMainVer) && !ver.SequenceEqual(pubMainVer))
                {
                    throw new FormatException("Unknown extended key version.");
                }
                isPublic = ver.SequenceEqual(pubMainVer);
            }
            else if (netType == NetworkType.TestNet || netType == NetworkType.RegTest)
            {
                if (!ver.SequenceEqual(prvTestVer) && !ver.SequenceEqual(pubTestVer))
                {
                    throw new FormatException("Unknown extended key version.");
                }
                isPublic = !ver.SequenceEqual(pubTestVer);
            }
            else
            {
                throw new ArgumentException("Invalid network type.");
            }


            ExtendedKeyDepth = decoded[4];
            ParentFingerPrint = decoded.SubArray(5, 4);
            ChildNumber = decoded.SubArray(9, 4);
            ChainCode = decoded.SubArray(13, 32);

            byte[] key = decoded.SubArray(45, 33);

            if (!isPublic && key[0] != 0)
            {
                throw new FormatException($"The key has an invalid first byte, " +
                    $"it should be 0 for private keys but it is 0x{key[0]:x2}.");
            }

            if (isPublic)
            {
                PrvKey = null;
                if (!PublicKey.TryRead(key, out PubKey))
                {
                    throw new ArgumentOutOfRangeException("public key", "Invalid public key format.");
                }
            }
            else
            {
                // The following line will check if the key is valid and throws ArgumentOutOfRangeException if not
                PrvKey = new PrivateKey(key.SubArray(1));
                PubKey = PrvKey.ToPublicKey();
            }
        }



        // 2^31
        internal const uint HardenedIndex = 0x80000000;
        // version(4) + depth(1) + fingerPrint(4) + index(4) + chain(32) + key(33)
        internal const int ExtendedKeyLength = 78;
        private const int MinEntropyLength = 16;
        private const int MaxEntropyLength = 64;
        private readonly byte[] prvMainVer = { 0x04, 0x88, 0xad, 0xe4 };
        private readonly byte[] pubMainVer = { 0x04, 0x88, 0xb2, 0x1e };
        private readonly byte[] prvTestVer = { 0x04, 0x35, 0x83, 0x94 };
        private readonly byte[] pubTestVer = { 0x04, 0x35, 0x87, 0xcf };

        private readonly BigInteger N = new SecP256k1().N;

        // TODO: hardcode this value?
        private readonly byte[] MasterKeyHashKey = Encoding.UTF8.GetBytes("Bitcoin seed");

        private HmacSha512 hmac = new HmacSha512();
        private Base58 b58Encoder = new Base58();

        private byte ExtendedKeyDepth;
        internal byte[] ParentFingerPrint;
        internal byte[] ChildNumber;
        private byte[] ChainCode;
        private PrivateKey PrvKey;
        private PublicKey PubKey;



        /// <summary>
        /// Initializes this instance using the given entropy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Is thrown if the left 32 byte chunk of the HMAC result is not a valid private key.
        /// </exception>
        /// <param name="entropy">Entropy to use (it must be between 128 and 512 bits; ie. 16 and 64 bytes) </param>
        protected void SetEntropy(byte[] entropy)
        {
            HmacAndSplitData(entropy, MasterKeyHashKey, out byte[] il, out byte[] ir);

            // Master key doesn't have the (+previous private key % N), we use 'il' itself
            // The following line will check if the key is valid and throws ArgumentOutOfRangeException if not
            PrvKey = new PrivateKey(il);
            PubKey = PrvKey.ToPublicKey();
            ChainCode = ir;

            ExtendedKeyDepth = 0;
            ParentFingerPrint = new byte[4];
            ChildNumber = new byte[4];
        }


        private void HmacAndSplitData(byte[] data, byte[] key, out byte[] left32, out byte[] right32)
        {
            byte[] hash = hmac.ComputeHash(data, key);
            left32 = hash.SubArray(0, 32);
            right32 = hash.SubArray(32);
        }

        private bool IsHardendedIndex(uint i) => (i & HardenedIndex) != 0;


        public PrivateKey[] GetPrivateKeys(BIP0032Path path, uint count, uint startIndex = 0, uint step = 1)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0032));
            if (path is null)
                throw new ArgumentNullException(nameof(path), "Path can not be null!");
            if (PrvKey is null)
                throw new ArgumentNullException(nameof(PrvKey), "Can not get child private keys from extended public key.");
            if (ExtendedKeyDepth == byte.MaxValue || ExtendedKeyDepth + path.Indexes.Length + 1 > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(ExtendedKeyDepth), "Can not get children since " +
                    "depth will be bigger than 1 byte");

            // Two quick fixes:
            if (count == 0)
                return null;
            if (step < 1)
                step = 1;

            // First start deriving the extended keys for each index
            byte[] prevPrvBa = PrvKey.ToBytes();
            BigInteger prevPrvInt = PrvKey.ToBigInt();
            byte[] prevPubBa = PubKey.ToByteArray(true);
            byte[] tempCC = ChainCode;
            byte[] tempLeft;
            foreach (var index in path.Indexes)
            {
                FastStream stream = new FastStream(33 + 4);
                if (IsHardendedIndex(index))
                {
                    stream.Write((byte)0);
                    stream.Write(prevPrvBa);
                }
                else
                {
                    stream.Write(prevPubBa);
                }
                stream.WriteBigEndian(index);
                HmacAndSplitData(stream.ToByteArray(), tempCC, out tempLeft, out tempCC);

                BigInteger kTemp = tempLeft.ToBigInt(true, true);
                // Note that we throw an exception here if the values were invalid (highly unlikely)
                // because it is the extended keys, we can't skip anything here.
                if (kTemp == 0 || kTemp >= N)
                {
                    throw new ArgumentException();
                }
                // Let PrivateKey do the additional checks and throw here
                PrivateKey temp = new PrivateKey((kTemp + prevPrvInt) % N);
                prevPrvInt = temp.ToBigInt();
                prevPrvBa = temp.ToBytes();
                prevPubBa = temp.ToPublicKey().ToByteArray(true);
            }

            // Then derive the actual keys
            PrivateKey[] result = new PrivateKey[count];
            int i = 0;
            uint childIndex = startIndex;
            while (i < count)
            {
                try
                {
                    FastStream stream = new FastStream(33 + 4);
                    if (IsHardendedIndex(childIndex))
                    {
                        stream.Write((byte)0);
                        stream.Write(prevPrvBa);
                    }
                    else
                    {
                        stream.Write(prevPubBa);
                    }
                    stream.WriteBigEndian(childIndex);
                    HmacAndSplitData(stream.ToByteArray(), tempCC, out tempLeft, out _);

                    BigInteger kTemp = tempLeft.ToBigInt(true, true);
                    // Note: we don't throw any exceptions here. We simply ignore invalid values (highly unlikely)
                    // and move on to the next index. The returned value will always be filled with expected number of items.
                    if (kTemp == 0 || kTemp >= N)
                    {
                        continue;
                    }
                    PrivateKey temp = new PrivateKey((kTemp + prevPrvInt) % N);
                    result[i++] = temp;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Only ignore this type that is thrown by the PrivateKey constructor.
                }
                finally
                {
                    childIndex += step;
                }
            }
            return result;
        }


        public PublicKey[] GetPublicKeys(BIP0032Path path, uint count, uint startIndex = 0, uint step = 1)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0032));
            if (path is null)
                throw new ArgumentNullException(nameof(path), "Path can not be null!");
            if (ExtendedKeyDepth == byte.MaxValue || ExtendedKeyDepth + path.Indexes.Length + 1 > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(ExtendedKeyDepth), "Can not get children since " +
                    "depth will be bigger than 1 byte");

            PublicKey[] result = new PublicKey[count];
            if (!(PrvKey is null))
            {
                PrivateKey[] tempPK = GetPrivateKeys(path, count, startIndex, step);
                for (int j = 0; j < tempPK.Length; j++)
                {
                    result[j] = tempPK[j].ToPublicKey();
                    tempPK[j].Dispose();
                }
                return result;
            }

            // If we are here it means PrivateKey was null
            bool anyHardened = path.Indexes.Any(x => IsHardendedIndex(x));
            if (anyHardened || IsHardendedIndex(startIndex))
            {
                throw new ArgumentException();
            }

            // Two quick fixes:
            if (count == 0)
                return null;
            if (step < 1)
                step = 1;

            // First start deriving the extended keys for each index
            BigInteger prevPrvInt = PrvKey.ToBigInt();
            byte[] prevPubBa = PubKey.ToByteArray(true);
            EllipticCurvePoint prevPubPoint = PubKey.ToPoint();
            byte[] tempCC = ChainCode;
            byte[] tempLeft;
            EllipticCurveCalculator calc = new EllipticCurveCalculator();
            foreach (var index in path.Indexes)
            {
                // There is no hardened indexes thanks to first check
                FastStream stream = new FastStream(33 + 4);
                stream.Write(prevPubBa);
                stream.WriteBigEndian(index);
                HmacAndSplitData(stream.ToByteArray(), tempCC, out tempLeft, out tempCC);

                BigInteger kTemp = tempLeft.ToBigInt(true, true);
                // Note that we throw an exception here if the values were invalid (highly unlikely)
                // because it is the extended keys, we can't skip anything here.
                if (kTemp == 0 || kTemp >= N)
                {
                    throw new ArgumentException();
                }
                EllipticCurvePoint pt = calc.AddChecked(calc.MultiplyByG(kTemp), prevPubPoint);
                PublicKey tempPub = new PublicKey(pt);
                prevPubPoint = tempPub.ToPoint();
                prevPubBa = tempPub.ToByteArray(true);
            }

            // Then derive the actual keys

            int i = 0;
            uint childIndex = startIndex;
            while (i < count)
            {
                // There is no hardened indexes thanks to first check
                FastStream stream = new FastStream(33 + 4);
                stream.Write(prevPubBa);
                stream.WriteBigEndian(childIndex);
                HmacAndSplitData(stream.ToByteArray(), tempCC, out tempLeft, out _);

                BigInteger kTemp = tempLeft.ToBigInt(true, true);
                // Note: we don't throw any exceptions here. We simply ignore invalid values (highly unlikely)
                // and move on to the next index. The returned value will always be filled with expected number of items.
                if (kTemp == 0 || kTemp >= N)
                {
                    continue;
                }
                EllipticCurvePoint pt = calc.AddChecked(calc.MultiplyByG(kTemp), prevPubPoint);
                PublicKey temp = new PublicKey(pt);
                result[i++] = temp;

                childIndex += step;
            }
            return result;
        }


        /// <summary>
        /// Returns base58-encoded string representation of this instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="getPublic">If true returns extended public key, otherwise extended private key if possible.</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        /// <returns>Base58-encoded extended key</returns>
        public string ToBase58(bool getPublic, NetworkType netType = NetworkType.MainNet)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0032), "Instance was disposed.");
            if (!getPublic && PrvKey is null)
                throw new ArgumentNullException(nameof(PrvKey), "Can not get extended private key from public key.");

            byte[] ver = netType switch
            {
                NetworkType.MainNet => getPublic ? pubMainVer : prvMainVer,
                NetworkType.TestNet => getPublic ? pubTestVer : prvTestVer,
                NetworkType.RegTest => getPublic ? pubTestVer : prvTestVer,
                _ => throw new ArgumentException("Network type is not defined."),
            };

            FastStream stream = new FastStream(ExtendedKeyLength);
            stream.Write(ver);
            stream.Write(ExtendedKeyDepth);
            stream.Write(ParentFingerPrint);
            stream.Write(ChildNumber);
            stream.Write(ChainCode);
            if (getPublic)
            {
                stream.Write(PubKey.ToByteArray(true));
            }
            else
            {
                stream.Write((byte)0);
                stream.Write(PrvKey.ToBytes());
            }

            return b58Encoder.EncodeWithCheckSum(stream.ToByteArray());
        }



        private bool isDisposed = false;

        /// <summary>
        /// Releases the resources used by the <see cref="BIP0032"/> class.
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
                    if (!(hmac is null))
                        hmac.Dispose();
                    hmac = null;

                    if (!(PrvKey is null))
                        PrvKey.Dispose();
                    PrvKey = null;

                    PubKey = null;

                    if (ChainCode != null)
                        Array.Clear(ChainCode, 0, ChainCode.Length);
                    ChainCode = null;

                    if (ParentFingerPrint != null)
                        Array.Clear(ParentFingerPrint, 0, ParentFingerPrint.Length);
                    ParentFingerPrint = null;

                    if (ChildNumber != null)
                        Array.Clear(ChildNumber, 0, ChildNumber.Length);
                    ChildNumber = null;

                    b58Encoder = null;
                }

                isDisposed = true;
            }
        }


        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="BIP0032"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
