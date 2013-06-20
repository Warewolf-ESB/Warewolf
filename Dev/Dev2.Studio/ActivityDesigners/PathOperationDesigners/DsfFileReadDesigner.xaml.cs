#define Debug

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.UiAutomation;
using Dev2.Studio.Views.UIAutomation;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfFileReadDesigner.xaml
    public partial class DsfFileReadDesigner
    {
        public DsfFileReadDesigner()
        {
            InitializeComponent();            
        }

     

//        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
//        {
//#if Debug
//            //AutomationIdCreaterView view = new AutomationIdCreaterView();
//            string _automationId = string.Empty;
//            if (this.ModelItem.Properties["AutomationID"].ComputedValue != null)
//            {
//                _automationId = this.ModelItem.Properties["AutomationID"].ComputedValue.ToString();
//            }
//            AutomationIdCreaterViewModel viewModel = new AutomationIdCreaterViewModel();
//            if (!string.IsNullOrEmpty(_automationId))
//            {
//                viewModel.AutomationID = _automationId;
//            }
//            WindowNavigationBehavior.ShowDialog(viewModel);

//            //view.DataContext = viewModel;
//            //view.ShowDialog();
//            if (viewModel.DialogResult == ViewModelDialogResults.Okay)
//            {
//                this.ModelItem.Properties["AutomationID"].ComputedValue = viewModel.AutomationID;
//            }           
//#endif
//        }


        private void ActivityDesigner_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {

            //string original = AutomationProperties.GetAutomationId(result);
            //AutomationProperties.SetAutomationId(result, "siudfnisundifunuios");
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfFileReadDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
