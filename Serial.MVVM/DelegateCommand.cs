using System;
using System.Threading.Tasks;
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

        public DelegateCommand(Action<object> execute, bool canExecute = true)
        {
            Execute = execute;
            CanExecute = canExecute;
        }

        public DelegateCommand(Action execute, bool canExecute = true) : this(_ => execute?.Invoke(), canExecute)
        {
        }

        public static void ExecuteCommand(DelegateCommand command, object parameter = null)
        {
            if (command.CanExecute)
                command.Execute?.Invoke(parameter);
        }

        public static async Task ExecuteCommandAsync(AsyncDelegateCommand command, object parameter = null)
        {
            if (command.CanExecute && command.Execute != null)
                await command.Execute(parameter);
        }
    }
}
