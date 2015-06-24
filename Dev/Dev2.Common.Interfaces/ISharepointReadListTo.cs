using Dev2.Interfaces;

namespace Dev2.Common.Interfaces
{
    public interface ISharepointReadListTo : IDev2TOFn
    {
        string FieldName { get; set; }
        string VariableName { get; set; }
    }
}