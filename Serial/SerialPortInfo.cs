using System.Collections.Generic;
using System.Management;

namespace Serial
{
    public class SerialPortInfo
    {
        public string PortName { get; set; }
        public string Description { get; set; }

        public SerialPortInfo()
        {
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Description) ? PortName
                                                     : $"{PortName} ({Description})";
        }

        public static List<SerialPortInfo> GetSerialPortInfoList()
        {
            List<SerialPortInfo> list = new List<SerialPortInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        list.Add(new SerialPortInfo()
                        {
                            PortName = obj["DeviceID"].ToString(),
                            Description = obj["Description"].ToString()
                        });
                    }
                }
            }
            catch { }
            return list;
        }
    }
}
