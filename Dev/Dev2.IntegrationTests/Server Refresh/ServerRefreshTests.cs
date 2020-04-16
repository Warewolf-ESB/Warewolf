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
            var explorerRefresh = ExecuteRequest(new Uri("http://localhost:3142/services/FetchExplorerItemsService.json?ReloadResourceCatalogue=true"));
            explorerRefresh.Wait();

            var url1 = $"http://localhost:3142/secure/RefreshWorkflow1.json";
            var passRequest1 = ExecuteRequest(new Uri(url1));
            passRequest1.Wait();
            //Delete this workflow and continue making requests to it
            MoveFileTemporarily(PassResult);
            // Execute workflow from the resource cache
            var passRequest2 = ExecuteRequest(new Uri(url1));
            var passRequest3 = ExecuteRequest(new Uri(url1));
            var passRequest4 = ExecuteRequest(new Uri(url1));

            Task.WaitAll(passRequest2, passRequest3, passRequest4);

            //refresh the server and wait fot it to finish
            explorerRefresh = ExecuteRequest(new Uri("http://localhost:3142/services/FetchExplorerItemsService.json?ReloadResourceCatalogue=true"));
            explorerRefresh.Wait();
            //execute this workflow after the refresh, we should get failures based on the fact that the refresh has finish executing
            var failRequest1 = ExecuteRequest(new Uri(url1));
            var failRequest2 = ExecuteRequest(new Uri(url1));
            var failRequest3 = ExecuteRequest(new Uri(url1));
            string failRequest1Result;
            string failRequest2Result;
            string failRequest3Result;
            try
            {
                failRequest1Result = failRequest1.Result;
            }
            catch (AggregateException e)
            {
                failRequest1Result = new StreamReader((e.InnerExceptions[0] as WebException)?.Response.GetResponseStream()).ReadToEnd();
            }
            try
            {
                failRequest2Result = failRequest2.Result;
            }
            catch (AggregateException e)
            {
                failRequest2Result = new StreamReader((e.InnerExceptions[0] as WebException)?.Response.GetResponseStream()).ReadToEnd();
            }
            try
            {
                failRequest3Result = failRequest3.Result;
            }
            catch (AggregateException e)
            {
                failRequest3Result =new StreamReader((e.InnerExceptions[0] as WebException)?.Response.GetResponseStream()).ReadToEnd();
            }
            var passRequest1Result = passRequest1.Result;
            var passRequest2Result = passRequest2.Result;
            var passRequest3Result = passRequest3.Result;
            var passRequest4Result = passRequest4.Result;
            StringAssert.Contains(failRequest1Result, "Resource RefreshTest not found");
            StringAssert.Contains(failRequest2Result, "Resource RefreshTest not found");
            StringAssert.Contains(failRequest3Result, "Resource RefreshTest not found");
            StringAssert.Contains(passRequest1Result, "Pass");
            StringAssert.Contains(passRequest2Result, "Pass");
            StringAssert.Contains(passRequest3Result, "Pass");
            StringAssert.Contains(passRequest4Result, "Pass");
            explorerRefresh = ExecuteRequest(new Uri("http://localhost:3142/services/FetchExplorerItemsService.json?ReloadResourceCatalogue=true"));
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

        public Task<string> ExecuteRequest(Uri url)
        {
            var client = new PatientWebClient { Credentials = CredentialCache.DefaultNetworkCredentials };
            using (client)
            {
                return Task.Run(() => client.DownloadString(url));
            }
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
