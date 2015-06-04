namespace Dev2.Common.Interfaces
{
    public interface INamespaceItem
    {
        string AssemblyLocation { get; set; }
        string AssemblyName { get; set; }
        string FullName { get; set; }
        string MethodName { get; set; }
    }
}