using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface ICreateDuplicateResourceView : IView
    {
        void ShowView();

        void CloseView();
    }
}