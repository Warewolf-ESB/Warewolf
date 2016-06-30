using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Warewolf.Studio.Core;

namespace Dev2.Views.DropBox2016
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DropBoxViewWindow
    {
        readonly Grid _blackoutGrid = new Grid();

        public DropBoxViewWindow()
        {
            InitializeComponent();
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
        }

        void RequestClose()
        {
            Close();
        }

        void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            RequestClose();
        }

        void DropBoxViewWindow_OnClosing(object sender, CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }
    }
}
