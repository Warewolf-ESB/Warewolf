#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
        static string Location => _location ?? (_location = Assembly.GetExecutingAssembly().Location);

        const string MakeCertPath = @"\SSL Generation\CreateCertificate.bat";

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
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
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

        static string AuthorityName() => Guid.NewGuid().ToString();

        public static bool BindSslCertToPorts(IPEndPoint endPoint, string sslCertPath)
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
