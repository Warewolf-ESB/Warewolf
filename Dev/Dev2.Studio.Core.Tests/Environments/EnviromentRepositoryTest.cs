using System.Linq.Expressions;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Core.Tests.Utils;
using Dev2.Data.ServiceModel;
using Dev2.Providers.Events;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using ResourceType = Dev2.Studio.Core.AppResources.Enums.ResourceType;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Environments
{
    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

    /// <summary>
    /// Summary description for EnvironmentRepositoryTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EnviromentRepositoryTest
    {
        static readonly object TestLock = new object();

        #region MyClass/TestInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            SetupMef();
        }

        static void SetupMef()
        {
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(TestLock);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Monitor.Exit(TestLock);
        }

        #endregion

        #region Constructor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentRepositoryConstructorWithNullSourceExpectedThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new TestEnvironmentRespository(null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void EnvironmentRepositoryConstructorWithSourceExpectedAddsSource()
        {
            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object);
            Assert.AreEqual(1, repo.All().Count);
        }

        [TestMethod]
        public void EnvironmentRepositoryConstructorWithNoParametersExpectedCreatesAndAddsDefaultSource()
        {
            var repo = new TestEnvironmentRespository();
            var environmentModels = repo.All().ToList();
            Assert.AreEqual(1, environmentModels.Count);
            var localhostEnvironment = environmentModels[0];
            Assert.IsNotNull(localhostEnvironment);
            StringAssert.Contains(localhostEnvironment.DisplayName.ToLower(), Environment.MachineName.ToLower());


        }

        #endregion

        #region Clear

        [TestMethod]
        public void EnvironmentRepositoryClearExpectedDisconnectsAndRemovesAllItems()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();
            e1.Setup(e => e.Disconnect()).Verifiable();
            var e2 = new Mock<IEnvironmentModel>();
            e2.Setup(e => e.Disconnect()).Verifiable();

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            Assert.AreEqual(3, repo.All().Count);

            repo.Clear();

            Assert.AreEqual(0, repo.All().Count);
            source.Verify(e => e.Disconnect());
            e1.Verify(e => e.Disconnect());
            e2.Verify(e => e.Disconnect());
        }

        #endregion

        #region All

        [TestMethod]
        public void EnvironmentRepositoryAllExpectedReturnsAllItems()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();
            var e2 = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            Assert.AreEqual(3, repo.All().Count);
        }

        #endregion

        #region Find

        [TestMethod]
        public void EnvironmentRepositoryFindWithNullExpectedReturnsEmptyList()
        {
            var source = CreateMockEnvironment();
            var e1 = CreateMockEnvironment();
            var e2 = CreateMockEnvironment();

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            var actual = repo.Find(null);

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void EnvironmentRepositoryFindWithMatchingCriteriaExpectedReturnsMatchingList()
        {
            var source = CreateMockEnvironment();
            var e1 = CreateMockEnvironment();
            var e2 = CreateMockEnvironment();

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            var actual = repo.Find(e => e.ID == e1.Object.ID).ToList();

            Assert.AreEqual(1, actual.Count);
            Assert.AreSame(e1.Object, actual[0]);
        }

        [TestMethod]
        public void EnvironmentRepositoryFindWithNonMatchingCriteriaExpectedReturnsEmptyList()
        {
            var source = CreateMockEnvironment();
            var e1 = CreateMockEnvironment();
            var e2 = CreateMockEnvironment();

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            var actual = repo.Find(e => e.ID == Guid.NewGuid()).ToList();

            Assert.AreEqual(0, actual.Count);
        }

        #endregion

        #region FindSingle

        [TestMethod]
        public void EnvironmentRepositoryFindSingleWithNullExpectedReturnsNull()
        {
            var source = CreateMockEnvironment();
            var e1 = CreateMockEnvironment();
            var e2 = CreateMockEnvironment();

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            var actual = repo.FindSingle(null);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void EnvironmentRepositoryFindSingleWithMatchingCriteriaExpectedReturnsMatchingItem()
        {
            var source = CreateMockEnvironment();
            var e1 = CreateMockEnvironment();
            var e2 = CreateMockEnvironment();

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            var actual = repo.FindSingle(e => e.ID == e1.Object.ID);

            Assert.IsNotNull(actual);
            Assert.AreSame(e1.Object, actual);
        }

        [TestMethod]
        public void EnvironmentRepositoryFindSingleWithNonMatchingCriteriaExpectedReturnsNull()
        {
            var source = CreateMockEnvironment();
            var e1 = CreateMockEnvironment();
            var e2 = CreateMockEnvironment();

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);
            var actual = repo.FindSingle(e => e.ID == Guid.NewGuid());

            Assert.IsNull(actual);
        }

        #endregion

        #region Load

        [TestMethod]
        public void EnvironmentRepositoryLoadExpectedSetsIsLoadedToFalseAndInvokesLoadInternal()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();
            var e2 = new Mock<IEnvironmentModel>();

            var repo = new TestLoadEnvironmentRespository(source.Object, e1.Object, e2.Object) { IsLoaded = true };
            Assert.IsTrue(repo.IsLoaded);
            repo.ForceLoad();
            Assert.IsFalse(repo.IsLoaded);
            Assert.AreEqual(1, repo.LoadInternalHitCount);
        }

        #endregion

        #region Save

        [TestMethod]
        public void EnvironmentRepositorySaveWithManyNullExpectedDoesNothing()
        {
            var source = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);
            var startCount = repo.All().Count;

            repo.Save((ICollection<IEnvironmentModel>)null);
            Assert.AreEqual(startCount, repo.All().Count);
            Assert.AreEqual(0, repo.AddInternalHitCount);
            Assert.AreEqual(0, repo.WriteSessionHitCount);
        }

        [TestMethod]
        public void EnvironmentRepositorySaveWithManyItemsExpectedAddsItems()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();
            var e2 = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);
            var startCount = repo.All().Count;

            repo.Save(new List<IEnvironmentModel> { e1.Object, e2.Object });
            Assert.AreEqual(startCount + 2, repo.All().Count);
            Assert.AreEqual(2, repo.AddInternalHitCount);
        }

        [TestMethod]
        public void EnvironmentRepositorySaveWithManyItemsExpectedDoesNotInvokesWriteSession()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();
            var e2 = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);

            repo.Save(new List<IEnvironmentModel> { e1.Object, e2.Object });
            Assert.AreEqual(0, repo.WriteSessionHitCount);
        }

        [TestMethod]
        public void EnvironmentRepositorySaveWithSingleNullExpectedDoesNothing()
        {
            var source = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);
            var startCount = repo.All().Count;

            repo.Save((ICollection<IEnvironmentModel>)null);
            Assert.AreEqual(startCount, repo.All().Count);
            Assert.AreEqual(0, repo.AddInternalHitCount);
            Assert.AreEqual(0, repo.WriteSessionHitCount);
        }

        [TestMethod]
        public void EnvironmentRepositorySaveWithSingleItemExpectedAddsItem()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);
            var startCount = repo.All().Count;

            repo.Save(e1.Object);
            Assert.AreEqual(startCount + 1, repo.All().Count);
            Assert.AreEqual(1, repo.AddInternalHitCount);
        }

        [TestMethod]
        public void EnvironmentRepository_Save_ValidEnvironmentModel_ReturnsASaveMessage()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);

            var result = repo.Save(e1.Object);

            Assert.AreEqual(result, "Saved");
        }


        [TestMethod]
        public void EnvironmentRepository_Save_ValidEnvironmentModel_ReturnsNotSaveMessage()
        {
            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object);
            IEnvironmentModel e1 = null;
            // ReSharper disable ExpressionIsAlwaysNull
            var result = repo.Save(e1);
            // ReSharper restore ExpressionIsAlwaysNull
            Assert.AreEqual(result, "Not Saved");
        }

        [TestMethod]
        public void EnvironmentRepositorySaveWithSingleItemExpectedDoesNotInvokesWriteSession()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);

            repo.Save(e1.Object);
            Assert.AreEqual(0, repo.WriteSessionHitCount);
        }

        [TestMethod]
        public void EnvironmentRepositorySaveWithSingleExistingItemExpectedReplacesItem()
        {
            // DO NOT use mock as test requires IEquatable of IEnvironmentModel
            var c1 = CreateMockConnection();
            //var wizard = new Mock<IWizardEngine>();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object);

            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object, e1);
            var startCount = repo.All().Count;

            repo.Save(e1);

            Assert.AreEqual(startCount, repo.All().Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentRepository_Save")]
        public void EnvironmentRepository_Save_ExistingEnvironment_RaisesItemEditedEvent()
        {
            //------------Setup for test--------------------------
            var c1 = CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object);
            var source = new Mock<IEnvironmentModel>();
            IEnvironmentModel _editedEnvironment = null;
            var repo = new TestEnvironmentRespository(source.Object, e1);
            repo.ItemEdited += (sender, args) =>
            {
                _editedEnvironment = args.Environment;
            };
            e1.Name = "New Name";
            //------------Execute Test---------------------------
            repo.Save(e1);
            //------------Assert Results-------------------------
            Assert.IsNotNull(_editedEnvironment);
            Assert.AreEqual("New Name", _editedEnvironment.Name);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentRepository_Save")]
        public void EnvironmentRepository_Save_NotExistingEnvironment_DoesNotRaisesItemEditedEvent()
        {
            //------------Setup for test--------------------------
            var c1 = CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object);
            var source = new Mock<IEnvironmentModel>();
            IEnvironmentModel _editedEnvironment = null;
            var repo = new TestEnvironmentRespository(source.Object);
            repo.ItemEdited += (sender, args) =>
            {
                _editedEnvironment = args.Environment;
            };
            e1.Name = "New Name";
            //------------Execute Test---------------------------
            repo.Save(e1);
            //------------Assert Results-------------------------
            Assert.IsNull(_editedEnvironment);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentRepository_Save")]
        public void EnvironmentRepository_Save_RaisesItemAddedEvent()
        {
            //------------Setup for test--------------------------
            var c1 = CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object);
            var source = new Mock<IEnvironmentModel>();
            bool _eventFired = false;
            var repo = new TestEnvironmentRespository(source.Object);
            repo.ItemAdded += (sender, args) =>
            {
                _eventFired = true;
            };
            e1.Name = "New Name";
            //------------Execute Test---------------------------
            repo.Save(e1);
            //------------Assert Results-------------------------
            Assert.IsTrue(_eventFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentRepository_Fetch")]
        public void EnvironmentRepository_Fetch_ReturnsFoundEnvironment()
        {
            //------------Setup for test--------------------------
            var c1 = CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object);
            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object);
            repo.Save(e1);
            //------------Execute Test---------------------------
            var environmentModel = repo.Fetch(source.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(environmentModel);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentRepository_Fetch")]
        public void EnvironmentRepository_Fetch_Null_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var c1 = CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object);
            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object);
            repo.Save(e1);
            //------------Execute Test---------------------------
            var environmentModel = repo.Fetch(null);
            //------------Assert Results-------------------------
            Assert.IsNull(environmentModel);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentRepository_Fetch")]
        public void EnvironmentRepository_Fetch_NotFound_ReturnsEnvironment()
        {
            //------------Setup for test--------------------------
            var c1 = CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object);
            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object);
            repo.Save(e1);
            //------------Execute Test---------------------------
            var environmentModel = repo.Fetch(new Mock<IEnvironmentModel>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(environmentModel);
        }


        [TestMethod]
        public void EnvironmentRepositorySaveWithSingleExpectedDoesNotConnect()
        {
            // DO NOT use mock as test requires IEquatable of IEnvironmentModel
            var c1 = CreateMockConnection();
            c1.Setup(c => c.Connect(It.IsAny<Guid>())).Verifiable();

            //var wizard = new Mock<IWizardEngine>();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object);

            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object);

            repo.Save(e1);

            c1.Verify(c => c.Connect(It.IsAny<Guid>()), Times.Never());
        }

        #endregion

        #region Remove

        [TestMethod]
        public void EnvironmentRepositoryRemoveWithManyNullExpectedDoesNothing()
        {
            var source = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);
            var startCount = repo.All().Count;

            repo.Remove((ICollection<IEnvironmentModel>)null);
            Assert.AreEqual(startCount, repo.All().Count);
            Assert.AreEqual(0, repo.RemoveInternalHitCount);
        }

        [TestMethod]
        public void EnvironmentRepositoryRemoveWithManyItemsExpectedDisconnectsAndRemovesItems()
        {
            // DO NOT use mock as test requires IEquatable of IEnvironmentModel

            var source = new Mock<IEnvironmentModel>();
            var c1 = CreateMockConnection();
            var c2 = CreateMockConnection();
            var c3 = CreateMockConnection();

            c1.Setup(c => c.Disconnect()).Verifiable();
            c2.Setup(c => c.Disconnect()).Verifiable();
            c3.Setup(c => c.Disconnect()).Verifiable();

            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);
            var e2 = new EnvironmentModel(Guid.NewGuid(), c2.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);
            var e3 = new EnvironmentModel(Guid.NewGuid(), c3.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);

            var repo = new TestEnvironmentRespository(source.Object, e1, e2, e3);

            repo.Remove(new List<IEnvironmentModel> { e1, e3 });
            var actual = repo.All().ToList();

            c1.Verify(c => c.Disconnect(), Times.Once());
            c2.Verify(c => c.Disconnect(), Times.Never());
            c3.Verify(c => c.Disconnect(), Times.Once());

            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(2, repo.RemoveInternalHitCount);
            Assert.AreSame(source.Object, actual[0]);
            Assert.AreSame(e2, actual[1]);
        }

        [TestMethod]
        public void EnvironmentRepositoryRemoveWithManyItemsExpectedDoesNotInvokesWriteSession()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();
            var e2 = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);

            repo.Remove(new List<IEnvironmentModel> { e1.Object, e2.Object });
            Assert.AreEqual(0, repo.WriteSessionHitCount);
        }

        [TestMethod]
        public void EnvironmentRepositoryRemoveWithSingleNullExpectedDoesNothing()
        {
            var source = new Mock<IEnvironmentModel>();

            var repo = new TestEnvironmentRespository(source.Object);
            var startCount = repo.All().Count;

            repo.Remove((ICollection<IEnvironmentModel>)null);
            Assert.AreEqual(startCount, repo.All().Count);
            Assert.AreEqual(0, repo.RemoveInternalHitCount);
            Assert.AreEqual(0, repo.WriteSessionHitCount);
        }

        [TestMethod]
        public void EnvironmentRepositoryRemoveWithSingleItemExpectedDisconnectsAndRemovesItem()
        {
            var source = new Mock<IEnvironmentModel>();
            var c1 = CreateMockConnection();
            var c2 = CreateMockConnection();
            var c3 = CreateMockConnection();

            c1.Setup(c => c.Disconnect()).Verifiable();
            c2.Setup(c => c.Disconnect()).Verifiable();
            c3.Setup(c => c.Disconnect()).Verifiable();

            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);
            var e2 = new EnvironmentModel(Guid.NewGuid(), c2.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);
            var e3 = new EnvironmentModel(Guid.NewGuid(), c3.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);

            var repo = new TestEnvironmentRespository(source.Object, e1, e2, e3);

            repo.Remove(e2);
            var actual = repo.All().ToList();

            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Once());
            c3.Verify(c => c.Disconnect(), Times.Never());

            Assert.AreEqual(3, actual.Count);
            Assert.AreEqual(1, repo.RemoveInternalHitCount);
            Assert.AreSame(source.Object, actual[0]);
            Assert.AreSame(e1, actual[1]);
            Assert.AreSame(e3, actual[2]);
        }

        [TestMethod]
        public void EnvironmentRepositoryRemoveWithSingleItemExpectedDoesNotInvokesWriteSession()
        {
            var source = new Mock<IEnvironmentModel>();
            var c1 = CreateMockConnection();
            var c2 = CreateMockConnection();
            var c3 = CreateMockConnection();

            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);
            var e2 = new EnvironmentModel(Guid.NewGuid(), c2.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);
            var e3 = new EnvironmentModel(Guid.NewGuid(), c3.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);

            var repo = new TestEnvironmentRespository(source.Object, e1, e2, e3);

            repo.Remove(e1);
            Assert.AreEqual(0, repo.WriteSessionHitCount);
        }

        [TestMethod]
        public void EnvironmentRepositoryRemoveWithSingleNonExistingItemExpectedDoesNotRemoveItem()
        {
            var source = new Mock<IEnvironmentModel>();
            var c1 = CreateMockConnection();
            var c2 = CreateMockConnection();
            var c3 = CreateMockConnection();

            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);
            var e2 = new EnvironmentModel(Guid.NewGuid(), c2.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);
            var e3 = new EnvironmentModel(Guid.NewGuid(), c3.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);

            var repo = new TestEnvironmentRespository(source.Object, e1, e2);
            var startCount = repo.All().Count;

            repo.Remove(e3);

            Assert.AreEqual(startCount, repo.All().Count);
            Assert.AreEqual(1, repo.RemoveInternalHitCount);
            Assert.AreEqual(0, repo.WriteSessionHitCount);
        }

        #endregion

        #region Persistence

        [TestMethod]
        public void EnvironmentRepositoryPersistenceExpectedUsesCurrentUsersAppDataFolder()
        {
            var expected = Path.Combine(new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                StringResources.App_Data_Directory,
                StringResources.Environments_Directory
            });

            var actual = EnvironmentRepository.GetEnvironmentsDirectory();

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region ReadSession

        [TestMethod]
        public void EnvironmentRepositoryReadSessionWithNonExistingFileExpectedReturnsEmptyList()
        {
            var path = EnvironmentRepository.GetEnvironmentsFilePath();
            var bakPath = RetryUtility.RetryMethod(() => BackupFile(path), 15, 1000, null);

            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object) { IsReadWriteEnabled = true };
            var result = repo.ReadSession();

            Assert.AreEqual(0, result.Count);

            RetryUtility.RetryAction(() => RestoreFile(path, bakPath), 15, 1000);
        }

        [TestMethod]
        public void EnvironmentRepositoryReadSessionWithOneEnvironmentExpectedReturnsOneEnvironment()
        {
            var path = EnvironmentRepository.GetEnvironmentsFilePath();
            var bakPath = RetryUtility.RetryMethod(() => BackupFile(path), 15, 1000, null);

            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object) { IsReadWriteEnabled = true };
            repo.WriteSession(new List<Guid> { Guid.NewGuid() });
            var result = repo.ReadSession();

            Assert.AreEqual(1, result.Count);

            // ReSharper disable ImplicitlyCapturedClosure
            RetryUtility.RetryAction(() => DeleteFile(path), 15, 1000);
            // ReSharper restore ImplicitlyCapturedClosure
            RetryUtility.RetryAction(() => RestoreFile(path, bakPath), 15, 1000);
        }

        #endregion

        #region WriteSession

        [TestMethod]
        public void EnvironmentRepositoryWriteSessionWithNonExistingFileExpectedCreatesFile()
        {
            var path = EnvironmentRepository.GetEnvironmentsFilePath();
            var bakPath = RetryUtility.RetryMethod(() => BackupFile(path), 15, 1000, null);

            var source = new Mock<IEnvironmentModel>();
            var repo = new TestEnvironmentRespository(source.Object) { IsReadWriteEnabled = true };
            repo.WriteSession(null);

            var exists = File.Exists(path);
            Assert.AreEqual(true, exists);

            // ReSharper disable ImplicitlyCapturedClosure
            RetryUtility.RetryAction(() => DeleteFile(path), 15, 1000);
            // ReSharper restore ImplicitlyCapturedClosure
            RetryUtility.RetryAction(() => RestoreFile(path, bakPath), 15, 1000);
        }

        [TestMethod]
        public void EnvironmentRepositoryWriteSessionWithExistingFileExpectedOverwritesFile()
        {
            var path = EnvironmentRepository.GetEnvironmentsFilePath();
            var bakPath = RetryUtility.RetryMethod(() => BackupFile(path), 15, 1000, null);

            var c1 = CreateMockConnection();
            var e1 = new EnvironmentModel(Guid.NewGuid(), c1.Object, new Mock<IResourceRepository>().Object, new Mock<IStudioResourceRepository>().Object);
            var repo = new TestEnvironmentRespository(e1) { IsReadWriteEnabled = true };

            // Create file
            repo.WriteSession(new List<Guid> { Guid.NewGuid() });

            var xml = XElement.Load(path);
            var actual = xml.Descendants("Environment").Count();
            Assert.AreEqual(1, actual);

            // Overwrite file            
            repo.WriteSession(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            xml = XElement.Load(path);
            actual = xml.Descendants("Environment").Count();
            Assert.AreEqual(2, actual);

            // ReSharper disable ImplicitlyCapturedClosure
            RetryUtility.RetryAction(() => DeleteFile(path), 15, 1000);
            // ReSharper restore ImplicitlyCapturedClosure
            RetryUtility.RetryAction(() => RestoreFile(path, bakPath), 15, 1000);
        }

        #endregion

        #region LookupEnvironments

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentRepositoryLookupEnvironmentsWithNullParametersExpectedThrowsArgumentNullException()
        {
            EnvironmentRepository.LookupEnvironments(null);
        }

        [TestMethod]
        public void EnvironmentRepositoryLookupEnvironmentsWithNoEnvironmentIDsExpectedReturnsListOfServers()
        {
            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var repo = new Mock<IResourceRepository>();

            var id = Guid.NewGuid();

            Connection theCon = new Connection
            {
                Address = "http://127.0.0.1:1234",
                ResourceName = "TheConnection",
                ResourceID = id,
                WebServerPort = 1234
            };

            List<Connection> cons = new List<Connection> { theCon };
            repo.Setup(r => r.FindSourcesByType<Connection>(It.IsAny<IEnvironmentModel>(), enSourceType.Dev2Server)).Returns(cons);

            con.Setup(c => c.IsConnected).Returns(true);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var result = EnvironmentRepository.LookupEnvironments(env.Object);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(id, result[0].ID, "EnvironmentRepository did not assign the resource ID to the environment ID.");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentRepository_Save")]
        public void EnvironmentRepository_All_ReturnsListOfServers()
        {
            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var repo = new Mock<IResourceRepository>();

            var id = Guid.NewGuid();

            Connection theCon = new Connection
            {
                Address = "http://127.0.0.1:1234",
                ResourceName = "TheConnection",
                ResourceID = id,
                AuthenticationType = AuthenticationType.User,
                UserName = "Hagashen",
                Password = "password",
                WebServerPort = 1234
            };

            List<Connection> cons = new List<Connection> { theCon };
            repo.Setup(r => r.FindSourcesByType<Connection>(It.IsAny<IEnvironmentModel>(), enSourceType.Dev2Server)).Returns(cons);

            con.Setup(c => c.IsConnected).Returns(true);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            EnvironmentRepository.Instance.Save(env.Object);
            //-----------------Execute Process---------------------
            var result = EnvironmentRepository.Instance.All().ToList();
            //-----------------Assert------------------------------
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void EnvironmentRepositoryLookupEnvironmentsWithAuthenticationTypeExpectedReturnsListOfServers()
        {
            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var repo = new Mock<IResourceRepository>();

            var id = Guid.NewGuid();

            Connection theCon = new Connection
            {
                Address = "http://127.0.0.1:1234",
                ResourceName = "TheConnection",
                ResourceID = id,
                AuthenticationType = AuthenticationType.User,
                UserName = "Hagashen",
                Password = "password",
                WebServerPort = 1234
            };

            List<Connection> cons = new List<Connection> { theCon };
            repo.Setup(r => r.FindSourcesByType<Connection>(It.IsAny<IEnvironmentModel>(), enSourceType.Dev2Server)).Returns(cons);

            con.Setup(c => c.IsConnected).Returns(true);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var result = EnvironmentRepository.LookupEnvironments(env.Object);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(id, result[0].ID, "EnvironmentRepository did not assign the resource ID to the environment ID.");
            Assert.AreEqual(AuthenticationType.User, result[0].Connection.AuthenticationType);
            Assert.AreEqual("Hagashen", result[0].Connection.UserName);
            Assert.AreEqual("password", result[0].Connection.Password);
        }

        [TestMethod]
        public void EnvironmentRepositoryLookupEnvironmentsWithInvalidParametersExpectedReturnsEmptyList()
        {
            var env = CreateMockEnvironment();
            var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { "xxx", "aaa" });
            // Test
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void EnvironmentRepositoryLookupEnvironmentsWithInvalidEnvironmentIDExpectedReturnsEmptyList()
        {
            var env = CreateMockEnvironment(
                "<Source ID=\"{5E8EB586-1D63-4C9F-9A35-CD05ACC2B6}\" ConnectionString=\"AppServerUri=//127.0.0.1:77/dsf;WebServerPort=1234\" Name=\"TheName\" Type=\"Dev2Server\"><DisplayName>The Name</DisplayName></Source>");

            var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { Server1ID, "{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}" });
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void EnvironmentRepositoryLookupEnvironmentsWithInvalidEnvironmentAppServerUriExpectedReturnsEmptyList()
        {

            var env = CreateMockEnvironment(
                "<Source ID=\"{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}\" ConnectionString=\"AppServerUri=//127.0.0.1:77/dsf;WebServerPort=1234\" Name=\"TheName\" Type=\"Dev2Server\"><DisplayName>The Name</DisplayName></Source>");

            var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { Server1ID, "{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}" });
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void EnvironmentRepositoryLookupEnvironmentsWithInvalidEnvironmentWebServerPortExpectedReturnsEmptyList()
        {
            var env = CreateMockEnvironment(
                "<Source ID=\"{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}\" ConnectionString=\"AppServerUri=http://127.0.0.1:77/dsf;WebServerPort=12a34\" Name=\"TheName\" Type=\"Dev2Server\"><DisplayName>The Name</DisplayName></Source>");

            var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { Server1ID, "{5E8EB586-1D63-4C9F-9A35-CD05ACC2B607}" });
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void EnvironmentRepositoryLookupEnvironmentsWithOneValidEnvironmentIDExpectedReturnsOneEnvironment()
        {

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var repo = new Mock<IResourceRepository>();

            ResourceModel rm = new ResourceModel(env.Object)
            {
                WorkflowXaml = new StringBuilder(@"<Source ID=""7e9eead4-d876-4bc1-a71d-66c76255795f"" Name=""bld"" Type=""Dev2Server"" ConnectionString=""AppServerUri=http://TST-CI-REMOTE:3142/dsf;WebServerPort=3142"" Version=""1.0"" ResourceType=""Server"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
	<TypeOf>Dev2Server</TypeOf>
	<DisplayName>bld</DisplayName>
	<Category/>
	<AuthorRoles />
	<Comment />
	<Tags />
	<UnitTestTargetWorkflowService />
	<HelpLink />
	<DataList />
</Source>")
            };

            List<IResourceModel> models = new List<IResourceModel> { rm };

            repo.Setup(r => r.FindResourcesByID(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnumerable<string>>(), ResourceType.Source)).Returns(models);

            con.Setup(c => c.IsConnected).Returns(true);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { Server1ID });
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("http://tst-ci-remote:3142/", result[0].Connection.WebServerUri.AbsoluteUri);
        }

        [TestMethod]
        public void EnvironmentRepositoryLookupEnvironmentsWithOneValidEnvironmentAuthenticationTypeExpectedReturnsOneEnvironment()
        {

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var repo = new Mock<IResourceRepository>();

            ResourceModel rm = new ResourceModel(env.Object)
            {
                WorkflowXaml = new StringBuilder(@"<Source ID=""7e9eead4-d876-4bc1-a71d-66c76255795f"" Name=""bld"" Type=""Dev2Server"" ConnectionString=""AppServerUri=http://TST-CI-REMOTE:3142/dsf;WebServerPort=3142;AuthenticationType=User;UserName=dev2\hagashen.naidu;Password=hahaha"" Version=""1.0"" ResourceType=""Server"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
	<TypeOf>Dev2Server</TypeOf>
	<DisplayName>bld</DisplayName>
	<Category/>
	<AuthorRoles />
	<Comment />
	<Tags />
	<UnitTestTargetWorkflowService />
	<HelpLink />
	<DataList />
</Source>")
            };

            List<IResourceModel> models = new List<IResourceModel> { rm };

            repo.Setup(r => r.FindResourcesByID(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnumerable<string>>(), ResourceType.Source)).Returns(models);

            con.Setup(c => c.IsConnected).Returns(true);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var result = EnvironmentRepository.LookupEnvironments(env.Object, new List<string> { Server1ID });
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("http://tst-ci-remote:3142/", result[0].Connection.WebServerUri.AbsoluteUri);
            Assert.AreEqual(AuthenticationType.User, result[0].Connection.AuthenticationType);
            Assert.AreEqual("dev2\\hagashen.naidu", result[0].Connection.UserName);
            Assert.AreEqual("hahaha", result[0].Connection.Password);
        }

        [TestMethod]
        public void EnvironmentRepository_UnitTest_LookupEnvironmentsWithDefaultEnvironmentExpectDoesNotThrowException()
        {

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var repo = new Mock<IResourceRepository>();

            var id = Guid.NewGuid();

            Connection theCon = new Connection
            {
                Address = "http://127.0.0.1:1234",
                ResourceName = "TheConnection",
                ResourceID = id,
                WebServerPort = 1234
            };

            List<Connection> cons = new List<Connection> { theCon };
            repo.Setup(r => r.FindSourcesByType<Connection>(It.IsAny<IEnvironmentModel>(), enSourceType.Dev2Server)).Returns(cons);

            con.Setup(c => c.IsConnected).Returns(true);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);
            var studiorepo = new Mock<IStudioResourceRepository>();
            //------------Setup for test--------------------------
            var defaultEnvironment = new EnvironmentModel(Guid.NewGuid(), CreateMockConnection(new[] { "localhost" }).Object, repo.Object, studiorepo.Object);
            //------------Execute Test---------------------------
            EnvironmentRepository.LookupEnvironments(defaultEnvironment);
            //------------Assert Results-------------------------
            Assert.IsTrue(true);
        }

        [TestMethod]
        [TestCategory("EnvironmentRepository_LookupEnvironments")]
        [Description("EnvironmentRepository must initialize EnvironmentModels with their catalog resource ID.")]
        [Owner("Trevor Williams-Ros")]
        public void EnvironmentRepository_UnitTest_EnvironmentModelID_ResourceID()
        {

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var repo = new Mock<IResourceRepository>();

            var id = Guid.NewGuid();

            Connection theCon = new Connection
                {
                    Address = "http://127.0.0.1:1234",
                    ResourceName = "TheConnection",
                    ResourceID = id,
                    WebServerPort = 1234
                };

            List<Connection> cons = new List<Connection> { theCon };
            repo.Setup(r => r.FindSourcesByType<Connection>(It.IsAny<IEnvironmentModel>(), enSourceType.Dev2Server)).Returns(cons);

            con.Setup(c => c.IsConnected).Returns(true);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);
            env.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var result = EnvironmentRepository.LookupEnvironments(env.Object);
            Assert.AreEqual(1, result.Count, "EnvironmentRepository failed to load environment.");
            Assert.AreEqual(id, result[0].ID, "EnvironmentRepository did not assign the resource ID to the environment ID.");
        }

        #endregion

        //
        // Static Helpers
        //

        #region Backup/Restore File

        static string BackupFile(string path)
        {
            // ReSharper disable AssignNullToNotNullAttribute
            var bakPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + ".bak");
            // ReSharper restore AssignNullToNotNullAttribute
            if(File.Exists(bakPath))
            {
                File.Delete(bakPath);
            }
            if(File.Exists(path))
            {
                File.Move(path, bakPath);
            }
            return bakPath;
        }

        static void RestoreFile(string path, string bakPath)
        {
            if(File.Exists(path))
            {
                File.Delete(path);
            }
            if(File.Exists(bakPath))
            {
                File.Move(bakPath, path);
            }
        }

        static void DeleteFile(string path)
        {
            File.Delete(path);
        }

        #endregion

        [TestMethod]
        public void ParseConnectionStringIntoAppServerUri()
        {
            var toParse = TestResourceStringsTest.ResourceToHydrateConnectionString1;
            var result = EnvironmentRepository.GetAppServerUriFromConnectionString(toParse);
            Assert.AreEqual(TestResourceStringsTest.ResourceToHydrateActualAppUri, result);
        }

        #region CreateMockEnvironment

        public static readonly string Server1Source = "<Source ID=\"{70238921-FDC7-4F7A-9651-3104EEDA1211}\" Name=\"MyDevServer\" Type=\"Dev2Server\" ConnectionString=\"AppServerUri=http://127.0.0.1:77/dsf;WebServerPort=1234\" ServerID=\"d53bbcc5-4794-4dfa-b096-3aa815692e66\"><TypeOf>Dev2Server</TypeOf><DisplayName>My Dev Server</DisplayName></Source>";
        public static readonly string Server1ID = "{70238921-FDC7-4F7A-9651-3104EEDA1211}";
        public static readonly Guid Server2ID = Guid.Parse("{70238921-FDC7-4F7A-9651-3104EEDA1211}");

        public static Mock<IEnvironmentModel> CreateMockEnvironment(bool overrideExecuteCommand, params string[] sources)
        {

            var env = new Mock<IEnvironmentModel>();
            var con = new Mock<IEnvironmentConnection>();
            var repo = new Mock<IResourceRepository>();

            ResourceModel rm = new ResourceModel(env.Object);

            if(sources != null && sources.Length > 0)
            {
                rm.WorkflowXaml = new StringBuilder(sources[0]);

                List<IResourceModel> models = new List<IResourceModel> { rm };

                repo.Setup(
                    r =>
                    r.FindResourcesByID(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnumerable<string>>(),
                                        ResourceType.Source)).Returns(models);

                repo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), It.IsAny<bool>())).Returns(new Mock<IResourceModel>().Object);
                con.Setup(c => c.IsConnected).Returns(true);
                if(overrideExecuteCommand)
                {
                    con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                       .Returns(new StringBuilder(sources[0]));
                }

                con.Setup(c => c.ServerEvents).Returns(new EventPublisher());

                env.Setup(e => e.IsConnected).Returns(true);
                env.Setup(e => e.Connection).Returns(con.Object);
                env.Setup(e => e.ResourceRepository).Returns(repo.Object);
            }
            else
            {
                return CreateMockEnviromentModel();
            }

            return env;

        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment(params string[] sources)
        {

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.IsAuthorized).Returns(true);
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(true);
            env.Setup(model => model.AuthorizationService).Returns(mockAuthorizationService.Object);
            var con = new Mock<IEnvironmentConnection>();
            var repo = new Mock<IResourceRepository>();

            ResourceModel rm = new ResourceModel(env.Object);

            if(sources != null && sources.Length > 0)
            {
                rm.WorkflowXaml = new StringBuilder(sources[0]);

                List<IResourceModel> models = new List<IResourceModel> { rm };

                repo.Setup(
                    r =>
                    r.FindResourcesByID(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnumerable<string>>(),
                                        ResourceType.Source)).Returns(models);

                repo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), It.IsAny<bool>())).Returns(new Mock<IResourceModel>().Object);
                con.Setup(c => c.IsConnected).Returns(true);
                con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());
                con.Setup(c => c.ServerEvents).Returns(new EventPublisher());

                env.Setup(e => e.IsConnected).Returns(true);
                env.Setup(e => e.Connection).Returns(con.Object);
                env.Setup(e => e.ResourceRepository).Returns(repo.Object);
            }
            else
            {
                return CreateMockEnviromentModel();
            }

            return env;

        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment(IResourceRepository resourceRepository, params string[] sources)
        {

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.IsAuthorized).Returns(true);
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.DeployFrom, null)).Returns(true);
            mockAuthorizationService.Setup(service => service.IsAuthorized(AuthorizationContext.DeployTo, null)).Returns(true);
            env.Setup(model => model.AuthorizationService).Returns(mockAuthorizationService.Object);
            var con = new Mock<IEnvironmentConnection>();


            if(sources != null && sources.Length > 0)
            {
                con.Setup(c => c.IsConnected).Returns(true);
                con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());
                con.Setup(c => c.ServerEvents).Returns(new EventPublisher());

                env.Setup(e => e.IsConnected).Returns(true);
                env.Setup(e => e.Connection).Returns(con.Object);
                env.Setup(e => e.ResourceRepository).Returns(resourceRepository);
            }
            else
            {
                return CreateMockEnviromentModel();
            }

            return env;

        }

        public static Mock<IEnvironmentModel> CreateMockEnviromentModel()
        {

            var rand = new Random();
            var connection = CreateMockConnection(rand, null);

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.IsAuthorized).Returns(true);
            env.Setup(model => model.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
            env.Setup(e => e.Connection).Returns(connection.Object);

            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ID).Returns(Guid.NewGuid());

            env.Setup(e => e.Name).Returns(string.Format("Server_{0}", rand.Next(1, 100)));

            List<IResourceModel> models = new List<IResourceModel>();

            var repo = new Mock<IResourceRepository>();

            repo.Setup(r => r.FindResourcesByID(It.IsAny<IEnvironmentModel>(), It.IsAny<IEnumerable<string>>(), ResourceType.Source)).Returns(models);
            repo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), It.IsAny<bool>())).Returns(new Mock<IResourceModel>().Object);
            env.Setup(r => r.ResourceRepository).Returns(repo.Object);

            return env;
        }

        #endregion

        #region CreateMockConnection

        public static Mock<IEnvironmentConnection> CreateMockConnection(params string[] sources)
        {
            return CreateMockConnection(new Random(), sources);
        }

        public static Mock<IEnvironmentConnection> CreateMockConnection(Random rand, params string[] sources)
        {

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerID).Returns(Guid.NewGuid());
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            connection.SetupGet(environmentConnection => environmentConnection.AsyncWorker).Returns(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            connection.SetupProperty(c => c.DisplayName);
            if(sources != null && sources.Length > 0)
            {
                connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                          .Returns(new StringBuilder(sources[0]));
            }
            else
            {
                connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                          .Returns(new StringBuilder());
            }

            return connection;
        }

        #endregion
    }
}
