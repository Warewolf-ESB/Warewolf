/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Net;
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
            rsaKey.Save("C:\\Temp\\private.key","Q/ulw&]");
            TextWriter txt = new StreamWriter(File.OpenWrite(@".\FTP.log"));
            // add keys, bindings and users
            var server = new SftpServer { Log = Console.Out };
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
