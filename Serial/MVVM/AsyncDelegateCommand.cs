using System;
using System.Threading.Tasks;

namespace Serial.MVVM
{
    public class AsyncDelegateCommand : DelegateCommand
    {
        public new Func<object, Task> Execute { get; set; }

        private async void CallAsyncExecute(object parameter)
        {
            await Execute(parameter);
        }

        public AsyncDelegateCommand(Func<object, Task> execute) : this(execute, true)
        {
        }

        public AsyncDelegateCommand(Func<object, Task> execute, bool canExecute) : base((Action<object>)null, canExecute)
        {
            Execute = execute;
            base.Execute = CallAsyncExecute;
        }

        public AsyncDelegateCommand(Func<Task> execute) : this(execute, true)
        {
        }

        public AsyncDelegateCommand(Func<Task> execute, bool canExecute) : base((Action<object>)null, canExecute)
        {
            Execute = async _ => await execute();
            base.Execute = CallAsyncExecute;
        }
    }
}
