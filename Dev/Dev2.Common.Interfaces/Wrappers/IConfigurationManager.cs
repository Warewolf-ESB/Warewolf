namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IConfigurationManager
    {
        string this[string settingName, string defaultValue = null] { get; set; }
    }
}
