using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for MessageBoxView.xaml
    /// </summary>
    public partial class MessageBoxView
    {
        readonly Grid _blackoutGrid = new Grid();
        private bool _openDependencyGraph;
        public bool OpenDependencyGraph => _openDependencyGraph;

        public MessageBoxView()
        {
            InitializeComponent();
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
        }

        void MessageBoxView_OnClosing(object sender, CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        void BtnDependencies_OnClick(object sender, RoutedEventArgs e)
        {
            _openDependencyGraph = true;
            DialogResult = false;
        }
    }
}
