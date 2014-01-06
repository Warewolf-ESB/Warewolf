
namespace Unlimited.Framework.Converters.Graph.Interfaces
{
    public interface IOutputDescriptionSerializationService
    {
        string Serialize(IOutputDescription outputDescription);
        IOutputDescription Deserialize(string data);
    }
}
