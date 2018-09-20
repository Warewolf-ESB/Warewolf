namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IConfigurationManager
    {
        string this[params string[] args] { get; set; }
    }
}
