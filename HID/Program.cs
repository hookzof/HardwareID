using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Management;
using System.Collections;

namespace HID
{
    internal class Program
    {
        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi)]
        public static extern int GetAdaptersInfo(IntPtr intptr0, ref long long0);

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
            internal const int MaxAdapterDescriptionLength = 0x80;
            internal const int MaxAdapterNameLength = 0x100;
            internal const int MaxAdapterAddressLength = 8;
            internal IntPtr Next;
            internal uint ComboIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string AdapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x84)]
            internal string Description;
            internal uint AddressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal byte[] Address;
            internal uint Index;
            internal OldInterfaceType Type;
            internal bool DhcpEnabled;
            internal IntPtr CurrentIpAddress;
            internal IpAddrString IpAddressList;
            internal IpAddrString GatewayList;
            internal IpAddrString DhcpServer;
            [MarshalAs(UnmanagedType.Bool)]
            internal bool HaveWins;
            internal IpAddrString PrimaryWinsServer;
            internal IpAddrString SecondaryWinsServer;
            internal uint LeaseObtained;
            internal uint LeaseExpires;
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

        private static void Main()
        {
            var value = GenerateHwid(true, false, true, false);

            Console.WriteLine(value);
            Console.ReadKey();
        }

        internal static string GetMac()
        {
            var text = string.Empty;
            try
            {
                var value = (long)Marshal.SizeOf(typeof(IpAdapterInfo));
                var intPtr = Marshal.AllocHGlobal(new IntPtr(value));
                var adaptersInfo = GetAdaptersInfo(intPtr, ref value);
                if (adaptersInfo == 111)
                {
                    intPtr = Marshal.ReAllocHGlobal(intPtr, new IntPtr(value));
                    adaptersInfo = GetAdaptersInfo(intPtr, ref value);
                }
                if (adaptersInfo == 0)
                {
                    var ptr = intPtr;
                    var ipAdapterInfo = (IpAdapterInfo)Marshal.PtrToStructure(ptr, typeof(IpAdapterInfo));
                    for (var i = 0; i < (long)(ulong)ipAdapterInfo.AddressLength; i++)
                    {
                        text = string.Concat(text, ipAdapterInfo.Address[i].ToString("X2"));
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
                // ignored
            }

            if (text == string.Empty)
            {
                text = "";
            }
            return text;
        }

        private static string GetProcessorId()
        {
            var text = string.Empty;
            try
            {
                var managementClass = new ManagementClass("Win32_Processor");
                var instances = managementClass.GetInstances();
                using (var enumerator = instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var managementObject = (ManagementObject)enumerator.Current;
                        if (text != string.Empty) continue;
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
                            // ignored
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return text;
        }

        private static string GetMacAddress()
        {
            var text = string.Empty;
            try
            {
                text = GetMac();
                if (text.Length == 0)
                {
                    text = string.Empty;
                    var managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
                    var instances = managementClass.GetInstances();
                    using (var enumerator = instances.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var managementObject = (ManagementObject)enumerator.Current;
                            if (text != string.Empty)
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
                                // ignored
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return text;
        }

        private static string GetMotherBoardId()
        {
            try
            {
                return string.Concat(GetProduct(), "-", GetManufacturer(), "-", GetSerialNumber());
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetManufacturer()
        {
            string result;
            try
            {
                var text = string.Empty;
                var managementClass = new ManagementClass("Win32_BaseBoard");
                var instances = managementClass.GetInstances();
                using (var enumerator = instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var managementObject = (ManagementObject)enumerator.Current;
                        try
                        {
                            if (text == string.Empty && managementObject.Properties["Manufacturer"].Value != null)
                            {
                                text = managementObject.Properties["Manufacturer"].Value.ToString();
                                if (text.Length != 0)
                                {
                                    break;
                                }
                                text = string.Empty;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                result = text;
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }

        private static string GetSerialNumber()
        {
            string result;
            try
            {
                var text = string.Empty;
                var managementClass = new ManagementClass("Win32_BaseBoard");
                var instances = managementClass.GetInstances();
                using (var enumerator = instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var managementObject = (ManagementObject)enumerator.Current;
                        try
                        {
                            if (text == string.Empty && managementObject.Properties["SerialNumber"].Value != null)
                            {
                                text = managementObject.Properties["SerialNumber"].Value.ToString();
                                if (text.Length != 0)
                                {
                                    break;
                                }
                                text = string.Empty;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                result = text;
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }

        private static string GetProduct()
        {
            string result;
            try
            {
                var text = string.Empty;
                var managementClass = new ManagementClass("Win32_BaseBoard");
                var instances = managementClass.GetInstances();
                using (var enumerator = instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var managementObject = (ManagementObject)enumerator.Current;
                        try
                        {
                            if (managementObject.Properties["Product"].Value != null && text == string.Empty)
                            {
                                text = managementObject.Properties["Product"].Value.ToString();
                                if (text.Length != 0)
                                {
                                    break;
                                }
                                text = string.Empty;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                result = text;
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }

        private static string GetDiskId()
        {
            try
            {
                var arrayList = new ArrayList();
                var managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                using (var enumerator = managementObjectSearcher.Get().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var managementObject = (ManagementObject)enumerator.Current;
                        if (managementObject["DeviceID"] == null || managementObject["InterfaceType"] == null ||
                            managementObject["InterfaceType"].ToString() == "USB" ||
                            managementObject["InterfaceType"].ToString() == "1394") continue;
                        var flag = !(managementObject["MediaType"] != null && managementObject["MediaType"].ToString() == "Removable Media");
                        if (!flag) continue;
                        var obj = managementObject["SerialNumber"];
                        if (obj != null && obj.ToString().Trim() != string.Empty && obj.ToString()[0] != Convert.ToChar(31))
                        {
                            return obj.ToString().Trim();
                        }
                        arrayList.Add(managementObject["DeviceID"].ToString());
                    }
                }
                managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
                var managementObjectCollection = managementObjectSearcher.Get();
                foreach (string b in arrayList)
                {
                    using (var enumerator3 = managementObjectCollection.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            var managementObject2 = (ManagementObject)enumerator3.Current;
                            if (managementObject2["Tag"] == null) continue;
                            var a = managementObject2["Tag"].ToString();
                            if (a != b || managementObject2["SerialNumber"] == null) continue;
                            var obj2 = managementObject2["SerialNumber"];
                            if (obj2 != null && obj2.ToString() != string.Empty && obj2.ToString()[0] != Convert.ToChar(31))
                            {
                                return obj2.ToString().Trim().Replace(" ", "");
                            }
                            break;
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }

        internal static string GenerateHwid(bool proccessor, bool mac, bool motherboard, bool disk)
        {
            var text = "";
            RSACryptoServiceProvider.UseMachineKeyStore = true;
            var md5 = MD5.Create();
            if (proccessor)
            {
                var array = md5.ComputeHash(Encoding.Unicode.GetBytes(GetProcessorId()));
                text += array[3].ToString("X2");
                text = text + array[11].ToString("X2") + "-";
            }
            else
            {
                text = "85C1-";
            }
            if (mac)
            {
                var array2 = md5.ComputeHash(Encoding.Unicode.GetBytes(GetMacAddress()));
                text += array2[3].ToString("X2");
                text = text + array2[11].ToString("X2") + "-";
            }
            else
            {
                var array3 = md5.ComputeHash(Encoding.Unicode.GetBytes(text));
                text += array3[15].ToString("X2");
                text = text + array3[7].ToString("X2") + "-";
            }
            if (motherboard)
            {
                var array4 = md5.ComputeHash(Encoding.Unicode.GetBytes(GetMotherBoardId()));
                text += array4[3].ToString("X2");
                text = text + array4[11].ToString("X2") + "-";
            }
            else
            {
                var array5 = md5.ComputeHash(Encoding.Unicode.GetBytes(text));
                text += array5[2].ToString("X2");
                text = text + array5[14].ToString("X2") + "-";
            }
            if (disk)
            {
                var array6 = md5.ComputeHash(Encoding.Unicode.GetBytes(GetDiskId()));
                text += array6[3].ToString("X2");
                text = text + array6[11].ToString("X2") + "-";
            }
            else
            {
                var array7 = md5.ComputeHash(Encoding.Unicode.GetBytes(text));
                text += array7[1].ToString("X2");
                text = text + array7[9].ToString("X2") + "-";
            }
            var array8 = md5.ComputeHash(Encoding.Unicode.GetBytes(text));
            text += array8[1].ToString("X2");
            return text + array8[9].ToString("X2");
        }
    }
}
