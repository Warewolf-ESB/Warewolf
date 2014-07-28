using Dev2.Services.Security.MoqInstallerActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Server.Tests
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
