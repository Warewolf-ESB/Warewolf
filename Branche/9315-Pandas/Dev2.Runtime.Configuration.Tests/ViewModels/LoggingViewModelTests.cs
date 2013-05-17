using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Runtime.Configuration.Tests.ViewModels
{
    [TestClass]
    public class LoggingViewModelTests
    {
        [TestMethod]
        public void HasServiceInputOptionsExpectsTrue()
        {
            var settings = GetSettingsObject();
            settings.RunPostWorkflow = true;
            var postWorkflow = GetWorkFlowDescriptor();
            settings.PostWorkflow = postWorkflow;

            var vm = new LoggingViewModel();
            vm.ServiceInputOptions.Add("TestInput");
            vm.Object = settings;

            Assert.IsTrue(vm.HasServiceInputOptions);
        }

        [TestMethod]
        public void HasServiceInputOptionsExpectsFalseWhenNoOptions()
        {
            var settings = GetSettingsObject();
            settings.RunPostWorkflow = true;
            var postWorkflow = GetWorkFlowDescriptor();
            settings.PostWorkflow = postWorkflow;

            var vm = new LoggingViewModel();
            vm.Object = settings;

            Assert.IsFalse(vm.HasServiceInputOptions);
        }

        [TestMethod]
        public void HasServiceInputOptionsExpectsFalseWhenNoPostWorkflow()
        {
            var settings = GetSettingsObject();
            settings.RunPostWorkflow = false;

            var vm = new LoggingViewModel();
            vm.ServiceInputOptions.Add("TestInput");
            vm.Object = settings;

            Assert.IsFalse(vm.HasServiceInputOptions);
        }

        [TestMethod]
        public void HasServiceInputOptionsExpectsFalseWhenRunPostWorkflowFalse()
        {
            var settings = GetSettingsObject();
            settings.RunPostWorkflow = false;
            var postWorkflow = GetWorkFlowDescriptor();
            settings.PostWorkflow = postWorkflow;

            var vm = new LoggingViewModel();
            vm.ServiceInputOptions.Add("TestInput");
            vm.Object = settings;

            Assert.IsFalse(vm.HasServiceInputOptions);
        }

        [TestMethod]
        public void LogAllToggledToFalseExpectsSettingsToggledAndWorkflowsToggled()
        {
            var descriptors = GetWorkFlowDescriptors(3, true);
            var settings = GetSettingsObject(descriptors);

            var vm = new LoggingViewModel() {LogAll = true};
            vm.Object = settings;
            vm.LogAll = false;

            Assert.IsFalse(vm.LogAll);
            vm.LoggingSettings.Workflows.ToList().ForEach(wf => Assert.IsFalse(wf.IsSelected));
        }

        [TestMethod]
        public void LogAllToggledToTrueExpectsSettingsToggledAndWorkflowsToggled()
        {
            var descriptors = GetWorkFlowDescriptors(3, true);
            var settings = GetSettingsObject(descriptors);

            var vm = new LoggingViewModel() {LogAll = false};
            vm.Object = settings;
            vm.LogAll = true;

            Assert.IsTrue(vm.LogAll);
            vm.LoggingSettings.Workflows.ToList().ForEach(wf => Assert.IsTrue(wf.IsSelected));
        }

        private static ILoggingSettings GetSettingsObject(IEnumerable<IWorkflowDescriptor> workflows = null)
        {
            var settings = new LoggingSettings("InvalidUri");
            if (workflows != null)
            {
                workflows.ToList().ForEach(wf => settings.Workflows.Add(wf));
            }
            return settings;
        }

        private static IEnumerable<IWorkflowDescriptor> GetWorkFlowDescriptors(int number, bool isSelected = false)
        {
            var descriptors = new List<IWorkflowDescriptor>();
            for (int i = 0; i < number; i++)
            {
                descriptors.Add(GetWorkFlowDescriptor(isSelected));
            }
            return descriptors;
        }

        private static IWorkflowDescriptor GetWorkFlowDescriptor(bool isSelected = false)
        {
            var descriptor = new WorkflowDescriptor() { IsSelected = isSelected };
            return descriptor;
        }
    }
}
