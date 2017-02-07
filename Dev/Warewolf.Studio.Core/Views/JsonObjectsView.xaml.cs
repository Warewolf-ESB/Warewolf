using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Views
{
    /// <summary>
    /// Interaction logic for JsonObjectsView.xaml
    /// </summary>
    public partial class JsonObjectsView: IJsonObjectsView
    {
        readonly Grid _blackoutGrid = new Grid();

        public JsonObjectsView()
        {
            InitializeComponent();
        }

        void JsonObjectsView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        void JsonObjectsView_OnClosed(object sender, EventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public void ShowJsonString(string jsonString)
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            ResponseTextbox.Text = jsonString;
            Height = 280;
            ShowDialog();
        }

        private void JsonObjectsView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                mainViewModel?.ResetMainView();
            }
        }
    }
}
