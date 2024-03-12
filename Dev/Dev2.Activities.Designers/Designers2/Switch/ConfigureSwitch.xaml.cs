using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;

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

		public string Path => throw new System.NotImplementedException();

		public Task RenderAsync(ViewContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
