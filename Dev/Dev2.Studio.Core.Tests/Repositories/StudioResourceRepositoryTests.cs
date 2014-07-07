using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Data.ServiceModel;
using Dev2.Explorer;
using Dev2.Interfaces;
using Dev2.Models;
using Dev2.Runtime.Hosting;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Dev2.Core.Tests.Repositories
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class StudioResourceRepositoryTests
    {
        readonly Action<System.Action, DispatcherPriority> _invoke = (a, b) => { };



        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_Constructor")]
        public void StudioResourceRepository_Constructor_ExplorerItemIsNull_ExplorerItemCollectionHasZeroItems()
        {
            AppSettings.LocalHost = "http://localhost:3145";
            var repo = new StudioResourceRepository(null, Guid.Empty, _invoke);
            Assert.AreEqual(0, repo.ExplorerItemModels.Count);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_Constructor")]
        public void StudioResourceRepository_Constructor_ExplorerItemHasData_ExplorerItemCollectionHasData()
        {
            AppSettings.LocalHost = "http://localhost:3142/";
            var repository = new StudioResourceRepository(GetTestData(), Guid.Empty, _invoke);
            Assert.AreEqual(1, repository.ExplorerItemModels.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_Load")]
        public void StudioResourceRepository_Load_CallsServiceLoad()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.Load(It.IsAny<Guid>()))
                                          .Returns(new ServerExplorerItem())
                                          .Verifiable();

            SetupEnvironmentRepo(Guid.Empty);

            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };

            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(Guid.Empty);
            GetEnvironmentRepository(mockEnvironment);

            //------------Execute Test---------------------------
            repository.Load(Guid.Empty, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.Load(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_EnvironmentEdited")]
        public void StudioResourceRepository_WhenEnvironmentEdited_EnvironmentConnected_CallsLoadOnEnvironment()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.Load(It.IsAny<Guid>()))
                                          .Returns(new ServerExplorerItem())
                                          .Verifiable();

            var c1 = EnviromentRepositoryTest.CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object, false);
            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object, e1);
            new EnvironmentRepository(repo);
            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");

            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };

            mockEnvironment.Setup(model => model.ID).Returns(Guid.Empty);
            repository.Load(Guid.Empty, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Execute Test---------------------------
            EnvironmentRepository.Instance.Save(e1);
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.Load(It.IsAny<Guid>()), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_EnvironmentEdited")]
        public void StudioResourceRepository_WhenEnvironmentEdited_ShouldFireEventOnce()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.Load(It.IsAny<Guid>()))
                                          .Returns(new ServerExplorerItem())
                                          .Verifiable();

            var c1 = EnviromentRepositoryTest.CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object, false);
            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object, e1);
            new EnvironmentRepository(repo);
            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");

            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };

            mockEnvironment.Setup(model => model.ID).Returns(Guid.Empty);
            repository.Load(Guid.Empty, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            repository.Load(Guid.Empty, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Execute Test---------------------------
            EnvironmentRepository.Instance.Save(e1);
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.Load(It.IsAny<Guid>()), Times.Exactly(3));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_EnvironmentEdited")]
        public void StudioResourceRepository_WhenEnvironmentEdited_EnvironmentNotConnected_DoesNotCallsLoadOnEnvironment()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.Load(It.IsAny<Guid>()))
                                          .Returns(new ServerExplorerItem())
                                          .Verifiable();

            var c1 = EnviromentRepositoryTest.CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object, false);
            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object, e1);
            new EnvironmentRepository(repo);
            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");

            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };

            mockEnvironment.Setup(model => model.ID).Returns(Guid.Empty);
            repository.Load(Guid.Empty, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            c1.Setup(connection => connection.IsConnected).Returns(false);
            //------------Execute Test---------------------------
            EnvironmentRepository.Instance.Save(e1);
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.Load(It.IsAny<Guid>()), Times.Exactly(1));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_Load")]
        public void StudioResourceRepository_Load_ServiceCallReturnsData_ExplorerItemsModelsIsBuilt()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.Load(It.IsAny<Guid>()))
                                          .Returns(GetTestData());

            SetupEnvironmentRepo(Guid.Empty);

            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            var countBeforeConnecting = repository.ExplorerItemModels.Count;
            //------------Execute Test---------------------------
            repository.Load(Guid.Empty, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, countBeforeConnecting);
            Assert.AreEqual(1, repository.ExplorerItemModels.Count);
            Assert.AreEqual(2, repository.ExplorerItemModels[0].Children.Count);
            Assert.AreEqual("folder1", repository.ExplorerItemModels[0].Children[0].DisplayName);
            Assert.AreEqual(4, repository.ExplorerItemModels[0].Children[0].Children.Count);
            Assert.AreEqual("dbService1", repository.ExplorerItemModels[0].Children[0].Children[0].DisplayName);
            Assert.AreEqual("webService1", repository.ExplorerItemModels[0].Children[0].Children[1].DisplayName);
            Assert.AreEqual("pluginService1", repository.ExplorerItemModels[0].Children[0].Children[2].DisplayName);
            Assert.AreEqual("subfolder1", repository.ExplorerItemModels[0].Children[0].Children[3].DisplayName);
            Assert.AreEqual(0, repository.ExplorerItemModels[0].Children[1].Children.Count);
            Assert.AreEqual("workflow1", repository.ExplorerItemModels[0].Children[1].DisplayName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_Load")]
        public void StudioResourceRepository_Load_ServiceCallReturnsNoData_ExplorerItemsHasNoData()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.Load(It.IsAny<Guid>()));

            SetupEnvironmentRepo(Guid.Empty);

            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            var countBeforeConnecting = repository.ExplorerItemModels.Count;
            //------------Execute Test---------------------------
            repository.Load(Guid.Empty, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, countBeforeConnecting);
            Assert.AreEqual(0, repository.ExplorerItemModels.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("StudioResourceRepository_AddItem")]
        public void StudioResourceRepository_AddItem_ItemIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            SetupEnvironmentRepo(Guid.Empty);
            var repository = new StudioResourceRepository(GetTestData(), Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            repository.AddItem(null);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("StudioResourceRepository_AddItem")]
        public void StudioResourceRepository_AddItem_ItemNotAttachedToParent_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            SetupEnvironmentRepo(Guid.Empty);
            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };

            var environmentId = Guid.NewGuid();
            var folderId = Guid.NewGuid();
            var explorerItemModel = new ExplorerItemModel
            {
                DisplayName = "FOLDER 1",
                EnvironmentId = environmentId,
                Permissions = Permissions.Contribute,
                ResourceId = folderId,
            };
            //------------Execute Test---------------------------
            repository.AddItem(explorerItemModel);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_AddItem")]
        public void StudioResourceRepository_AddItem_AttachedToParent_AddCalledOnServer()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.AddItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"));

            SetupEnvironmentRepo(Guid.Empty);

            var repository = new StudioResourceRepository(GetTestData(), Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            var parent = repository.ExplorerItemModels[0].Children[0];

            var environmentId = Guid.NewGuid();
            var folderId = Guid.NewGuid();

            var folder1 = new ExplorerItemModel
            {
                Parent = parent,
                DisplayName = "subFolder 1",
                EnvironmentId = environmentId,
                Permissions = Permissions.Contribute,
                ResourceId = folderId,
            };
            //------------Execute Test---------------------------
            repository.AddItem(folder1);
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.AddItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_Filter")]
        public void StudioResourceRepository_Filter_NullString_ReturnsSourceItems()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            var explorerItem = GetTestData();
            SetupEnvironmentRepo(Guid.Empty);
            var repository = new StudioResourceRepository(explorerItem, Guid.Empty, _invoke) { GetExplorerProxy = id => mockExplorerResourceRepository.Object };
            //------------Execute Test---------------------------
            var explorerItemModels = repository.Filter(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(explorerItemModels);
            Assert.AreEqual(explorerItemModels[0].DisplayName, repository.ExplorerItemModels[0].DisplayName);
            Assert.AreEqual(explorerItemModels[0].Children.Count, repository.ExplorerItemModels[0].Children.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_Filter")]
        public void StudioResourceRepository_Filter_String_ReturnsMatchingItems()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            var explorerItem = GetTestData();
            SetupEnvironmentRepo(Guid.Empty);
            var repository = new StudioResourceRepository(explorerItem, Guid.Empty, _invoke) { GetExplorerProxy = id => mockExplorerResourceRepository.Object };
            //------------Execute Test---------------------------
            var explorerItemModels = repository.Filter(model => model.DisplayName.Contains("dbService"));
            //------------Assert Results-------------------------
            Assert.IsNotNull(explorerItemModels);
            Assert.AreEqual(explorerItemModels[0].DisplayName, repository.ExplorerItemModels[0].DisplayName);
            Assert.AreNotEqual(explorerItemModels[0].Children.Count, repository.ExplorerItemModels[0].Children.Count);
            Assert.AreEqual(1, explorerItemModels[0].Children.Count);
            Assert.AreEqual("folder1", explorerItemModels[0].Children[0].DisplayName);
            Assert.AreEqual("dbService1", explorerItemModels[0].Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_Filter")]
        public void StudioResourceRepository_Filter_NullAfterFilter_ReturnsOriginalCollection()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            var explorerItem = GetTestData();
            SetupEnvironmentRepo(Guid.Empty);
            var repository = new StudioResourceRepository(explorerItem, Guid.Empty, _invoke) { GetExplorerProxy = id => mockExplorerResourceRepository.Object };
            //------------Preconditions--------------------------
            Assert.AreEqual(4, repository.ExplorerItemModels[0].ChildrenCount);
            var explorerItemModels = repository.Filter(model => model.DisplayName.Contains("r1"));
            Assert.AreEqual(0, explorerItemModels[0].ChildrenCount);
            Assert.AreEqual(1, explorerItemModels[0].Children.Count);
            //------------Execute Test---------------------------
            repository.Filter(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, repository.ExplorerItemModels[0].ChildrenCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_Filter")]
        public void StudioResourceRepository_Filter_String_Multiple_ReturnsMatchingItems()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            var explorerItem = GetTestData();
            SetupEnvironmentRepo(Guid.Empty);
            var repository = new StudioResourceRepository(explorerItem, Guid.Empty, _invoke) { GetExplorerProxy = id => mockExplorerResourceRepository.Object };
            //------------Execute Test---------------------------
            var explorerItemModels = repository.Filter(model => model.DisplayName.Contains("r1"));
            //------------Assert Results-------------------------
            Assert.IsNotNull(explorerItemModels);
            Assert.AreEqual(explorerItemModels[0].DisplayName, repository.ExplorerItemModels[0].DisplayName);
            Assert.AreNotEqual(explorerItemModels[0].Children.Count, repository.ExplorerItemModels[0].Children.Count);
            Assert.AreEqual(1, explorerItemModels[0].Children.Count);
            Assert.AreEqual("folder1", explorerItemModels[0].Children[0].DisplayName);
            Assert.AreEqual("subfolder1", explorerItemModels[0].Children[0].Children[0].DisplayName);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_AddItem")]
        public void StudioResourceRepository_AddItem_CallsServiceAddItem()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.AddItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();
            SetupEnvironmentRepo(Guid.Empty);
            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            repository.AddItem(new ExplorerItemModel { Parent = new ExplorerItemModel() });
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.AddItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(Exception))]
        [TestCategory("StudioResourceRepository_AddItem")]
        public void StudioResourceRepository_AddItem_ServiceReturnsAndError_ThrowsAnException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.AddItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Fail, "Just Failed"));
            SetupEnvironmentRepo(Guid.Empty);
            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            repository.AddItem(new ExplorerItemModel { Parent = new ExplorerItemModel() });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(Exception))]
        [TestCategory("StudioResourceRepository_AddItem")]
        public void StudioResourceRepository_AddItem_ServiceThrowsAnException_ReThrowsAnException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.AddItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Throws(new Exception("Something really bad happened, reboot your PC or risk a Harddrive crash"));
            SetupEnvironmentRepo(Guid.Empty);
            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            repository.AddItem(new ExplorerItemModel { Parent = new ExplorerItemModel() });
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_DeleteFolder")]
        public void StudioResourceRepository_DeleteFolder_HasWorkflow_WorkflowIsRemoved()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                               .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"));

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke);
            var beforeDelete = repository.ExplorerItemModels[0].Children.Count;
            repository.GetExplorerProxy = id => mockExplorerResourceRepository.Object;
            //------------Execute Test---------------------------
            repository.DeleteItem(environmentId, workflowId);
            var afterDelete = repository.ExplorerItemModels[0].Children.Count;
            //------------Assert Results-------------------------
            Assert.AreEqual(beforeDelete, 2);
            Assert.AreEqual(afterDelete, 1);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_DeleteFolder")]
        public void StudioResourceRepository_DeleteFolder_CallsDeleteOnService()
        {
            //------------Setup for test--------------------------
            var _propertyChangedCalled = false;
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            var folderID = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString(), folderID: folderID), environmentId, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            var folderItem = repository.FindItemById(folderID);
            folderItem.Parent.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ChildrenCount")
                {
                    _propertyChangedCalled = true;
                }
            };
            repository.DeleteFolder(folderItem);
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()), Times.Once());
            Assert.IsTrue(_propertyChangedCalled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void StudioResourceRepository_DeleteFolder_ResourceIdDoesnotExists_DoesNothing()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>())).Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            repository.DeleteItem(environmentId, Guid.NewGuid());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(Exception))]
        [TestCategory("StudioResourceRepository_DeleteFolder")]
        public void StudioResourceRepository_DeleteFolder_EnvironmentIdDoesnotExists_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>())).Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            repository.DeleteItem(Guid.NewGuid(), workflowId);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_DeleteFolder")]
        public void StudioResourceRepository_DeleteFolder_ItemIsNotOnExplorerItemModels_DoesNotCallsDeleteOnService()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>())).Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            repository.DeleteFolder(new ExplorerItemModel());
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_DeleteFolder")]
        public void StudioResourceRepository_DeleteFolder_ItemIsOnExplorerItemModels_CallsDeleteOnService()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.DeleteFolder(explorerItemModel);
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(Exception))]
        [TestCategory("StudioResourceRepository_DeleteFolder")]
        public void StudioResourceRepository_DeleteFolder_DeleteServiceReturnsAnError_ThrowsAnException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Fail, "Just Failed"));

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            var folderID = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString(), folderID: folderID), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.DeleteFolder(explorerItemModel);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(Exception))]
        [TestCategory("StudioResourceRepository_DeleteFolder")]
        public void StudioResourceRepository_DeleteFolder_DeleteServiceThrowsAndException_ReThrowsAnException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Throws(new Exception("Something really bad happened, reboot your PC or risk a Harddrive crash"));
            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.DeleteFolder(explorerItemModel);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("StudioResourceRepository_DeleteFolder")]
        public void StudioResourceRepository_DeleteFolder_ExplorerItemModelIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke);
            //------------Execute Test---------------------------
            repository.DeleteItem(null);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_DeleteItem")]
        public void StudioResourceRepository_DeleteItem_ItemRemoved_ChildrenChangedFired()
        {
            //------------Setup for test--------------------------
            var _propertyChangedCalled = false;
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            var folderID = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString(), folderID: folderID), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            var folderItem = repository.FindItemById(folderID);
            folderItem.Parent.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ChildrenCount")
                {
                    _propertyChangedCalled = true;
                }
            };

            //------------Execute Test---------------------------
            repository.DeleteItem(environmentId, folderID);
            //------------Assert Results-------------------------
            var foundItem = repository.FindItemById(folderID);
            Assert.IsNull(foundItem);
            Assert.IsTrue(_propertyChangedCalled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_DeleteItem")]
        public void StudioResourceRepository_DeleteItem_ItemRemoved_ResourceNotFound_ChildrenChangedNotFired()
        {
            //------------Setup for test--------------------------
            var _propertyChangedCalled = false;
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            var folderID = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString(), folderID: folderID), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            var folderItem = repository.FindItemById(folderID);
            folderItem.Parent.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ChildrenCount")
                {
                    _propertyChangedCalled = true;
                }
            };

            //------------Execute Test---------------------------
            repository.DeleteItem(environmentId, environmentId);
            //------------Assert Results-------------------------
            var foundItem = repository.FindItemById(environmentId);
            Assert.IsNull(foundItem);
            Assert.IsFalse(_propertyChangedCalled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_DeleteItem")]
        [ExpectedException(typeof(Exception))]
        public void StudioResourceRepository_DeleteItem_ItemRemoved_EnvironmentNotFound_ChildrenChangedNotFired()
        {
            //------------Setup for test--------------------------
            var _propertyChangedCalled = false;
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            var folderID = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString(), folderID: folderID), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            var folderItem = repository.FindItemById(folderID);
            folderItem.Parent.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ChildrenCount")
                {
                    _propertyChangedCalled = true;
                }
            };

            //------------Execute Test---------------------------
            repository.DeleteItem(folderID, environmentId);
            //------------Assert Results-------------------------
            var foundItem = repository.FindItemById(environmentId);
            Assert.IsNotNull(foundItem);
            Assert.IsFalse(_propertyChangedCalled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_DeleteItem")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudioResourceRepository_DeleteItem_ItemNull_Exception()
        {
            //------------Setup for test--------------------------
            var _propertyChangedCalled = false;
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            var folderID = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString(), folderID: folderID), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            var folderItem = repository.FindItemById(folderID);
            folderItem.Parent.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ChildrenCount")
                {
                    _propertyChangedCalled = true;
                }
            };

            //------------Execute Test---------------------------
            repository.DeleteItem(null);
            //------------Assert Results-------------------------
            var foundItem = repository.FindItemById(environmentId);
            Assert.IsNotNull(foundItem);
            Assert.IsFalse(_propertyChangedCalled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_DeleteItem")]
        public void StudioResourceRepository_DeleteItem_ItemParentNull_Exception()
        {
            //------------Setup for test--------------------------
            var _propertyChangedCalled = false;
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            var folderID = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString(), folderID: folderID), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            var folderItem = repository.FindItemById(folderID);
            folderItem.Parent.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ChildrenCount")
                {
                    _propertyChangedCalled = true;
                }
            };

            //------------Execute Test---------------------------
            ExplorerItemModel explorerItemModel = new ExplorerItemModel();
            repository.DeleteItem(explorerItemModel);
            //------------Assert Results-------------------------
            Assert.IsFalse(_propertyChangedCalled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_Disconnect")]
        public void StudioResourceRepository_Disconnect_ServerHasChildren_ServerIsDisconnectedAndChildrenAreCollapsed()
        {
            //------------Setup for test--------------------------
            var environmentId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);

            var repository = new StudioResourceRepository(GetTestData(), environmentId, _invoke);
            //------------Execute Test---------------------------
            var countBeforeDisconnect = repository.ExplorerItemModels[0].Children.Count;
            var isConnectedBeforeDisconnect = repository.ExplorerItemModels[0].IsConnected;
            repository.Disconnect(environmentId);
            var countAfterDisconnect = repository.ExplorerItemModels[0].Children.Count;
            var isConnectedAfterDisconnect = repository.ExplorerItemModels[0].IsConnected;
            //------------Assert Results-------------------------
            Assert.AreEqual(2, countBeforeDisconnect);
            Assert.AreEqual(0, countAfterDisconnect);
            Assert.AreEqual(true, isConnectedBeforeDisconnect);
            Assert.AreEqual(false, isConnectedAfterDisconnect);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_Disconnect")]
        public void StudioResourceRepository_Disconnect_PublishesRemoveEnvironmentMessage_ServerIsDisconnectedAndChildrenAreCollapsed()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IEnvironmentModel actualEnvironmentInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<RemoveEnvironmentMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is RemoveEnvironmentMessage) ? (msg as RemoveEnvironmentMessage).EnvironmentModel : null;
                actualEnvironmentInvoked = workSurfaceObject;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var environmentId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);

            var repository = new StudioResourceRepository(GetTestData(), environmentId, _invoke);
            //------------Execute Test---------------------------
            var countBeforeDisconnect = repository.ExplorerItemModels[0].Children.Count;
            var isConnectedBeforeDisconnect = repository.ExplorerItemModels[0].IsConnected;
            repository.Disconnect(environmentId);
            var countAfterDisconnect = repository.ExplorerItemModels[0].Children.Count;
            var isConnectedAfterDisconnect = repository.ExplorerItemModels[0].IsConnected;
            //------------Assert Results-------------------------
            Assert.AreEqual(2, countBeforeDisconnect);
            Assert.AreEqual(0, countAfterDisconnect);
            Assert.AreEqual(true, isConnectedBeforeDisconnect);
            Assert.AreEqual(false, isConnectedAfterDisconnect);
            Assert.IsNull(actualEnvironmentInvoked);
        }

        private static void GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {
            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object) { IsLoaded = true };
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(repo);
            // ReSharper restore ObjectCreationAsStatement
        }
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_RemoveEnvironment")]
        public void StudioResourceRepository_RemoveEnvironment_ServerHasChildren_ServerIsRemovedFromTree()
        {
            //------------Setup for test--------------------------
            var environmentId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(), environmentId, _invoke);
            //------------Execute Test---------------------------
            var countBeforeDisconnect = repository.ExplorerItemModels.Count;
            repository.RemoveEnvironment(environmentId);
            var countAfterDisconnect = repository.ExplorerItemModels.Count;
            //------------Assert Results-------------------------
            Assert.AreEqual(1, countBeforeDisconnect);
            Assert.AreEqual(0, countAfterDisconnect);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(Exception))]
        [TestCategory("StudioResourceRepository_Connect")]
        public void StudioResourceRepository_Connect_EnvironmentNotFound_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            repository.Connect(Guid.Empty);
        }



        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_Connect")]
        public void StudioResourceRepository_Connect_EnvironmentNodeExists_CallsServiceLoad()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.Load(It.IsAny<Guid>()))
                                          .Returns(new ServerExplorerItem())
                                          .Verifiable();

            var environmentId = Guid.NewGuid();

            SetupEnvironmentRepo(environmentId);

            var repository = new StudioResourceRepository(
                                 new ServerExplorerItem
                                 {
                                     DisplayName = "LOCALHOST",
                                     Permissions = Permissions.Administrator,
                                     ResourceType = ResourceType.Server
                                 }, environmentId, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            repository.Connect(environmentId);
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.Load(It.IsAny<Guid>()), Times.Once());
        }

        static Mock<IResourceRepository> SetupEnvironmentRepo(Guid environmentId)
        {
            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(environmentId);
            GetEnvironmentRepository(mockEnvironment);
            return mockResourceRepository;
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioResourceRepository_Connect")]
        public void StudioResourceRepository_Connect_EnvironmentNodeExists_CallsServiceLoad_DoesNotDuplicateServer()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.Load(It.IsAny<Guid>()))
                                          .Returns(new ServerExplorerItem())
                                          .Verifiable();

            var environmentId = Guid.NewGuid();

            SetupEnvironmentRepo(environmentId);

            var repository = new StudioResourceRepository(
                                 new ServerExplorerItem
                                 {
                                     DisplayName = "LOCALHOST",
                                     Permissions = Permissions.Administrator,
                                     ResourceType = ResourceType.Server
                                 }, environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            repository.Connect(environmentId);
            repository.Connect(environmentId);
            //------------Assert Results-------------------------
            Assert.AreEqual(repository.ExplorerItemModels.Count, 1);
        }



        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_Connect")]
        public void StudioResourceRepository_Connect_ServiceCallReturnsData_ExplorerItemsModelsIsBuilt()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.Load(It.IsAny<Guid>()))
                                          .Returns(GetTestData());

            var environmentId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(
                                 new ServerExplorerItem
                                 {
                                     DisplayName = "LOCALHOST",
                                     Permissions = Permissions.Administrator,
                                     ResourceType = ResourceType.Server
                                 }, environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            var countBeforeConnecting = repository.ExplorerItemModels.Count;
            //------------Execute Test---------------------------
            repository.Connect(environmentId);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, countBeforeConnecting);
            Assert.AreEqual(1, repository.ExplorerItemModels.Count);
            Assert.AreEqual(2, repository.ExplorerItemModels[0].Children.Count);
            Assert.AreEqual("folder1", repository.ExplorerItemModels[0].Children[0].DisplayName);
            Assert.AreEqual(4, repository.ExplorerItemModels[0].Children[0].Children.Count);
            Assert.AreEqual("dbService1", repository.ExplorerItemModels[0].Children[0].Children[0].DisplayName);
            Assert.AreEqual("webService1", repository.ExplorerItemModels[0].Children[0].Children[1].DisplayName);
            Assert.AreEqual("pluginService1", repository.ExplorerItemModels[0].Children[0].Children[2].DisplayName);
            Assert.AreEqual("subfolder1", repository.ExplorerItemModels[0].Children[0].Children[3].DisplayName);
            Assert.AreEqual(0, repository.ExplorerItemModels[0].Children[1].Children.Count);
            Assert.AreEqual("workflow1", repository.ExplorerItemModels[0].Children[1].DisplayName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_RenameItem")]
        public void StudioResourceRepository_RenameItem_ItemIsOnExplorerItemModelsAndHasNewNameParam_CallsRenameItemOnService()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.RenameItem(explorerItemModel, "New Name");
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("StudioResourceRepository_RenameItem")]
        public void StudioResourceRepository_RenameItem_ItemIsOnExplorerItemModelsAndHasSameNameParam_DoesntCallRenameItemOnService()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.RenameItem(explorerItemModel, "folder1");
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(Exception))]
        [TestCategory("StudioResourceRepository_RenameItem")]
        public void StudioResourceRepository_RenameItem_ServiceReturnsAndError_ThrowsAndException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Fail, "Just Failed"));

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.RenameItem(explorerItemModel, "New Name");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(Exception))]
        [TestCategory("StudioResourceRepository_RenameItem")]
        public void StudioResourceRepository_RenameItem_ServiceThrowsAnException_ReThrowsAndException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                          .Throws(new Exception("Something really bad happened, reboot your PC or risk a Harddrive crash"));

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.RenameItem(explorerItemModel, "New Name");
        }



        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("StudioResourceRepository_RenameItem")]
        public void StudioResourceRepository_RenameItem_ItemIsOnExplorerItemModelsAndNewNameParamIsEmpty_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                         .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                         .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object
                };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.RenameItem(explorerItemModel, "");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("StudioResourceRepository_RenameItem")]
        public void StudioResourceRepository_RenameItem_ExplorerItemModelIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke);
            //------------Execute Test---------------------------
            repository.RenameItem(null, "New Name");
        }

        #region RenameFolder Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("StudioResourceRepository_RenameFolder")]
        public void StudioResourceRepository_RenameFolder_ItemIsOnExplorerItemModelsAndHasNewNameParam_CallsRenameItemOnService()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.SetupProperty(model => model.Category);
            mockResourceModel.SetupProperty(model => model.ResourceName);
            mockResourceModel.Object.Category = "MyCat";
            mockResourceModel.Object.ResourceName = "MyResName";
            mockResourceRepository.Setup(resourceRepository => resourceRepository.Find(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(new List<IResourceModel> { mockResourceModel.Object });
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(environmentId);
            GetEnvironmentRepository(mockEnvironment);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            explorerItemModel.ResourcePath = "folder1";
            repository.RenameFolder(explorerItemModel, "New Name");
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.RenameFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("StudioResourceRepository_RenameFolder")]
        public void StudioResourceRepository_RenameFolder_ItemIsOnExplorerItemModelsAndHasSameNameParam_DoesntCallRenameItemOnService()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.RenameFolder(explorerItemModel, "folder1");
            //------------Assert Results-------------------------
            mockExplorerResourceRepository.Verify(m => m.RenameFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [ExpectedException(typeof(NullReferenceException))]
        [TestCategory("StudioResourceRepository_RenameFolder")]
        public void StudioResourceRepository_RenameFolder_ServiceReturnsAndError_ThrowsAndException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Fail, "Just Failed"));

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.RenameFolder(explorerItemModel, "New Name");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [ExpectedException(typeof(NullReferenceException))]
        [TestCategory("StudioResourceRepository_RenameFolder")]
        public void StudioResourceRepository_RenameFolder_ServiceThrowsAnException_ReThrowsAndException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                          .Throws(new Exception("Something really bad happened, reboot your PC or risk a Harddrive crash"));

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.RenameFolder(explorerItemModel, "New Name");
        }



        [TestMethod]
        [Owner("Massimo Guerrera")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("StudioResourceRepository_RenameFolder")]
        public void StudioResourceRepository_RenameFolder_ItemIsOnExplorerItemModelsAndNewNameParamIsEmpty_ThrowsException()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                         .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                         .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };
            //------------Execute Test---------------------------
            var explorerItemModel = repository.ExplorerItemModels[0].Children[0];
            repository.RenameFolder(explorerItemModel, "");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("StudioResourceRepository_RenameFolder")]
        public void StudioResourceRepository_RenameFolder_ExplorerItemModelIsNull_ThrowsException()
        {
            //------------Setup for test--------------------------
            var repository = new StudioResourceRepository(null, Guid.Empty, _invoke);
            //------------Execute Test---------------------------
            repository.RenameFolder(null, "New Name");
        }

        #endregion

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_FindItemByID")]
        public void StudioResourceRepository_FindItemByID_ItemExists_ShouldReturnItem()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>())).Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            //------------Execute Test---------------------------
            var findItemById = repository.FindItemById(workflowId);
            //------------Assert Results-------------------------
            Assert.IsNotNull(findItemById);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_FindItemByID")]
        public void StudioResourceRepository_FindItemByID_ItemDoesNotExist_ShouldReturnNull()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            //------------Execute Test---------------------------
            var findItemById = repository.FindItemById(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsNull(findItemById);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_FindItem")]
        public void StudioResourceRepository_FindItem_FuncMatchesItem_ReturnsItemMatchingPredicate()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            //------------Execute Test---------------------------
            var explorerItemModel = repository.FindItem(model => model.DisplayName == "workflow1");
            //------------Assert Results-------------------------
            Assert.IsNotNull(explorerItemModel);
            Assert.AreEqual("workflow1", explorerItemModel.DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_FindItem")]
        public void StudioResourceRepository_FindItem_FuncDoesNotMatchItem_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            //------------Execute Test---------------------------
            var explorerItemModel = repository.FindItem(model => model.DisplayName == "workfl1");
            //------------Assert Results-------------------------
            Assert.IsNull(explorerItemModel);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_FindItem")]
        public void StudioResourceRepository_FindItem_NullFunc_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            //------------Execute Test---------------------------
            var explorerItemModel = repository.FindItem(null);
            //------------Assert Results-------------------------
            Assert.IsNull(explorerItemModel);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioResourceRepository_UpdateItem")]
        public void StudioResourceRepository_UpdateItem_ExpectSuccessfulUpdate()
        {
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();
            mockExplorerResourceRepository.Setup(m => m.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()))
                                          .Returns(new ExplorerRepositoryResult(ExecStatus.Success, "Success"))
                                          .Verifiable();

            var environmentId = Guid.NewGuid();
            var workflowId = Guid.NewGuid();
            SetupEnvironmentRepo(environmentId);
            var repository = new StudioResourceRepository(GetTestData(workflowId.ToString()), environmentId, _invoke)
            {
                GetExplorerProxy = id => mockExplorerResourceRepository.Object
            };

            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(environmentId);
            GetEnvironmentRepository(mockEnvironment);

            bool updated = false;
            repository.UpdateItem(workflowId, a =>
                {
                    updated = true;
                }, environmentId);
            Assert.IsTrue(updated);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioResourceRepository_UpdateItem")]
        public void StudioResourceRepository_UpdateItem_NonExistentGuid_ExpectNoActionCalled()
        {
            SetupEnvironmentRepo(Guid.Empty);
            var repo = new StudioResourceRepository(GetTestData(), Guid.Empty, _invoke);
            var item = repo.ExplorerItemModels.First();
            Assert.AreNotEqual(item.DisplayName, "bob");
            bool updated = false;
            repo.UpdateItem(Guid.NewGuid(), a =>
            {
                a.DisplayName = "bob";
                updated = true;
            }, Guid.Empty);

            Assert.IsFalse(updated);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioResourceRepository_UpdateItem")]
        public void StudioResourceRepository_UpdateItem_NullAction_ExpectException()
        {
            SetupEnvironmentRepo(Guid.Empty);
            var repo = new StudioResourceRepository(GetTestData(), Guid.Empty, _invoke);
            var item = repo.ExplorerItemModels.First();
            Assert.AreNotEqual(item.DisplayName, "bob");
            repo.UpdateItem(item.ResourceId, null, Guid.Empty);

        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_ItemAddedMessageHandler")]
        public void StudioResourceRepository_ItemAddedMessageHandler_ItemNotFoundInResourceRepo_Service_AddedSuccessfully()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var parent = new ServerExplorerItem
                {
                    ResourceType = ResourceType.Folder,
                    DisplayName = "SUB FOLDER",
                    ResourceId = Guid.NewGuid(),
                    Permissions = Permissions.Contribute,
                    ResourcePath = "MANFOLDER\\SUB FOLDER"
                };
            var mockResourceRepo = SetupEnvironmentRepo(Guid.Empty);
            mockResourceRepo.Setup(repository => repository.ReloadResource(It.IsAny<Guid>(), It.Is<Studio.Core.AppResources.Enums.ResourceType>(type => type == Studio.Core.AppResources.Enums.ResourceType.Service), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()));
            // ReSharper disable ObjectCreationAsStatement
            new StudioResourceRepository(parent, Guid.Empty, _invoke)
                // ReSharper restore ObjectCreationAsStatement
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object,
                    GetCurrentEnvironment = () => Guid.Empty
                };

            var studioResourceRepository = StudioResourceRepository.Instance;
            //------------Execute Test---------------------------
            studioResourceRepository.ItemAddedMessageHandler(new ServerExplorerItem
                {
                    DisplayName = "TEST FOLDER",
                    ResourcePath = "MANFOLDER\\SUB FOLDER\\TEST FOLDER",
                    ResourceType = ResourceType.DbService
                });

            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ReloadResource(It.IsAny<Guid>(), It.Is<Studio.Core.AppResources.Enums.ResourceType>(type => type == Studio.Core.AppResources.Enums.ResourceType.Service), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_ItemAddedMessageHandler")]
        public void StudioResourceRepository_ItemAddedMessageHandler_ItemIsFolder_AddedSuccessfully()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var parent = new ServerExplorerItem
                {
                    ResourceType = ResourceType.Folder,
                    DisplayName = "SUB FOLDER",
                    ResourceId = Guid.NewGuid(),
                    Permissions = Permissions.Contribute,
                    ResourcePath = "MANFOLDER\\SUB FOLDER"
                };
            var mockResourceRepo = SetupEnvironmentRepo(Guid.Empty);
            mockResourceRepo.Setup(repository => repository.ReloadResource(It.IsAny<Guid>(), It.Is<Studio.Core.AppResources.Enums.ResourceType>(type => type == Studio.Core.AppResources.Enums.ResourceType.Service), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()));
            // ReSharper disable ObjectCreationAsStatement
            new StudioResourceRepository(parent, Guid.Empty, _invoke)
                // ReSharper restore ObjectCreationAsStatement
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object,
                    GetCurrentEnvironment = () => Guid.Empty
                };

            var studioResourceRepository = StudioResourceRepository.Instance;
            var before = studioResourceRepository.ExplorerItemModels[0].Children.Count();
            //------------Execute Test---------------------------
            studioResourceRepository.ItemAddedMessageHandler(new ServerExplorerItem
                {
                    DisplayName = "TEST FOLDER",
                    ResourcePath = "MANFOLDER\\SUB FOLDER\\TEST FOLDER",
                    ResourceType = ResourceType.Folder
                });
            var after = studioResourceRepository.ExplorerItemModels[0].Children.Count();
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ReloadResource(It.IsAny<Guid>(), It.Is<Studio.Core.AppResources.Enums.ResourceType>(type => type == Studio.Core.AppResources.Enums.ResourceType.Service), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()), Times.Never());
            Assert.AreEqual(0, before);
            Assert.AreEqual(1, after);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_ItemAddedMessageHandler")]
        public void StudioResourceRepository_ItemAddedMessageHandler_ItemNotFoundInResourceRepo_Source_AddedSuccessfully()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var parent = new ServerExplorerItem
                {
                    ResourceType = ResourceType.Folder,
                    DisplayName = "SUB FOLDER",
                    ResourceId = Guid.NewGuid(),
                    Permissions = Permissions.Contribute,
                    ResourcePath = "MANFOLDER\\SUB FOLDER"
                };
            var mockResourceRepo = SetupEnvironmentRepo(Guid.Empty);
            mockResourceRepo.Setup(repository => repository.ReloadResource(It.IsAny<Guid>(), It.Is<Studio.Core.AppResources.Enums.ResourceType>(type => type == Studio.Core.AppResources.Enums.ResourceType.Source), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()));
            // ReSharper disable ObjectCreationAsStatement
            new StudioResourceRepository(parent, Guid.Empty, _invoke)
                // ReSharper restore ObjectCreationAsStatement
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object,
                    GetCurrentEnvironment = () => Guid.Empty
                };

            var studioResourceRepository = StudioResourceRepository.Instance;
            //------------Execute Test---------------------------
            studioResourceRepository.ItemAddedMessageHandler(new ServerExplorerItem
                {
                    DisplayName = "TEST FOLDER",
                    ResourcePath = "MANFOLDER\\SUB FOLDER\\TEST FOLDER",
                    ResourceType = ResourceType.DbSource
                });

            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ReloadResource(It.IsAny<Guid>(), It.Is<Studio.Core.AppResources.Enums.ResourceType>(type => type == Studio.Core.AppResources.Enums.ResourceType.Source), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_ItemAddedMessageHandler")]
        public void StudioResourceRepository_ItemAddedMessageHandler_ItemNotFoundInResourceRepo_Workflow_AddedSuccessfully()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var parent = new ServerExplorerItem
                {
                    ResourceType = ResourceType.Folder,
                    DisplayName = "SUB FOLDER",
                    ResourceId = Guid.NewGuid(),
                    Permissions = Permissions.Contribute,
                    ResourcePath = "MANFOLDER\\SUB FOLDER"
                };
            var mockResourceRepo = SetupEnvironmentRepo(Guid.Empty);
            mockResourceRepo.Setup(repository => repository.ReloadResource(It.IsAny<Guid>(), It.Is<Studio.Core.AppResources.Enums.ResourceType>(type => type == Studio.Core.AppResources.Enums.ResourceType.WorkflowService), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()));
            // ReSharper disable ObjectCreationAsStatement
            new StudioResourceRepository(parent, Guid.Empty, _invoke)
                // ReSharper restore ObjectCreationAsStatement
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object,
                    GetCurrentEnvironment = () => Guid.Empty
                };

            var studioResourceRepository = StudioResourceRepository.Instance;
            //------------Execute Test---------------------------
            studioResourceRepository.ItemAddedMessageHandler(new ServerExplorerItem
                {
                    DisplayName = "TEST FOLDER",
                    ResourcePath = "MANFOLDER\\SUB FOLDER\\TEST FOLDER",
                    ResourceType = ResourceType.WorkflowService
                });

            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ReloadResource(It.IsAny<Guid>(), It.Is<Studio.Core.AppResources.Enums.ResourceType>(type => type == Studio.Core.AppResources.Enums.ResourceType.WorkflowService), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_ItemAddedMessageHandler")]
        public void StudioResourceRepository_ItemAddedMessageHandler_ParentExistsOnTree_AddedSuccessfully()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var parent = new ServerExplorerItem
                {
                    ResourceType = ResourceType.Folder,
                    DisplayName = "SUB FOLDER",
                    ResourceId = Guid.NewGuid(),
                    Permissions = Permissions.Contribute,
                    ResourcePath = "MANFOLDER\\SUB FOLDER"
                };
            var mockResourceRepo = SetupEnvironmentRepo(Guid.Empty);
            mockResourceRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(new Mock<IResourceModel>().Object);
            mockResourceRepo.Setup(repository => repository.ReloadResource(It.IsAny<Guid>(), It.IsAny<Studio.Core.AppResources.Enums.ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()));
            // ReSharper disable ObjectCreationAsStatement
            new StudioResourceRepository(parent, Guid.Empty, _invoke)
                // ReSharper restore ObjectCreationAsStatement
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object,
                    GetCurrentEnvironment = () => Guid.Empty
                };

            var studioResourceRepository = StudioResourceRepository.Instance;
            var before = studioResourceRepository.ExplorerItemModels[0].Children.Count();
            //------------Execute Test---------------------------
            studioResourceRepository.ItemAddedMessageHandler(new ServerExplorerItem
                {
                    DisplayName = "TEST FOLDER",
                    ResourcePath = "MANFOLDER\\SUB FOLDER\\TEST FOLDER"
                });
            var after = studioResourceRepository.ExplorerItemModels[0].Children.Count();
            //------------Assert Results-------------------------
            mockResourceRepo.Verify(repository => repository.ReloadResource(It.IsAny<Guid>(), It.IsAny<Studio.Core.AppResources.Enums.ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>()), Times.Never());
            Assert.AreEqual(0, before);
            Assert.AreEqual(1, after);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("StudioResourceRepository_ItemAddedMessageHandler")]
        public void StudioResourceRepository_ItemAddedMessageHandler_ParentDoesNotExistsOnTree_NotAdded()
        {
            //------------Setup for test--------------------------
            var mockExplorerResourceRepository = new Mock<IExplorerResourceRepository>();

            var parent = new ServerExplorerItem
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "SUB FOLDER",
                ResourceId = Guid.NewGuid(),
                Permissions = Permissions.Contribute,
                ResourcePath = "MANFOLDER\\APRIL WORK\\SUB FOLDER"
            };
            SetupEnvironmentRepo(Guid.Empty);
            var repo = new StudioResourceRepository(parent, Guid.Empty, _invoke)
                {
                    GetExplorerProxy = id => mockExplorerResourceRepository.Object,
                    GetCurrentEnvironment = () => Guid.Empty
                };

            repo.GetCurrentEnvironment = () => Guid.Empty;

            var studioResourceRepository = StudioResourceRepository.Instance;
            var before = studioResourceRepository.ExplorerItemModels[0].Children.Count();
            //------------Execute Test---------------------------
            studioResourceRepository.ItemAddedMessageHandler(new ServerExplorerItem
            {
                DisplayName = "TEST FOLDER",
                ResourcePath = "MANFOLDER\\APRIL\\TEST FOLDER"
            });

            var after = studioResourceRepository.ExplorerItemModels[0].Children.Count();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, before);
            Assert.AreEqual(0, after);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioResourceRepository_GetEnvironmentModel")]
        public void StudioResourceRepository_GetEnvironmentModel_NullItemReturnsFalse()
        {
            Assert.IsFalse(StudioResourceRepository.GetEnvironmentModel(new Mock<IEnvironmentModel>().Object, null, Guid.NewGuid()));
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioResourceRepository_GetEnvironmentModel")]
        public void StudioResourceRepository_GetEnvironmentModel_NullEnvReturnsFalse()
        {
            Assert.IsFalse(StudioResourceRepository.GetEnvironmentModel(null, new ServerExplorerItem(), Guid.NewGuid()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioResourceRepository_GetEnvironmentModel")]
        public void StudioResourceRepository_GetEnvironmentModel_NullItemURIReturnsFalse()
        {
            var environmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(a => a.WebServerUri).Returns(new Uri("http://www.bob.com"));
            environmentModel.Setup(a => a.Connection).Returns(mockConnection.Object);

            Assert.IsFalse(StudioResourceRepository.GetEnvironmentModel(environmentModel.Object, new ServerExplorerItem { WebserverUri = null }, Guid.NewGuid()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioResourceRepository_GetEnvironmentModel")]
        public void StudioResourceRepository_GetEnvironmentModel_NullConnectionReturnsFalse()
        {
            var environmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();

            environmentModel.Setup(a => a.Connection).Returns(mockConnection.Object);

            Assert.IsFalse(StudioResourceRepository.GetEnvironmentModel(environmentModel.Object, new ServerExplorerItem { WebserverUri = "bob" }, Guid.NewGuid()));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_GetEnvironmentModel")]
        public void StudioResourceRepository_GetEnvironmentModel_HasMatchingID_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var environmentModel = new Mock<IEnvironmentModel>();
            var environmentId = Guid.NewGuid();
            environmentModel.Setup(model => model.ID).Returns(environmentId);
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.WebServerUri).Returns(new Uri("http://bob:3142/"));
            var serverID = Guid.NewGuid();
            mockConnection.Setup(connection => connection.ServerID).Returns(serverID);
            environmentModel.Setup(a => a.Connection).Returns(mockConnection.Object);
            ServerExplorerItem serverExplorerItem = new ServerExplorerItem { WebserverUri = "http://bob:3142/", ServerId = serverID };
            //------------Execute Test---------------------------
            var found = StudioResourceRepository.GetEnvironmentModel(environmentModel.Object, serverExplorerItem, environmentId);
            //------------Assert Results-------------------------
            Assert.IsTrue(found);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_AddResourceItem")]
        public void StudioResourceRepository_AddResourceItem_ItemAdded()
        {
            //------------Setup for test--------------------------
            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            var connection = new Mock<IEnvironmentConnection>();

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
            var environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            var serverItemModel = new ExplorerItemModel { DisplayName = "localhost", ResourceType = ResourceType.Server, EnvironmentId = environmentModel.ID, ResourcePath = "", ResourceId = Guid.NewGuid() };
            var rootItem = serverItemModel;
            ExplorerItemModel workflowsFolder = new ExplorerItemModel { DisplayName = "WORKFLOWS", ResourceType = ResourceType.Folder, ResourcePath = "WORKFLOWS", EnvironmentId = mockEnvironmentModel.Object.ID, ResourceId = Guid.NewGuid() };
            serverItemModel.Children.Add(workflowsFolder);

            var studioResourceRepository = new StudioResourceRepository(serverItemModel, _invoke);
            resourceModel.Setup(model => model.Category).Returns("WORKFLOWS\\" + resourceModel.Object.DisplayName);
            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(environmentModel);
            new EnvironmentRepository(testEnvironmentRespository);
            IEnvironmentModel internalEnvironmentModel = environmentModel;
            studioResourceRepository.GetCurrentEnvironment = () => internalEnvironmentModel.ID;
            //------------Execute Test---------------------------
            studioResourceRepository.AddResouceItem(resourceModel.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, rootItem.Children.Count);
            Assert.AreEqual(1, rootItem.Children[0].Children.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_AddServerNode")]
        public void StudioResourceRepository_AddServerNode_NonExistent_ItemAdded()
        {
            //------------Setup for test--------------------------
            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            var connection = new Mock<IEnvironmentConnection>();

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
            var environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            var serverItemModel = new ExplorerItemModel { DisplayName = "localhost", ResourceType = ResourceType.Server, EnvironmentId = environmentModel.ID, ResourcePath = "", ResourceId = Guid.NewGuid() };
            ExplorerItemModel workflowsFolder = new ExplorerItemModel { DisplayName = "WORKFLOWS", ResourceType = ResourceType.Folder, ResourcePath = "WORKFLOWS", EnvironmentId = mockEnvironmentModel.Object.ID, ResourceId = Guid.NewGuid() };
            serverItemModel.Children.Add(workflowsFolder);

            var studioResourceRepository = new StudioResourceRepository(serverItemModel, _invoke);
            resourceModel.Setup(model => model.Category).Returns("WORKFLOWS\\" + resourceModel.Object.DisplayName);
            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(environmentModel);
            new EnvironmentRepository(testEnvironmentRespository);
            IEnvironmentModel internalEnvironmentModel = environmentModel;
            studioResourceRepository.GetCurrentEnvironment = () => internalEnvironmentModel.ID;
            ExplorerItemModel serverExplorerItem = new ExplorerItemModel(studioResourceRepository, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object) { EnvironmentId = Guid.NewGuid(), ResourceType = ResourceType.Server };
            //------------Execute Test---------------------------
            studioResourceRepository.AddServerNode(serverExplorerItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, studioResourceRepository.ExplorerItemModels.Count);
            Assert.IsTrue(studioResourceRepository.ExplorerItemModels[1].IsExplorerSelected);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_AddServerNode")]
        public void StudioResourceRepository_AddServerNode_OtherServerCollapsed()
        {
            //------------Setup for test--------------------------
            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            var connection = new Mock<IEnvironmentConnection>();

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
            var environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            var serverItemModel = new ExplorerItemModel { DisplayName = "localhost", ResourceType = ResourceType.Server, EnvironmentId = environmentModel.ID, ResourcePath = "", ResourceId = Guid.NewGuid() };
            ExplorerItemModel workflowsFolder = new ExplorerItemModel { DisplayName = "WORKFLOWS", ResourceType = ResourceType.Folder, ResourcePath = "WORKFLOWS", EnvironmentId = mockEnvironmentModel.Object.ID, ResourceId = Guid.NewGuid() };
            serverItemModel.Children.Add(workflowsFolder);

            var studioResourceRepository = new StudioResourceRepository(serverItemModel, _invoke);
            resourceModel.Setup(model => model.Category).Returns("WORKFLOWS\\" + resourceModel.Object.DisplayName);
            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(environmentModel);
            new EnvironmentRepository(testEnvironmentRespository);
            IEnvironmentModel internalEnvironmentModel = environmentModel;
            studioResourceRepository.GetCurrentEnvironment = () => internalEnvironmentModel.ID;
            ExplorerItemModel serverExplorerItem = new ExplorerItemModel(studioResourceRepository, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object) { EnvironmentId = Guid.NewGuid(), ResourceType = ResourceType.Server };
            //------------Execute Test---------------------------
            studioResourceRepository.AddServerNode(serverExplorerItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, studioResourceRepository.ExplorerItemModels.Count);
            Assert.IsTrue(studioResourceRepository.ExplorerItemModels[1].IsExplorerSelected);
            Assert.IsFalse(studioResourceRepository.ExplorerItemModels[0].IsExplorerSelected);
            Assert.IsFalse(studioResourceRepository.ExplorerItemModels[0].IsExplorerExpanded);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StudioResourceRepository_AddServerNode")]
        public void StudioResourceRepository_AddServerNode_Existing_ItemExpandedAndSelected()
        {
            //------------Setup for test--------------------------
            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            var connection = new Mock<IEnvironmentConnection>();

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(e => e.Connection).Returns(connection.Object);
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
            var environmentModel = Dev2MockFactory.SetupEnvironmentModel(resourceModel, new List<IResourceModel>()).Object;

            var serverItemModel = new ExplorerItemModel { DisplayName = "localhost", ResourceType = ResourceType.Server, EnvironmentId = environmentModel.ID, ResourcePath = "", ResourceId = Guid.NewGuid() };
            ExplorerItemModel workflowsFolder = new ExplorerItemModel { DisplayName = "WORKFLOWS", ResourceType = ResourceType.Folder, ResourcePath = "WORKFLOWS", EnvironmentId = mockEnvironmentModel.Object.ID, ResourceId = Guid.NewGuid() };
            serverItemModel.Children.Add(workflowsFolder);

            var studioResourceRepository = new StudioResourceRepository(serverItemModel, _invoke);
            resourceModel.Setup(model => model.Category).Returns("WORKFLOWS\\" + resourceModel.Object.DisplayName);
            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(environmentModel);
            new EnvironmentRepository(testEnvironmentRespository);
            IEnvironmentModel internalEnvironmentModel = environmentModel;
            studioResourceRepository.GetCurrentEnvironment = () => internalEnvironmentModel.ID;
            ExplorerItemModel serverExplorerItem = new ExplorerItemModel(studioResourceRepository, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object) { EnvironmentId = Guid.NewGuid(), ResourceType = ResourceType.Server };
            studioResourceRepository.AddServerNode(serverExplorerItem);
            //------------Execute Test---------------------------
            studioResourceRepository.AddServerNode(serverExplorerItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, studioResourceRepository.ExplorerItemModels.Count);
            Assert.IsTrue(studioResourceRepository.ExplorerItemModels[1].IsExplorerSelected);
            Assert.IsTrue(studioResourceRepository.ExplorerItemModels[1].IsExplorerExpanded);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioResourceRepository_GetEnvironmentModel")]
        public void StudioResourceRepository_GetEnvironmentModel_NullConnectionURIReturnsFalse()
        {
            var environmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();

            environmentModel.Setup(a => a.Connection).Returns(mockConnection.Object);

            Assert.IsFalse(StudioResourceRepository.GetEnvironmentModel(environmentModel.Object, new ServerExplorerItem { WebserverUri = "bob" }, Guid.NewGuid()));
        }

        private IExplorerItem GetTestData(string workFlowId = "DF279411-F678-4FCC-BE88-A1B613EE51E3",
                                          string dbServiceId = "DF279411-F678-4FCC-BE88-A1B613EE51E3", Guid? folderID = null)
        {
            var workflow1 = new ServerExplorerItem
                {
                    ResourceType = ResourceType.WorkflowService,
                    DisplayName = "workflow1",
                    ResourceId = string.IsNullOrEmpty(workFlowId) ? Guid.NewGuid() : Guid.Parse(workFlowId),
                    Permissions = Permissions.Administrator
                };

            var dbService1 = new ServerExplorerItem { ResourceType = ResourceType.DbService, DisplayName = "dbService1", ResourceId = string.IsNullOrEmpty(dbServiceId) ? Guid.NewGuid() : Guid.Parse(dbServiceId), Permissions = Permissions.Contribute };
            var webService1 = new ServerExplorerItem { ResourceType = ResourceType.WebService, DisplayName = "webService1", ResourceId = Guid.NewGuid(), Permissions = Permissions.View };
            var pluginService1 = new ServerExplorerItem { ResourceType = ResourceType.PluginService, DisplayName = "pluginService1", ResourceId = Guid.NewGuid(), Permissions = Permissions.View };
            var dbSource1 = new ServerExplorerItem { ResourceType = ResourceType.DbSource, DisplayName = "dbSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var webSource1 = new ServerExplorerItem { ResourceType = ResourceType.WebSource, DisplayName = "webSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var pluginSource1 = new ServerExplorerItem { ResourceType = ResourceType.PluginSource, DisplayName = "pluginSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var emailSource1 = new ServerExplorerItem { ResourceType = ResourceType.EmailSource, DisplayName = "emailSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var serverSource1 = new ServerExplorerItem { ResourceType = ResourceType.ServerSource, DisplayName = "serverSource1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var folder1 = new ServerExplorerItem { ResourceType = ResourceType.Folder, DisplayName = "folder1", ResourceId = folderID ?? Guid.NewGuid(), Permissions = Permissions.Administrator };
            var folder2 = new ServerExplorerItem { ResourceType = ResourceType.Folder, DisplayName = "folder2", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var subfolder1 = new ServerExplorerItem { ResourceType = ResourceType.Folder, DisplayName = "subfolder1", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };
            var localhost = new ServerExplorerItem { ResourceType = ResourceType.Server, DisplayName = "localhost", ResourceId = Guid.NewGuid(), Permissions = Permissions.Administrator };

            dbService1.Parent = webService1.Parent = pluginService1.Parent = subfolder1.Parent = folder1;
            dbSource1.Parent = webSource1.Parent = pluginSource1.Parent = emailSource1.Parent = serverSource1.Parent = folder2;

            folder2.Children = new List<IExplorerItem>
                {
                    dbSource1,
                    webSource1,
                    pluginSource1,
                    emailSource1,
                    serverSource1
                };


            folder1.Children = new List<IExplorerItem>
                {
                    dbService1, 
                    webService1,
                    pluginService1, 
                    subfolder1
                };

            localhost.Children = new List<IExplorerItem> { folder1, workflow1 };
            workflow1.Parent = localhost;
            folder1.Parent = localhost;

            return localhost;
        }
    }
}
