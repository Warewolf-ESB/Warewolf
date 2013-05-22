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

        public Guid ID { get; set; }

        public bool AllowCategoryEditing { get; set; }

        public string AuthorRoles { get; set; }

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

        public string WorkflowXaml { get; set; }

        public List<string> TagList { get; private set; }

        public string Tags { get; set; }

        public new string this[string columnName] {
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

        private readonly Guid _serverID ;
        public Guid ServerID
        {
            get { return _serverID; }
        }

        #endregion Properties

        #region Methods

        public void UpdateIconPath(string iconPath)
        {
            return;
        }

        public bool IsNewWorkflow { get; set; }
        public string ServerResourceType { get; set; }

        public void Update(IResourceModel resourceModel) {
            return;
        }

        public string ConnectionString { get; set; }
        public bool IsDuplicate { get; set; }

        public string ToServiceDefinition() {
            return "TestDefinition";
        }

        #endregion Methods

        #endregion Inteface Implementation

        #region Properties



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

        #region Behaviour Overrides

        internal override void ChangeReturnValue(string Name, Enums.enTestObjectBehaviourChangeType behaviourType, object ReturnValue) {
            base.ChangeReturnValue(Name, behaviourType, ReturnValue);
        }

        #endregion Behaviour Overrides


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
    }
}
