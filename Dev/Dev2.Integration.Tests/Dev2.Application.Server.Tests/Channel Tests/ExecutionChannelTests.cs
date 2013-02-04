using System;
using System.IO;
using System.Net;
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

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Channel_Tests
{
    [TestClass]
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
            // Setup MEF context
            //
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            //
            // Setup test data
            //
            bool callbackRecieved = false;

            ErrorResultTO errors = new ErrorResultTO();
            Guid callBackID = Guid.NewGuid();
            string serviceName = "wizardtest";

            ServiceLocator serviceLocator = new ServiceLocator();
            ImportService.SatisfyImports(serviceLocator);

            //
            // Connect to the server
            //
            IEnvironmentConnection conn = new EnvironmentConnection();
            conn.Address = new Uri(ServerSettings.DsfAddress);
            conn.Connect();

            IEventAggregator eventAggregator = ImportService.GetExportValue<IEventAggregator>();
            IFrameworkSecurityContext securityContext = ImportService.GetExportValue<IFrameworkSecurityContext>();
            IEnvironmentConnection environmentConnection = ImportService.GetExportValue<IEnvironmentConnection>();

            IEnvironmentModel environment = new EnvironmentModel(eventAggregator, securityContext, environmentConnection);
            environment.EnvironmentConnection = conn;

            //
            // Add an execution callback, the action of this call back will set the reset event allowing the test to continue
            //
            conn.ExecutionChannel.AddExecutionStatusCallback(callBackID, new Action<ExecutionStatusCallbackMessage>(m =>
            {
                if (m.MessageType == ExecutionStatusCallbackMessageType.CompletedCallback)
                {
                    callbackRecieved = true;
                    resetEvent.Set();
                }
            }));

            //
            // Get endpoint to query and make request to server
            //
            Uri enpoint = serviceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.ServiceWithExecutionCallBackKey, new Tuple<string, IEnvironmentModel, Guid>(serviceName, environment, callBackID));

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

            Assert.IsTrue(callbackRecieved);
        }
    }
}
