/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Dev2.Activities.Specs.Scheduler;
using Dev2.Network;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using SecPermissions = Dev2.Common.Interfaces.Security.Permissions;
namespace Dev2.Activities.Specs.Permissions
{
    [Binding]
    public class SettingsPermissionsSteps
    {
        private readonly ScenarioContext scenarioContext;

        public SettingsPermissionsSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException(nameof(scenarioContext));
            this.scenarioContext = scenarioContext;
        }


        [BeforeFeature("@Security")]
        public static void InitializeFeature()
        {
            SetupUser();
            var securitySpecsUser = GetSecuritySpecsUser();
            var securitySpecsPassword = GetSecuritySpecsPassword();
            var userGroup = GetUserGroup();
            AppSettings.LocalHost = $"http://{Environment.MachineName.ToLowerInvariant()}:3142";
            var environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            while (!environmentModel.IsConnected)
            {
                environmentModel.Connect();
            }

            var currentSettings = environmentModel.ResourceRepository.ReadSettings(environmentModel);
            FeatureContext.Current.Add("initialSettings", currentSettings);
            Data.Settings.Settings settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission>())
            };

            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
            environmentModel.Disconnect();
            FeatureContext.Current.Add("environment", environmentModel);

            var reconnectModel = new EnvironmentModel(Guid.NewGuid(), new ServerProxy(AppSettings.LocalHost, securitySpecsUser, securitySpecsPassword)) { Name = "Other Connection" };
            try
            {
                reconnectModel.Connect();
            }
            catch (UnauthorizedAccessException)
            {
                Assert.Fail("Connection unauthorized when connecting to local Warewolf server as user who is part of '" + userGroup + "' user group.");
            }
            FeatureContext.Current.Add("currentEnvironment", reconnectModel);

        }

        private static string GetUserGroup()
        {
            return ConfigurationManager.AppSettings["userGroup"];
        }

        private static string GetSecuritySpecsPassword()
        {
            return ConfigurationManager.AppSettings["SecuritySpecsPassword"];
        }

        private static string GetSecuritySpecsUser()
        {
            return ConfigurationManager.AppSettings["SecuritySpecsUser"];
        }

        [Given(@"it has ""(.*)"" with ""(.*)""")]
        public void GivenItHasWith(string groupName, string groupRights)
        {
            var groupPermssions = new WindowsGroupPermission
            {
                WindowsGroup = groupName,
                ResourceID = Guid.Empty,
                IsServer = true
            };
            var permissionsStrings = groupRights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                SecPermissions permission;
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    groupPermssions.Permissions |= permission;
                }
            }
            Data.Settings.Settings settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission> { groupPermssions })
            };
            
            var environmentModel = FeatureContext.Current.Get<IEnvironmentModel>("environment");
            EnsureEnvironmentConnected(environmentModel);
            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
            environmentModel.Disconnect();
        }
        [Given(@"I have Public with ""(.*)""")]
        public void GivenIHavePublicWith(string groupRights)
        {
            var groupPermssions = new WindowsGroupPermission
            {
                WindowsGroup = "Public",
                ResourceID = Guid.Empty,
                IsServer = true
            };
            var permissionsStrings = groupRights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                SecPermissions permission;
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    groupPermssions.Permissions |= permission;
                }
            }
            Data.Settings.Settings settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission> { groupPermssions })
            };

            var environmentModel = FeatureContext.Current.Get<IEnvironmentModel>("environment");
            EnsureEnvironmentConnected(environmentModel);
            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
            environmentModel.Disconnect();
        }

        [Given(@"I have Users with ""(.*)""")]
        public void GivenIHaveUsersWith(string groupRights)
        {
            var groupPermssions = new WindowsGroupPermission
            {
                WindowsGroup = "Users",
                ResourceID = Guid.Empty,
                IsServer = true
            };
            var permissionsStrings = groupRights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                SecPermissions permission;
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    groupPermssions.Permissions |= permission;
                }
            }
            Data.Settings.Settings settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission> { groupPermssions })
            };

            var environmentModel = FeatureContext.Current.Get<IEnvironmentModel>("environment");
            EnsureEnvironmentConnected(environmentModel);
            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
            environmentModel.Disconnect();
        }



        static void EnsureEnvironmentConnected(IEnvironmentModel environmentModel)
        {
            if (!environmentModel.IsConnected)
            {
                environmentModel.Connect();
            }
        }

        static void SetupUser()
        {
            var securitySpecsUser = GetSecuritySpecsUser();
            var accountExists = SchedulerSteps.AccountExists(securitySpecsUser);
            if (!accountExists)
            {
                try
                {
                    SchedulerSteps.CreateLocalWindowsAccount(GetSecuritySpecsUser(), GetSecuritySpecsPassword(), GetUserGroup());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"error creating user" + ex.Message);
                }
            }
        }

        [When(@"connected as user part of ""(.*)""")]
        public void WhenConnectedAsUserPartOf(string userGroup)
        {
            var securitySpecsUser = GetSecuritySpecsUser();

            var reconnectModel = new EnvironmentModel(Guid.NewGuid(), new ServerProxy(AppSettings.LocalHost, securitySpecsUser, GetSecuritySpecsPassword())) { Name = "Other Connection" };
            try
            {
                
                reconnectModel.Connect();
            }
            catch (UnauthorizedAccessException)
            {
                Assert.Fail("Connection unauthorized when connecting to local Warewolf server as user who is part of '" + userGroup + "' user group.");
            }
            FeatureContext.Current["currentEnvironment"] = reconnectModel;
        }

        IEnvironmentModel LoadResources()
        {
            var environmentModel = FeatureContext.Current.Get<IEnvironmentModel>("currentEnvironment");
            EnsureEnvironmentConnected(environmentModel);
            if (environmentModel.IsConnected)
            {
                if (!environmentModel.HasLoadedResources)
                {
                    environmentModel.ForceLoadResources();
                }
            }
            return environmentModel;
        }

        [Then(@"resources should have ""(.*)""")]
        public void ThenResourcesShouldHave(string resourcePerms)
        {
            var environmentModel = LoadResources();
            SecPermissions resourcePermissions = SecPermissions.None;
            var permissionsStrings = resourcePerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                SecPermissions permission;
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    resourcePermissions |= permission;
                }
            }
            var resourceModels = environmentModel.ResourceRepository.All();
            var allMatch = resourceModels.Count(model => model.UserPermissions == resourcePermissions);
            var totalNumberOfResources = resourceModels.Count;
            var totalNumberOfResourcesWithoutMatch = totalNumberOfResources - allMatch;
            Assert.IsTrue(totalNumberOfResourcesWithoutMatch <= 1, "Total number of resources with " + resourcePermissions + " permission is " + allMatch + ". There are " + totalNumberOfResources + " resources in total. Therefore " + totalNumberOfResourcesWithoutMatch + " total resources do not have that permission.");
        }

        [Then(@"resources should not have ""(.*)""")]
        public void ThenResourcesShouldNotHave(string resourcePerms)
        {
            var environmentModel = LoadResources();
            SecPermissions resourcePermissions = SecPermissions.None;
            var permissionsStrings = resourcePerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                SecPermissions permission;
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    resourcePermissions |= permission;
                }
            }
            var resourceModels = environmentModel.ResourceRepository.All();
            var allMatch = resourceModels.Count(model => model.UserPermissions == resourcePermissions);
            var totalNumberOfResources = resourceModels.Count;
            var totalNumberOfResourcesWithoutMatch = totalNumberOfResources - allMatch;
            Assert.IsTrue(totalNumberOfResourcesWithoutMatch <= 1, "Total number of resources with " + resourcePermissions + " permission is " + allMatch + ". There are " + totalNumberOfResources + " resources in total. Therefore " + totalNumberOfResourcesWithoutMatch + " total resources do not have that permission.");
        }

        [Given(@"Resource ""(.*)"" has rights ""(.*)"" for ""(.*)""")]
        public void GivenResourceHasRights(string resourceName, string resourceRights, string groupName)
        {
            var environmentModel = FeatureContext.Current.Get<IEnvironmentModel>("environment");
            EnsureEnvironmentConnected(environmentModel);
            var resourceRepository = environmentModel.ResourceRepository;
            var settings = resourceRepository.ReadSettings(environmentModel);
            environmentModel.ForceLoadResources();

            var resourceModel = resourceRepository.FindSingle(model => model.Category.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(resourceModel, "Did not find: " + resourceName);
            SecPermissions resourcePermissions = SecPermissions.None;
            var permissionsStrings = resourceRights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                SecPermissions permission;
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    resourcePermissions |= permission;
                }
            }
            settings.Security.WindowsGroupPermissions.RemoveAll(permission => permission.ResourceID == resourceModel.ID);
            var windowsGroupPermission = new WindowsGroupPermission { WindowsGroup = groupName, ResourceID = resourceModel.ID, ResourceName = resourceName, IsServer = false, Permissions = resourcePermissions };
            settings.Security.WindowsGroupPermissions.Add(windowsGroupPermission);
            var SettingsWriteResult = resourceRepository.WriteSettings(environmentModel, settings);
            Assert.IsFalse(SettingsWriteResult.HasError, "Cannot setup for security spec.\n Error writing initial resource permissions settings to localhost server.\n" + SettingsWriteResult.Message);
        }

        [Then(@"""(.*)"" should have ""(.*)""")]
        public void ThenShouldHave(string resourceName, string resourcePerms)
        {
            var environmentModel = FeatureContext.Current.Get<IEnvironmentModel>("currentEnvironment");
            EnsureEnvironmentConnected(environmentModel);
            var resourceRepository = environmentModel.ResourceRepository;
            environmentModel.ForceLoadResources();

            var resourceModel = resourceRepository.FindSingle(model => model.Category.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(resourceModel);
            SecPermissions resourcePermissions = SecPermissions.None;
            var permissionsStrings = resourcePerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                SecPermissions permission;
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    resourcePermissions |= permission;
                }
            }
            resourceModel.UserPermissions = environmentModel.AuthorizationService.GetResourcePermissions(resourceModel.ID);
            Assert.AreEqual(resourcePermissions, resourceModel.UserPermissions);
        }

        [AfterScenario("Security")]
        public void DoCleanUp()
        {
            IEnvironmentModel currentEnvironment;
            FeatureContext.Current.TryGetValue("currentEnvironment", out currentEnvironment);
            IEnvironmentModel environmentModel;
            FeatureContext.Current.TryGetValue("environment", out environmentModel);
            Data.Settings.Settings currentSettings;
            FeatureContext.Current.TryGetValue("initialSettings", out currentSettings);

            if (environmentModel != null)
            {
                try
                {
                    if (currentSettings != null)
                        environmentModel.ResourceRepository.WriteSettings(environmentModel, currentSettings);

                }
                finally { environmentModel.Disconnect(); }


            }
            currentEnvironment?.Disconnect();
        }

        [Given(@"I have a server ""(.*)""")]
        public void GivenIHaveAServer(string serverName)
        {
            AppSettings.LocalHost = string.Format("http://{0}:3142", Environment.MachineName.ToLowerInvariant());
            var environmentModel = EnvironmentRepository.Instance.Source;
            scenarioContext.Add("environment", environmentModel);
        }
    }
}
