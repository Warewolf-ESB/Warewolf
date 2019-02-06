/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.Configuration.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Configuration.Tests.ViewModels
{
    [TestClass]
    public class SettingsViewModelBaseTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SettingsViewModelBase))]
        public void SettingsViewModelBase_Object_SetProperty_ExpectValue()
        {
            //----------------------Arrange------------------------
            var obj = new object();

            var settingsViewModelBase = new TestSettingsViewModelBase();
            //----------------------Act----------------------------
            settingsViewModelBase.Object = obj;
            //----------------------Assert-------------------------
            Assert.AreEqual(obj, settingsViewModelBase.Object);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SettingsViewModelBase))]
        public void SettingsViewModelBase_UnderlyingObjectChanged_NotCalled_ExpectFalse()
        {
            //----------------------Arrange------------------------
            var isDelegateCalled = false;

            var settingsViewModelBase = new TestSettingsViewModelBase();
            //----------------------Act----------------------------
            settingsViewModelBase.UnderlyingObjectChanged += () => { isDelegateCalled = true; };
            //----------------------Assert-------------------------
            Assert.IsFalse(isDelegateCalled);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SettingsViewModelBase))]
        public void SettingsViewModelBase_UnderlyingObjectChanged_IsCalled_ExpectTrue()
        {
            //----------------------Arrange------------------------
            var isDelegateCalled = false;

            var settingsViewModelBase = new TestSettingsViewModelBase();
            //----------------------Act----------------------------
            settingsViewModelBase.UnderlyingObjectChanged += () => { isDelegateCalled = true; };
            settingsViewModelBase.TestOnUnderlyingObjectChanged();
            //----------------------Assert-------------------------
            Assert.IsTrue(isDelegateCalled);
        }

        class TestSettingsViewModelBase : SettingsViewModelBase
        {
            public void TestOnUnderlyingObjectChanged()
            {
                base.OnUnderlyingObjectChanged();
            }
        }
    }
}
