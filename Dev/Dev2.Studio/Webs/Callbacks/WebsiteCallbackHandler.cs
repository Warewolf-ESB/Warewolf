using System;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Utils;
using Dev2.Composition;
using Dev2.Interfaces;
using Dev2.Providers.Logs;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.InterfaceImplementors;
using Dev2.Workspaces;
using Newtonsoft.Json.Linq;

namespace Dev2.Webs.Callbacks
{
    public abstract class WebsiteCallbackHandler : IPropertyEditorWizard
    {
        protected readonly IEventAggregator EventPublisher;

        protected WebsiteCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository, IShowDependencyProvider showDependencyProvider = null)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("currentEnvironmentRepository", currentEnvironmentRepository);
            EventPublisher = eventPublisher;

            CurrentEnvironmentRepository = currentEnvironmentRepository;
            ImportService.SatisfyImports(this);
            ShowDependencyProvider = showDependencyProvider ?? new ShowDependencyProvider();
        }

        public IShowDependencyProvider ShowDependencyProvider { get; set; }

        #region Properties

        public Window Owner { get; set; }

        public IEnvironmentRepository CurrentEnvironmentRepository { get; private set; }

        #endregion

        protected abstract void Save(IEnvironmentModel environmentModel, dynamic jsonArgs);

        #region Navigate

        protected virtual void Navigate(IEnvironmentModel environmentModel, string uri, dynamic jsonArgs, string returnUri)
        {
        }

        #endregion

        #region ReloadResource

        protected void ReloadResource(IEnvironmentModel environmentModel, Guid resourceID, ResourceType resourceType)
        {
            if(environmentModel == null || environmentModel.ResourceRepository == null)
            {
                return;
            }

            var getWorksurfaceItemRepo = WorkspaceItemRepository.Instance;

            CheckForServerMessages(environmentModel, resourceID, getWorksurfaceItemRepo);
            var effectedResources = environmentModel.ResourceRepository.ReloadResource(resourceID, resourceType, ResourceModelEqualityComparer.Current, true);
            if(effectedResources != null)
            {
                foreach(var resource in effectedResources)
                {
                    var resourceWithContext = new ResourceModel(environmentModel);
                    resourceWithContext.Update(resource);
                    this.TraceInfo("Publish message of type - " + typeof(UpdateResourceMessage));
                    EventPublisher.Publish(new UpdateResourceMessage(resourceWithContext));
                }
            }
        }

        #endregion

        #region Implementation of IPropertyEditorWizard

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get
            {
                return null;
            }
        }

        public virtual void Save(string value, bool closeBrowserWindow = true)
        {
            Save(value, EnvironmentRepository.Instance.Source, closeBrowserWindow);
        }

        public virtual void Save(string value, IEnvironmentModel environmentModel, bool closeBrowserWindow = true)
        {
            if(closeBrowserWindow)
            {
                Close();
            }

            if(string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }
            value = JSONUtils.ScrubJSON(value);

            dynamic jsonObj = JObject.Parse(value);
            Save(environmentModel, jsonObj);
        }

        public virtual void OpenPropertyEditor()
        {
        }

        public virtual void Dev2Set(string data, string uri)
        {
        }

        public virtual void Dev2SetValue(string value)
        {
        }

        public virtual void Dev2Done()
        {
        }

        public virtual void Dev2ReloadResource(Guid resourceName, string resourceType)
        {
            throw new NotImplementedException();
        }

        public virtual void Close()
        {
            if(Owner != null)
            {
                Owner.Close();
            }
        }

        public virtual void Cancel()
        {
            Close();
        }

        public string FetchData(string args)
        {
            return null;
        }

        public string GetIntellisenseResults(string searchTerm, int caretPosition)
        {
            return GetJsonIntellisenseResults(searchTerm, caretPosition);
        }

        public event NavigateRequestedEventHandler NavigateRequested;

        protected void Navigate(string uri)
        {
            if(NavigateRequested != null)
            {
                NavigateRequested(uri);
            }
        }

        #endregion

        #region GetJsonIntellisenseResults

        public static string GetJsonIntellisenseResults(string searchTerm, int caretPosition)
        {
            var provider = new DefaultIntellisenseProvider();
            var context = new IntellisenseProviderContext { InputText = searchTerm, CaretPosition = caretPosition };

            return "[" + string.Join(",", provider.GetIntellisenseResults(context).Where(c => !c.IsError).Select(r => string.Format("\"{0}\"", r.ToString()))) + "]";
        }

        #endregion

        protected void CheckForServerMessages(IEnvironmentModel environmentModel, Guid resourceID, IWorkspaceItemRepository workspace)
        {
            var resourceModel = environmentModel.ResourceRepository.FindSingle(model => model.ID == resourceID);
            if(resourceModel != null)
            {
                var resource = new ResourceModel(environmentModel);
                resource.Update(resourceModel);
                var compileMessageList = new StudioCompileMessageRepo().GetCompileMessagesFromServer(resource);

                if(compileMessageList.Count == 0)
                {
                    return;
                }

                var numberOfDependants = compileMessageList.Dependants.Count;
                //2013.07.29: Ashley Lewis for bug 9640 - If only dependancy is open right now, don't notify of change
                if(numberOfDependants == 1)
                {
                    if(
                        compileMessageList.Dependants.Any(
                            dep =>
                                workspace.WorkspaceItems != null &&
                                workspace.WorkspaceItems.Any(c => c.ServiceName == dep)))
                    {
                        return;
                    }
                }
                //2013.07.29: Ashley Lewis for bug 10199 - Don't show dependancy viewer dialog on source type callback
                if(resource.ResourceType != ResourceType.Source)
                {
                    ShowDependencyProvider.ShowDependencyViewer(resource, compileMessageList.Dependants);
                }
            }
        }
    }
}
