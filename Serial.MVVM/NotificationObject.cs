using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Serial.MVVM
{
    public class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Dispatcher UIThreadDispatcher { get; set; }

        public NotificationObject()
        {
            SetUIThreadDispatcher(Dispatcher.CurrentDispatcher);
        }

        public void SetUIThreadDispatcher(Dispatcher dispatcher)
        {
            UIThreadDispatcher = dispatcher;
        }

        protected void ExecuteOnUIThread(Action action)
        {
            UIThreadDispatcher.Invoke(action);
        }

        protected async Task ExecuteOnUIThreadAsync(Action action)
        {
            await UIThreadDispatcher.InvokeAsync(action);
        }

        protected void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        protected void UpdateValue<T>(ref T field, T value, [CallerMemberName] string propName = null)
        {
            field = value;
            RaisePropertyChanged(propName);
        }
    }
}
