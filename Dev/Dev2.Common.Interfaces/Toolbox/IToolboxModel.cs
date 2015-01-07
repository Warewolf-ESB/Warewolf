using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolboxModel
    {
        IServer Server{get;}
        IList<IToolDescriptor> GetTools();
        bool IsToolSupported(IToolDescriptor tool);
        void LoadTool(IToolDescriptor tool);
        void DeleteTool(IToolDescriptor tool);
        IList<IToolDescriptor> Filter(string search); 
    }
}