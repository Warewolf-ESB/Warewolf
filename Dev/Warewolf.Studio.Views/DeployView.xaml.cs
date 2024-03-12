#if NETFRAMEWORK
using Microsoft.Practices.Prism.Mvvm;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;
#endif

namespace Warewolf.Studio.Views
{
    public partial class DeployView : IView
    {
        public DeployView()
        {
            InitializeComponent();
        }

#if !NETFRAMEWORK
        public string Path => throw new System.NotImplementedException();

        public Task RenderAsync(ViewContext context)
        {
            throw new System.NotImplementedException();
        }
#endif
    }
}
