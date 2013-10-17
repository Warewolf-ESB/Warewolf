

using System.Runtime.Remoting.Contexts;
using Dev2.Studio.Core.Activities.Services;

namespace Dev2.Activities.Designers2.Service
{
    public partial class ServiceDesigner
    {
        public ServiceDesigner()
        {
            InitializeComponent();
        }

        #region Overrides of ActivityDesigner<ServiceDesignerViewModel>

        protected override ServiceDesignerViewModel CreateViewModel()
        {
            var designerManagementService = Context.Services.GetService<IDesignerManagementService>();
            return new ServiceDesignerViewModel(ModelItem, designerManagementService.GetRootResourceModel());
        }

        #endregion
    }
}
