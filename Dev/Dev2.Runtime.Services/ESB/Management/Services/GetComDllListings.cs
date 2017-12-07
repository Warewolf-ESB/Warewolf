using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Microsoft.Win32;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetComDllListings : DefaultEsbManagementEndpoint
    {
        #region Implementation of DefaultEsbManagementEndpoint

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            Dev2Logger.Info("Get COMDll Listings", GlobalConstants.WarewolfInfo);
            
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
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                msg.HasError = true;
                msg.SetMessage(ex.Message);
            }

            return serializer.SerializeToBuilder(msg);
        }

        #endregion

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetComDllListingsService";
    }
}