
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.IO;
using System.Net;
using System.Threading;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Integration.Tests.MEF;
using Dev2.Network.Execution;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Wizards;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Wizard_tests
{
    [TestClass]
    public class WizardModeTests
    {
        [TestMethod]
        public void WorkflowExecuted_Where_ExecutionStatusCallBackRegisteredAndDataListMergeRequested_Expected_DataMergedInAndOutOfWorkflow()
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
            ErrorResultTO errors = new ErrorResultTO();
            Guid callBackID = Guid.NewGuid();
            string serviceName = "wizardtest";

            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            dataList.TryCreateScalarTemplate("", "testvalue", "", true, out error);
            dataList.TryCreateScalarValue("321", "testvalue", out error);

            DataListMergeOpsTO dataListMergeOpsTO = new DataListMergeOpsTO(dataList.UID);

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
            // Write the datalist to the server over the datalist channel
            //
            conn.DataListChannel.WriteDataList(dataList.UID, dataList, errors);

            //
            // Add an execution callback, the action of this call back will set the reset event allowing the test to continue
            //
            conn.ExecutionChannel.AddExecutionStatusCallback(callBackID, new Action<ExecutionStatusCallbackMessage>(m =>
            {
                if (m.MessageType == ExecutionStatusCallbackMessageType.CompletedCallback)
                {
                    resetEvent.Set();
                }
            }));

            //
            // Get endpoint to query and make request to server
            //
            Uri enpoint = serviceLocator.GetEndpoint(WizardEndpointGenerationStrategyProvider.ServiceWithDataListMergeAndExecutionCallBackKey, new Tuple<string, IEnvironmentModel, DataListMergeOpsTO, Guid>(serviceName, environment, dataListMergeOpsTO, callBackID));

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
            // Get datalist from the server
            //
            IBinaryDataList resultDataList = conn.DataListChannel.ReadDatalist(dataList.UID, errors);

            //
            // Clean up
            //
            conn.Disconnect();

            IBinaryDataListEntry testValueEntry;
            resultDataList.TryGetEntry("testvalue", out testValueEntry, out error);

            IBinaryDataListEntry inValueEntry;
            resultDataList.TryGetEntry("invalue", out inValueEntry, out error);

            string expected = "123|321";
            string actual = testValueEntry.FetchScalar().TheValue + "|" + inValueEntry.FetchScalar().TheValue;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WorkflowExecuted_Where_ExecutionStatusCallBackRegisteredAndDataListMergeNotRequested_Expected_DataListContainsRemainsEmpty()
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
            ErrorResultTO errors = new ErrorResultTO();
            Guid callBackID = Guid.NewGuid();
            string serviceName = "wizardtest";

            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();

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
            // Write the datalist to the server over the datalist channel
            //
            conn.DataListChannel.WriteDataList(dataList.UID, dataList, errors);

            //
            // Add an execution callback, the action of this call back will set the reset event allowing the test to continue
            //
            conn.ExecutionChannel.AddExecutionStatusCallback(callBackID, new Action<ExecutionStatusCallbackMessage>(m =>
            {
                if (m.MessageType == ExecutionStatusCallbackMessageType.CompletedCallback)
                {
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
            // Get datalist from the server
            //
            IBinaryDataList resultDataList = conn.DataListChannel.ReadDatalist(dataList.UID, errors);

            //
            // Clean up
            //
            conn.Disconnect();

            bool expected = false;

            string error;
            IBinaryDataListEntry entry;
            bool actual = resultDataList.TryGetEntry("testvalue", out entry, out error);

            Assert.AreEqual(expected, actual);
        }
    }
}
