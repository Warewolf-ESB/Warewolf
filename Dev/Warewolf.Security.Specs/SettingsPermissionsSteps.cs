#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, CC0022, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001, IDE0019, CC0105, RECS008, CA2202, IDE0016
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
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using SecPermissions = Dev2.Common.Interfaces.Security.Permissions;
namespace Dev2.Activities.Specs.Permissions
{
    [Binding]
    public class SettingsPermissionsSteps
    {
        readonly ScenarioContext _scenarioContext;

        public SettingsPermissionsSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException(nameof(scenarioContext));
            }

            this._scenarioContext = scenarioContext;
        }


        [BeforeFeature("@Security")]
        public static void InitializeFeature()
        {
            SetupUser();
            var securitySpecsUser = GetSecuritySpecsUser();
            var securitySpecsPassword = GetSecuritySpecsPassword();
            var userGroup = GetUserGroup();
            AppUsageStats.LocalHost = $"http://{Environment.MachineName.ToLowerInvariant()}:3142";
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            while (!environmentModel.IsConnected)
            {
                environmentModel.Connect();
            }

            var currentSettings = environmentModel.ResourceRepository.ReadSettings(environmentModel);
            FeatureContext.Current.Add("initialSettings", currentSettings);
            var settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission>())
            };

            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
            environmentModel.Disconnect();
            FeatureContext.Current.Add("environment", environmentModel);

            var reconnectModel = new Server(Guid.NewGuid(), new ServerProxy(AppUsageStats.LocalHost, securitySpecsUser, securitySpecsPassword)) { Name = "Other Connection" };
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

        static string GetUserGroup() => ConfigurationManager.AppSettings["userGroup"];

        static string GetSecuritySpecsPassword() => ConfigurationManager.AppSettings["SecuritySpecsPassword"];

        static string GetSecuritySpecsUser() => ConfigurationManager.AppSettings["SecuritySpecsUser"];

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
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out SecPermissions permission))
                {
                    groupPermssions.Permissions |= permission;
                }
            }
            var settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission> { groupPermssions })
            };

            var environmentModel = FeatureContext.Current.Get<IServer>("environment");
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
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out SecPermissions permission))
                {
                    groupPermssions.Permissions |= permission;
                }
            }
            var settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission> { groupPermssions })
            };

            var environmentModel = FeatureContext.Current.Get<IServer>("environment");
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
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out SecPermissions permission))
                {
                    groupPermssions.Permissions |= permission;
                }
            }
            var settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission> { groupPermssions })
            };

            var environmentModel = FeatureContext.Current.Get<IServer>("environment");
            EnsureEnvironmentConnected(environmentModel);
            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
            environmentModel.Disconnect();
        }



        static void EnsureEnvironmentConnected(IServer server)
        {
            if (!server.IsConnected)
            {
                server.Connect();
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

            var reconnectModel = new Server(Guid.NewGuid(), new ServerProxy(AppUsageStats.LocalHost, securitySpecsUser, GetSecuritySpecsPassword())) { Name = "Other Connection" };
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

        static IServer LoadResources()
        {
            var environmentModel = FeatureContext.Current.Get<IServer>("currentEnvironment");
            EnsureEnvironmentConnected(environmentModel);
            if (environmentModel.IsConnected && !environmentModel.HasLoadedResources)
            {
                environmentModel.ForceLoadResources();
            }

            return environmentModel;
        }

        [Then(@"resources should have ""(.*)""")]
        public static void ThenResourcesShouldHave(string resourcePerms)
        {
            var environmentModel = LoadResources();
            var resourcePermissions = SecPermissions.None;
            var permissionsStrings = resourcePerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out SecPermissions permission))
                {
                    resourcePermissions |= permission;
                }
            }
            var resourceModels = environmentModel.ResourceRepository.All();
            Assert.IsTrue(resourceModels.Count() > 0, "Cannot load any resources from " + environmentModel.DisplayName);
            var allMatch = resourceModels.Count(model => model.UserPermissions == resourcePermissions);
            var totalNumberOfResources = resourceModels.Count;
            var totalNumberOfResourcesWithoutMatch = totalNumberOfResources - allMatch;
            Assert.IsTrue(totalNumberOfResourcesWithoutMatch <= 1, "Total number of resources with " + resourcePermissions + " permission is " + allMatch + ". There are " + totalNumberOfResources + " resources in total. Therefore " + totalNumberOfResourcesWithoutMatch + " total resources do not have that permission.");
        }

        [Then(@"resources should not have ""(.*)""")]
        public void ThenResourcesShouldNotHave(string resourcePerms)
        {
            var environmentModel = LoadResources();
            var resourcePermissions = SecPermissions.None;
            var permissionsStrings = resourcePerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out SecPermissions permission))
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
            var environmentModel = FeatureContext.Current.Get<IServer>("environment");
            EnsureEnvironmentConnected(environmentModel);
            var resourceRepository = environmentModel.ResourceRepository;
            var settings = resourceRepository.ReadSettings(environmentModel);
            environmentModel.ForceLoadResources();

            var resourceModel = resourceRepository.FindSingle(model => model.Category.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(resourceModel, "Did not find: " + resourceName);
            var resourcePermissions = SecPermissions.None;
            var permissionsStrings = resourceRights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out SecPermissions permission))
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
            var environmentModel = FeatureContext.Current.Get<IServer>("currentEnvironment");
            EnsureEnvironmentConnected(environmentModel);
            var resourceRepository = environmentModel.ResourceRepository;
            environmentModel.ForceLoadResources();

            var resourceModel = resourceRepository.FindSingle(model => model.Category.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(resourceModel);
            var resourcePermissions = SecPermissions.None;
            var permissionsStrings = resourcePerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out SecPermissions permission))
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
            FeatureContext.Current.TryGetValue("currentEnvironment", out IServer currentEnvironment);
            FeatureContext.Current.TryGetValue("environment", out IServer server);
            FeatureContext.Current.TryGetValue("initialSettings", out Data.Settings.Settings currentSettings);

            if (server != null)
            {
                try
                {
                    if (currentSettings != null)
                    {
                        server.ResourceRepository.WriteSettings(server, currentSettings);
                    }
                }
                finally { server.Disconnect(); }


            }
            currentEnvironment?.Disconnect();
        }

        [Given(@"I have a server ""(.*)""")]
        public void GivenIHaveAServer(string serverName)
        {
            AppUsageStats.LocalHost = string.Format("http://{0}:3142", Environment.MachineName.ToLowerInvariant());
            var environmentModel = ServerRepository.Instance.Source;
            _scenarioContext.Add("environment", environmentModel);
        }
    }
}
