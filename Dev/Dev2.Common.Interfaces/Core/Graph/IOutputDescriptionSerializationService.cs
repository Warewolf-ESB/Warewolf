
namespace Dev2.Common.Interfaces.Core.Graph
{
    public interface IOutputDescriptionSerializationService
    {
        string Serialize(IOutputDescription outputDescription);
        IOutputDescription Deserialize(string data);
    }
}
