
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
    [TestClass]
    public class IntallerActionsForDevelopmentTest
    {
        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InstallerActionsForDevelopment_ExecuteInstallerActions")]
        public void InstallerActionsForDevelopment_ExecuteInstallerActions_WhenNormalOperation_ExpectGroupCreatedAndUserAdded()
        {
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            warewolfGroupOps.DeleteWarewolfGroup();
            var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent(false);

            var installerActionsForDevelopment = new InstallerActionsForDevelopment();

            //------------Execute Test---------------------------
            installerActionsForDevelopment.ExecuteMoqInstallerActions();

            //------------Assert Results-------------------------
            var isGroupCreated = warewolfGroupOps.DoesWarewolfGroupExist();
            Assert.IsTrue(isGroupCreated);
            var result = warewolfGroupOps.IsUserInGroup(currentUser.Name);
            Assert.IsTrue(result);
            var isAdminAMember = warewolfGroupOps.IsAdminMemberOfWarewolf();
            Assert.IsTrue(isAdminAMember);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InstallerActionsForDevelopment_ExecuteInstallerActions")]
        // ReSharper disable InconsistentNaming
        public void InstallerActionsForDevelopment_ExecuteInstallerActions_WhenGroupExist_ExpectUserNotAdded()
        {
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();
            var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent(false);

            var installerActionsForDevelopment = new InstallerActionsForDevelopment();

            //------------Execute Test---------------------------
            installerActionsForDevelopment.ExecuteMoqInstallerActions();

            //------------Assert Results-------------------------
            var isGroupCreated = warewolfGroupOps.DoesWarewolfGroupExist();
            Assert.IsTrue(isGroupCreated);
            var result = warewolfGroupOps.IsUserInGroup(currentUser.Name);
            Assert.IsTrue(result);
            var isAdminAMember = warewolfGroupOps.IsAdminMemberOfWarewolf();
            Assert.IsTrue(isAdminAMember);
        }

        // ReSharper restore InconsistentNaming
    }
}
