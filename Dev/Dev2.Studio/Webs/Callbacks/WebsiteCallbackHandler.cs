using System;
using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
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

        #region Properties

        public Window Owner { get; set; }

        [Import]
        public IFrameworkRepository<IEnvironmentModel> CurrentEnvironmentRepository { get; set; }

        [Import]
        public IEventAggregator EventAggregator { get; set; }

        #endregion

        protected abstract void Save(IEnvironmentModel environmentModel, dynamic jsonArgs);

        #region Navigate

        protected virtual void Navigate(IEnvironmentModel environmentModel, string uri, dynamic jsonArgs, string returnUri)
        {
        }

        #endregion

        #region ReloadResource

        protected void ReloadResource(IEnvironmentModel environmentModel, string resourceName, ResourceType resourceType)
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

        #endregion

        #region Implementation of IPropertyEditorWizard

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get
            {
                return null;
            }
        }

        public virtual void Save(string value)
        {
            Save(value, EnvironmentRepository.DefaultEnvironment);
        }

        public virtual void Save(string value, IEnvironmentModel environmentModel)
        {
            Close();

            if(string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }
            dynamic jsonObj = JObject.Parse(value);
            Save(environmentModel, jsonObj);
        }

        public virtual void NavigateTo(string uri, string args, string returnUri)
        {
            dynamic jsonArgs = JObject.Parse(args);
            Navigate(EnvironmentRepository.DefaultEnvironment, uri, jsonArgs, returnUri);
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

        public virtual void Dev2ReloadResource(string resourceName, string resourceType)
        {
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

        public event NavigateRequestedEventHandler NavigateRequested;

        protected void Navigate(string uri)
        {
            if(NavigateRequested != null)
            {
                NavigateRequested(uri);
            }
        }

        #endregion
    }
}
