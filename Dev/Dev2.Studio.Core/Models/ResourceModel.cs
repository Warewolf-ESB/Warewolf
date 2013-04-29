using Caliburn.Micro;
using Dev2.Common.ExtMethods;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Dev2.Studio.Core.Models
{
    public class ResourceModel : ValidationController, IDataErrorInfo, IContextualResourceModel
    {
        #region Class Members

        private readonly List<string> _tagList;
        private bool _allowCategoryEditing = true;
        private string _authorRoles;
        private string _category;
        private string _comment;
        private string _dataList;
        private string _dataTags;
        private string _displayName = string.Empty;
        private IEnvironmentModel _environment;
        private string _helpLink;
        private string _iconPath;
        private bool _isDatabaseService;
        private bool _isDebugMode;
        private bool _isResourceService;
        private bool _requiresSignOff;
        private string _resourceName;
        private ResourceType _resourceType;
        private string _serviceDefinition;
        private string _tags;
        private string _unitTestTargetWorkflowService;
        private string _workflowXaml;
        private Version _version;
        private bool _isNewWorkflow = false;

        #endregion Class Members

        #region Constructors

        //public ResourceModel()
        //{
        //    _tagList = new List<string>();
        //}

        public ResourceModel(IEnvironmentModel environment)
        {
            ImportService.SatisfyImports(this);

            _tagList = new List<string>();
            Environment = environment;

            if (environment != null && environment.DataListChannel != null)
                ServerID = environment.DataListChannel.ServerID;
        }

        #endregion Constructors

        #region Properties

        [Import]
        public IEventAggregator EventAggregator { get; set; }

        public IEnvironmentModel Environment
        {
            get { return _environment; }
            private set
            {
                _environment = value;
                NotifyOfPropertyChange("Environment");
                NotifyOfPropertyChange("CanExecute");
            }
        }

        public Guid ServerID { get; set; }

        public bool IsDatabaseService
        {
            get { return _isDatabaseService; }
            set
            {
                _isDatabaseService = value;
                NotifyOfPropertyChange("IsDatabaseService");
            }
        }

        public bool IsResourceService
        {
            get { return _isResourceService; }
            set
            {
                _isResourceService = value;
                NotifyOfPropertyChange("IsResourceService");
            }
        }

        public Guid ID { get; set; }

        public Version Version
        {
            get { return _version; }
            set
            {
                _version = value;
                NotifyOfPropertyChange("Version");
            }
        }

        public bool AllowCategoryEditing
        {
            get { return _allowCategoryEditing; }
            set
            {
                _allowCategoryEditing = value;
                NotifyOfPropertyChange("AllowCategoryEditing");
            }
        }

        public Activity WorkflowActivity
        {
            get
            {
                if (!string.IsNullOrEmpty(ServiceDefinition))
                {
                    byte[] xamlStream = Encoding.UTF8.GetBytes(ServiceDefinition);
                    return ActivityXamlServices.Load(new MemoryStream(xamlStream));
                }
                return null;
            }
        }

        [Required(ErrorMessage = "Please enter a name for this resource")]
        public string ResourceName
        {
            get { return _resourceName; }
            set
            {
                _resourceName = value.Trim();
                NotifyOfPropertyChange("ResourceName");
            }
        }

        public override string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                {
                    if (ResourceType == ResourceType.WorkflowService)
                    {
                        _displayName = "Workflow";
                    }
                    else
                    {
                        _displayName = ResourceType.ToString();
                    }
                }

                return _displayName;
            }
            set
            {
                _displayName = value;
                NotifyOfPropertyChange("DisplayName");
            }
        }

        public string IconPath
        {
            get { return _iconPath; }
            set
            {
                _iconPath = value;
                NotifyOfPropertyChange("IconPath");
            }
        }

        public string UnitTestTargetWorkflowService
        {
            get { return _unitTestTargetWorkflowService; }
            set
            {
                _unitTestTargetWorkflowService = value;
                NotifyOfPropertyChange("UnitTestTargetWorkflowService");
            }
        }

        public List<string> TagList
        {
            get { return _tagList; }
        }

        public string DataList
        {
            get { return _dataList; }
            set
            {
                _dataList = value;
                NotifyOfPropertyChange("DataList");
            }
        }

        public ResourceType ResourceType
        {
            get { return _resourceType; }
            set
            {
                _resourceType = value;
                NotifyOfPropertyChange("ResourceType");
            }
        }

        [Required(ErrorMessage = "Please enter a Category for this resource")]
        public string Category
        {
            get { return _category; }
            set
            {
                _category = value;
                NotifyOfPropertyChange("Category");
            }
        }

        public string Tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                NotifyOfPropertyChange("Tags");
            }
        }

        [Required(ErrorMessage = "Please enter the Comment for this resource")]
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                NotifyOfPropertyChange("Comment");
            }
        }

        public virtual string ServiceDefinition
        {
            get { return _serviceDefinition; }
            set
            {
                //if(!string.IsNullOrEmpty(value)){
                //    try {
                //        dynamic data = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(value);

                //        if (data != null) {
                //            if (data.Comment is string) {
                //                this.Comment = data.Comment;
                //            }

                //            if (data.Category is string) {

                //                this.Category = data.Category;
                //            }

                //            if (data.Tags is string) {
                //                this.Tags = data.Tags;
                //            }

                //            if (data.HelpLink is string) {
                //                if (!string.IsNullOrEmpty(data.HelpLink)) {
                //                    this.HelpLink = data.HelpLink;
                //                }
                //            }
                //        }
                //    }
                //    catch {}
                //}

                _serviceDefinition = value;
                NotifyOfPropertyChange("ServiceDefinition");
                NotifyOfPropertyChange("WorkflowActivity");
            }
        }

        public string WorkflowXaml
        {
            get { return _workflowXaml; }
            set
            {
                _workflowXaml = value;
                NotifyOfPropertyChange("WorkflowXaml");
            }
        }

        public bool RequiresSignOff
        {
            get { return _requiresSignOff; }
            set
            {
                _requiresSignOff = value;
                NotifyOfPropertyChange("RequiresSignOff");
            }
        }

        public bool HasErrors
        {
            get { return validationErrors.Count > 0; }
        }

        public string DataTags
        {
            get { return _dataTags; }
            set
            {
                _dataTags = value;
                NotifyOfPropertyChange("DataTags");
            }
        }

        [Required(ErrorMessage = "Please enter a valid help link")]
        public string HelpLink
        {
            get { return _helpLink; }
            set
            {
                _helpLink = value;
                NotifyOfPropertyChange("HelpLink");
            }
        }

        public bool IsDebugMode
        {
            get { return _isDebugMode; }
            set
            {
                _isDebugMode = value;
                NotifyOfPropertyChange("IsDebugMode");
            }
        }

        [Required(ErrorMessage = "Please select the roles that are allowed to author this workflow")]
        public string AuthorRoles
        {
            get { return _authorRoles; }
            set
            {
                _authorRoles = value;
                NotifyOfPropertyChange("AuthorRoles");
            }
        }

        public bool IsNewWorkflow
        {
            get
            {
                return _isNewWorkflow;
            }
            set
            {
                _isNewWorkflow = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///     Updates the non workflow related details of from another resource model.
        /// </summary>
        /// <param name="resourceModel">The resource model to update from.</param>
        public void Update(IResourceModel resourceModel)
        {
            AllowCategoryEditing = resourceModel.AllowCategoryEditing;
            AuthorRoles = resourceModel.AuthorRoles;
            Category = resourceModel.Category;
            Comment = resourceModel.Comment;
            DataTags = resourceModel.DataTags;
            DisplayName = resourceModel.DisplayName;
            HelpLink = resourceModel.HelpLink;
            IsDebugMode = resourceModel.IsDebugMode;
            RequiresSignOff = resourceModel.RequiresSignOff;
            ResourceName = resourceModel.ResourceName;
            ResourceType = resourceModel.ResourceType;
            Tags = resourceModel.Tags;
            ServiceDefinition = resourceModel.ServiceDefinition;
            WorkflowXaml = resourceModel.WorkflowXaml;
            //UnitTestTargetWorkflowService = resourceModel.UnitTestTargetWorkflowService;
            DataList = resourceModel.DataList;
            UpdateIconPath(resourceModel.IconPath);
            Version = resourceModel.Version;

            EventAggregator.Publish(new UpdateResourceDesignerMessage(this));
        }

        public void UpdateIconPath(string iconPath)
        {
            IconPath = string.IsNullOrEmpty(iconPath) ? ResourceType.GetIconLocation() : iconPath;
        }

        public string ToServiceDefinition()
        {
            //TODO this method replicates functionality that is available in the server. There is a serious need to create a common library for resource contracts and resource serialization.
            string result;

            if (ResourceType == ResourceType.WorkflowService)
            {
                XElement dataList = string.IsNullOrEmpty(DataList) ? new XElement("DataList") : XElement.Parse(DataList);

                XElement service = new XElement("Service",
                    new XAttribute("ID", ID),
                    new XAttribute("Version", (Version != null) ? Version.ToString() : "1.0"),
                    new XAttribute("ServerID", ServerID.ToString()),
                    new XAttribute("Name", ResourceName ?? string.Empty),
                    new XAttribute("ResourceType", ResourceType),                    
                    new XElement("DisplayName", ResourceName ?? string.Empty),
                    new XElement("Category", Category ?? string.Empty),
                    new XElement("IsNewWorkflow", IsNewWorkflow),
                    new XElement("AuthorRoles", AuthorRoles ?? string.Empty),
                    new XElement("Comment", Comment ?? string.Empty),
                    new XElement("Tags", Tags ?? string.Empty),
                    new XElement("IconPath", IconPath ?? string.Empty),
                    new XElement("HelpLink", HelpLink ?? string.Empty),
                    new XElement("UnitTestTargetWorkflowService", UnitTestTargetWorkflowService ?? string.Empty),
                    dataList,
                    new XElement("Action",
                        new XAttribute("Name", "InvokeWorkflow"),
                        new XAttribute("Type", "Workflow"),
                        new XElement("XamlDefinition", WorkflowXaml ?? string.Empty))
                    );

                result = service.ToString();
            }
            else
            {
                throw new Exception("ToServiceDefinition doesn't support resources of type source. Sources are meant to be managed through the Web API.");
            }

            return result;
        }

        /// <summary>
        ///     This method will check if there has been any change on the workflow that havnt been saved
        /// </summary>
        /// <param name="viewModelServiceDef">The service definition of the view model of the workflow that need to be checked</param>
        /// <returns>Boolean stating if the workflow has been saved</returns>
        public bool IsWorkflowSaved(string viewModelServiceDef)
        {
            bool _isWorkflowSaved = false;
            string current = viewModelServiceDef;

            // Sanity check ;)
            if (current == null || WorkflowXaml == null)
            {
                throw new InvalidOperationException("Null Workflow Data");
            }

            XElement comp1 = XElement.Parse(current);
            XElement comp2 = XElement.Parse(WorkflowXaml);

            if (XNode.DeepEquals(comp1, comp2))
            {
                _isWorkflowSaved = true;
            }
            else
            {
                _isWorkflowSaved = false;
            }
            return _isWorkflowSaved;
        }

        #endregion Methods

        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                PropertyInfo prop = GetType().GetProperty(columnName);
                IEnumerable<ValidationAttribute> validationMap =
                    prop.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
                string errMsg = null;


                foreach (ValidationAttribute v in validationMap)
                {
                    try
                    {
                        v.Validate(prop.GetValue(this, null), columnName);
                        RemoveError(columnName);
                    }
                    catch (Exception)
                    {
                        AddError(columnName, v.ErrorMessage);

                        return v.ErrorMessage;
                    }
                }

                if (columnName == "ResourceName")
                {
                    if (string.IsNullOrEmpty(ResourceName))
                    {
                        errMsg = "Resource Name must be entered";
                        AddError("NoResourceName", errMsg);
                        return errMsg;
                    }
                    else
                    {
                        RemoveError("NoResourceName");
                    }
                }

                if (columnName == "IconPath")
                {
                    Uri testUri = null;
                    if (!Uri.TryCreate(IconPath, UriKind.Absolute, out testUri) && !string.IsNullOrEmpty(IconPath))
                    {
                        errMsg = "Icon Path Does Not Exist or is not valid";
                        AddError("IconPathFileDoesNotExist", errMsg);
                        return errMsg;
                    }
                    else
                    {
                        RemoveError("IconPathFileDoesNotExist");
                    }
                }

                if (columnName == "HelpLink")
                {
                    Uri testUri = null;
                    if (!Uri.TryCreate(HelpLink, UriKind.Absolute, out testUri))
                    {
                        errMsg = "The help link is not in a valid format";
                        AddError(columnName, errMsg);
                        return errMsg;
                    }
                }

                return null;
            }
        }

        #endregion
    }
}