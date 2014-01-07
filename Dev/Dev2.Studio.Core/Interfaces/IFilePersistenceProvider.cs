
namespace Dev2.Studio.Core.Interfaces
{
    public interface IFilePersistenceProvider
    {
        void Write(string containerName, string data);
        void Delete(string containerName);
        string Read(string containerName);
    }
}
