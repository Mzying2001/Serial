using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Serial.MVVM
{
    public class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
