using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.DropboxFileActivity;
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
    public class DsfDropboxFileListActivityComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId, DisplayName = "a" };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId, DisplayName = "A" };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId, DisplayName = "AAA" };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfDropboxFileListActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "a" };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "AAA" };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_ToPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId, ToPath = "a" };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, ToPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_ToPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId, ToPath = "A" };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, ToPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_ToPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropboxFileListActivity { UniqueID = uniqueId, ToPath = "AAA" };
            var multiAssign1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, ToPath = "aaa" };
            //---------------Assert DsfDropboxFileListActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_DropBoxSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            var dropBoxDeleteActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
            //---------------Execute Test ----------------------
            dropBoxDeleteActivity.SelectedSource = new DropBoxSource
            {
                ResourceID = Guid.NewGuid()
            };
            dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
            var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_DropBoxSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            var dropBoxDeleteActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
            //---------------Execute Test ----------------------
            dropBoxDeleteActivity.SelectedSource = new DropBoxSource();
            dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
            var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IncludeMediaInfo_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IncludeMediaInfo = true;
            rabbitMqActivity1.IncludeMediaInfo = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IncludeMediaInfo_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IncludeMediaInfo = true;
            rabbitMqActivity1.IncludeMediaInfo = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsRecursive_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsRecursive = true;
            rabbitMqActivity1.IsRecursive = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsRecursive_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsRecursive = true;
            rabbitMqActivity1.IsRecursive = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IncludeDeleted_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IncludeDeleted = true;
            rabbitMqActivity1.IncludeDeleted = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IncludeDeleted_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IncludeDeleted = true;
            rabbitMqActivity1.IncludeDeleted = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFilesSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsFilesSelected = true;
            rabbitMqActivity1.IsFilesSelected = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFilesSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsFilesSelected = true;
            rabbitMqActivity1.IsFilesSelected = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFoldersSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsFoldersSelected = true;
            rabbitMqActivity1.IsFoldersSelected = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFoldersSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsFoldersSelected = true;
            rabbitMqActivity1.IsFoldersSelected = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFilesAndFoldersSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsFilesAndFoldersSelected = true;
            rabbitMqActivity1.IsFilesAndFoldersSelected = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFilesAndFoldersSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsFilesAndFoldersSelected = true;
            rabbitMqActivity1.IsFilesAndFoldersSelected = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var selectedSource = new MockOAuthSource(uniqueId);

            //------------Setup for test--------------------------
            var dropboxFileListActivity = new DsfDropboxFileListActivity
            {
                SelectedSource = selectedSource,
                ToPath = "Path_To",
                IsFilesSelected = false,
                IsFoldersSelected = false,
                IsFilesAndFoldersSelected = false,
                IsRecursive = false,
                Result = "List_Complete"
            };
            //------------Execute Test---------------------------
            var stateItems = dropboxFileListActivity.GetState();
            Assert.AreEqual(7, stateItems.Count());

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
                    Name = "ToPath",
                    Type = StateVariable.StateType.Input,
                    Value = "Path_To"
                },
                new StateVariable
                {
                    Name = "IsFilesSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name = "IsFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name = "IsFilesAndFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name = "IsRecursive",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "List_Complete"
                }
            };

            var iter = dropboxFileListActivity.GetState().Select(
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

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_ObjEquals_ExpertFalse()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            //--------------------------Assert-----------------------------
            Assert.IsFalse(dsfDropboxFileListActivity.Equals(new object()));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_ObjEquals_NotSame_ExpertFalse()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            var obj = new object();
            obj = new DsfDropboxFileListActivity();
            //--------------------------Act--------------------------------
            //--------------------------Assert-----------------------------
            Assert.IsFalse(dsfDropboxFileListActivity.Equals(obj));
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_ObjEquals_Same_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            var obj = new object();
            obj = dsfDropboxFileListActivity;
            //--------------------------Act--------------------------------
            //--------------------------Assert-----------------------------
            Assert.IsTrue(dsfDropboxFileListActivity.Equals(obj));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Equals_Null_ExpertFalse()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            //--------------------------Assert-----------------------------
            Assert.IsFalse(dsfDropboxFileListActivity.Equals(null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Equals_IsSame_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            //--------------------------Assert-----------------------------
            Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivity));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Equals_IsNotSame_ExpertFalse()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            //--------------------------Assert-----------------------------
            Assert.IsFalse(dsfDropboxFileListActivity.Equals(new DsfDropboxFileListActivity()));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_GetHashCode_Properties_NotNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                ToPath = "TestToPath",
                DisplayName = "TestDisplayName"
            };
            dsfDropboxFileListActivity.Files.Add("TestFile1"); 
            //--------------------------Act--------------------------------
            var getHash = dsfDropboxFileListActivity.GetHashCode();
            //--------------------------Assert-----------------------------
            Assert.IsNotNull(getHash);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_GetHashCode_Properties_IsNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            var getHash = dsfDropboxFileListActivity.GetHashCode();
            //--------------------------Assert-----------------------------
            Assert.IsNotNull(getHash);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_TestExecuteTool_ExpertNullReferenceException()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            //--------------------------Assert-----------------------------
            Assert.ThrowsException<NullReferenceException>(()=> dsfDropboxFileListActivity.TestExecuteTool(mockDSFDataObject.Object, 0));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_TestPerformExecution_DropboxExecutionResult_DropboxFailureResult_ExpertException()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", @"C:\Users\temp\testToPath\");
            dictionery.Add("FromPath", @"C:\Users\temp");
            
            var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
            };
            //--------------------------Act--------------------------------
            //--------------------------Assert-----------------------------
            Assert.ThrowsException<Exception>(()=> dsfDropboxFileListActivity.TestPerformExecution(dictionery)); 
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IsFilesSelected_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata>() { new Mock<Metadata>().Object }, "TestCusor", false);

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));
           
            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", @"C:\Users\temp\testToPath\");
            dictionery.Add("FromPath", @"C:\Users\temp");

            var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                MockSingleExecutor = mockDropboxSingleExecutor
            };
            //--------------------------Act--------------------------------
            var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(1, listPerformExecution.Count);
            Assert.AreEqual("Success", listPerformExecution[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IsFoldersSelected_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata>() { new Mock<Metadata>().Object }, "TestCusor", false);

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", @"C:\Users\temp\testToPath\");
            dictionery.Add("FromPath", @"C:\Users\temp");

            var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                MockSingleExecutor = mockDropboxSingleExecutor,
                IsFoldersSelected = true
            };
            //--------------------------Act--------------------------------
            var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(1, listPerformExecution.Count);
            Assert.AreEqual("Success", listPerformExecution[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IncludeDeleted_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata>() { new Mock<Metadata>().Object }, "TestCusor", false);

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);

            var metaList = new ListFolderResult();
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", @"C:\Users\temp\testToPath\");
            dictionery.Add("FromPath", @"C:\Users\temp");

            var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                MockSingleExecutor = mockDropboxSingleExecutor,
                IncludeDeleted = true
            };
            //--------------------------Act--------------------------------
            var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(1, listPerformExecution.Count);
            Assert.AreEqual("Success", listPerformExecution[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IsFilesAndFoldersSelected_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata>() { new Mock<Metadata>().Object }, "TestCusor", false);

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", @"C:\Users\temp\testToPath\");
            dictionery.Add("FromPath", @"C:\Users\temp");

            var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                MockSingleExecutor = mockDropboxSingleExecutor,
                IsFilesAndFoldersSelected = true
            };
            //--------------------------Act--------------------------------
            var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(1, listPerformExecution.Count);
            Assert.AreEqual("Success", listPerformExecution[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_GetDebugInputs_Null_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            var list = dsfDropboxFileListActivity.GetDebugInputs(null, 0);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_GetDebugInputs_NotNull_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            var list = dsfDropboxFileListActivity.GetDebugInputs(mockExecutionEnvironment.Object, 0);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(4,list.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_GetDebugInputs_NotNull_WithSetProperties_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var dsfDropboxFileListActivity = new DsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                IsFoldersSelected = true,
                IsFilesSelected =  false,
                IsFilesAndFoldersSelected = true,
                IsRecursive = true,
            };
            //--------------------------Act--------------------------------
            var list = dsfDropboxFileListActivity.GetDebugInputs(mockExecutionEnvironment.Object, 0);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(4, list.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_AssignResult_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockIDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            mockIDSFDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);

            var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                Files = new List<string>() { "file1", "file2" }, 
                Result = "testResult"
            };
            //--------------------------Act--------------------------------
            dsfDropboxFileListActivity.TestAssignResult(mockIDSFDataObject.Object, 0);
            //--------------------------Assert-----------------------------
            mockIDSFDataObject.Verify(o => o.Environment, Times.Exactly(2));
        }

        class TestDsfDropboxFileListActivity : DsfDropboxFileListActivity
        {
            public Mock<IDropboxSingleExecutor<IDropboxResult>> MockSingleExecutor { get; internal set; }
            
            public TestDsfDropboxFileListActivity(IDropboxClientFactory dropboxClientFactory)
                : base(dropboxClientFactory)
            {

            }

            public TestDsfDropboxFileListActivity()
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

            public void TestSetupDropboxClient(string accessToken)
            {
                SetupDropboxClient(accessToken);
            }

            public override IDropboxSingleExecutor<IDropboxResult> GetDropboxSingleExecutor(IDropboxSingleExecutor<IDropboxResult> singleExecutor)
            {
                if (MockSingleExecutor is null)
                {
                    return singleExecutor;
                }
                return MockSingleExecutor.Object;
            }

            public void TestAssignResult(IDSFDataObject dSFDataObject, int update)
            {
                base.AssignResult(dSFDataObject, update);
            }
        }
    }
}