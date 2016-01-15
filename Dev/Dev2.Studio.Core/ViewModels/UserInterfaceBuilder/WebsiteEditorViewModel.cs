
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using CefSharp.Wpf;
using Dev2.Common;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Exceptions;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Base;
using Unlimited.Framework;

namespace Dev2.Studio.Core.ViewModels
{
    public class WebsiteEditorViewModel : SimpleBaseViewModel, IWebsiteEditorViewModel
    {
        #region Class Members

        private string _html = string.Empty;
        private readonly string _currentWebPartName = string.Empty;
        private string _metaTags = string.Empty;
        private bool _isValidMarkup = true;
        private readonly IWebActivity _webActivity;
        private readonly IContextualResourceModel _resource;
        private IResourceModel _selectedDefaultWebpage;
        private readonly ObservableCollection<ILayoutObjectViewModel> _layoutObjects;
        private IFrameworkRepository<IWebResourceViewModel> _webResources;
        private IWebResourceViewModel _rootWebResource;
        private RelayCommand _editCommand;
        private RelayCommand _addExistingWebResourceCommand;
        private RelayCommand _removedWebResourceCommand;
        private RelayCommand _copyCommand;
        //private ILayoutGridViewModel _grid;
        private int _rows = 2;
        private ILayoutObjectViewModel[] previous;
        private ILayoutObjectViewModel[] lastGood;

        public string FetchData(string args)
        {
            return null;
        }

        public string GetIntellisenseResults(string searchTerm, int caretPosition)
        {
            return null;
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

        #region Ctor

        public WebsiteEditorViewModel(IWebActivity webActivity)
        {
            WebCommunication = ImportService.GetExportValue<IWebCommunication>();
            FileNameProvider = ImportService.GetExportValue<IFileNameProvider>();
            UserMessageProvider = ImportService.GetExportValue<IUserMessageProvider>();

            if(webActivity == null)
            {
                throw new ArgumentNullException("webActivity");
            }
            _webActivity = webActivity;
            _layoutObjects = new ObservableCollection<ILayoutObjectViewModel>();

            _resource = webActivity.ResourceModel;

            InitializeWebResources();
            SetDefaultWebpage();
        }

        #endregion

        #region Properties

        public Window Owner { get; set; }

        public IWebCommunication WebCommunication { get; set; }

        public IFileNameProvider FileNameProvider { get; set; }

        public IUserMessageProvider UserMessageProvider { get; set; }

        public bool IsValidMarkup
        {
            get
            {
                return _isValidMarkup;
            }
            set
            {
                _isValidMarkup = value;
                base.OnPropertyChanged("IsValidMarkup");
            }
        }


        public string MetaTags
        {
            get
            {
                return _metaTags;
            }
            set
            {
                _metaTags = value;
                base.OnPropertyChanged("MetaTags");
                WriteToModelItemDataProperty("DEV2MetaTags", _metaTags);

                Deploy();
            }
        }

        public IWebResourceViewModel SelectedWebResource
        {
            get;
            set;
        }


        public object Browser
        {
            get;
            set;
        }

        public IEnvironmentModel ResourceEnvironment
        {
            get
            {
                IEnvironmentModel environment = null;

                if(_webActivity != null && _webActivity.ResourceModel != null)
                {
                    environment = _webActivity.ResourceModel.Environment;
                }

                return environment;
            }
        }

        public int Rows
        {
            get
            {
                return _rows;
            }
            set
            {
                _rows = value;
                base.OnPropertyChanged("Rows");
            }
        }

        public IWebResourceViewModel RootWebResource
        {
            get
            {
                return _rootWebResource;
            }
        }

        public ObservableCollection<ILayoutObjectViewModel> LayoutObjects
        {
            get
            {
                return _layoutObjects;
            }
        }

        public ObservableCollection<IResourceModel> WebpageServices
        {
            get
            {
                ObservableCollection<IResourceModel> webpageServices;

                if(_webActivity != null && _webActivity.ResourceModel != null && _webActivity.ResourceModel.Environment != null)
                {
                    ObservableCollection<IResourceModel> itemList = _webActivity.ResourceModel.Environment.Resources.All()
                        .Where(c => c.Category.Equals("Human Interface Workflow", StringComparison.InvariantCultureIgnoreCase) || c.Category.Equals("Webpage", StringComparison.InvariantCultureIgnoreCase))
                        .ToObservableCollection();
                    ResourceModel noResourceModel = new ResourceModel(_webActivity.ResourceModel.Environment);
                    noResourceModel.ResourceName = "None";
                    itemList.Insert(0, noResourceModel);
                    return itemList;
                }
                else
                {
                    webpageServices = new ObservableCollection<IResourceModel>();
                }

                return webpageServices;
            }
        }

        public IResourceModel SelectedDefaultWebpage
        {
            get
            {
                if(_selectedDefaultWebpage == null)
                {
                    ResourceModel noResourceModel = new ResourceModel(_webActivity.ResourceModel.Environment);
                    noResourceModel.ResourceName = "None";
                    return noResourceModel;
                }
                return _selectedDefaultWebpage;
            }
            set
            {
                _selectedDefaultWebpage = value;
                base.OnPropertyChanged("SelectedDefaultWebpage");
                WriteToModelItemDataProperty("DEV2DefaultWebpage", _selectedDefaultWebpage.ResourceName);
                Deploy();
            }

        }

        private void WriteToModelItemDataProperty(string propertyName, string propertyValue)
        {

            string xmlConfig = _webActivity.XMLConfiguration;

            UnlimitedObject d = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(xmlConfig);
            d.GetElement(propertyName).SetValue(propertyValue);

            _webActivity.XMLConfiguration = d.XmlString;
        }

        private string ReadFromModelItemDataProperty(string propertyName)
        {
            dynamic data = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(_webActivity.XMLConfiguration);

            return data.GetValue(propertyName);
        }


        public string Url
        {
            get
            {
                Uri url = new Uri(StringResources.Uri_WebServer);

                if(_webActivity != null && _webActivity.ResourceModel != null && _webActivity.ResourceModel.Environment != null)
                {
                    if(!Uri.TryCreate(_webActivity.ResourceModel.Environment.WebServerAddress, "/services/" + ResourceModel.ResourceName, out url))
                    {
                        Uri.TryCreate(new Uri(StringResources.Uri_WebServer), "/services/" + ResourceModel.ResourceName, out url);
                    }
                }

                return url.AbsoluteUri;
            }
        }

        public string WizardUrl
        {
            get
            {
                Uri url = new Uri(StringResources.Uri_WebServer);

                if(_webActivity != null && _webActivity.ResourceModel != null && _webActivity.ResourceModel.Environment != null)
                {
                    if(!Uri.TryCreate(_webActivity.ResourceModel.Environment.WebServerAddress, "/services/Website_Wizard", out url))
                    {
                        Uri.TryCreate(new Uri(StringResources.Uri_WebServer), "/services/Website_Wizard", out url);
                    }
                }

                return url.AbsoluteUri;
            }
        }

        public string ResourceName
        {
            get
            {
                return ResourceModel.ResourceName;
            }
        }

        public string XMLConfiguration
        {
            get
            {
                return (_webActivity as dynamic).XMLConfiguration;
            }
            set
            {

                _webActivity.XMLConfiguration = value;
                base.OnPropertyChanged("XMLConfiguration");
                Navigate();
            }
        }

        public IWebActivity WebActivity
        {
            get
            {
                return _webActivity;
            }
        }

        public IResourceModel ResourceModel
        {
            get
            {
                return _resource;
            }
        }

        private ILayoutObjectViewModel _selectedLayoutObject;

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get { return _selectedLayoutObject; }
        }

        public string Html
        {
            get
            {
                return _html;
            }
            set
            {
                _html = value;


                base.OnPropertyChanged("Html");
                base.OnPropertyChanged("IsValidMarkup");

                UpdateModelItem();
                // deploy if it is valid HTML
                if(IsValidMarkup)
                {
                    Deploy();
                }
            }
        }

        private string _searchEngineKeywords;
        public string SearchEngineKeywords
        {
            get
            {
                return _searchEngineKeywords;
            }
            set
            {
                _searchEngineKeywords = value;
                base.OnPropertyChanged("SearchEngineKeywords");
            }
        }

        public bool CanRemove
        {
            get
            {
                if((SelectedWebResource == null) || SelectedWebResource.IsFolder)
                {
                    return false;
                }
                return true;
            }
        }
        #endregion

        #region Commands
        public ICommand AddExistingWebResourceCommand
        {
            get
            {
                if(_addExistingWebResourceCommand == null)
                {
                    _addExistingWebResourceCommand = new RelayCommand(c => AddExistingWebResource());
                }

                return _addExistingWebResourceCommand;
            }
        }

        public ICommand RemoveWebResourceCommand
        {
            get
            {

                if(_removedWebResourceCommand == null)
                {
                    _removedWebResourceCommand = new RelayCommand(c => RemoveWebResource(), c => CanRemove);
                }

                return _removedWebResourceCommand;
            }
        }


        public ICommand CopyCommand
        {
            get
            {
                if(_copyCommand == null)
                {
                    _copyCommand = new RelayCommand(c => Copy(), c => SelectedWebResource != null);
                }
                return _copyCommand;
            }
        }

        private void Copy()
        {
            if(SelectedWebResource != null)
            {
                Clipboard.SetText(SelectedWebResource.Name);
            }
        }

        public ICommand EditCommand
        {
            get
            {
                if(_editCommand == null)
                {
                    _editCommand = new RelayCommand(c => Configure());
                }
                return _editCommand;
            }
        }
        #endregion

        #region Public Methods

        public void Dev2Set(string value1, string value2)
        {
            throw new NotImplementedException();
            //if (_webActivity != null && _webActivity.ResourceModel != null && _webActivity.ResourceModel.Environment != null)
            //{
            //    string postUri = string.Format("{0}{1}", _webActivity.ResourceModel.Environment.WebServerAddress.AbsoluteUri, value2);

            //    IWebCommunicationResponse response = WebCommunication.Post(postUri, value1);

            //    if (response != null)
            //    {
            //        switch (response.ContentType)
            //        {
            //            case "text/html":

            //                //string html = response.Content;

            //                //html = ResourceHelper.PropertyEditorHtmlInject(html, MainViewModel.CurrentWebServer);

            //                //if (NavigateRequested != null) {
            //                //    NavigateRequested(html);
            //                //}

            //                if (NavigateRequested != null)
            //                {
            //                    NavigateRequested(postUri);
            //                }
            //                break;

            //            case "text/xml":

            //                UnlimitedObject xmlResult = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(response.Content);

            //                if (xmlResult.HasError)
            //                {
            //                    Close();
            //                }
            //                else
            //                {
            //                    xmlResult.RemoveElementsByTagName("InstanceId");
            //                    xmlResult.RemoveElementsByTagName("Bookmark");
            //                    xmlResult.RemoveElementsByTagName("ParentWorkflowInstanceId");
            //                    xmlResult.RemoveElementsByTagName("ParentServiceName");
            //                    xmlResult.RemoveElementsByTagName("FormView");
            //                    xmlResult.RemoveElementsByTagName("JSON");
            //                }
            //                SetConfigFragment(SelectedLayoutObject.WebpartServiceDisplayName, xmlResult);
            //                Close();

            //                break;

            //            default:
            //                break;
            //        }
            //    }

            //    Deploy();
            //    Navigate();
            //}
        }


        public void Dev2SetValue(string value)
        {
            throw new NotImplementedException();
        }

        public void Dev2Done()
        {
            throw new NotImplementedException();
        }

        public void Dev2ReloadResource(string resourceName, string resourceType)
        {
            throw new NotImplementedException();
        }

        public string CheckForDuplicateNames(string xmlConfig, string testName)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xmlConfig);
            bool duplicateExists = false;
            foreach(XmlNode xnode in xdoc.SelectNodes("//Dev2elementName"))
            {
                if(xnode.InnerText == testName)
                {
                    duplicateExists = true;
                }
            }
            if(duplicateExists == true)
            {
                return "True,This name already exists";
            }
            return string.Empty;
        }

        public void Deploy()
        {
            if(_webActivity != null && _webActivity.ResourceModel != null && _webActivity.ResourceModel.Environment != null)
            {
                //Flush the workflow designer so changes are written back into the Resource Model
                //so that we can save the workflow at its latest state
                Mediator.SendMessage(MediatorMessages.BindViewToViewModel, _resource);
                Mediator.SendMessage(MediatorMessages.SaveResourceModel, _resource);

                //Create the necessary folder structure on the web server if it does not already exist.
                dynamic package = new UnlimitedObject();
                package.Service = StringResources.Website_BootStrap_Service;
                package.Dev2WebsiteName = _resource.ResourceName;
                var workspaceID = ((IStudioClientContext)_webActivity.ResourceModel.Environment.DsfChannel).AccountID;

                var result = _webActivity.ResourceModel.Environment.DsfChannel.ExecuteCommand(package.XmlString, workspaceID, GlobalConstants.NullDataListID);
                if (result == null)
                {
                    throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, StringResources.Website_BootStrap_Service));
                }
                
            }
        }

        public void Close()
        {
            Mediator.SendMessage(MediatorMessages.CloseWizard, this);
        }

        public void Cancel()
        {
            Close();
        }

        public void Navigate()
        {
            if(_webActivity != null && _webActivity.ResourceModel != null && _webActivity.ResourceModel.Environment != null)
            {
                Deploy();

                Uri url;
                if(!Uri.TryCreate(_webActivity.ResourceModel.Environment.WebServerAddress, "/services/" + ResourceModel.ResourceName + "?DEV2WebsiteEditingMode=true", out url))
                {
                    Uri.TryCreate(new Uri(StringResources.Uri_WebServer), "/services/" + ResourceModel.ResourceName + "?DEV2WebsiteEditingMode=true", out url);
                }

                //(Browser as dynamic).Navigate(uri);

                var browser = Browser as WebView;
                if(browser != null)
                {
                    browser.LoadSafe(url.ToString());
                }
            }
        }

        public void SetSelected(ILayoutObjectViewModel obj)
        {

            _layoutObjects.ToList().ForEach(c => c.IsSelected = false);
            obj.IsSelected = true;

            _selectedLayoutObject = obj;
        }

        public void UpdateModelItem()
        {
            if(!string.IsNullOrEmpty(Html) && _webActivity != null && _webActivity.ResourceModel != null && _webActivity.ResourceModel.Environment != null)
            {
                int rows = 0;

                UnlimitedObject tags = new UnlimitedObject();

                try
                {
                    tags = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(Html.ToLower());
                }
                catch { IsValidMarkup = false; return; }

                if(tags.HasError)
                {
                    IsValidMarkup = false;
                    return;
                }
                else
                {
                    IsValidMarkup = true;
                }

                var tagItems = tags.GetAllElements("dev2html");

                bool copyToPrevious = false;

                if(lastGood == null)
                {
                    copyToPrevious = true;
                }
                else
                {

                    previous = new ILayoutObjectViewModel[lastGood.Length];
                    lastGood.CopyTo(previous, 0);
                }

                _layoutObjects.Clear();

                foreach(dynamic tag in tagItems)
                {

                    IResourceModel res = null;

                    string name = null;
                    string type = null;
                    string iconpath = null;

                    if(tag.name is string)
                    {
                        name = tag.name;
                    }

                    if(tag.type is string)
                    {
                        type = tag.type;
                        res = _webActivity.ResourceModel.Environment.Resources.All().FirstOrDefault(c => c.ResourceName.Equals(type, StringComparison.InvariantCultureIgnoreCase));
                        if(res != null)
                        {
                            iconpath = res.IconPath;
                        }
                    }

                    var exclusions = new List<string>() { "form", "meta", "pagetitle" };
                    if(!exclusions.Contains(type.ToLower()))
                    {

                        if(string.IsNullOrEmpty(name))
                        {
                            IsValidMarkup = false;
                            return;
                        }

                        if(string.IsNullOrEmpty(type))
                        {
                            IsValidMarkup = false;
                            return;
                        }

                        if(string.IsNullOrEmpty(StringResources.Website_Supported_Webparts))
                        {
                            throw new Exception(StringResources.Error_Supported_Webparts_Not_Set);
                        }

                        List<string> inclusions = StringResources.Website_Supported_Webparts.Split(new char[] { ',' }).ToList();
                        if(!inclusions.Contains(type.ToLower()))
                        {
                            IsValidMarkup = false;
                            return;
                        }

                        var newobject = LayoutObjectViewModelFactory.CreateLayoutObject(type, name, iconpath, GetConfigFragment(name));

                        //Check if another webpart with the same name - not type - exists
                        //If so then we want to mark the website html as invalid
                        //as we don't allow multiple parts with the same name
                        var match = _layoutObjects.Any(c => c.WebpartServiceDisplayName.Equals(newobject.WebpartServiceDisplayName, StringComparison.InvariantCultureIgnoreCase));

                        if(match)
                        {
                            IsValidMarkup = false;
                            return;
                        }

                        IsValidMarkup = true;
                        _layoutObjects.Add(newobject);
                        _webActivity.Html = Html;
                        rows++;
                    }
                }

                if(_layoutObjects.Count > 0)
                {
                    lastGood = new ILayoutObjectViewModel[_layoutObjects.Count];
                    _layoutObjects.CopyTo(lastGood, 0);
                }

                //Initial load only
                if(copyToPrevious)
                {
                    if(lastGood != null)
                    {
                        previous = new ILayoutObjectViewModel[lastGood.Length];
                        lastGood.CopyTo(previous, 0);
                    }
                    copyToPrevious = false;
                }
                if(lastGood != null)
                {
                    CompareAndRemoveDeleted(lastGood.ToList().ToObservableCollection(), previous.ToObservableCollection());
                }
                // Update the HTML markup
                if(IsValidMarkup)
                {
                    _webActivity.Html = Html;

                }
                Rows = rows;

            }
        }

        private void CompareAndRemoveDeleted(IEnumerable<ILayoutObjectViewModel> layoutObjects, IEnumerable<ILayoutObjectViewModel> _previous)
        {
            var result = _previous.Except<ILayoutObjectViewModel>(layoutObjects, new WebsiteLayoutObjectViewModelEqualityComparer());
            foreach(var itemToDelete in result)
            {
                SetConfigFragment(itemToDelete.WebpartServiceDisplayName, new UnlimitedObject("Empty"));

            }
        }

        public void Save(string value, bool closeBrowserWindow = true)
        {
            }

        public void NavigateTo(string uri, string args, string returnUri)
        {

        }

        public void OpenPropertyEditor()
        {
            Mediator.SendMessage(MediatorMessages.ShowWebpartWizard, this);
        }

        public void Update()
        {
            base.OnPropertyChanged("LayoutObjects");
        }

        #endregion

        #region Private Methods

        private void SetDefaultWebpage()
        {
            if(_webActivity != null && _webActivity.ResourceModel != null && _webActivity.ResourceModel.Environment != null)
            {
                _html = (_webActivity as dynamic).Html;

                dynamic defaultWebPage = ReadFromModelItemDataProperty("DEV2DefaultWebpage");

                var webpageService = _webActivity.ResourceModel.Environment.Resources.All().FirstOrDefault(c => c.ResourceName.Equals(defaultWebPage, StringComparison.InvariantCultureIgnoreCase));

                if(webpageService != null)
                {
                    SelectedDefaultWebpage = webpageService;
                }

                MetaTags = ReadFromModelItemDataProperty("DEV2MetaTags");
            }
        }

        private void InitializeWebResources()
        {
            _webResources = WebResourceRepositoryFactory.CreateWebResourceRepository(_resource);
            _webResources.Load();

            if(_webResources == null)
            {
                throw new InvalidOperationException("WebResourceFactory returned null repository");
            }

            _rootWebResource = new WebResourceViewModel(null) { Name = "root", IsRoot = true };
            _webResources.All().ToList().ForEach(c => _rootWebResource.Children.Add(c));
        }

        private void Configure()
        {
            if(SelectedLayoutObject != null)
            {
                OpenPropertyEditor();
            }

        }

        private string GetConfigFragment(string webPartElementName)
        {
            string fragment = string.Empty;
            string configuration = WebActivity.XMLConfiguration;

            string tmpConfig = configuration.ToLower();

            int startIndex = tmpConfig.IndexOf("<" + webPartElementName + ">") + webPartElementName.Length + 2;
            int endIndex = tmpConfig.IndexOf(@"</" + webPartElementName + ">");

            if(startIndex > webPartElementName.Length && endIndex > 0)
            {
                fragment = configuration.Substring(startIndex, endIndex - startIndex);
            }

            return fragment;
        }

        private void SetConfigFragment(string webPartElementName, dynamic xmlConfiguration)
        {
            string configuration = WebActivity.XMLConfiguration;
            UnlimitedObject configData = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(configuration);

            if(configData.ElementExists(webPartElementName.ToLower()))
            {
                configData.RemoveElementsByTagName(webPartElementName.ToLower());
            }

            configData.CreateElement(webPartElementName.ToLower()).Add(xmlConfiguration);

            //Write xml configuration to the layout cell
            dynamic layoutXml = new UnlimitedObject(webPartElementName.ToLower());
            layoutXml.Add(xmlConfiguration);
            SelectedLayoutObject.XmlConfiguration = layoutXml.XmlString;


            _webActivity.XMLConfiguration = configData.XmlString;
            Deploy();
        }


        private void RemoveWebResource()
        {
            if(SelectedWebResource != null)
            {
                try
                {
                    var parent = SelectedWebResource.Parent;
                    _webResources.Remove(SelectedWebResource);
                    SelectedWebResource = parent;
                    base.OnPropertyChanged("RootWebResource");
                }
                catch(WebResourceRemoveFailedException)
                {

                }
            }
        }

        private void AddExistingWebResource()
        {
            if((SelectedWebResource != null) && !string.IsNullOrEmpty(SelectedWebResource.Uri))
            {
                dynamic fileData = FileNameProvider.GetFileName();

                if(!fileData.HasError)
                {
                    string localPath = fileData.LocalFilePath;
                    string fileName = fileData.FileName;
                    if(!string.IsNullOrEmpty(localPath))
                    {
                        var newWebResource = new WebResourceViewModel(null);
                        newWebResource.Name = fileName;
                        newWebResource.Base64Data = Convert.ToBase64String(File.ReadAllBytes(localPath));
                        newWebResource.Uri = SelectedWebResource.Uri;

                        newWebResource.SetParent(SelectedWebResource.Parent ?? SelectedWebResource);

                        string fullName = string.Format("/{0}/{1}", newWebResource.Uri, newWebResource.Name);
                        var existingWebResource = newWebResource.Parent.Children.FirstOrDefault(c => c.Name.Equals(fullName, StringComparison.InvariantCultureIgnoreCase));
                        if(existingWebResource != null)
                        {
                            UserMessageProvider.ShowUserErrorMessage(StringResources.Error_Website_Resource_Exists);
                            return;

                        }

                        try
                        {
                            _webResources.Save(newWebResource);

                            newWebResource.IsSelected = true;
                            newWebResource.Parent.IsExpanded = true;

                            SelectedWebResource = newWebResource;

                            base.OnPropertyChanged("RootWebResource");
                        }
                        catch(WebResourceUploadFailedException)
                        {

                        }
                    }
                }
            }
        }
        #endregion
    }

}
