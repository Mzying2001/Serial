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
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%)'"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string deviceName = obj["Name"].ToString();
                    string portName = ExtractPortName(deviceName);
                    string description = obj["Description"].ToString();

                    list.Add(new SerialPortInfo()
                    {
                        PortName = portName,
                        Description = description
                    });
                }
            }
            return list;
        }

        private static string ExtractPortName(string deviceName)
        {
            // Assuming the format is "<SomeText> (<PortName>)"
            int startIndex = deviceName.LastIndexOf('(');
            int endIndex = deviceName.LastIndexOf(')');

            if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
            {
                int portNameLength = endIndex - startIndex - 1;
                string portName = deviceName.Substring(startIndex + 1, portNameLength);
                return portName;
            }

            // If the expected format is not found, return the entire device name
            return deviceName;
        }
    }
}
