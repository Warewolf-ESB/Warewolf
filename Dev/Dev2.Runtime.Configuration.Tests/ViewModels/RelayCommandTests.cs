/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Configuration.Tests.ViewModels
{
    [TestClass]
    public class RelayCommandTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RelayCommand))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelayCommand_Constructor_ActionIsNull_ThrowsException()
        {
            new RelayCommand(null);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RelayCommand))]
        public void RelayCommand_Execute_PassingAnObject_ObjectPassedToAction()
        {
            //------------Setup for test--------------------------
            dynamic prop = null;
            var relayCommand = new RelayCommand(o =>
            {
                prop = o;
            });
            //------------Execute Test---------------------------
            relayCommand.Execute(new { Name = "Tshepo", Surname = "Ntlhokoa" });
            //------------Assert Results-------------------------
            Assert.IsNotNull(prop);
            Assert.AreEqual("Tshepo", prop.Name);
            Assert.AreEqual("Ntlhokoa", prop.Surname);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RelayCommand))]
        public void RelayCommand_CanExecute_WhenConstructedWithAPredicate_PredicateIsCalled()
        {
            //------------Setup for test--------------------------
            var canExecuteWasCalled = false;
            var relayCommand = new RelayCommand(o => { }, o =>
            {
                canExecuteWasCalled = true;
                return true;
            });
            //------------Execute Test---------------------------
            var canExecute = relayCommand.CanExecute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(canExecuteWasCalled);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RelayCommand))]
        public void RelayCommand_CanExecute_WhenConstructedWithoutAPredicate_ReturnsTrueAsADefault()
        {
            //------------Setup for test--------------------------
            var relayCommand = new RelayCommand(o => { });
            //------------Execute Test---------------------------
            var canExecute = relayCommand.CanExecute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RelayCommand))]
        public void RelayCommand_RaiseCanExecuteChanged_EventNotTriggered_isDelegateCalled_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var isDelegateCalled = false;

            var relayCommand = new RelayCommand(o => { });
            //------------Execute Test---------------------------
            relayCommand.RaiseCanExecuteChanged();
            //------------Assert Results-------------------------
            Assert.IsFalse(isDelegateCalled);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RelayCommand))]
        public void RelayCommand_RaiseCanExecuteChanged_EventTriggered_isDelegateCalled_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var isDelegateCalled = false;
            
            var relayCommand = new RelayCommand(o => { });
            //------------Execute Test---------------------------
            relayCommand.CanExecuteChanged += (s, MouseEventArgs) => { isDelegateCalled = true; };
            relayCommand.RaiseCanExecuteChanged();
            //------------Assert Results-------------------------
            Assert.IsTrue(isDelegateCalled);
        }
    }
}
