using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Utils;
using Dev2.Webs.Callbacks;
using System;

// ReSharper disable once CheckNamespace
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
            : this(EnvironmentRepository.Instance, resourceModel)
        {
        }

        public SaveNewWorkflowCallbackHandler(IEnvironmentRepository currentEnvironmentRepository, IContextualResourceModel resourceModel)
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

                if(_resourceModel != null)
                {
                    _resourceModel.IsNewWorkflow = false;
                    this.TraceInfo("Publish message of type - " + typeof(SaveResourceMessage));
                    EventPublisher.Publish(new SaveResourceMessage(_resourceModel, true, false));
                    IContextualResourceModel newResourceModel =
                        ResourceModelFactory.CreateResourceModel(_resourceModel.Environment, "Workflow",
                                                                 resName);
                    newResourceModel.Category = resCat;
                    newResourceModel.ResourceName = resName;

                    newResourceModel.WorkflowXaml = _resourceModel.WorkflowXaml.Replace(_resourceModel.DisplayName,
                                                                                        resName);
                    newResourceModel.DataList = _resourceModel.DataList;
                    newResourceModel.IsNewWorkflow = false;

                    this.TraceInfo("Publish message of type - " + typeof(UpdateResourceMessage));
                    EventPublisher.Publish(new UpdateResourceMessage(newResourceModel));
                    if(_addToTabManager)
                    {
                        this.TraceInfo("Publish message of type - " + typeof(AddWorkSurfaceMessage));
                        EventPublisher.Publish(new AddWorkSurfaceMessage(newResourceModel));
                    }
                    this.TraceInfo("Publish message of type - " + typeof(SaveResourceMessage));
                    EventPublisher.Publish(new SaveResourceMessage(newResourceModel, false, _addToTabManager));
                    this.TraceInfo("Publish message of type - " + typeof(RemoveResourceAndCloseTabMessage));
                    EventPublisher.Publish(new RemoveResourceAndCloseTabMessage(_resourceModel));

                    NewWorkflowNames.Instance.Remove(_resourceModel.ResourceName);
                }

                Close();
            }
            catch(Exception e)
            {
                Exception e1 = new Exception("There was a problem saving. Please try again.", e);

                Logger.TraceInfo(e.Message + Environment.NewLine + " Stacktrace : " + e.StackTrace + Environment.NewLine + " jsonObj: " + jsonObj.ToString());

                throw e1;
            }
        }
        #endregion
    }
}
