using System;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Workspaces;
using Dev2.Webs.Callbacks;
using Newtonsoft.Json.Linq;

namespace Dev2.Core.Tests.Webs
{
    public class ServiceCallbackHandlerMock : ServiceCallbackHandler
    {
        public ServiceCallbackHandlerMock(IEventAggregator eventPublisher, IEnvironmentRepository environmentRepository, IShowDependencyProvider provider)
            : base(eventPublisher, environmentRepository, provider)
        {
        }

        public void TestSave(IEnvironmentModel environmentModel, JObject jsonObj)
        {
            base.Save(environmentModel, jsonObj);
        }

        public void TestCheckForServerMessages(IEnvironmentModel environmentModel, Guid id, IWorkspaceItemRepository workspace)
        {
            CheckForServerMessages(environmentModel, id, workspace);
        }
    }
}