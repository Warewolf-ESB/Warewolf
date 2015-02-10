using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolManager
    {

        IList<IToolDescriptor> LoadTools();

    }
}