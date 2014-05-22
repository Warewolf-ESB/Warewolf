using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nuane.Net;
namespace SFTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            SshKey rsaKey = SshKey.Generate(SshKeyAlgorithm.RSA, 1024);
            SshKey dssKey = SshKey.Generate(SshKeyAlgorithm.DSS, 1024);
            if (File.Exists(@"\FTP.log"))
                File.Create(@"\FTP.log");
            
            TextWriter txt = new StreamWriter(File.OpenWrite(@".\FTP.log"));
            // add keys, bindings and users
            var server = new SftpServer { Log = txt };
            server.Keys.Add(rsaKey);
            server.Keys.Add(dssKey);
            server.Bindings.Add(IPAddress.Any, 22);
            server.Users.Add(new SshUser("dev2", "Q/ulw&]", @"C:\Temp"));
            // start the server                                                    
            server.Start();
            Console.ReadLine();
        }
    }
}
