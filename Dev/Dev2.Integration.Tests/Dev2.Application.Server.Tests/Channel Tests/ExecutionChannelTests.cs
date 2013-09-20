using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Threading;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.Integration.Tests.MEF;
using Dev2.Network.Execution;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Wizards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Channel_Tests
{
    [TestClass][Ignore]//Ashley: round 2 hunting the evil test
    public class ExecutionChannelTests
    {

        [TestMethod]
        public void WorkflowExecuted_Where_ExecutionStatusCallBackRegistered_Expected_OnCompletedCallbackRecieved()
        {
            //
            // Create a reset event which is used to wait for callbacks form the server
            //
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            //
            // Setup test data
            //
            bool callbackRecieved = false;

            ErrorResultTO errors = new ErrorResultTO();
            Guid callBackID = Guid.NewGuid();
            string serviceName = "wizardtest";

            ServiceLocator serviceLocator = new ServiceLocator();
            serviceLocator.EndpointGenerationStrategyProviders = new List<IServiceEndpointGenerationStrategyProvider>();
            serviceLocator.EndpointGenerationStrategyProviders.Add(new WizardEndpointGenerationStrategyProvider());
            serviceLocator.Initialize();
            
            //
            // Connect to the server
            //
            var conn = CreateConnection();
            conn.Connect();

            var repo = new Mock<IResourceRepository>();
            IEnvironmentModel environment = new EnvironmentModel(Guid.NewGuid(), conn, repo.Object, false);

            //
            // Add an execution callback, the action of this call back will set the reset event allowing the test to continue
            //
            conn.ExecutionChannel.AddExecutionStatusCallback(callBackID, m =>
            {
                if (m.MessageType == ExecutionStatusCallbackMessageType.CompletedCallback)
                {
                    callbackRecieved = true;
                    resetEvent.Set();
                }
            });

            //
            // Get endpoint to query and make request to server
            //
            Uri enpoint = serviceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.ServiceWithExecutionCallBackKey, new Tuple<string, IEnvironmentModel, Guid>(serviceName, environment, callBackID));

            conn.ExecuteCommand("<x></x>", Guid.Empty, Guid.Empty);

            WebRequest wr = WebRequest.Create(enpoint);
            WebResponse wrsp = wr.GetResponse();
            Stream s = wrsp.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            string payload = sr.ReadToEnd();

            //
            // Wait for completed callback
            //
            resetEvent.Wait(10000);

            //
            // Clean up
            //
            conn.Disconnect();

            Assert.IsTrue(callbackRecieved, "Error - No callback was received.");
        }


        #region CreateConnection

        static TcpConnection CreateConnection(bool isAuxiliary = false)
        {
            return CreateConnection(ServerSettings.DsfAddress, isAuxiliary);
        }

        static TcpConnection CreateConnection(string appServerUri, bool isAuxiliary = false)
        {
            var securityContetxt = new Mock<IFrameworkSecurityContext>();
            securityContetxt.Setup(c => c.UserIdentity).Returns(WindowsIdentity.GetCurrent());

            return new TcpConnection(securityContetxt.Object, new Uri(appServerUri), Int32.Parse(ServerSettings.WebserverPort), isAuxiliary);
        }

        #endregion
    }
}
