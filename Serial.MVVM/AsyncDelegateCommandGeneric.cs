﻿using System;
using System.Threading.Tasks;

namespace Serial.MVVM
{
    public class AsyncDelegateCommand<T> : AsyncDelegateCommand
    {
        public new Func<T, Task> Execute { get; set; }

        private async Task CallAsyncExecuteGeneric(object parameter)
        {
            if (Execute != null)
                await Execute((T)parameter);
        }

        public AsyncDelegateCommand(Func<T, Task> execute, bool canExecute = true) : base((Func<object, Task>)null, canExecute)
        {
            Execute = execute;
            base.Execute = CallAsyncExecuteGeneric;
        }
    }
}
