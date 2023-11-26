// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Denovo.ViewModels;
using Denovo.Views;

namespace Denovo
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                NetworkType network = NetworkType.MainNet;
                if (desktop.Args.Length > 1)
                {
                    for (int i = 0; i < desktop.Args.Length; i++)
                    {
                        if (desktop.Args.Length - i >= 2)
                        {
                            if (desktop.Args[i].StartsWith('-'))
                            {
                                if (desktop.Args[i] == "-n" || desktop.Args[i] == "-network")
                                {
                                    string value = desktop.Args[i + 1].ToLower();
                                    if (value == "testnet")
                                    {
                                        network = NetworkType.TestNet;
                                    }
                                    else if (value == "regtest")
                                    {
                                        network = NetworkType.RegTest;
                                    }
                                }
                                // Add more options here
                            }
                            i++;
                        }
                    }
                }

                MainWindowViewModel vm = new(network);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = vm
                };
                vm.Clipboard = desktop.MainWindow.Clipboard;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
