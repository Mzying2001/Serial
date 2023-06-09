using System;
using System.Windows.Input;

namespace Serial.MVVM
{
    public class DelegateCommand : NotificationObject, ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Action<object> Execute { get; set; }

        private bool _canExecute;
        public bool CanExecute
        {
            get { return _canExecute; }
            set
            {
                UpdateValue(ref _canExecute, value);
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute;
        }

        void ICommand.Execute(object parameter)
        {
            Execute?.Invoke(parameter);
        }

        public DelegateCommand(Action<object> execute) : this(execute, true)
        {
        }

        public DelegateCommand(Action<object> execute, bool canExecute)
        {
            Execute = execute;
            CanExecute = canExecute;
        }

        public DelegateCommand(Action execute) : this(execute, true)
        {
        }

        public DelegateCommand(Action execute, bool canExecute)
        {
            Execute = _ => execute();
            CanExecute = canExecute;
        }
    }
}
