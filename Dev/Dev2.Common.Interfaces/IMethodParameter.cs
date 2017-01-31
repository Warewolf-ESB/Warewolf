namespace Dev2.Common.Interfaces
{
    public interface IMethodParameter
    {
        string Name { get; set; }
        bool EmptyToNull { get; set; }
        bool IsRequired { get; set; }
        string Value { get; set; }
        string DefaultValue { get; set; }
        string TypeName { get; set; }
        bool IsObject { get; set; }
        string Dev2ReturnType { get; set; }
        string ShortTypeName { get; set; }
    }
}