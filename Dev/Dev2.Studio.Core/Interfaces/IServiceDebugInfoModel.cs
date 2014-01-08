using Dev2.Studio.Core.AppResources;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IServiceDebugInfoModel
    {
        IContextualResourceModel ResourceModel { get; set; }
        string ServiceInputData { get; set; }
        DebugMode DebugModeSetting { get; set; }
        bool RememberInputs { get; set; }
    }
}
