using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.InterfaceImplementors.WizardResourceKeys;
using Dev2.Studio.ViewModels.Web;
using Dev2.Studio.Views.UserInterfaceBuilder;
using Unlimited.Applications.BusinessDesignStudio.Views;
using Unlimited.Applications.BusinessDesignStudio.Views.WebsiteBuilder;

namespace Dev2.Studio.Webs
{

    [Export(typeof(IWebController))]
    public class WebController : IHandle<ShowWebpartWizardMessage>, IHandle<CloseWizardMessage>,
                                 IHandle<SetActivePageMessage>, IWebController
    {
        private readonly IPopupController _popupProvider;
        private readonly IEventAggregator _eventAggregator;
        private ILayoutGridViewModel _activePage;
        private WebPropertyEditorWindow _win;
        private ILayoutObjectViewModel _activeCell;

        [ImportingConstructor]
        public WebController([Import]IPopupController popupProvider, [Import]IEventAggregator eventAggregator)
        {
            _popupProvider = popupProvider;
            _eventAggregator = eventAggregator;
            if (_eventAggregator != null)
                _eventAggregator.Subscribe(this);

        }

        public void DisplayDialogue(IContextualResourceModel resourceModel, bool includeArgs, bool isSaveDialogStandAlone = false)
        {
            if (RootWebSite.ShowDialog(resourceModel, isSaveDialogStandAlone))
            {
                return;
            }
        }

        private void ShowWebpartWizard(IPropertyEditorWizard layoutObjectToOpenWizardFor)
        {
            if (layoutObjectToOpenWizardFor != null && layoutObjectToOpenWizardFor.SelectedLayoutObject != null &&
                layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid != null &&
                layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid.ResourceModel != null &&
                layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid.ResourceModel.Environment != null)
            {
                IEnvironmentModel environment =
                    layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid.ResourceModel.Environment;
                string relativeUri = string.Format("services/{0}.wiz",
                                                   layoutObjectToOpenWizardFor.SelectedLayoutObject.WebpartServiceName);
                Uri requestUri;
                if (!Uri.TryCreate(environment.Connection.WebServerUri, relativeUri, out requestUri))
                {
                    requestUri = new Uri(environment.Connection.WebServerUri, relativeUri);
                }

                try
                {
                    string xmlConfig = layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid == null
                                           ? layoutObjectToOpenWizardFor.SelectedLayoutObject.XmlConfiguration
                                           : layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid
                                                                        .XmlConfiguration;

                    string elementNames = ResourceHelper.GetWebPageElementNames(xmlConfig);
                    // Travis.Frisinger : 06-07-2012 - Remove junk in the config
                    string xmlOutput =
                        ResourceHelper.MergeXmlConfig(
                            layoutObjectToOpenWizardFor.SelectedLayoutObject.XmlConfiguration, elementNames);
                    ErrorResultTO errors;
                    Guid dataListID = environment.UploadToDataList(xmlOutput, out errors);

                    string uriString = Browser.FormatUrl(requestUri.AbsoluteUri, dataListID);
                    _win = new WebPropertyEditorWindow(layoutObjectToOpenWizardFor, uriString)
                    {
                        Width = 850,
                        Height = 600
                    };
                    _win.ShowDialog();
                }
                catch
                {
                }
            }
        }


        internal void AddUserInterfaceWorkflow(IContextualResourceModel resource,
                                               IWorkflowDesignerViewModel workflowViemModel)
        {
            bool isWebpage = ResourceHelper.IsWebpage(resource);
            Type userInterfaceType = ResourceHelper.GetUserInterfaceType(resource);
            if (userInterfaceType == null) return;

            var modelService = workflowViemModel.Designer.Context.Services.GetService<ModelService>();

            IEnumerable<ModelItem> items = modelService.Find(modelService.Root, userInterfaceType);

            IWebActivity webActivity = null;

            IList<ModelItem> modelItems = items as IList<ModelItem> ?? items.ToList();
            if (modelItems.Any())
            {
                ModelItem item = modelItems.First();
                ModelProperty displayNameProperty = item.Properties["DisplayName"];
                if (displayNameProperty != null)
                    webActivity = WebActivityFactory.CreateWebActivity(item, resource,
                                                                       displayNameProperty.ComputedValue.ToString());
                if (isWebpage)
                {
                    AddWebPageDesigner(webActivity);
                }
                else
                {
                    AddWebsiteDesigner(webActivity);
                }
            }
            else
            {
                ModelProperty implementationProperty = modelService.Root.Properties["Implementation"];
                if (modelService.Root.Content != null)
                {
                    var fc = (Flowchart)modelService.Root.Content.ComputedValue;
                    var fs = new FlowStep();
                    dynamic wsa = Activator.CreateInstance(userInterfaceType);
                    fs.Action = wsa;
                    fc.StartNode = fs;
                    if (implementationProperty != null)
                        if (implementationProperty.Value != null)
                        {
                            ModelProperty nodesProperty = implementationProperty.Value.Properties["Nodes"];
                            if (nodesProperty != null)
                                if (nodesProperty.Collection != null) nodesProperty.Collection.Add(fs);
                        }
                }
                if (implementationProperty != null)
                {
                    if (implementationProperty.Value != null)
                    {
                        ModelProperty nodesProperty = implementationProperty.Value.Properties["Nodes"];
                        if (nodesProperty != null)
                        {
                            if (nodesProperty.Collection != null)
                            {
                                ModelItem wsmodelitem = nodesProperty.Collection.Last();
                                ModelProperty actionProperty = wsmodelitem.Properties["Action"];
                                if (actionProperty != null)
                                {
                                    ModelItem amodelitem = actionProperty.Value;

                                    if (amodelitem != null)
                                    {
                                        ModelProperty displayNameProperty = amodelitem.Properties["DisplayName"];
                                        if (displayNameProperty != null)
                                            if (isWebpage)
                                            {
                                                webActivity = WebActivityFactory
                                                    .CreateWebActivity(amodelitem, resource,
                                                                       displayNameProperty.ComputedValue.ToString());
                                                AddWebPageDesigner(webActivity);
                                            }
                                            else
                                            {
                                                webActivity = WebActivityFactory
                                                    .CreateWebActivity(amodelitem, resource,
                                                                       displayNameProperty.ComputedValue.ToString());
                                                AddWebsiteDesigner(webActivity);
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void AddWebsiteDesigner(IWebActivity webActivity)
        {
            var viewModel = new WebsiteEditorViewModel(webActivity);
            ImportService.SatisfyImports(viewModel);
            var editor = new WebsiteEditorWindow(viewModel);
            //SetActive
        }

        internal void AddWebPageDesigner(IWebActivity webActivity)
        {
            if (webActivity == null) return;

            string xmlConfig = "<WebParts/>";

            if (!String.IsNullOrEmpty(webActivity.XMLConfiguration))
            {
                xmlConfig = webActivity.XMLConfiguration;
            }

            if (xmlConfig.StartsWith("[[")) return;

            if (!XmlHelper.IsValidXElement(xmlConfig)) return;

            var l = new LayoutGridViewModel(webActivity);
            ImportService.SatisfyImports(l);
            var layoutGrid = new AutoLayoutGridWindow(l);

            //Set the active page to signal user interface transitions.
            if (l.LayoutObjects.Any())
            {
                SetActivePage(l.LayoutObjects.First());
            }

            //SetActive
        }

        public void CloseWizard()
        {
            if (_win != null)
            {
                _win.Close();
            }
        }

        public void SetActivePage(ILayoutObjectViewModel cell)
        {
            if (cell == null) return;

            _activePage = cell.LayoutObjectGrid;
            _activeCell = cell;
            //NotifyOfPropertyChange(() => ActivePage);
        }

        #region IHandle

        public void Handle(ShowWebpartWizardMessage message)
        {
            ShowWebpartWizard(message.LayoutObjectViewModel);
        }

        public void Handle(CloseWizardMessage message)
        {
            CloseWizard();
        }

        public void Handle(SetActivePageMessage message)
        {
            SetActivePage(message.LayoutObjectViewModel);
        }
        #endregion IHandle

    }
}
