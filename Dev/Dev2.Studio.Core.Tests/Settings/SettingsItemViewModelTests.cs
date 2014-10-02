
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class SettingsItemViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsItemViewModel_Constructor")]
        public void SettingsItemViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsItemViewModel = new TestSettingsItemViewModel();

            //------------Assert Results-------------------------
            Assert.IsNotNull(settingsItemViewModel.CloseHelpCommand);
        }
    }
}
