using System.Windows;

namespace Dev2.Activities.Designers.DsfDateTime
{
    // Interaction logic for DsfDateTimeActivityDesigner.xaml
    public partial class DsfDateTimeActivityDesigner
    {
        public DsfDateTimeActivityDesigner()
        {
            InitializeComponent();
        }
        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            // 2013.07.29: Ashley Lewis for bug 9949 - workaround for Automatic-drill-down
        }
    }
}
