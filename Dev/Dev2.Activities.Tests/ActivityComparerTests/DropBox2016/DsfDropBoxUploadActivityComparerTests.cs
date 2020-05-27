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
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.State;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityComparerTests.DropBox2016
{
    [TestClass]
    public class DsfDropBoxUploadActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, DisplayName = "a" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, DisplayName = "A" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, DisplayName = "AAA" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfDropBoxUploadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "a" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "AAA" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_FromPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, FromPath = "a" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, FromPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_FromPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, FromPath = "A" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, FromPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_FromPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, FromPath = "AAA" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, FromPath = "aaa" };
            //---------------Assert DsfDropBoxUploadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ToPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, ToPath = "a" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, ToPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ToPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, ToPath = "A" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, ToPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ToPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity { UniqueID = uniqueId, ToPath = "AAA" };
            var multiAssign1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, ToPath = "aaa" };
            //---------------Assert DsfDropBoxUploadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_DropBoxSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");

                var dsfDropBoxUploadActivityOther = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxUploadActivity.SelectedSource = new DropBoxSource
                {
                    ResourceID = Guid.NewGuid()
                };
                dsfDropBoxUploadActivityOther.SelectedSource = new DropBoxSource();
                var @equals = dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_DropBoxSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");

                var dsfDropBoxUploadActivityOther = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxUploadActivity.SelectedSource = new DropBoxSource();
                dsfDropBoxUploadActivityOther.SelectedSource = new DropBoxSource();
                var @equals = dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_AddMode_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");

                var dsfDropBoxUploadActivityOther = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxUploadActivity.AddMode = true;
                dsfDropBoxUploadActivityOther.AddMode = false;
                var @equals = dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_AddMode_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");

                var dsfDropBoxUploadActivityOther = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxUploadActivity.AddMode = true;
                dsfDropBoxUploadActivityOther.AddMode = true;
                var @equals = dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_OverWriteMode_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");

                var dsfDropBoxUploadActivityOther = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxUploadActivity.OverWriteMode = true;
                dsfDropBoxUploadActivityOther.OverWriteMode = false;
                var @equals = dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_OverWriteMode_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");

                var dsfDropBoxUploadActivityOther = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther));
                //---------------Execute Test ----------------------
                dsfDropBoxUploadActivity.OverWriteMode = true;
                dsfDropBoxUploadActivityOther.OverWriteMode = true;
                var @equals = dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var selectedSource = new MockOAuthSource(uniqueId);

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //------------Setup for test--------------------------
            using (var dropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = selectedSource,
                FromPath = "Path_From",
                ToPath = "Path_To",
                OverWriteMode = false,
                Result = "Uploaded"
            })
            {
                dropBoxUploadActivity.SetupDropboxClient("");

                //------------Execute Test---------------------------
                var stateItems = dropBoxUploadActivity.GetState();
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
                    Name = "OverWriteMode",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "Uploaded"
                }
            };

                var iter = dropBoxUploadActivity.GetState().Select(
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
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ObjectEquals_IsFalse_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var obj = new object();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object))
            {
                obj = new DsfDropBoxUploadActivity();
                //-----------------------Act--------------------------------
                dsfDropBoxUploadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.IsFalse(dsfDropBoxUploadActivity.Equals(obj));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ObjectEquals_IsTrue_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var obj = new object();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object))
            {
                obj = dsfDropBoxUploadActivity;
                //-----------------------Act--------------------------------
                dsfDropBoxUploadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.IsTrue(dsfDropBoxUploadActivity.Equals(obj));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ObjectEquals_IsNotExpectedObject_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object))
            {
                //-----------------------Act--------------------------------
                dsfDropBoxUploadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.IsFalse(dsfDropBoxUploadActivity.Equals(new object()));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object))
            {
                //-----------------------Act--------------------------------
                dsfDropBoxUploadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                var dsfDropBoxUploadActivityOther = new DsfDropBoxUploadActivity();
                Assert.IsFalse(dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivityOther));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_IsNull_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object))
            {
                //-----------------------Act--------------------------------
                dsfDropBoxUploadActivity.SetupDropboxClient("");
                //-----------------------Assert-----------------------------
                Assert.IsFalse(dsfDropBoxUploadActivity.Equals(null));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_IsEqual_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxUploadActivity = new DsfDropBoxUploadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsTrue(dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivity));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_GetHashCode_Properties_NotNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                ToPath = "TestToPath",
                DisplayName = "TestDisplayName"
            })
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");
                //--------------------------Act--------------------------------
                var getHash = dsfDropBoxUploadActivity.GetHashCode();
                //--------------------------Assert-----------------------------
                Assert.IsNotNull(getHash);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_GetHashCode_Properties_IsNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object))
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");
                //--------------------------Act--------------------------------
                var getHash = dsfDropBoxUploadActivity.GetHashCode();
                //--------------------------Assert-----------------------------
                Assert.IsNotNull(getHash);
            }
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_PerformExecution_GetDebugInputs_Null_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object))
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");
                //--------------------------Act--------------------------------
                var list = dsfDropBoxUploadActivity.GetDebugInputs(null, 0);
                //--------------------------Assert-----------------------------
                Assert.AreEqual(0, list.Count);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_PerformExecution_GetDebugInputs_NotNull_WithSetProperties_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            using (var dsfDropBoxUploadActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object))
            {
                dsfDropBoxUploadActivity.SetupDropboxClient("");
                //--------------------------Act--------------------------------
                var list = dsfDropBoxUploadActivity.GetDebugInputs(mockExecutionEnvironment.Object, 0);
                //--------------------------Assert-----------------------------
                Assert.AreEqual(2, list.Count);
            }
        }


        class TestDsfDropBoxUploadActivity : DsfDropBoxUploadActivity
        {
            public Mock<IDropboxSingleExecutor<IDropboxResult>> MockSingleExecutor { get; internal set; }

            public TestDsfDropBoxUploadActivity(IDropboxClientFactory dropboxClientFactory)
                : base(dropboxClientFactory)
            {

            }

            public TestDsfDropBoxUploadActivity()
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

            public void TestAssignResult(IDSFDataObject dSFDataObject, int update)
            {
                base.AssignResult(dSFDataObject, update);
            }
        }
    }
}