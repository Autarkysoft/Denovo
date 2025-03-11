// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Denovo.Models;
using Denovo.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Denovo.Services
{
    public interface IWindowManager
    {
        Task ShowDialog(VmWithSizeBase vm);
        Task<MessageBoxResult> ShowMessageBox(MessageBoxType mbType, string message);
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

            var lf = (IClassicDesktopStyleApplicationLifetime?)Application.Current?.ApplicationLifetime;
            Debug.Assert(lf is not null);
            Debug.Assert(lf.MainWindow is not null);

            return win.ShowDialog(lf.MainWindow);
        }

        public async Task<MessageBoxResult> ShowMessageBox(MessageBoxType mbType, string message)
        {
            MessageBoxViewModel vm = new(mbType, message);
            Window win = new()
            {
                Content = vm,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                Title = "Warning!",
            };
            vm.CLoseEvent += (s, e) => win.Close();

            var lf = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            await win.ShowDialog(lf.MainWindow);

            return vm.Result;
        }
    }
}
