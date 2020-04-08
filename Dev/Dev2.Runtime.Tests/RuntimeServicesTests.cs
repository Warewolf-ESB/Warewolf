/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Runtime.ESB.Management;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Dev2.Tests.Runtime
{
    [TestClass]
    public class RuntimeServicesTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(EsbManagementServiceLocator))]
        public void EsbManagementServiceLocator_GivenGetServicesCalled_ExpectSpecificServices()
        {
            var services = EsbManagementServiceLocator.GetServices();

            var names = services.Select(o => o.Name).ToDictionary((s) => s);

            Assert.IsTrue(names.Remove("SavePerformanceCounters"));
            Assert.IsTrue(names.Remove("DeleteAllTestsService"));
            Assert.IsTrue(names.Remove("ReloadResourceService"));
            Assert.IsTrue(names.Remove("GetSharepointListService"));
            Assert.IsTrue(names.Remove("FetchDependantCompileMessagesService"));
            Assert.IsTrue(names.Remove("SettingsWriteService"));
            Assert.IsTrue(names.Remove("FindDependencyService"));
            Assert.IsTrue(names.Remove("TestSqliteService"));
            Assert.IsTrue(names.Remove("DeployResourceService"));
            Assert.IsTrue(names.Remove("DeleteResourceService"));
            Assert.IsTrue(names.Remove("FindOptionsBy"));
            Assert.IsTrue(names.Remove("FetchExchangeSources"));
            Assert.IsTrue(names.Remove("TestRabbitMQServiceSource"));
            Assert.IsTrue(names.Remove("FetchCurrentServerLogService"));
            Assert.IsTrue(names.Remove("GetScheduledResourceHistoryService"));
            Assert.IsTrue(names.Remove("FetchComPluginSources"));
            Assert.IsTrue(names.Remove("GetScheduledResources"));
            Assert.IsTrue(names.Remove("SaveRabbitMQServiceSource"));
            Assert.IsTrue(names.Remove("DeleteScheduledResourceService"));
            Assert.IsTrue(names.Remove("TestExchangeServiceSource"));
            Assert.IsTrue(names.Remove("GetServerSettings"));
            Assert.IsTrue(names.Remove("TestDbSourceService"));
            Assert.IsTrue(names.Remove("RenameFolderService"));
            Assert.IsTrue(names.Remove("GetMinSupportedVersion"));
            Assert.IsTrue(names.Remove("SecurityReadService"));
            Assert.IsTrue(names.Remove("FindOptions"));
            Assert.IsTrue(names.Remove("SaveComPluginSource"));
            Assert.IsTrue(names.Remove("GetServerInformation"));
            Assert.IsTrue(names.Remove("RenameItemService"));
            Assert.IsTrue(names.Remove("SaveWebService"));
            Assert.IsTrue(names.Remove("SavePluginSource"));
            Assert.IsTrue(names.Remove("SaveServerSettings"));
            Assert.IsTrue(names.Remove("TestDbService"));
            Assert.IsTrue(names.Remove("TestWcfServiceSource"));
            Assert.IsTrue(names.Remove("SettingsReadService"));
            Assert.IsTrue(names.Remove("FetchComPluginNameSpaces"));
            Assert.IsTrue(names.Remove("FindSourcesByType"));
            Assert.IsTrue(names.Remove("SaveOAuthSource"));
            Assert.IsTrue(names.Remove("SaveTriggerQueueService"));
            Assert.IsTrue(names.Remove("FindResourceService"));
            Assert.IsTrue(names.Remove("SaveSharepointServerService"));
            Assert.IsTrue(names.Remove("FetchPluginActions"));
            Assert.IsTrue(names.Remove("FetchWcfSources"));
            Assert.IsTrue(names.Remove("TestEmailServiceSource"));
            Assert.IsTrue(names.Remove("SaveTests"));
            Assert.IsTrue(names.Remove("DuplicateResourceService"));
            Assert.IsTrue(names.Remove("GetSharepointListFields"));
            Assert.IsTrue(names.Remove("FetchDbActions"));
            Assert.IsTrue(names.Remove("FetchTestsForDeploy"));
            Assert.IsTrue(names.Remove("SaveScheduledResourceService"));
            Assert.IsTrue(names.Remove("FetchTriggerQueues"));
            Assert.IsTrue(names.Remove("FetchRemoteDebugMessagesService"));
            Assert.IsTrue(names.Remove("TestWebService"));
            Assert.IsTrue(names.Remove("GetDatabaseTablesService"));
            Assert.IsTrue(names.Remove("FetchTests"));
            Assert.IsTrue(names.Remove("TestWcfService"));
            Assert.IsTrue(names.Remove("LoggingSettingsReadService"));
            Assert.IsTrue(names.Remove("FetchPerformanceCounters"));
            Assert.IsTrue(names.Remove("FindResourcesByType"));
            Assert.IsTrue(names.Remove("FetchPluginNameSpaces"));
            Assert.IsTrue(names.Remove("SaveResourceService"));
            Assert.IsTrue(names.Remove("TestRedisSource"));
            Assert.IsTrue(names.Remove("DeleteLogService"));
            Assert.IsTrue(names.Remove("FetchWebServiceSources"));
            Assert.IsTrue(names.Remove("FetchExplorerItemsService"));
            Assert.IsTrue(names.Remove("DeleteTest"));
            Assert.IsTrue(names.Remove("SaveEmailServiceSource"));
            Assert.IsTrue(names.Remove("TestPluginService"));
            Assert.IsTrue(names.Remove("GetComDllListingsService"));
            Assert.IsTrue(names.Remove("SaveServerSourceService"));
            Assert.IsTrue(names.Remove("MoveItemService"));
            Assert.IsTrue(names.Remove("FetchResourceDefinitionService"));
            Assert.IsTrue(names.Remove("TerminateExecutionService"));
            Assert.IsTrue(names.Remove("DeleteVersion"));
            Assert.IsTrue(names.Remove("TestSharepointServerService"));
            Assert.IsTrue(names.Remove("GetFilterListService"));
            Assert.IsTrue(names.Remove("SaveExchangeServiceSource"));
            Assert.IsTrue(names.Remove("GetVersion"));
            Assert.IsTrue(names.Remove("WorkflowResume"));
            Assert.IsTrue(names.Remove("SaveWebserviceSource"));
            Assert.IsTrue(names.Remove("FetchWcfAction"));
            Assert.IsTrue(names.Remove("FetchRabbitMQServiceSources"));
            Assert.IsTrue(names.Remove("GetDatabaseColumnsForTableService"));
            Assert.IsTrue(names.Remove("GetDirectoriesRelativeToServerService"));
            Assert.IsTrue(names.Remove("FetchPluginSources"));
            Assert.IsTrue(names.Remove("ResetPerformanceCounters"));
            Assert.IsTrue(names.Remove("SecurityWriteService"));
            Assert.IsTrue(names.Remove("FetchPluginActionsWithReturnsTypes"));
            Assert.IsTrue(names.Remove("ReloadAllTests"));
            Assert.IsTrue(names.Remove("GetVersions"));
            Assert.IsTrue(names.Remove("Ping"));
            Assert.IsTrue(names.Remove("FetchDbSources"));
            Assert.IsTrue(names.Remove("SavePluginService"));
            Assert.IsTrue(names.Remove("FindAutocompleteOptions"));
            Assert.IsTrue(names.Remove("TestWebserviceSource"));
            Assert.IsTrue(names.Remove("GetDependanciesOnListService"));
            Assert.IsTrue(names.Remove("TestComPluginService"));
            Assert.IsTrue(names.Remove("SaveDbSourceService"));
            Assert.IsTrue(names.Remove("GetDllListingsService"));
            Assert.IsTrue(names.Remove("GetFiles"));
            Assert.IsTrue(names.Remove("SaveRedisSource"));
            Assert.IsTrue(names.Remove("FetchResourceDuplicates"));
            Assert.IsTrue(names.Remove("FindResourcesByID"));
            Assert.IsTrue(names.Remove("GetResourceById"));
            Assert.IsTrue(names.Remove("FetchDebugItemFileService"));
            Assert.IsTrue(names.Remove("GetExecutionHistoryService"));
            Assert.IsTrue(names.Remove("AddFolderService"));
            Assert.IsTrue(names.Remove("FetchServerPermissions"));
            Assert.IsTrue(names.Remove("GetComputerNamesService"));
            Assert.IsTrue(names.Remove("FetchAllTests"));
            Assert.IsTrue(names.Remove("DeleteTriggerQueueService"));
            Assert.IsTrue(names.Remove("ClearLogService"));
            Assert.IsTrue(names.Remove("GetServerInformationalVersion"));
            Assert.IsTrue(names.Remove("TestConnectionService"));
            Assert.IsTrue(names.Remove("RollbackTo"));
            Assert.IsTrue(names.Remove("DirectDeploy"));
            Assert.IsTrue(names.Remove("FetchDropBoxSources"));
            Assert.IsTrue(names.Remove("GetLogDataService"));
            Assert.IsTrue(names.Remove("FetchPluginConstructors"));
            Assert.IsTrue(names.Remove("SaveWcfServiceSource"));
            Assert.IsTrue(names.Remove("GetServerVersion"));
            Assert.IsTrue(names.Remove("DuplicateFolderService"));
            Assert.IsTrue(names.Remove("LoggingSettingsWriteService"));
            Assert.IsTrue(names.Remove("FetchToolsService"));
            Assert.IsTrue(names.Remove("DeleteItemService"));
            Assert.IsTrue(names.Remove("ReloadTestsService"));
            Assert.IsTrue(names.Remove("GetClusterSettings"));
            Assert.IsTrue(names.Remove("SaveClusterSettings"));
            Assert.IsTrue(names.Remove("SaveDbService"));
            Assert.IsTrue(names.Remove("FetchComPluginActions"));
            Assert.IsTrue(names.Remove("TestClusterConnection"));
            Assert.IsTrue(names.Remove("ClusterJoinRequest"));
            Assert.IsTrue(names.Remove("TestClusterLeaderConnection"));
            
            // We expect that all services were found and so were able to be removed
            // And we expect that there are no other services
            Assert.AreEqual(0, names.Count, "unexpected internal services encountered");
        }
    }
}
