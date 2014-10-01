
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Services.Security.MoqInstallerActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.MoqInstallerActions
{
    /// <summary>
    /// Summary description for MoqInstallerActionFactoryTest
    /// </summary>
    [TestClass]
    public class MoqInstallerActionFactoryTest
    {
        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("MoqInstallerActionFactory_CreateInstallerActions")]
        public void MoqInstallerActionFactory_CreateInstallerActions_WhenCreatingNew_ExpectNewObject()
        {
            //------------Execute Test---------------------------
            var result = MoqInstallerActionFactory.CreateInstallerActions();

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("MoqInstallerActionFactory_CreateSecurityOperationsObject")]
        public void MoqInstallerActionFactory_CreateSecurityOperationsObject_WhenCreatingNew_ExpectNewObject()
        {
            //------------Execute Test---------------------------
            var result = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
        }

        // ReSharper restore InconsistentNaming
    }
}
