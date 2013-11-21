using System.Net;

namespace Dev2.Runtime.WebServer
{
    /// <summary>
    /// TO for moving endpoints around between the lifecycle manager and the http server ;)
    /// 
    /// HTTPS : http://webserver.codeplex.com/discussions/430976
    /// http://webserver.codeplex.com/wikipage?title=HTTPS
    /// 
    /// HOWTO GENERATE CERT : 
    /// 
    /// 0) Start VS Cmd Prompt to issue instructions
    /// 
    /// 1) Make Root Authority Cert
    /// makecert -pe -n "CN=Test And Dev Root Authority" -ss my -sr LocalMachine -a sha1 -sky signature -r "Test and Dev Root Authority.cer" 
    /// 
    /// 2) Generate SSL Cert
    /// makecert -pe -n "CN=warewolf.yourdomainname" -ss my -sr LocalMachine -a sha1 -sky exchange -eku 1.3.6.1.5.5.7.3.1 -in "Test and Dev Root Authority" -is MY -ir LocalMachine -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12 WarewolfServer.cer 
    /// 
    /// </summary>
    public class Dev2Endpoint
    {
        public IPEndPoint TheIPEndPoint { get; private set; }
        public string CertificatePath { get; private set; }

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

        public Dev2Endpoint(IPEndPoint endPoint) : this(endPoint, null)
        {
        }

        public Dev2Endpoint(IPEndPoint endPoint, string certPath)
        {
            TheIPEndPoint = endPoint;
            CertificatePath = certPath;
        }
    }
}
