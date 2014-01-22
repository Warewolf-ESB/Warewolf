using System;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Converters.DateAndTime;
using Microsoft.VisualBasic.Devices;

namespace Dev2.Activities
{
    public interface IGetSystemInformation
    {
        //CPU Available
        //CPU Total
        //Number of Warewolf Agents

        string GetOperatingSystemInformation();
        string GetServicePackInformation();
        string GetOSBitValueInformation();
        string GetFullDateTimeInformation();
        string GetDateTimeFormatInformation();
        string GetDiskSpaceAvailableInformation();
        string GetDiskSpaceTotalInformation();
        string GetPhysicalMemoryAvailableInformation();
        string GetVirtualMemoryAvailableInformation();
        string GetPhysicalMemoryTotalInformation();
        string GetVirtualMemoryTotalInformation();
        string GetCPUAvailableInformation();
        string GetCPUTotalInformation();
        string GetLanguageInformation();
        string GetRegionInformation();
        string GetUserRolesInformation();
        string GetDomainInformation();
        string GetUserNameInformation();
        string GetNumberOfWareWolfAgentsInformation();
    }

    public class GetSystemInformationHelper : IGetSystemInformation
    {
        #region Implementation of IGetSystemInformation

        public string GetOperatingSystemInformation()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                        select x.GetPropertyValue("Caption")).First();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0}", name);
            return stringBuilder.ToString();
        }

        public string GetServicePackInformation()
        {

            var stringBuilder = new StringBuilder();
            var operatingSystem = Environment.OSVersion;
            stringBuilder.AppendFormat("{0}", operatingSystem.ServicePack.Replace("Service Pack ", ""));
            return stringBuilder.ToString();
        }

        public string GetOSBitValueInformation()
        {

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(Environment.Is64BitOperatingSystem ? " 64" : " 32");
            return stringBuilder.ToString();
        }

        public string GetFullDateTimeInformation()
        {
            string fullPattern = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;
            if(fullPattern.Contains("ss"))
            {
                fullPattern = fullPattern.Insert(fullPattern.IndexOf("ss", StringComparison.Ordinal) + 2, ".fff");
            }
            var dateTimeString = DateTime.Now.ToString(fullPattern);
            return dateTimeString;
        }

        public string GetDateTimeFormatInformation()
        {
            var dateTimeParser = new DateTimeParser();
            string error;
            var translatedDateTimeFormat = dateTimeParser.TranslateDotNetToDev2Format(CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern, out error);
            return translatedDateTimeFormat;
        }

        public string GetDiskSpaceAvailableInformation()
        {
            var stringBuilder = new StringBuilder();
            foreach(System.IO.DriveInfo driveInfo1 in System.IO.DriveInfo.GetDrives())
            {
                try
                {
                    stringBuilder.AppendFormat("{0}" +
                      " {1},",
                      driveInfo1.Name, ConvertToGB(driveInfo1.AvailableFreeSpace));
                }
                catch(Exception ex)
                {
                    this.LogError(ex);
                }
            }
            return stringBuilder.ToString().TrimEnd(new[] { ',' });
        }

        public string GetDiskSpaceTotalInformation()
        {
            var stringBuilder = new StringBuilder();
            foreach(System.IO.DriveInfo driveInfo1 in System.IO.DriveInfo.GetDrives())
            {
                try
                {
                    stringBuilder.AppendFormat("{0}" + " {1},",
                        driveInfo1.Name, ConvertToGB(driveInfo1.TotalSize));
                }
                catch(Exception ex)
                {
                    this.LogError(ex);
                }
            }
            return stringBuilder.ToString().TrimEnd(new[] { ',' });
        }

        public string GetPhysicalMemoryAvailableInformation()
        {
            var computerInfo = new ComputerInfo();
            var stringBuilder = new StringBuilder();
            var availablePhysicalMemory = ConvertToMB(computerInfo.AvailablePhysicalMemory);
            stringBuilder.Append(availablePhysicalMemory.ToString());
            return stringBuilder.ToString();
        }

        public string GetVirtualMemoryAvailableInformation()
        {
            var computerInfo = new ComputerInfo();
            var stringBuilder = new StringBuilder();
            var availableVirtualMemory = ConvertToMB(computerInfo.AvailableVirtualMemory);
            stringBuilder.Append(availableVirtualMemory.ToString());
            return stringBuilder.ToString();
        }

        public string GetPhysicalMemoryTotalInformation()
        {
            var computerInfo = new ComputerInfo();
            var stringBuilder = new StringBuilder();
            var totalPhysicalMemory = ConvertToMB(computerInfo.TotalPhysicalMemory);
            stringBuilder.Append(totalPhysicalMemory.ToString());
            return stringBuilder.ToString();
        }

        public string GetVirtualMemoryTotalInformation()
        {
            var computerInfo = new ComputerInfo();
            var stringBuilder = new StringBuilder();
            var totalVirtualMemory = ConvertToMB(computerInfo.TotalVirtualMemory);
            stringBuilder.Append(totalVirtualMemory.ToString());
            return stringBuilder.ToString();
        }

        ulong ConvertToMB(ulong valueToConvert)
        {
            var convertedValue = valueToConvert / 1024 / 1024;
            return convertedValue;
        }

        ulong ConvertToGB(long valueToConvert)
        {
            var convertedValue = ConvertToMB((ulong)valueToConvert) / 1024;
            return convertedValue;
        }

        public string GetCPUAvailableInformation()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT LoadPercentage FROM Win32_Processor");
            var searcher = new ManagementObjectSearcher(winQuery);

            foreach(ManagementObject item in searcher.Get())
            {
                stringBuilder.Append((100 - Convert.ToInt32(item["LoadPercentage"])) + "%");
            }
            return stringBuilder.ToString();
        }

        public string GetCPUTotalInformation()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT MaxClockSpeed,NumberOfLogicalProcessors FROM Win32_Processor");
            var searcher = new ManagementObjectSearcher(winQuery);
            foreach(ManagementObject item in searcher.Get())
            {
                var maxClockSpeed = Convert.ToInt32(item["MaxClockSpeed"]);
                var numberOfProcessors = Convert.ToInt32(item["NumberOfLogicalProcessors"]);
                stringBuilder.Append(numberOfProcessors + "*" + maxClockSpeed + " Mhz");
            }
            return stringBuilder.ToString();
        }

        public string GetLanguageInformation()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0}", CultureInfo.CurrentCulture.Parent.DisplayName);
            return stringBuilder.ToString();
        }

        public string GetRegionInformation()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0}", RegionInfo.CurrentRegion.DisplayName);
            return stringBuilder.ToString();
        }

        public string GetUserRolesInformation()
        {
            var stringBuilder = new StringBuilder();
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            if(currentIdentity != null)
            {
                var groups = from sid in currentIdentity.Groups select sid.Translate(typeof(NTAccount)).Value;
                foreach(var grp in groups)
                {
                    stringBuilder.AppendFormat(grp + ",");
                }
            }
            return stringBuilder.ToString().TrimEnd(new[] { ',' });
        }

        public string GetUserNameInformation()
        {
            return Environment.UserName;
        }

        public string GetDomainInformation()
        {
            return Environment.UserDomainName;
        }

        public string GetNumberOfWareWolfAgentsInformation()
        {
            //Note this is for future functionality
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT Name FROM Win32_Process Where Name LIKE '%chrome%'");
            var searcher = new ManagementObjectSearcher(winQuery);
            var managementObjectCollection = searcher.Get();
            stringBuilder.Append(managementObjectCollection.Count.ToString());
            return stringBuilder.ToString();
        }

        #endregion
    }
}