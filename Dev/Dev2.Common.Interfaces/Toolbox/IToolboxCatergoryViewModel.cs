using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolboxCatergoryViewModel
    {
        string Name { get; }
        ICollection<IToolDescriptorViewModel> Tools { get; } 
    }
}