using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Decision;
using Dev2.Common.Interfaces.Studio.Controller;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ActivityDefaultWindow.xaml
    /// </summary>
    public partial class ActivityDefaultWindow
    {
        readonly Grid _blackoutGrid = new Grid();
        private static readonly IPopupController PopupController = CustomContainer.Get<IPopupController>();

        public ActivityDefaultWindow()
        {
            InitializeComponent();
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
        }

        void WindowBorderLess_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        void WindowBorderLess_OnClosed(object sender, EventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        #region Implementation of IComponentConnector

        #endregion

        void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            var content = ControlContentPresenter.Content as ActivityDesignerTemplate;

            var dataContext = content?.DataContext as DecisionDesignerViewModel;
            if(dataContext != null)
            {
                dataContext.Validate();
                if (dataContext.Errors != null)
                {
                    PopupController.Show(dataContext.Errors[0].Message, "Decision Error", MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false);
                    valid = false;
                }
            }

            if (valid)
            {
                DialogResult = true;
                Close();
            }
        }

        void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void SetEnableDoneButtonState(bool value)
        {
            DoneButton.IsEnabled = value;
            DoneButton.ToolTip = "Done";
            if (!value)
            {
                DoneButton.ToolTip = "You cannot make changes from the Test framework.";
            }
        }
    }
}
