// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Denovo.ViewModels;
using System.Threading.Tasks;

namespace Denovo.Services
{
    public interface IWindowManager
    {
        Task ShowDialog(VmWithSizeBase vm);
    }


    public class WindowManager : IWindowManager
    {
        public Task ShowDialog(VmWithSizeBase vm)
        {
            Window win = new()
            {
                Content = vm,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Width = vm.Width,
                Height = vm.Height,
                Title = vm.GetType().Name.Replace("ViewModel", ""),
            };

            vm.CLoseEvent += (s, e) => win.Close();

            var lf = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            return win.ShowDialog(lf.MainWindow);
        }
    }
}
