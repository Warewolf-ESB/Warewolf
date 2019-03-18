#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Microsoft.Win32;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class ComDllLoaderHandler: MarshalByRefObject
    {
        public static List<DllListing> TryGetListings(RegistryKey registry)
        {
            var dllListings = new List<DllListing>();
            var regClis = registry.OpenSubKey("CLSID");

            if (regClis != null)
            {
                foreach (var clsid in regClis.GetSubKeyNames())
                {
                    var regClsidKey = regClis.OpenSubKey(clsid);
                    RegistryKey progID = null;
                    Type typeFromProgID = null;
                    GetTypeFromCLSID(clsid, regClsidKey, ref progID, ref typeFromProgID);
                    if (typeFromProgID == null)
                    {
                        continue;
                    }
                    if (typeFromProgID != null)
                    {
                        var fullName = typeFromProgID.FullName;
                        dllListings.Add(new DllListing
                        {
                            ClsId = clsid,
                            Is32Bit = fullName.Equals("System.__ComObject"),
                            Name = progID.GetValue("").ToString(),
                            IsDirectory = false,
                            FullName = progID.GetValue("").ToString(),
                            Children = new IFileListing[0]
                        });
                    }
                    regClsidKey?.Close();
                }
                regClis.Close();
            }

            dllListings = dllListings.OrderBy(listing => listing.FullName).ToList();

            return dllListings;
        }

        static void GetTypeFromCLSID(string clsid, RegistryKey regClsidKey, ref RegistryKey progID, ref Type typeFromProgID)
        {
            if (regClsidKey != null)
            {
                progID = regClsidKey.OpenSubKey("ProgID");
                var regPath = regClsidKey.OpenSubKey("InprocServer32" +
                                                     "") ?? regClsidKey.OpenSubKey("LocalServer");

                if (regPath != null && progID != null && progID.GetValue("") != null)
                {
                    regPath.Close();
                    try
                    {
                        typeFromProgID = Type.GetTypeFromCLSID(Guid.Parse(clsid));
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error("GetComDllListingsService-Execute", e, GlobalConstants.WarewolfError);
                    }
                }
            }
        }
    }
}