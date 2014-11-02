/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Net;

namespace Dev2.Runtime.WebServer
{
    /// <summary>
    ///     TO for moving endpoints around between the lifecycle manager and the http server ;)
    ///     HTTPS : http://webserver.codeplex.com/discussions/430976
    ///     http://webserver.codeplex.com/wikipage?title=HTTPS
    ///     HOWTO GENERATE CERT :
    ///     0) Start VS Cmd Prompt to issue instructions
    ///     1) Make Root Authority Cert
    ///     makecert -pe -n "CN=Test And Dev Root Authority" -ss my -sr LocalMachine -a sha1 -sky signature -r "Test and Dev
    ///     Root Authority.cer"
    ///     2) Generate SSL Cert
    ///     makecert -pe -n "CN=warewolf.yourdomainname" -ss my -sr LocalMachine -a sha1 -sky exchange -eku 1.3.6.1.5.5.7.3.1
    ///     -in "Test and Dev Root Authority" -is MY -ir LocalMachine -sp "Microsoft RSA SChannel Cryptographic Provider" -sy
    ///     12 WarewolfServer.cer
    /// </summary>
    public class Dev2Endpoint
    {
        public Dev2Endpoint(IPEndPoint endPoint, string url) : this(endPoint, url, null)
        {
        }

        public Dev2Endpoint(IPEndPoint endPoint, string url, string certPath)
        {
            TheIPEndPoint = endPoint;
            Url = url;
            CertificatePath = certPath;
        }

        public IPEndPoint TheIPEndPoint { get; private set; }
        public string CertificatePath { get; private set; }
        public string Url { get; private set; }

        public bool IsSecured
        {
            get { return !string.IsNullOrEmpty(CertificatePath); }
        }

        public IPAddress Address
        {
            get
            {
                if (TheIPEndPoint != null)
                {
                    return TheIPEndPoint.Address;
                }

                return null;
            }
        }

        public int Port
        {
            get
            {
                if (TheIPEndPoint != null)
                {
                    return TheIPEndPoint.Port;
                }

                return -1;
            }
        }
    }
}