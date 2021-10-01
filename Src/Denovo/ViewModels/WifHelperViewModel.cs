// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Denovo.MVVM;
using System;
using System.Linq;

namespace Denovo.ViewModels
{
    public class WifHelperViewModel : VmWithSizeBase
    {
        public WifHelperViewModel() : base(400, 550)
        {
            ConversionType = EnumHelper.GetAllEnumValues<ConversionTypes>().ToArray();
            SelectedConversionType = ConversionType[0];
        }


        public enum ConversionTypes
        {
            WifToWords,
            WordsToWif,
            VersionWifToWif,
            ElectrumVersionWifToWif,
        }

        public ConversionTypes[] ConversionType { get; }

        private ConversionTypes _selConvT;
        public ConversionTypes SelectedConversionType
        {
            get => _selConvT;
            set => SetField(ref _selConvT, value);
        }

        private string _input;
        public string Input
        {
            get => _input;
            set => SetField(ref _input, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetField(ref _output, value);
        }

        public void Convert()
        {
            if (SelectedConversionType == ConversionTypes.WifToWords)
            {
                try
                {
                    using PrivateKey key = new(Input);
                    byte[] ba = key.ToBytes();
                    using BIP0039 bip = new(ba);
                    Output = bip.ToMnemonic();
                }
                catch (Exception ex)
                {
                    Output = $"An error occured: {ex.Message}";
                }
            }
            else if (SelectedConversionType == ConversionTypes.WordsToWif)
            {
                try
                {
                    using PrivateKey key = ConvertBip39();
                    Output = key.ToWif(true);
                }
                catch (Exception ex)
                {
                    Output = $"An error occured: {ex.Message}";
                }
            }
            else if (SelectedConversionType == ConversionTypes.VersionWifToWif ||
                     SelectedConversionType == ConversionTypes.ElectrumVersionWifToWif)
            {
                bool isElec = (SelectedConversionType == ConversionTypes.ElectrumVersionWifToWif);
                try
                {
                    BIP0178 bip = new();
                    using PrivateKey key = isElec ? bip.DecodeElectrumVersionedWif(Input) : bip.Decode(Input);
                    Output = key.ToWif(true);
                }
                catch (Exception ex)
                {
                    Output = $"An error occured: {ex.Message}";
                }
            }
            else
            {
                Output = "Not yet defined.";
            }
        }

        private PrivateKey ConvertBip39()
        {
            // Quick test to see if input is valid
            using BIP0039 bip = new(Input);

            string[] allWords = BIP0039.GetAllWords(BIP0039.WordLists.English);
            string[] words = bip.ToMnemonic().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            uint[] wordIndexes = new uint[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                wordIndexes[i] = (uint)Array.IndexOf(allWords, words[i]);
            }

            // Compute and check checksum
            int MS = words.Length;
            int ENTCS = MS * 11;
            int CS = ENTCS % 32;
            int ENT = ENTCS - CS;

            byte[] entropy = new byte[ENT / 8];

            int itemIndex = 0;
            int bitIndex = 0;
            // Number of bits in a word
            int toTake = 8;
            // Indexes are held in a UInt32 but they are only 11 bits
            int maxBits = 11;
            for (int i = 0; i < entropy.Length; i++)
            {
                if (bitIndex + toTake <= maxBits)
                {
                    entropy[i] = (byte)(wordIndexes[itemIndex] >> (3 - bitIndex));
                }
                else
                {
                    entropy[i] = (byte)(((wordIndexes[itemIndex] << (bitIndex - 3)) & 0xff) |
                                         (wordIndexes[itemIndex + 1] >> (14 - bitIndex)));
                }

                bitIndex += toTake;
                if (bitIndex >= maxBits)
                {
                    bitIndex -= maxBits;
                    itemIndex++;
                }
            }

            return new PrivateKey(entropy);
        }
    }
}
