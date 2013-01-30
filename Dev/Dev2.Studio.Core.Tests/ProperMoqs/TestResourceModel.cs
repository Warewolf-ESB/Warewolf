using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core;
using System.Activities;
using System.Reflection;

namespace Dev2.Core.Tests.ProperMoqs {
    internal class TestResourceModel : MockBehaviourBase, IContextualResourceModel {

        #region Interface Implementation

        #region Properties

        public bool AllowCategoryEditing { 
            get { return _allowCategoryEditing; } 
            set { _allowCategoryEditing = value; }
        }

        public string AuthorRoles {
            get { return _authorRoles; }
            set { _authorRoles = value; }
        }

        public string Category {
            get { return _category; }
            set { _category = value; }
        }

        public string Comment {
            get { return _comment; }
            set { _comment = value; }
        }

        public string DataTags {
            get { return _dataTags; }
            set { _dataTags = value; }
        }

        public string DisplayName {
            get { return _displayName; }
            set { _displayName = value;}
        }

        public string Error {
            get { return _error; }
        }

        public bool HasErrors {
            get { return _hasErrors; }
        }

        public string HelpLink {
            get { return _helpLink; }
            set { _helpLink = value; }
        }

        public string IconPath {
            get { return _iconPath; }
            set { _iconPath = value; }
        }

        public bool IsDebugMode {
            get { return _isDebugMode; }
            set { _isDebugMode = value; }
        }

        public bool RequiresSignOff {
            get { return _requiresSignOff; }
            set { _requiresSignOff = value; }
        }

        public string ResourceName {
            get { return _resourceName; }
            set { _resourceName = value; }
        }

        public ResourceType ResourceType
        {
            get { return _resourceType; }
            set { _resourceType = value; }
        }

        public string ServiceDefinition {
            get { return _serviceDefinition; }
            set { _serviceDefinition = value; }
        }

        public string WorkflowXaml {
            get { return _workflowXaml; }
            set { _workflowXaml = value; }
        }

        public List<string> TagList {
            get { return _tagList; }
        }

        public string Tags {
            get { return _tags; }
            set { _tags = value; }
        }
        public new string this[string columnName] {
            get { throw new NotImplementedException(); }
        }

        public string UnitTestTargetWorkflowService {
            get { return _unitTestTargetWorkflowService; }
            set { _unitTestTargetWorkflowService = value; }
        }

        public string DataList {
            get { return _dataList; }
            set { _dataList = value; }
        }

        public System.Activities.Activity WorkflowActivity {
            get { return _workflowActivity; }
        }

        public bool IsDatabaseService {
            get { return _isDatabaseServer; }
            set { _isDatabaseServer = value; }
        }

        public bool IsResourceService {
            get { return _isResourceService; }
            set { _isResourceService = value; }
        }

        public IEnvironmentModel Environment {
            get { return _environment; }
        }

        #endregion Properties

        #region Methods

        public void UpdateIconPath(string iconPath)
        {
            return;
        }

        public void Update(IResourceModel resourceModel) {
            return;
        }

        public string ToServiceDefinition() {
            return "TestDefinition";
        }

        public bool IsWorkflowSaved(string viewModelServiceDef) {
            return true;
        }

        #endregion Methods

        #endregion Inteface Implementation

        #region Properties



        #endregion Properties

        #region Locals

        private bool _allowCategoryEditing;
        private string _authorRoles;
        private string _category;
        private string _comment;
        private string _dataTags;
        private string _displayName;
        private string _error = "";
        private bool _hasErrors = false;
        private string _helpLink;
        private string _iconPath;
        private bool _isDebugMode;
        private bool _requiresSignOff;
        private string _resourceName;
        private ResourceType _resourceType;
        private string _serviceDefinition;
        private string _workflowXaml;
        private List<string> _tagList = new List<string>();
        private string _tags;

        private string _unitTestTargetWorkflowService;
        private string _dataList;
        private Activity _workflowActivity = null;
        private bool _isDatabaseServer;
        private bool _isResourceService;
        private IEnvironmentModel _environment;

        #endregion Locals

        #region CTOR

        public TestResourceModel() {
            
        }

        public TestResourceModel(IEnvironmentModel environmentModel) {
            _environment = environmentModel;
        }

        #endregion CTOR

        #region Behaviour Overrides

        internal override void ChangeReturnValue(string Name, Enums.enTestObjectBehaviourChangeType behaviourType, object ReturnValue) {
            base.ChangeReturnValue(Name, behaviourType, ReturnValue);
        }

        #endregion Behaviour Overrides
    }
}
