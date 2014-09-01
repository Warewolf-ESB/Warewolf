
namespace Dev2.Common.Interfaces.Core.Graph
{
    public interface IPathSegment
    {
        string ActualSegment { get; set; }
        string DisplaySegment { get; set; }
        bool IsEnumarable { get; set; }

        string ToString(bool considerEnumerable);
    }
}
