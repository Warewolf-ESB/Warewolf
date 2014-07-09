namespace Tu.Servers
{
    public interface IFtpServer
    {
        bool Upload(string relativeUri, string data);

        string Download(string relativeUri);

        bool Rename(string fromRelativeUri, string toRelativeUri);

        bool Delete(string relativeUri);
    }
}