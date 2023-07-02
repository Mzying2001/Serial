using System;

namespace Serial.MVVM
{
    public class DelegateCommand<T> : DelegateCommand
    {
        public new Action<T> Execute { get; set; }

        private void CallExecuteGeneric(object parameter)
        {
            Execute?.Invoke((T)parameter);
        }

        public DelegateCommand(Action<T> execute, bool canExecute = true) : base((Action<object>)null, canExecute)
        {
            Execute = execute;
            base.Execute = CallExecuteGeneric;
        }
    }
}
