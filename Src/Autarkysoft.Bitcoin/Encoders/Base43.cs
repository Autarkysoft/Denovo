// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Encoders
{
    /// <summary>
    /// Base-43 is a special encoding based on <see cref="Base58"/> encoding that Electrum uses to encode transactions 
    /// (without the checksum) before turning them into QR code for a smaller result.
    /// </summary>
    public static class Base43
    {
        /// <summary>
        /// The 43 characters used by this encoding
        /// </summary>
        /// <remarks>
        /// 0-9 and A-Z and $*+-./:
        /// <para/>https://github.com/spesmilo/electrum/blob/b39c51adf7ef9d56bd45b1c30a86d4d415ef7940/electrum/bitcoin.py#L428
        /// </remarks>
        public const string CharSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ$*+-./:";

        /// <summary>
        /// Checks to see if a given string is a valid base-43 encoded string (without checking the checksum).
        /// </summary>
        /// <param name="encoded">String to check</param>
        /// <returns>True if input was a valid base-43 encoded string; otherwise false.</returns>
        public static bool IsValid(string encoded) => Base58.HasValidChars(encoded, Base58.Mode.B43);


        /// <summary>
        /// Converts a base-43 encoded string back to its byte array representation.
        /// </summary>
        /// <exception cref="FormatException"/>
        /// <param name="encoded">Base-58 encoded string.</param>
        /// <returns>Byte array of the given string.</returns>
        public static byte[] Decode(string encoded)
        {
            if (!Base58.HasValidChars(encoded, Base58.Mode.B43))
                throw new FormatException("Input is not a valid Base-43 encoded string.");

            return Base58.DecodeWithoutValidation(encoded, Base58.Mode.B43);
        }


        /// <summary>
        /// Converts the given byte array to its equivalent string representation that is encoded with base-58 digits.
        /// </summary>
        /// <remarks>
        /// Unlike Decode functions, using BigInteger here makes things slightly faster. 
        /// The difference will be more noticeable with larger byte arrays such as extended keys (BIP32).
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">Byte array to encode.</param>
        /// <returns>The string representation in base-58.</returns>
        public static string Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Input can not be null.");

            return Base58.Encode(data, Base58.Mode.B43);
        }
    }
}
