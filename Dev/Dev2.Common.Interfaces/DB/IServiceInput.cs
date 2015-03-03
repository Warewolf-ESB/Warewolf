namespace Dev2.Common.Interfaces.DB
{
    public interface IServiceInput
    {
        string Name { get; set; }
        string Value { get; set; }
        bool Required { get; set; }
        bool EmptyIsNull { get; set; }
    }
}