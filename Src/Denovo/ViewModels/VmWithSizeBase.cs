// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Denovo.ViewModels
{
    /// <summary>
    /// Base (abstract) class for view models that have to be shown in a new window 
    /// and need to set the window's height and width.
    /// </summary>
    public abstract class VmWithSizeBase : ViewModelBase
    {
        public VmWithSizeBase(double height, double width)
        {
            Height = height;
            Width = width;
        }


        private double _height;
        public double Height
        {
            get => _height;
            set => SetField(ref _height, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => SetField(ref _width, value);
        }
    }
}
