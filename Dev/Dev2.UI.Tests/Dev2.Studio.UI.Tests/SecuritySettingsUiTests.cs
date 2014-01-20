using Dev2.Studio.UI.Tests.WrapperClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class SecuritySettingsUiTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SecuritySettingsUiTests")]
        public void SecuritySettingsUiTestsAddResourcesAndRelatedPriviledgesResourcesAreAddedSuccessfullyAndSaveButtonDisabled()
        {
            using (var securityWrapper = new ManageSecuritySecuritySettingsTestWrapper())
            {
                //Add resource and set priviledges
                securityWrapper.AddResource("Utility - Email", "WORKFLOWS", "EXAMPLES");
                securityWrapper.SetWindowsGroupText("Administrators");
                securityWrapper.ClickSaveButton();
                Assert.AreEqual("Administrators", securityWrapper.GetWindowsGroupText());
                Assert.IsFalse(securityWrapper.IsSaveButtonEnabled());

                //Modify the properties
                securityWrapper.SetViewCheckBox(true);
                securityWrapper.SetExecuteCheckBox(true);
                securityWrapper.SetContributeCheckBox(true);
                Assert.IsTrue(securityWrapper.IsSaveButtonEnabled());
                securityWrapper.ClickSaveButton();
                Assert.IsFalse(securityWrapper.IsSaveButtonEnabled());

                //Remove resources and associated priviledges
                securityWrapper.SetWindowsGroupText(string.Empty);
                securityWrapper.SetViewCheckBox(false);
                securityWrapper.ClickSaveButton();
                Assert.AreEqual(string.Empty, securityWrapper.GetWindowsGroupText());
            }
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SecuritySettingsUiTests")]
        public void SecuritySettingsUiTestsOpenHelpAdornersHelpAdornersOpenedAndClosedSuccessfully()
        {
            using (var securityWrapper = new ManageSecuritySecuritySettingsTestWrapper())
            {
                //Toggle Server Help
                securityWrapper.ToggleServerHelpButton();
                Assert.IsTrue(securityWrapper.IsCloseHelpViewButtonEnabled());
                //Toggle Resource Help
                securityWrapper.ToggleResourceHelpButton();
                Assert.IsTrue(securityWrapper.IsCloseHelpViewButtonEnabled());
                securityWrapper.ToggleResourceHelpButton();
            }
        }

    }
}





