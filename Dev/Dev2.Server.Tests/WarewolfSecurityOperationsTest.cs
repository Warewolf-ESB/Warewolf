
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
using System.Reflection;
using Dev2.Services.Security.MoqInstallerActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace Dev2.InstallerActions
// ReSharper restore CheckNamespace
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WarewolfSecurityOperationsTest
    {
        // ReSharper disable InconsistentNaming
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_AddWarewolfGroup")]
        public void WarewolfSecurityOperations_AddWarewolfGroup_ExpectGroupAdded()
        {
            var grpOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            grpOps.DeleteWarewolfGroup();
            grpOps.AddWarewolfGroup();
            var result = grpOps.DoesWarewolfGroupExist();

            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_DoesWarewolfGroupExist")]
        public void WarewolfSecurityOperationsDoesWarewolfGroupExistWhenGroupDoesNotExistExpectFalse()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            warewolfGroupOps.DeleteWarewolfGroup();

            //------------Execute Test---------------------------
            var result = warewolfGroupOps.DoesWarewolfGroupExist();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_DoesWarewolfGroupExist")]
        public void WarewolfSecurityOperations_DoesWarewolfGroupExist_WhenGroupDoesExist_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();

            //------------Execute Test---------------------------
            var result = warewolfGroupOps.DoesWarewolfGroupExist();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_DeleteGroup")]
        public void WarewolfSecurityOperations_DeleteGroupWorks_WhenGroupExist_ExpectGroupDeleted()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();

            //------------Execute Test---------------------------
            warewolfGroupOps.DeleteWarewolfGroup();

            //------------Assert Results-------------------------

            // Will throw exception on failure ;)
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_AddWarewolfGroupToAdministrators")]
        public void WarewolfSecurityOperations_AddWarewolfGroupToAdministrators_WhenNotAMember_ExpectNotAdded()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // Delete warewolf if already a member...
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();

            //------------Execute Test---------------------------
            var result = warewolfGroupOps.IsAdminMemberOfWarewolf();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_AddWarewolfGroupToAdministrators")]
        public void WarewolfSecurityOperations_AddWarewolfGroupToAdministrators_WhenNotAlreadyMember_ExpectAdministratorsMemberOfWarewolf()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // Delete warewolf if already a member...
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();

            //------------Execute Test---------------------------
            warewolfGroupOps.AddAdministratorsGroupToWarewolf();
            var result = warewolfGroupOps.IsAdminMemberOfWarewolf();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_AddWarewolfGroupToAdministrators")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void WarewolfSecurityOperations_AddWarewolfGroupToAdministrators_WhenAlreadyMember_ExpectException()
        {

            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // Delete warewolf if already a member...
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();
            warewolfGroupOps.AddAdministratorsGroupToWarewolf();

            //------------Execute Test---------------------------
            warewolfGroupOps.AddAdministratorsGroupToWarewolf();
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_DeleteGroup")]
        public void WarewolfSecurityOperations_AddDomainUserToWarewolfGroup_WhenUserNotPresent_ExpectUserAdded()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();
            var myPc = Environment.MachineName;

            var userStr = warewolfGroupOps.FormatUserForInsert("Dev2\\IntegrationTester", myPc);

            //------------Execute Test---------------------------
            warewolfGroupOps.AddUserToWarewolf(userStr);
            var result = warewolfGroupOps.IsUserInGroup("Dev2\\IntegrationTester");

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_DeleteGroup")]
        public void WarewolfSecurityOperations_IsUserInGroup_WhenUserNotPresent_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            //------------Execute Test---------------------------
            var result = warewolfGroupOps.IsUserInGroup("Dev2\\MyNewUser");

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_DeleteGroup")]
        public void WarewolfSecurityOperations_AddLocalUserToWarewolfGroup_WhenUserNotPresent_ExpectUserAdded()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            warewolfGroupOps.DeleteWarewolfGroup();
            warewolfGroupOps.AddWarewolfGroup();
            var myPc = Environment.MachineName;

            var userStr = warewolfGroupOps.FormatUserForInsert("Guest", myPc);

            //------------Execute Test---------------------------
            warewolfGroupOps.AddUserToWarewolf(userStr);
            var result = warewolfGroupOps.IsUserInGroup("Guest");

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_DeleteGroup")]
        public void WarewolfSecurityOperations_FormatUserForInsert_WhenLocalUser_ExpectUserFormated()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // Environment.MachineName
            //------------Execute Test---------------------------
            var result = warewolfGroupOps.FormatUserForInsert("Guest", "MyPC");

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "WinNT://MyPC/Guest");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_DeleteGroup")]
        public void WarewolfSecurityOperations_FormatUserForInsert_WhenDomainUser_ExpectUserFormated()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // Environment.MachineName
            //------------Execute Test---------------------------
            var result = warewolfGroupOps.FormatUserForInsert("Dev2\\DummyUser", "MyPC");

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "WinNT://Dev2/DummyUser");
        }

        #region Exception Test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_AddUserToWarewolf")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WarewolfSecurityOperations_AddUserToWarewolfGroup_WhenUserNull_ExpectException()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            //------------Execute Test---------------------------
            warewolfGroupOps.AddUserToWarewolf(null);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_FormatUserForInsert")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WarewolfSecurityOperations_FormatUserForInsert_WhenNullUser_ExpectException()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            //------------Execute Test---------------------------
            warewolfGroupOps.FormatUserForInsert(null, null);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_FormatUserForInsert")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WarewolfSecurityOperations_FormatUserForInsert_WhenNullMachineName_ExpectException()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            //------------Execute Test---------------------------
            warewolfGroupOps.FormatUserForInsert("testUser", null);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WarewolfSecurityOperations_DeleteGroup")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WarewolfSecurityOperations_IsUserInGroup_WhenUserNotPresent_ExpectException()
        {
            //------------Setup for test--------------------------
            var warewolfGroupOps = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            //------------Execute Test---------------------------
            warewolfGroupOps.IsUserInGroup(null);
        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}
