using System.Windows;
using System.Windows.Controls;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for WorkflowDesignerView.xaml
    /// </summary>
    public partial class WorkflowDesignerView : UserControl,IDesignerView
    {
        public WorkflowDesignerView()
        {
            InitializeComponent();
        }

        public UIElement Designer
        {
            get;
            set;
        }
    }
}
