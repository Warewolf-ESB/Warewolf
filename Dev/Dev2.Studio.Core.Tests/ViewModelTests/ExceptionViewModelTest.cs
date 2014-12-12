
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Caliburn.Micro;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.ViewModels.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class ExceptionViewModelTest
    {
        #region Class Members
        private static string _tempTestFolder;

        #endregion Class Members

        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion Properties

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            _tempTestFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempTestFolder);
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
            DeleteTempTestFolder();
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
        }

        #endregion
        // ReSharper disable InconsistentNaming

        [TestMethod]
        public void Send_Where_OutputPathDoesntExist_Expected_PathCreatedAndActionIvoked()
        {
            var tmpOutputPath = new FileInfo(GetUniqueOutputPath(".txt"));
            if(tmpOutputPath.Directory != null)
            {
                string newOutputFolder = Path.Combine(tmpOutputPath.Directory.FullName, Guid.NewGuid().ToString());
                string newOutputpath = Path.Combine(newOutputFolder, tmpOutputPath.Name);

                var vm = new ExceptionViewModel { OutputPath = newOutputpath, OutputText = ExceptionFactory.Create(GetException()).ToString() };

                var mockEmailAction = new Mock<IFeedbackAction>();
                mockEmailAction.Setup(c => c.StartFeedback()).Verifiable();

                var mockInvoker = new Mock<IFeedbackInvoker>();
                mockInvoker.Setup(i => i.InvokeFeedback(It.IsAny<IFeedbackAction>())).Verifiable();

                vm.FeedbackAction = mockEmailAction.Object;
                vm.FeedbackInvoker = mockInvoker.Object;

                vm.SendReport();

                mockInvoker.Verify(a => a.InvokeFeedback(It.IsAny<IFeedbackAction>()), Times.Once());
                Assert.AreEqual(Directory.Exists(newOutputFolder), true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Send_Where_OutputPathIsntZipOrXml_Expected_InvalidOperationException()
        {
            string outputPath = GetUniqueOutputPath(".cake");
            var vm = new ExceptionViewModel { OutputPath = outputPath };
            vm.SendReport();
        }

        [TestMethod]
        public void ShowExceptionDialog_Expected_WindowManagerInvokedForViewModel()
        {
            var vm = new ExceptionViewModel();

            Mock<IWindowManager> mockWinManager = new Mock<IWindowManager>();
            mockWinManager.Setup(c => c.ShowDialog(It.IsAny<BaseViewModel>(), null, null)).Verifiable();

            vm.WindowNavigation = mockWinManager.Object;
            vm.Show();

            mockWinManager.Verify(mgr => mgr.ShowDialog(It.IsAny<ExceptionViewModel>(), null, null), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void Send_Where_OutputPathAlreadyExists_Expected_FileIOException()
        {
            //Create file which is to conflict with the output path of the recorder
            FileInfo conflictingPath = new FileInfo(GetUniqueOutputPath(".txt"));
            conflictingPath.Create().Close();

            var vm = new ExceptionViewModel { OutputPath = conflictingPath.FullName };

            vm.SendReport();
        }

        private static Exception GetException()
        {
            return new Exception("Test Exception", new Exception("Test inner Exception"));
        }

        private static void DeleteTempTestFolder()
        {
            try
            {
                Directory.Delete(_tempTestFolder, true);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //Fail silently if folder couldn't be deleted.
            }
        }

        private static string GetUniqueOutputPath(string extension)
        {
            return Path.Combine(_tempTestFolder, Guid.NewGuid() + extension);
        }
    }
}
