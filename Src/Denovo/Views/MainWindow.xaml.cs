// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Denovo.ViewModels;

namespace Denovo.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Opened += MainWindow_Opened;
        }

        private void MainWindow_Opened(object sender, System.EventArgs e)
        {
            // Previewer doesn't have datacontext and vm will be null, checking like this prevents an exception being thrown
            if (DataContext is MainWindowViewModel vm && !vm.IsInitialized)
            {
                vm.OpenConfig();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
