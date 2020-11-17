using System;
using System.Windows.Input;

namespace EmuController.DevManager.Commands
{
    public class DeviceCommand : ICommand
    {
        public DeviceCommand(Action<object> execute) : this(execute, null) { }
        public DeviceCommand(Action<object> execute, Predicate<object> canExecute)
        {
            CanExecuteDelegate = canExecute;
            ExecuteDelegate = execute;
        }

        public Predicate<object> CanExecuteDelegate { get; set; }

        public Action<object> ExecuteDelegate { get; set; }

        public bool CanExecute(object parameter)
        {
            return CanExecuteDelegate == null || CanExecuteDelegate(parameter);
        }

        public void SetExecuteState(bool canExecute)
        {
            CanExecuteDelegate = x => canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object parameter)
        {
            ExecuteDelegate?.Invoke(parameter);
        }
    }
}
