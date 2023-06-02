using System;
using System.IO;
using System.Text;
using System.Management;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows;

namespace SerialKeyFX
{
    public static class LicenseCheck
    {
        public static bool CheckLicense()
        {
            string computerCode = GetCode();

#if !DEBUG
            bool ret = false;
            try
            {
                var file = new StreamReader("../Licenses/License.lic");
                string currentSerialKey = file.ReadLine();
                ret = currentSerialKey != "" && computerCode == currentSerialKey;
            }
            catch { }
            if(!ret)
            {
                string cpuId = GetCpuID();
                bool fileWrited = false;
                try
                {
                    Directory.CreateDirectory("../Licenses");
                    var file = new StreamWriter("../Licenses/LicenseRequest.txt");
                    file.Write(cpuId);
                    file.Close();
                    fileWrited = true;
                }
                catch { }
                if (fileWrited)
                {
                    MessageBox.Show($@"You are not allowed to use this tool. " + Environment.NewLine +
                        "Please, contact Indra personnel and provide them the following code: " + Environment.NewLine +
                        cpuId + Environment.NewLine + Environment.NewLine +
                        "Or file " + Path.GetFullPath("../Licenses/LicenseRequest.txt"), "License error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($@"You are not allowed to use this tool. " + Environment.NewLine +
                        "Please, contact Indra personnel and provide them the following code: " + Environment.NewLine +
                        cpuId, "License error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Application.Current.Shutdown();
            }
            return ret;
#else
            return true;
#endif
        }

        #region Codify

        private static string GetCode()
        {
            return Codify(GetCpuID());
        }

        private static string GetCpuID()
        {
            var cpuInfo = "";
            var managClass = new ManagementClass("Win32_BaseBoard");
            var managCollec = managClass.GetInstances();
            foreach (ManagementObject managObj in managCollec)
            {
                if (!String.IsNullOrEmpty(managObj.Properties["SerialNumber"].Value.ToString()))
                {
                    cpuInfo = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName + "-" + Environment.MachineName + "-" + managObj.Properties["SerialNumber"].Value.ToString().ToLower();
                    return cpuInfo;
                }
            }

            managClass = new ManagementClass("Win32_BIOS");
            managCollec = managClass.GetInstances();
            foreach (ManagementObject managObj in managCollec)
            {
                if (!String.IsNullOrEmpty(managObj.Properties["SerialNumber"].Value.ToString()))
                {
                    cpuInfo = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName + "-" + Environment.MachineName + "-" + managObj.Properties["SerialNumber"].Value.ToString().ToLower();
                    return cpuInfo;
                }
            }

            managClass = new ManagementClass("Win32_ComputerSystemProduct");
            managCollec = managClass.GetInstances();
            foreach (ManagementObject managObj in managCollec)
            {
                if (!String.IsNullOrEmpty(managObj.Properties["IdentifyingNumber"].Value.ToString()))
                {
                    cpuInfo = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName + "-" + Environment.MachineName + "-" + managObj.Properties["IdentifyingNumber"].Value.ToString().ToLower();
                    return cpuInfo;
                }
            }

            managClass = new ManagementClass("win32_processor");
            managCollec = managClass.GetInstances();
            foreach (ManagementObject managObj in managCollec)
            {
                if (!String.IsNullOrEmpty(managObj.Properties["processorID"].Value.ToString()))
                {
                    cpuInfo = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName + "-" + Environment.MachineName + "-" + managObj.Properties["processorID"].Value.ToString().ToLower();
                    return cpuInfo;
                }
            }

            return System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName + "-" + Environment.MachineName + "-";
        }

        public static string Codify(string code)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                string newCode = code + "InDrA";
                string hash = GetMd5Hash(md5Hash, newCode);
                return hash;
            }
        }

        private static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        #endregion
    }
}