/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    public class ExceptionViewModelTest
    {
        #region Class Members

        private static string _tempTestFolder;

        #endregion Class Members

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

        #endregion Additional test attributes

        // ReSharper disable InconsistentNaming

        [TestMethod]
        public void ShowExceptionDialog_Expected_WindowManagerInvokedForViewModel()
        {
            ExceptionViewModel vm = new ExceptionViewModel(new AsyncWorker());

            Mock<IWindowManager> mockWinManager = new Mock<IWindowManager>();
            mockWinManager.Setup(c => c.ShowDialog(It.IsAny<BaseViewModel>(), null, null)).Verifiable();

            vm.WindowNavigation = mockWinManager.Object;
            vm.Show();

            mockWinManager.Verify(mgr => mgr.ShowDialog(It.IsAny<ExceptionViewModel>(), null, null), Times.Once());
        }

        [TestMethod]
        public void SendErrorCommandTest()
        {
            Mock<IAsyncWorker> asyncWorker = new Mock<IAsyncWorker>();

            ExceptionViewModel vm = new ExceptionViewModel(asyncWorker.Object);

            vm.SendErrorCommand.Execute(null);

            asyncWorker.Verify(a => a.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>()), Times.Once);
            Assert.IsTrue(vm.Testing);
        }

        [TestMethod]
        public void CancelCommandTest()
        {
            Mock<IAsyncWorker> asyncWorker = new Mock<IAsyncWorker>();

            ExceptionViewModel vm = new ExceptionViewModel(asyncWorker.Object);
            vm.Testing = true;

            vm.CancelCommand.Execute(null);

            Assert.IsFalse(vm.Testing);
        }

        [TestMethod]
        public void ExceptionViewModelConstructorNullAsyncWorker()
        {
            IAsyncWorker asyncWorker = null;

            ExceptionViewModel vm = new ExceptionViewModel(asyncWorker);

            Assert.IsNotNull(vm.AsyncWorker);
        }

        [TestMethod]
        public void ExceptionViewModelConstructorNewAsyncWorker()
        {
            IAsyncWorker asyncWorker = new AsyncWorker();

            ExceptionViewModel vm = new ExceptionViewModel(asyncWorker);

            Assert.IsNotNull(vm.AsyncWorker);
            Assert.AreSame(asyncWorker, vm.AsyncWorker);
        }
    }
}