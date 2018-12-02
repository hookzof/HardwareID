using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Management;
using System.Collections;

namespace HID
{
    class Program
    {
        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi)]
        public static extern int GetAdaptersInfo(IntPtr intptr_0, ref long long_0);

        [StructLayout(LayoutKind.Sequential)]
        internal struct IpAddrString
        {
            internal IntPtr Next;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x10)]
            internal string IpAddress;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x10)]
            internal string IpMask;
            internal uint Context;
        }
        internal struct IpAdapterInfo
        {
            internal const int MAX_ADAPTER_DESCRIPTION_LENGTH = 0x80;
            internal const int MAX_ADAPTER_NAME_LENGTH = 0x100;
            internal const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
            internal IntPtr Next;
            internal uint comboIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string adapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x84)]
            internal string description;
            internal uint addressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal byte[] address;
            internal uint index;
            internal OldInterfaceType type;
            internal bool dhcpEnabled;
            internal IntPtr currentIpAddress;
            internal IpAddrString ipAddressList;
            internal IpAddrString gatewayList;
            internal IpAddrString dhcpServer;
            [MarshalAs(UnmanagedType.Bool)]
            internal bool haveWins;
            internal IpAddrString primaryWinsServer;
            internal IpAddrString secondaryWinsServer;
            internal uint leaseObtained;
            internal uint leaseExpires;
        }

        internal enum OldInterfaceType
        {
            Ethernet = 6,
            Fddi = 15,
            Loopback = 0x18,
            Ppp = 0x17,
            Slip = 0x1c,
            TokenRing = 9,
            Unknown = 0
        }

        internal static string GetMAC()
        {
            string text = string.Empty;
            try
            {
                long value = (long)Marshal.SizeOf(typeof(IpAdapterInfo));
                IntPtr intPtr = Marshal.AllocHGlobal(new IntPtr(value));
                int adaptersInfo = GetAdaptersInfo(intPtr, ref value);
                if (adaptersInfo == 111)
                {
                    intPtr = Marshal.ReAllocHGlobal(intPtr, new IntPtr(value));
                    adaptersInfo = GetAdaptersInfo(intPtr, ref value);
                }
                if (adaptersInfo == 0)
                {
                    IntPtr ptr = intPtr;
                    IpAdapterInfo IpAdapterInfo = (IpAdapterInfo)Marshal.PtrToStructure(ptr, typeof(IpAdapterInfo));
                    for (int i = 0; (long)i < (long)((ulong)IpAdapterInfo.addressLength); i++)
                    {
                        text = string.Concat(text, IpAdapterInfo.address[i].ToString("X2"));
                    }
                    Marshal.FreeHGlobal(intPtr);
                }
                else
                {
                    Marshal.FreeHGlobal(intPtr);
                }
            }
            catch
            {
            }
            if (text == string.Empty)
            {
                text = "";
            }
            return text;
        }

        private static string GetProcessorID()
        {
            string text = string.Empty;
            try
            {
                ManagementClass managementClass = new ManagementClass("Win32_Processor");
                ManagementObjectCollection instances = managementClass.GetInstances();
                using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ManagementObject managementObject = (ManagementObject)enumerator.Current;
                        if (text == string.Empty)
                        {
                            try
                            {
                                text = managementObject.Properties["ProcessorId"].Value.ToString();
                                if (text.Length != 0)
                                {
                                    break;
                                }
                                text = string.Empty;
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return text;
        }

        private static string GetMACAddress()
        {
            string text = String.Empty;
            try
            {
                text = GetMAC();
                if (text.Length == 0)
                {
                    text = string.Empty;
                    ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
                    ManagementObjectCollection instances = managementClass.GetInstances();
                    using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            ManagementObject managementObject = (ManagementObject)enumerator.Current;
                            if (!(text == string.Empty))
                            {
                                break;
                            }
                            try
                            {
                                if (managementObject["IPEnabled"] != null && (bool)managementObject["IPEnabled"] && managementObject["MacAddress"] != null && managementObject["MacAddress"].ToString().Length > 0)
                                {
                                    text = managementObject["MacAddress"].ToString().ToUpper();
                                    text = text.Replace(":", "");
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return text;
        }

        private static string GetMotherBoardID()
        {
            try
            {
                return string.Concat(new string[]
                {
                    GetProduct(),
                    "-",
                    GetManufacturer(),
                    "-",
                    GetSerialNumber()
                });
            }
            catch
            {
                return String.Empty;
            }
        }

        private static string GetManufacturer()
        {
            string result;
            try
            {
                string text = String.Empty;
                ManagementClass managementClass = new ManagementClass("Win32_BaseBoard");
                ManagementObjectCollection instances = managementClass.GetInstances();
                using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ManagementObject managementObject = (ManagementObject)enumerator.Current;
                        try
                        {
                            if (text == string.Empty && managementObject.Properties["Manufacturer"] != null && managementObject.Properties["Manufacturer"].Value != null)
                            {
                                text = managementObject.Properties["Manufacturer"].Value.ToString();
                                if (text.Length != 0)
                                {
                                    break;
                                }
                                text = String.Empty;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                result = text;
            }
            catch
            {
                result = String.Empty;
            }
            return result;
        }

        private static string GetSerialNumber()
        {
            string result;
            try
            {
                string text = String.Empty;
                ManagementClass managementClass = new ManagementClass("Win32_BaseBoard");
                ManagementObjectCollection instances = managementClass.GetInstances();
                using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ManagementObject managementObject = (ManagementObject)enumerator.Current;
                        try
                        {
                            if (text == string.Empty && managementObject.Properties["SerialNumber"] != null && managementObject.Properties["SerialNumber"].Value != null)
                            {
                                text = managementObject.Properties["SerialNumber"].Value.ToString();
                                if (text.Length != 0)
                                {
                                    break;
                                }
                                text = String.Empty;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                result = text;
            }
            catch
            {
                result = String.Empty;
            }
            return result;
        }
        private static string GetProduct()
        {
            string result;
            try
            {
                string text = String.Empty;
                ManagementClass managementClass = new ManagementClass("Win32_BaseBoard");
                ManagementObjectCollection instances = managementClass.GetInstances();
                using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ManagementObject managementObject = (ManagementObject)enumerator.Current;
                        try
                        {
                            if (managementObject.Properties["Product"] != null && managementObject.Properties["Product"].Value != null && text == string.Empty)
                            {
                                text = managementObject.Properties["Product"].Value.ToString();
                                if (text.Length != 0)
                                {
                                    break;
                                }
                                text = String.Empty;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                result = text;
            }
            catch
            {
                result = String.Empty;
            }
            return result;
        }

        private static string GetDiskID()
        {
            try
            {
                ArrayList arrayList = new ArrayList();
                ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = managementObjectSearcher.Get().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ManagementObject managementObject = (ManagementObject)enumerator.Current;
                        if (managementObject["DeviceID"] != null && managementObject["InterfaceType"] != null && managementObject["InterfaceType"].ToString() != "USB" && managementObject["InterfaceType"].ToString() != "1394")
                        {
                            bool flag = true;
                            if (managementObject["MediaType"] != null && managementObject["MediaType"].ToString() == "Removable Media")
                            {
                                flag = false;
                            }
                            if (flag)
                            {
                                if (managementObject["SerialNumber"] != null)
                                {
                                    object obj = managementObject["SerialNumber"];
                                    if (obj != null && !(obj.ToString().Trim() == string.Empty) && obj.ToString()[0] != Convert.ToChar(31))
                                    {
                                        return obj.ToString().Trim();
                                    }
                                }
                                arrayList.Add(managementObject["DeviceID"].ToString());
                            }
                        }
                    }
                }
                managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
                ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
                foreach (string b in arrayList)
                {
                    using (ManagementObjectCollection.ManagementObjectEnumerator enumerator3 = managementObjectCollection.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            ManagementObject managementObject2 = (ManagementObject)enumerator3.Current;
                            if (managementObject2["Tag"] != null)
                            {
                                string a = managementObject2["Tag"].ToString();
                                if (a == b && managementObject2["SerialNumber"] != null)
                                {
                                    object obj2 = managementObject2["SerialNumber"];
                                    if (obj2 != null && !(obj2.ToString() == string.Empty) && obj2.ToString()[0] != Convert.ToChar(31))
                                    {
                                        return obj2.ToString().Trim().Replace(" ", "");
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return String.Empty;
            }
            return String.Empty;
        }

        internal static string GenerateHWID(bool bool_7, bool bool_8, bool bool_9, bool bool_10)
        {
            string text = "";
            RSACryptoServiceProvider.UseMachineKeyStore = true;
            MD5 md5 = MD5.Create();
            if (bool_7)
            {
                byte[] array = md5.ComputeHash(Encoding.Unicode.GetBytes(GetProcessorID()));
                text += array[3].ToString("X2");
                text = text + array[11].ToString("X2") + "-";
            }
            else
            {
                text = "85C1-";
            }
            if (bool_8)
            {
                byte[] array2 = md5.ComputeHash(Encoding.Unicode.GetBytes(GetMACAddress()));
                text += array2[3].ToString("X2");
                text = text + array2[11].ToString("X2") + "-";
            }
            else
            {
                byte[] array3 = md5.ComputeHash(Encoding.Unicode.GetBytes(text));
                text += array3[15].ToString("X2");
                text = text + array3[7].ToString("X2") + "-";
            }
            if (bool_9)
            {
                byte[] array4 = md5.ComputeHash(Encoding.Unicode.GetBytes(GetMotherBoardID()));
                text += array4[3].ToString("X2");
                text = text + array4[11].ToString("X2") + "-";
            }
            else
            {
                byte[] array5 = md5.ComputeHash(Encoding.Unicode.GetBytes(text));
                text += array5[2].ToString("X2");
                text = text + array5[14].ToString("X2") + "-";
            }
            if (bool_10)
            {
                byte[] array6 = md5.ComputeHash(Encoding.Unicode.GetBytes(GetDiskID()));
                text += array6[3].ToString("X2");
                text = text + array6[11].ToString("X2") + "-";
            }
            else
            {
                byte[] array7 = md5.ComputeHash(Encoding.Unicode.GetBytes(text));
                text += array7[1].ToString("X2");
                text = text + array7[9].ToString("X2") + "-";
            }
            byte[] array8 = md5.ComputeHash(Encoding.Unicode.GetBytes(text));
            text += array8[1].ToString("X2");
            return text + array8[9].ToString("X2");
        }
        static void Main(string[] args)
        {
            bool bool_ = true;
            bool bool_2 = false;
            bool bool_3 = true;
            bool bool_4 = false;
            string value = GenerateHWID(bool_, bool_2, bool_3, bool_4);

            Console.WriteLine(value);
            Console.ReadKey();
        }
    }
}
