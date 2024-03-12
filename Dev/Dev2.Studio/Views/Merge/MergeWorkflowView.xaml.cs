#if NETFRAMEWORK
using Microsoft.Practices.Prism.Mvvm;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;
#endif

namespace Dev2.Views.Merge
{
    /// <summary>
    /// Interaction logic for MergeWorkflowView.xaml
    /// </summary>
    public partial class MergeWorkflowView : IView
    {
        public MergeWorkflowView()
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
