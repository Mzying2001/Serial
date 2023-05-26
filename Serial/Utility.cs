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
