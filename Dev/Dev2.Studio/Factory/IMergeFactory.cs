using Dev2.Studio.Interfaces;

namespace Dev2.Factory
{
    public interface IMergeFactory
    {
        void OpenMergeWindow(IShellViewModel shellViewModel, string item);
    }
}