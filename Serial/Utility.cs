using System;
using System.Collections.Generic;
using System.Windows;

namespace Serial
{
    public static class Utility
    {
        public static void Ask(string msg, Action<bool> callback)
        {
            MessageBoxResult result = MessageBox.Show(msg, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            callback?.Invoke(result == MessageBoxResult.Yes);
        }

        public static void AskOpenFile(string filter, Action<bool, string> callback)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = filter };
            callback?.Invoke(ofd.ShowDialog() ?? false, ofd.FileName);
        }

        public static void AskSaveFile(string filter, Action<bool, string> callback)
        {
            var sfd = new Microsoft.Win32.SaveFileDialog { Filter = filter };
            callback?.Invoke(sfd.ShowDialog() ?? false, sfd.FileName);
        }

        public static void ShowErrorMsg(string msg)
        {
            MessageBox.Show(msg, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static List<T> GetEnumList<T>()
        {
            List<T> list = new List<T>();
            foreach (var item in Enum.GetValues(typeof(T)))
                list.Add((T)item);
            return list;
        }
    }
}
