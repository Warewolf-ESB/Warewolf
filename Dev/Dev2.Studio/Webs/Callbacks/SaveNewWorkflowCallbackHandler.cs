using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Factory;

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
            : this(EnvironmentRepository.Instance, resourceModel, true)
        {
        }

        public SaveNewWorkflowCallbackHandler(IEnvironmentRepository currentEnvironmentRepository, IContextualResourceModel resourceModel, bool addToTabManager)
            : this(EventPublishers.Aggregator, currentEnvironmentRepository, resourceModel, true)
        {
        }

        public SaveNewWorkflowCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository, IContextualResourceModel resourceModel, bool addToTabManager)
            : base(eventPublisher, currentEnvironmentRepository)
        {
            _addToTabManager = addToTabManager;
            _resourceModel = resourceModel;
        }
   

        #region Overrides of WebsiteCallbackHandler

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            try
            {
                string resName = jsonObj.resourceName;
                string resCat = jsonObj.resourcePath;

                if (_resourceModel != null)
                {
                    _resourceModel.IsNewWorkflow = false;
                    _eventPublisher.Publish(new SaveResourceMessage(_resourceModel, true, false));
                    IContextualResourceModel newResourceModel =
                        ResourceModelFactory.CreateResourceModel(_resourceModel.Environment, "Workflow",
                                                                 resName);
                    newResourceModel.Category = resCat;
                    newResourceModel.ResourceName = resName;
                    newResourceModel.ServiceDefinition = _resourceModel.ServiceDefinition;
                    newResourceModel.WorkflowXaml = _resourceModel.WorkflowXaml.Replace(_resourceModel.DisplayName,
                                                                                        resName);
                    newResourceModel.DataList = _resourceModel.DataList;
                    newResourceModel.IsNewWorkflow = false;

                    _eventPublisher.Publish(new UpdateResourceMessage(newResourceModel));
                    if (_addToTabManager)
                    {
                        _eventPublisher.Publish(new AddWorkSurfaceMessage(newResourceModel));
                    }
                    _eventPublisher.Publish(new SaveResourceMessage(newResourceModel, false, _addToTabManager));
                    _eventPublisher.Publish(new RemoveResourceAndCloseTabMessage(_resourceModel));

                    NewWorkflowNames.Instance.Remove(_resourceModel.ResourceName);
                }

                Close();
            }
            catch (Exception e)
            {
                Exception e1 = new Exception("There was a problem saving. Please try again.", e);

                StudioLogger.LogMessage(e.Message + Environment.NewLine + " Stacktrace : " + e.StackTrace + Environment.NewLine + " jsonObj: " + jsonObj.ToString());

                throw e1;
            }
        }
        #endregion
    }
}
