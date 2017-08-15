using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;



namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>

    public class FetchTests : IEsbManagementEndpoint
    {


        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            StringBuilder tmp;
            requestArgs.TryGetValue("resourceID", out tmp);
            if (tmp != null)
            {
                Guid resourceId;
                if (Guid.TryParse(tmp.ToString(), out resourceId))
                {
                    return resourceId;
                }
            }

            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        private ITestCatalog _testCatalog;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Fetch Tests Service", "Warewolf Info");

                StringBuilder resourceIdString;
                values.TryGetValue("resourceID", out resourceIdString);
                if (resourceIdString == null)
                {
                    throw new InvalidDataContractException("resourceID is missing");
                }
                Guid resourceId;
                if (!Guid.TryParse(resourceIdString.ToString(), out resourceId))
                {
                    throw new InvalidDataContractException("resourceID is not a valid GUID.");
                }
                var tests = TestCatalog.Fetch(resourceId);
                CompressedExecuteMessage message = new CompressedExecuteMessage();
                message.SetMessage(serializer.Serialize(tests));
                message.HasError = false;
                
                return serializer.SerializeToBuilder(message);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, "Warewolf Error");
                var res = new CompressedExecuteMessage { HasError = true, Message = new StringBuilder(err.Message) };
                return serializer.SerializeToBuilder(res);
            }
        }

        public ITestCatalog TestCatalog
        {
            get
            {
                return _testCatalog ?? Runtime.TestCatalog.Instance;
            }
            set
            {
                _testCatalog = value;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType() };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "FetchTests";
        }
    }
}