using System;
using System.IO;
using System.Net;
using System.ServiceProcess;
using Nuane.Net;
// ReSharper disable NonLocalizedString

namespace SFTPServerService
{
    public partial class SftpServerService : ServiceBase
    {
        private SftpServer _server;
        const string PrivatePublicKeyFile = @"D:\\Temp\\key.opk";
        public SftpServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartSftpServer();
        }

        private void StartSftpServer()
        {
            SshKey rsaKey = SshKey.Generate(SshKeyAlgorithm.RSA, 1024);
            SshKey dssKey = SshKey.Generate(SshKeyAlgorithm.DSS, 1024);
            string privateKeyData = "-----BEGIN RSA PRIVATE KEY-----" +
                                    Environment.NewLine +
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
            if(!Directory.Exists("D:\\Temp"))
            {
                Directory.CreateDirectory("D:\\Temp");
            }
            File.WriteAllText(PrivatePublicKeyFile, privateKeyData);
            // add keys, bindings and users
            _server = new SftpServer();
            _server.Keys.Add(rsaKey);
            _server.Keys.Add(dssKey);
            _server.Bindings.Add(IPAddress.Any, 22);
            if(Directory.Exists(@"D:\Temp\SFTP"))
            {
                Directory.Delete(@"D:\Temp\SFTP", true);
            }
            Directory.CreateDirectory(@"D:\Temp\SFTP");
            _server.Users.Add(new SshUser("dev2", "Q/ulw&]", @"D:\Temp\SFTP"));

            // start the server                                                    
            _server.Start();
        }

        protected override void OnStop()
        {
            ShutdownSftpServer();
        }

        private void ShutdownSftpServer()
        {
            try
            {
                if (_server != null)
                {
                    _server.Bindings.Clear();
                    _server.Stop();

                }
            }
            catch
            {
                //Server may already be stopped
            }

            _server = null;
        }
    }
}
