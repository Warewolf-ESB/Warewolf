using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IToolboxView : IView
    {
        void EnterSearch(string searchTerm);

        bool CheckToolIsVisible(string toolName);

        bool CheckAllToolsNotVisible();

        void ClearFilter();

        int GetToolCount();
    }
}