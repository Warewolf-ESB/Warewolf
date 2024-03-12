using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Dev2.Common.Interfaces
{
    public interface ICreateDuplicateResourceView : IView
    {
        void ShowView();

        void CloseView();
    }
}