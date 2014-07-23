using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Dev2.Data.Settings;
using Dev2.Network;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Permissions
{
    [Binding]
    public class SettingsPermissionsSteps
    {
        [BeforeScenario("Security")]
        public void ClearSecuritySettings()
        {
            AppSettings.LocalHost = "http://localhost:3142";
            var environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();

            Data.Settings.Settings settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission>())
            };

            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
            environmentModel.Disconnect();
        }

        [Given(@"I have a server ""(.*)""")]
        public void GivenIHaveAServer(string serverName)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            var environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            ScenarioContext.Current.Add("environment", environmentModel);
        }

        [Given(@"it has '(.*)' with '(.*)'")]
        public void GivenItHasWith(string groupName, string groupRights)
        {
            var groupPermssions = new WindowsGroupPermission
            {
                WindowsGroup = groupName,
                ResourceID = Guid.Empty,
                IsServer = true
            };
            var permissionsStrings = groupRights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var permissionsString in permissionsStrings)
            {
                Services.Security.Permissions permission;
                if(Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    groupPermssions.Permissions |= permission;
                }
            }
            Data.Settings.Settings settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission> { groupPermssions })
            };

            var environmentModel = ScenarioContext.Current.Get<IEnvironmentModel>("environment");
            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
            environmentModel.Disconnect();
        }

        [When(@"connected as user part of '(.*)'")]
        public void WhenConnectedAsUserPartOf(string userGroup)
        {
            UserPrincipal userPrincipal;
            PrincipalContext context;
            SecurityIdentifier id;
            if(AccountExists("SpecsUser"))
            {
                if(!IsUserInGroup("SpecsUser", userGroup))
                {
                    try
                    {
                        id = GetUserSecurityIdentifier("SpecsUser");
                        context = new PrincipalContext(ContextType.Machine);
                        userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Sid, id.Value);
                        AddUserToGroup(userGroup, context, userPrincipal);
                    }
                    catch(Exception)
                    {
                        Assert.Fail("User not found");
                    }
                }
            }
            else
            {
                CreateLocalWindowsAccount("SpecsUser", userGroup);
            }

            id = GetUserSecurityIdentifier("SpecsUser");
            context = new PrincipalContext(ContextType.Machine);
            userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Sid, id.Value);

            if(userPrincipal != null)
            {
                Impersonator.RunAs(userPrincipal.DisplayName, "", "T35t3r!@#", () =>
                {
                    var windowsIdentity = WindowsIdentity.GetCurrent();
                    if(windowsIdentity != null)
                    {
                        //windowsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Users"));
                        Thread.CurrentPrincipal = new WindowsPrincipal(windowsIdentity);
                    }
                });
            }

            var reconnectModel = new EnvironmentModel(Guid.NewGuid(), new ServerProxy(AppSettings.LocalHost, "SpecsUser", "T35t3r!@#"), false) { Name = "Other Connection" };
            reconnectModel.Connect();
            ScenarioContext.Current.Add("currentEnvironment", reconnectModel);
        }

        public static bool CreateLocalWindowsAccount(string username, string groupName)
        {
            try
            {
                PrincipalContext context = new PrincipalContext(ContextType.Machine);
                UserPrincipal user = new UserPrincipal(context);
                user.SetPassword("T35t3r!@#");
                user.DisplayName = username;
                user.Name = username;
                user.UserCannotChangePassword = true;
                user.PasswordNeverExpires = true;

                user.Save();
                AddUserToGroup(groupName, context, user);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(@"error creating user" + ex.Message);
                return false;
            }
        }

        static void AddUserToGroup(string groupName, PrincipalContext context, UserPrincipal user)
        {
            GroupPrincipal usersGroup = GroupPrincipal.FindByIdentity(context, groupName);
            if(usersGroup != null)
            {
                usersGroup.Members.Add(user);
                usersGroup.Save();
            }
        }

        bool AccountExists(string name)
        {
            bool accountExists = false;
            try
            {
                var id = GetUserSecurityIdentifier(name);
                accountExists = id.IsAccountSid();
            }
            catch(Exception)
            {
                /* Invalid user account */
            }
            return accountExists;
        }

        static SecurityIdentifier GetUserSecurityIdentifier(string name)
        {
            NTAccount acct = new NTAccount(Environment.MachineName, name);
            SecurityIdentifier id = (SecurityIdentifier)acct.Translate(typeof(SecurityIdentifier));
            return id;
        }

        bool IsUserInGroup(string name, string group)
        {
            PrincipalContext context = new PrincipalContext(ContextType.Machine);
            GroupPrincipal usersGroup = GroupPrincipal.FindByIdentity(context, group);
            if(usersGroup != null)
            {
                var principalCollection = usersGroup.Members;
                if((from member in principalCollection
                    select name.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase)).Any())
                {
                    return true;
                }
            }
            return false;
        }

        [Then(@"resources should have '(.*)'")]
        public void ThenResourcesShouldHave(string resourcePerms)
        {
            var environmentModel = LoadResources();
            Services.Security.Permissions resourcePermissions = Services.Security.Permissions.None;
            var permissionsStrings = resourcePerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var permissionsString in permissionsStrings)
            {
                Services.Security.Permissions permission;
                if(Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    resourcePermissions |= permission;
                }
            }
            var resourceModels = environmentModel.ResourceRepository.All();
            var totalNumberOfResources = resourceModels.Count;
            var allMatch = resourceModels.Count(model => model.UserPermissions == resourcePermissions);
            Assert.IsTrue(totalNumberOfResources - allMatch <= 1); //This is to cater for the scenerios where we specify a resource permission
            environmentModel.Disconnect();
        }

        static IEnvironmentModel LoadResources()
        {
            var environmentModel = ScenarioContext.Current.Get<IEnvironmentModel>("currentEnvironment");
            if(environmentModel.IsConnected)
            {
                if(!environmentModel.HasLoadedResources)
                {
                    environmentModel.ForceLoadResources();
                }
            }
            else
            {
                environmentModel.Connect();
                environmentModel.ForceLoadResources();

            }
            return environmentModel;
        }

        [Then(@"resources should not have '(.*)'")]
        public void ThenResourcesShouldNotHave(string resourcePerms)
        {
            var environmentModel = LoadResources();
            Services.Security.Permissions resourcePermissions = Services.Security.Permissions.None;
            var permissionsStrings = resourcePerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var permissionsString in permissionsStrings)
            {
                Services.Security.Permissions permission;
                if(Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    resourcePermissions |= permission;
                }
            }
            var resourceModels = environmentModel.ResourceRepository.All();
            var allMatch = resourceModels.Count(model => model.UserPermissions == resourcePermissions);
            var totalNumberOfResources = resourceModels.Count;
            Assert.IsTrue(totalNumberOfResources - allMatch <= 1);
            environmentModel.Disconnect();
        }

        [Given(@"Resource '(.*)' has rights '(.*)' for '(.*)'")]
        public void GivenResourceHasRights(string resourceName, string resourceRights, string groupName)
        {
            var environmentModel = ScenarioContext.Current.Get<IEnvironmentModel>("environment");
            if(!environmentModel.IsConnected)
            {
                environmentModel.Connect();
            }
            var resourceRepository = environmentModel.ResourceRepository;
            var settings = resourceRepository.ReadSettings(environmentModel);
            environmentModel.ForceLoadResources();

            var resourceModel = resourceRepository.FindSingle(model => model.Category.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
            Services.Security.Permissions resourcePermissions = Services.Security.Permissions.None;
            var permissionsStrings = resourceRights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var permissionsString in permissionsStrings)
            {
                Services.Security.Permissions permission;
                if(Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    resourcePermissions |= permission;
                }
            }
            settings.Security.WindowsGroupPermissions.RemoveAll(permission => permission.ResourceID == resourceModel.ID);
            var windowsGroupPermission = new WindowsGroupPermission { WindowsGroup = groupName, ResourceID = resourceModel.ID, ResourceName = resourceName, IsServer = false, Permissions = resourcePermissions };
            settings.Security.WindowsGroupPermissions.Add(windowsGroupPermission);
            resourceRepository.WriteSettings(environmentModel, settings);
        }

        [Then(@"'(.*)' should have '(.*)'")]
        public void ThenShouldHave(string resourceName, string resourcePerms)
        {
            var environmentModel = ScenarioContext.Current.Get<IEnvironmentModel>("currentEnvironment");
            var resourceRepository = environmentModel.ResourceRepository;
            environmentModel.ForceLoadResources();

            var resourceModel = resourceRepository.FindSingle(model => model.Category.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
            Services.Security.Permissions resourcePermissions = Services.Security.Permissions.None;
            var permissionsStrings = resourcePerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var permissionsString in permissionsStrings)
            {
                Services.Security.Permissions permission;
                if(Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    resourcePermissions |= permission;
                }
            }
            if(resourceModel != null)
            {
                Assert.AreEqual(resourcePermissions, resourceModel.UserPermissions);
            }
        }
    }
}
