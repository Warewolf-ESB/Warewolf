using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Dev2.Services.Security;
using Dev2.Studio.UI.Tests.UIMaps.Settings;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Studio.UI.Tests.Tests.Settings
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class SecuritySettingsUiTests : UIMapBase
    {
        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SecuritySettingsUiTests")]
        public void SecuritySettingsUiTestsAddResourcesAndRelatedPriviledgesResourcesAreAddedSuccessfullyAndSaveButtonDisabled()
        {
            using(var securityWrapper = new SecuritySettingsUiMap())
            {
                //Add resource and set privileges
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

                //Remove resources and associated privileges
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
            using(var securityWrapper = new SecuritySettingsUiMap())
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

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SecuritySettingsUiTests")]
        public void SecuritySettingsUiTestsAdd20ResourcesMakeSureScrollBarIsThere()
        {
            //Set the settings file to have 20 resource permissions
            List<WindowsGroupPermission> permissionList = new List<WindowsGroupPermission>();
            for(int i = 0; i < 20; i++)
            {
                permissionList.Add(new WindowsGroupPermission { ResourceName = "Utility - Email", ResourceID = new Guid(), View = true, IsServer = false, WindowsGroup = "Users" });
            }
            permissionList.Add(WindowsGroupPermission.CreateAdministrators());
            var securityTO = new SecuritySettingsTO(permissionList);
            using(var webClient = new WebClient { Credentials = CredentialCache.DefaultCredentials, Encoding = Encoding.UTF8 })
            {
                webClient.UploadString("http://localhost:3142/services/SecurityWriteService", "POST", string.Format("<DataList><SecuritySettings>{0}</SecuritySettings></DataList>", JsonConvert.SerializeObject(securityTO)));
            }

            using(var securityWrapper = new SecuritySettingsUiMap())
            {
                Assert.IsTrue(securityWrapper.IsResourcePermissionScrollbarVisible());
            }

            //Set the settings file back to original state
            permissionList.Clear();
            permissionList.Add(WindowsGroupPermission.CreateAdministrators());
            securityTO = new SecuritySettingsTO(permissionList);
            using(var webClient = new WebClient { Credentials = CredentialCache.DefaultCredentials, Encoding = Encoding.UTF8 })
            {
                webClient.UploadString("http://localhost:3142/services/SecurityWriteService", "POST", string.Format("<DataList><SecuritySettings>{0}</SecuritySettings></DataList>", JsonConvert.SerializeObject(securityTO)));
            }
        }

    }
}





