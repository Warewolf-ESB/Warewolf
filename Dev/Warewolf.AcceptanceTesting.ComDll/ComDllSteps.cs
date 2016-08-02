using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Designers2.ComDLL;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Unlimited.Framework.Converters.Graph.Ouput;
using Warewolf.Core;
using Warewolf.Storage;
using static System.String;

namespace Warewolf.AcceptanceTesting.ComDll
{
    [Binding]
    public class ComDllSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public ComDllSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
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
            comDllActivity.SourceId = new Guid("f9c016b6-9db4-4971-9634-60295bfc546f");
            var dataObject = new Mock<IDSFDataObject>();
            var channel = new Mock<IEsbChannel>();
            var environment = new Mock<IExecutionEnvironment>();
            var errors = new HashSet<string>();
            environment.SetupGet(executionEnvironment => executionEnvironment.Errors).Returns(errors);
            dataObject.Setup(dObj => dObj.Environment).Returns(environment.Object);
            dataObject.Setup(dObj => dObj.EsbChannel).Returns(channel.Object);
            dataObject.SetupGet(dObj => dObj.WorkspaceID).Returns(Guid.NewGuid);
            var modelItem = ModelItemUtils.CreateModelItem(comDllActivity);
            var pluginServiceModel = new Mock<IComPluginServiceModel>();
            var plugInSources = new Mock<ObservableCollection<IComPluginSource>>();
            pluginServiceModel.Setup(serviceModel => serviceModel.RetrieveSources())
                .Returns(plugInSources.Object);
            var viewModel = new ComDllViewModel(modelItem, pluginServiceModel.Object);
            var privateObject = new PrivateObject(comDllActivity);

            _scenarioContext.Add("ViewModel", viewModel);
            _scenarioContext.Add("DataObject", dataObject.Object);
            _scenarioContext.Add("Channel", channel);
            _scenarioContext.Add("Environment", environment.Object);
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
            vm.SourceRegion.SelectedSource = SourceDefinitions().FirstOrDefault(definition => definition.ResourceName == source);
        }

        private IEnumerable<IComPluginSource> SourceDefinitions()
        {
            return new List<IComPluginSource>(new[]
            {
                new ComPluginSourceDefinition
                {
                    ResourceName = "ComDllSource"
                    ,
                    ClsId = "00000507-0000-0010-8000-00AA006D2EA4"
                    ,
                    Id = new Guid("4ef43652-655e-440a-b25a-0b1eb149ad04")
                    ,
                    ResourcePath = "Test_path"
                }
            });
        }

        [Then(@"I click Edit source")]
        public void ThenIClickEditSource()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            vm.SourceRegion.EditSourceCommand.Execute(null);
            vm.SourceRegion.SelectedSource = SourceDefinitions().FirstOrDefault(definition => definition.ResourceName == "ComDllSource");
        }

        [Then(@"the Comdll Source window is opened with ComDllSource source")]
        public void ThenTheComdllSourceWindowIsOpenedWithComDllSourceSource()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            vm.SourceRegion.SelectedSource = SourceDefinitions().FirstOrDefault(definition => definition.ResourceName == "ComDllSource");
            var model = _scenarioContext.Get<Mock<IComPluginServiceModel>>("Model");
            model.Verify(serviceModel => serviceModel.EditSource(It.IsAny<IComPluginSource>()));
        }

        private static IResource ComPlusgInResource()
        {
            IResource resource = new ComPluginSource();
            return resource;
        }

        [Then(@"Empty source error is Returned")]
        public void ThenEmptySourceErrorIsReturned()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.AreEqual(1, vm.Errors.Count);
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
                    ,
                    Method = "ToString"
                    ,
                    Inputs = ServiceInputs.ToList()
                    
                }
            };
        }

        [Then(@"I click Generate output")]
        public void ThenIClickGenerateOutput()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.InputArea.IsEnabled);
            vm.TestProcedure();
        }

        [Then(@"Inputs windo is open")]
        public void ThenInputsWindoIsOpen()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.IsTrue(vm.OutputsRegion.OutputMappingEnabled);
        }

        [When(@"I click Done to execute tool")]
        public void WhenIClickDoneToExecuteTool()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            vm.ManageServiceInputViewModel.OkCommand.Execute(null);
        }

        [Then(@"Validation is successful")]
        public void ThenValidationIsSuccessful()
        {
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            Assert.AreEqual(0, vm.Errors.Count);
        }
        
        [Then(@"I click fSix to Execute the tool the result is ""(.*)""")]
        public void ThenIClickFSixToExecuteTheToolTheResultIs(string result)
        {
            var dataObject = _scenarioContext.Get<IDSFDataObject>("DataObject");
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            var act = _scenarioContext.Get<DsfComDllActivity>("Activity");
            MyActivity activity = new MyActivity(dataObject, vm, act);
            activity.ExeTool();
            Assert.IsNotNull(activity);
            Assert.AreEqual(result, activity._result);
        }
        [Then(@"I click fSix to Execute the tool")]
        public void ThenIClickFSixToExecuteTheTool()
        {
            var dataObject = _scenarioContext.Get<IDSFDataObject>("DataObject");
            var vm = _scenarioContext.Get<ComDllViewModel>("ViewModel");
            var act = _scenarioContext.Get<DsfComDllActivity>("Activity");
            MyActivity activity = new MyActivity(dataObject, vm, act);
            activity.ExeTool();
        }

        [Then(@"The result is returned with error ""(.*)""")]
        public void ThenTheResultIsReturnedWithError(string errorMessage)
        {
            var environment = _scenarioContext.Get<IExecutionEnvironment>("Environment");
            Assert.AreEqual(1, environment.Errors.Count);
            Assert.AreEqual(errorMessage, environment.Errors.First());
        }


        public ICollection<IServiceInput> ServiceInputs
        {
            get
            {
                return new List<IServiceInput> { new ServiceInput
                {
                    Name = "test"
                    ,
                    Value = "test"
                    ,
                    TypeName = typeof(void).Name
                    
                } };
            }
        }

        private static NamespaceItem CreateNameSpace()
        {
            return new NamespaceItem
            {
                AssemblyLocation = "00000514-0000-0010-8000-00AA006D2EA4",
                FullName = "testing",
                AssemblyName = "testing"
            };
        }
    }

    public class MyActivity : DsfComDllActivity
    {
        private IDSFDataObject _dsfDataObject;
        public string _result = Empty;
        
        public MyActivity(IDSFDataObject dsfDataObject, ComDllViewModel vm, DsfComDllActivity act)
        {           
            OutputDescription = new OutputDescription();
            Outputs = new List<IServiceOutputMapping>();
            Inputs = new List<IServiceInput>();
            SourceId = new Guid("4ef43652-655e-440a-b25a-0b1eb149ad04");
            _dsfDataObject = dsfDataObject;
            ResourceID = Guid.NewGuid();
            Method = vm.ActionRegion.SelectedAction;
        }
        public void ExeTool()
        {
            ExecuteTool(_dsfDataObject, 0);
            _result = base._result;
        }
    }
}
