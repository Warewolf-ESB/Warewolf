namespace Tu.Servers
{
    public interface IFileServer
    {
        void WriteAllText(string fileName, string contents);

        string ReadAllText(string fileName);

        void Delete(string fileName);
    }
}