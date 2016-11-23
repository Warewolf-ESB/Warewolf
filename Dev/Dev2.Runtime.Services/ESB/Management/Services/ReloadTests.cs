using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Reload a resource from disk ;)
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ReloadTests : IEsbManagementEndpoint
    {
        #region Implementation of ISpookyLoadable<out string>



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
            CompressedExecuteMessage result = new CompressedExecuteMessage { HasError = false };
            try
            {
                TestCatalog.Load();
                result.SetMessage("Test reload succesful");
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.SetMessage("Error reloading tests...");
                Dev2Logger.Error(ex);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            DynamicService reloadResourceServicesBinder = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceID ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction reloadResourceServiceActionBinder = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };
            reloadResourceServicesBinder.Actions.Add(reloadResourceServiceActionBinder);
            return reloadResourceServicesBinder;
        }

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

        public string HandlesType()
        {
            return "ReloadTestsService";
        }
        public ITestCatalog TestCatalog
        {
            private get
            {
                return _testCatalog ?? Runtime.TestCatalog.Instance;
            }
            set
            {
                _testCatalog = value;
            }
        }
        private ITestCatalog _testCatalog;
        #endregion
    }
}
