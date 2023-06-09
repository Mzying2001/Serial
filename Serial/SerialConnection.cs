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
        /// <summary>
        /// 串口
        /// </summary>
        private readonly System.IO.Ports.SerialPort serialPort;

        /// <summary>
        /// 用于中断循环发送
        /// </summary>
        private CancellationTokenSource loopSendingCTS;

        /// <summary>
        /// 接收数据发生错误时触发该事件
        /// </summary>
        public event ErrorEventHandler OnReceivedDataError;

        /// <summary>
        /// 发送数据发送错误时触发该事件
        /// </summary>
        public event ErrorEventHandler OnSendingDataError;

        /// <summary>
        /// 打开或关闭串口失败是触发该事件
        /// </summary>
        public event ErrorEventHandler OnSwitchingIsOpenError;

        public SerialConnection(EncodingInfo encodingInfo)
        {
            EncodingInfo = encodingInfo;

            serialPort = new System.IO.Ports.SerialPort() { ReadTimeout = 4000, WriteTimeout = 4000 };
            serialPort.DataReceived += SerialPort_DataReceived;

            DataList = new ObservableCollection<SerialData>();

            OpenCmd = new DelegateCommand(Open);
            CloseCmd = new DelegateCommand(Close);
            SendStringCmd = new AsyncDelegateCommand(SendString);
            ClearDataCmd = new DelegateCommand(ClearData);
            SendFileCmd = new AsyncDelegateCommand(SendFile);
            RemoveDataItemCmd = new DelegateCommand<SerialData>(RemoveDataItem);
            StartLoopSendCmd = new AsyncDelegateCommand(StartLoopSend);
            StopLoopSendCmd = new AsyncDelegateCommand(StopLoopSend);
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

                ExecuteOnUIThread(() =>
                {
                    AddSerialDataToList(new SerialData(data, EncodingInfo.GetEncoding()) { Hex = showHexByDefault });
                    UpdateReceivedCounter(data);
                });
            }
            catch (Exception ex)
            {
                ProcessError(ex, OnReceivedDataError);
            }
        }

        /// <summary>
        /// 调用错误处理函数，若无处理函数则抛出错误
        /// </summary>
        private void ProcessError(Exception e, ErrorEventHandler handler)
        {
            if (handler == null)
            {
                throw e;
            }
            else
            {
                handler(this, new ErrorEventArgs(e));
            }
        }

        /// <summary>
        /// 添加串口数据到列表
        /// </summary>
        /// <param name="serialData"></param>
        private void AddSerialDataToList(SerialData serialData)
        {
            DataList.Add(serialData);
            SelectedData = serialData;
            if (DataList.Count > 5000)
                DataList.RemoveAt(0);
        }

        /// <summary>
        /// 更新 <see cref="ReceivedDataCount"/> 和 <see cref="ReceivedDataByteCount"/>
        /// </summary>
        /// <param name="data">新接收的数据</param>
        private void UpdateReceivedCounter(byte[] data)
        {
            ReceivedDataCount++;
            ReceivedDataByteCount += (ulong)data.Length;
        }

        /// <summary>
        /// 将 <see cref="StrToSend"/> 转换为 <see cref="byte[]"/>
        /// </summary>
        /// <returns></returns>
        private byte[] ConvertStrToSend()
        {
            return ParseStrToHex ? HexStr.Parse(StrToSend) : EncodingInfo.GetEncoding().GetBytes(StrToSend);
        }

        #region properties
        /// <summary>
        /// 数据列表
        /// </summary>
        public ObservableCollection<SerialData> DataList { get; }

        private bool loopSending;
        /// <summary>
        /// 是否正在循环发送
        /// </summary>
        public bool LoopSending
        {
            get => loopSending;
            private set => UpdateValue(ref loopSending, value);
        }

        private bool loopSendingMode;
        /// <summary>
        /// 循环发送模式
        /// </summary>
        public bool LoopSendingMode
        {
            get => loopSendingMode;
            set => UpdateValue(ref loopSendingMode, value);
        }

        private int loopSendingInterval = 1000;
        /// <summary>
        /// 循环发送的间隔时间
        /// </summary>
        public int LoopSendingInterval
        {
            get => loopSendingInterval;
            set => UpdateValue(ref loopSendingInterval, value);
        }

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

        private ulong receivedDataByteCount = 0;
        /// <summary>
        /// 已接收到数据的字节数
        /// </summary>
        public ulong ReceivedDataByteCount
        {
            get => receivedDataByteCount;
            private set => UpdateValue(ref receivedDataByteCount, value);
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

        private SerialPortInfo selectedSerialPortInfo;
        /// <summary>
        /// 所选串口的信息
        /// </summary>
        public SerialPortInfo SelectedSerialPortInfo
        {
            get => selectedSerialPortInfo;
            set => UpdateValue(ref selectedSerialPortInfo, value);
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
                        PortName = SelectedSerialPortInfo?.PortName;
                        serialPort.Open();
                    }
                    else
                    {
                        serialPort.Close();
                    }
                }
                catch (Exception e)
                {
                    ProcessError(e, OnSwitchingIsOpenError);
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
            if (LoopSending)
                StopLoopSend().Wait();
            IsOpen = false;
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        public DelegateCommand SendStringCmd { get; }
        private async Task SendString()
        {
            if (string.IsNullOrEmpty(StrToSend))
                return;

            SendStringCmd.CanExecute = false;

            try
            {
                byte[] data = ConvertStrToSend();
                await serialPort.BaseStream.WriteAsync(data, 0, data.Length);
                AddSerialDataToList(SerialData.CreateSentData(StrToSend));
            }
            catch (Exception e)
            {
                ProcessError(e, OnSendingDataError);
            }

            SendStringCmd.CanExecute = true;
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public DelegateCommand ClearDataCmd { get; }
        private void ClearData()
        {
            DataList.Clear();
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        public DelegateCommand SendFileCmd { get; }
        private async Task SendFile()
        {
            if (string.IsNullOrEmpty(FileToSend))
                return;

            SendFileCmd.CanExecute = false;

            try
            {
                byte[] data = await Task.Run(() => File.ReadAllBytes(FileToSend));
                await serialPort.BaseStream.WriteAsync(data, 0, data.Length);
                AddSerialDataToList(SerialData.CreateSentData($"[文件] {FileToSend}"));
            }
            catch (Exception e)
            {
                ProcessError(e, OnSendingDataError);
            }

            SendFileCmd.CanExecute = true;
        }

        /// <summary>
        /// 删除某条数据
        /// </summary>
        public DelegateCommand RemoveDataItemCmd { get; }
        private void RemoveDataItem(SerialData serialData)
        {
            DataList.Remove(serialData);
        }

        /// <summary>
        /// 开始循环发送
        /// </summary>
        public DelegateCommand StartLoopSendCmd { get; }
        private async Task StartLoopSend()
        {
            if (string.IsNullOrEmpty(StrToSend))
                return;

            byte[] data;
            try
            {
                data = ConvertStrToSend();
            }
            catch (Exception e)
            {
                ProcessError(e, OnSendingDataError);
                return;
            }

            LoopSending = true;
            StartLoopSendCmd.CanExecute = false;
            SendStringCmd.CanExecute = false;

            loopSendingCTS = new CancellationTokenSource();
            CancellationToken cancellationToken = loopSendingCTS.Token;
            LoopSendingInterval = Math.Max(1, Math.Min(LoopSendingInterval, (int)1e5));

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await serialPort.BaseStream.WriteAsync(data, 0, data.Length);
                    AddSerialDataToList(SerialData.CreateSentData(StrToSend));
                    await Task.Delay(LoopSendingInterval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    ProcessError(e, OnSendingDataError);
                    break;
                }
            }

            LoopSending = false;
            StartLoopSendCmd.CanExecute = true;
            SendStringCmd.CanExecute = true;
        }

        /// <summary>
        /// 停止循环发送
        /// </summary>
        public DelegateCommand StopLoopSendCmd { get; }
        private async Task StopLoopSend()
        {
            if (loopSendingCTS == null || loopSendingCTS.IsCancellationRequested)
                return;
            StopLoopSendCmd.CanExecute = false;
            loopSendingCTS.Cancel();
            while (LoopSending)
                await Task.Delay(10);
            StopLoopSendCmd.CanExecute = true;
        }
        #endregion
    }
}
