using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;


namespace Dev2.Studio.Core.ViewModels
{
    public class ResourceWizardViewModel : SimpleBaseViewModel, IPropertyEditorWizard
    {
        #region Class Members

        private IContextualResourceModel _resource;
        private string _wizardFeedback;

        #endregion Class Members

        #region Constructor

        public ResourceWizardViewModel(IContextualResourceModel model)
        {
            WebCommunication = ImportService.GetExportValue<IWebCommunication>();
            PopupProvider = ImportService.GetExportValue<IPopUp>();
            _resource = model;
        }

        #endregion Constructor

        #region Properties

        public Window Owner { get; set; }

        public IWebCommunication WebCommunication { get; set; }

        public IPopUp PopupProvider { get; set; }

        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// Stores the feedback from the wizard
        /// </summary>
        public string WizardFeedback
        {
            get
            {
                return _wizardFeedback;
            }
            private set
            {
                _wizardFeedback = value;
            }
        }

        #endregion

        #region Methods

        public void Update()
        {
            base.OnPropertyChanged("SelectedResource");
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Updates the model from the wizard feedback
        /// </summary>
        private void UpdateModel()
        {
            XElement feedback = XElement.Parse(WizardFeedback);

            _resource.ResourceName = feedback.Element("ResourceName").Value;
            _resource.ResourceType = (ResourceType)Enum.Parse(typeof(ResourceType), feedback.Element("ResourceType").Value);
            _resource.Category = feedback.Element("Category").Value;
            _resource.Comment = feedback.Element("Comment").Value;
            _resource.Tags = feedback.Element("Tags").Value;
            _resource.HelpLink = feedback.Element("HelpLink").Value;
            _resource.UpdateIconPath(feedback.Element("IconPath").Value);
        }

        #endregion Private Methods

        #region IPropertyEditorWizard Implementation

        #region Events

        #region NavigateRequested

        public string FetchData(string args)
        {
            return null;
        }

        public string GetIntellisenseResults(string searchTerm, int caretPosition)
        {
            return null;
        }

        public event NavigateRequestedEventHandler NavigateRequested;

        protected virtual void OnNavigateRequested(string uri)
        {
            if(NavigateRequested != null)
            {
                NavigateRequested(uri);
            }
        }

        #endregion NavigateRequested

        #endregion Events

        #region Properties

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get
            {
                return null;
            }
        }

        #endregion Properties

        #region Methods

        public void Save(string value, bool closeBrowserWindow = true)
        {
        }

        public void NavigateTo(string uri, string args, string returnUri)
        {

        }

        public void OpenPropertyEditor()
        {
            //Do nothing here as this view model doesn't handle children which need wizzards
        }

        public void Dev2Set(string data, string uri)
        {
            Uri postUri;
            if(!Uri.TryCreate(_resource.Environment.WebServerAddress, uri, out postUri))
            {
                if(!Uri.TryCreate(new Uri(StringResources.Uri_WebServer), uri, out postUri))
                {
                    throw new Exception("Unable to create the URL to post wizard information to the server.");
                }
            }

            //string postUri = string.Format("{0}{1}", MainViewModel.CurrentWebServer, uri);

            IWebCommunicationResponse response = WebCommunication.Post(postUri.AbsoluteUri, data);
            if(response != null)
            {
                switch(response.ContentType)
                {
                    case "text/html":
                        if(NavigateRequested != null)
                        {
                            NavigateRequested(postUri.AbsoluteUri);
                        }
                        break;

                    default:
                        PopupProvider.Buttons = MessageBoxButton.OK;
                        PopupProvider.Description = string.Concat("Unexpected result type from '", uri, "'.");
                        PopupProvider.Header = "Unexpected Result";
                        PopupProvider.ImageType = MessageBoxImage.Error;
                        PopupProvider.Show();
                        break;
                }
            }
            else
            {
                PopupProvider.Buttons = MessageBoxButton.OK;
                PopupProvider.Description = string.Concat("Unexpected result type from '", uri, "'.");
                PopupProvider.Header = "Unexpected Result";
                PopupProvider.ImageType = MessageBoxImage.Error;
                PopupProvider.Show();
            }
        }

        public void Dev2SetValue(string value)
        {
            WizardFeedback = value;
        }

        public void Dev2Done()
        {
            if(_resource != null && _resource.Environment != null && _resource.Environment.Resources != null)
            {
                try
                {
                    UpdateModel();
                }
                catch(Exception)
                {
                    PopupProvider.Buttons = MessageBoxButton.OK;
                    PopupProvider.Description = "Invalid response from wizard.";
                    PopupProvider.Header = "Invalid Response";
                    PopupProvider.ImageType = MessageBoxImage.Error;
                    PopupProvider.Show();
                }

                IResourceModel res = _resource.Environment.Resources.FindSingle(r => r.ResourceName == _resource.ResourceName);
                bool savedByWizard = (_resource.ResourceType == ResourceType.Source || _resource.ResourceType == ResourceType.Service);
                bool newResource = (res == null);

                if(savedByWizard)
                {
                    //
                    // Reload resource call
                    //
                    List<IResourceModel> effectedResources = _resource.Environment.Resources.ReloadResource(_resource.ResourceName, _resource.ResourceType, ResourceModelEqualityComparer.Current);
                    foreach(IResourceModel resource in effectedResources)
                    {
                        EventAggregator.Publish(new UpdateResourceMessage(resource));
                    }
                }
                else
                {
                    _resource.Environment.Resources.Save(_resource);
                    EventAggregator.Publish(new UpdateResourceMessage(_resource));
                }

                if(newResource && _resource.ResourceType == ResourceType.WorkflowService)
                {
                    Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, _resource);
                }

                //if (!savedByWizard && newResource)
                //{
                //    _resource.Environment.Resources.Save(_resource);
                //    if (_resource.ResourceType == ResourceType.WorkflowService)
                //    {
                //        Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, _resource);
                //    }
                //    else
                //    {
                //        Mediator.SendMessage(MediatorMessages.AddResourceDocument, _resource);
                //    }
                //    EventAggregator.Publish(new UpdateResourceMessage(_resource));
                //}
                //else if (!savedByWizard && !newResource)
                //{
                //    _resource.Environment.Resources.Save(_resource);
                //    EventAggregator.Publish(new UpdateResourceMessage(_resource));
                //}
                //else if (savedByWizard)
                //{
                //    //
                //    // Reload resource call
                //    //
                //    List<IResourceModel> effectedResources = _resource.Environment.Resources.ReloadResource(_resource.ResourceName, _resource.ResourceType, ResourceModelEqualityComparer.Current);
                //    foreach (IResourceModel resource in effectedResources)
                //    {
                //        EventAggregator.Publish(new UpdateResourceMessage(resource));
                //    }
                //}
            }

            Close();
        }

        public void Dev2ReloadResource(string resourceName, string resourceType)
        {
            ResourceType parsedResourceType;
            if(Enum.TryParse<ResourceType>(resourceType, out parsedResourceType) &&
                _resource != null && _resource.Environment != null && _resource.Environment.Resources != null)
            {
                //
                // Reload resource call
                //
                List<IResourceModel> effectedResources = _resource.Environment.Resources.ReloadResource(resourceName, parsedResourceType, ResourceModelEqualityComparer.Current);
                foreach(IResourceModel resource in effectedResources)
                {
                    EventAggregator.Publish(new UpdateResourceMessage(resource));
                }
            }
        }

        public void Close()
        {
            Mediator.SendMessage(MediatorMessages.CloseWizard, this);
        }

        public void Cancel()
        {
            Mediator.SendMessage(MediatorMessages.CloseWizard, this);
        }

        #endregion Methods

        #endregion IPropertyEditorWizard Implementation
    }
}
