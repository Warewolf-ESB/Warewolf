using System;
using System.Collections.Generic;
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
    public class GetComDllListings : IEsbManagementEndpoint
    {
        private List<KeyValuePair<Guid, Type>> TypesKeyValuePairs { get; set; }
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

            #region Old Code

            /* StringBuilder dllListing;

            //values.TryGetValue("currentDllListing", out dllListing);
            //if (dllListing != null)
            //{
            //    var src = serializer.Deserialize(dllListing.ToString(), typeof(IFileListing)) as IFileListing;
            //    try
            //    {
            //        //msg.HasError = false;
            //        //msg.Message = serializer.SerializeToBuilder(GetDllListing(src));
            //    }
            //    catch (Exception)
            //    {
            //      /*  Dev2Logger.Error(ex);
            //        msg.HasError = true;
            //        msg.SetMessage(ex.Message);*/
            //    }
            //}*/

            #endregion
            

            TypesKeyValuePairs = new List<KeyValuePair<Guid, Type>>();
            using (var openSubKey = Registry.LocalMachine.OpenSubKey(@"Software\Classes\CLSID"))
            {
                if (openSubKey != null)
                {
                    try
                    {
                        var subKeyNames = openSubKey
                         .GetSubKeyNames()
                         .Where(s => !s.StartsWith("."))
                         .Distinct()
                         .ToArray();
                        foreach (var subKeyName in subKeyNames)
                        {
                            Guid g;
                            var tryParse = Guid.TryParse(subKeyName, out g);
                            if (tryParse)
                            {
                                if (TypesKeyValuePairs.Exists(pair => pair.Key == g)) continue;
                                var typeFromClsid = Type.GetTypeFromCLSID(g);
                                if (typeFromClsid.IsVisible)
                                {
                                    TypesKeyValuePairs.Add(new KeyValuePair<Guid, Type>(g, typeFromClsid));
                                }
                            }
                        }
                        var dllListings = TypesKeyValuePairs.Select(p => new DllListing() { Name = p.Value.Name, FullName = p.Value.FullName }).ToList();
                        msg.HasError = false;
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

                }

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