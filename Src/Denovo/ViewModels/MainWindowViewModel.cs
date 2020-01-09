// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Reflection;

namespace Denovo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Message => "This application is still under construction.";

        public string Risk
        {
            get
            {
                Version ver = Assembly.GetExecutingAssembly().GetName().Version;

                return $"This is Denovo version {ver.ToString(4)}{Environment.NewLine}" +
                    $"{((ver.Major == 0 && ver.Minor == 0) ? "That is version zero (incomplete preview release!)" : "")}" +
                    $"{((ver.Major == 0 && ver.Minor == 1) ? "That is a beta release" : "")}" +
                    $"{Environment.NewLine}" +
                    $"If you use this application with real funds the risk of losing them is " +
                    $"{(ver.Major == 0 && ver.Minor == 0 ? "100%" : ver.Minor < 5 ? "80%" : "50%")}";
            }
        }
    }
}
