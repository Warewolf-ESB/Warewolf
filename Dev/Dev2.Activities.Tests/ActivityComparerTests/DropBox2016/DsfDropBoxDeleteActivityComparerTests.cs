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
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.DeleteActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.State;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Tests.Activities.ActivityTests.DropBox2016;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityComparerTests.DropBox2016
{
    [TestClass]
    public class DsfDropBoxDeleteActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "a" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "A" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "AAA" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfDropBoxDeleteActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "a" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "AAA" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DeletePath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "a" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DeletePath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "A" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DeletePath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "AAA" };
            var multiAssign1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, DeletePath = "aaa" };
            //---------------Assert DsfDropBoxDeleteActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DropBoxSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dropBoxDeleteActivity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object) { UniqueID = uniqueId, Result = "A" })
            {
                dropBoxDeleteActivity.SetupDropboxClient("");

                var dropBoxDeleteActivity1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
                //---------------Execute Test ----------------------
                dropBoxDeleteActivity.SelectedSource = new DropBoxSource()
                {
                    ResourceID = Guid.NewGuid()
                };
                dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
                var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
                //---------------Test Result -----------------------
                Assert.IsFalse(equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_DropBoxSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dropBoxDeleteActivity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dropBoxDeleteActivity.SetupDropboxClient("");

                var dropBoxDeleteActivity1 = new DsfDropBoxDeleteActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
                //---------------Execute Test ----------------------
                dropBoxDeleteActivity.SelectedSource = new DropBoxSource();
                dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
                var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
                //---------------Test Result -----------------------
                Assert.IsTrue(equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var selectedSource = new MockOAuthSource(uniqueId);
            
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //------------Setup for test--------------------------
            using (var dropBoxDeleteActivity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object) { SelectedSource = selectedSource, DeletePath = "Path", Result = "Deleted" })
            {
                dropBoxDeleteActivity.SetupDropboxClient("");

                {
                    //------------Execute Test---------------------------
                    var stateItems = dropBoxDeleteActivity.GetState();
                    Assert.AreEqual(3, stateItems.Count());

                    var expectedResults = new[]
                    {
                new StateVariable
                {
                    Name = "SelectedSource.ResourceID",
                    Type = StateVariable.StateType.Input,
                    Value = uniqueId.ToString()
                },
                new StateVariable
                {
                    Name = "DeletePath",
                    Type = StateVariable.StateType.Input,
                    Value = "Path"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "Deleted"
                }
            };

                    var iter = dropBoxDeleteActivity.GetState().Select(
                        (item, index) => new
                        {
                            value = item,
                            expectValue = expectedResults[index]
                        }
                        );

                    //------------Assert Results-------------------------
                    foreach (var entry in iter)
                    {
                        Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                        Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                        Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
                    }
                }
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void DsfDropBoxDeleteActivity_PerformExecution_GivenNoPaths_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxDeleteActivityMock(mockExecutor.Object, dropboxClient.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            dsfDropBoxUploadAcivtityMock.PerfomBaseExecution(new Dictionary<string, string>());
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_PerformExecution_DropboxUploadSuccessResult_GivenPaths_ExpectSuccess()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            dropboxClient.Setup(wrapper => wrapper.UploadAsync(It.IsAny<string>(), It.IsAny<WriteMode>(), It.IsAny<bool>(), null, It.IsAny<bool>(), It.IsAny<MemoryStream>()))
                .Returns(Task.FromResult(TestConstant.FileMetadataInstance.Value));
            mockExecutor.Setup(executor => executor.ExecuteTask(dropboxClient.Object))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxDeleteAcivtityMock = new DsfDropBoxDeleteActivityMock(mockExecutor.Object, dropboxClient.Object)
            {
                IsUplodValidSuccess = true,
                SelectedSource = new DropBoxSource()
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxDeleteAcivtityMock);
            //---------------Execute Test ----------------------
            var location = Assembly.GetExecutingAssembly().Location;
            var listPerfomBaseExecution = dsfDropBoxDeleteAcivtityMock.PerfomBaseExecution(new Dictionary<string, string>
            {
                {"DeletePath",location },
            });
            //---------------Test Result -----------------------
            Assert.AreEqual("Success", listPerfomBaseExecution);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        [ExpectedException(typeof(Exception))]
        public void DsfDropBoxDeleteActivity_PerformExecution_DropboxUploadSuccessResult_GivenPaths_ExpectException()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();

            Task<Metadata> value = null;

            dropboxClient.Setup(wrapper => wrapper.UploadAsync(It.IsAny<string>(), It.IsAny<WriteMode>(), It.IsAny<bool>(), null, It.IsAny<bool>(), It.IsAny<MemoryStream>()))
                .Returns(Task.FromResult(TestConstant.FileMetadataInstance.Value));
            dropboxClient.Setup(wrapper => wrapper.DeleteAsync(It.IsAny<string>())).Returns(value);

            mockExecutor.Setup(executor => executor.ExecuteTask(dropboxClient.Object))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxDeleteAcivtityMock = new DsfDropBoxDeleteActivityMock(mockExecutor.Object, dropboxClient.Object)
            {
                IsUplodValidSuccess = true,
                SelectedSource = new DropBoxSource()
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxDeleteAcivtityMock);
            //---------------Execute Test ----------------------
            var location = Assembly.GetExecutingAssembly().Location;
            var listPerfomBaseExecution = dsfDropBoxDeleteAcivtityMock.PerfomBaseExecution(new Dictionary<string, string>
            {
                {"DeletePath",location },
            });
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_ObjectEquals_IsFalse_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var obj = new object();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDeleteAcivtity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object))
            {
                dsfDropBoxDeleteAcivtity.SetupDropboxClient("");

                obj = new DsfDropBoxDeleteActivity();
                //-----------------------Act--------------------------------
                //-----------------------Assert-----------------------------
                Assert.IsFalse(dsfDropBoxDeleteAcivtity.Equals(obj));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_ObjectEquals_IsTrue_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var obj = new object();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDeleteAcivtity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object))
            {
                dsfDropBoxDeleteAcivtity.SetupDropboxClient("");

                obj = dsfDropBoxDeleteAcivtity;
                //-----------------------Act--------------------------------
                //-----------------------Assert-----------------------------
                Assert.IsTrue(dsfDropBoxDeleteAcivtity.Equals(obj));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_ObjectEquals_IsNotExpectedObject_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDeleteAcivtity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object))
            {
                dsfDropBoxDeleteAcivtity.SetupDropboxClient("");
                //-----------------------Act--------------------------------
                //-----------------------Assert-----------------------------
                Assert.IsFalse(dsfDropBoxDeleteAcivtity.Equals(new object()));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Equals_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDeleteAcivtity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object))
            {
                dsfDropBoxDeleteAcivtity.SetupDropboxClient("");
                //-----------------------Act--------------------------------
                //-----------------------Assert-----------------------------
                var dsfDropBoxDeleteActivity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object);
                Assert.IsFalse(dsfDropBoxDeleteAcivtity.Equals(dsfDropBoxDeleteActivity));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Equals_IsNull_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDeleteAcivtity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object))
            {
                dsfDropBoxDeleteAcivtity.SetupDropboxClient("");
                //-----------------------Act--------------------------------
                //-----------------------Assert-----------------------------
                Assert.IsFalse(dsfDropBoxDeleteAcivtity.Equals(null));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_Equals_IsEqual_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxDeleteAcivtity = new DsfDropBoxDeleteActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsTrue(dsfDropBoxDeleteAcivtity.Equals(dsfDropBoxDeleteAcivtity));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_GetHashCode_Properties_NotNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDeleteAcivtity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object)
            {
                
                SelectedSource = new DropBoxSource(),
                DisplayName = "TestDisplayName"
            })
            {
                dsfDropBoxDeleteAcivtity.SetupDropboxClient("");
                //--------------------------Act--------------------------------
                var getHash = dsfDropBoxDeleteAcivtity.GetHashCode();
                //--------------------------Assert-----------------------------
                Assert.IsNotNull(getHash);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_GetHashCode_Properties_IsNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDeleteAcivtity = new DsfDropBoxDeleteActivityMock(mockDropboxClientFactory.Object))
            {
                dsfDropBoxDeleteAcivtity.SetupDropboxClient("");
                //--------------------------Act--------------------------------
                var getHash = dsfDropBoxDeleteAcivtity.GetHashCode();
                //--------------------------Assert-----------------------------
                Assert.IsNotNull(getHash);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_ExecuteTool_GivenNoFromPath_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxDeleteAcivtityMock = new DsfDropBoxDeleteActivityMock(mockExecutor.Object, dropboxClient.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxDeleteAcivtityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);

            var dataObject = datObj.Object;
            dsfDropBoxDeleteAcivtityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
            executionEnvironment.Verify(environment => environment.AddError("Please confirm that the correct file location has been entered"));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_ExecuteTool_GivenNoToPath_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxDeleteAcivtityMock = new DsfDropBoxDeleteActivityMock(mockExecutor.Object, dropboxClient.Object)
            {
                IsUplodValidSuccess = true,
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxDeleteAcivtityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);

            var dataObject = datObj.Object;
            dsfDropBoxDeleteAcivtityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
            datObj.VerifyAll();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDeleteActivity))]
        public void DsfDropBoxDeleteActivity_GetFindMissingType_ExpectStaticActivity()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var dropboxClient = new Mock<IDropboxClient>();

            var dsfDropBoxDeleteAcivtityMock = new DsfDropBoxDeleteActivityMock(mockExecutor.Object, dropboxClient.Object)
            {
                IsUplodValidSuccess = true,
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxDeleteAcivtityMock);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.StaticActivity, dsfDropBoxDeleteAcivtityMock.GetFindMissingType());
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

        public class DsfDropBoxDeleteActivityMock : DsfDropBoxDeleteActivity
        {
            public DsfDropBoxDeleteActivityMock(IDropboxClientFactory dropboxClientFactory)
                : base(dropboxClientFactory)
            {

            }

            public DsfDropBoxDeleteActivityMock(IDropboxSingleExecutor<IDropboxResult> singleExecutor, IDropboxClient dropboxClient
                )
                : base(new MyClass(dropboxClient))
            {
                DropboxSingleExecutor = singleExecutor;
            }

            public static void Execute(out ErrorResultTO tmpErrors)
            {
                tmpErrors = new ErrorResultTO();
            }
            
            protected override void ExecuteTool(IDSFDataObject dataObject, int update)
            {
                base.ExecuteTool(dataObject, update);
            }

            public new void SetupDropboxClient(string accessToken)
            {
                base.SetupDropboxClient(accessToken);
            }

            public string PerfomBaseExecution(Dictionary<string, string> dictionaryValues)
            {
                var perfomBaseExecution = base.PerformExecution(dictionaryValues);
                return perfomBaseExecution[0];
            }

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
}
