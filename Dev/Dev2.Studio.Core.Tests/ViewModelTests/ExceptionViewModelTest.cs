
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
using Dev2.Studio.ViewModels.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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



        #endregion
        // ReSharper disable InconsistentNaming


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


    }
}
