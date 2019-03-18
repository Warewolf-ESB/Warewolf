#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.Switch;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Interfaces;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.Views
{
    public partial class ActivityDefaultWindow
    {
        readonly Grid _blackoutGrid = new Grid();
        static readonly IPopupController PopupController = CustomContainer.Get<IPopupController>();

        public ActivityDefaultWindow()
        {
            InitializeComponent();
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
        }

        void WindowBorderLess_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        void WindowBorderLess_OnClosed(object sender, EventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            var valid = true;
            var content = ControlContentPresenter.Content as ActivityDesignerTemplate;

            if (content == null)
            {
                valid = ValidateSwitchCase(true);
            }
            else
            {
                if (content.DataContext is DecisionDesignerViewModel dataContext)
                {
                    dataContext.Validate();
                    if (dataContext.Errors != null)
                    {
                        PopupController.Show(dataContext.Errors[0].Message, "Decision Error", MessageBoxButton.OK,
                            MessageBoxImage.Error, "", false, true, false, false, false, false);
                        valid = false;
                    }
                }
            }
            if (valid)
            {
                DialogResult = true;
                Close();
            }
        }

        bool ValidateSwitchCase(bool valid)
        {
            var configureSwitchArm = ControlContentPresenter.Content as ConfigureSwitchArm;

            if (configureSwitchArm?.DataContext is SwitchDesignerViewModel switchDesignerViewModel)
            {
                if (string.IsNullOrWhiteSpace(switchDesignerViewModel.SwitchExpression))
                {
                    PopupController.Show(Studio.Resources.Languages.Core.SwitchCaseEmptyExpressionMessage, Studio.Resources.Languages.Core.SwitchFlowErrorHeader,
                        MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false, false, false);
                    valid = false;
                }
                else
                {
                    switchDesignerViewModel.Validate();
                    if (!switchDesignerViewModel.ValidExpression)
                    {
                        PopupController.Show(Studio.Resources.Languages.Core.SwitchCaseUniqueMessage, Studio.Resources.Languages.Core.SwitchFlowErrorHeader,
                            MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false, false, false);

                        valid = false;
                    }
                }
            }
            return valid;
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

        void ActivityDefaultWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                mainViewModel?.ResetMainView();
            }
        }
    }
}
