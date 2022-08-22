using System;
using System.Net;
using System.Security;

namespace Warewolf.Sharepoint
{
    internal class SharePointOnlineCredentials : ICredentials
    {
        private string userName;
        private SecureString secureString;

        public SharePointOnlineCredentials(string userName, SecureString secureString)
        {
            this.userName = userName;
            this.secureString = secureString;
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            throw new NotImplementedException();
        }
    }
}