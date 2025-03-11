﻿// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Denovo.MVVM;
using System;

namespace Denovo.ViewModels
{
    /// <summary>
    /// Base (abstract) class for ViewModels. Inherits from <see cref="InpcBase"/>.
    /// </summary>
    public abstract class ViewModelBase : InpcBase
    {
        public event EventHandler? CLoseEvent;

        public void RaiseCloseEvent() => CLoseEvent?.Invoke(this, EventArgs.Empty);
    }
}
