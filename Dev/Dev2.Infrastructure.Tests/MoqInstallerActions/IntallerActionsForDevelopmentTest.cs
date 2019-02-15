/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Runtime.InteropServices;
using System.Security.Principal;
using Dev2.Services.Security.MoqInstallerActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.MoqInstallerActions
{
    [TestClass]
    public class IntallerActionsForDevelopmentTest
    {
        

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InstallerActionsForDevelopment_ExecuteInstallerActions")]
        public void InstallerActionsForDevelopment_ExecuteInstallerActions_WhenNormalOperation_ExpectGroupCreatedAndUserAdded()
        {
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            warewolfGroupOps.DeleteWarewolfGroup();
            var currentUser = WindowsIdentity.GetCurrent(false);

            var installerActionsForDevelopment = new InstallerActionsForDevelopment();

            //------------Execute Test---------------------------
            try
            { 
                installerActionsForDevelopment.ExecuteMoqInstallerActions();
            }
            catch (COMException e)
            {
                //'The Server service is not started.' error is expected in containers. See: https://github.com/moby/moby/issues/26409#issuecomment-304978309
                if (e.Message != "The Server service is not started.\r\n")
                {
                    throw e;
                }
            }

            //------------Assert Results-------------------------
            var isGroupCreated = warewolfGroupOps.DoesWarewolfGroupExist();
            Assert.IsTrue(isGroupCreated);
            var isAdminAMember = warewolfGroupOps.IsAdminMemberOfWarewolf();
            Assert.IsTrue(isAdminAMember);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InstallerActionsForDevelopment_ExecuteInstallerActions")]
        
        public void InstallerActionsForDevelopment_ExecuteInstallerActions_WhenGroupExist_ExpectUserNotAdded()
        {
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();
            var currentUser = WindowsIdentity.GetCurrent(false);

            var installerActionsForDevelopment = new InstallerActionsForDevelopment();

            //------------Execute Test---------------------------\
            try
            {
                installerActionsForDevelopment.ExecuteMoqInstallerActions();
            }
            catch (COMException e)
            {
                //'The Server service is not started.' error is expected in containers. See: https://github.com/moby/moby/issues/26409#issuecomment-304978309
                if (e.Message != "The Server service is not started.\r\n")
                {
                    throw e;
                }
            }

            //------------Assert Results-------------------------
            var isGroupCreated = warewolfGroupOps.DoesWarewolfGroupExist();
            Assert.IsTrue(isGroupCreated);
            var isAdminAMember = warewolfGroupOps.IsAdminMemberOfWarewolf();
            Assert.IsTrue(isAdminAMember);
        }


    }
}
