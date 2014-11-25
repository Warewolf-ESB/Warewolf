
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Converters.DateAndTime;
using Microsoft.VisualBasic.Devices;

// ReSharper disable InconsistentNaming
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
        string GetPhysicalMemoryTotalInformation();
        string GetCPUAvailableInformation();
        string GetCPUTotalInformation();
        string GetLanguageInformation();
        string GetRegionInformation();
        string GetUserRolesInformation(IIdentity currentIdentity);
        string GetDomainInformation();
        string GetUserNameInformation();
        string GetNumberOfWareWolfAgentsInformation();
    }

    [ExcludeFromCodeCoverage]
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
                    Dev2Logger.Log.Error(ex);
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
                    Dev2Logger.Log.Error(ex);
                }
            }
            return stringBuilder.ToString().TrimEnd(new[] { ',' });
        }

        public string GetPhysicalMemoryAvailableInformation()
        {
            var computerInfo = new ComputerInfo();
            var stringBuilder = new StringBuilder();
            var availablePhysicalMemory = ConvertToMB(computerInfo.AvailablePhysicalMemory);
            stringBuilder.Append(availablePhysicalMemory.ToString(CultureInfo.InvariantCulture));
            return stringBuilder.ToString();
        }


        public string GetPhysicalMemoryTotalInformation()
        {
            var computerInfo = new ComputerInfo();
            var stringBuilder = new StringBuilder();
            var totalPhysicalMemory = ConvertToMB(computerInfo.TotalPhysicalMemory);
            stringBuilder.Append(totalPhysicalMemory.ToString(CultureInfo.InvariantCulture));
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

            foreach(var o in searcher.Get())
            {
                var item = (ManagementObject)o;
                stringBuilder.Append((100 - Convert.ToInt32(item["LoadPercentage"])) + "%");
            }
            return stringBuilder.ToString();
        }

        public string GetCPUTotalInformation()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT MaxClockSpeed,NumberOfLogicalProcessors FROM Win32_Processor");
            var searcher = new ManagementObjectSearcher(winQuery);
            foreach(var o in searcher.Get())
            {
                var item = (ManagementObject)o;
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

        public string GetUserRolesInformation(IIdentity currentIdentity = null)
        {
            var stringBuilder = new StringBuilder();
            WindowsIdentity identity = currentIdentity as WindowsIdentity ?? WindowsIdentity.GetCurrent();
            if(identity != null)
            {
                if(identity.Groups != null)
                {
                    foreach(var sid in identity.Groups)
                    {
                        try
                        {
                            var translatedGroup = sid.Translate(typeof(NTAccount));
                            var name = translatedGroup.Value;
                            stringBuilder.AppendFormat(name + ",");
                        }
                        catch(Exception)
                        {
                            var winQuery = new ObjectQuery("SELECT * FROM Win32_Group WHERE SID='" + sid.Value + "'");
                            var searcher = new ManagementObjectSearcher(winQuery);
                            foreach(var o in searcher.Get())
                            {
                                var item = (ManagementObject)o;
                                var name = Convert.ToString(item["Name"]);
                                stringBuilder.AppendFormat(name + ",");
                            }
                        }
                    }
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
            stringBuilder.Append(managementObjectCollection.Count.ToString(CultureInfo.InvariantCulture));
            return stringBuilder.ToString();
        }

        #endregion
    }
}
