// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Denovo.MVVM;
using System;
using System.Reflection;

namespace Denovo.ViewModels
{
    public class AboutViewModel : VmWithSizeBase
    {
        public AboutViewModel() : base(400, 600)
        {
            CopyCommand = new(Copy);
            OpenBrowserCommand = new(OpenBrowser);
        }

        public AboutViewModel(IClipboard? clipboard, ILauncher launcher) : this()
        {
            Clipboard = clipboard;
            Launcher = launcher;
        }


        public IClipboard? Clipboard { get; set; }
        public ILauncher? Launcher { get; set; }
        public string NameAndVersion => $"Denovo {Assembly.GetExecutingAssembly().GetName().Version?.ToString(4)}";
        public string SourceLink => "https://github.com/Autarkysoft/Denovo";
        public string BitcointalkLink => "";
        public string AvaloniaLink => "https://avaloniaui.net/";
        public string DonationUri1 => $"bitcoin:{DonationAddr1}{Bip21Extras}";
        public string DonationAddr1 => "1Q9swRQuwhTtjZZ2yguFWk7m7pszknkWyk";
        public string DonationUri2 => $"bitcoin:{DonationAddr2}{Bip21Extras}";
        public string DonationAddr2 => "bc1q3n5t9gv40ayq68nwf0yth49dt5c799wpld376s";

        private const string Bip21Extras = "?label=Coding-Enthusiast&message=Donation%20for%20Denovo%20project";


        public BindableCommand<string> CopyCommand { get; }
        public async void Copy(string i)
        {
            if (Clipboard is not null)
            {
                await Clipboard.SetTextAsync(i == "1" ? DonationAddr1 : DonationAddr2);
            }
        }

        public BindableCommand<string> OpenBrowserCommand { get; }
        public async void OpenBrowser(string url)
        {
            if (Launcher is not null)
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
        }
    }
}
