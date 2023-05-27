using Serial.MVVM;
using System;
using System.Text;

namespace Serial
{
    public class SerialData : NotificationObject
    {
        private byte[] data;
        private string str;
        private string hexStr;

        public DelegateCommand ChangeEncodingCmd { get; }

        public Encoding Encoding { get; private set; }
        public DateTime Time { get; private set; }
        public bool IsSentData { get; private set; }
        public byte[] Bytes { get => data; }

        private bool hex;
        public bool Hex
        {
            get => hex;
            set
            {
                if (!IsSentData)
                {
                    UpdateValue(ref hex, value);
                    RaisePropertyChanged(nameof(Data));
                }
            }
        }

        public string Data
        {
            get
            {
                if (IsSentData)
                {
                    return str;
                }
                else
                {
                    return Hex ? AsHex() : AsStr();
                }
            }
        }

        private SerialData()
        {
            Time = DateTime.Now;
            ChangeEncodingCmd = new DelegateCommand<EncodingInfo>(ChangeEncoding);
        }

        public SerialData(byte[] data, Encoding encoding) : this()
        {
            this.data = data;
            Encoding = encoding;
        }

        public static SerialData CreateSentData(string description)
        {
            return new SerialData() { IsSentData = true, str = description };
        }

        private string AsStr()
        {
            if (str == null)
                str = Encoding.GetString(data);
            return str;
        }

        private string AsHex()
        {
            if (hexStr == null)
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
                hexStr = sb.ToString();
            }
            return hexStr;
        }

        private void ChangeEncoding(EncodingInfo encodingInfo)
        {
            if (IsSentData)
                return;
            Encoding = encodingInfo.GetEncoding();
            str = null;
            if (!Hex)
                RaisePropertyChanged(nameof(Data));
        }
    }
}
