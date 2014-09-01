using Dev2.Common.Interfaces.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IAutoMappingOutputAction
    {
        IInputOutputViewModel LoadOutputAutoMapping(IInputOutputViewModel item);
    }
}
