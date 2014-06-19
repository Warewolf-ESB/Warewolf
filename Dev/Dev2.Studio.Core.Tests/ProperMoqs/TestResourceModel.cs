using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Dev2.Collections;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Core.Tests.ProperMoqs
{
    internal class TestResourceModel : MockBehaviourBase, IContextualResourceModel
    {

        #region Interface Implementation

        #region Properties

        public bool IsValid { get; set; }

        public Guid ID { get; set; }
        public Permissions UserPermissions { get; set; }

        public bool IsAuthorized(AuthorizationContext authorizationContext)
        {
            return false;
        }

        public bool AllowCategoryEditing { get; set; }

        public string Category { get; set; }

        public string Comment { get; set; }

        public string DataTags { get; set; }

        public string DisplayName { get; set; }

        public string Error { get; private set; }

        public bool HasErrors { get; private set; }

        public string HelpLink { get; set; }

        public string IconPath { get; set; }

        public bool IsDebugMode { get; set; }

        public bool RequiresSignOff { get; set; }

        public string ResourceName { get; set; }

        public ResourceType ResourceType { get; set; }

        public string ServiceDefinition { get; set; }

        public StringBuilder WorkflowXaml { get; set; }

        public List<string> TagList { get; private set; }

        public string Tags { get; set; }

        public new string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string UnitTestTargetWorkflowService { get; set; }

        public string DataList { get; set; }

        public Activity WorkflowActivity { get; private set; }

        public bool IsDatabaseService { get; set; }

        public bool IsPluginService { get; set; }

        public bool IsResourceService { get; set; }

        public bool IsWorkflowSaved { get; set; }

        public IEnvironmentModel Environment { get; private set; }

        private readonly Guid _serverID;
        public Guid ServerID
        {
            get { return _serverID; }
        }

        #endregion Properties

        #region Methods

        public void UpdateIconPath(string iconPath)
        {
        }

        public bool IsNewWorkflow { get; set; }
        public string ServerResourceType { get; set; }
        public event Action<IContextualResourceModel> OnResourceSaved;
        public event Action OnDataListChanged;
        public event EventHandler<DesignValidationMemo> OnDesignValidationReceived;

        public void ClearErrors()
        {
        }

        public void RaiseEvents()
        {
            if(OnResourceSaved != null)
            {
                OnResourceSaved(this);
            }
            if(OnDataListChanged != null)
            {
                OnDataListChanged();
            }
            if(OnDesignValidationReceived != null)
            {
                OnDesignValidationReceived(this, new DesignValidationMemo());
            }
        }

        public void Update(IResourceModel resourceModel)
        {
        }

        public string ConnectionString { get; set; }

        public StringBuilder ToServiceDefinition()
        {
            return new StringBuilder("TestDefinition");
        }

        #endregion Methods

        #endregion Inteface Implementation

        #region Properties

        public string Inputs { get; set; }

        public string Outputs { get; set; }

        #endregion Properties

        #region Locals

        #endregion Locals

        #region CTOR

        public TestResourceModel()
        {
            _serverID = Guid.Empty;
            WorkflowActivity = null;
            TagList = new List<string>();
            Error = "";
            HasErrors = false;
            Errors = new ObservableReadOnlyList<IErrorInfo>();
            FixedErrors = new ObservableReadOnlyList<IErrorInfo>();
        }

        public TestResourceModel(IEnvironmentModel environmentModel, Guid serverID)
        {
            WorkflowActivity = null;
            TagList = new List<string>();
            Error = "";
            HasErrors = false;
            Environment = environmentModel;
            _serverID = serverID;
        }

        #endregion CTOR



        Guid IContextualResourceModel.ServerID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public Version Version
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public IObservableReadOnlyList<IErrorInfo> Errors { get; private set; }
        public IObservableReadOnlyList<IErrorInfo> FixedErrors { get; private set; }

        public IList<IErrorInfo> GetErrors(Guid instanceId)
        {
            return null;
        }

        public void AddError(IErrorInfo error)
        {
        }

        public void RemoveError(IErrorInfo error)
        {
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
