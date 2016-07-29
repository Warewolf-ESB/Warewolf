using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Activities;
using Dev2.Activities.Designers2.ComDLL;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;

namespace Warewolf.AcceptanceTesting.ComDll
{
    [Binding]
    public class ComDllSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public ComDllSteps(ScenarioContext scenarioContext)
        {
            this._scenarioContext = scenarioContext;
        }

        [Given(@"I create New Workflow")]
        public void GivenICreateNewWorkflow()
        {
            var dsfComDllActivity = new DsfComDllActivity();
            Assert.IsNotNull(dsfComDllActivity);
        }

        [Given(@"I drag Comdll tool onto the design surface")]
        public void GivenIDragComdllToolOntoTheDesignSurface()
        {
            var comDllActivity = new DsfComDllActivity();

            var modelItem = ModelItemUtils.CreateModelItem(comDllActivity);
            var pluginServiceModel = new Mock<IComPluginServiceModel>();
            var plugInSources = new Mock<ObservableCollection<IComPluginSource>>();            
            pluginServiceModel.Setup(serviceModel => serviceModel.RetrieveSources())
                .Returns(plugInSources.Object);
            var viewModel = new ComDllViewModel(modelItem, pluginServiceModel.Object);
            var privateObject = new PrivateObject(comDllActivity);

            _scenarioContext.Add("ViewModel", viewModel);
            _scenarioContext.Add("Model", pluginServiceModel);
            _scenarioContext.Add("Activity", comDllActivity);
            _scenarioContext.Add("PrivateObj", privateObject);
            _scenarioContext.Add("PlugInSources", plugInSources);
        }

        [Given(@"EditButton is Disabled")]
        public void GivenEditButtonIsDisabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsFalse(vm.SourceRegion.EditSourceCommand.CanExecute(null));
        }

        [Then(@"EditButton is Enabled")]
        public void ThenEditButtonIsEnabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.SourceRegion.EditSourceCommand.CanExecute(null));
        }
        [Given(@"Comdll Source is Enabled")]
        public void GivenComdllSourceIsEnabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.SourceRegion.NewSourceCommand.CanExecute(null));
        }

        [Given(@"Namespace is disabled")]
        public void GivenNamespaceIsDisabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsFalse(vm.NamespaceRegion.IsNamespaceEnabled);
        }

        [Then(@"Namespace is Enabled")]
        public void ThenNamespaceIsEnabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.NamespaceRegion.IsNamespaceEnabled);
        }
        [Given(@"Namespace refresh is disabled")]
        public void GivenNamespaceRefreshIsDisabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsFalse(vm.NamespaceRegion.RefreshNamespaceCommand.CanExecute(null));
        }

        [Given(@"Action is disabled")]
        public void GivenActionIsDisabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.ActionRegion.IsActionEnabled);
        }


        [Then(@"Action is Enabled")]
        public void ThenActionIsEnabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.ActionRegion.IsActionEnabled);
        }

        [Given(@"Action refresh is disabled")]
        public void GivenActionRefreshIsDisabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsFalse(vm.ActionRegion.RefreshActionsCommand.CanExecute(null));
        }

        [Given(@"Action refresh is Enabled")]
        public void GivenActionRefreshIsEnabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.ActionRegion.RefreshActionsCommand.CanExecute(null));
        }

        [Given(@"New button is Enabled")]
        public void GivenNewButtonIsEnabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.SourceRegion.NewSourceCommand.CanExecute(null));
        }

        [When(@"I click new source")]
        public void WhenIClickNewSource()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            vm.SourceRegion.NewSourceCommand.Execute(null);
        }

        [Then(@"the Comdll Source window is opened")]
        public void ThenTheComdllSourceWindowIsOpened()
        {
            var model = _scenarioContext.Get<Mock<IComPluginServiceModel>>("Model");
            model.Verify(a => a.CreateNewSource(), Times.AtLeastOnce);
        }

        [When(@"I select ""(.*)"" from source list as the source")]
        public void WhenISelectFromSourceListAsTheSource(string source)
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            vm.SourceRegion.SelectedSource = SourceDefinitions().FirstOrDefault(definition => definition.Name == source);
        }

        private IEnumerable<IComPluginSource> SourceDefinitions()
        {
            return new List<IComPluginSource>(new[]
            {
                new ComPluginSourceDefinition
                {
                    Name = "ComDllSource"
                    ,
                    ClsId = ""
                    ,
                    Id = Guid.NewGuid()
                    ,
                    ProgId = ""
                    ,
                    Path = @"C:\Development\Dev\Binaries\MS Fakes\Microsoft.QualityTools.Testing.Fakes.dll"
                }
            });
        }        

        [Then(@"I click Edit source")]
        public void ThenIClickEditSource()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            vm.SourceRegion.EditSourceCommand.Execute(null);
            vm.SourceRegion.SelectedSource = SourceDefinitions().FirstOrDefault(definition => definition.Name == "ComDllSource");
        }

        [Then(@"the Comdll Source window is opened with ComDllSource source")]
        public void ThenTheComdllSourceWindowIsOpenedWithComDllSourceSource()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            vm.SourceRegion.SelectedSource = SourceDefinitions().FirstOrDefault(definition => definition.Name == "ComDllSource");
            var model = _scenarioContext.Get<Mock<IComPluginServiceModel>>("Model");
            model.Verify(a => a.EditSource(vm.SourceRegion.SelectedSource), Times.AtLeastOnce);
        }

        [Given(@"I execute tool without a source")]
        public void GivenIExecuteToolWithoutASource()
        {
            ScenarioContext.Current.Pending();
        }        

        [When(@"I hit F-six to execute tool")]
        public void WhenIHitF_SixToExecuteTool()
        {            
            var privateObject = _scenarioContext.Get<PrivateObject>("PrivateObj");
            var parameters = new Dictionary<string, string>();
            var executeResults = privateObject.Invoke("ExecuteService", parameters);
            Assert.IsNotNull(executeResults);
        }

        [Then(@"Empty source error is Returned")]
        public void ThenEmptySourceErrorIsReturned()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"GenerateOutput is disabled")]
        public void GivenGenerateOutputIsDisabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsFalse(vm.GenerateOutputsVisible);
        }

        [Then(@"GenerateOutput is disabled")]
        public void ThenGenerateOutputIsDisabled()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsFalse(vm.GenerateOutputsVisible);
        }

        [Then(@"I select Action")]
        public void ThenISelectAction()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            vm.ActionRegion.SelectedAction = ActionDefinitions().FirstOrDefault(definition => definition.FullName == "ToString");
        }

        private IEnumerable<IPluginAction> ActionDefinitions()
        {
            return new List<IPluginAction>
            {
                new PluginAction
                {
                    FullName = "ToString"
                }
            };
        }

        [Then(@"I click Generate output")]
        public void ThenIClickGenerateOutput()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.InputArea.IsEnabled);
            vm.TestProcedure();            
            Assert.IsNotNull(vm.ManageServiceInputViewModel);
            Assert.IsTrue(vm.GenerateOutputsVisible);
            Assert.IsFalse(vm.InputArea.IsEnabled);
        }

        [Then(@"Inputs windo is open")]
        public void ThenInputsWindoIsOpen()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.OutputsRegion.OutputMappingEnabled);
        }

        [Then(@"Test is Enabled")]
        public void ThenTestIsEnabled()
        {
            //var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            //vm.TestProcedure();
        }
    }
}
