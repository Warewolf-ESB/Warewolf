using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.WCFEndPoint;
using Dev2.Activities.WcfEndPoint;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Testing;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;

namespace Dev2.Activities.Designers.Tests.WCFEndPoint
{
    [TestClass]
    public class WcfEndPointViewModelTests
    {
        public const string TestOwner = "Bernardt Joubert";
        public const string Category = "WcfService";

        private ModelItem CreateModelItem()
        {
            var wcfActivity = new DsfWcfEndPointActivity();

            return ModelItemUtils.CreateModelItem(wcfActivity);
        }

        private static readonly Guid Id = Guid.NewGuid();
        private static Mock<IWcfServiceModel> SetupEmptyMockSource()
        {
            var ps = new Mock<IWcfServiceModel>();
            ps.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IWcfServerSource>() { new WcfServiceSourceDefinition() { Id = Id } });
            ps.Setup(a => a.GetActions(It.IsAny<IWcfServerSource>())).Returns(new ObservableCollection<IWcfAction>() { new WcfAction() { FullName = "bob", Inputs = new List<IServiceInput>() } });
            return ps;
        }
        public WcfEndPointViewModel GetViewModel()
        {
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ActiveServer).Returns(new ServerForTesting(new Mock<IExplorerRepository>()));
            CustomContainer.Register(mockShellViewModel.Object);
            var ps = SetupEmptyMockSource();
            var outputRegion = new OutputsRegion()
            {
                Outputs = new List<IServiceOutputMapping>()
                {
                    new ServiceOutputMapping()
                    {
                        MappedFrom = "Test",
                        MappedTo = "20",
                        RecordSetName = "TestOutPut"
                    }
                }
            };
            return new WcfEndPointViewModel(CreateModelItem(),ps.Object) {OutputsRegion = outputRegion};
        }
        private static WcfServiceSourceDefinition GetSource()
        {
            return new WcfServiceSourceDefinition()
            {
                Name = "test",
                ResourceID = Guid.NewGuid(),
                EndpointUrl = "test",
                Id = Guid.NewGuid(),
                ResourceName = "Test",
                ResourceType = "WcfSource",

            };
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_CreateNewInstance_ReturnsSuccess()
        {
            var model = GetViewModel();

            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WcfEndPointViewModel_Handle")]
        public void WcfEndPointViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = GetViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_ClearValidation_Success()
        {
            var model = GetViewModel();
            model.ClearValidationMemoWithNoFoundError();
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_SetRegion_Vizibility_Success()
        {
            var model = GetViewModel();
            model.SetRegionVisibility(true);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_SetException_Success()
        {
            var model = GetViewModel();
            model.ErrorMessage(new Exception("Error"),true );
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_SetDisplayName_Success()
        {
            var model = GetViewModel();
            model.ModelItem.SetProperty("DisplayName","DsfWcf -connector");
            model.SetDisplayName("dsf-wcf");
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_ToModel_Success()
        {
            var model = GetViewModel();
            model.SourceRegion.SelectedSource = GetSource();

            model.InputArea.Inputs = new List<IServiceInput>
            {
                new ServiceInput {Name = "test", TypeName = typeof (string).FullName}
            };
          
            model.ToModel();
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_GenerateOutputs_Success()
        {
            var model = GetViewModel();
            model.GenerateOutputsVisible = true;

            Assert.IsTrue(model.GenerateOutputsVisible);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_GenerateOutputsFalse_Success()
        {
            var model = GetViewModel();
            model.GenerateOutputsVisible = false;

            Assert.IsFalse(model.GenerateOutputsVisible);
            Assert.IsNotNull(model.WorstDesignError);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_InitialiseProperties_Success()
        {
            var model = GetViewModel();
            
            Assert.IsNotNull(model.LabelWidth);
            Assert.IsNotNull(model.NoError);
            Assert.IsNotNull(model.TestInputCommand);
            Assert.IsNotNull(model.FixErrorsCommand);
            Assert.IsNotNull(model.ButtonDisplayValue);
            Assert.IsNotNull(model.WorstError);
            Assert.IsNotNull(model.IsWorstErrorReadOnly);

        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_Validate_Success()
        {
            var model = GetViewModel();

            model.Validate();

            Assert.IsNotNull(model.IsWorstErrorReadOnly);

        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfEndPointViewModel_TestProcedure_Success()
        {
            var model = GetViewModel();
            model.ActionRegion.SelectedAction = new WcfAction()
            {
                FullName = "test",
                Method = "test",
                Inputs = new List<IServiceInput>(),
                ReturnType = typeof(string),
                Variables = new List<INameValue>()
            };
            model.TestProcedure();

            Assert.IsNotNull(model.IsWorstErrorReadOnly);
            Assert.AreEqual("test", model.ActionRegion.SelectedAction.FullName);
            Assert.AreEqual("test", model.ActionRegion.SelectedAction.Method);
            Assert.AreEqual(typeof(string), model.ActionRegion.SelectedAction.ReturnType);
            Assert.AreEqual(0, model.ActionRegion.SelectedAction.Variables.Count);
        }
    }
}
