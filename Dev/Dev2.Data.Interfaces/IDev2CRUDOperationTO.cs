#pragma warning disable
namespace Dev2.Data.Interfaces
{
    public interface IDev2CRUDOperationTO : IPathOverwrite
    {
        bool DoRecursiveCopy { get; set; }
    }
}