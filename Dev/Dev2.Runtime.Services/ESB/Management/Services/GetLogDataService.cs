using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetLogDataService : IEsbManagementEndpoint
    {
        private string _serverLogFilePath;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Administrator;
        }

        public string HandlesType()
        {
            return "GetLogDataService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2Logger.Info("Get Log Data Service");

            var serializer = new Dev2JsonSerializer();
            try
            {
//                if (values == null)
//                {
//                    throw new ArgumentNullException(nameof(values));
//                }
//                StringBuilder tmp;
//                values.TryGetValue("ReloadResourceCatalogue", out tmp);
//                string reloadResourceCatalogueString = "";
//                if (tmp != null)
//                {
//                    reloadResourceCatalogueString = tmp.ToString();
//                }
//                bool reloadResourceCatalogue = false;
//                if (!string.IsNullOrEmpty(reloadResourceCatalogueString))
//                {
//
//                    if (!bool.TryParse(reloadResourceCatalogueString, out reloadResourceCatalogue))
//                    {
//                        reloadResourceCatalogue = false;
//                    }
//                }
//                if (reloadResourceCatalogue)
//                {
//                }
                return serializer.SerializeToBuilder("");
            }
            catch (Exception e)
            {
                Dev2Logger.Info("Get Log Data ServiceError", e);
            }
            return serializer.SerializeToBuilder("");
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
        
        public string ServerLogFilePath
        {
            get
            {
                return _serverLogFilePath ?? EnvironmentVariables.ServerLogFile;
            }
            set
            {
                _serverLogFilePath = value;
            }
        }
    }
}