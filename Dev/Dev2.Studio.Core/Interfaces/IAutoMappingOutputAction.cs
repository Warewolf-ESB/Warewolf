using Dev2.Data.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IAutoMappingOutputAction
    {
        IInputOutputViewModel LoadOutputAutoMapping(IInputOutputViewModel item);
    }
}
