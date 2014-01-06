
namespace Unlimited.Framework.Converters.Graph.Interfaces
{
    public interface IPathSegment
    {
        string ActualSegment { get; set; }
        string DisplaySegment { get; set; }
        bool IsEnumarable { get; set; }

        string ToString(bool considerEnumerable);
    }
}
