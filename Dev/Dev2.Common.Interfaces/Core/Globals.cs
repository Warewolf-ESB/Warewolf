
namespace Dev2.Common.Interfaces.Core
{
    public enum enObjectState { NEW, UNCHANGED, CHANGED };
    public delegate void MessageEventHandler(string message);
}

// Moved Dev2.DynamicServices enums to Dev2.Data assembly
