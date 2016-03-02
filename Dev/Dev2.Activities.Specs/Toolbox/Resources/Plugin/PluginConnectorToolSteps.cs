using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Net_DLL;
using Dev2.Collections;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
        [Binding]
    public class PluginConnectorToolSteps
    {
        [Given(@"I open New Plugin Tool")]
        public void GivenIOpenNewPluginTool()
        {
            var activity = new DsfDotNetDllActivity();
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var mockServiceInputViewModel = new Mock<IManagePluginServiceInputViewModel>();
            var mockDbServiceModel = new Mock<IPluginServiceModel>();
            var mockEnvironmentRepo = new Mock<IEnvironmentRepository>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveEnvironment).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockEnvironmentModel.Object);

           var src = new PluginSourceDefinition()
            {
                Name = "Echo",
                Id = Guid.NewGuid(),
                Path = "k:\\bob.dll",
                SelectedDll = new FileListing(new FileListing(){FullName = "Echo.dll", Name = "Echo.dll"})
            };

            var srcs = new ObservableCollection<IPluginSource> { src };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(srcs);
            var eck = new NamespaceItem() { FullName = "Echo Me" };
            ICollection<INamespaceItem> ns = new List<INamespaceItem>{eck,new NamespaceItem(){FullName = "Person"}};
            mockDbServiceModel.Setup(a => a.GetNameSpaces(src)).Returns(ns);
            mockDbServiceModel.Setup(a => a.GetActions(src,eck)).Returns(new List<IPluginAction>{new PluginAction(){FullName = "Echome"},new PluginAction(){FullName = "GetPeople"}});
            mockServiceInputViewModel.SetupAllProperties();
            var resource = new Mock<IContextualResourceModel>();
            resource.Setup(a => a.GetErrors(It.IsAny<Guid>())).Returns(new ObservableReadOnlyList<IErrorInfo>());
            var sqlServerDesignerViewModel = new DotNetDllViewModel(modelItem, mockDbServiceModel.Object);
           //PrivateObject po = new PrivateObject(sqlServerDesignerViewModel.RootModel);
           //po.SetField("_errors",new  ObservableReadOnlyList<IErrorInfo>());
            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Then(@"""(.*)"" combobox is enabled")]
        public void ThenComboboxIsEnabled(string p0)
        {
            var vm =ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            Assert.IsTrue(vm.SourceRegion.IsVisible);
        }

        [Then(@"Selected Source is null")]
        public void ThenSelectedSourceIsNull()
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            Assert.IsNull(vm.SourceRegion.SelectedSource);
        }

        [Then(@"Selected Namespace is Null")]
        public void ThenSelectedNamespaceIsNull()
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            Assert.IsNull(vm.NamespaceRegion.SelectedNamespace);
        }

        [Then(@"Selected Method is Null")]
        public void ThenSelectedMethodIsNull()
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            Assert.IsNull(vm.ActionRegion.SelectedAction);
        }

        [Then(@"Inputs are")]
        public void ThenInputsAre(Table table)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            if (table.Rows.Count == 0)
            {
                if (vm.InputArea.Inputs!= null)
                Assert.IsTrue(vm.InputArea.Inputs.Count==0);
            }
            else
            {
                var matched = table.Rows.Zip(vm.InputArea.Inputs, (a, b) => new Tuple<TableRow, IServiceInput>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual(a.Item1[0], a.Item2.Name);
                    Assert.AreEqual(a.Item1[1], a.Item2.Value);
                    Assert.AreEqual(bool.Parse( a.Item1[2]), a.Item2.EmptyIsNull);
                    Assert.AreEqual(bool.Parse(a.Item1[3]), a.Item2.RequiredField);
                }
            }
        }

        [Then(@"Outputs are")]
        public void ThenOutputsAre(Table table)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            if (table.Rows.Count == 0)
            {
                if(vm.OutputsRegion.Outputs!=null)
                Assert.AreEqual(vm.OutputsRegion.Outputs.Count,0);
            }
            else
            {
                var matched = table.Rows.Zip(vm.OutputsRegion.Outputs, (a, b) => new Tuple<TableRow, IServiceOutputMapping>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual(a.Item1[0], a.Item2.MappedFrom);
                    Assert.AreEqual(a.Item1[1], a.Item2.MappedTo);

                }
            }
        }

        [Then(@"Recordset is ""(.*)""")]
        public void ThenRecordsetIs(string p0)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            Assert.IsTrue(String.IsNullOrEmpty(vm.OutputsRegion.RecordsetName));
        }

        [Then(@"there are ""(.*)"" validation errors of ""(.*)""")]
        public void ThenThereAreValidationErrorsOf(string p0, string p1)
        {
            if (p0.ToLower() == "no")
            {
                var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
                Assert.IsTrue(vm.Errors==null||vm.Errors.Count == 0);
            }
            else
            {


                var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
                Assert.AreEqual(String.Join(Environment.NewLine, vm.Errors.Select(a => a.Message)), p1);
            }
        }

        [When(@"I select the Source ""(.*)""")]
        public void WhenISelectTheSource(string p0)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            vm.SourceRegion.SelectedSource = vm.SourceRegion.Sources.FirstOrDefault(a => a.Name == p0);
        }

        [Then(@"Selected Source is ""(.*)""")]
        public void ThenSelectedSourceIs(string p0)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
           Assert.AreEqual(vm.SourceRegion.SelectedSource.Name,p0);
        }

        [Then(@"the Namespaces are")]
        public void ThenTheNamespacesAre(Table table)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            int i = 0;
            foreach(var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow[0] ,    vm.NamespaceRegion.Namespaces.ToArray()[i].FullName               );
                i++;
            }
        }

        [When(@"I select the NameSpace ""(.*)""")]
        public void WhenISelectTheNameSpace(string p0)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            vm.NamespaceRegion.SelectedNamespace = vm.NamespaceRegion.Namespaces.FirstOrDefault(a => a.FullName == p0);
        }

        [Then(@"Selected Namespace is ""(.*)""")]
        public void ThenSelectedNamespaceIs(string p0)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            Assert.AreEqual(vm.NamespaceRegion.SelectedNamespace.FullName,p0);
        }

        [Then(@"the available methods in the dropdown are")]
        public void ThenTheAvailableMethodsInTheDropdownAre(Table table)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            int i = 0;
            foreach (var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow[0], vm.ActionRegion.Actions.ToArray()[i].FullName);
                i++;
            }
        }


        [When(@"I select the Method ""(.*)""")]
        public void WhenISelectTheMethod(string p0)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            vm.ActionRegion.SelectedAction = vm.ActionRegion.Actions.FirstOrDefault(a => a.FullName == p0);
        }

        [Then(@"Selected Method is ""(.*)""")]
        public void ThenSelectedMethodIs(string p0)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            Assert.AreEqual(vm.ActionRegion.SelectedAction.FullName, p0);
        }

        [Then(@"The available methods are")]
        public void ThenTheAvailableMethodsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Validate is ""(.*)""")]
        public void ThenValidateIs(string p0)
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            Assert.AreEqual(vm.InputArea.IsVisible, p0.ToLower()=="enabled");
        }

        [Given(@"I open Saved Plugin Tool")]
        public void GivenIOpenSavedPluginTool()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I refresh the namespaces and there is no change")]
        public void WhenIRefreshTheNamespacesAndThereIsNoChange()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I set the RecordSet to ""(.*)""")]
        public void WhenISetTheRecordSetTo(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I validate Sucessfully")]
        public void WhenIValidateSucessfully()
        {
            var vm = ScenarioContext.Current.Get<DotNetDllViewModel>("viewModel");
            vm.TestProcedure();
         
        }


    }
}
