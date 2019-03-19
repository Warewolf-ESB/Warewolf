#pragma warning disable
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