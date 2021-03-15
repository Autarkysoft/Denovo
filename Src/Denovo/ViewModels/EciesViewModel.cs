// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Encoders;
using Denovo.Models;
using Denovo.MVVM;
using System;
using System.Linq;
using System.Text;

namespace Denovo.ViewModels
{
    public class EciesViewModel : VmWithSizeBase
    {
        public EciesViewModel() : base(450, 800)
        {
            InputEncodingList = EnumHelper.GetEnumValues<EncodingNames>().ToArray();
            SelectedInputEncoding = EncodingNames.UTF8;

            KeyEncodingList = new[] { EncodingNames.Base58Check, EncodingNames.Base16 };
            SelectedKeyEncoding = KeyEncodingList[0];

            OutputEncodingList = EnumHelper.GetEnumValues<EncodingNames>().ToArray();
            SelectedOutputEncoding = OutputEncodingList[1];
        }

        public EncodingNames[] InputEncodingList { get; }
        public EncodingNames[] KeyEncodingList { get; }
        public EncodingNames[] OutputEncodingList { get; }


        private byte[] Decode(EncodingNames fromEnc, string input)
        {
            return fromEnc switch
            {
                EncodingNames.Base16 => Base16.Decode(input),
                EncodingNames.Base43 => new Base43().Decode(input),
                EncodingNames.Base58 => new Base58().Decode(input),
                EncodingNames.Base58Check => new Base58().DecodeWithCheckSum(input),
                EncodingNames.Base64 => Convert.FromBase64String(input),
                EncodingNames.UTF8 => Encoding.UTF8.GetBytes(input),
                EncodingNames.Unicode => Encoding.Unicode.GetBytes(input),
                _ => throw new ArgumentException("undefined encoding.")
            };
        }

        private string Encode(EncodingNames toEnc, byte[] data)
        {
            return toEnc switch
            {
                EncodingNames.Base16 => Base16.Encode(data),
                EncodingNames.Base43 => new Base43().Encode(data),
                EncodingNames.Base58 => new Base58().Encode(data),
                EncodingNames.Base58Check => new Base58().EncodeWithCheckSum(data),
                EncodingNames.Base64 => Convert.ToBase64String(data),
                EncodingNames.UTF8 => Encoding.UTF8.GetString(data),
                EncodingNames.Unicode => Encoding.Unicode.GetString(data),
                _ => throw new ArgumentException("undefined encoding.")
            };
        }

        private bool ChangeEncoding(EncodingNames fromEnc, EncodingNames toEnc, string input, out string output)
        {
            try
            {
                output = Encode(toEnc, Decode(fromEnc, input));
                return true;
            }
            catch
            {
                output = null;
                return false;
            }
        }

        private EncodingNames _selInEnc;
        public EncodingNames SelectedInputEncoding
        {
            get => _selInEnc;
            set => SetField(ref _selInEnc, value);
        }

        private EncodingNames _selKeyEnc;
        public EncodingNames SelectedKeyEncoding
        {
            get => _selKeyEnc;
            set => SetField(ref _selKeyEnc, value);
        }

        private EncodingNames _selOutEnc;
        public EncodingNames SelectedOutputEncoding
        {
            get => _selOutEnc;
            set
            {
                if (value != _selOutEnc)
                {
                    try
                    {
                        Error = string.Empty;
                        ChangeEncoding(_selOutEnc, value, Output, out string temp);
                        Output = temp;
                    }
                    catch (Exception ex)
                    {
                        Error = $"Failed to change encoding: {ex.Message}";
                    }

                    SetField(ref _selOutEnc, value);
                }
            }
        }


        private string _input;
        public string Input
        {
            get => _input;
            set => SetField(ref _input, value);
        }

        private string _key;
        public string Key
        {
            get => _key;
            set => SetField(ref _key, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetField(ref _output, value);
        }

        private string _err;
        public string Error
        {
            get => _err;
            set => SetField(ref _err, value);
        }


        public void Decrypt()
        {
            Error = string.Empty;
            try
            {
                using PrivateKey prv = SelectedKeyEncoding == EncodingNames.Base16 ?
                                       new PrivateKey(Base16.Decode(Key)) :
                                       new PrivateKey(Key);
                // TODO: change Decrypt to accept byte[] by default
                byte[] inputBytes = Decode(SelectedInputEncoding, Input);
                byte[] result = Decode(EncodingNames.UTF8, prv.Decrypt(inputBytes.ToBase64()));
                Output = Encode(SelectedOutputEncoding, result);
            }
            catch (Exception ex)
            {
                Output = string.Empty;
                Error = $"Failed: {ex.Message}";
            }
        }

        public void Encrypt()
        {
            Error = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(Input))
                {
                    Error = "Message is empty";
                    return;
                }

                byte[] pubBa = SelectedKeyEncoding switch
                {
                    EncodingNames.Base16 => Base16.Decode(Key),
                    EncodingNames.Base58Check => new Base58().DecodeWithCheckSum(Key),
                    _ => null
                };

                if (pubBa == null)
                {
                    Error = "Undefined encoding.";
                }
                else if (PublicKey.TryRead(pubBa, out PublicKey pub))
                {
                    // TODO: change Encrypt to return byte[]
                    byte[] inputBytes = Decode(SelectedInputEncoding, Input);
                    byte[] result = Decode(EncodingNames.Base64, pub.Encrypt(Encoding.UTF8.GetString(inputBytes)));
                    Output = Encode(SelectedOutputEncoding, result);
                }
                else
                {
                    Output = string.Empty;
                    Error = "Failed: invalid public key.";
                }
            }
            catch (Exception ex)
            {
                Output = string.Empty;
                Error = $"Failed: {ex.Message}";
            }
        }
    }
}
