#if NETFRAMEWORK
using Microsoft.Practices.Prism.Mvvm;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;
#endif

namespace Dev2.Activities.Designers2.Switch
{
    /// <summary>
    /// Interaction logic for ConfigureSwitch.xaml
    /// </summary>
    public partial class ConfigureSwitch : IView
    {
        public ConfigureSwitch()
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
