/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;



namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Upload
{
    [TestClass]
    public class DsfDropBoxUploadActivityTests
    {
        static DsfDropBoxUploadActivity CreateDropboxActivity()
        {
            return new DsfDropBoxUploadActivity();
        }

        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_DsfDropBoxUpload_GivenNewInstance_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(boxUploadAcivtity);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_CreateNewActivity_GivenIsNew_ShouldHaveDisplayName()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(boxUploadAcivtity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("Upload to Dropbox", boxUploadAcivtity.DisplayName);

        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_GetFindMissingType_GivenIsNew_ShouldSetDatagridAcitivity()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var enFindMissingType = boxUploadAcivtity.GetFindMissingType();
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.StaticActivity, enFindMissingType);

        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUploadActivity_GetDebugOutputs_GivenNullEnvironment_ShouldHaveNoDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = boxUploadAcivtity.GetDebugOutputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUploadActivity_GetDebugOutputs_GivenFileMetadataIsNull_ShouldHaveNoDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = boxUploadAcivtity.GetDebugOutputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUploadActivity_GetDebugOutputs_GivenFileMetadataIsNotNull_ShouldHaveOneDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = new Mock<DsfDropBoxUploadActivity>();
            boxUploadAcivtity.SetupAllProperties();
            boxUploadAcivtity.Setup(acivtity => acivtity.GetDebugOutputs(It.IsAny<IExecutionEnvironment>(), It.IsAny<int>()))
                .Returns(new List<DebugItem>()
                {
                    new DebugItem()
                });
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = boxUploadAcivtity.Object.GetDebugOutputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugOutputs.Count);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUploadActivity_GetDebugOutputs_GivenWebRequestSuccess_ShouldCorrectDebugValue()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new TestDsfDropBoxUploadActivity(mockExecutor.Object, dropboxClient.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var esbChannel = new Mock<IEsbChannel>().Object;
            var datObj = new Mock<IDSFDataObject>().Object;
            var executionEnvironment = new Mock<IExecutionEnvironment>().Object;
            
            var errorResultTO = new ErrorResultTO();
            dsfDropBoxUploadAcivtityMock.Execute(esbChannel, datObj, String.Empty, String.Empty, out errorResultTO, 0);
            var debugOutputs = dsfDropBoxUploadAcivtityMock.GetDebugOutputs(executionEnvironment, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUploadActivity_ExecuteTool_GivenNoFromPath_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new TestDsfDropBoxUploadActivity(mockExecutor.Object, dropboxClient.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            
            var dataObject = datObj.Object;
            dsfDropBoxUploadAcivtityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
            executionEnvironment.Verify(environment => environment.AddError("Please confirm that the correct file location has been entered"));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUploadActivity_ExecuteTool_GivenNoToPath_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new TestDsfDropBoxUploadActivity(mockExecutor.Object, dropboxClient.Object)
            {
                IsUplodValidSuccess = true,
                FromPath = "File.txt"
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);

            var dataObject = datObj.Object;
            dsfDropBoxUploadAcivtityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
            executionEnvironment.Verify(environment => environment.AddError("Please confirm that the correct file destination has been entered"));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void DsfDropBoxUploadActivity_PerformExecution_GivenNoPaths_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new TestDsfDropBoxUploadActivity(mockExecutor.Object, dropboxClient.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            dsfDropBoxUploadAcivtityMock.PerfomBaseExecution(new Dictionary<string, string>());
            //---------------Test Result -----------------------
            Assert.Fail("Exception Not Throw");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUploadActivity_PerformExecution_DropboxUploadSuccessResult_GivenPaths_ShouldPassthrough()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            dropboxClient.Setup(wrapper => wrapper.UploadAsync(It.IsAny<string>(), It.IsAny<WriteMode>(), It.IsAny<bool>(), null, It.IsAny<bool>(), It.IsAny<MemoryStream>()))
                .Returns(Task.FromResult(TestConstant.FileMetadataInstance.Value));
            mockExecutor.Setup(executor => executor.ExecuteTask(dropboxClient.Object))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new TestDsfDropBoxUploadActivity(mockExecutor.Object, dropboxClient.Object)
            {
                IsUplodValidSuccess = true,
                SelectedSource = new DropBoxSource()
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var location = Assembly.GetExecutingAssembly().Location;
            dsfDropBoxUploadAcivtityMock.PerfomBaseExecution(new Dictionary<string, string>()
            {
                {"ToPath","a" },
                {"FromPath",location },
                
            });
            //---------------Test Result -----------------------
            dropboxClient.Verify(wrapper => wrapper.UploadAsync(It.IsAny<string>(), It.IsAny<WriteMode>(), It.IsAny<bool>(), null, It.IsAny<bool>(), It.IsAny<MemoryStream>()));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUploadActivity_PerformExecution_DropboxFailureResult_GivenPaths_ShouldPassthrough()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            dropboxClient.Setup(wrapper => wrapper.UploadAsync(It.IsAny<string>(), It.IsAny<WriteMode>(), It.IsAny<bool>(), null, It.IsAny<bool>(), It.IsAny<MemoryStream>()))
                .Returns(Task.FromResult(TestConstant.FileMetadataInstance.Value));
            mockExecutor.Setup(executor => executor.ExecuteTask(dropboxClient.Object))
                .Returns(new DropboxFailureResult(new Exception("Test Error: not_file")));
            var dsfDropBoxUploadAcivtityMock = new TestDsfDropBoxUploadActivity(mockExecutor.Object, dropboxClient.Object)
            {
                IsUplodValidSuccess = false,
                SelectedSource = new DropBoxSource()
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var location = Assembly.GetExecutingAssembly().Location;
            dsfDropBoxUploadAcivtityMock.PerfomBaseExecution(new Dictionary<string, string>()
            {
                {"ToPath","a" },
                {"FromPath",location },

            });
            //---------------Test Result -----------------------
            dropboxClient.Verify(wrapper => wrapper.UploadAsync(It.IsAny<string>(), It.IsAny<WriteMode>(), It.IsAny<bool>(), null, It.IsAny<bool>(), It.IsAny<MemoryStream>()));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUploadActivity_GetDebugInputs_GivenEnvironment_ShouldhaveDebugInputs()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new TestDsfDropBoxUploadActivity(mockExecutor.Object, dropboxClient.Object)
            {
                IsUplodValidSuccess = true,
                ToPath = "DDD",
                FromPath = "DDD",
                AddMode = true
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var mock = new Mock<IExecutionEnvironment>();
            var debugInputs = dsfDropBoxUploadAcivtityMock.GetDebugInputs(mock.Object, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, debugInputs.Count);
        }
    }

    class MyClass : IDropboxClientFactory
    {
        private readonly IDropboxClient _dropboxClient;
        public MyClass(IDropboxClient dropboxClient)
        {
            _dropboxClient = dropboxClient;
        }
        public IDropboxClient CreateWithSecret(string accessToken)
        {
            return _dropboxClient;
        }

        public IDropboxClient New(string accessToken, HttpClient httpClient)
        {
            return _dropboxClient;
        }
    }

    public class TestDsfDropBoxUploadActivity : DsfDropBoxUploadActivity
    {
        public TestDsfDropBoxUploadActivity(IDropboxSingleExecutor<IDropboxResult> singleExecutor, IDropboxClient dropboxClient
            )
            : base(new MyClass(dropboxClient))
        {
            DropboxSingleExecutor = singleExecutor;
        }

        public void Execute(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
        }

        #region Overrides of DsfDropBoxUploadActivity

        #endregion
        

        #region Overrides of DsfDropBoxUploadActivity

        
        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            base.ExecuteTool(dataObject, update);
        }

        #endregion

        public string PerfomBaseExecution(Dictionary<string, string> dictionaryValues)
        {
            var perfomBaseExecution = base.PerformExecution(dictionaryValues);
            return perfomBaseExecution[0];
        }

        #region Overrides of DsfNativeActivity<string>



        #endregion

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                var dropboxResult = DropboxSingleExecutor.ExecuteTask(_dropboxClient);
                if (!IsUplodValidSuccess)
                {
                    Exception = ((DropboxFailureResult)dropboxResult).GetException();
                }

                return new List<string> { string.Empty };
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
                Exception = new DropboxFailureResult(new Exception()).GetException();
                return new List<string> { string.Empty };
            }
        }

        public bool IsUplodValidSuccess { get; set; }


    }


}