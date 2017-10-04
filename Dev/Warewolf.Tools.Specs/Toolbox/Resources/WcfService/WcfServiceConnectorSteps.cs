using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using ActivityUnitTests;
using Dev2.Activities.Designers2.WCFEndPoint;
using Dev2.Activities.WcfEndPoint;
using Dev2.Collections;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Unlimited.Framework.Converters.Graph.Ouput;
using Warewolf.Core;
using Dev2.Common.Interfaces.Core;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Specs.Toolbox.WcfService
{
    [Binding]
    public class WcfServiceConnectorSteps : BaseActivityUnitTest
    {
        private readonly ScenarioContext scenarioContext;

        public WcfServiceConnectorSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        public WcfSource GetSource()
        {
            return new WcfSource()
            {
                EndpointUrl = scenarioContext.Get<string>("EndPointUrl"),
                ResourceID = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                Name = "Testwcf",
                ResourceName = "Wcftest"
            };
        }

        IWcfAction GetAction()
        {
            return new WcfAction()
            {
                FullName = scenarioContext.Get<string>("MethodName"),
                Method = scenarioContext.Get<string>("MethodName"),
            };
        }

        public DsfWcfEndPointActivity GetActivity()
        {
            var inputs = scenarioContext.Get<Table>("Inputs");
            var serviceInputs = new List<IServiceInput>();

            foreach (var row in inputs.Rows)
            {
                foreach (var r in row)
                {
                    serviceInputs.Add(new ServiceInput()
                    {
                        Name = r.Key,
                        Value = r.Value,
                        TypeName = typeof(string).FullName
                    });
                }
            }

            return new DsfWcfEndPointActivity()
            {
                Source = GetSource(),
                Method = GetAction(),
                Inputs = serviceInputs,
                OutputDescription = new OutputDescription()
            };
        }

        [Given(@"I open New Wcf Tool")]
        public void GivenIOpenNewWcfTool()
        {
            var activity = new DsfWcfEndPointActivity();
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var mockServiceInputViewModel = new Mock<IManageWcfSourceViewModel>();
            var mockDbServiceModel = new Mock<IWcfServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            var src = new WcfServiceSourceDefinition()
            {
                Name = "Echo",
                Id = Guid.NewGuid(),
                Path = "k:\\bob.dll",
                EndpointUrl = "Https:/localhost"
            };

            var srcs = new ObservableCollection<IWcfServerSource> { src };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(srcs);
            mockDbServiceModel.Setup(a => a.GetActions(src)).Returns(new List<IWcfAction> { new WcfAction() { FullName = "Echome" } });
            mockServiceInputViewModel.SetupAllProperties();
            var resource = new Mock<IContextualResourceModel>();
            resource.Setup(a => a.GetErrors(It.IsAny<Guid>())).Returns(new ObservableReadOnlyList<IErrorInfo>());
            var sqlServerDesignerViewModel = new WcfEndPointViewModel(modelItem, mockDbServiceModel.Object);
          
            scenarioContext.Add("viewModel", sqlServerDesignerViewModel);
            scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Then(@"""(.*)"" wcf combobox is enabled")]
        public void ThenWcfComboboxIsEnabled(string p0)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            Assert.IsTrue(vm.SourceRegion.IsEnabled);
        }
        [Then(@"Selected wcf Source is null")]
        public void ThenSelectedWcfSourceIsNull()
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            Assert.IsNull(vm.SourceRegion.SelectedSource);
        }

        [Then(@"Selected wcf Method is Null")]
        public void ThenSelectedWcfMethodIsNull()
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            Assert.IsNull(vm.ActionRegion.SelectedAction);
        }

        [Then(@"wcf Inputs are")]
        public void ThenWcfInputsAre(Table table)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            if (table.Rows.Count == 0)
            {
                if (vm.InputArea.Inputs != null)
                {
                    Assert.IsTrue(vm.InputArea.Inputs.Count == 0);
                }
            }
            else
            {
                var matched = table.Rows.Zip(vm.InputArea.Inputs, (a, b) => new Tuple<TableRow, IServiceInput>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual(a.Item1[0], a.Item2.Name);
                    Assert.AreEqual(a.Item1[1], a.Item2.Value);
                    Assert.AreEqual(bool.Parse(a.Item1[2]), a.Item2.EmptyIsNull);
                    Assert.AreEqual(bool.Parse(a.Item1[3]), a.Item2.RequiredField);
                }
            }
        }

        [Then(@"wcf Outputs are")]
        public void ThenWcfOutputsAre(Table table)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            if (table.Rows.Count == 0)
            {
                if (vm.OutputsRegion.Outputs != null)
                {
                    Assert.AreEqual(vm.OutputsRegion.Outputs.Count, 0);
                }
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
        [Then(@"wcf Recordset is ""(.*)""")]
        public void ThenWcfRecordsetIs(string p0)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            Assert.IsTrue(String.IsNullOrEmpty(vm.OutputsRegion.RecordsetName));
        }

        [Then(@"there are ""(.*)"" wcf validation errors of ""(.*)""")]
        public void ThenThereAreWcfValidationErrorsOf(string p0, string p1)
        {
            if (p0.ToLower() == "no")
            {
                var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
                Assert.IsTrue(vm.Errors == null || vm.Errors.Count == 0);
            }
            else
            {
                var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
                Assert.AreEqual(String.Join(Environment.NewLine, vm.Errors.Select(a => a.Message)), p1);
            }
        }

        [When(@"I select the wcf Source ""(.*)""")]
        public void WhenISelectTheWcfSource(string p0)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            vm.SourceRegion.SelectedSource = vm.SourceRegion.Sources.FirstOrDefault(a => a.Name == p0);
        }
        [Then(@"Selected wcf Source is ""(.*)""")]
        public void ThenSelectedWcfSourceIs(string p0)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            Assert.AreEqual(vm.SourceRegion.SelectedSource.Name, p0);
        }

        [Then(@"I select the wcf Method ""(.*)""")]
        public void ThenISelectTheWcfMethod(string p0)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            vm.ActionRegion.SelectedAction = vm.ActionRegion.Actions.FirstOrDefault(a => a.FullName == p0);
        }
        [Then(@"Selected wcf Method is ""(.*)""")]
        public void ThenSelectedWcfMethodIs(string p0)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            vm.ActionRegion.SelectedAction = vm.ActionRegion.Actions.FirstOrDefault(a => a.FullName == p0);
        }

        [Then(@"the available wcf methods in the dropdown are")]
        public void ThenTheAvailableWcfMethodsInTheDropdownAre(Table table)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
          
        }
        [Then(@"Validate wcf is ""(.*)""")]
        public void ThenValidateWcfIs(string p0)
        {
            var vm = scenarioContext.Get<WcfEndPointViewModel>("viewModel");
            Assert.IsFalse(vm.InputArea.IsEnabled);
        }

    }
}
