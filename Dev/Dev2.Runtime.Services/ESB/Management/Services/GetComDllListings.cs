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
            var regClis = Registry.ClassesRoot.OpenSubKey("CLSID");
            List<DllListing> dllListings = new List<DllListing>();
            try
            {
                if (regClis != null)
                {
                    foreach (var clsid in regClis.GetSubKeyNames())
                    {
                        var regClsidKey = regClis.OpenSubKey(clsid);
                        var progID = regClsidKey?.OpenSubKey("ProgID");
                        Guid g;
                        var tryParse = Guid.TryParse(clsid, out g);
                        if (tryParse)
                        {
                            if (dllListings.Exists(listing => listing.ClsId == clsid)) continue;
                            var typeFromClsid = Type.GetTypeFromCLSID(g);
                            if (!typeFromClsid.IsVisible) continue;
                            var progId = progID?.GetValue("").ToString();
                            dllListings.Add(new DllListing
                            {
                                Name = typeFromClsid.Name,
                                FullName = typeFromClsid.FullName,
                                Children = new List<IFileListing>(),
                                IsDirectory = false,
                                ClsId = clsid,
                                ProgId = progId
                            });
                        }

                        msg.HasError = false;
                        msg.Message = serializer.SerializeToBuilder(dllListings.OrderBy(listing => listing.Name));
                    }
                }
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