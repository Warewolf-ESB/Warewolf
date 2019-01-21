using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Common.Interfaces.Dropbox
{
    public interface IDropboxFactory
    {
        IDropboxClientWrapper CreateWithSecret(string secret);
    }
}
