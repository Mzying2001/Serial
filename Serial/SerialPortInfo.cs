using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;

namespace Serial
{
    public class SerialPortInfo : IComparable<SerialPortInfo>
    {
        public string PortName { get; set; }
        public string Description { get; set; }

        public SerialPortInfo()
        {
        }

        public override string ToString()
        {
            return $"{PortName} {Description}";
        }

        public int CompareTo(SerialPortInfo other)
        {
            if (PortName.Length == other.PortName.Length)
                return PortName.CompareTo(other.PortName);
            else
                return PortName.Length - other.PortName.Length;
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
            list.Sort();
            return list;
        }

        // Regular expression for PortName
        private static readonly Regex portNameRegex =
            new Regex(@"COM\d+", RegexOptions.IgnoreCase);

        // Helper method to extract port name from device name
        private static string ExtractPortName(string deviceName)
        {
            // Assuming the format is "<SomeText> (<PortName>)"
            int startIndex = deviceName.LastIndexOf('(');
            int endIndex = deviceName.LastIndexOf(')');

            if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
            {
                int portNameLength = endIndex - startIndex - 1;
                string portName = deviceName.Substring(startIndex + 1, portNameLength);

                foreach (Match match in portNameRegex.Matches(portName))
                {
                    portName = match.Value;
                    return portName;
                }
            }

            // If the expected format is not found, return the entire device name
            return deviceName;
        }
    }
}
