
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Configuration;
using System.IO;
using System.Text;
using Dev2.Common.Common;
using Dev2.Runtime.Security;

namespace Dev2.Runtime.SignerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SignFiles(
                ConfigurationManager.AppSettings["Inbox"],
                ConfigurationManager.AppSettings["Outbox"]);
        }

        static void SignFiles(string srcPath, string dstPath)
        {
            var files = Directory.GetFiles(srcPath);
            foreach(var srcFile in files)
            {
                var dstFile = Path.Combine(dstPath, Path.GetFileName(srcFile));
                SignFile(srcFile, dstFile);
            }
        }

        #region SignFile

        static void SignFile(string srcPath, string dstPath)
        {
            var xml = new StringBuilder(File.ReadAllText(srcPath));
            var signedXml = HostSecurityProvider.Instance.SignXml(xml);

            signedXml.WriteToFile(dstPath,Encoding.UTF8);

            //ValidateFile(signedXml);
        }

        #endregion

        
    }


    #region HostSecurityProviderImpl class

    public class HostSecurityProviderImpl : HostSecurityProvider
    {
        public HostSecurityProviderImpl(ISecureConfig config)
            : base(config)
        {

        }

    }

    #endregion

}
