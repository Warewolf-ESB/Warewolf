
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
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BusinessDesignStudio.Unit.Tests.Unlimited.UnitTest.BusinessDesignStudio
{
    /// <summary>
    /// Summary description for LayoutGridViewModelTest
    /// </summary>
    [TestClass()]
    public class LayoutGridViewModelTest : IHandle<AddWorkSurfaceMessage>
    {

        #region Local Test Variables

        private ILayoutGridViewModel LayoutGrid;
        private Mock<IEnvironmentModel> _mockEnvironment = Dev2MockFactory.SetupEnvironmentModel();
        private Mock<IStudioClientContext> _mockFrameworkDataChannel = Dev2MockFactory.SetupIFrameworkDataChannel();
        private Mock<IWebActivity> _mockWebActivity = new Mock<IWebActivity>();
        private Mock<ILayoutObjectViewModel> _mockLayoutObject = new Mock<ILayoutObjectViewModel>();
        private static ImportServiceContext _importServiceContext;
        bool _addWorkflowDesignerMessageReceived = false;
        #endregion Local Test Variables

        #region Constructor and TestContext
        private TestContext testContextInstance;
        static Mock<IEventAggregator> _aggregator;
        public static readonly object DataListSingletonTestGuard = new object();
        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion Constructor and TestContext

        #region Additional result attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first result in the class

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {

        }

        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        /// <summary>
        /// We are setting MEF up here to retrieve all exports and use them for dependency injection
        /// </summary>
        [TestInitialize()]
        public void EnvironmentTestsInitialize()
        {
            Monitor.Enter(DataListSingletonTestGuard);
            _aggregator = new Mock<IEventAggregator>();

            _aggregator.Setup(e => e.Publish(It.IsAny<object>())).Verifiable();
            _importServiceContext = CompositionInitializer.InitializeMockedMainViewModel();
            ImportService.CurrentContext = _importServiceContext;

            SetupMocks();

            LayoutGrid = new LayoutGridViewModel(_aggregator.Object, _mockWebActivity.Object);

            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.RemoveUnusedDataListItems);
            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddMissingDataListItems);
            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddWorkflowDesigner);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Monitor.Exit(DataListSingletonTestGuard);
        }
        #region Mock Setup

        private void SetupMocks()
        {
            Mock<IContextualResourceModel> _mockRes = new Mock<IContextualResourceModel>();
            _mockRes.Setup(c => c.ResourceName).Returns("result");
            _mockRes.Setup(c => c.Category).Returns("WEBSITE");

            ICollection<IResourceModel> col = new List<IResourceModel>() { _mockRes.Object };

            Mock<IResourceRepository> _mockResRepository = new Mock<IResourceRepository>();
            _mockResRepository.Setup(c => c.All()).Returns(col);

            _mockEnvironment.Setup(env => env.Connection.WebServerUri).Returns(new Uri("http://localhost:1234"));
            _mockEnvironment.Setup(env => env.Connection.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            _mockEnvironment.Setup(env => env.Name).Returns("result");
            _mockEnvironment.Setup(env => env.Connect()).Verifiable();
            _mockEnvironment.Setup(env => env.IsConnected).Returns(true);
            _mockEnvironment.Setup(env => env.ResourceRepository).Returns(_mockResRepository.Object);
            _mockEnvironment.Setup(env => env.DsfChannel).Returns(_mockFrameworkDataChannel.Object);

            _mockRes.Setup(c => c.Environment).Returns(_mockEnvironment.Object);

            _mockWebActivity.Setup(c => c.XMLConfiguration).Returns("<WebParts/>").Verifiable();
            _mockWebActivity.Setup(c => c.ResourceModel).Returns(_mockRes.Object);
        }

        #endregion Mock Setup

        #endregion

        #region CTOR Tests
        /// <summary>
        /// Positive result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void DefaultLayoutGridCreate_ExpectedGridCreated()
        {
            //Assert
            Assert.IsTrue(LayoutGrid.LayoutObjects.Count == 16 && LayoutGrid.LayoutObjects != null && LayoutGrid.LayoutObjects[0] != null);
        }

        /// <summary>
        /// Testing the creation of LayoutGridViewModel with null WebActivity object
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateLayoutGrid_NullWebActivity_Expected_ArgumentNullExceptionThrown()
        {
            LayoutGrid = new LayoutGridViewModel(new Mock<IEventAggregator>().Object, null);
        }

        #endregion CTOR Tests

        #region Movement and resize commands tests
        /// <summary>
        /// Move up command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveUpCommand_LayoutObjectBeforeCurrentIsSelected()
        {

            LayoutGrid.LayoutObjects[5].IsSelected = true;
            LayoutGrid.MoveUpCommand.Execute(null);

            Assert.IsTrue(LayoutGrid.LayoutObjects[1].IsSelected);
        }
        /// <summary>
        /// Move up command with no cell selected result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveUpWithNoSelectedCell_ExpectedNoCellSelection()
        {
            //Act
            LayoutGrid.MoveUpCommand.Execute(null);
            //Assert
            Assert.IsTrue(!LayoutGrid.IsAnyCellSelected);
        }
        /// <summary>
        /// Move down command with no cell selected result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveDownWithNoSelectedCell_ExpectedNoCellSelection()
        {
            //Act
            LayoutGrid.MoveDownCommand.Execute(null);
            //Assert
            Assert.IsTrue(!LayoutGrid.IsAnyCellSelected);
        }
        /// <summary>
        /// Move left command with no cell selected result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveLeftWithNoSelectedCell_ExpectedNoCellSelected()
        {
            //Act
            LayoutGrid.MoveLeftCommand.Execute(null);
            //Assert
            Assert.IsTrue(!LayoutGrid.IsAnyCellSelected);
        }
        /// <summary>
        /// Move right command with n cell selected result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveRightWithNoSelectedCell_NoCellSelected()
        {
            //Act
            LayoutGrid.MoveRightCommand.Execute(null);
            //Assert
            Assert.IsTrue(!LayoutGrid.IsAnyCellSelected);
        }
        /// <summary>
        /// Multiple move commands with no cell selected result case LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MultipleMoveWithNoInitialSelectedCell_NoCellSelected()
        {
            //Act
            LayoutGrid.MoveUpCommand.Execute(null);
            LayoutGrid.MoveRightCommand.Execute(null);
            //Assert
            Assert.IsTrue(!LayoutGrid.IsAnyCellSelected);
        }

        /// <summary>
        /// Move right command with the cell on the last column selected result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveRightOnLastColumn_SameCellSelected()
        {
            //Act
            //LayoutGrid.SetActiveCell(LayoutGrid.LayoutObjects.Last());
            LayoutGrid.LayoutObjects.Last().IsSelected = true;
            LayoutGrid.MoveRightCommand.Execute(null);
            //Assert

            Assert.IsTrue(LayoutGrid.LayoutObjects.Last().IsSelected);
        }
        /// <summary>
        /// Move up command with the cell on the first row selected result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveUpOnFirstRow_ExpectedSameCellSelected()
        {
            //Act
            LayoutGrid.LayoutObjects.First().IsSelected = true;
            LayoutGrid.MoveUpCommand.Execute(null);
            //Assert
            Assert.IsTrue(LayoutGrid.LayoutObjects.First().IsSelected);
        }
        /// <summary>
        /// Move down command with the cell on the last row selected result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveDownOnLastRow_ExpectedSameCellSelected()
        {
            //Act
            LayoutGrid.LayoutObjects.Last().IsSelected = true;
            LayoutGrid.MoveDownCommand.Execute(null);
            //Assert
            Assert.IsTrue(LayoutGrid.LayoutObjects.Last().IsSelected);
        }

        /// <summary>
        /// Move left command with the cell on the first column selected result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveLeftOnFirstColumn_ExpectedSameCellSelected()
        {
            //Act
            LayoutGrid.LayoutObjects.First().IsSelected = true;
            LayoutGrid.MoveLeftCommand.Execute(null);
            //Assert
            Assert.IsTrue(LayoutGrid.LayoutObjects.First().IsSelected);
        }

        /// <summary>
        /// Move down command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveDownCommand_ExpectedCellBelowCurrentSelected()
        {
            //Act
            LayoutGrid.LayoutObjects[1].IsSelected = true;

            LayoutGrid.MoveDownCommand.Execute(null);
            Assert.IsTrue(LayoutGrid.ActiveCell == LayoutGrid.LayoutObjects[5]);
        }

        /// <summary>
        /// Move left result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveLeftCommand_ExpectedCellAboveInGridSelected()
        {
            //Act
            LayoutGrid.LayoutObjects[1].IsSelected = true;
            LayoutGrid.MoveLeftCommand.Execute(null);
            //Assert
            Assert.IsTrue(LayoutGrid.LayoutObjects[0].IsSelected);
        }

        /// <summary>
        /// Move right result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void MoveRightCommand_ExpectedCell()
        {
            //Act
            LayoutGrid.LayoutObjects[1].IsSelected = true;
            LayoutGrid.MoveRightCommand.Execute(null);
            //Assert
            Assert.IsTrue(LayoutGrid.LayoutObjects[2].IsSelected);
        }

        /// <summary>
        /// Add column to the right command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void AddColumnRightCommand()
        {
            //Act
            LayoutGrid.LayoutObjects[3].IsSelected = true;
            LayoutGrid.LayoutObjects.First(c => c.IsSelected == true).AddColumnRightCommand.Execute(null);
            //Assert
            Assert.AreEqual(20, LayoutGrid.LayoutObjects.Count);
        }
        /// <summary>
        /// Add column to the left command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void AddColumnLeftCommand()
        {
            //Act
            LayoutGrid.LayoutObjects[3].IsSelected = true;
            LayoutGrid.LayoutObjects[3].AddColumnLeftCommand.Execute(null);
            //Assert
            Assert.AreEqual(20, LayoutGrid.LayoutObjects.Count);
        }

        /// <summary>
        /// Remove row result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void RemoveRow()
        {
            //Act
            LayoutGrid.RemoveRow(1);
            //Assert
            Assert.AreEqual(12, LayoutGrid.LayoutObjects.Count);
        }

        /// <summary>
        /// Remove column result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void RemoveColumn()
        {
            //Act
            LayoutGrid.RemoveColumn(1);
            //Assert
            Assert.AreEqual(12, LayoutGrid.LayoutObjects.Count);
        }

        /// <summary>
        /// Delete column result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void DeleteColumnCommand()
        {
            //Act
            LayoutGrid.SetDefaultSelected();
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteColumnCommand.Execute(null);
            //Assert
            Assert.AreEqual(12, LayoutGrid.LayoutObjects.Count);
        }
        /// <summary>
        /// Delete row command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void DeleteRowCommand()
        {
            //Act
            LayoutGrid.SetDefaultSelected();
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteRowCommand.Execute(null);
            //Assert
            Assert.AreEqual(12, LayoutGrid.LayoutObjects.Count);
        }
        /// <summary>
        /// Delete row command with only one cell left result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void DeleteRowWithOnlyOneCellLeft()
        {
            //Act
            LayoutGrid.SetDefaultSelected();
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteRowCommand.Execute(null);
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteRowCommand.Execute(null);
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteRowCommand.Execute(null);
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteRowCommand.Execute(null);
            //Assert
            Assert.AreEqual(4, LayoutGrid.LayoutObjects.Count);
        }
        /// <summary>
        /// Delete column command with only one cell left result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void DeleteColumnWithOnlyOneCellLeft()
        {
            //Act
            LayoutGrid.SetDefaultSelected();
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteColumnCommand.Execute(null);
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteColumnCommand.Execute(null);
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteColumnCommand.Execute(null);
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).DeleteColumnCommand.Execute(null);
            //Assert
            Assert.AreEqual(4, LayoutGrid.LayoutObjects.Count);
        }
        /// <summary>
        /// Add row above command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void AddRowAboveCommand()
        {
            //Act
            LayoutGrid.LayoutObjects[5].IsSelected = true;
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).AddRowAboveCommand.Execute(null);
            //Assert
            Assert.AreEqual(20, LayoutGrid.LayoutObjects.Count);
        }
        /// <summary>
        /// Add row below command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void AddRowBelowCommand()
        {
            //Act
            LayoutGrid.LayoutObjects[5].IsSelected = true;
            LayoutGrid.LayoutObjects.First(col => col.IsSelected == true).AddRowBelowCommand.Execute(null);
            //Assert
            Assert.AreEqual(20, LayoutGrid.LayoutObjects.Count);
        }


        /// <summary>
        /// Clear all cells command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void ClearAllCommand()
        {
            //Act
            Add_Webpart(0);
            Add_Webpart(1);
            LayoutGrid.LayoutObjects[7].ClearAllCommand.Execute(null);
            //Assert
            Assert.AreEqual(string.Empty, LayoutGrid.LayoutObjects[0].XmlConfiguration);
            Assert.AreEqual(string.Empty, LayoutGrid.LayoutObjects[1].XmlConfiguration);
        }

        /// <summary>
        /// Set default selected result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void SetDefaultSelected()
        {
            //Act
            LayoutGrid.SetDefaultSelected();
            //Assert
            Assert.IsTrue(LayoutGrid.LayoutObjects[0].IsSelected);
        }

        /// <summary>
        /// Calls the OpenWebsiteCommand() and check if the method has been called
        /// </summary>
        [TestMethod]
        public void OpenWebsiteCommand()
        {
            //Act
            //Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => messageRecieved = true);
            LayoutGrid.OpenWebsiteCommand.Execute(null);
            _aggregator.Verify(e => e.Publish(It.IsAny<AddWorkSurfaceMessage>
               ()), Times.Once());
            //Assert
            //_mockMainViewModel.Verify(ver => ver.OpenWebsiteCommand.Execute(null));
        }

        /// <summary>
        /// Calls the BindXmlConfigurationToGrid() and check if the method has been called
        /// </summary>
        [TestMethod]
        public void BindXmlConfigurationToGrid()
        {
            //Act
            Add_Webpart(0);
            LayoutGrid.BindXmlConfigurationToGrid();
            //Assert
            Assert.AreEqual(LayoutGrid.XmlConfiguration, LayoutGrid.ActivityModelItem.XMLConfiguration);
        }

        /// <summary>
        /// Update the model item result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void UpDateModelItem()
        {
            //Act
            string expected = Add_Webpart(0);
            LayoutGrid.UpdateModelItem();
            Console.WriteLine(expected);
            //Assert
            Assert.AreEqual(expected, LayoutGrid.LayoutObjects[0].XmlConfiguration);
        }

        /// <summary>
        /// Creating a new UI element result
        /// </summary>
        [TestMethod]
        public void AddNewUIElement()
        {
            //Act            
            LayoutGrid.AddNewUIElement(LayoutGrid.LayoutObjects[1], "Button", @"C:\Development\BusinessDesignStudio.Unit.Tests-branch-Mo\BusinessDesignStudio.Unit.Tests\Test References\icon.png");
            //Assert
            Assert.AreEqual("Button", LayoutGrid.LayoutObjects[1].WebpartServiceName);
        }

        /// <summary>
        /// Setting a active cell result in the Layoutgrid View Model
        /// </summary>
        [TestMethod]
        public void SetActiveCell()
        {
            //Act
            LayoutGrid.SetActiveCell(LayoutGrid.LayoutObjects[2]);
            //Assert
            Assert.AreEqual(LayoutGrid.LayoutObjects[2], LayoutGrid.ActiveCell);
        }

        /// <summary>
        /// Move method result for the Layoutgrid View Model
        /// </summary>
        [TestMethod]
        public void Move()
        {
            //Act
            LayoutGrid.Move(LayoutGrid.LayoutObjects[0], LayoutGrid.LayoutObjects[1]);
            //Assert
            Assert.IsTrue(LayoutGrid.LayoutObjects[1].IsSelected);
        }

        #endregion Movement and resize commands tests

        #region Add Webpart result
        /// <summary>
        /// Adding a web part to the grid result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void Add_Webpart()
        {
            //Act
            string expected = Add_Webpart(0);
            //Assert
            Assert.AreEqual(null, LayoutGrid.LayoutObjects[0].PreviousXmlConfig);
            Assert.AreEqual(expected, LayoutGrid.LayoutObjects[0].XmlConfiguration);
        }

        #endregion Add Webpart result

        #region Copy/Paste/Undo/Redo tests

        /// <summary>
        /// Cut method result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void Cut()
        {
            //Act
            Add_Webpart(0);
            LayoutGrid.Cut();
            //Assert
            Assert.AreEqual(string.Empty, LayoutGrid.LayoutObjects[1].XmlConfiguration);
        }

        /// <summary>
        /// Copy and paste commands result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void CopyCommand_PasteCommand()
        {
            //Act
            Add_Webpart(0);
            Copy_Paste();
            //Assert
            Assert.AreEqual(LayoutGrid.LayoutObjects[0].XmlConfiguration, LayoutGrid.LayoutObjects[1].XmlConfiguration);
        }
        /// <summary>
        /// Undo command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void UndoCommand()
        {
            //Act
            Add_Webpart(0);
            Copy_Paste();
            LayoutGrid.UndoCommand.Execute(null);
            //Assert
            Assert.AreEqual(string.Empty, LayoutGrid.LayoutObjects[1].XmlConfiguration);
        }
        /// <summary>
        /// Redo command result case for LayoutGridViewModelTest
        /// </summary>
        [TestMethod]
        public void RedoCommand()
        {
            //Act
            string expected = Add_Webpart(0);
            Copy_Paste();
            LayoutGrid.UndoCommand.Execute(null);
            LayoutGrid.RedoCommand.Execute(null);
            //Assert
            Assert.AreEqual(expected, LayoutGrid.LayoutObjects[1].XmlConfiguration);
        }

        [TestMethod]
        public void Undo_Redo_Redo_Command_Expected_Second_Redo_Does_Nothing()
        {
            //Act
            string expected = Add_Webpart(0);
            Copy_Paste();
            LayoutGrid.UndoCommand.Execute(null);
            LayoutGrid.RedoCommand.Execute(null);
            LayoutGrid.RedoCommand.Execute(null);

            //Assert
            Assert.AreEqual(expected, LayoutGrid.LayoutObjects[1].XmlConfiguration);
        }

        #endregion Copy/Paste/Undo/Redo tests

        #region Methods used by tests
        /// <summary>
        /// Method for adding a webpart to the layoutGrid
        /// </summary>
        public string Add_Webpart(int objNumber)
        {
            LayoutGrid.LayoutObjects[objNumber].IsSelected = true;
            LayoutGrid.LayoutObjects[objNumber].WebpartServiceName = "Button";
            LayoutGrid.LayoutObjects[objNumber].WebpartServiceDisplayName = "Test Button";
            LayoutGrid.LayoutObjects[objNumber].IconPath = @"C:\Development\BusinessDesignStudio.Unit.Tests-branch-Mo\BusinessDesignStudio.Unit.Tests\Test References\icon.png";
            string expected = @"<XmlData>
              <Dev2elementNameButton>ButtonClicked</Dev2elementNameButton>
              <displayTextButton>Test Button</displayTextButton>
              <btnType>submit</btnType>
              <customButtonCode></customButtonCode>
              <Dev2tabIndexButton></Dev2tabIndexButton>
              <Dev2toolTipButton></Dev2toolTipButton>
              <Dev2customStyleButton></Dev2customStyleButton>
              <Dev2widthButton></Dev2widthButton>
              <Dev2heightButton></Dev2heightButton>
            </XmlData>";
            LayoutGrid.LayoutObjects[objNumber].XmlConfiguration = expected;
            return expected;
        }
        /// <summary>
        /// Method for copying and pasteing cell content from one cell to another
        /// </summary>
        public void Copy_Paste()
        {
            LayoutGrid.LayoutObjects[0].IsSelected = true;
            LayoutGrid.LayoutObjects.First(cell => cell.IsSelected == true).CopyCommand.Execute(null);
            LayoutGrid.LayoutObjects[0].IsSelected = false;
            LayoutGrid.LayoutObjects[1].IsSelected = true;
            LayoutGrid.LayoutObjects.First(cell => cell.IsSelected == true).PasteCommand.Execute(null);
        }

        #endregion Methods used by tests

        #region Implementation of IHandle<AddWorkflowDesignerMessage>

        public void Handle(AddWorkSurfaceMessage message)
        {
            _addWorkflowDesignerMessageReceived = true;
        }

        #endregion
    }
}
