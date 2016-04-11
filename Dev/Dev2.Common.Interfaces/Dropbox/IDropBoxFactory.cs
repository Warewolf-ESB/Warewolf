using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;

namespace Dev2.Common.Interfaces.Dropbox
{
    public interface IDropboxFactory
    {
        DropboxClient Create();
        DropboxClient CreateWithSecret(string secret);
    }

}
