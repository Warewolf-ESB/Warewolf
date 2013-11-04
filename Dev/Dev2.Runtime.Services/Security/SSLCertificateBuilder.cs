using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Dev2.Common;

namespace Dev2.Runtime.Security
{
   
    /// <summary>
    /// Build a self-signed SSL cert
    /// </summary>
    public class SSLCertificateBuilder
    {

        // NOTE : We need to ensure we change the -r value for each generation ;)
        private static string MakeCertPath = @"\SSL Generation\CreateCertificate.bat";

        public bool EnsureSSLCertificate(string certPath)
        {
            bool result = false;
            var asmLoc = Assembly.GetExecutingAssembly().Location;
            var exeBase = string.Empty;
            var authName = AuthorityName();
            var masterData = string.Empty;
            var workingDir = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(asmLoc))
                {
                    asmLoc = Path.GetDirectoryName(asmLoc);
                    workingDir = String.Concat(asmLoc, @"\SSL Generation");
                    exeBase = string.Concat(asmLoc, MakeCertPath);
                    masterData = File.ReadAllText(exeBase);
                    var writeBack = string.Format(masterData, authName);

                    File.WriteAllText(exeBase, writeBack);
                }

                if(InvokeProcess("CreateCertificate.bat", workingDir))
                {
                    result = true;
                    
                }
            }
            catch (Exception e)
            {
                ServerLogger.LogError(e);
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

        private string AuthorityName()
        {
            return Guid.NewGuid().ToString();
        }

        private bool InvokeProcess(string cmd, string workingDir)
        {
            bool invoked = true;

            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = cmd;
            p.StartInfo.WorkingDirectory = workingDir;
            
            var proc = p.Start();
            
            if (p.Start())
            {
                // wait up to 10 seconds for exit ;)
                p.WaitForExit(10000); 
                
            }
            else
            {
                invoked = false;
            }

            return invoked;
        }
    }
}
