// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Encoders
{
    /// <summary>
    /// Base-43 is a special encoding based on <see cref="Base58"/> encoding that Electrum uses to encode transactions 
    /// (without the checksum) before turning them into QR code for a smaller result.
    /// </summary>
    public class Base43 : Base58
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Base43"/>.
        /// </summary>
        public Base43()
        {
            // https://github.com/spesmilo/electrum/blob/b39c51adf7ef9d56bd45b1c30a86d4d415ef7940/electrum/bitcoin.py#L428
            b58Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ$*+-./:";

            baseValue = 43;
            logBaseValue = 679;
        }
    }
}
