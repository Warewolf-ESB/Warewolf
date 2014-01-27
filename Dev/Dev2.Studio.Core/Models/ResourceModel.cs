using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Collections;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Action = System.Action;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Models
{
    public class ResourceModel : ValidationController, IDataErrorInfo, IContextualResourceModel
    {
        #region Class Members

        private readonly List<string> _tagList;
        private bool _allowCategoryEditing = true;
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
        private string _tags;
        private string _unitTestTargetWorkflowService;
        private StringBuilder _workflowXaml;
        private Version _version;
        bool _isPluginService;
        bool _isWorkflowSaved;
        Guid _id;

        IDesignValidationService _validationService;
        IPermissionsModifiedService _permissionsModifiedService;

        readonly ObservableReadOnlyList<IErrorInfo> _errors = new ObservableReadOnlyList<IErrorInfo>();
        readonly ObservableReadOnlyList<IErrorInfo> _fixedErrors = new ObservableReadOnlyList<IErrorInfo>();
        bool _isValid;
        Permissions _userPermissions;

        #endregion Class Members

        #region Constructors

        public ResourceModel(IEnvironmentModel environment)
            : this(environment, EventPublishers.Aggregator)
        {
        }

        public ResourceModel(IEnvironmentModel environment, IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);

            _tagList = new List<string>();
            Environment = environment;

            if(environment != null && environment.Connection != null)
            {
                ServerID = environment.Connection.ServerID;
            }
            IsWorkflowSaved = true;
        }

        #endregion Constructors

        #region Properties

        public string Inputs { get; set; }

        public string Outputs { get; set; }

        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
                NotifyOfPropertyChange("IsValid");
            }
        }

        public IObservableReadOnlyList<IErrorInfo> Errors { get { return _errors; } }
        public IObservableReadOnlyList<IErrorInfo> FixedErrors { get { return _fixedErrors; } }

        public bool IsWorkflowSaved
        {
            get
            {
                return _isWorkflowSaved;
            }
            set
            {
                _isWorkflowSaved = value;
                if(OnResourceSaved != null)
                {
                    OnResourceSaved(this);
                }
            }
        }

        public IEnvironmentModel Environment
        {
            get { return _environment; }
            private set
            {
                _environment = value;
                if(value != null && _environment.Connection != null)
                {
                    _validationService = new DesignValidationService(_environment.Connection.ServerEvents);

                    // BUG 9634 - 2013.07.17 - TWR : added
                    _validationService.Subscribe(_environment.ID, ReceiveEnvironmentValidation);

                    _permissionsModifiedService = new PermissionsModifiedService(_environment.Connection.ServerEvents);

                    // MUST subscribe to Guid.Empty as memo.InstanceID is NOT set by server!
                    _permissionsModifiedService.Subscribe(Guid.Empty, ReceivePermissionsModified);
                }
                NotifyOfPropertyChange("Environment");
                // ReSharper disable NotResolvedInText
                NotifyOfPropertyChange("CanExecute");
                // ReSharper restore NotResolvedInText
            }
        }

        public event EventHandler<PermissionsModifiedMemo> OnPermissionsModifiedReceived;

        void ReceivePermissionsModified(PermissionsModifiedMemo memo)
        {
            var modifiedPermissions = memo.ModifiedPermissions.Where(p => p.ResourceID == ID).ToList();
            if(modifiedPermissions.Count > 0)
            {
                UserPermissions = Environment.AuthorizationService.GetResourcePermissions(ID);
            }

            if(OnPermissionsModifiedReceived != null)
            {
                OnPermissionsModifiedReceived(this, memo);
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

        public bool IsPluginService
        {
            get { return _isPluginService; }
            set
            {
                _isPluginService = value;
                NotifyOfPropertyChange("IsPluginService");
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

        public Guid ID
        {
            get { return _id; }
            set
            {
                _id = value;
                if(_validationService != null)
                {
                    _validationService.Subscribe(_id, ReceiveDesignValidation);
                }
            }
        }

        public Permissions UserPermissions
        {
            get { return _userPermissions; }
            set
            {
                if(value == _userPermissions)
                {
                    return;
                }
                _userPermissions = value;
                NotifyOfPropertyChange(() => UserPermissions);
            }
        }

        public bool IsAuthorized(AuthorizationContext authorizationContext)
        {
            return (UserPermissions & authorizationContext.ToPermissions()) != 0;
        }

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

        [Required(ErrorMessage = @"Please enter a name for this resource")]
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
                if(string.IsNullOrEmpty(_displayName))
                {
                    _displayName = ResourceType == ResourceType.WorkflowService ? "Workflow" : ResourceType.ToString();
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
                if(value == _dataList)
                {
                    return;
                }
                _dataList = value;
                NotifyOfPropertyChange("DataList");
                if(OnDataListChanged != null)
                {
                    OnDataListChanged();
                }

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

        [Required(ErrorMessage = @"Please enter a Category for this resource")]
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

        [Required(ErrorMessage = @"Please enter the Comment for this resource")]
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                NotifyOfPropertyChange("Comment");
            }
        }

        // Problems ;)
        public StringBuilder WorkflowXaml
        {
            get
            {
                return _workflowXaml;
            }
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
            get { return ValidationErrors.Count > 0; }
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

        [Required(ErrorMessage = @"Please enter a valid help link")]
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

        public bool IsNewWorkflow { get; set; }

        public string ServerResourceType { get; set; }
        public event Action<IContextualResourceModel> OnResourceSaved;
        public event Action OnDataListChanged;

        #endregion Properties

        #region Methods

        public event EventHandler<DesignValidationMemo> OnDesignValidationReceived;

        void ReceiveDesignValidation(DesignValidationMemo memo)
        {
            if(memo.Errors.Any(info => info.InstanceID != Guid.Empty))
            {
                IsValid = memo.IsValid && _errors.Count == 0;
                if(memo.Errors.Count > 0)
                {
                    foreach(var error in Errors.Where(error => !memo.Errors.Contains(error)))
                    {
                        _fixedErrors.Add(error);
                    }
                    if(_errors.Count > 0)
                    {
                        _errors.Clear();
                    }
                    foreach(var error in memo.Errors)
                    {
                        _errors.Add(error);
                    }
                }
            }
            if(OnDesignValidationReceived != null)
            {
                OnDesignValidationReceived(this, memo);
            }
        }

        public event EventHandler<DesignValidationMemo> OnEnvironmentValidationReceived;

        // BUG 9634 - 2013.07.17 - TWR : added
        void ReceiveEnvironmentValidation(DesignValidationMemo memo)
        {
            foreach(var error in memo.Errors)
            {
                _errors.Add(error);
            }
            if(OnEnvironmentValidationReceived != null)
            {
                OnEnvironmentValidationReceived(this, memo);
            }
        }

        public IList<IErrorInfo> GetErrors(Guid instanceID)
        {
            return _errors.Where(e => e.InstanceID == instanceID).ToList();
        }

        public void AddError(IErrorInfo error)
        {
            _errors.Add(error);
        }

        public void RemoveError(IErrorInfo error)
        {
            var theError = Errors.FirstOrDefault(info => info.Equals(error)) ?? Errors.FirstOrDefault(info => info.ErrorType == error.ErrorType && info.FixType == error.FixType);
            if(theError != null)
            {
                _fixedErrors.Add(theError);
                _errors.Remove(theError);
            }
        }

        public void Commit()
        {
            _fixedErrors.Clear();
        }

        public void Rollback()
        {
            foreach(var fixedError in _fixedErrors)
            {
                _errors.Add(fixedError);
            }
            _fixedErrors.Clear();
        }

        /// <summary>
        ///     Updates the non workflow related details of from another resource model.
        /// </summary>
        /// <param name="resourceModel">The resource model to update from.</param>
        public void Update(IResourceModel resourceModel)
        {
            AllowCategoryEditing = resourceModel.AllowCategoryEditing;
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
            WorkflowXaml = resourceModel.WorkflowXaml;
            DataList = resourceModel.DataList;
            UpdateIconPath(resourceModel.IconPath);
            Version = resourceModel.Version;
            ConnectionString = resourceModel.ConnectionString;
            ID = resourceModel.ID;
            ServerResourceType = resourceModel.ServerResourceType;
            UserPermissions = resourceModel.UserPermissions;

            _errors.Clear();
            if(resourceModel.Errors != null)
            {
                foreach(var error in resourceModel.Errors)
                {
                    _errors.Add(error);
                }
            }
        }

        public string ConnectionString { get; set; }

        public void UpdateIconPath(string iconPath)
        {
            IconPath = string.IsNullOrEmpty(iconPath) ? ResourceType.GetIconLocation() : iconPath;
        }

        public StringBuilder ToServiceDefinition()
        {
            //TODO this method replicates functionality that is available in the server. There is a serious need to create a common library for resource contracts and resource serialization.
            StringBuilder result = new StringBuilder();

            if(ResourceType == ResourceType.WorkflowService)
            {


                var xaml = WorkflowXaml;

                if(xaml == null || xaml.Length == 0)
                {
                    var msg = Environment.ResourceRepository.FetchResourceDefinition(Environment, GlobalConstants.ServerWorkspaceID, ID);
                    xaml = msg.Message;
                }

                var service = CreateWorkflowXElement(xaml);

                // save to the string builder ;)

                XmlWriterSettings xws = new XmlWriterSettings { OmitXmlDeclaration = true };
                using(XmlWriter xwriter = XmlWriter.Create(result, xws))
                {
                    service.Save(xwriter);
                }
            }
            else if(ResourceType == ResourceType.Source || ResourceType == ResourceType.Service)
            {
                result = WorkflowXaml;

                // when null fetch the XAML ;)
                if(result == null)
                {
                    var msg = Environment.ResourceRepository.FetchResourceDefinition(Environment, GlobalConstants.ServerWorkspaceID, ID);
                    var actionDefintion = msg.Message;
                    if(ResourceType == ResourceType.Source)
                    {
                        result = actionDefintion;
                    }
                    else
                    {
                        var completeDefintion = CreateServiceXElement(actionDefintion);
                        result = completeDefintion.ToStringBuilder();
                    }
                }

                //2013.07.05: Ashley Lewis for bug 9487 - category may have changed!
                var startNode = result.IndexOf("<Category>", 0, true) + "<Category>".Length;
                var endNode = result.IndexOf("</Category>", 0, true);
                if(endNode > startNode)
                {
                    var len = (endNode - startNode);
                    var oldCategory = result.Substring(startNode, len);
                    if(oldCategory != Category)
                    {
                        result = result.Replace(oldCategory, Category);
                    }
                }
            }
            else
            {
                throw new Exception("ToServiceDefinition doesn't support resources of type source. Sources are meant to be managed through the Web API.");
            }

            return result;
        }

        XElement CreateWorkflowXElement(StringBuilder xaml)
        {
            XElement dataList = string.IsNullOrEmpty(DataList) ? new XElement("DataList") : XElement.Parse(DataList);
            XElement service = new XElement("Service",
                new XAttribute("ID", ID),
                new XAttribute("Version", (Version != null) ? Version.ToString() : "1.0"),
                new XAttribute("ServerID", ServerID.ToString()),
                new XAttribute("Name", ResourceName ?? string.Empty),
                new XAttribute("ResourceType", ResourceType),
                new XAttribute("IsValid", IsValid),
                new XElement("DisplayName", ResourceName ?? string.Empty),
                new XElement("Category", Category ?? string.Empty),
                new XElement("IsNewWorkflow", IsNewWorkflow),
                new XElement("AuthorRoles", string.Empty),
                new XElement("Comment", Comment ?? string.Empty),
                new XElement("Tags", Tags ?? string.Empty),
                new XElement("IconPath", IconPath ?? string.Empty),
                new XElement("HelpLink", HelpLink ?? string.Empty),
                new XElement("UnitTestTargetWorkflowService", UnitTestTargetWorkflowService ?? string.Empty),
                dataList,
                new XElement("Action",
                    new XAttribute("Name", "InvokeWorkflow"),
                    new XAttribute("Type", "Workflow"),
                    new XElement("XamlDefinition", xaml)),
                new XElement("ErrorMessages", WriteErrors())
                );
            return service;
        }

        XElement CreateServiceXElement(StringBuilder xaml)
        {
            XElement dataList = string.IsNullOrEmpty(DataList) ? new XElement("DataList") : XElement.Parse(DataList);
            XElement service = new XElement("Service",
                new XAttribute("ID", ID),
                new XAttribute("Version", (Version != null) ? Version.ToString() : "1.0"),
                new XAttribute("ServerID", ServerID.ToString()),
                new XAttribute("Name", ResourceName ?? string.Empty),
                new XAttribute("ResourceType", ServerResourceType),
                new XAttribute("IsValid", IsValid),
                new XElement("DisplayName", ResourceName ?? string.Empty),
                new XElement("Category", Category ?? string.Empty),
                new XElement("AuthorRoles", string.Empty),
                new XElement("Comment", Comment ?? string.Empty),
                new XElement("Tags", Tags ?? string.Empty),
                new XElement("IconPath", IconPath ?? string.Empty),
                new XElement("HelpLink", HelpLink ?? string.Empty),
                new XElement("UnitTestTargetWorkflowService", UnitTestTargetWorkflowService ?? string.Empty),
                dataList,
                new XElement("Actions", xaml.ToXElement()),
                new XElement("ErrorMessages", WriteErrors())
                );
            return service;
        }

        List<XElement> WriteErrors()
        {
            if(Errors == null || Errors.Count == 0) return null;
            var errorElements = new List<XElement>();
            foreach(var errorInfo in Errors)
            {
                var xElement = new XElement("ErrorMessage");
                xElement.Add(new XAttribute("InstanceID", errorInfo.InstanceID));
                xElement.Add(new XAttribute("Message", errorInfo.Message ?? ""));
                xElement.Add(new XAttribute("ErrorType", errorInfo.ErrorType));
                xElement.Add(new XAttribute("FixType", errorInfo.FixType));
                xElement.Add(new XCData(errorInfo.FixData));
                errorElements.Add(xElement);
            }

            return errorElements;
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
                IEnumerable<ValidationAttribute> validationMap = prop.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
                string errMsg;


                foreach(ValidationAttribute v in validationMap)
                {
                    try
                    {
                        v.Validate(prop.GetValue(this, null), columnName);
                        RemoveError(columnName);
                    }
                    catch(Exception)
                    {
                        AddError(columnName, v.ErrorMessage);

                        return v.ErrorMessage;
                    }
                }

                if(columnName == "ResourceName")
                {
                    if(string.IsNullOrEmpty(ResourceName))
                    {
                        errMsg = "Resource Name must be entered";
                        AddError("NoResourceName", errMsg);
                        return errMsg;
                    }
                    RemoveError("NoResourceName");
                }

                if(columnName == "IconPath")
                {
                    Uri testUri;
                    if(!Uri.TryCreate(IconPath, UriKind.Absolute, out testUri) && !string.IsNullOrEmpty(IconPath))
                    {
                        errMsg = "Icon Path Does Not Exist or is not valid";
                        AddError("IconPathFileDoesNotExist", errMsg);
                        return errMsg;
                    }
                    RemoveError("IconPathFileDoesNotExist");
                }

                if(columnName == "HelpLink")
                {
                    Uri testUri;
                    if(!Uri.TryCreate(HelpLink, UriKind.Absolute, out testUri))
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

        protected override void OnDispose()
        {
            if(_validationService != null)
            {
                _validationService.Dispose();
            }
            if(_permissionsModifiedService != null)
            {
                _permissionsModifiedService.Dispose();
            }
            base.OnDispose();
        }
    }
}