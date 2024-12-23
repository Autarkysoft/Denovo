// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Windows.Input;

namespace Denovo.MVVM
{
    public class BindableCommand : ICommand
    {
        public BindableCommand(Action executeMethod) : this(executeMethod, null)
        {
        }

        public BindableCommand(Action executeMethod, Func<bool>? canExecuteMethod)
        {
            ExecuteMethod = executeMethod;
            CanExecuteMethod = canExecuteMethod;
        }


        private readonly Action ExecuteMethod;
        private readonly Func<bool>? CanExecuteMethod;

        public event EventHandler? CanExecuteChanged;


        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object? parameter) => CanExecuteMethod is null || CanExecuteMethod();

        public void Execute(object? parameter) => ExecuteMethod?.Invoke();
    }


    public class BindableCommand<T> : ICommand
    {
        public BindableCommand(Action<T> executeMethod) : this(executeMethod, null)
        {
        }

        public BindableCommand(Action<T> executeMethod, Func<bool>? canExecuteMethod)
        {
            ExecuteMethod = executeMethod;
            CanExecuteMethod = canExecuteMethod;
        }


        private readonly Action<T> ExecuteMethod;
        private readonly Func<bool>? CanExecuteMethod;

        public event EventHandler? CanExecuteChanged;


        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object? parameter) => CanExecuteMethod is null || CanExecuteMethod();

        public void Execute(object? parameter) => ExecuteMethod?.Invoke((T)parameter);
    }
}
