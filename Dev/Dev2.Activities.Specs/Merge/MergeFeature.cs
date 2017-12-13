﻿using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Dev2.Util;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using TechTalk.SpecFlow;
using Warewolf.MergeParser;
using Warewolf.Studio.ViewModels;

namespace Dev2.Activities.Specs.Merge
{
    [Binding]
    public sealed class MergeFeature
    {
        const string remoteResourceString = "RemoteResource";
        const string localResourceString = "LocalResource";
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
            localHost.Connect();
            localHost.ResourceRepository.ForceLoad();
        }

        IServer localHost;
        IServerRepository environmentModel = ServerRepository.Instance;
        [Given(@"I Load workflow ""(.*)"" from ""(.*)""")]
        public void GivenILoadWorkflowFrom(string resourceName, string serverName)
        {
            if (!serverName.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                var remoteServer = environmentModel.FindSingle(a => a.Name.Equals(serverName, StringComparison.InvariantCultureIgnoreCase));
                remoteServer.Connect();
                remoteServer.ResourceRepository.ForceLoad();
                var remoteResource = remoteServer.ResourceRepository.FindSingle(p => p.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
                _scenarioContext.Add(remoteResourceString, remoteResource);
            }
            else
            {
                var localResource = localHost.ResourceRepository.FindSingle(p => p.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
                _scenarioContext.Add(localResourceString, localResource);
            }
        }

        [Given(@"I Load workflow version ""(.*)"" of ""(.*)"" from ""(.*)""")]
        public void GivenILoadWorkflowVersionOfFrom(int versionNo, string resourceName, string serverName)
        {
            if (!serverName.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                var remoteServer = environmentModel.FindSingle(a => a.Name.Equals(serverName, StringComparison.InvariantCultureIgnoreCase));
                remoteServer.Connect();
                remoteServer.ResourceRepository.ForceLoad();
                var remoteResource = remoteServer.ResourceRepository.FindSingle(p => p.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
                var versions = remoteServer.ExplorerRepository.GetVersions(remoteResource.ID);
                var version = versions.Single(a => a.VersionNumber == versionNo.ToString());
                var remoteResourceVersion = version.ToContextualResourceModel(remoteServer, remoteResource.ID);
                _scenarioContext.Add(remoteResourceString, remoteResourceVersion);
            }
            else
            {
                var localResource = localHost.ResourceRepository.FindSingle(p => p.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
                var versions = localHost.ExplorerRepository.GetVersions(localResource.ID);
                var version = versions.Single(a => a.VersionNumber == versionNo.ToString());
                var localResourceVersion = version.ToContextualResourceModel(localHost, localResource.ID);
                _scenarioContext.Add(localResourceString, localResourceVersion);
            }
        }

        [When(@"Merge Window is opened with ""(.*)""")]
        public void WhenMergeWindowIsOpenedWith(string p0)
        {
            var remoteResource = _scenarioContext.Get<IContextualResourceModel>(remoteResourceString);
            var localResource = _scenarioContext.Get<IContextualResourceModel>(localResourceString);
            var mergeVm = new MergeWorkflowViewModel(localResource, remoteResource, true);
            _scenarioContext.Add(mergeVmString, mergeVm);
        }

        [Then(@"Current workflow contains ""(.*)"" tools")]
        public void ThenCurrentWorkflowContainsTools(int currentToolCount)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var count = mergeVm.Conflicts.Where(a => a is ToolConflict).Cast<ToolConflict>().Select(p => p.CurrentViewModel).Count();
            var count1 = mergeVm.Conflicts.Where(a => a is ArmConnectorConflict).Cast<ArmConnectorConflict>().Select(p => p.CurrentArmConnector).Count();
            Assert.AreEqual(currentToolCount, count + count1);
        }

        [Then(@"Different workflow contains ""(.*)"" tools")]
        public void ThenDifferentWorkflowContainsTools(int toolCount)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var count = mergeVm.Conflicts.Where(a => a is ToolConflict).Cast<ToolConflict>().Select(p => p.DiffViewModel).Count();
            var count1 = mergeVm.Conflicts.Where(a => a is ArmConnectorConflict).Cast<ArmConnectorConflict>().Select(p => p.DifferentArmConnector).Count();
            Assert.AreEqual(toolCount, count + count1);
        }

        [Then(@"Merge conflicts count is ""(.*)""")]
        public void ThenMergeConflictsCountIs(int conflictsCount)
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var count = mergeVm.Conflicts.Count;
            Assert.AreEqual(conflictsCount, count);
        }

        [Then(@"Merge variable conflicts is false")]
        public void ThenMergeVariableConflictsIsFalse()
        {
            var mergeVm = _scenarioContext.Get<MergeWorkflowViewModel>(mergeVmString);
            var a = mergeVm.HasVariablesConflict;
            Assert.IsFalse(a);
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

        [AfterScenario]
        public void CleanUp()
        {
            _scenarioContext?.Clear();
        }
    }
}
