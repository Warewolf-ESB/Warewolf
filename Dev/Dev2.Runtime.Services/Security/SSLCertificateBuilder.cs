
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
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Dev2.Common;

namespace Dev2.Runtime.Security
{
    /// <summary>
    /// Build a self-signed SSL cert
    /// </summary>
    public class SslCertificateBuilder
    {
        static string _location;
        static string Location { get { return _location ?? (_location = Assembly.GetExecutingAssembly().Location); } }

        private const string MakeCertPath = @"\SSL Generation\CreateCertificate.bat";

        public bool EnsureSslCertificate(string certPath, IPEndPoint endPoint)
        {
            var result = false;
            var asmLoc = Location;
            var exeBase = string.Empty;
            var authName = AuthorityName();
            var masterData = string.Empty;
            var workingDir = string.Empty;

            try
            {
                if(!string.IsNullOrEmpty(asmLoc))
                {
                    asmLoc = Path.GetDirectoryName(asmLoc);
                    workingDir = String.Concat(asmLoc, @"\SSL Generation");
                    exeBase = string.Concat(asmLoc, MakeCertPath);
                    masterData = File.ReadAllText(exeBase);
                    var writeBack = string.Format(masterData, authName);

                    File.WriteAllText(exeBase, writeBack);
                }

                if(ProcessHost.Invoke(workingDir, "CreateCertificate.bat", null))
                {
                    result = BindSslCertToPorts(endPoint, certPath);
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
            }
            finally
            {
                if(!string.IsNullOrEmpty(masterData))
                {
                    File.WriteAllText(exeBase, masterData);
                }
            }

            return result;
        }

        static string AuthorityName()
        {
            return Guid.NewGuid().ToString();
        }

        static bool BindSslCertToPorts(IPEndPoint endPoint, string sslCertPath)
        {
            //
            // To verify run this at the command prompt:
            //
            // netsh http show sslcert ipport=0.0.0.0:1236
            //
            var cert = new X509Certificate(sslCertPath);
            var certHash = cert.GetCertHashString();
            var args = string.Format("http add sslcert ipport={0}:{1} appid={{12345678-db90-4b66-8b01-88f7af2e36bf}} certhash={2}",
                    endPoint.Address, endPoint.Port, certHash);
            return ProcessHost.Invoke(null, "netsh.exe", args);
        }

    }
}
