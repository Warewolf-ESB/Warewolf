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
using Dev2.Activities.DropBox2016.DropboxFileActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_DropBoxSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.SelectedSource = new DropBoxSource
                {
                    ResourceID = Guid.NewGuid()
                };
                dsfDropboxFileListActivityOther.SelectedSource = new DropBoxSource();
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_DropBoxSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.SelectedSource = new DropBoxSource();
                dsfDropboxFileListActivityOther.SelectedSource = new DropBoxSource();
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IncludeMediaInfo_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IncludeMediaInfo = true;
                dsfDropboxFileListActivityOther.IncludeMediaInfo = false;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IncludeMediaInfo_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IncludeMediaInfo = true;
                dsfDropboxFileListActivityOther.IncludeMediaInfo = true;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsRecursive_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IsRecursive = true;
                dsfDropboxFileListActivityOther.IsRecursive = false;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsRecursive_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IsRecursive = true;
                dsfDropboxFileListActivityOther.IsRecursive = true;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IncludeDeleted_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IncludeDeleted = true;
                dsfDropboxFileListActivityOther.IncludeDeleted = false;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IncludeDeleted_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IncludeDeleted = true;
                dsfDropboxFileListActivityOther.IncludeDeleted = true;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFilesSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IsFilesSelected = true;
                dsfDropboxFileListActivityOther.IsFilesSelected = false;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFilesSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IsFilesSelected = true;
                dsfDropboxFileListActivityOther.IsFilesSelected = true;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFoldersSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IsFoldersSelected = true;
                dsfDropboxFileListActivityOther.IsFoldersSelected = false;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFoldersSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IsFoldersSelected = true;
                dsfDropboxFileListActivityOther.IsFoldersSelected = true;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFilesAndFoldersSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IsFilesAndFoldersSelected = true;
                dsfDropboxFileListActivityOther.IsFilesAndFoldersSelected = false;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsFalse(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_IsFilesAndFoldersSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                UniqueID = uniqueId,
                Result = "A"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity { UniqueID = uniqueId, Result = "A" };
                //---------------Assert Precondition----------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
                //---------------Execute Test ----------------------
                dsfDropboxFileListActivity.IsFilesAndFoldersSelected = true;
                dsfDropboxFileListActivityOther.IsFilesAndFoldersSelected = true;
                var @equals = dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther);
                //---------------Test Result -----------------------
                Assert.IsTrue(@equals);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var selectedSource = new MockOAuthSource(uniqueId);

            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //------------Setup for test--------------------------
            using (var dropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = selectedSource,
                ToPath = "Path_To",
                IsFilesSelected = false,
                IsFoldersSelected = false,
                IsFilesAndFoldersSelected = false,
                IsRecursive = false,
                Result = "List_Complete"
            })
            {
                dropboxFileListActivity.TestSetupDropboxClient("");

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
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_ObjEquals_ExpertFalse()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Assert-----------------------------
                Assert.IsFalse(dsfDropboxFileListActivity.Equals(new object()));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_ObjEquals_NotSame_ExpertFalse()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                var obj = new object();
                obj = new DsfDropboxFileListActivity();
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Assert-----------------------------
                Assert.IsFalse(dsfDropboxFileListActivity.Equals(obj));
            }
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_ObjEquals_Same_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                var obj = new object();
                obj = dsfDropboxFileListActivity;
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Assert-----------------------------
                Assert.IsTrue(dsfDropboxFileListActivity.Equals(obj));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Equals_Null_ExpertFalse()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Assert-----------------------------
                Assert.IsFalse(dsfDropboxFileListActivity.Equals(null));
            }
        }

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_Equals_IsNotSame_ExpertFalse()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Assert-----------------------------
                var dsfDropboxFileListActivityOther = new DsfDropboxFileListActivity();
                Assert.IsFalse(dsfDropboxFileListActivity.Equals(dsfDropboxFileListActivityOther));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_GetHashCode_Properties_NotNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                ToPath = "TestToPath",
                DisplayName = "TestDisplayName"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                dsfDropboxFileListActivity.Files.Add("TestFile1");
                //--------------------------Act--------------------------------
                var getHash = dsfDropboxFileListActivity.GetHashCode();
                //--------------------------Assert-----------------------------
                Assert.IsNotNull(getHash);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_GetHashCode_Properties_IsNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Act--------------------------------
                var getHash = dsfDropboxFileListActivity.GetHashCode();
                //--------------------------Assert-----------------------------
                Assert.IsNotNull(getHash);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_TestExecuteTool_ExpertNullReferenceException()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Assert-----------------------------
                Assert.ThrowsException<NullReferenceException>(() => dsfDropboxFileListActivity.TestExecuteTool(mockDSFDataObject.Object, 0));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        [Ignore("Incompatible with the latest version of Moq.")]
        public void DsfDropboxFileListActivity_TestExecuteTool_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                dsfDropboxFileListActivity.ToPath = "TestToPath";
                dsfDropboxFileListActivity.TestExecuteTool(mockDSFDataObject.Object, 0);
                //--------------------------Assert-----------------------------
                mockDSFDataObject.Verify(o => o.Environment.AddError(It.IsAny<string>()), Times.Never);
            }
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_TestExecuteTool_IncludeDeleted_IsTrue_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockListFolderResult = new Mock<IListFolderResult>();

            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));
            
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockExecutionEnvironment.Setup(o => o.Eval(It.IsAny<string>(), It.IsAny<int>())).Returns(warewolfAtomResult);
            mockDSFDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);
            
            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                var metadata = new DataNode { IsDeleted = true, IsFile = true, IsFolder = true, PathLower = "Test_PathLower" };

                var list = new List<IDataNode>();
                list.Add(metadata);
                
                mockListFolderResult.Setup(o => o.Entries).Returns(list);
                dsfDropboxFileListActivity.MockSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
                dsfDropboxFileListActivity.MockSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(mockListFolderResult.Object));
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.ToPath = "TestToPath";
                dsfDropboxFileListActivity.SelectedSource = new DropBoxSource();
                dsfDropboxFileListActivity.IncludeDeleted = true;
                dsfDropboxFileListActivity.IsFoldersSelected = false;

                dsfDropboxFileListActivity.TestExecuteTool(mockDSFDataObject.Object, 0);
                //--------------------------Assert-----------------------------
                mockDSFDataObject.Verify(o => o.Environment.AddError(It.IsAny<string>()), Times.Never);
                Assert.AreEqual(2, dsfDropboxFileListActivity.Files.Count);
                Assert.AreEqual("Test_PathLower", dsfDropboxFileListActivity.Files[0]);
            }
         }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_TestExecuteTool_IsFoldersSelected_IsTrue_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockListFolderResult = new Mock<IListFolderResult>();

            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));

            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockExecutionEnvironment.Setup(o => o.Eval(It.IsAny<string>(), It.IsAny<int>())).Returns(warewolfAtomResult);
            mockDSFDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                var metadata = new DataNode { IsDeleted = true, IsFile = true, IsFolder = true, PathLower = "Test_PathLower" };

                var list = new List<IDataNode>();
                list.Add(metadata);
                
                mockListFolderResult.Setup(o => o.Entries).Returns(list);
                dsfDropboxFileListActivity.MockSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
                dsfDropboxFileListActivity.MockSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(mockListFolderResult.Object));
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.ToPath = "TestToPath";
                dsfDropboxFileListActivity.SelectedSource = new DropBoxSource();
                dsfDropboxFileListActivity.IncludeDeleted = false;
                dsfDropboxFileListActivity.IsFoldersSelected = true;

                dsfDropboxFileListActivity.TestExecuteTool(mockDSFDataObject.Object, 0);
                //--------------------------Assert-----------------------------
                mockDSFDataObject.Verify(o => o.Environment.AddError(It.IsAny<string>()), Times.Never);
                Assert.AreEqual(2, dsfDropboxFileListActivity.Files.Count);
                Assert.AreEqual("Test_PathLower", dsfDropboxFileListActivity.Files[0]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_TestExecuteTool_IsAllTrue_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockListFolderResult = new Mock<IListFolderResult>();;

            var warewolfAtomResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("{\"PolicyNo\":\"A0003\",\"DateId\":32,\"SomeVal\":\"Bob\"}"));

            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockExecutionEnvironment.Setup(o => o.Eval(It.IsAny<string>(), It.IsAny<int>())).Returns(warewolfAtomResult);
            mockDSFDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                var metadata = new DataNode { IsDeleted = true, IsFile = true, IsFolder = true, PathLower = "Test_PathLower" };
                var list = new List<IDataNode>();

                list.Add(metadata);

                mockListFolderResult.Setup(o => o.Entries).Returns(list);
                dsfDropboxFileListActivity.MockSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
                dsfDropboxFileListActivity.MockSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(mockListFolderResult.Object));
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.ToPath = "TestToPath";
                dsfDropboxFileListActivity.SelectedSource = new DropBoxSource();
                dsfDropboxFileListActivity.IncludeDeleted = true;
                dsfDropboxFileListActivity.IsFoldersSelected = true;
                dsfDropboxFileListActivity.IsFilesAndFoldersSelected = true;

                dsfDropboxFileListActivity.TestExecuteTool(mockDSFDataObject.Object, 0);
                //--------------------------Assert-----------------------------
                mockDSFDataObject.Verify(o => o.Environment.AddError(It.IsAny<string>()), Times.Never);
                Assert.AreEqual(5, dsfDropboxFileListActivity.Files.Count);
                Assert.AreEqual("Test_PathLower", dsfDropboxFileListActivity.Files[0]);
            }
        }

        class TestMetadata : Metadata
        {
            public TestMetadata(string name, string pathLower = null, string pathDisplay = null, string parentSharedFolderId = null)
                : base(name, pathLower, pathDisplay, parentSharedFolderId)
            {
                
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_TestPerformExecution_DropboxExecutionResult_DropboxFailureResult_ExpertException()
        {
            //-----------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();

            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxFailureResult(new Exception("test error!")));

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                MockSingleExecutor = mockDropboxSingleExecutor
            })
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "ToPath", @"C:\Users\temp\testToPath\" },
                    { "FromPath", @"C:\Users\temp" }
                };
                //-----------------------Act--------------------------------
                //-----------------------Assert-----------------------------
                try
                {
                    dsfDropboxFileListActivity.TestPerformExecution(dictionary);
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
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IsFilesSelected_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata> { new Mock<Metadata>().Object }, "TestCusor", false);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionary = new Dictionary<string, string>
            {
                { "ToPath", @"C:\Users\temp\testToPath\" },
                { "FromPath", @"C:\Users\temp" }
            };

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                MockSingleExecutor = mockDropboxSingleExecutor
            })
            {
                //--------------------------Act--------------------------------
                var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionary);
                //--------------------------Assert-----------------------------
                Assert.AreEqual(1, listPerformExecution.Count);
                Assert.AreEqual("Success", listPerformExecution[0]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IsFoldersSelected_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata> { new Mock<Metadata>().Object }, "TestCusor", false);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionary = new Dictionary<string, string>
            {
                { "ToPath", @"C:\Users\temp\testToPath\" },
                { "FromPath", @"C:\Users\temp" }
            };

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                MockSingleExecutor = mockDropboxSingleExecutor,
                IsFoldersSelected = true
            })
            {
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionary);
                //--------------------------Assert-----------------------------
                Assert.AreEqual(1, listPerformExecution.Count);
                Assert.AreEqual("Success", listPerformExecution[0]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IncludeDeleted_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata> { new Mock<Metadata>().Object }, "TestCusor", false);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionery = new Dictionary<string, string>
            {
                { "ToPath", @"C:\Users\temp\testToPath\" },
                { "FromPath", @"C:\Users\temp" }
            };

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                MockSingleExecutor = mockDropboxSingleExecutor,
                IncludeDeleted = true
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");

                //--------------------------Act--------------------------------
                var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
                //--------------------------Assert-----------------------------
                Assert.AreEqual(1, listPerformExecution.Count);
                Assert.AreEqual("Success", listPerformExecution[0]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IsFilesAndFoldersSelected_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata> { new Mock<Metadata>().Object }, "TestCusor", false);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionery = new Dictionary<string, string>
            {
                { "ToPath", @"C:\Users\temp\testToPath\" },
                { "FromPath", @"C:\Users\temp" }
            };

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                MockSingleExecutor = mockDropboxSingleExecutor,
                IsFilesAndFoldersSelected = true
            })
            {
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
                //--------------------------Assert-----------------------------
                Assert.AreEqual(1, listPerformExecution.Count);
                Assert.AreEqual("Success", listPerformExecution[0]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_GetDebugInputs_Null_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Act--------------------------------
                var list = dsfDropboxFileListActivity.GetDebugInputs(null, 0);
                //--------------------------Assert-----------------------------
                Assert.AreEqual(0, list.Count);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_GetDebugInputs_NotNull_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object))
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Act--------------------------------
                var list = dsfDropboxFileListActivity.GetDebugInputs(mockExecutionEnvironment.Object, 0);
                //--------------------------Assert-----------------------------
                Assert.AreEqual(4, list.Count);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_GetDebugInputs_NotNull_WithSetProperties_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                IsFoldersSelected = true,
                IsFilesSelected = false,
                IsFilesAndFoldersSelected = true,
                IsRecursive = true,
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Act--------------------------------
                var list = dsfDropboxFileListActivity.GetDebugInputs(mockExecutionEnvironment.Object, 0);
                //--------------------------Assert-----------------------------
                Assert.AreEqual(4, list.Count);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropboxFileListActivity))]
        public void DsfDropboxFileListActivity_AssignResult_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            var mockIDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            mockIDSFDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);

            using (var dsfDropboxFileListActivity = new TestDsfDropboxFileListActivity(mockDropboxClientFactory.Object)
            {
                Files = new List<string> { "file1", "file2" },
                Result = "testResult"
            })
            {
                dsfDropboxFileListActivity.TestSetupDropboxClient("");
                //--------------------------Act--------------------------------
                dsfDropboxFileListActivity.TestAssignResult(mockIDSFDataObject.Object, 0);
                //--------------------------Assert-----------------------------
                mockIDSFDataObject.Verify(o => o.Environment, Times.Exactly(2));
            }
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