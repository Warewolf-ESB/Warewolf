using Dev2.PathOperations;
using Nuane.Net;
using System;
using System.Net;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.BaseTypes
{
    [Binding]
    public class FileToolsBase : RecordSetBases
    {
        public static SftpServer _server;
        public static readonly object ServerLock = new object();

        #region Overrides of RecordSetBases

        protected override void BuildDataList()
        {
        }

        /// <summary>
        /// Starts the SFTP server.
        /// </summary>
        protected static void StartSftpServer()
        {
            lock(ServerLock)
            {
                if(_server == null)
                {
                    SshKey rsaKey = SshKey.Generate(SshKeyAlgorithm.RSA, 1024);
                    SshKey dssKey = SshKey.Generate(SshKeyAlgorithm.DSS, 1024);

                    // add keys, bindings and users
                    _server = new SftpServer { Log = Console.Out };
                    _server.Keys.Add(rsaKey);
                    _server.Keys.Add(dssKey);
                    _server.Bindings.Add(IPAddress.Any, 22);
                    _server.Users.Add(new SshUser("dev2", "Q/ulw&]", @"C:\Temp"));
                    // start the server                                                    
                    _server.Start();
                }
            }
        }

        protected static void RemovedFilesCreatedForTesting()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            string destLocation;
            if(ScenarioContext.Current.TryGetValue(CommonSteps.ActualDestinationHolder, out destLocation))
            {
                IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(destLocation,
                    ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder),
                    ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder),
                    true);
                IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                broker.Delete(dstEndPoint);
            }

            IActivityIOPath source = ActivityIOFactory.CreatePathFromString(ScenarioContext.Current.Get<string>(CommonSteps.ActualSourceHolder),
                ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                true);
            IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);
            broker.Delete(sourceEndPoint);

            if(_server != null)
            {
                _server.Bindings.Clear();
                _server.Stop();
            }
            _server = null;
        }
        #endregion
    }
}