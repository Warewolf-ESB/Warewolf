
namespace Unlimited.Framework.Converters.Graph.Interfaces
{
    public interface IOutputFormatter
    {
        IOutputDescription OutputDescription { get; }
        object Format(object data);
    }
}
