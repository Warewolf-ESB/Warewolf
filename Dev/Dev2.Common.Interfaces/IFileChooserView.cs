using System.Collections.Generic;
#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ViewEngines;
#else
using Microsoft.Practices.Prism.Mvvm;
#endif

namespace Dev2.Common.Interfaces
{
    public interface IFileChooserView : IView
    {
        void ShowView(IList<string> files);
        void RequestClose();
        void ShowView(IList<string> files, string filter);
        void ShowView(bool allowMultipleSelection);
    }
}
