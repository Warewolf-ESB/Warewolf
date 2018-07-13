
namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IZipFile
    {
        void CreateFromDirectory(string sourceDirectory, string destinationZippedDirectory);
    }
}