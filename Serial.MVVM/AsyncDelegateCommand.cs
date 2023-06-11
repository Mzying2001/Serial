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

        public AsyncDelegateCommand(Func<object, Task> execute, bool canExecute = true) : base((Action<object>)null, canExecute)
        {
            Execute = execute;
            base.Execute = CallAsyncExecute;
        }

        public AsyncDelegateCommand(Func<Task> execute, bool canExecute = true)
            : this(async _ => await (execute != null ? execute() : Task.Run(() => { })), canExecute)
        {
        }
    }
}
