// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Encoders;
using System;

namespace Autarkysoft.Bitcoin.Experimental
{
    /// <summary>
    /// Contains experimental ideas for encoding private keys using the Bech32 encoder
    /// </summary>
    public class Bech32PrivateKey
    {
        /// <summary>
        /// Type of the script and corresponding address created from the private key
        /// </summary>
        public enum ScriptType
        {
            /// <summary>
            /// P2PKH script created from the uncompressed public key
            /// </summary>
            UncompressedP2pkh,
            /// <summary>
            /// P2PKH script created from the compressed public key
            /// </summary>
            CompressedP2pkh,
            /// <summary>
            /// P2WPKH script for version 0 witness program (created from the compressed public key)
            /// </summary>
            P2wpkh,
            /// <summary>
            /// P2SH-P2WPKH script for version 0 witness program (created from the compressed public key)
            /// </summary>
            P2sh_P2wpkh
        }


        /// <summary>
        /// Converts the special bech-32 encoded private key back to its byte array representation.
        /// </summary>
        /// <param name="bech">Special bech-32 encoded private key</param>
        /// <param name="scrT">Corresponding script type</param>
        /// <param name="hrp">Human readable part</param>
        /// <param name="creationDate">The time this key was created (can help speed up re-scanning)</param>
        /// <returns>Private key as an array of bytes</returns>
        public byte[] Decode(string bech, out ScriptType scrT, out string hrp, out DateTime creationDate)
        {
            var data = Bech32.Decode(bech, Bech32.Mode.B32m, out byte ver, out hrp);
            scrT = (ScriptType)ver;
            creationDate = UnixTimeStamp.EpochToTime(data[32] | 
                                                    (long)data[33] << 8 | 
                                                    (long)data[34] << 16 | 
                                                    (long)data[35] << 24 | 
                                                    (long)data[36] << 32);
            return data.SubArray(0, 32);
        }

        /// <summary>
        /// Converts the given private key WIF to a special bech-32 encoded string with script type, time and a checksum.
        /// </summary>
        /// <param name="wif">Wallet import format</param>
        /// <param name="scrT">Corresponding script type</param>
        /// <param name="creationDate"></param>
        /// <param name="netType">The time this key was created (can help speed up re-scanning)</param>
        /// <param name="hrp">Human readable part</param>
        /// <returns>A special bech-32 encoded private key</returns>
        public string Encode(string wif, ScriptType scrT, DateTime creationDate,
                             NetworkType netType = NetworkType.MainNet, string hrp = "bprv")
        {
            using PrivateKey key = new PrivateKey(wif, netType);

            byte[] data = new byte[32 + 5];
            Buffer.BlockCopy(key.ToBytes(), 0, data, 0, 32);
            long val = UnixTimeStamp.TimeToEpoch(creationDate);
            data[32] = (byte)val;
            data[33] = (byte)(val >> 8);
            data[34] = (byte)(val >> 16);
            data[35] = (byte)(val >> 24);
            data[36] = (byte)(val >> 32);

            return Bech32.Encode(data, Bech32.Mode.B32m, (byte)scrT, hrp);
        }
    }
}
