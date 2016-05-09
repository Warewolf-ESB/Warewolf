
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Net;
using Dev2.Common;
using Dev2.Data.PathOperations.Enums;
using Dev2.PathOperations;
using Nuane.Net;
using TechTalk.SpecFlow;

namespace Warewolf.Tools.Specs.BaseTypes
{
    [Binding]
    public class FileToolsBase : RecordSetBases
    {
        public const string PrivatePublicKeyFile = "C:\\Temp\\key.opk";
        
        public static SftpServer Server;
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
            Dev2Logger.Debug("ServerStartup");
            lock (ServerLock)
            {
                if (Server == null)
                {
                    SshKey rsaKey = SshKey.Generate(SshKeyAlgorithm.RSA, 1024);
                    SshKey dssKey = SshKey.Generate(SshKeyAlgorithm.DSS, 1024);
                    string privateKeyData = "-----BEGIN RSA PRIVATE KEY-----" +
                                                    Environment.NewLine+
                                              "Proc-Type: 4,ENCRYPTED" +
                                              Environment.NewLine +
                                              "DEK-Info: DES-EDE3-CBC,5CDA5D2764661954" +
                                              Environment.NewLine +
                                              Environment.NewLine +
                                              "2UsQKxjSeLNXEs3FoK4fO8u5e2RoxVKAjEpp5K5xreRFoTysuMI5R6fwiyrNpJIx" +
                                              Environment.NewLine +
                                              "zm+6WamQSRmqJBZ23VKB/NOpKrTKLosgLz9LlPxiGUEDGClmh0rJgJHH+GVB+Tbu" +
                                              Environment.NewLine +
                                              "4EW1J4rGbV2MMFzU5gpTEv4VrmmVfEoj8qZcQ91fiPdDS5Qe/Aqudq8/QoSx9Dr5" +
                                              Environment.NewLine +
                                              "oG2aZ5Q/mQ0cZ5le2Mnyp+Uzg9MHFSHIciGWGGhKQcB9YFNddqsFj8lXslXZ2S5d" +
                                              Environment.NewLine +
                                              "GLD8RuWSwVw9TVy/vKt240wS4q/s+Af8w7xZ3ngbA8ibo6zyAC/GNZhTfwOzKwPW" +
                                              Environment.NewLine +
                                              "USQR1HkC6GbMOxH2TaAUrpmFl/AZKrC9ot7S/uMhbmTU8b+TwjT7nIWqz02cM3Co" +
                                              Environment.NewLine +
                                              "KV24aj83VV5iA3rfwRJbBIu2CuZuEZoWyDs/NFURAd+T01zI8YENAPSLaVdxGMcZ" +
                                              Environment.NewLine +
                                              "7AeklCphHkcI8ifVNYpUfRMmpjOnVsWs3lnGUwYvIoEo7VrbYuWGgD29ewIDrV2Y" +
                                              Environment.NewLine +
                                              "A9a0K11v76JR60HBBZmCdwSKlJGjMU35vve/k4FC/GqMbS5C1YqRrNvwFbNTRd3x" +
                                              Environment.NewLine +
                                              "rzP3ZEGdoLA9sKPNZAx0WEbYQ30XXv1ZWE5U3/tDV3HvBe8/aO8leUfR9G1fyHIA" +
                                              Environment.NewLine +
                                              "foD2Q/7kSasqqGbQYWCRAVsm4sgA8+pXF7D9gllO37hk9tgwtCvj+Gcdeto0EVF3" +
                                              Environment.NewLine +
                                              "j+rgY6YDQIssNSBbQ3ZptOKCppY1a1wUgdE70IMQ7bbtNegr/RDnQcfnm6lVfWCV" +
                                              Environment.NewLine +
                                              "G5KhlDKAFw3/dNPhsITyaOOzCRN12Xzyfkmub1RSAVOlTcb0jt10CQ==" +
                                               Environment.NewLine +
                                              "-----END RSA PRIVATE KEY-----";
                    if(File.Exists(PrivatePublicKeyFile))
                    {
                        File.Delete(PrivatePublicKeyFile);
                    }
                    if (!Directory.Exists("C:\\Temp"))
                    {
                        Directory.CreateDirectory("C:\\Temp");
                    }
                    File.WriteAllText(PrivatePublicKeyFile,privateKeyData);
                    // add keys, bindings and users
                    Server = new SftpServer
                    {
                        Log = Console.Out
                    };
                    Server.Keys.Add(rsaKey);
                    Server.Keys.Add(dssKey);
                    Server.Bindings.Add(IPAddress.Any, 22);
                    if(Directory.Exists(@"C:\Temp\SFTP"))
                    {
                        Directory.Delete(@"C:\Temp\SFTP", true);
                    }
                    Directory.CreateDirectory(@"C:\Temp\SFTP");
                    Server.Users.Add(new SshUser("dev2", "Q/ulw&]", @"C:\Temp\SFTP"));
                    
                    // start the server                                                    
                    Server.Start();
                }
            }
        }

        protected static void RemovedFilesCreatedForTesting()
        {
            // ReSharper disable EmptyGeneralCatchClause


            Dev2Logger.Debug("Cleanup");

            var broker = ActivityIOFactory.CreateOperationsBroker();
            string destLocation;
            if (ScenarioContext.Current != null && ScenarioContext.Current.TryGetValue(CommonSteps.ActualDestinationHolder, out destLocation))
            {
                IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(destLocation,
                    ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder),
                    ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder),
                    true);
                IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                try
                {
                    if (dstEndPoint.PathIs(dstEndPoint.IOPath) == enPathType.File)
                    {
                        broker.Delete(dstEndPoint);
                    }
                }
                catch (Exception)
                {
                    Dev2Logger.Debug("Cleanup Error");
                //    throw;
                }
            }

            string sourceLocation;
            if (ScenarioContext.Current != null && ScenarioContext.Current.TryGetValue(CommonSteps.ActualSourceHolder, out sourceLocation))
            {
                IActivityIOPath source = ActivityIOFactory.CreatePathFromString(sourceLocation,
                    ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                    ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                    true);
                IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);
                try
                {
                    if (sourceEndPoint.PathIs(sourceEndPoint.IOPath) == enPathType.File)
                    {
                        broker.Delete(sourceEndPoint);
                    }
                }
                catch (Exception)
                {
                    Dev2Logger.Debug("Cleanup Error");
                    //The file may already be deleted
                }
            }

        }

        protected static void ShutdownSftpServer()
        {
            try
            {
                if (Server != null)
                {
                    Dev2Logger.Debug("Server Shutdown");
                    Server.Bindings.Clear();
                    Server.Stop();

                }
            }
            catch
            {
                //Server may already be stopped
            }

            Server = null;
        }
        #endregion
    }
}
