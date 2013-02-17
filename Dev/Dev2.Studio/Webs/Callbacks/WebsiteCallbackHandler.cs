using System;
using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Newtonsoft.Json.Linq;

namespace Dev2.Studio.Webs.Callbacks
{
    public abstract class WebsiteCallbackHandler : IPropertyEditorWizard
    {
        protected WebsiteCallbackHandler()
        {
            ImportService.SatisfyImports(this);
        }

        public Window Owner { get; set; }

        [Import]
        public IFrameworkRepository<IEnvironmentModel> CurrentEnvironmentRepository { get; set; }

        [Import]
        public IEventAggregator EventAggregator { get; set; }

        protected abstract void Save(IEnvironmentModel environmentModel, dynamic jsonObj);

        protected void ReloadResource(IEnvironmentModel environmentModel, string resourceName, Core.AppResources.Enums.ResourceType resourceType)
        {
            if(EventAggregator == null || environmentModel == null || environmentModel.Resources == null)
            {
                return;
            }
            var effectedResources = environmentModel.Resources.ReloadResource(resourceName, resourceType, ResourceModelEqualityComparer.Current);
            foreach(var resource in effectedResources)
            {
                EventAggregator.Publish(new UpdateResourceMessage(resource));
            }
        }

        #region Implementation of IPropertyEditorWizard

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get
            {
                return null;
            }
        }

        public void OpenPropertyEditor()
        {
        }

        public void Dev2Set(string data, string uri)
        {
        }

        public void Dev2SetValue(string value)
        {
            Dev2SetValue(value, EnvironmentRepository.DefaultEnvironment);
        }

        public void Dev2SetValue(string value, IEnvironmentModel environmentModel)
        {
            Close();

            if(string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }
            dynamic jsonObj = JObject.Parse(value);
            Save(environmentModel, jsonObj);
        }

        public void Dev2Done()
        {
        }

        public void Dev2ReloadResource(string resourceName, string resourceType)
        {
        }

        public void Close()
        {
            if(Owner != null)
            {
                Owner.Close();
            }
        }

        public void Cancel()
        {
            Close();
        }

        public event NavigateRequestedEventHandler NavigateRequested;

        protected void OnNavigateRequested(string uri)
        {
            if(NavigateRequested != null)
            {
                NavigateRequested(uri);
            }
        }

        #endregion
    }
}
