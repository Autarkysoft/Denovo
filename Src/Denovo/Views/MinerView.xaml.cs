using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Denovo.Views
{
    public class MinerView : UserControl
    {
        public MinerView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
