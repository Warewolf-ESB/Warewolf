
using System.Windows.Input;

namespace Dev2.Studio.ActivityDesigners
{
    // Interaction logic for DsfScriptingJavaScrip.xaml
    public partial class DsfScriptingJavaScriptDesigner
    {
        public DsfScriptingJavaScriptDesigner()
        {
            InitializeComponent();
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfScriptingJavaScriptDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
