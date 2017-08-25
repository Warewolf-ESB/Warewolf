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
        public List<DllListing> GetListings(RegistryKey registry)
        {
            List<DllListing> dllListings = new List<DllListing>();
            var regClis = registry.OpenSubKey("CLSID");

            if (regClis != null)
            {
                foreach (var clsid in regClis.GetSubKeyNames())
                {
                    var regClsidKey = regClis.OpenSubKey(clsid);
                    if (regClsidKey != null)
                    {
                        var progID = regClsidKey.OpenSubKey("ProgID");
                        var regPath = regClsidKey.OpenSubKey("InprocServer32" +
                                                             "") ?? regClsidKey.OpenSubKey("LocalServer");

                        if (regPath != null && progID != null)
                        {
                            var pid = progID.GetValue("");
                            regPath.Close();

                            try
                            {
                                if (pid != null)
                                {
                                    var typeFromProgID = Type.GetTypeFromCLSID(Guid.Parse(clsid));
                                    if (typeFromProgID == null)
                                    {
                                        continue;
                                    }
                                    var fullName = typeFromProgID.FullName;
                                    dllListings.Add(new DllListing
                                    {
                                        ClsId = clsid,
                                        Is32Bit = fullName.Equals("System.__ComObject"),
                                        Name = pid.ToString(),
                                        IsDirectory = false,
                                        FullName = pid.ToString(),
                                        Children = new IFileListing[0]
                                    });
                                }
                            }
                            catch (Exception e)
                            {
                                Dev2Logger.Error("GetComDllListingsService-Execute", e, GlobalConstants.WarewolfError);
                            }
                        }
                    }
                    regClsidKey?.Close();
                }
                regClis.Close();
            }

            dllListings = dllListings.OrderBy(listing => listing.FullName).ToList();

            return dllListings;
        }
    }
}