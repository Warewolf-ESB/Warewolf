
namespace Dev2.Data.Interfaces
{
    public interface IDev2ConfigurationProvider {

        string ReadKey(string key);

        void OnReadFailure();
    }
}
