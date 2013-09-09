using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Web;
using Unlimited.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Dev2.Studio.Core.Interfaces;
using Moq;
using Dev2.Studio.Core;
using Dev2.Composition;


namespace Dev2.Core.Tests {

    /// <summary>
    ///This is a result class for LayoutObjectViewModelTest and is intended
    ///to contain all LayoutObjectViewModelTest Unit Tests
    ///</summary>

    [TestClass()]
    public class LayoutObjectViewModelTest : IHandle<CloseWizardMessage>
    {
        public LayoutGridViewModel LayoutGrid;
        Mock<IEnvironmentModel> _moqEnvironment = new Mock<IEnvironmentModel>();
        Mock<IWebActivity> _test = new Mock<IWebActivity>();
        Mock<IResourceRepository> repo = new Mock<IResourceRepository>();
        int _count;

        TestContext testContextInstance;
        Mock<IEventAggregator> _eventAggregator;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext { get { return testContextInstance; } set { testContextInstance = value; } }

        #region Additional result attributes

        //
        //Use TestInitialize to run code before running each result
        [TestInitialize()]
        public void MyTestInitialize() {

            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();
            _eventAggregator = new Mock<IEventAggregator>();
            EventPublishers.Aggregator = _eventAggregator.Object;
            _eventAggregator.Setup(e => e.Publish(It.IsAny<object>())).Verifiable();

//            EventAggregator = ImportService.GetExportValue<IEventAggregator>();
//            EventAggregator.Subscribe(this);

            _moqEnvironment.Setup(env => env.Connection.WebServerUri).Returns(new Uri("http://localhost:1234"));
            _moqEnvironment.Setup(env => env.Connection.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            _moqEnvironment.Setup(env => env.Name).Returns("Test");
            _moqEnvironment.Setup(env => env.Connect()).Verifiable();
            _moqEnvironment.Setup(env => env.IsConnected).Returns(true);
            _moqEnvironment.Setup(env => env.ResourceRepository).Returns(repo.Object);

            _test.Setup(c => c.XMLConfiguration).Returns("<WebParts/>").Verifiable();
        }

        //
        //Use TestCleanup to run code after each result has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            _count = 0;
            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.CloseWizard);
        }

        //

        #endregion

        #region Dev2Set Tests

        // require input from Brendon on this

        //[TestMethod]
        //public void Dev2Set_Expected_XmlConfigurationUpdated() {
        //    string formData = SetupResponseDataAsXML();
        //    LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
        //    Mock<IWebCommunicationResponse> webResponse = SetupWebCommunicationResponse();
        //    Mock<IWebCommunication> webCommunication = SetupWebCommunicationMock();
        //    Mock<IWebActivity> webActivity = Dev2MockFactory.SetupWebActivityMock();
        //    webActivity.Setup(c => c.XMLConfiguration).Returns(SetupResponseDataAsXML());
        //    ILayoutGridViewModel testParent = CreateLayoutGridVieWModel(webActivity.Object);


        //    webResponse.Setup(c => c.ContentType).Returns("text/xml");
        //    webResponse.Setup(c => c.Content).Returns(formData);
        //    webCommunication.Setup(c => c.Post(It.IsAny<string>(), It.IsAny<string>())).Returns(webResponse.Object);

        //    layoutObjectViewModel.WebCommunication = webCommunication.Object;
        //    testParent.BindXmlConfigurationToGrid();

        //    layoutObjectViewModel.Dev2Set(formData, "http://someServer:1234/server");

        //}

        [TestMethod]
        public void Dev2Set_NoXmlConfiguration_Expected_XmlConfigurationBlanked()
        {
        }

        #endregion Dev2Set Tests

        #region Close Tests

        [TestMethod]
        public void Close_Expected_CloseWizardMediatorMessageSent()
        {

            LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.CloseWizard);
            //Mediator.RegisterToReceiveMessage(MediatorMessages.CloseWizard, input => RecieveMediatorMessage());
            layoutObjectViewModel.Close();
            _eventAggregator.Verify(e => e.Publish(It.Is<CloseWizardMessage>
                (t => t.ResourceWizardViewModel == layoutObjectViewModel)), Times.Once());
            // Assert that the mediator message was sent
            //Assert.AreEqual(1, _count);
        }

        #endregion Close Tests

        #region Cancel Tests

        /// <summary>
        /// Tests that cancel clears the cell contents
        /// </summary>
        [TestMethod]
        public void Cancel_Expected_EmptyXmlConfiguration()
        {
            LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
            layoutObjectViewModel.SetGrid(new LayoutGridViewModel());
            layoutObjectViewModel.XmlConfiguration = string.Empty;
            layoutObjectViewModel.Cancel();
            Assert.AreEqual(string.Empty, layoutObjectViewModel.XmlConfiguration);
        }

        /// <summary>
        /// Tests that cancel restores the cells previous contents
        /// </summary>
        [TestMethod]
        public void Cancel_CellHadContent_Expected_PreviousContentRestoredToCell()
        {
        }

        #endregion Cancel Tests

        #region CopyFrom Tests

        /// <summary>
        /// Tests that CopyFrom correctly copies contents from one cell to another
        /// </summary>
        [TestMethod]
        public void CopyFrom_Expected_CopiedCellContentsInNewCell()
        {
            LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
            var test2 = new Mock<ILayoutObjectViewModel>();
            layoutObjectViewModel.CopyFrom(test2.Object);
            Assert.AreEqual(test2.Object.XmlConfiguration, layoutObjectViewModel.XmlConfiguration);
        }

        /// <summary>
        /// Tests that copying Null object throws exception - ***currently not handled
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void CopyFrom_NullLayoutObject_Expected_CopiedCellContentsInNewCell()
        {
            LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
            var test2 = new Mock<ILayoutObjectViewModel>();
            layoutObjectViewModel.CopyFrom(null);
            Assert.AreEqual(test2.Object.XmlConfiguration, layoutObjectViewModel.XmlConfiguration);
        }

        #endregion CopyFrom Tests

        #region Clear Tests

        /// <summary>
        /// Tests that clear cell removes it's contents
        /// </summary>
        [TestMethod]
        public void Clear_HasConfiguration_Expected_ConfigurationCleared()
        {
            LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
            layoutObjectViewModel.XmlConfiguration = SetupResponseDataAsXML();
            layoutObjectViewModel.Clear(false);
            Assert.AreEqual(string.Empty, layoutObjectViewModel.XmlConfiguration);
        }


        #endregion Clear Tests

        #region ClearCellContents Tests

        /// <summary>
        /// Tests that the contents of a cell are cleared upon calling this method
        /// </summary>
        [TestMethod]
        public void ClearCellContent_ValidCell_Expected_ContentsOfCellAreRemoved()
        {
            LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
            layoutObjectViewModel.XmlConfiguration = SetupResponseDataAsXML();
            layoutObjectViewModel.WebpartServiceName = "MyService";
            layoutObjectViewModel.ClearCellContent(false);
            Assert.IsTrue(layoutObjectViewModel.XmlConfiguration == string.Empty &&
                          layoutObjectViewModel.WebpartServiceName == string.Empty);
        }

        /// <summary>
        /// Tests that the logic behind updating the layoutgridview model is called when this is set to true
        /// </summary>
        [TestMethod]
        public void ClearCellContent_UpdateModelItemTrue_Expectd_ContentsClearedAndUpdateModelItemOnLayoutGridViewModelCalled()
        {
            LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
            layoutObjectViewModel.XmlConfiguration = SetupResponseDataAsXML();
            layoutObjectViewModel.WebpartServiceName = "MyService";
            layoutObjectViewModel.ClearCellContent(false);
            Assert.IsTrue(layoutObjectViewModel.XmlConfiguration == string.Empty &&
                          layoutObjectViewModel.WebpartServiceName == string.Empty);
        }

        #endregion ClearCellContents Tests

        #region Delete Tests

        //Sashen - 18-10-2012 This test requires input from Brendon

        /// <summary>
        /// Tests that the layout grid view model is removed from it's parent when delete is called
        /// </summary>
        //[TestMethod]
        //public void Delete_CurrentLayouObject_Expected_LayoutObjectDeleted() {
        //    LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
        //    var test2 = new Mock<ILayoutObjectViewModel>();
        //    layoutObjectViewModel.Delete(false);
        //    Assert.AreEqual(string.Empty, layoutObjectViewModel.XmlConfiguration);
        //}

        #endregion Delete Tests

        #region ClearPreviousContents Tests

        [TestMethod]
        public void ClearPreviousContents_Expected_PreviousContentsAreEmpty()
        {
            LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
            var test2 = new Mock<ILayoutObjectViewModel>();
            layoutObjectViewModel.PreviousIconPath = "test";
            layoutObjectViewModel.PreviousWebpartServiceName = "testService";
            layoutObjectViewModel.PreviousXmlConfig = "test";
            layoutObjectViewModel.ClearPreviousContents();

            Assert.IsTrue(layoutObjectViewModel.PreviousIconPath == string.Empty
                          && layoutObjectViewModel.PreviousWebpartServiceName == string.Empty
                          && layoutObjectViewModel.PreviousXmlConfig == string.Empty);

        }

        #endregion ClearPreviousContents Tests

        #region ClearAll Tests

        // Test Requires input from Brendon
        /// <summary>
        /// Tests that calling the ClearAll method clears all cell data
        /// </summary>
        //[TestMethod]
        //public void ClearAll_Expected_LayoutGridViewModelHasNoData() {
        //    LayoutObjectViewModel layoutObjectViewModel = new LayoutObjectViewModel();
        //    var test2 = new Mock<ILayoutObjectViewModel>();
        //    layoutObjectViewModel.ClearAll();
        //}

        #endregion Clear All Tests

        #region SetGrid Tests

        /// <summary>
        /// Tests that the layout grid is correctly set by the SetGrid Method
        /// </summary>
        [TestMethod]
        public void SetGrid_EmptyLayoutGrid_Expected_LayouGridCorrectlySet()
        {
            LayoutObjectViewModel layoutObject = new LayoutObjectViewModel();
            LayoutGridViewModel layoutGrid = new LayoutGridViewModel();
            layoutObject.SetGrid(layoutGrid);
            Assert.AreEqual(layoutGrid, layoutObject.LayoutObjectGrid);
        }

        #endregion SetGrid Tests

        #region Internal Test Methods

        Mock<IWebCommunication> SetupWebCommunicationMock()
        {
            Mock<IWebCommunication> webCommunication = new Mock<IWebCommunication>();

            return webCommunication;
        }

        Mock<IWebCommunicationResponse> SetupWebCommunicationResponse()
        {
            Mock<IWebCommunicationResponse> webResponse = new Mock<IWebCommunicationResponse>();

            return webResponse;
        }

        string SetupResponseDataAsXML()
        {
            return @"<WebPage><WebPageServiceName>TestFlow</WebPageServiceName><WebParts><WebPart><WebPartServiceName>Button</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>0</RowIndex><Dev2XMLResult>
                                  <sr>
                                    <sr>
                                      <XmlData>
                                        <Dev2ResumeData>
                                          <XmlData>
                                            <Dev2WebServer>http://localhost:1234</Dev2WebServer>
                                            <XmlData>
                                              <Dev2elementNameButton>ButtonClicked</Dev2elementNameButton>
                                              <displayTextButton>Click! Click!</displayTextButton>
                                              <btnType>submit</btnType>
                                              <customButtonCode></customButtonCode>
                                              <Dev2tabIndexButton></Dev2tabIndexButton>
                                              <Dev2toolTipButton></Dev2toolTipButton>
                                              <Dev2customStyleButton></Dev2customStyleButton>
                                              <Dev2widthButton></Dev2widthButton>
                                              <Dev2heightButton></Dev2heightButton>
                                            </XmlData>
                                            <Async />
                                          </XmlData>
                                        </Dev2ResumeData>
                                      </XmlData>
                                    </sr>
                                  </sr>
                                </Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>0</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>0</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>0</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>1</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>1</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>1</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>1</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>2</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>2</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>2</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>2</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>3</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>3</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>3</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>3</RowIndex></WebPart><Rows>4</Rows><Cols>4</Cols></WebParts></WebPage>";
        }

        ILayoutGridViewModel CreateLayoutGridViewModel()
        {
            ILayoutGridViewModel layoutGridViewModel = new LayoutGridViewModel();
            return layoutGridViewModel;
        }

        ILayoutGridViewModel CreateLayoutGridVieWModel(IWebActivity webActivity)
        {
            ILayoutGridViewModel layoutGridViewModel = new LayoutGridViewModel(webActivity);
            return layoutGridViewModel;
        }

        void RecieveMediatorMessage()
        {
            _count = _count + 1;
        }


        #endregion Internal Test Methods

        #region Implementation of IHandle<CloseWizardMessage>

        public void Handle(CloseWizardMessage message)
        {
            RecieveMediatorMessage();
        }

        #endregion
    }
}
