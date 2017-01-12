using System.ComponentModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ChooseDLLView.xaml
    /// </summary>
    public partial class ChooseDLLView : IChooseDLLView
    {
        public ChooseDLLView()
        {
            InitializeComponent();
        }

        public void RequestClose()
        {
            DialogResult = true;
            Close();
        }

        public void ShowView(IDLLChooser chooser)
        {
            Closing += ChooseDLLView_OnClosing;
            MouseDown += ChooseDLLView_OnMouseDown;
            DataContext = chooser;
            ShowDialog();
        }

        private void ChooseDLLView_OnClosing(object sender, CancelEventArgs e)
        {
        }

        private void ChooseDLLView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
