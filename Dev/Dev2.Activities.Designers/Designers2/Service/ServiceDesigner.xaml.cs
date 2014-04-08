using System.Windows;
using Dev2.Studio.Core.Activities.Services;

namespace Dev2.Activities.Designers2.Service
{
    public partial class ServiceDesigner
    {
        public ServiceDesigner()
        {
            InitializeComponent();
        }

        protected override ServiceDesignerViewModel CreateViewModel()
        {
            var designerManagementService = Context.Services.GetService<IDesignerManagementService>();
            return new ServiceDesignerViewModel(ModelItem, designerManagementService.GetRootResourceModel());
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            ViewModel.UpdateMappings();
        }
    }
}
    