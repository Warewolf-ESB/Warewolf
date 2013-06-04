using System;
using System.ServiceModel;

namespace Dev2
{
    public interface IFrameworkDataChannel
    {
        string ExecuteCommand(string xmlRequest, Guid workspaceID, Guid dataListID);
    }

    public interface IFrameworkWorkspaceChannel : IFrameworkDataChannel
    {

    }

    public interface IFrameworkActivityChannel
    {
        bool ExecuteParallel(IFrameworkActivityInstruction[] instructions);
    }

    public interface IFrameworkActivityInstruction
    {
        string Instruction { get; }
        string Result { get; set; }
        Guid dataListID { get; set; }
    }
}
