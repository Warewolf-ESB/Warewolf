
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    /// <summary>
    /// Summary description for SecuritySettingsTOTests
    /// </summary>
    [TestClass]
    public class SecuritySettingsTOTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SecuritySettingsTO_Contructor")]
        public void SecuritySettingsTO_Contructor_CallBasicCtor_WindowsGroupPermissionsEmpty()
        {
            //------------Execute Test---------------------------
            var securitySettingsTO = new SecuritySettingsTO();
            //------------Assert Results-------------------------
            Assert.IsNotNull(securitySettingsTO.WindowsGroupPermissions);
            Assert.AreEqual(0, securitySettingsTO.WindowsGroupPermissions.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SecuritySettingsTO_Contructor")]
        public void SecuritySettingsTO_Contructor_CallOverloadedCtor_WindowsGroupPermissionsEmpty()
        {
            //------------Setup for test--------------------------
            List<WindowsGroupPermission> permissions = new List<WindowsGroupPermission>();
            permissions.Add(new WindowsGroupPermission());
            permissions.Add(new WindowsGroupPermission());
            //------------Execute Test---------------------------
            var securitySettingsTO = new SecuritySettingsTO(permissions);
            //------------Assert Results-------------------------
            Assert.IsNotNull(securitySettingsTO.WindowsGroupPermissions);
            Assert.AreEqual(2, securitySettingsTO.WindowsGroupPermissions.Count);
        }
    }
}
