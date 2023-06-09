﻿using Serial.Core;
using Serial.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial.WPF
{
    public class MainViewModel : NotificationObject
    {
        public static MainViewModel Instance { get; } = new MainViewModel();


        public DelegateCommand AddSerialConnectionCmd { get; }
        public DelegateCommand RemoveSerialConnectionCmd { get; }
        public DelegateCommand ExportSerialDataCmd { get; }
        public DelegateCommand UpdateAvaliablePortsCmd { get; }
        public DelegateCommand SelectFileToSendCmd { get; }


        public List<EncodingInfo> EncodingInfoList { get; }
        public List<SerialPortInfo> AvaliablePorts { get; private set; }
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
            sc.SelectedSerialPortInfo = AvaliablePorts.FirstOrDefault();
            sc.OnReceivedDataError += SerialErrorHandler;
            sc.OnSendingDataError += SerialErrorHandler;
            sc.OnSwitchingIsOpenError += SerialErrorHandler;
            SelectedSerialConnection = sc;
            SerialConnections.Add(sc);
        }

        private void SerialErrorHandler(object sender, ErrorEventArgs e)
        {
            SimpleDialogs.ShowErrorMsg(e.GetException().Message);
        }

        private void RemoveSerialConnection(SerialConnection sc)
        {
            if (sc == null)
                return;

            bool flag = true;
            if (sc.IsOpen)
            {
                SimpleDialogs.Ask("该串口已打开，是否要关闭并移除？", result =>
                {
                    if (result)
                        DelegateCommand.ExecuteCommand(sc.CloseCmd);
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

        private void ExportSerialData(Core.SerialData serialData)
        {
            SimpleDialogs.AskSaveFile("文本文档|*.txt|所有文件|*.*", (ok, fileName) =>
            {
                if (ok)
                {
                    try
                    {
                        File.WriteAllBytes(fileName, serialData.Bytes);
                    }
                    catch (Exception e)
                    {
                        SimpleDialogs.ShowErrorMsg(e.Message);
                    }
                }
            });
        }

        private void SelectFileToSend(SerialConnection serialConnection)
        {
            SimpleDialogs.AskOpenFile("", (ok, fileName) =>
            {
                if (ok)
                    serialConnection.FileToSend = fileName;
            });
        }

        private async void UpdateAvaliablePorts()
        {
            UpdateAvaliablePortsCmd.CanExecute = false;
            AvaliablePorts = await Task.Run(() => SerialPortInfo.GetSerialPortInfoList());
            RaisePropertyChanged(nameof(AvaliablePorts));
            UpdateAvaliablePortsCmd.CanExecute = true;
        }


        private MainViewModel()
        {
            SerialConnections = new ObservableCollection<SerialConnection>();
            SerialConnections.CollectionChanged += SerialConnectionsCollectionChanged;

            AvaliablePorts = SerialPortInfo.GetSerialPortInfoList();
            EncodingInfoList = Encoding.GetEncodings().OrderBy(i => i.DisplayName).ToList();
            BraudRateList = new List<int> { 110   , 300   , 600   , 1200  , 2400  , 4800   , 9600   , 14400  ,
                                            19200 , 38400 , 57600 , 115200, 128000, 230400 , 256000 , 460800 ,
                                            500000, 512000, 600000, 750000, 921600, 1000000, 1500000, 2000000, };
            DataBitsList = new List<int> { 5, 6, 7, 8 };
            ParityList = Enum.GetValues<Parity>().ToList();
            StopBitsList = Enum.GetValues<StopBits>().ToList();

            AddSerialConnectionCmd = new DelegateCommand(AddSerialConnection);
            RemoveSerialConnectionCmd = new DelegateCommand<SerialConnection>(RemoveSerialConnection, false);
            ExportSerialDataCmd = new DelegateCommand<Core.SerialData>(ExportSerialData);
            UpdateAvaliablePortsCmd = new DelegateCommand(UpdateAvaliablePorts);
            SelectFileToSendCmd = new DelegateCommand<SerialConnection>(SelectFileToSend);

            AddSerialConnection();
        }
    }
}
