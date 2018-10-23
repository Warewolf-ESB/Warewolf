namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IConfigurationManager
    {
        string this[string args] { get; set; }
    }
}
