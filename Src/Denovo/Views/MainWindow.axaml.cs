// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Avalonia.Controls;
using Denovo.ViewModels;
using System;

namespace Denovo.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Opened += MainWindow_Opened;
        }

        private void MainWindow_Opened(object? sender, EventArgs e)
        {
            // Previewer doesn't have datacontext and vm will be null, checking like this prevents an exception being thrown
            if (DataContext is MainWindowViewModel vm && !vm.IsInitialized)
            {
                vm.OpenConfig();
            }
        }
    }
}
