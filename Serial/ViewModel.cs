using Serial.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Serial
{
    public class ViewModel : NotificationObject
    {
        public static ViewModel Instance { get; } = new ViewModel();


        public DelegateCommand AddSerialConnectionCmd { get; }
        public DelegateCommand RemoveSerialConnectionCmd { get; }
        public DelegateCommand ExportSerialDataCmd { get; }
        public DelegateCommand UpdateAvaliablePortsCmd { get; }


        public List<EncodingInfo> EncodingInfoList { get; }
        public List<string> AvaliablePorts { get; private set; }
        public List<int> BraudRateList { get; }
        public List<int> DataBitsList { get; }
        public List<Parity> ParityList { get; }
        public List<StopBits> StopBitsList { get; }
        public ObservableCollection<SerialConnection> SerialConnections { get; }


        private SerialConnection selectedSerialConnection;
        public SerialConnection SelectedSerialConnection
        {
            get => selectedSerialConnection;
            set => UpdateValue(ref selectedSerialConnection, value);
        }


        private void SerialConnectionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RemoveSerialConnectionCmd.CanExecute = SerialConnections.Count > 0 && SelectedSerialConnection != null;
        }

        private void AddSerialConnection()
        {
            var sc = new SerialConnection(EncodingInfoList.Where(i => i.GetEncoding() == Encoding.UTF8).FirstOrDefault()); //默认使用utf-8
            SelectedSerialConnection = sc;
            SerialConnections.Add(sc);
        }

        private void RemoveSerialConnection(SerialConnection sc)
        {
            if (sc == null)
                return;

            bool flag = true;
            if (sc.IsOpen)
            {
                Utility.Ask("该串口已打开，是否要关闭并移除？", result =>
                {
                    if (result)
                        sc.IsOpen = false;
                    else
                        flag = false;
                });
            }

            if (flag)
            {
                int index = SerialConnections.IndexOf(sc);
                SerialConnections.Remove(sc);

                if (index < SerialConnections.Count)
                    SelectedSerialConnection = SerialConnections[index];
                else if (SerialConnections.Count > 0)
                    SelectedSerialConnection = SerialConnections[Math.Max(0, index - 1)];
            }
        }

        private void ExportSerialData(SerialData serialData)
        {
            var sfd = new Microsoft.Win32.SaveFileDialog() { Filter = "TXT|*.txt|BIN|*.bin" };
            if (sfd.ShowDialog() == true)
                File.WriteAllBytes(sfd.FileName, serialData.Bytes);
        }

        private void UpdateAvaliablePorts()
        {
            AvaliablePorts = SerialPort.GetPortNames().ToList();
            RaisePropertyChanged(nameof(AvaliablePorts));
        }


        private ViewModel()
        {
            SerialConnections = new ObservableCollection<SerialConnection>();
            SerialConnections.CollectionChanged += SerialConnectionsCollectionChanged;

            EncodingInfoList = Encoding.GetEncodings().OrderBy(i => i.Name).ToList();
            AvaliablePorts = SerialPort.GetPortNames().ToList();
            BraudRateList = new List<int> { 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 43000, 56000, 57600, 115200 };
            DataBitsList = new List<int> { 5, 6, 7, 8 };
            ParityList = Utility.GetEnumList<Parity>();
            StopBitsList = Utility.GetEnumList<StopBits>();

            AddSerialConnectionCmd = new DelegateCommand(AddSerialConnection);
            RemoveSerialConnectionCmd = new DelegateCommand<SerialConnection>(RemoveSerialConnection, false);
            ExportSerialDataCmd = new DelegateCommand<SerialData>(ExportSerialData);
            UpdateAvaliablePortsCmd = new DelegateCommand(UpdateAvaliablePorts);

            AddSerialConnection();
        }
    }
}
