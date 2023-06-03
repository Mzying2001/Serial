using Serial.MVVM;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serial
{
    public class SerialConnection : NotificationObject
    {
        private readonly System.IO.Ports.SerialPort serialPort;

        public SerialConnection(EncodingInfo encodingInfo)
        {
            EncodingInfo = encodingInfo;

            serialPort = new System.IO.Ports.SerialPort()
            {
                ReadTimeout = 4000,
                WriteTimeout = 4000
            };
            serialPort.DataReceived += SerialPort_DataReceived;

            DataList = new ObservableCollection<SerialData>();

            OpenCmd = new DelegateCommand(Open);
            CloseCmd = new DelegateCommand(Close);
            SendStringCmd = new DelegateCommand(SendString);
            ClearDataCmd = new DelegateCommand(ClearData);
            SelectFileCmd = new DelegateCommand(SelectFile);
            SendFileCmd = new DelegateCommand(SendFile);
            RemoveDataItemCmd = new DelegateCommand<SerialData>(RemoveDataItem);
        }

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (PauseReceiving)
            {
                serialPort.DiscardInBuffer();
                return;
            }
            try
            {
                Thread.Sleep(10); // 防止获取数据不完整

                if (!serialPort.IsOpen)
                    return;

                byte[] data = new byte[serialPort.BytesToRead];
                serialPort.Read(data, 0, data.Length);

                App.Current?.Dispatcher?.Invoke(() =>
                {
                    AddSerialDataToList(new SerialData(data, EncodingInfo.GetEncoding()) { Hex = showHexByDefault });
                    ReceivedDataCount++;
                });
            }
            catch (Exception ex)
            {
                Utility.ShowErrorMsg(ex.ToString());
            }
        }

        private void AddSerialDataToList(SerialData serialData)
        {
            DataList.Add(serialData);
            SelectedData = serialData;
            if (DataList.Count > 5000)
                DataList.RemoveAt(0);
        }

        #region properties
        /// <summary>
        /// 数据列表
        /// </summary>
        public ObservableCollection<SerialData> DataList { get; }

        private EncodingInfo encodingInfo;
        /// <summary>
        /// 字符集信息
        /// </summary>
        public EncodingInfo EncodingInfo
        {
            get => encodingInfo;
            set => UpdateValue(ref encodingInfo, value);
        }

        private SerialData selectedData;
        /// <summary>
        /// 选中的数据项
        /// </summary>
        public SerialData SelectedData
        {
            get => selectedData;
            set => UpdateValue(ref selectedData, value);
        }

        private ulong receivedDataCount = 0;
        /// <summary>
        /// 已接收到的数据数
        /// </summary>
        public ulong ReceivedDataCount
        {
            get => receivedDataCount;
            private set => UpdateValue(ref receivedDataCount, value);
        }

        private string strToSend = "";
        /// <summary>
        /// 要发送的字符串
        /// </summary>
        public string StrToSend
        {
            get => strToSend;
            set => UpdateValue(ref strToSend, value);
        }

        private bool parseStrToHex;
        /// <summary>
        /// 解析字符串为HEX
        /// </summary>
        public bool ParseStrToHex
        {
            get => parseStrToHex;
            set => UpdateValue(ref parseStrToHex, value);
        }

        private string fileToSend = "";
        /// <summary>
        /// 要发送的文件
        /// </summary>
        public string FileToSend
        {
            get => fileToSend;
            set => UpdateValue(ref fileToSend, value);
        }

        private bool pauseReceiving = false;
        /// <summary>
        /// 暂停接收数据
        /// </summary>
        public bool PauseReceiving
        {
            get => pauseReceiving;
            set => UpdateValue(ref pauseReceiving, value);
        }

        private bool showHexByDefault = false;
        /// <summary>
        /// 默认显示十六进制
        /// </summary>
        public bool ShowHexByDefault
        {
            get => showHexByDefault;
            set => UpdateValue(ref showHexByDefault, value);
        }

        /// <summary>
        /// 串口名
        /// </summary>
        public string PortName
        {
            get => serialPort.PortName;
            set
            {
                serialPort.PortName = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 波特率
        /// </summary>
        public int BraudRate
        {
            get => serialPort.BaudRate;
            set
            {
                serialPort.BaudRate = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 奇偶校验检查协议
        /// </summary>
        public System.IO.Ports.Parity Parity
        {
            get => serialPort.Parity;
            set
            {
                serialPort.Parity = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 字节标准数据位长度
        /// </summary>
        public int DataBits
        {
            get => serialPort.DataBits;
            set
            {
                serialPort.DataBits = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 每个字节标准停止位数
        /// </summary>
        public System.IO.Ports.StopBits StopBits
        {
            get => serialPort.StopBits;
            set
            {
                serialPort.StopBits = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 串口是否打开
        /// </summary>
        public bool IsOpen
        {
            get => serialPort.IsOpen;
            set
            {
                try
                {
                    if (value)
                    {
                        serialPort.Open();
                    }
                    else
                    {
                        serialPort.Close();
                    }
                }
                catch (Exception e)
                {
                    Utility.ShowErrorMsg(e.Message);
                }
                RaisePropertyChanged();
            }
        }
        #endregion

        #region commands
        /// <summary>
        /// 打开串口
        /// </summary>
        public DelegateCommand OpenCmd { get; }
        private void Open()
        {
            IsOpen = true;
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public DelegateCommand CloseCmd { get; }
        private void Close()
        {
            IsOpen = false;
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        public DelegateCommand SendStringCmd { get; }
        public async void SendString()
        {
            if (string.IsNullOrEmpty(StrToSend))
                return;

            SerialData serialData = null;
            SendStringCmd.CanExecute = false;

            await Task.Run(() =>
            {
                try
                {
                    byte[] data = ParseStrToHex ? HexStr.Parse(StrToSend) : EncodingInfo.GetEncoding().GetBytes(StrToSend);
                    serialPort.Write(data, 0, data.Length);
                    serialData = SerialData.CreateSentData(StrToSend);
                }
                catch (Exception e)
                {
                    App.Current.Dispatcher.Invoke(() => Utility.ShowErrorMsg(e.Message));
                }
            });

            if (serialData != null)
                AddSerialDataToList(serialData);
            SendStringCmd.CanExecute = true;
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public DelegateCommand ClearDataCmd { get; }
        public void ClearData()
        {
            DataList.Clear();
        }

        /// <summary>
        /// 选择要发送的文件
        /// </summary>
        public DelegateCommand SelectFileCmd { get; }
        public void SelectFile()
        {
            Utility.AskOpenFile("", (ok, fileName) =>
            {
                if (ok)
                    FileToSend = fileName;
            });
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        public DelegateCommand SendFileCmd { get; }
        public async void SendFile()
        {
            if (string.IsNullOrEmpty(FileToSend))
                return;

            SerialData serialData = null;
            SendFileCmd.CanExecute = false;
            SelectFileCmd.CanExecute = false;

            await Task.Run(() =>
            {
                try
                {
                    byte[] data = File.ReadAllBytes(FileToSend);
                    serialPort.Write(data, 0, data.Length);
                    serialData = SerialData.CreateSentData($"[文件] {FileToSend}");
                }
                catch (Exception e)
                {
                    App.Current.Dispatcher.Invoke(() => Utility.ShowErrorMsg(e.Message));
                }
            });

            if (serialData != null)
                AddSerialDataToList(serialData);
            SendFileCmd.CanExecute = true;
            SelectFileCmd.CanExecute = true;
        }

        /// <summary>
        /// 删除某条数据
        /// </summary>
        public DelegateCommand RemoveDataItemCmd { get; }
        private void RemoveDataItem(SerialData serialData)
        {
            DataList.Remove(serialData);
        }
        #endregion
    }
}