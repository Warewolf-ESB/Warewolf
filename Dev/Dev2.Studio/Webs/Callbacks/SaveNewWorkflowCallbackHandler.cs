using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;

namespace Dev2.Studio.Webs.Callbacks
{
    public class SaveNewWorkflowCallbackHandler
       : WebsiteCallbackHandler
    {
        #region Fields

        private readonly IContextualResourceModel _resourceModel;
        private readonly bool _addToTabManager;

        #endregion

        public SaveNewWorkflowCallbackHandler(IContextualResourceModel resourceModel)
            : this(EnvironmentRepository.Instance,resourceModel,true)
        {
        }

        public SaveNewWorkflowCallbackHandler(IEnvironmentRepository currentEnvironmentRepository, IContextualResourceModel resourceModel, bool addToTabManager)
            : base(currentEnvironmentRepository)
        {
            _addToTabManager = addToTabManager;
            _resourceModel = resourceModel;
        }
   

        #region Overrides of WebsiteCallbackHandler

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            string resName = jsonObj.resourceName;
            string resCat = jsonObj.resourcePath;

            if (_resourceModel != null)
            {
                _resourceModel.IsNewWorkflow = false;
                EventAggregator.Publish(new SaveResourceMessage(_resourceModel, true));
                IContextualResourceModel newResourceModel = ResourceModelFactory.CreateResourceModel(_resourceModel.Environment, "Workflow",
                                                                                             resName);                
                newResourceModel.Category = resCat;
                newResourceModel.ResourceName = resName;
                newResourceModel.ServiceDefinition = _resourceModel.ServiceDefinition;
                newResourceModel.WorkflowXaml = _resourceModel.WorkflowXaml.Replace(_resourceModel.DisplayName, resName);
                newResourceModel.DataList = _resourceModel.DataList;
                newResourceModel.IsNewWorkflow = false;

                EventAggregator.Publish(new UpdateResourceMessage(newResourceModel));
                if (_addToTabManager)
                {
                    EventAggregator.Publish(new AddWorkSurfaceMessage(newResourceModel));
                }
                EventAggregator.Publish(new SaveResourceMessage(_resourceModel, false));
                EventAggregator.Publish(new RemoveResourceAndCloseTabMessage(_resourceModel));

                NewWorkflowNames.Instance.Remove(_resourceModel.ResourceName);
            }
            Close();
        }      

        #endregion
    }
}
