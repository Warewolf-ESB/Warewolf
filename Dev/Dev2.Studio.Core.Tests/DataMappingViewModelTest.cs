using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests {
    /// <summary>
    ///This is a result class for DataMappingViewModelTest and is intended
    ///to contain all DataMappingViewModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataMappingViewModelTest {
        Mock<IWebActivity> _mockWebActivity = new Mock<IWebActivity>();
        Mock<IContextualResourceModel> _mockresource = new Mock<IContextualResourceModel>(); 
        Mock<IDataListViewModel> _mockDataListViewModel = new Mock<IDataListViewModel>();
        IList<IInputOutputViewModel> _outputInOutList = new List<IInputOutputViewModel>();
        IList<IInputOutputViewModel> _inputInOutList = new List<IInputOutputViewModel>();

        DataMappingViewModel _dataMappingViewModel;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional result attributes

        //Use TestInitialize to run code before running each result
        [TestInitialize()]
        public void MyTestInitialize() 
        {

           // ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

            IList<IDev2Definition> inputDev2defList = new List<IDev2Definition>();

            Mock<IDev2Definition> _mockDev2DefIn1 = Dev2MockFactory.SetupIDev2Definition("reg", "", "", "", "", true, false, true, "NUD2347");
            Mock<IDev2Definition> _mockDev2DefIn2 = Dev2MockFactory.SetupIDev2Definition("asdfsad", "registration223", "", "", "registration223", true, false, true, "w3rt24324");
            Mock<IDev2Definition> _mockDev2DefIn3 = Dev2MockFactory.SetupIDev2Definition("number", "", "", "", "", false, false, true, "");

            inputDev2defList.Add(_mockDev2DefIn1.Object);
            inputDev2defList.Add(_mockDev2DefIn2.Object);
            inputDev2defList.Add(_mockDev2DefIn3.Object);

            IList<IDev2Definition> outputDev2defList = new List<IDev2Definition>();

            Mock<IDev2Definition> _mockDev2DefOut1 = Dev2MockFactory.SetupIDev2Definition("vehicleVin", "", "","", "VIN", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut2 = Dev2MockFactory.SetupIDev2Definition("vehicleColor", "", "", "", "vehicleColor", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut3 = Dev2MockFactory.SetupIDev2Definition("Fines", "", "", "", "", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut4 = Dev2MockFactory.SetupIDev2Definition("speed", "", "Fines", "", "speed", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut5 = Dev2MockFactory.SetupIDev2Definition("date", "Fines.Date", "", "", "date", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut6 = Dev2MockFactory.SetupIDev2Definition("location", "", "Fines", "", "location", false, false, true, "");                      

            outputDev2defList.Add(_mockDev2DefOut1.Object);
            outputDev2defList.Add(_mockDev2DefOut2.Object);
            outputDev2defList.Add(_mockDev2DefOut3.Object);
            outputDev2defList.Add(_mockDev2DefOut4.Object);
            outputDev2defList.Add(_mockDev2DefOut5.Object);
            outputDev2defList.Add(_mockDev2DefOut6.Object);

            _inputInOutList = new List<IInputOutputViewModel>();
            Mock<IInputOutputViewModel> _mockInOutVm1 = Dev2MockFactory.SetupIInputOutputViewModel("reg", "", "", false, "", true, "reg", "NUD2347");
            Mock<IInputOutputViewModel> _mockInOutVm2 = Dev2MockFactory.SetupIInputOutputViewModel("asdfsad", "registration223", "", false, "registration223", true, "asdfsad", "w3rt24324");
            Mock<IInputOutputViewModel> _mockInOutVm3 = Dev2MockFactory.SetupIInputOutputViewModel("number", "", "", false, "", false, "number", "");
            _inputInOutList.Add(_mockInOutVm1.Object);
            _inputInOutList.Add(_mockInOutVm2.Object);
            _inputInOutList.Add(_mockInOutVm3.Object);

            _outputInOutList = new List<IInputOutputViewModel>();
            Mock<IInputOutputViewModel> _mockInOutVm4 = Dev2MockFactory.SetupIInputOutputViewModel("vehicleVin", "", "", false, "VIN", false, "vehicleVin", "");
            Mock<IInputOutputViewModel> _mockInOutVm5 = Dev2MockFactory.SetupIInputOutputViewModel("vehicleColor", "", "", false, "", true, "vehicleColor", "");
            Mock<IInputOutputViewModel> _mockInOutVm6 = Dev2MockFactory.SetupIInputOutputViewModel("Fines", "", "", false, "", false, "Fines", "");
            Mock<IInputOutputViewModel> _mockInOutVm7 = Dev2MockFactory.SetupIInputOutputViewModel("speed", "", "Fines", false, "", false, "speed", "NUD2347");
            Mock<IInputOutputViewModel> _mockInOutVm8 = Dev2MockFactory.SetupIInputOutputViewModel("date", "Fines.Date", "Fines", false, "registration223", true, "date", "w3rt24324");
            Mock<IInputOutputViewModel> _mockInOutVm9 = Dev2MockFactory.SetupIInputOutputViewModel("location", "", "Fines", false, "", false, "location", "");
            _outputInOutList.Add(_mockInOutVm4.Object);
            _outputInOutList.Add(_mockInOutVm5.Object);
            _outputInOutList.Add(_mockInOutVm6.Object);
            _outputInOutList.Add(_mockInOutVm7.Object);
            _outputInOutList.Add(_mockInOutVm8.Object);
            _outputInOutList.Add(_mockInOutVm9.Object);

            _mockresource.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            _mockresource.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            _mockresource.Setup(res => res.ResourceType).Returns(ResourceType.Service);
            _mockresource.Setup(res => res.Environment).Returns(Dev2MockFactory.SetupEnvironmentModel().Object);

            _mockDataListViewModel.Setup(dlvm => dlvm.Resource).Returns(_mockresource.Object);
            DataListSingleton.SetDataList(_mockDataListViewModel.Object);

            _mockWebActivity.SetupAllProperties();
            _mockWebActivity.Setup(activity => activity.XMLConfiguration).Returns(StringResourcesTest.WebActivity_XmlConfig);
            _mockWebActivity.Object.SavedInputMapping = StringResourcesTest.WebActivity_SavedInputMapping;
            _mockWebActivity.Object.SavedOutputMapping = StringResourcesTest.WebActivity_SavedOutputMapping;
            _mockWebActivity.Object.LiveInputMapping = StringResourcesTest.WebActivity_LiveInputMapping;
            _mockWebActivity.Object.LiveOutputMapping = StringResourcesTest.WebActivity_LiveOutputMapping;
            _mockWebActivity.Object.ServiceName = "MyTestActivity";
            _mockWebActivity.Object.ResourceModel = _mockresource.Object;

            _mockWebActivity.Setup(activity => activity.UnderlyingWebActivityObjectType).Returns(typeof(DsfWebPageActivity));
            _dataMappingViewModel = new DataMappingViewModel(_mockWebActivity.Object);

        }

        #endregion

        /// <summary>
        ///test the CreateXmlOutput method
        ///</summary>
        [TestMethod()]   
        public void CreateXmlOutput() {                        
            _dataMappingViewModel.CreateXmlOutput(_dataMappingViewModel.Outputs, _dataMappingViewModel.Inputs);
            string result1 = _dataMappingViewModel.Activity.LiveInputMapping;
            string result2 = _dataMappingViewModel.Activity.LiveOutputMapping;
            string expected1 = @"<Inputs><Input Name=""reg"" Source=""[[reg]]"" DefaultValue=""NUD2347""><Validator Type=""Required"" /></Input><Input Name=""asdfsad"" Source=""registration223"" DefaultValue=""w3rt24324""><Validator Type=""Required"" /></Input><Input Name=""number"" Source=""[[number]]"" /></Inputs>";
            string expected2 = @"<Outputs><Output Name=""vehicleVin"" MapsTo=""[[vehicleVin]]"" Value=""[[vehicleVin]]"" /><Output Name=""vehicleColor"" MapsTo=""[[vehicleColor]]"" Value=""[[vehicleColor]]"" /><Output Name=""speed"" MapsTo=""[[Fines(*).speed]]"" Value=""[[Fines().speed]]"" Recordset=""Fines"" /><Output Name=""date"" MapsTo=""Fines.Date"" Value=""Fines.Date"" Recordset=""Fines"" /><Output Name=""location"" MapsTo=""[[Fines(*).location]]"" Value=""[[Fines().location]]"" Recordset=""Fines"" /></Outputs>";
            Assert.AreEqual(expected1, result1);
            Assert.AreEqual(expected2, result2);
        }

        /// <summary>
        ///A result for DataMappingViewModel Constructor
        ///</summary>
        [TestMethod()]
        public void SetMainViewModel_InitialCall_ExpectedInputOutputFieldsMappedFromServiceDefinition()
        {
            Assert.AreEqual(3, _dataMappingViewModel.Inputs.Count);
            Assert.AreEqual(5, _dataMappingViewModel.Outputs.Count);
        }
  
        [TestMethod()]
        public void Given_OnlyInputsInDataList_Expected_ValuesOfModifiedInputsPersist()
        {
            Mock<IWebActivity> mockAct = new Mock<IWebActivity>();
            mockAct.SetupAllProperties();
            mockAct.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            mockAct.Setup(c => c.ResourceModel).Returns(Dev2MockFactory.SetupResourceModelWithOnlyInputsMock().Object);

            string value = "[[123]]";

            DataMappingViewModel dataMappingViewModel = new DataMappingViewModel(mockAct.Object);
            dataMappingViewModel.Inputs[0].MapsTo = value;
            dataMappingViewModel.CreateXmlOutput(dataMappingViewModel.Outputs, dataMappingViewModel.Inputs);

            DataMappingViewModel newDataMappingViewModel = new DataMappingViewModel(mockAct.Object);

            Assert.IsTrue(newDataMappingViewModel.Inputs.Count(o => o.MapsTo == value) > 0);
        }

        [TestMethod()]
        public void Given_OnlyOutputsInDataList_Expected_ValuesOfModifiedOutputsPersist()
        {
            Mock<IWebActivity> mockAct = new Mock<IWebActivity>();
            mockAct.SetupAllProperties();
            mockAct.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            mockAct.Setup(c => c.ResourceModel).Returns(Dev2MockFactory.SetupResourceModelWithOnlyOuputsMock().Object);

            string value = "[[123]]";

            DataMappingViewModel dataMappingViewModel = new DataMappingViewModel(mockAct.Object);
            dataMappingViewModel.Outputs[0].Value = value;
            dataMappingViewModel.CreateXmlOutput(dataMappingViewModel.Outputs.ToList(), dataMappingViewModel.Inputs.ToList());
            
            DataMappingViewModel newDataMappingViewModel = new DataMappingViewModel(mockAct.Object);

            Assert.IsTrue(newDataMappingViewModel.Outputs.Count(o => o.Value == value) > 0);
        }

        [TestMethod()]
        public void Given_InputsAndOutputsInDataList_Expected_ValuesOfModifiedInputsAndOutputsPersist()
        {
            Mock<IWebActivity> mockAct = new Mock<IWebActivity>();
            mockAct.SetupAllProperties();
            mockAct.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            mockAct.Setup(c => c.ResourceModel).Returns(Dev2MockFactory.SetupResourceModelMock().Object);

            string inputValue = "[[123]]";
            string outputValue = "[[1234]]";

            DataMappingViewModel dataMappingViewModel = new DataMappingViewModel(mockAct.Object);
            dataMappingViewModel.Inputs[0].MapsTo = inputValue;
            dataMappingViewModel.Outputs[0].Value = outputValue;
            dataMappingViewModel.CreateXmlOutput(dataMappingViewModel.Outputs.ToList(), dataMappingViewModel.Inputs.ToList());

            DataMappingViewModel newDataMappingViewModel = new DataMappingViewModel(mockAct.Object);

            Assert.IsTrue(newDataMappingViewModel.Inputs.Count(o => o.MapsTo == inputValue) > 0);
            Assert.IsTrue(newDataMappingViewModel.Outputs.Count(o => o.Value == outputValue) > 0);
        }
    }
}
