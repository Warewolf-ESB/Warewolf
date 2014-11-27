
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces.ComponentModel;
using Dev2.Common.Interfaces.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Services;
using Dev2.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.Tests.XML;
using Dev2.Runtime.Configuration.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Runtime.Configuration.Tests.ViewModels
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class LoggingViewModelTests
    {
        [TestMethod]
        public void HasServiceInputOptionsExpectsTrue()
        {
            var settings = GetSettingsObject();
            settings.RunPostWorkflow = true;
            var postWorkflow = GetWorkFlowDescriptor();
            postWorkflow.ResourceName = "TestPostWorkflow";
            postWorkflow.ResourceID = Guid.NewGuid().ToString();
            settings.PostWorkflow = postWorkflow;
            settings.Workflows.Add(postWorkflow);

            var vm = GetVM();

            var commService = new Mock<ICommunicationService>();

            commService.Setup(s => s.GetResources(It.IsAny<string>()))
                .Returns(new List<WorkflowDescriptor> { postWorkflow });
            commService.Setup(s => s.GetDataListInputs(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<DataListVariable> { new DataListVariable { Name = "TestInput" } });
            vm.CommunicationService = commService.Object;

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

            var vm = GetVM();

            vm.Object = settings;

            Assert.IsFalse(vm.HasServiceInputOptions);
        }

        [TestMethod]
        public void HasServiceInputOptionsExpectsFalseWhenNoPostWorkflow()
        {
            var settings = GetSettingsObject();
            settings.RunPostWorkflow = false;

            var vm = GetVM();
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

            var vm = GetVM();
            vm.ServiceInputOptions.Add("TestInput");
            vm.Object = settings;

            Assert.IsFalse(vm.HasServiceInputOptions);
        }

        [TestMethod]
        public void LogAllToggledToFalseExpectsSettingsToggledAndWorkflowsToggled()
        {
            //Setup
            var descriptors = GetWorkFlowDescriptors(3, true).ToList();
            var settings = GetSettingsObject(descriptors);
            var vm = GetVM(descriptors);
            vm.LogAll = true;

            //Test
            vm.Object = settings;
            vm.LogAll = false;

            //Assert
            Assert.IsFalse(vm.LogAll);
            vm.LoggingSettings.Workflows.ToList().ForEach(wf => Assert.IsFalse(wf.IsSelected));
        }

        [TestMethod]
        public void LogAllToggledToTrueExpectsSettingsToggledAndWorkflowsToggled()
        {
            //Setup
            var descriptors = GetWorkFlowDescriptors(3, false).ToList();
            var settings = GetSettingsObject(descriptors);
            var vm = GetVM(descriptors);
            vm.LogAll = false;

            //Test
            vm.Object = settings;
            vm.LogAll = true;

            //Assert
            Assert.IsTrue(vm.LogAll);
            vm.LoggingSettings.Workflows.ToList().ForEach(wf => Assert.IsTrue(wf.IsSelected));
        }

        [TestMethod]
        public void LoggingSettingsWithValidRunPostWorkflowReturnsNoError()
        {
            var logging = new LoggingSettings(XmlResource.Fetch("LoggingSettings"), "localhost");

            logging.PostWorkflow = logging.Workflows.First(wf => wf.ResourceID == logging.PostWorkflow.ResourceID);

            var vm = GetVM(logging.Workflows.Cast<WorkflowDescriptor>());
            vm.Object = logging;

            var errorResult = vm["PostWorkflowName"];
            Assert.AreEqual(errorResult, "");
            Assert.AreEqual(vm.Error, "");
        }

        [TestMethod]
        public void LoggingSettingsWithRunPostWorkflowSetButInvalidPostWorkflowSelectedReturnsError()
        {
            var logging = new LoggingSettings(XmlResource.Fetch("LoggingSettings"), "localhost");

            logging.PostWorkflow = new WorkflowDescriptor
                {
                    ResourceID = Guid.NewGuid().ToString(),
                    ResourceName = "Fail"
                };

            var vm = GetVM(logging.Workflows.Cast<WorkflowDescriptor>());

            vm.Object = logging;

            vm.RunPostWorkflow = true;

            var errorResult = vm["PostWorkflowName"];
            Assert.AreEqual(errorResult, "Invalid workflow selected");
            Assert.AreEqual(vm.Error, "Invalid workflow selected");
        }

        private static ILoggingSettings GetSettingsObject(IEnumerable<IWorkflowDescriptor> workflows = null)
        {
            var settings = new LoggingSettings("InvalidUri");
            if(workflows != null)
            {
                workflows.ToList().ForEach(wf => settings.Workflows.Add(wf));
            }
            return settings;
        }

        private static IEnumerable<WorkflowDescriptor> GetWorkFlowDescriptors(int number, bool isSelected = false)
        {
            var descriptors = new List<WorkflowDescriptor>();
            for(int i = 0; i < number; i++)
            {
                descriptors.Add(GetWorkFlowDescriptor(isSelected));
            }
            return descriptors;
        }

        private static WorkflowDescriptor GetWorkFlowDescriptor(bool isSelected = false)
        {
            var descriptor = new WorkflowDescriptor() { IsSelected = isSelected, ResourceID = Guid.NewGuid().ToString() };
            return descriptor;
        }

        private static LoggingViewModel GetVM(IEnumerable<WorkflowDescriptor> descriptors = null)
        {
            var vm = new LoggingViewModel();
            var commService = new Mock<ICommunicationService>();
            if(descriptors == null)
                descriptors = new List<WorkflowDescriptor>();
            commService.Setup(s => s.GetResources(It.IsAny<string>())).Returns(descriptors);
            commService.Setup(s => s.GetDataListInputs(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<DataListVariable>());
            vm.CommunicationService = commService.Object;
            return vm;
        }
    }
}
