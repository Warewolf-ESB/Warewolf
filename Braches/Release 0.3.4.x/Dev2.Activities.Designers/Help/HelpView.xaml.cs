using System.Windows;

namespace Dev2.Activities.Help
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView
    {
        public HelpView()
        {
            if(Application.Current != null)
            {
                InitializeComponent();
            }
        }
    }
}
