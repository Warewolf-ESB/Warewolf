/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

            Assert.AreEqual(129, names.Count);
            Assert.IsTrue(names.ContainsKey("SavePerformanceCounters"));
            Assert.IsTrue(names.ContainsKey("DeleteAllTestsService"));
            Assert.IsTrue(names.ContainsKey("ReloadResourceService"));
            Assert.IsTrue(names.ContainsKey("GetSharepointListService"));
            Assert.IsTrue(names.ContainsKey("FetchDependantCompileMessagesService"));
            Assert.IsTrue(names.ContainsKey("SettingsWriteService"));
            Assert.IsTrue(names.ContainsKey("FindDependencyService"));
            Assert.IsTrue(names.ContainsKey("TestSqliteService"));
            Assert.IsTrue(names.ContainsKey("DeployResourceService"));
            Assert.IsTrue(names.ContainsKey("DeleteResourceService"));
            Assert.IsTrue(names.ContainsKey("FindOptionsBy"));
            Assert.IsTrue(names.ContainsKey("FetchExchangeSources"));
            Assert.IsTrue(names.ContainsKey("TestRabbitMQServiceSource"));
            Assert.IsTrue(names.ContainsKey("FetchCurrentServerLogService"));
            Assert.IsTrue(names.ContainsKey("GetScheduledResourceHistoryService"));
            Assert.IsTrue(names.ContainsKey("FetchComPluginSources"));
            Assert.IsTrue(names.ContainsKey("GetScheduledResources"));
            Assert.IsTrue(names.ContainsKey("SaveRabbitMQServiceSource"));
            Assert.IsTrue(names.ContainsKey("DeleteScheduledResourceService"));
            Assert.IsTrue(names.ContainsKey("TestExchangeServiceSource"));
            Assert.IsTrue(names.ContainsKey("GetServerSettings"));
            Assert.IsTrue(names.ContainsKey("TestDbSourceService"));
            Assert.IsTrue(names.ContainsKey("RenameFolderService"));
            Assert.IsTrue(names.ContainsKey("GetMinSupportedVersion"));
            Assert.IsTrue(names.ContainsKey("SecurityReadService"));
            Assert.IsTrue(names.ContainsKey("FindOptions"));
            Assert.IsTrue(names.ContainsKey("SaveComPluginSource"));
            Assert.IsTrue(names.ContainsKey("GetServerInformation"));
            Assert.IsTrue(names.ContainsKey("RenameItemService"));
            Assert.IsTrue(names.ContainsKey("SaveWebService"));
            Assert.IsTrue(names.ContainsKey("SavePluginSource"));
            Assert.IsTrue(names.ContainsKey("SaveServerSettings"));
            Assert.IsTrue(names.ContainsKey("TestDbService"));
            Assert.IsTrue(names.ContainsKey("TestWcfServiceSource"));
            Assert.IsTrue(names.ContainsKey("SettingsReadService"));
            Assert.IsTrue(names.ContainsKey("FetchComPluginNameSpaces"));
            Assert.IsTrue(names.ContainsKey("FindSourcesByType"));
            Assert.IsTrue(names.ContainsKey("SaveOAuthSource"));
            Assert.IsTrue(names.ContainsKey("SaveTriggerQueueService"));
            Assert.IsTrue(names.ContainsKey("FindResourceService"));
            Assert.IsTrue(names.ContainsKey("SaveSharepointServerService"));
            Assert.IsTrue(names.ContainsKey("FetchPluginActions"));
            Assert.IsTrue(names.ContainsKey("FetchWcfSources"));
            Assert.IsTrue(names.ContainsKey("TestEmailServiceSource"));
            Assert.IsTrue(names.ContainsKey("SaveTests"));
            Assert.IsTrue(names.ContainsKey("DuplicateResourceService"));
            Assert.IsTrue(names.ContainsKey("GetSharepointListFields"));
            Assert.IsTrue(names.ContainsKey("FetchDbActions"));
            Assert.IsTrue(names.ContainsKey("FetchTestsForDeploy"));
            Assert.IsTrue(names.ContainsKey("SaveScheduledResourceService"));
            Assert.IsTrue(names.ContainsKey("FetchTriggerQueues"));
            Assert.IsTrue(names.ContainsKey("FetchRemoteDebugMessagesService"));
            Assert.IsTrue(names.ContainsKey("TestWebService"));
            Assert.IsTrue(names.ContainsKey("GetDatabaseTablesService"));
            Assert.IsTrue(names.ContainsKey("FetchTests"));
            Assert.IsTrue(names.ContainsKey("TestWcfService"));
            Assert.IsTrue(names.ContainsKey("LoggingSettingsReadService"));
            Assert.IsTrue(names.ContainsKey("FetchPerformanceCounters"));
            Assert.IsTrue(names.ContainsKey("FindResourcesByType"));
            Assert.IsTrue(names.ContainsKey("FetchPluginNameSpaces"));
            Assert.IsTrue(names.ContainsKey("SaveResourceService"));
            Assert.IsTrue(names.ContainsKey("TestRedisSource"));
            Assert.IsTrue(names.ContainsKey("DeleteLogService"));
            Assert.IsTrue(names.ContainsKey("FetchWebServiceSources"));
            Assert.IsTrue(names.ContainsKey("FetchExplorerItemsService"));
            Assert.IsTrue(names.ContainsKey("DeleteTest"));
            Assert.IsTrue(names.ContainsKey("SaveEmailServiceSource"));
            Assert.IsTrue(names.ContainsKey("TestPluginService"));
            Assert.IsTrue(names.ContainsKey("GetComDllListingsService"));
            Assert.IsTrue(names.ContainsKey("SaveServerSourceService"));
            Assert.IsTrue(names.ContainsKey("MoveItemService"));
            Assert.IsTrue(names.ContainsKey("FetchResourceDefinitionService"));
            Assert.IsTrue(names.ContainsKey("TerminateExecutionService"));
            Assert.IsTrue(names.ContainsKey("DeleteVersion"));
            Assert.IsTrue(names.ContainsKey("TestSharepointServerService"));
            Assert.IsTrue(names.ContainsKey("GetFilterListService"));
            Assert.IsTrue(names.ContainsKey("SaveExchangeServiceSource"));
            Assert.IsTrue(names.ContainsKey("GetVersion"));
            Assert.IsTrue(names.ContainsKey("WorkflowResume"));
            Assert.IsTrue(names.ContainsKey("SaveWebserviceSource"));
            Assert.IsTrue(names.ContainsKey("FetchWcfAction"));
            Assert.IsTrue(names.ContainsKey("FetchRabbitMQServiceSources"));
            Assert.IsTrue(names.ContainsKey("GetDatabaseColumnsForTableService"));
            Assert.IsTrue(names.ContainsKey("GetDirectoriesRelativeToServerService"));
            Assert.IsTrue(names.ContainsKey("FetchPluginSources"));
            Assert.IsTrue(names.ContainsKey("ResetPerformanceCounters"));
            Assert.IsTrue(names.ContainsKey("SecurityWriteService"));
            Assert.IsTrue(names.ContainsKey("FetchPluginActionsWithReturnsTypes"));
            Assert.IsTrue(names.ContainsKey("ReloadAllTests"));
            Assert.IsTrue(names.ContainsKey("GetVersions"));
            Assert.IsTrue(names.ContainsKey("Ping"));
            Assert.IsTrue(names.ContainsKey("FetchDbSources"));
            Assert.IsTrue(names.ContainsKey("SavePluginService"));
            Assert.IsTrue(names.ContainsKey("FindAutocompleteOptions"));
            Assert.IsTrue(names.ContainsKey("TestWebserviceSource"));
            Assert.IsTrue(names.ContainsKey("GetDependanciesOnListService"));
            Assert.IsTrue(names.ContainsKey("TestComPluginService"));
            Assert.IsTrue(names.ContainsKey("SaveDbSourceService"));
            Assert.IsTrue(names.ContainsKey("GetDllListingsService"));
            Assert.IsTrue(names.ContainsKey("GetFiles"));
            Assert.IsTrue(names.ContainsKey("GetDllListingsService"));
            Assert.IsTrue(names.ContainsKey("GetFiles"));
            Assert.IsTrue(names.ContainsKey("SaveRedisSource"));
            Assert.IsTrue(names.ContainsKey("FetchResourceDuplicates"));
            Assert.IsTrue(names.ContainsKey("FindResourcesByID"));
            Assert.IsTrue(names.ContainsKey("GetResourceById"));
            Assert.IsTrue(names.ContainsKey("FetchDebugItemFileService"));
            Assert.IsTrue(names.ContainsKey("GetExecutionHistoryService"));
            Assert.IsTrue(names.ContainsKey("AddFolderService"));
            Assert.IsTrue(names.ContainsKey("FetchServerPermissions"));
            Assert.IsTrue(names.ContainsKey("GetComputerNamesService"));
            Assert.IsTrue(names.ContainsKey("FetchAllTests"));
            Assert.IsTrue(names.ContainsKey("DeleteTriggerQueueService"));
            Assert.IsTrue(names.ContainsKey("ClearLogService"));
            Assert.IsTrue(names.ContainsKey("GetServerInformationalVersion"));
            Assert.IsTrue(names.ContainsKey("TestConnectionService"));
            Assert.IsTrue(names.ContainsKey("RollbackTo"));
            Assert.IsTrue(names.ContainsKey("DirectDeploy"));
            Assert.IsTrue(names.ContainsKey("FetchDropBoxSources"));
            Assert.IsTrue(names.ContainsKey("GetLogDataService"));
            Assert.IsTrue(names.ContainsKey("FetchPluginConstructors"));
            Assert.IsTrue(names.ContainsKey("SaveWcfServiceSource"));
            Assert.IsTrue(names.ContainsKey("GetServerVersion"));
            Assert.IsTrue(names.ContainsKey("DuplicateFolderService"));
            Assert.IsTrue(names.ContainsKey("LoggingSettingsWriteService"));
            Assert.IsTrue(names.ContainsKey("FetchToolsService"));
            Assert.IsTrue(names.ContainsKey("DeleteItemService"));
            Assert.IsTrue(names.ContainsKey("ReloadTestsService"));
            Assert.IsTrue(names.ContainsKey("GetClusterSettings"));
            Assert.IsTrue(names.ContainsKey("SaveDbService"));
            Assert.IsTrue(names.ContainsKey("FetchComPluginActions"));
        }
    }
}
