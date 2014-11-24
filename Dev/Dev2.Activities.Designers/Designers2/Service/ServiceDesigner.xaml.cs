
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
    
