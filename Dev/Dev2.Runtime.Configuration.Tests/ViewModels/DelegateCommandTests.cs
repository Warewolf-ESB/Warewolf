
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
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Runtime.Configuration.Tests.ViewModels
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class DelegateCommandTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RelayCommand_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelayCommand_Constructor_ActionIsNull_ThrowsException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new DelegateCommand(null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RelayCommand_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelayCommand_ConstructorOverload_ActionIsNull_ThrowsException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new DelegateCommand(null, o => false);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DelegateCommand_Execute")]
        public void DelegateCommand_Execute_PassingAnObject_ObjectPassedToAction()
        {
            //------------Setup for test--------------------------
            dynamic prop = null;
            var delegateCommand = new DelegateCommand(o =>
                {
                    prop = o;
                });
            //------------Execute Test---------------------------
            delegateCommand.Execute(new { Name = "Tshepo", Surname = "Ntlhokoa" });
            //------------Assert Results-------------------------
            Assert.IsNotNull(prop);
            Assert.IsNotNull("Tshepo", prop.Name);
            Assert.IsNotNull("Ntlhokoa", prop.Surname);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DelegateCommand_CanExecute")]
        public void DelegateCommand_CanExecute_WhenConstructedWithAPredicate_PredicateIsCalled()
        {
            //------------Setup for test--------------------------
            var canExecuteWasCalled = false;
            var delegateCommand = new DelegateCommand(o => { }, o =>
                {
                    canExecuteWasCalled = true;
                    return true;
                });
            //------------Execute Test---------------------------
            var canExecute = delegateCommand.CanExecute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(canExecuteWasCalled);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DelegateCommand_CanExecute")]
        public void DelegateCommand_CanExecute_WhenConstructedWithoutAPredicate_ReturnsTrueAsADefault()
        {
            //------------Setup for test--------------------------
            var delegateCommand = new DelegateCommand(o => { });
            //------------Execute Test---------------------------
            var canExecute = delegateCommand.CanExecute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(canExecute);
        }
    }
}
