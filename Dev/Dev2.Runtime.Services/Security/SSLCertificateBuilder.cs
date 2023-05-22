#pragma warning disable
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

        public bool EnsureSslCertificate(string certPath)
        {
            var result = false;
            try
            {
                if (!File.Exists(certPath) && GenerateCert(certPath))
                {
                    result = ImportCert(certPath);
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }

            return result;
        }

        static bool GenerateCert(string sslCertPath)
        {
            var args = string.Format("dev-certs https --export-path \"C:\\Builds\\Warewolf Repo\\Dev\\Dev2.Server\\bin\\Debug\\net6.0-windows\\{0}\" --password 456123", 
                sslCertPath);
            return ProcessHost.Invoke(null, "dotnet", args);
        }

        public static bool ImportCert(string sslCertPath)
        {
            var args = string.Format("dev-certs https --clean --import \"{0}\" --password 456123",
                    sslCertPath);
            return ProcessHost.Invoke(null, "dotnet", args);
        }
    }
}
