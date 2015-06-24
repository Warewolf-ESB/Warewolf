using Dev2.Interfaces;

namespace Dev2.Common.Interfaces
{
    public interface ISharepointReadListTo : IDev2TOFn
    {
        string FieldName { get; set; }
        string InternalName { get; set; }
        string VariableName { get; set; }
        string Type { get; set; }
        bool IsRequired { get; set; }
    }
}