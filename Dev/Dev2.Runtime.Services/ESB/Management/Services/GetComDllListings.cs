using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;
using Microsoft.Win32;
//http://procbits.com/2010/11/08/get-all-progid-on-system-for-com-automation

namespace Dev2.Runtime.ESB.Management.Services
{

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
            Dev2Logger.Info("Get COMDll Listings", "Warewolf Info");
            
            try
            {
                List<DllListing> dllListings;
                using (Isolated<ComDllLoaderHandler> isolated = new Isolated<ComDllLoaderHandler>())
                {
                    var openBaseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32);
                    dllListings = isolated.Value.GetListings(openBaseKey);
                    openBaseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
                    dllListings.AddRange(isolated.Value.GetListings(openBaseKey));
                }
                msg.Message = serializer.SerializeToBuilder(dllListings);
            }
            catch (COMException ex)
            {
                msg.HasError = true;
                msg.SetMessage(ex.Message);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, "Warewolf Error");
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

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }
}