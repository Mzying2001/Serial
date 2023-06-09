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

        public DelegateCommand(Action<T> execute) : this(execute, true)
        {
        }

        public DelegateCommand(Action<T> execute, bool canExecute) : base((Action<object>)null, canExecute)
        {
            Execute = execute;
            base.Execute = CallExecuteGeneric;
        }
    }
}
