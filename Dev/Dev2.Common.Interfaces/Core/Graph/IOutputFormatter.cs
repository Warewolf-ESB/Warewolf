
namespace Dev2.Common.Interfaces.Core.Graph
{
    public interface IOutputFormatter
    {
        IOutputDescription OutputDescription { get; }
        object Format(object data);
    }
}
