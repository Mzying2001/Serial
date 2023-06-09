using System.Collections.Generic;
using System.Text;

namespace Serial.Core
{
    public static class HexStr
    {
        public static string Format(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (i % 16 == 0)
                {
                    if (i != 0)
                        sb.AppendLine();
                }
                else
                    sb.Append(' ');
                sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static byte[] Parse(string hexStr)
        {
            List<byte> data = new List<byte>();
            foreach (string item in hexStr.Split(new char[] { ' ', '\t', '\r', '\n' }))
            {
                if (!string.IsNullOrEmpty(item))
                    data.Add(byte.Parse(item, System.Globalization.NumberStyles.HexNumber));
            }
            return data.ToArray();
        }
    }
}
