namespace Dev2.Common.Interfaces.DB
{
    public interface IDbInput
    {
        string Name{ get; set; }
        string Value { get; set; }
        string DefaultValue { get; set; }
        bool RequiredField { get; set; }
        bool EmptyIsNull { get; set; }
    }
}