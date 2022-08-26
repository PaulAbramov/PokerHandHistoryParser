using System;
using System.Windows.Input;

namespace PokerHandHistory
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> action;

        public RelayCommand(Action<object> _action)
        {
            action = _action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object _parameter)
        {
            action(_parameter);
        }

        public event EventHandler CanExecuteChanged = (sender, e) => { };
    }
}
