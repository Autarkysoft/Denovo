// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// URI Scheme
    /// <para>https://github.com/bitcoin/bips/blob/master/bip-0021.mediawiki</para>
    /// </summary>
    public class BIP0021
    {
        private BIP0021()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BIP0021"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">Coin address to make payment to (address will not be checked for validity)</param>
        /// <param name="coin">[Default value = bitcoin] Name of the coin to use as scheme</param>
        public BIP0021(string address, string coin = "bitcoin")
        {
            Scheme = coin;
            Address = address;
        }



        // According to https://tools.ietf.org/html/rfc3986#section-3.1
        private const string AllowedSchemeChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+.-";

        private string _scheme;
        /// <summary>
        /// The URI scheme string, that would be the coin name (eg. bitcoin).
        /// It will lower case the input and remove all extra white spaces.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        public string Scheme
        {
            get => _scheme;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(Scheme), "Scheme can not be null or empty.");
                value = value.ToLower().Trim();
                if (!char.IsLetter(value[0]))
                    throw new FormatException("Scheme must begin with a letter.");
                if (!value.All(c => AllowedSchemeChars.Contains(c)))
                    throw new FormatException("Scheme contains invalid characters.");

                _scheme = value;
            }
        }

        private string _addr;
        /// <summary>
        /// The address to use. The validity of the address is not checked.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public string Address
        {
            get => _addr;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(Address), "Address can not be null or empty.");

                _addr = value;
            }
        }

        private decimal _amount;
        /// <summary>
        /// [optional] Amount of coin to pay.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Amount can not be negative.");

                _amount = value;
            }
        }

        /// <summary>
        /// [optional] A label to use for transaction in clients.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// [optional] Additional message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// [optional] Anny additonal options that will be encoded as "key=value"
        /// </summary>
        public readonly Dictionary<string, string> AdditionalOptions = new Dictionary<string, string>();



        /// <summary>
        /// Decodes a given bip-21 encoded string into a new instance of <see cref="BIP0021"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="bip21String">Bip-21 encoded string.</param>
        public static BIP0021 Decode(string bip21String)
        {
            if (string.IsNullOrWhiteSpace(bip21String))
                throw new ArgumentNullException(nameof(bip21String), "Input can not be null or empty.");


            int schemeSepIndex = bip21String.IndexOf(':');
            int optionSepIndex = bip21String.IndexOf('?');

            if (schemeSepIndex < 0)
            {
                throw new FormatException("No scheme separator was found.");
            }

            int addrEndIndex = (optionSepIndex < 0) ?
                                bip21String.Length - schemeSepIndex - 1 :
                                optionSepIndex - schemeSepIndex - 1;

            var result = new BIP0021
            {
                Scheme = bip21String.Substring(0, schemeSepIndex),
                Address = bip21String.Substring(schemeSepIndex + 1, addrEndIndex)
            };

            if (optionSepIndex > 0)
            {
                string[] optionals = bip21String.Substring(optionSepIndex + 1).Split('&');
                foreach (var item in optionals)
                {
                    int i = item.IndexOf('=');
                    if (i < 0)
                    {
                        continue;
                    }
                    string command = item.Substring(0, i);
                    string value = item.Substring(i + 1);
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    switch (command.ToLower())
                    {
                        case "amount":
                            if (decimal.TryParse(value, out decimal d))
                                result.Amount = d;
                            break;
                        case "label":
                            result.Label = Uri.UnescapeDataString(value);
                            break;
                        case "message":
                            result.Message = Uri.UnescapeDataString(value);
                            break;
                        default:
                            result.AdditionalOptions.TryAdd(command, value);
                            break;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Encodes this instance into its string representaion.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public string Encode()
        {
            StringBuilder optional = new StringBuilder();

            bool hasAnyOptional = false;
            if (Amount != 0)
            {
                string seperator = hasAnyOptional ? "&" : "?";
                optional.Append($"{seperator}amount={Amount}");
                hasAnyOptional = true;
            }
            if (!string.IsNullOrWhiteSpace(Label))
            {
                string seperator = hasAnyOptional ? "&" : "?";
                optional.Append($"{seperator}label={Uri.EscapeDataString(Label)}");
                hasAnyOptional = true;
            }
            if (!string.IsNullOrWhiteSpace(Message))
            {
                string seperator = hasAnyOptional ? "&" : "?";
                optional.Append($"{seperator}message={Uri.EscapeDataString(Message)}");
                hasAnyOptional = true;
            }
            if (AdditionalOptions.Count != 0)
            {
                foreach (var item in AdditionalOptions)
                {
                    string seperator = hasAnyOptional ? "&" : "?";
                    optional.Append($"{seperator}{item.Key}={Uri.EscapeDataString(item.Value)}");
                    hasAnyOptional = true;
                }
            }

            return $"{Scheme}:{Address}{optional.ToString()}";
        }
    }
}
