// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Denovo.ViewModels;
using System.Diagnostics;

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
            var vm = DataContext as MainWindowViewModel;

            Debug.Assert(vm is not null);

            if (!vm.IsInitialized)
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
