using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;
using Microsoft.Win32;
//http://procbits.com/2010/11/08/get-all-progid-on-system-for-com-automation
namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class GetComDllListings : IEsbManagementEndpoint
    {
        #region Implementation of ISpookyLoadable<out string>

        public string HandlesType()
        {
            return "GetComDllListingsService";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            Dev2Logger.Info("Get COMDll Listings");
            List<DllListing> dllListings = new List<DllListing>();
            try
            {
                var regClis = Registry.ClassesRoot.OpenSubKey("CLSID");
                
                

                if (regClis != null)
                {
                    foreach (var clsid in regClis.GetSubKeyNames())
                    {
                        var regClsidKey = regClis.OpenSubKey(clsid);
                        if (regClsidKey != null)
                        {
                            var progID = regClsidKey.OpenSubKey("ProgID");
                            var regPath = regClsidKey.OpenSubKey("InprocServer" +
                                                                 "") ?? regClsidKey.OpenSubKey("LocalServer32");

                            if (regPath != null && progID != null)
                            {
                                var pid = progID.GetValue("");
                                regPath.Close();

                                try
                                {
                                    if (pid != null)
                                    {
                                        var typeFromProgID = Type.GetTypeFromProgID(pid.ToString());
                                        if (typeFromProgID == null) continue;
                                        dllListings.Add(new DllListing
                                        {
                                            ClsId = clsid,
                                            ProgId = pid.ToString(),
                                            Name = typeFromProgID.Name,
                                            IsDirectory = false,
                                            FullName = typeFromProgID.FullName,
                                            Children = new IFileListing[0]
                                        });
                                    }
                                }
                                catch (Exception e)
                                {
                                    //Assert.Fail(e.Message);
                                    Dev2Logger.Error("GetComDllListingsService-Execute", e);
                                }
                            }

                        }


                        regClsidKey?.Close();
                    }

                    regClis.Close();
                }

                dllListings = dllListings.OrderBy(listing => listing.ProgId).ToList();
                msg.Message = serializer.SerializeToBuilder(dllListings);
            }

            catch (COMException ex)
            {
                Dev2Logger.Error(ex);
                msg.HasError = true;
                msg.SetMessage(ex.Message);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex);
                msg.HasError = true;
                msg.SetMessage(ex.Message);
            }

            return serializer.SerializeToBuilder(msg);
        }

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var findDirectoryService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };

            var findDirectoryServiceAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        #endregion
    }
}