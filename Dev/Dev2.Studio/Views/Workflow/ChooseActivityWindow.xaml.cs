using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for ChooseActivityWindow.xaml
    /// </summary>
    public partial class ChooseActivityWindow
    {
        public ChooseActivityWindow()
        {
            InitializeComponent();
        }

        private void StackPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount >= 2)
            {
                dynamic dcontext = DataContext;
                if(dcontext != null)
                {
                    if(dcontext.OKCommand != null)
                        dcontext.OKCommand.Execute(null);
                }



            }
        }
    }
}
