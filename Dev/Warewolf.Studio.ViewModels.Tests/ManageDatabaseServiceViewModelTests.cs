using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;

// ReSharper disable InconsistentNaming
namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public  class ManageDatabaseServiceViewModelTests
    {

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageDatabaseServiceViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageDatabaseServiceViewModel_Constructor_NullModel_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new ManageDatabaseServiceViewModel(null,new Mock<IRequestServiceNameViewModel>().Object);
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageDatabaseServiceViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageDatabaseServiceViewModel_Constructor_NullSaveDialog_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new ManageDatabaseServiceViewModel(new Mock<IDbServiceModel>().Object,null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageDatabaseServiceViewModel_Constructor")]
        public void ManageDatabaseServiceViewModel_Constructor_ArgumentsValid_ShouldSetProperties()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var manageDatabaseServiceViewModel = new ManageDatabaseServiceViewModel(new Mock<IDbServiceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(manageDatabaseServiceViewModel.CanEditSource);
            Assert.IsNotNull(manageDatabaseServiceViewModel.CreateNewSourceCommand);
            Assert.IsNotNull(manageDatabaseServiceViewModel.EditSourceCommand);
            Assert.IsNotNull(manageDatabaseServiceViewModel.Header);
            Assert.IsNotNull(manageDatabaseServiceViewModel.TestProcedureCommand);
            Assert.IsNotNull(manageDatabaseServiceViewModel.Inputs);
            Assert.IsNotNull(manageDatabaseServiceViewModel.SaveCommand);
            Assert.IsNotNull(manageDatabaseServiceViewModel.RefreshCommand);
            Assert.IsFalse(manageDatabaseServiceViewModel.IsRefreshing);
            Assert.IsFalse(manageDatabaseServiceViewModel.IsTesting);
            Assert.IsFalse(manageDatabaseServiceViewModel.IsTestResultsEmptyRows);
            Assert.IsFalse(manageDatabaseServiceViewModel.IsInputsEmptyRows);
            Assert.IsFalse(manageDatabaseServiceViewModel.IsOutputMappingEmptyRows);
            Assert.IsTrue(manageDatabaseServiceViewModel.ShowRecordSet);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageDatabaseServiceViewModel_Constructor")]
        public void ManageDatabaseServiceViewModel_Constructor_WhenModelReturnsSources_ShouldPopulateSourcesCollection()
        {
            //------------Setup for test--------------------------
            var mockServiceModel = new Mock<IDbServiceModel>();
            mockServiceModel.Setup(model => model.RetrieveSources()).Returns(new List<IDbSource> { new DbSourceDefinition(), new DbSourceDefinition() });
            
            
            //------------Execute Test---------------------------
            var manageDatabaseServiceViewModel = new ManageDatabaseServiceViewModel(mockServiceModel.Object, new Mock<IRequestServiceNameViewModel>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(manageDatabaseServiceViewModel.Sources);
            Assert.AreEqual(2,manageDatabaseServiceViewModel.Sources.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageDatabaseServiceViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageDatabaseServiceViewModel_Constructor_WhenServiceNull_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new ManageDatabaseServiceViewModel(new Mock<IDbServiceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageDatabaseServiceViewModel_Constructor")]
        public void ManageDatabaseServiceViewModel_Constructor_WhenGivenService_ShouldSetPropertiesBasedOnService()
        {
            //------------Setup for test--------------------------
            var databaseService = new DatabaseService();
            var id = Guid.NewGuid();
            const string name = "TestDBService";
            const string path = "TestDbServicePath";
            databaseService.Id = id;
            databaseService.Inputs = new IServiceInput[] { new ServiceInput("TestInput", "TestValue"), new ServiceInput("TestInput2","TestValue2") };
            databaseService.Name = name;
            databaseService.Path = path;
            databaseService.Source = new DbSourceDefinition();
            databaseService.Action = new DbAction();
            databaseService.OutputMappings = new IServiceOutputMapping[] { new ServiceOutputMapping("TestOutput", "TestMapping") };
            //------------Execute Test---------------------------
            var manageDatabaseServiceViewModel = new ManageDatabaseServiceViewModel(new Mock<IDbServiceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object, databaseService);
            //------------Assert Results-------------------------
            Assert.AreEqual(id,manageDatabaseServiceViewModel.Id);
            Assert.AreEqual(name,manageDatabaseServiceViewModel.Name);
            Assert.AreEqual(path,manageDatabaseServiceViewModel.Path);
            Assert.AreEqual(2,manageDatabaseServiceViewModel.Inputs.Count);
            Assert.AreEqual(1,manageDatabaseServiceViewModel.OutputMapping.Count);
            StringAssert.Contains(manageDatabaseServiceViewModel.Header,name);
            Assert.AreEqual(databaseService,manageDatabaseServiceViewModel.Item);
            Assert.IsTrue(manageDatabaseServiceViewModel.CanEditMappings);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageDatabaseServiceViewModel_TestAction")]
        public void ManageDatabaseServiceViewModel_TestAction_WhenTestResultsNull_ShouldNotUpdate()
        {
            //------------Setup for test--------------------------
            var mockServiceModel = new Mock<IDbServiceModel>();
            mockServiceModel.Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns((DataTable)null);
            var manageDatabaseServiceViewModel = new ManageDatabaseServiceViewModel(mockServiceModel.Object, new Mock<IRequestServiceNameViewModel>().Object);
            
            //------------Execute Test---------------------------
            manageDatabaseServiceViewModel.TestProcedureCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(manageDatabaseServiceViewModel.IsTesting);
            Assert.AreEqual("",manageDatabaseServiceViewModel.ErrorText);
        }
    }
}