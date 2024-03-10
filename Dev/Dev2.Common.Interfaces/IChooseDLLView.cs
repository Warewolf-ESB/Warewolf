#pragma warning disable
using Microsoft.AspNetCore.Mvc.ViewEngines;


namespace Dev2.Common.Interfaces
{
    public interface IChooseDLLView : IView
    {
        void ShowView(IDLLChooser chooser);
        void RequestClose();
    }
}
