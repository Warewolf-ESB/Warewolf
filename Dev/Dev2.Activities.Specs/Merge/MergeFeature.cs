using Dev2.Activities.PathOperations;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Activities.Specs.Composition;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Core.Tests.Merge.Utils;
using Dev2.Studio.ActivityDesigners;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Dev2.Util;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Test.Agent;
using Warewolf.MergeParser;
using Warewolf.Studio.ViewModels;
using Dev2.Activities.DropBox2016;
using Warewolf.UnitTestAttributes;

namespace Dev2.Activities.Specs.Merge
{
    [Binding]
    public sealed class MergeFeature
    {
        const string remoteResourceString = "RemoteResource";
        const string localResourceString = "LocalResource";
        const string localResourceVersionString = "LocalResourceVersion";
        const string mergeVmString = "mergeVm";
        readonly ScenarioContext _scenarioContext;
        readonly CommonSteps _commonSteps;

        public MergeFeature(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            _commonSteps = new CommonSteps(_scenarioContext);
        }

        [BeforeScenario]
        public void Setup()
        {
            var merge = new MergeFeature(_scenarioContext);
            AppUsageStats.LocalHost = "http://localhost:3142";
            var mergeParser = new ServiceDifferenceParser();
            var pop = new Mock<IPopupController>();
            CustomContainer.Register<IServiceDifferenceParser>(mergeParser);
            CustomContainer.Register<IAsyncWorker>(new SynchronousAsyncWorker());
            CustomContainer.Register(environmentModel);
            CustomContainer.Register(pop.Object);
            localHost = environmentModel.Source;
            localHost.ConnectAsync().Wait(60000);
        }

        IServer localHost;
        readonly IServerRepository environmentModel = ServerRepository.Instance;
        [Given(@"I Load workflow ""(.*)"" from ""(.*)""")]
        public void GivenILoadWorkflowFrom(string resourceName, string serverName)
        {
            if (serverName == "Remote Container")
            {
                WorkflowExecutionSteps._containerOps = new Depends(Depends.ContainerType.Warewolf);
            }
            if (serverName == "Remote Connection Integration")
            {
                WorkflowExecutionSteps._containerOps = new Depends(Depends.ContainerType.CIRemote);
            }
            if (!serverName.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                var remoteServer = environmentModel.FindSingle(a => a.Name.Equals(serverName, StringComparison.InvariantCultureIgnoreCase));
                remoteServer.ConnectAsync().Wait(60000);
                remoteServer.ResourceRepository.Load(true);
                var remoteResource = remoteServer.ResourceRepository.FindSingle(p => p.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
                Assert.IsNotNull(remoteResource, "Resource \"" + resourceName + "\" not found on remote server \"" + serverName + "\".");
                _scenarioContext.Add(remoteResourceString, remoteResource);
            }
            else
            {
                var localResource = localHost.ResourceRepository.FindSingle(p => p.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
                Assert.IsNotNull(localResource, "Resource \"" + resourceName + "\" not found.");
                _scenarioContext.Add(localResourceString, localResource);
            }
        }

        [Given(@"I Load workflow version ""(.*)"" of ""(.*)"" from ""(.*)""")]
        public void GivenILoadWorkflowVersionOfFrom(int versionNo, string resourceName, string serverName)
        {
            if (!serverName.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                var remoteServer = environmentModel.FindSingle(a => a.Name.Equals(serverName, StringComparison.InvariantCultureIgnoreCase));
                remoteServer.ConnectAsync().Wait(60000);
                remoteServer.ResourceRepository.Load(true);
                var remoteResource = remoteServer.ResourceRepository.FindSingle(p => p.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
                Assert.IsNotNull(remoteResource, "Resource \"" + resourceName + "\" not found on remote server \"" + serverName + "\".");
                var versions = remoteServer.ExplorerRepository.GetVersions(remoteResource.ID);
                Assert.IsTrue(versions.Count > 0, "No versions found for resource \"" + resourceName + "\".");
                var version = versions.FirstOrDefault(a => a.VersionNumber == versionNo.ToString());
                Assert.IsNotNull(version, "Version \"" + versionNo + "\" of \"" + resourceName + "\" not found on remote server \"" + serverName + "\".");
                var remoteResourceVersion = version.ToContextualResourceModel(remoteServer, remoteResource.ID);
                remoteResourceVersion.VersionInfo = version;
                _scenarioContext.Add(remoteResourceString, remoteResourceVersion);
            }
            else
            {
                var localResource = localHost.ResourceRepository.FindSingle(p => p.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
                Assert.IsNotNull(localResource, "Resource \"" + resourceName + "\" not found.");
                var versions = localHost.ExplorerRepository.GetVersions(localResource.ID);
                Assert.IsTrue(versions.Count > 0, "No versions found for resource \"" + resourceName + "\" on remote server \"" + serverName + "\".");
                var version = versions.Single(a => a.VersionNumber == versionNo.ToString());
                Assert.IsNotNull(version, "Version \"" + versionNo + "\" of \"" + resourceName + "\" not found.");
                var localResourceVersion = version.ToContextualResourceModel(localHost, localResource.ID);
                localResourceVersion.VersionInfo = version;
                _scenarioContext.Add(localResourceVersionString, localResourceVersion);
            }
        }


        void AddWorkflowToScenario(string nameString, WorkflowData wfData)
        {
            var localResource = localHost.ResourceRepository.FindSingle(p => p.ResourceName.Equals(nameString, StringComparison.InvariantCultureIgnoreCase)) as ResourceModel;

            var localResourceVersion = CreateNewVersionResourceModel(localResource);
            localResourceVersion.WorkflowXaml = new System.Text.StringBuilder(wfData.Different);
            localResourceVersion.VersionInfo = wfData.VersionInfo;

            _scenarioContext.Add(localResourceVersionString, localResourceVersion);
        }

        [Given(@"I Load workflow version conflict MergeToolPositionChange")]
        public void GivenILoadWorkflowVersionConflictMergeToolPositionChange()
        {
            AddWorkflowToScenario("MergeToolPositionChange", WorkflowTestResources.MergeToolPositionChange);
        }

        [Given(@"I Load workflow version of MergeSwitchTool")]
        public void GivenILoadWorkflowVersionOfMergeSwitchTool()
        {
            AddWorkflowToScenario("MergeSwitchTool", WorkflowTestResources.MergeSwitchTool);
        }

        [Given(@"I Load workflow version of MergeRemovedTool")]
        public void GivenILoadWorkflowVersionOfMergeRemovedTool()
        {
            AddWorkflowToScenario("MergeRemovedTool", WorkflowTestResources.MergeRemovedTool);
        }

        [Given(@"I Load workflow version of WorkFlowWithOneObject")]
        public void GivenILoadWorkflowVersionOfWorkFlowWithOneObject()
        {
            AddWorkflowToScenario("WorkFlowWithOneObject", WorkflowTestResources.WorkFlowWithOneObject);
        }

        [Given(@"I Load workflow version of WorkFlowWithOneRecordSet")]
        public void GivenILoadWorkflowVersionOfWorkFlowWithOneRecordSet()
        {
            AddWorkflowToScenario("WorkFlowWithOneRecordSet", WorkflowTestResources.WorkFlowWithOneRecordSet);
        }

        [Given(@"I Load workflow version of WorkFlowWithOneScalar")]
        public void GivenILoadWorkflowVersionOfWorkFlowWithOneScalar()
        {
            AddWorkflowToScenario("WorkFlowWithOneScalar", WorkflowTestResources.WorkFlowWithOneScalar);
        }

        private static ResourceModel CreateNewVersionResourceModel(ResourceModel localResource)
        {
            return new ResourceModel(localResource.Environment)
            {
                Category = localResource.Category,
                DataList = localResource.DataList,
                ID = localResource.ID,
                ServerID = localResource.ServerID,
                IsDatabaseService = localResource.IsDatabaseService,
                IsPluginService = localResource.IsPluginService,
                IsResourceService = localResource.IsResourceService,
                UserPermissions = localResource.UserPermissions,
                ResourceName = localResource.ResourceName,
                ResourceType = localResource.ResourceType,
                IsVersionResource = true
            };
        }

        [When(@"Merge Window is opened with remote ""(.*)""")]
        public void WhenMergeWindowIsOpenedWithRemote(string p0)
        {
            var remoteResource = _scenarioContext.Get<IContextualResourceModel>(remoteResourceString);
            var localResource = _scenarioContext.Get<IContextualResourceModel>(localResourceString);
            var mergeVm = new MergeWorkflowViewModel(localResource, remoteResource, true);
            _scenarioContext.Add(mergeVmString, mergeVm);
        }

        [When(@"Merge Window is opened with local ""(.*)""")]
        public void WhenMergeWindowIsOpenedWithLocal(string p0)
        {
            var localResourceVersion = _scenarioContext.Get<IContextualResourceModel>(localResourceVersionString);
            var localResource = _scenarioContext.Get<IContextualResourceModel>(localResourceString);
            var mergeVm = new MergeWorkflowViewModel(localResource, localResourceVersion, false);
            _scenarioContext.Add(mergeVmString, mergeVm);
        }

        [When(@"Merge Window is opened with local version ""(.*)""")]
        public void WhenMergeWindowIsOpenedWithLocalVersion(string p0)
        {
            var localResourceVersion = _scenarioContext.Get<IContextualResourceModel>(localResourceVersionString);
            var localResource = _scenarioContext.Get<IContextualResourceModel>(localResourceString);
            var mergeVm = new MergeWorkflowViewModel(localResourceVersion, localResource, true);
            _scenarioContext.Add(mergeVmString, mergeVm);
        }

        [Then(@"I select Current Tool")]
        public void ThenISelectCurrentTool()
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var mergeToolModel = mergeVm.Conflicts.Where(a => a is ToolConflictRow && !a.HasConflict && a.IsCurrentChecked)
                                                  .Cast<ToolConflictRow>()
                                                  .Select(p => p.CurrentViewModel)
                                                  .FirstOrDefault() as IToolConflictItem;
            Assert.IsNotNull(mergeToolModel);
            mergeToolModel.IsChecked = true;
        }

        [Then(@"I select Different Tool")]
        public void ThenISelectDifferentTool()
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var mergeToolModel = mergeVm.Conflicts.Where(a => a is ToolConflictRow && !a.HasConflict && a.IsCurrentChecked)
                                                  .Cast<ToolConflictRow>()
                                                  .Select(p => p.DiffViewModel)
                                                  .FirstOrDefault() as IToolConflictItem;
            Assert.IsNotNull(mergeToolModel);
            mergeToolModel.IsChecked = true;
        }

        [Then(@"I select Current Arm")]
        public void ThenISelectCurrentArm()
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var mergeArmConnector = mergeVm.Conflicts.Where(a => a is ConnectorConflictRow && a.HasConflict && !a.IsChecked).Cast<ConnectorConflictRow>().Select(p => p.CurrentArmConnector).FirstOrDefault() as IConnectorConflictItem;
            Assert.IsNotNull(mergeArmConnector);
            mergeArmConnector.IsChecked = true;
        }

        [Then(@"I select Different Arm")]
        public void ThenISelectDifferentArm()
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var mergeArmConnector = mergeVm.Conflicts.Where(a => a is ConnectorConflictRow && a.HasConflict && !a.Different.IsChecked).Cast<ConnectorConflictRow>().Select(p => p.DifferentArmConnector).FirstOrDefault() as IConnectorConflictItem;
            Assert.IsNotNull(mergeArmConnector);
            mergeArmConnector.IsChecked = true;
        }

        [Then(@"Save is enabled")]
        public void ThenSaveIsEnabled()
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            Assert.IsTrue(mergeVm.CanSave);
        }

        [Then(@"Current workflow contains ""(.*)"" tools")]
        public void ThenCurrentWorkflowContainsTools(int expectedToolCount)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var toolCount = mergeVm.Conflicts.Where(a => a is ToolConflictRow).Cast<ToolConflictRow>().Select(p => p.CurrentViewModel).Count();
            Assert.AreEqual(expectedToolCount, toolCount);
        }

        [Then(@"Current workflow contains ""(.*)"" connectors")]
        public void ThenCurrentWorkflowContainsConnectors(int expectedConnectorCount)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var connectorCount = mergeVm.Conflicts.Where(a => a is ConnectorConflictRow).Cast<ConnectorConflictRow>().Select(p => p.CurrentArmConnector).Count();
            Assert.AreEqual(expectedConnectorCount, connectorCount);
        }

        [Then(@"Different workflow contains ""(.*)"" tools")]
        public void ThenDifferentWorkflowContainsTools(int expectedToolCount)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var toolCount = mergeVm.Conflicts.Where(a => a is ToolConflictRow).Cast<ToolConflictRow>().Select(p => p.DiffViewModel).Count();
            Assert.AreEqual(expectedToolCount, toolCount);
        }

        [Then(@"Different workflow contains ""(.*)"" connectors")]
        public void ThenDifferentWorkflowContainsConnectors(int expectedConnectorCount)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var connectorCount = mergeVm.Conflicts.Where(a => a is ConnectorConflictRow).Cast<ConnectorConflictRow>().Select(p => p.DifferentArmConnector).Count();
            Assert.AreEqual(expectedConnectorCount, connectorCount);
        }

        [Then(@"Current workflow header is ""(.*)""")]
        public void ThenCurrentWorkflowHeaderIs(string header)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            Assert.AreEqual(header, mergeVm.ModelFactoryCurrent.Header);
        }

        [Then(@"Current workflow header version is ""(.*)""")]
        public void ThenCurrentWorkflowHeaderVersionIs(string headerVersion)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            Assert.AreEqual(headerVersion, mergeVm.ModelFactoryCurrent.HeaderVersion);
        }

        [Then(@"Different workflow header is ""(.*)""")]
        public void ThenDifferentWorkflowHeaderIs(string header)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            Assert.AreEqual(header, mergeVm.ModelFactoryDifferent.Header);
        }

        [Then(@"Different workflow header version is ""(.*)""")]
        public void ThenDifferentWorkflowHeaderVersionIs(string headerVersion)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            Assert.AreEqual(headerVersion, mergeVm.ModelFactoryDifferent.HeaderVersion);
        }

        [Then(@"Merge conflicts count is ""(.*)""")]
        public void ThenMergeConflictsCountIs(int conflictsCount)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var count = mergeVm.Conflicts.Count();
            Assert.AreEqual(conflictsCount, count);
        }

        [Then(@"Merge variable conflicts is false")]
        public void ThenMergeVariableConflictsIsFalse()
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var a = mergeVm.HasVariablesConflict;
            Assert.IsFalse(a);
        }

        IToolConflictRow GetToolConflictFromRow(int conflictRow)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var conflict = mergeVm.Conflicts.ToList()[conflictRow];
            var toolConflict = conflict as IToolConflictRow;
            return toolConflict;
        }

        [Then(@"conflict ""(.*)"" Current matches tool ""(.*)""")]
        public void ThenConflictCurrentMatchesTool(int conflictRow, string mergeToolDescription)
        {
            var toolConflict = GetToolConflictFromRow(conflictRow);
            Assert.AreEqual(mergeToolDescription, toolConflict.CurrentViewModel.MergeDescription);
        }

        [Then(@"conflict ""(.*)"" Different matches tool ""(.*)""")]
        public void ThenConflictDifferentMatchesTool(int conflictRow, string mergeToolDescription)
        {
            var toolConflict = GetToolConflictFromRow(conflictRow);
            Assert.AreEqual(mergeToolDescription, toolConflict.DiffViewModel.MergeDescription);
        }

        IConnectorConflictRow GetArmConnectorFromRow(int conflictRow)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var conflict = mergeVm.Conflicts.ToList()[conflictRow];
            var toolConflict = conflict as IConnectorConflictRow;
            return toolConflict;
        }

        [Then(@"conflict ""(.*)"" Current Connector matches tool ""(.*)""")]
        public void ThenConflictCurrentConnectorMatchesTool(int conflictRow, string connectorDescription)
        {
            var connector = GetArmConnectorFromRow(conflictRow);
            Assert.AreEqual(connectorDescription, connector.CurrentArmConnector.ArmDescription);
        }

        [Then(@"conflict ""(.*)"" Current Connector matches tool is null")]
        public void ThenConflictCurrentConnectorMatchesToolIsNull(int conflictRow)
        {
            var connector = GetArmConnectorFromRow(conflictRow);
            Assert.IsNull(connector.CurrentArmConnector.ArmDescription);
        }

        [Then(@"conflict ""(.*)"" Different Connector matches tool ""(.*)""")]
        public void ThenConflictDifferentConnectorMatchesTool(int conflictRow, string connectorDescription)
        {
            var connector = GetArmConnectorFromRow(conflictRow);
            Assert.AreEqual(connectorDescription, connector.DifferentArmConnector.ArmDescription);
        }

        [Then(@"conflict ""(.*)"" Different Connector matches tool is null")]
        public void ThenConflictDifferentConnectorMatchesToolIsNull(int conflictRow)
        {
            var connector = GetArmConnectorFromRow(conflictRow);
            Assert.IsNull(connector.DifferentArmConnector.ArmDescription);
        }

        [Then(@"Merge variable conflicts is true")]
        public void ThenMergeVariableConflictsIsTrue()
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var a = mergeVm.HasVariablesConflict;
            Assert.IsTrue(a);
        }

        [Then(@"Merge window has no Conflicting tools")]
        public void ThenMergeWindowHasNoConflictingTools()
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var a = mergeVm.Conflicts.AsEnumerable().All(p => p.HasConflict);
            Assert.IsFalse(a);
        }

        [Then(@"Merge window has ""(.*)"" Conflicting tools")]
        public void ThenMergeWindowHasConflictingTools(int expectedConflicts)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var a = mergeVm.Conflicts.AsEnumerable().Count(p => p.HasConflict);
            Assert.AreEqual(expectedConflicts, a);
        }

        [Given(@"I Load All tools and expect all tools to be mapped")]
        public void GivenILoadAllToolsAndExpectAllToolsToBeMapped()
        {
            var dev2ActivityIOMapping = typeof(IDev2ActivityIOMapping);
            Type[] excludedTypes = { typeof(DsfBaseActivity),
                                     typeof(DsfDropBoxBaseActivity),
                                     typeof(DsfActivityAbstract<>),
                                     typeof(DsfMethodBasedActivity),
                                     typeof(DsfWebActivityBase),
                                     typeof(DsfFlowNodeActivity<>),
                                     typeof(DsfNativeActivity<>),
                                     typeof(DsfAbstractFileActivity),
                                     typeof(DsfFlowDecisionActivity),
                                     typeof(DsfFlowSwitchActivity),
                                     typeof(DsfDotNetDllActivity),
                                     typeof(DsfDatabaseActivity),
                                     typeof(IDev2ActivityIOMapping),
                                     typeof(DsfWorkflowActivity),
                                     typeof(TestMockStep),
                                     typeof(TestMockDecisionStep),
                                     typeof(TestMockSwitchStep),
                                     typeof(DsfAbstractMultipleFilesActivity)};
            
            var allActivityTypes = dev2ActivityIOMapping.Assembly.GetTypes().Where(t => dev2ActivityIOMapping.IsAssignableFrom(t) && (!excludedTypes.Contains(t)));
            
            var countOfAllTools = allActivityTypes.Count();
            var currentDesignerTools = DesignerAttributeMap.DesignerAttributes.Count;
            Assert.AreEqual(countOfAllTools, currentDesignerTools, "Count mismatch between the assembly activities and the mapped activities in DesignerAttributeMap class");
            var allActivitiesAreMapped = allActivityTypes.All(t => DesignerAttributeMap.DesignerAttributes.ContainsKey(t));
            Assert.IsTrue(allActivitiesAreMapped, "Not all activities are mapped in the DesignerAttributeMap class");


            Type[] extraExclusions = { typeof(DsfDecision), typeof(DsfSwitch) };
            var activityTypes = allActivityTypes.Where(t => !(extraExclusions.Contains(t)));
            var currentActivtyDesignerTools = ActivityDesignerHelper.DesignerAttributes.Count;
            Assert.AreEqual(activityTypes.Count(), currentActivtyDesignerTools, "Count mismatch between the assembly activities and the mapped activities in ActivityDesignerHelper class");
            var allActivitiesDesignersAreMapped = activityTypes.All(t => ActivityDesignerHelper.DesignerAttributes.ContainsKey(t));
            Assert.IsTrue(allActivitiesDesignersAreMapped, "Not all activities are mapped in the ActivityDesignerHelper class");
        }
    }
}