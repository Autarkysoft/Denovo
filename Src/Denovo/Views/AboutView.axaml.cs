// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Denovo.Views
{
    public partial class AboutView : UserControl
    {
        public AboutView() => InitializeComponent();
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
