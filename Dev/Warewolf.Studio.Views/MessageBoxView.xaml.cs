#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using Warewolf.Studio.Core;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    public partial class MessageBoxView
    {
        readonly Grid _blackoutGrid = new Grid();
        bool _openDependencyGraph;
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

        void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (sender is Hyperlink resourcePath)
            {
                var listStrLineElements = resourcePath.NavigateUri.OriginalString.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                for (int i = 1; i < listStrLineElements.Count; i++)
                {
                    Process.Start("explorer.exe", "/select," + listStrLineElements[i]);
                }
            }
        }

        void MessageBoxView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var messageBoxViewModel = DataContext as MessageBoxViewModel;
            if (messageBoxViewModel != null && messageBoxViewModel.IsYesButtonVisible)
            {
                BtnYesCommand.Focusable = true;
                BtnYesCommand.Focus();
            }
            if (messageBoxViewModel != null && messageBoxViewModel.IsOkButtonVisible)
            {
                BtnOkCommand.Focusable = true;
                BtnOkCommand.Focus();
            }
        }

        void MessageBoxView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        void MessageBoxView_OnPreviewKeyDown(object sender, KeyEventArgs e)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4) && Application.Current != null)
            {
                CloseAllWindows();
            }

            if (e.Key == Key.Escape)
            {
                var messageBoxViewModel = DataContext as MessageBoxViewModel;
                if (messageBoxViewModel != null && messageBoxViewModel.IsYesButtonVisible)
                {
                    BtnYesCommand.Focusable = false;
                    BtnYesCommand.Focus();
                }
                if (messageBoxViewModel != null && messageBoxViewModel.IsOkButtonVisible)
                {
                    BtnOkCommand.Focusable = false;
                    BtnOkCommand.Focus();
                }
                if (messageBoxViewModel != null && messageBoxViewModel.IsCancelButtonVisible)
                {
                    BtnCancelCommand.Focusable = true;
                    BtnCancelCommand.Focus();
                }
                if (messageBoxViewModel != null && messageBoxViewModel.IsNoButtonVisible)
                {
                    BtnNoCommand.Focusable = true;
                    BtnNoCommand.Focus();
                }
            }
        }

        private static void CloseAllWindows()
        {
            var windowCollection = Application.Current.Windows;
            foreach (var window in windowCollection)
            {
                if (window is Window window1 && window1.Name != "MainViewWindow")
                {
                    window1.Close();
                }
            }
        }

        void BtnDeleteAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MessageBoxViewModel messageBoxViewModel)
            {
                messageBoxViewModel.IsDeleteAnywaySelected = true;
            }
            DialogResult = false;
        }
    }
}
