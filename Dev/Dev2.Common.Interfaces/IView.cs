#if NETFRAMEWORK
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Prism.Mvvm
{
    public interface IView
    {
        string Path { get; }
        Task RenderAsync(ViewContext context);
    }
}
#endif