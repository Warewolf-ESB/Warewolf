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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.State;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityComparerTests.DropBox2016
{
    [TestClass]
    public class DsfDropBoxDownloadActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, DisplayName = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, DisplayName = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, DisplayName = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfDropBoxDownloadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_FromPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, FromPath = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, FromPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_FromPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, FromPath = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, FromPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_FromPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, FromPath = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, FromPath = "aaa" };
            //---------------Assert DsfDropBoxDownloadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ToPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, ToPath = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, ToPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ToPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, ToPath = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, ToPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ToPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, ToPath = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, ToPath = "aaa" };
            //---------------Assert DsfDropBoxDownloadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_DropBoxSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropBoxDownloadActivity.SetupDropboxClient("");

                var dsfDropBoxDownloadActivityOther = new TestDsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxDownloadActivity.SelectedSource = new DropBoxSource
                {
                    ResourceID = Guid.NewGuid()
                };
                dsfDropBoxDownloadActivityOther.SelectedSource = new DropBoxSource();
                var @equals = dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_DropBoxSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropBoxDownloadActivity.SetupDropboxClient("");

                var dsfDropBoxDownloadActivityOther = new TestDsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxDownloadActivity.SelectedSource = new DropBoxSource();
                dsfDropBoxDownloadActivityOther.SelectedSource = new DropBoxSource();
                var @equals = dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_OverwriteFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A",
            })
            {
                dsfDropBoxDownloadActivity.SetupDropboxClient("");

                var dsfDropBoxDownloadActivityOther = new TestDsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxDownloadActivity.OverwriteFile = true;
                dsfDropBoxDownloadActivityOther.OverwriteFile = false;
                var @equals = dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_OverwriteFile_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropBoxDownloadActivity.SetupDropboxClient("");

                var dsfDropBoxDownloadActivityOther = new TestDsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxDownloadActivity.OverwriteFile = true;
                dsfDropBoxDownloadActivityOther.OverwriteFile = true;
                var @equals = dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var selectedSource = new MockOAuthSource(uniqueId);

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //------------Setup for test--------------------------
            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = selectedSource,
                FromPath = "Path_From",
                ToPath = "Path_To",
                OverwriteFile = false,
                Result = "Downloaded"
            })
            {
                dsfDropBoxDownloadActivity.SetupDropboxClient("");

                //------------Execute Test---------------------------
                var stateItems = dsfDropBoxDownloadActivity.GetState();
                Assert.AreEqual(5, stateItems.Count());

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
                    Name = "FromPath",
                    Type = StateVariable.StateType.Input,
                    Value = "Path_From"
                },
                new StateVariable
                {
                    Name = "ToPath",
                    Type = StateVariable.StateType.Input,
                    Value = "Path_To"
                },
                new StateVariable
                {
                    Name = "OverwriteFile",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "Downloaded"
                }
            };

                var iter = dsfDropBoxDownloadActivity.GetState().Select(
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ObjectEquals_IsFalse_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var obj = new object();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object))
            {
                dsfDropBoxDownloadActivity.SetupDropboxClient("");

                obj = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object);
                //-----------------------Act--------------------------------
                //-----------------------Assert-----------------------------
                Assert.IsFalse(dsfDropBoxDownloadActivity.Equals(obj));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ObjectEquals_IsTrue_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var obj = new object();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object))
            {
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                obj = dsfDropBoxDownloadActivity;
                //-----------------------Act--------------------------------
                //-----------------------Assert-----------------------------
                Assert.IsTrue(dsfDropBoxDownloadActivity.Equals(obj));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ObjectEquals_IsNotExpectedObject_ExpectFalse()
        {
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //-----------------------Arrange----------------------------
            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object))
            {
                //-----------------------Act--------------------------------
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.IsFalse(dsfDropBoxDownloadActivity.Equals(new object()));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Equals_ExpectFalse()
        {
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //-----------------------Arrange----------------------------
            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object))
            {
                //-----------------------Act--------------------------------
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                var dsfDropBoxDownloadActivityOther = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object);
                Assert.IsFalse(dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivityOther));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Equals_IsNull_ExpectFalse()
        {
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //-----------------------Arrange----------------------------
            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object))
            {
                //-----------------------Act--------------------------------
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.IsFalse(dsfDropBoxDownloadActivity.Equals(null));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Equals_IsEqual_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsTrue(dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivity));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_GetDebugOutputs_ExecutionEnvironment_IsNull_ExpectFalse()
        {
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //-----------------------Arrange----------------------------
            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object))
            {
                //-----------------------Act--------------------------------
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.AreEqual(0, dsfDropBoxDownloadActivity.GetDebugInputs(null, 0).Count);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_GetDebugOutputs_ExecutionEnvironment_IsNotNull_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object))
            {
                //-----------------------Act--------------------------------
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.AreEqual(1, dsfDropBoxDownloadActivity.GetDebugInputs(mockExecutionEnvironment.Object, 0).Count);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_LocalPathManager_SetProperty_AreEqual_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var mockLocalPathManager = new Mock<ILocalPathManager>();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //-----------------------Act--------------------------------
            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                LocalPathManager = mockLocalPathManager.Object
            })
            {
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.AreEqual(mockLocalPathManager.Object, dsfDropBoxDownloadActivity.LocalPathManager);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ExecuteTool_FromPath_IsNullOrEmpty_VerifyMethodCall_3Times_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            mockDSFDataObject.Setup(o => o.Environment.AddError(It.IsAny<string>()));

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                FromPath = null,
                ToPath = null
            })
            {
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                //-----------------------Act--------------------------------
                dsfDropBoxDownloadActivity.TestExecuteTool(mockDSFDataObject.Object, 0);
                //-----------------------Assert-----------------------------
                mockDSFDataObject.Verify(o => o.Environment.AddError(It.IsAny<string>()), Times.Exactly(3));
            }
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_GetHashCode_PropertiesNull_IsNull_ExpectTrue()
        {
            var mockDropboxClient = new Mock<IDropboxClient>();

            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            //-----------------------Arrange----------------------------
            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object))
            {
                //-----------------------Act--------------------------------
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.IsNotNull(dsfDropBoxDownloadActivity.GetHashCode());
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_GetHashCode_PropertiesNull_IsNotNull_ExpectTrue()
        {
            var mockDropboxClient = new Mock<IDropboxClient>();

            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            //-----------------------Arrange----------------------------
            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                ToPath = "TestToPath",
                FromPath = "TestFromPath",
                SelectedSource = new DropBoxSource()
            })
            {
                //-----------------------Act--------------------------------
                dsfDropBoxDownloadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.IsNotNull(dsfDropBoxDownloadActivity.GetHashCode());
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_PerformExecution_ExpectSuccess()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockFile = new Mock<IFile>();

            using (var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object))
            {
                mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
                task.Start();
                mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
                mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

                using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
                {
                    DropboxFile = mockFile.Object
                })
                {
                    var dictionary = new Dictionary<string, string>
                    {
                        { "ToPath", "TestToPath" },
                        { "FromPath", @"C:\Users\temp" }
                    };
                    //-----------------------Act--------------------------------
                    var list = dsfDropBoxDownloadActivity.TestPerformExecution(dictionary);
                    //-----------------------Assert-----------------------------
                    mockDownloadResponse.VerifyAll();
                    mockDropboxClient.VerifyAll();
                    mockDropboxClientFactory.VerifyAll();
                    mockFile.VerifyAll();
                    Assert.AreEqual(1, list.Count);
                    Assert.AreEqual("Success", list[0]);
                }
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_PerformExecution_ExpectException()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockFile = new Mock<IFile>();

            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);

            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxFailureResult(new Exception("test error!")));

            using (var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                DropboxFile = mockFile.Object,
                MockSingleExecutor = mockDropboxSingleExecutor
            })
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "ToPath", @"C:\Users\temp\testToPath\" },
                    { "FromPath", @"C:\Users\temp" }
                };

                try
                {
                    dsfDropBoxDownloadActivity.TestPerformExecution(dictionary);
                    Assert.Fail("Expected exception");
                } catch (Exception e)
                {
                    Assert.AreEqual("test error!", e.Message);
                }
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_PerformExecution_ContainsNotFile_ExpectException()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockFile = new Mock<IFile>();

            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxFailureResult(new Exception("test error: not_file!")));

            var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                DropboxFile = mockFile.Object,
                MockSingleExecutor = mockDropboxSingleExecutor,
            };

            var dictionary = new Dictionary<string, string>
            {
                { "ToPath", @"C:\Users\temp\testToPath\" },
                { "FromPath", @"C:\Users\temp" }
            };
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.ThrowsException<Exception>(() => dsfDropBoxDownloadActivity.TestPerformExecution(dictionary));
            dsfDropBoxDownloadActivity.Dispose();
        }
    }

    class TestDsfDropBoxDownloadActivity : DsfDropBoxDownloadActivity
    {
        public Mock<IDropboxSingleExecutor<IDropboxResult>> MockSingleExecutor { get; internal set; }

        public TestDsfDropBoxDownloadActivity(IDropboxClientFactory dropboxClientFactory)
            : base(dropboxClientFactory)
        {

        }
        public TestDsfDropBoxDownloadActivity()
            : this(new DropboxClientWrapperFactory())
        {

        }
        public void TestExecuteTool(IDSFDataObject dataObject, int update)
        {
            base.ExecuteTool(dataObject, update);
        }
        public List<string> TestPerformExecution(Dictionary<string, string> evaluatedValues)
        {
            return base.PerformExecution(evaluatedValues);
        }

        public new void SetupDropboxClient(string accessToken)
        {
            base.SetupDropboxClient(accessToken);
        }

        public override IDropboxSingleExecutor<IDropboxResult> GetDropboxSingleExecutor(IDropboxSingleExecutor<IDropboxResult> singleExecutor)
        {
            if (MockSingleExecutor is null)
            {
                return singleExecutor;
            }
            return MockSingleExecutor.Object;
        }
    }
}