﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;

namespace Warewolf.Studio.CustomControls
{
    public partial class PasswordBoxUserControl
    {
        public PasswordBoxUserControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(PasswordBoxUserControl),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBoxUserControl source)
            {
                source.Text = (string) e.NewValue;
                source.txtPassword.Password = (string) e.NewValue;
            }
        }

        public string Text
        {
            get => GetValue(TextProperty) as string;
            set => SetValue(TextProperty, value);
        }

        private void ShowPassword()
        {
            PasswordBoxView.Visibility = Visibility.Visible;
            TextBoxView.Visibility = Visibility.Hidden;

            if (!string.IsNullOrEmpty(txtVisiblePassword.Text))
            {
                txtPassword.Password = txtVisiblePassword.Text;
            }
        }

        private void HidePassword()
        {
            PasswordBoxView.Visibility = Visibility.Hidden;
            TextBoxView.Visibility = Visibility.Visible;
            if (txtPassword.Password.StartsWith("[[") && txtPassword.Password.EndsWith("]]"))
            {
                txtVisiblePassword.Text = txtPassword.Password;
            }
            else
            {
                txtVisiblePassword.Text = string.Empty;
            }
        }

        private void ShowPasswordButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowPassword();
        }

        private void HidePasswordButton_OnClick(object sender, RoutedEventArgs e)
        {
            HidePassword();
        }

        private void TxtPassword_OnLostFocus(object sender, RoutedEventArgs e)
        {
            string value;
            if (sender is PasswordBox passwordBox)
            {
                value = passwordBox.Password;
            }
            else
            {
                value = ((AutoCompleteBox) sender).Text;
            }

            SetValue(TextProperty, value);
        }
    }
}