﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.DropboxFileActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dropbox.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.DropboxFiles
{
    [TestClass]
    public class DropboxFileListActivityShould
    {
        private static DsfDropboxFileListActivity CreateDropboxActivity()
        {
            return new DsfDropboxFileListActivity();
        }

        private static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropboxFileList_GivenNewInstance_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxFileListActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxFileListActivity);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewActivity_GivenIsNew_ShouldHaveDisplayName()
        {
            //---------------Set up test pack-------------------
            var dropboxFileListActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("List Dropbox files", dropboxFileListActivity.DisplayName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFindMissingType_GivenIsNew_ShouldSetStaticActivity()
        {
            //---------------Set up test pack-------------------
            var dropboxFileListActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var enFindMissingType = dropboxFileListActivity.GetFindMissingType();
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.StaticActivity, enFindMissingType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentIsNull_ShouldHaveNoDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var dropboxFileListActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = dropboxFileListActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironmentAndFromPath_ShouldHaveOneDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var dropboxFileListActivity = CreateDropboxActivity();
            dropboxFileListActivity.ToPath = "Random.txt";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var debugInputs = dropboxFileListActivity.GetDebugInputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenWebRequestSuccess_ShouldCorrectDebugValue()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropBoxDownloadActivityMock = new DsfDropboxFileListActivity();
            dropBoxDownloadActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxDownloadActivityMock);
            //---------------Execute Test ----------------------
            var executionEnvironment = new Mock<IExecutionEnvironment>().Object;
            // ReSharper disable once RedundantAssignment
            var debugOutputs = dropBoxDownloadActivityMock.GetDebugOutputs(executionEnvironment, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenFileMetadataIsNotNull_ShouldHaveOneDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = new Mock<DsfDropboxFileListActivity>();
            dropBoxDownloadActivity.SetupAllProperties();
            dropBoxDownloadActivity.Setup(
                acivtity => acivtity.GetDebugOutputs(It.IsAny<IExecutionEnvironment>(), It.IsAny<int>()))
                .Returns(new List<DebugItem>
                {
                    new DebugItem()
                });
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = dropBoxDownloadActivity.Object.GetDebugOutputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugOutputs.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTool_GivenPaths_ShouldExecuteTool()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivity = new DsfDropboxFileListActivity {ToPath = "Test.a"};
            dropboxFileListActivity.GetDropboxSingleExecutor(mockExecutor.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivity);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            // ReSharper disable once RedundantAssignment
            var dataObject = datObj.Object;
            var dev2Activity = dropboxFileListActivity.Execute(dataObject, 0);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof (ArgumentException))]
        public void PerformExecution_GivenNoPaths_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivityMock();
            dropboxFileListActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            dropboxFileListActivityMock.SelectedSource = new OauthSource
            {
                Secret = "Test"
            };
            //---------------Execute Test ----------------------
            dropboxFileListActivityMock.PerformBaseExecution(new Dictionary<string, string>
            {
                {"ToPath", ""},
                {"IsRecursive", "false"},
                {"IncludeMediaInfo", "false"},
                {"IncludeDeleted", "false"},
                {"IncludeFolders", "false"}
            });
            //---------------Test Result -----------------------
            Assert.Fail("Exception Not Thrown");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_GivenPath_ShouldReturnSuccess()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivityMock
            {
                SelectedSource = new OauthSource
                {
                    Secret = "Test"
                },
                IsFoldersSelected = true,
            };

            ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            //---------------Execute Test ----------------------
            var execution = dropboxFileListActivityMock.PerformBaseExecution(new Dictionary<string, string>
            {
                {"ToPath", "a.txt"},
                {"IsRecursive", "false"},
                {"IncludeMediaInfo", "false"},
                {"IncludeDeleted", "false"},
                {"IncludeFolders", "false"}
            });
            //---------------Test Result -----------------------
            Assert.AreEqual(execution, GlobalConstants.DropBoxSucces);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_GivenPathAndIncludeFolders_ShouldLoadFolders()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivityMock
            {
                SelectedSource = new OauthSource
                {
                    Secret = "Test"
                },
                IsFoldersSelected = true,
            };

            ;


            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            //---------------Execute Test ----------------------
            dropboxFileListActivityMock.PerformBaseExecution(new Dictionary<string, string>
            {
                {"ToPath", "a.txt"},
                {"IsRecursive", "false"},
                {"IncludeMediaInfo", "false"},
                {"IncludeDeleted", "false"},
                {"IncludeFolders", "true"}
            });
            //---------------Test Result -----------------------
            Assert.AreEqual(3, dropboxFileListActivityMock.Files.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_GivenPathAndIsFilesAndFoldersSelected_ShouldLoadFoldersAndFiles()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivityMock
            {
                SelectedSource = new OauthSource
                {
                    Secret = "Test"
                },
                IsFilesAndFoldersSelected = true
            };

            ;


            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            //---------------Execute Test ----------------------
            dropboxFileListActivityMock.PerformBaseExecution(new Dictionary<string, string>
            {
                {"ToPath", "a.txt"},
                {"IsRecursive", "false"},
                {"IncludeMediaInfo", "false"},
                {"IncludeDeleted", "false"},
                {"IncludeFolders", "true"}
            });
            //---------------Test Result -----------------------
            Assert.AreEqual(3, dropboxFileListActivityMock.Files.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void
            PerformExecution_GivenPathAndIsFilesAndFoldersSelectedAndIncludeDeleted_ShouldLoadFoldersAndFilesAndDeleted()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivityMock
            {
                SelectedSource = new OauthSource
                {
                    Secret = "Test"
                },
                IsFilesAndFoldersSelected = true,
                IncludeDeleted = true
            };

            ;


            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            //---------------Execute Test ----------------------
            dropboxFileListActivityMock.PerformBaseExecution(new Dictionary<string, string>
            {
                {"ToPath", "a.txt"},
                {"IsRecursive", "false"},
                {"IncludeMediaInfo", "false"},
                {"IncludeDeleted", "false"},
                {"IncludeFolders", "true"}
            });
            //---------------Test Result -----------------------
            Assert.AreEqual(4, dropboxFileListActivityMock.Files.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_GivenPathAndIncludeDeleted_ShouldLoadDeletedFiles()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivityMock
            {
                SelectedSource = new OauthSource
                {
                    Secret = "Test"
                },
                IncludeDeleted = true
            };

            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            //---------------Execute Test ----------------------
            dropboxFileListActivityMock.PerformBaseExecution(new Dictionary<string, string>
            {
                {"ToPath", "a.txt"},
                {"IsRecursive", "false"},
                {"IncludeMediaInfo", "false"},
                {"IncludeDeleted", "true"},
                {"IncludeFolders", "false"}
            });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, dropboxFileListActivityMock.Files.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_GivenPathNotIncludeFolders_ShouldLoadNotLoadFolders()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivityMock
            {
                SelectedSource = new OauthSource
                {
                    Secret = "Test"
                }
            };

            ;


            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            //---------------Execute Test ----------------------
            dropboxFileListActivityMock.PerformBaseExecution(new Dictionary<string, string>
            {
                {"ToPath", "a.txt"},
                {"IsRecursive", "false"},
                {"IncludeMediaInfo", "false"},
                {"IncludeDeleted", "false"},
                {"IncludeFolders", "false"}
            });
            //---------------Test Result -----------------------
            Assert.AreEqual(0, dropboxFileListActivityMock.Files.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_GivenHasError_ShouldReturnExceptionMessage()
        {
            try
            {
                //---------------Set up test pack-------------------
                var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
                mockExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                    .Returns(new DropboxFailureResult(TestConstant.ExceptionInstance.Value));
                var dropboxFileListActivityMock = new DsfDropboxFileListActivityMock();
                dropboxFileListActivityMock.SelectedSource = 
                    new OauthSource
                {
                    Secret = "Test"
                };
                dropboxFileListActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
               //---------------Assert Precondition----------------
                Assert.IsNotNull(dropboxFileListActivityMock);
                //---------------Execute Test ----------------------
                var execution = dropboxFileListActivityMock.PerformBaseExecution(new Dictionary<string, string>
                {
                    {"ToPath", "a.txt"},
                    {"IsRecursive", "false"},
                    {"IncludeMediaInfo", "false"},
                    {"IncludeDeleted", "false"},
                    {"IncludeFolders", "false"}
                });
            }
            catch (Exception e)
            {
                Assert.AreEqual(TestConstant.ExceptionInstance.Value.Message, e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTool_GivenNoToPath_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivityMock();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            // ReSharper disable once RedundantAssignment
            var dataObject = datObj.Object;
            dropboxFileListActivityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
            executionEnvironment.Verify(
                environment =>
                    environment.AddError("Please confirm that the correct Dropbox file location has been entered"));
        }

        [TestMethod]
        public void GetDropboxClient_GivenActivityIsNewAndSourceIsSelected_ShouldCreateNewClient()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivity();
            dropboxFileListActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            dropboxFileListActivityMock.SelectedSource = new OauthSource
            {
                Secret = "Test"
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            //---------------Execute Test ----------------------

            var dropboxClient = dropboxFileListActivityMock.GetDropboxClient();
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxClient);
        }

        [TestMethod]
        public void GetDropboxClient_GivenClientExists_ShouldReturnExistingClient()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new DsfDropboxFileListActivity();
            dropboxFileListActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            dropboxFileListActivityMock.SelectedSource = new OauthSource
            {
                Secret = "Test"
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileListActivityMock);
            //---------------Execute Test ----------------------
            var dropboxClient = dropboxFileListActivityMock.GetDropboxClient();
            var dropboxClient1 = dropboxFileListActivityMock.GetDropboxClient();

            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxClient);
            Assert.IsNotNull(dropboxClient1);
            Assert.AreEqual(dropboxClient, dropboxClient1);
        }
    }

    public class DsfDropboxFileListActivityMock : DsfDropboxFileListActivity
    {
        public string PerformBaseExecution(Dictionary<string, string> evaluatedValues)
        {
            return base.PerformExecution(evaluatedValues);
        }

        public override IDropboxSingleExecutor<IDropboxResult> GetDropboxSingleExecutor(
            IDropboxSingleExecutor<IDropboxResult> singleExecutor)
        {
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                .Returns(new DropboxListFolderSuccesResult(TestConstant.ListFolderResultInstance.Value));
            var dropboxFileListActivityMock = new Mock<DsfDropboxFileListActivityMock>();
            dropboxFileListActivityMock.Setup(
                mock => mock.GetDropboxSingleExecutor(It.IsAny<IDropboxSingleExecutor<IDropboxResult>>()))
                .Returns(mockExecutor.Object);
            return mockExecutor.Object;
        }
    }
}