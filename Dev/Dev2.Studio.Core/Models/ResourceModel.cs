#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Collections;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.Collections;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Data;
using Warewolf.Resource.Errors;


namespace Dev2.Studio.Core.Models
{
    public class ResourceModel : ValidationController, IDataErrorInfo, IContextualResourceModel
    {
        bool _allowCategoryEditing = true;
        string _category;
        string _comment;
        string _dataList;
        string _dataTags;
        string _displayName = string.Empty;
        IServer _environment;
        string _helpLink;
        bool _isDatabaseService;
        bool _isDebugMode;
        bool _isResourceService;
        string _resourceName;
        ResourceType _resourceType;
        string _tags;
        string _unitTestTargetWorkflowService;
        StringBuilder _workflowXaml;
        Version _version;
        bool _isPluginService;
        bool _isWorkflowSaved;
        Guid _id;

        IDesignValidationService _validationService;
        readonly ObservableReadOnlyList<IErrorInfo> _errors = new ObservableReadOnlyList<IErrorInfo>();
        readonly ObservableReadOnlyList<IErrorInfo> _fixedErrors = new ObservableReadOnlyList<IErrorInfo>();
        bool _isValid;
        Permissions _userPermissions;
        IVersionInfo _versionInfo;

        [ExcludeFromCodeCoverage]
        public ResourceModel() { }
        public ResourceModel(IServer environment)
            : this(environment, EventPublishers.Aggregator)
        {
        }

        public ResourceModel(IServer environment, IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);

            TagList = new List<string>();
            Environment = environment;

            if (environment?.Connection != null)
            {
                ServerID = environment.Connection.ServerID;
            }
            IsWorkflowSaved = true;
        }

        public string Inputs { get; set; }

        public string Outputs { get; set; }

        public Guid OriginalId { get; set; }

        public bool IsValid
        {
            get => _isValid;
            set
            {
                _isValid = value;
                NotifyOfPropertyChange(nameof(IsValid));
            }
        }

        public IObservableReadOnlyList<IErrorInfo> Errors => _errors;
        public IObservableReadOnlyList<IErrorInfo> FixedErrors => _fixedErrors;

        public bool IsWorkflowSaved
        {
            get => _isWorkflowSaved;
            set
            {
                _isWorkflowSaved = value;
                OnResourceSaved?.Invoke(this);
            }
        }

        public IServer Environment
        {
            get => _environment;
            private set
            {
                _environment = value;
                if (value != null && _environment.Connection != null)
                {
                    _validationService = new DesignValidationService(_environment.Connection.ServerEvents);

                    _validationService.Subscribe(_environment.EnvironmentID, ReceiveEnvironmentValidation);
                }
                NotifyOfPropertyChange(nameof(Environment));
                NotifyOfPropertyChange("CanExecute");
            }
        }

        public Guid ServerID { get; set; }

        public bool IsDatabaseService
        {
            get => _isDatabaseService;
            set
            {
                _isDatabaseService = value;
                NotifyOfPropertyChange(nameof(IsDatabaseService));
            }
        }

        public bool IsPluginService
        {
            get => _isPluginService;
            set
            {
                _isPluginService = value;
                NotifyOfPropertyChange(nameof(IsPluginService));
            }
        }

        public bool IsResourceService
        {
            get => _isResourceService;
            set
            {
                _isResourceService = value;
                NotifyOfPropertyChange(nameof(IsResourceService));
            }
        }

        public Guid ID
        {
            get => _id;
            set
            {
                _id = value;
                _validationService?.Subscribe(_id, ReceiveDesignValidation);
            }
        }

        public Permissions UserPermissions
        {
            get => _userPermissions;
            set
            {
                if (value == _userPermissions)
                {
                    return;
                }
                _userPermissions = value;
                NotifyOfPropertyChange(() => UserPermissions);
            }
        }

        public bool IsAuthorized(AuthorizationContext authorizationContext) => (UserPermissions & authorizationContext.ToPermissions()) != 0;

        public Version Version
        {
            get => _version;
            set
            {
                _version = value;
                NotifyOfPropertyChange(nameof(Version));
            }
        }

        public bool AllowCategoryEditing
        {
            get => _allowCategoryEditing;
            set
            {
                _allowCategoryEditing = value;
                NotifyOfPropertyChange(nameof(AllowCategoryEditing));
            }
        }

        [Required(ErrorMessage = @"Please enter a name for this resource")]
        public string ResourceName
        {
            get => _resourceName;
            set
            {
                _resourceName = value.Trim();
                NotifyOfPropertyChange(nameof(ResourceName));
            }
        }

        public override string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                {
                    _displayName = ResourceType == ResourceType.WorkflowService ? "Workflow" : ResourceType.ToString();
                }

                return _displayName;
            }
            set
            {
                _displayName = value;
                NotifyOfPropertyChange(nameof(DisplayName));
            }
        }

        public string UnitTestTargetWorkflowService
        {
            get => _unitTestTargetWorkflowService;
            set
            {
                _unitTestTargetWorkflowService = value;
                NotifyOfPropertyChange(nameof(UnitTestTargetWorkflowService));
            }
        }

        public List<string> TagList { get; }

        public string DataList
        {
            get => _dataList;
            set
            {
                if (value != _dataList)
                {
                    _dataList = value;
                    NotifyOfPropertyChange(nameof(DataList));
                    OnDataListChanged?.Invoke();
                }
            }
        }

        public ResourceType ResourceType
        {
            get => _resourceType;
            set
            {
                _resourceType = value;
                NotifyOfPropertyChange(nameof(ResourceType));
            }
        }

        [Required(ErrorMessage = @"Please enter a Category for this resource")]
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                NotifyOfPropertyChange(nameof(Category));
            }
        }

        public string Tags
        {
            get => _tags;
            set
            {
                _tags = value;
                NotifyOfPropertyChange(nameof(Tags));
            }
        }

        [Required(ErrorMessage = @"Please enter the Comment for this resource")]
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                NotifyOfPropertyChange(nameof(Comment));
            }
        }

        public StringBuilder WorkflowXaml
        {
            get => _workflowXaml;
            set
            {
                _workflowXaml = value;
                NotifyOfPropertyChange(nameof(WorkflowXaml));
            }
        }

        public bool IsVersionResource { get; set; }

        public bool HasErrors => ValidationErrors.Count > 0;

        public string DataTags
        {
            get => _dataTags;
            set
            {
                _dataTags = value;
                NotifyOfPropertyChange(nameof(DataTags));
            }
        }

        [Required(ErrorMessage = @"Please enter a valid help link")]
        public string HelpLink
        {
            get => _helpLink;
            set
            {
                _helpLink = value;
                NotifyOfPropertyChange(nameof(HelpLink));
            }
        }

        public bool IsDebugMode
        {
            get => _isDebugMode;
            set
            {
                _isDebugMode = value;
                NotifyOfPropertyChange(nameof(IsDebugMode));
            }
        }

        public bool IsNewWorkflow { get; set; }
        public bool IsNotWarewolfPath { get; set; }
        public bool IsOpeningFromOtherDir { get; set ; }

        public string ServerResourceType { get; set; }

        public IVersionInfo VersionInfo
        {
            get => _versionInfo;
            set
            {
                if (Equals(value, _versionInfo))
                {
                    return;
                }
                _versionInfo = value;
                NotifyOfPropertyChange(nameof(VersionInfo));
            }
        }

        public event Action<IContextualResourceModel> OnResourceSaved;
        public event System.Action OnDataListChanged;

        public event EventHandler<DesignValidationMemo> OnDesignValidationReceived;

        void ReceiveDesignValidation(DesignValidationMemo memo)
        {
            if (memo.Errors.Any(info => info.InstanceID != Guid.Empty))
            {
                IsValid = memo.IsValid && _errors.Count == 0;
                if (memo.Errors.Count > 0)
                {
                    foreach (var error in Errors.Where(error => !memo.Errors.Contains(error)))
                    {
                        _fixedErrors.Add(error);
                    }
                    if (_errors.Count > 0)
                    {
                        _errors.Clear();
                    }
                    foreach (var error in memo.Errors)
                    {
                        _errors.Add(error);
                    }
                }
            }
            OnDesignValidationReceived?.Invoke(this, memo);
        }

        public IView GetView(Func<IView> view) => view.Invoke();

        public void ClearErrors()
        {
            _errors.Clear();
            NotifyOfPropertyChange(() => Errors);
            NotifyOfPropertyChange(() => IsValid);
        }

        void ReceiveEnvironmentValidation(DesignValidationMemo memo)
        {
            foreach (var error in memo.Errors)
            {
                _errors.Add(error);
            }
        }

        public IList<IErrorInfo> GetErrors(Guid instanceId) => _errors.Where(e => e.InstanceID == instanceId).ToList();

        public void AddError(IErrorInfo error)
        {
            _errors.Add(error);
        }

        public void RemoveError(IErrorInfo error)
        {
            var theError = Errors.FirstOrDefault(info => info.Equals(error)) ?? Errors.FirstOrDefault(info => info.ErrorType == error.ErrorType && info.FixType == error.FixType);
            if (theError != null)
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
            foreach (var fixedError in _fixedErrors)
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
            if (resourceModel != null)
            {
                AllowCategoryEditing = resourceModel.AllowCategoryEditing;
                Category = resourceModel.Category;
                Comment = resourceModel.Comment;
                DataTags = resourceModel.DataTags;
                DisplayName = resourceModel.DisplayName;
                VersionInfo = resourceModel.VersionInfo;
                IsVersionResource = resourceModel.IsVersionResource;
                HelpLink = resourceModel.HelpLink;
                IsDebugMode = resourceModel.IsDebugMode;
                ResourceName = resourceModel.ResourceName;
                ResourceType = resourceModel.ResourceType;
                Tags = resourceModel.Tags;
                DataList = resourceModel.DataList;
                Version = resourceModel.Version;
                ConnectionString = resourceModel.ConnectionString;
                ID = resourceModel.ID;
                ServerResourceType = resourceModel.ServerResourceType;
                UserPermissions = resourceModel.UserPermissions;
                Inputs = resourceModel.Inputs;
                Outputs = resourceModel.Outputs;
                WorkflowXaml = resourceModel.WorkflowXaml;
                _errors.Clear();
                if (resourceModel.Errors != null)
                {
                    foreach (var error in resourceModel.Errors)
                    {
                        _errors.Add(error);
                    }
                }
            }
        }

        public string ConnectionString { get; set; }

        public StringBuilder ToServiceDefinition() => ToServiceDefinition(false);

        public StringBuilder ToServiceDefinition(bool prepairForDeployment)
        {
            if (ResourceType == ResourceType.WorkflowService)
            {
                StringBuilder result = WorkflowServiceResourceType();
                return result;
            }
            if (ResourceType == ResourceType.Source || ResourceType == ResourceType.Server)
            {
                StringBuilder result = SourceOrServerResourceType(prepairForDeployment);
                return result;
            }
            throw new Exception(ErrorResource.ToServiceDefinitionDoesNotRupportResourcesOfTypeSource);
        }

        private StringBuilder WorkflowServiceResourceType()
        {
            var result = new StringBuilder();
            var xaml = WorkflowXaml;
            if (xaml == null || xaml.Length == 0)
            {
                var msg = Environment.ResourceRepository.FetchResourceDefinition(Environment, GlobalConstants.ServerWorkspaceID, ID, false);
                if (msg?.Message != null)
                {
                    xaml = msg.Message;
                }
            }
            if (xaml != null && xaml.Length != 0)
            {
                var service = CreateWorkflowXElement(xaml);
                var xws = new XmlWriterSettings { OmitXmlDeclaration = true };
                using (XmlWriter xwriter = XmlWriter.Create(result, xws))
                {
                    service.Save(xwriter);
                }
            }

            return result;
        }

        private StringBuilder SourceOrServerResourceType(bool prepairForDeployment)
        {
            var msg = Environment.ResourceRepository.FetchResourceDefinition(Environment, GlobalConstants.ServerWorkspaceID, ID, prepairForDeployment);
            StringBuilder result = msg.Message;

            if (result == null || result.Length == 0)
            {
                result = WorkflowXaml;
            }

            if (result != null)
            {
                var startNode = result.IndexOf("<Category>", 0, true) + "<Category>".Length;
                var endNode = result.IndexOf("</Category>", 0, true);
                if (endNode > startNode)
                {
                    var len = endNode - startNode;
                    var oldCategory = result.Substring(startNode, len);
                    if (oldCategory != Category)
                    {
                        result = result.Replace(oldCategory, Category);
                    }
                }
            }

            return result;
        }

        XElement CreateWorkflowXElement(StringBuilder xaml)
        {
            var dataList = string.IsNullOrEmpty(DataList) ? new XElement("DataList") : XElement.Parse(DataList);
            var service = CreateServiceElement(xaml, dataList);
            return service;
        }

        private XElement CreateServiceElement(StringBuilder xaml, XElement dataList)
        {
            return new XElement("Service",
                        new XAttribute("ID", ID),
                        new XAttribute("Version", Version?.ToString() ?? "1.0"),
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
                    new XElement("HelpLink", HelpLink ?? string.Empty),
                    new XElement("UnitTestTargetWorkflowService", UnitTestTargetWorkflowService ?? string.Empty),
                    dataList,
                    new XElement("Action",
                        new XAttribute("Name", "InvokeWorkflow"),
                        new XAttribute("Type", "Workflow"),
                    new XElement("XamlDefinition", xaml)),
                    new XElement("ErrorMessages", WriteErrors()));
        }

        List<XElement> WriteErrors()
        {
            if (Errors == null || Errors.Count == 0)
            {
                return null;
            }

            var errorElements = new List<XElement>();
            foreach (var errorInfo in Errors)
            {
                var xElement = new XElement("ErrorMessage");
                xElement.Add(new XAttribute("InstanceID", errorInfo.InstanceID));
                xElement.Add(new XAttribute("Message", errorInfo.Message ?? ""));
                xElement.Add(new XAttribute("ErrorType", errorInfo.ErrorType));
                xElement.Add(new XAttribute("FixType", errorInfo.FixType));
                if (!string.IsNullOrEmpty(errorInfo.FixData))
                {
                    xElement.Add(new XCData(errorInfo.FixData));
                }
                errorElements.Add(xElement);
            }

            return errorElements;
        }
        
        [ExcludeFromCodeCoverage]
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                var prop = GetType().GetProperty(columnName);
                var validationMap = prop.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
                string errMsg;

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
                    RemoveError("NoResourceName");
                }

                if (columnName == "HelpLink" && !Uri.TryCreate(HelpLink, UriKind.Absolute, out Uri testUri))
                {
                    errMsg = "The help link is not in a valid format";
                    AddError(columnName, errMsg);
                    return errMsg;
                }


                return null;
            }
        }

        public string GetSavePath()
        {
            if (!string.IsNullOrEmpty(Category))
            {
                var savePath = Category;
                var resourceNameIndex = Category.LastIndexOf(ResourceName, StringComparison.InvariantCultureIgnoreCase);
                if (resourceNameIndex >= 0)
                {
                    savePath = Category.Substring(0, resourceNameIndex);
                }
                return savePath;
            }
            return "";
        }

        protected override void OnDispose()
        {
            _validationService?.Dispose();
            base.OnDispose();
        }

        public StringBuilder GetWorkflowXaml()
        {
            if (WorkflowXaml != null)
            {
                return WorkflowXaml;
            }

            var msg = Environment?.ResourceRepository.FetchResourceDefinition(Environment, GlobalConstants.ServerWorkspaceID, ID, true);
            if (msg != null && msg.Message.Length != 0)
            {
                WorkflowXaml = msg.Message;
            }

            return WorkflowXaml;
        }
    }
}
