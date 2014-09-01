// ReSharper disable once CheckNamespace

using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces.Studio.Core
{
    public interface IAutoMappingOutputAction
    {
        IInputOutputViewModel LoadOutputAutoMapping(IInputOutputViewModel item);
    }
}
