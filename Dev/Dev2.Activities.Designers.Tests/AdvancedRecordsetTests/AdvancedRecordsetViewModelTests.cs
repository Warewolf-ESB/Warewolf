using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Activities.Designers2.AdvancedRecordset;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Interfaces.DataList;
using System.Collections.ObjectModel;

namespace Dev2.Activities.Designers.Tests.AdvancedRecordset
{
    [TestClass]
    public class AdvancedRecordsetViewModelTests
    {

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_MethodName")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void MethodName_ClearErrors()
        {
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqliteModel();
            var act = new AdvancedRecordsetActivity();

            using (var advancedRecordset = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                //------------Execute Test---------------------------
                advancedRecordset.ClearValidationMemoWithNoFoundError();
                //------------Assert Results-------------------------
                Assert.IsNull(advancedRecordset.Errors);
                Assert.AreEqual(advancedRecordset.DesignValidationErrors.Count, 1);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_MethodName")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);


            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqliteModel{
                mod.HasRecError = true
            };
            var act = new AdvancedRecordsetActivity();

            //------------Execute Test---------------------------
            using (var advancedRecordset = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                advancedRecordset.GenerateOutputsCommand.Execute(null);

                //------------Assert Results-------------------------
                Assert.IsTrue(advancedRecordset.ErrorRegion.IsEnabled);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void ParseTSQL_SelectStatementWithAllias_1_ReturnOutputs()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new Mock<IResourceModel>().Object);
            var recordSetItemModel = new RecordSetItemModel("person", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels = new ObservableCollection<IRecordSetFieldItemModel>
            {
                new RecordSetFieldItemModel("name", recordSetItemModel),
                new RecordSetFieldItemModel("age", recordSetItemModel),
                new RecordSetFieldItemModel("address_id", recordSetItemModel)
            };
            recordSetItemModel.Children = recordSetFieldItemModels;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel);
            DataListSingleton.SetDataList(dataListViewModel);

            var act = new AdvancedRecordsetActivity();
            const var query = "select name as username,age,address_id from person";
            //------------Execute Test---------------------------
            using (var viewModel = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                viewModel.SqlQuery = query;
                viewModel.GenerateOutputsCommand.Execute(query);

                //------------Assert Results-------------------------
                Assert.IsTrue(viewModel.OutputsRegion.IsEnabled);
                Assert.IsNotNull(viewModel.OutputsRegion.Outputs);
                Assert.IsTrue(viewModel.ErrorRegion.IsEnabled);
                Assert.AreEqual(3, viewModel.OutputsRegion.Outputs.Count);
                Assert.AreEqual("[[TableCopy().username]]", viewModel.OutputsRegion.Outputs.First().MappedTo);
                Assert.AreEqual("username", viewModel.OutputsRegion.Outputs.First().MappedFrom);
                Assert.AreEqual("[[TableCopy().address_id]]", viewModel.OutputsRegion.Outputs.Last().MappedTo);
                Assert.AreEqual("address_id", viewModel.OutputsRegion.Outputs.Last().MappedFrom);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void ParseTSQL_SelectStatementWithAllias_2_ReturnOutputs()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var act = new AdvancedRecordsetActivity();

            const var query = "select p.name as username,p.address_id as address from person p";
            //------------Execute Test---------------------------
            using (var viewModel = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                viewModel.SqlQuery = query;
                viewModel.OutputsRegion.RecordsetName = "NewPerson";
                viewModel.GenerateOutputsCommand.Execute(query);

                //------------Assert Results-------------------------
                Assert.IsNotNull(viewModel.OutputsRegion.Outputs);
                Assert.IsTrue(viewModel.OutputsRegion.IsEnabled);
                Assert.IsTrue(viewModel.ErrorRegion.IsEnabled);

                Assert.AreEqual(2, viewModel.OutputsRegion.Outputs.Count);
                Assert.AreEqual("[[NewPerson().username]]", viewModel.OutputsRegion.Outputs.First().MappedTo);
                Assert.AreEqual("username", viewModel.OutputsRegion.Outputs.First().MappedFrom);
                Assert.AreEqual("[[NewPerson().address]]", viewModel.OutputsRegion.Outputs.Last().MappedTo);
                Assert.AreEqual("address", viewModel.OutputsRegion.Outputs.Last().MappedFrom);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void ParseTSQL_SelectStatementWithAllias_3_ReturnOutputs()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new Mock<IResourceModel>().Object);
            var recordSetItemModel = new RecordSetItemModel("person", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels = new ObservableCollection<IRecordSetFieldItemModel>
            {
                new RecordSetFieldItemModel("name", recordSetItemModel),
                new RecordSetFieldItemModel("age", recordSetItemModel),
                new RecordSetFieldItemModel("address_id", recordSetItemModel)
            };
            recordSetItemModel.Children = recordSetFieldItemModels;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel);
            DataListSingleton.SetDataList(dataListViewModel);
            var act = new AdvancedRecordsetActivity();
            const var query = "select person.name as username,person.age as Age,person.address_id as Address from person";
            //------------Execute Test---------------------------
            using (var viewModel = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                viewModel.SqlQuery = query;
                viewModel.GenerateOutputsCommand.Execute(query);

                //------------Assert Results-------------------------
                Assert.IsNotNull(viewModel.OutputsRegion.Outputs);
                Assert.IsTrue(viewModel.OutputsRegion.IsEnabled);
                Assert.IsTrue(viewModel.ErrorRegion.IsEnabled);

                Assert.AreEqual(3, viewModel.OutputsRegion.Outputs.Count);
                Assert.AreEqual("[[TableCopy().username]]", viewModel.OutputsRegion.Outputs.First().MappedTo);
                Assert.AreEqual("username", viewModel.OutputsRegion.Outputs.First().MappedFrom);
                Assert.AreEqual("[[TableCopy().Address]]", viewModel.OutputsRegion.Outputs.Last().MappedTo);
                Assert.AreEqual("Address", viewModel.OutputsRegion.Outputs.Last().MappedFrom);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void ParseTSQL_SelectStatementWithAllias_4_ReturnOutputs()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new Mock<IResourceModel>().Object);
            var recordSetItemModel = new RecordSetItemModel("person", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels = new ObservableCollection<IRecordSetFieldItemModel>
            {
                new RecordSetFieldItemModel("name", recordSetItemModel),
                new RecordSetFieldItemModel("age", recordSetItemModel),
                new RecordSetFieldItemModel("address_id", recordSetItemModel)
            };
            recordSetItemModel.Children = recordSetFieldItemModels;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel);
            DataListSingleton.SetDataList(dataListViewModel);
            var act = new AdvancedRecordsetActivity();
            const var query = "select name as username,age as Age,address_id as Address from person";
            //------------Execute Test---------------------------
            using (var viewModel = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                viewModel.SqlQuery = query;
                viewModel.GenerateOutputsCommand.Execute(query);

                //------------Assert Results-------------------------
                Assert.IsNotNull(viewModel.OutputsRegion.Outputs);
                Assert.IsTrue(viewModel.OutputsRegion.IsEnabled);
                Assert.IsTrue(viewModel.ErrorRegion.IsEnabled);

                Assert.AreEqual(3, viewModel.OutputsRegion.Outputs.Count);
                Assert.AreEqual("[[TableCopy().username]]", viewModel.OutputsRegion.Outputs.First().MappedTo);
                Assert.AreEqual("username", viewModel.OutputsRegion.Outputs.First().MappedFrom);
                Assert.AreEqual("[[TableCopy().Address]]", viewModel.OutputsRegion.Outputs.Last().MappedTo);
                Assert.AreEqual("Address", viewModel.OutputsRegion.Outputs.Last().MappedFrom);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void ParseTSQL_SelectStatementWithAllias_6_ReturnOutputs()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new Mock<IResourceModel>().Object);
            var recordSetItemModel = new RecordSetItemModel("person", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels = new ObservableCollection<IRecordSetFieldItemModel>
            {
                new RecordSetFieldItemModel("name", recordSetItemModel),
                new RecordSetFieldItemModel("age", recordSetItemModel),
                new RecordSetFieldItemModel("address_id", recordSetItemModel)
            };
            recordSetItemModel.Children = recordSetFieldItemModels;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel);

            var recordSetItemModel2 = new RecordSetItemModel("address", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels2 = new ObservableCollection<IRecordSetFieldItemModel>
            {
                new RecordSetFieldItemModel("addr", recordSetItemModel2),
            };
            recordSetItemModel2.Children = recordSetFieldItemModels2;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel2);
            DataListSingleton.SetDataList(dataListViewModel);

            var act = new AdvancedRecordsetActivity();
            const var query = "select name as username,age as age,address_id as Address from person;Select addr from address";
            //------------Execute Test---------------------------
            using (var viewModel = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                viewModel.SqlQuery = query;
                viewModel.GenerateOutputsCommand.Execute(query);

                //------------Assert Results-------------------------
                Assert.IsNotNull(viewModel.OutputsRegion.Outputs);
                Assert.IsTrue(viewModel.OutputsRegion.IsEnabled);
                Assert.IsTrue(viewModel.ErrorRegion.IsEnabled);

                Assert.AreEqual(1, viewModel.OutputsRegion.Outputs.Count);
                Assert.AreEqual("[[TableCopy().addr]]", viewModel.OutputsRegion.Outputs.Last().MappedTo);
                Assert.AreEqual("addr", viewModel.OutputsRegion.Outputs.Last().MappedFrom);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void ParseTSQL_SelectStatementWithAllias_5_ReturnOutputs()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new Mock<IResourceModel>().Object);
            var recordSetItemModel = new RecordSetItemModel("person", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels = new ObservableCollection<IRecordSetFieldItemModel>
            {
                new RecordSetFieldItemModel("name", recordSetItemModel),
                new RecordSetFieldItemModel("age", recordSetItemModel),
                new RecordSetFieldItemModel("address_id", recordSetItemModel)
            };
            recordSetItemModel.Children = recordSetFieldItemModels;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel);
            DataListSingleton.SetDataList(dataListViewModel);

            var act = new AdvancedRecordsetActivity();
            const var query = "Update person set age = 30 where name = 'bob'";
            //------------Execute Test---------------------------
            using (var viewModel = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                viewModel.SqlQuery = query;
                viewModel.GenerateOutputsCommand.Execute(query);

                //------------Assert Results-------------------------
                Assert.IsNotNull(viewModel.OutputsRegion.Outputs);
                Assert.IsTrue(viewModel.OutputsRegion.IsEnabled);
                Assert.IsTrue(viewModel.ErrorRegion.IsEnabled);

                Assert.AreEqual(1, viewModel.OutputsRegion.Outputs.Count);
                Assert.AreEqual("[[Table1Copy().records_affected]]", viewModel.OutputsRegion.Outputs.First().MappedTo);
                Assert.AreEqual("records_affected", viewModel.OutputsRegion.Outputs.First().MappedFrom);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("AdvancedRecordset_Operations")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void ParseTSQL_SelectStatementWithAllias_7_ReturnOutputs()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new Mock<IResourceModel>().Object);
            var recordSetItemModel = new RecordSetItemModel("person", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels = new ObservableCollection<IRecordSetFieldItemModel>
            {
                new RecordSetFieldItemModel("name", recordSetItemModel),
                new RecordSetFieldItemModel("age", recordSetItemModel),
                new RecordSetFieldItemModel("address_id", recordSetItemModel)
            };
            recordSetItemModel.Children = recordSetFieldItemModels;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel);
            var recordSetItemModel2 = new RecordSetItemModel("address", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels2 = new ObservableCollection<IRecordSetFieldItemModel>
            {
                new RecordSetFieldItemModel("addr", recordSetItemModel2),
            };
            recordSetItemModel2.Children = recordSetFieldItemModels2;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel2);
            DataListSingleton.SetDataList(dataListViewModel);
            var act = new AdvancedRecordsetActivity();
            const var query = "select name as username,age as Age,address_id as Address from person p;Select addr from address a";
            //------------Execute Test---------------------------
            using (var viewModel = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                viewModel.SqlQuery = query;
                viewModel.GenerateOutputsCommand.Execute(query);

                //------------Assert Results-------------------------
                Assert.IsNotNull(viewModel.OutputsRegion.Outputs);
                Assert.IsTrue(viewModel.OutputsRegion.IsEnabled);
                Assert.IsTrue(viewModel.ErrorRegion.IsEnabled);

                Assert.AreEqual(1, viewModel.OutputsRegion.Outputs.Count);
                Assert.AreEqual("[[TableCopy().addr]]", viewModel.OutputsRegion.Outputs.Last().MappedTo);
                Assert.AreEqual("addr", viewModel.OutputsRegion.Outputs.Last().MappedFrom);
            }
        }
        
        [TestMethod]
        [Owner("Candice Daniel")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        [TestCategory("AdvancedRecordset_Handle")]
        public void UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            CustomContainer.LoadedTypes = new List<Type>
            {
                typeof(ManageSqliteServiceInputViewModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var mod = new SqliteModel{
                mod.HasRecError = true
            };
            var act = new AdvancedRecordsetActivity();
            using (var viewModel = new AdvancedRecordsetDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder()))
            {
                //------------Execute Test---------------------------
                viewModel.UpdateHelpDescriptor("help");
            }
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

    }
    public class SqliteModel : ISqliteService
    {
#pragma warning disable 649
        IStudioUpdateManager _updateRepository;
#pragma warning restore 649

        public bool HasRecError { get; set; }

        #region Implementation of IDbServiceModel

        public DataTable TestService(ISqliteService inputValues)
        {
            if (ThrowsTestError)
            {
                throw new Exception("bob");
            }

            if (HasRecError)
            {
                return null;
            }
            var dt = new DataTable();
            dt.Columns.Add("a");
            dt.Columns.Add("b");
            dt.Columns.Add("c");
            dt.TableName = "bob";
            return dt;

        }

        public IStudioUpdateManager UpdateRepository => _updateRepository;
        public bool ThrowsTestError { get; set; }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISqliteDBSource Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IList<IServiceOutputMapping> OutputMappings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IList<IServiceInput> Inputs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion
    }


    class InputViewForTest : ManageDatabaseServiceInputViewModel
    {
        public InputViewForTest(IDatabaseServiceViewModel model, IDbServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
    }
}
