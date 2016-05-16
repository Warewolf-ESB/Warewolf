using Dropbox.Api;

namespace Dev2.Common.Interfaces.Dropbox
{
    public interface IDropboxFactory
    {
        DropboxClient Create();
        DropboxClient CreateWithSecret(string secret);
    }

}
