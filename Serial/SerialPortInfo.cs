using System.Management;

namespace Serial
{
    public class SerialPortInfo
    {
        public string PortName { get; set; }
        public string Description { get; set; }

        public SerialPortInfo(string portName)
        {
            PortName = portName;
            Description = GetSerialPortDescription(portName) ?? "";
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Description) ? PortName
                                                     : $"{PortName} ({Description})";
        }

        public string GetSerialPortDescription(string portName)
        {
            string description = null;
            try
            {
                // 查询串口设备
                using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_SerialPort WHERE DeviceID='{portName}'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        description = obj["Description"].ToString();
                        break; // 只获取第一个匹配项
                    }
                }
            }
            catch { }
            return description;
        }
    }
}
