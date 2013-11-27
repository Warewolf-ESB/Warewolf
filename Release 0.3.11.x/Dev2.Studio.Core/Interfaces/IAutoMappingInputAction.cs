using System;
using Dev2.Data.Interfaces;

namespace Dev2.Studio.Core.Interfaces {
    public interface IAutoMappingInputAction {
        IInputOutputViewModel LoadInputAutoMapping(IInputOutputViewModel item);        
    }
}
