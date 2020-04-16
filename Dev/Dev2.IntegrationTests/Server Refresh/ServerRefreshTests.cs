using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces.Security;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Server_Refresh
{
    [TestClass]
    public class ServerRefreshTests
    {
        const string PassResult = @"C:\ProgramData\Warewolf\Resources\PassResult.bite";
        const string PassResultOld = @"C:\ProgramData\Warewolf\Resources\PassResult.xml";

        [TestCleanup]
        public void Cleanup()
        {
            TryUndoMoveFileTemporarily(PassResult);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]

        public void Run_a_workflow_to_test_server_refresh()
        {
            Assert.IsTrue(File.Exists(PassResult));
            Assert.IsFalse(File.Exists(PassResultOld));

            SetupPermissions();
            ExecuteRequest(new Uri("http://localhost:3142/services/FetchExplorerItemsService.json?ReloadResourceCatalogue=true"));

            var url1 = $"http://localhost:3142/secure/RefreshWorkflow1.json";
            var passRequest1 = ExecuteRequest(new Uri(url1));
            //Delete this workflow and continue making requests to it
            MoveFileTemporarily(PassResult);
            // Execute workflow from the resource cache
            var passRequest2 = ExecuteRequest(new Uri(url1));
            var passRequest3 = ExecuteRequest(new Uri(url1));
            var passRequest4 = ExecuteRequest(new Uri(url1));

            //refresh the server and wait fot it to finish
            ExecuteRequest(new Uri("http://localhost:3142/services/FetchExplorerItemsService.json?ReloadResourceCatalogue=true"));
            //execute this workflow after the refresh, we should get failures based on the fact that the refresh has finish executing
            var failRequest1 = ExecuteRequest(new Uri(url1));
            var failRequest2 = ExecuteRequest(new Uri(url1));
            var failRequest3 = ExecuteRequest(new Uri(url1));
            StringAssert.Contains(failRequest1, "Resource RefreshTest not found");
            StringAssert.Contains(failRequest2, "Resource RefreshTest not found");
            StringAssert.Contains(failRequest3, "Resource RefreshTest not found");
            StringAssert.Contains(passRequest1, "Pass");
            StringAssert.Contains(passRequest2, "Pass");
            StringAssert.Contains(passRequest3, "Pass");
            StringAssert.Contains(passRequest4, "Pass");
            ExecuteRequest(new Uri("http://localhost:3142/services/FetchExplorerItemsService.json?ReloadResourceCatalogue=true"));
        }

        class PatientWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                var w = base.GetWebRequest(uri);
                w.Timeout = 20 * 60 * 1000;
                return w;
            }
        }

        void MoveFileTemporarily(string fileName)
        {
            File.Move(fileName, $"{fileName}.Moved");
        }
        void TryUndoMoveFileTemporarily(string fileName)
        {
            var tmpName = $"{fileName}.Moved";
            if (File.Exists(tmpName))
            {
                File.Move(tmpName, fileName);
            }
        }

        string ExecuteRequest(Uri url)
        {
            Task<string> failRequest;
            var client = new PatientWebClient { Credentials = CredentialCache.DefaultNetworkCredentials };
            using (client)
            {
                failRequest = Task.Run(() => client.DownloadString(url));
            }
            string failRequestResult;
            try
            {
                failRequestResult = failRequest.Result;
            }
            catch (AggregateException e)
            {
                return new StreamReader((e.InnerExceptions[0] as WebException)?.Response.GetResponseStream()).ReadToEnd();
            }

            return failRequestResult;
        }

        static void SetupPermissions()
        {
            var groupRights = "View, Execute, Contribute, Deploy To, Deploy From, Administrator";
            var groupPermssions = new WindowsGroupPermission
            {
                WindowsGroup = "Public",
                ResourceID = Guid.Empty,
                IsServer = true
            };
            var permissionsStrings = groupRights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                Permissions permission;
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    groupPermssions.Permissions |= permission;
                }
            }
            var settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission> { groupPermssions })
            };
            AppUsageStats.LocalHost = "http://localhost:3142";
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
        }
    }
}
