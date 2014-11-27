
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

// ReSharper disable CheckNamespace
namespace Dev2.CustomControls.Behavior
{
    public class TextChangedRegexBehavior : Behavior<TextBox>
    {
        #region dependency properties

        public string RegexOptions
        {
            get { return (string)GetValue(RegexOptionsProperty); }
            set { SetValue(RegexOptionsProperty, value); }
        }

        public static readonly DependencyProperty RegexOptionsProperty =
            DependencyProperty.Register("RegexOptions", typeof(string), typeof(TextChangedRegexBehavior), new PropertyMetadata(string.Empty));


        public int MaxStringLength
        {
            get { return (int)GetValue(MaxStringLengthProperty); }
            set { SetValue(MaxStringLengthProperty, value); }
        }

        public static readonly DependencyProperty MaxStringLengthProperty =
            DependencyProperty.Register("MaxStringLength", typeof(int), typeof(TextChangedRegexBehavior), new PropertyMetadata(0));

        string _originalText;

        #endregion dependency properties

        #region attach/detach

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += AssociatedObjectTextChanged;
            AssociatedObject.GotKeyboardFocus += AssociatedObjectGotKeyboardFocus;
            AssociatedObject.KeyDown += AssociatedObjectKeyDown;
        }

        void AssociatedObjectKeyDown(object sender, KeyEventArgs e)
        {
            if(String.IsNullOrEmpty(_originalText)) return;
            if(e.Key == Key.Escape)
            {
                AssociatedObject.Text = _originalText;
            }
        }

        void AssociatedObjectGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _originalText = AssociatedObject.Text;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= AssociatedObjectTextChanged;
            AssociatedObject.GotKeyboardFocus -= AssociatedObjectGotKeyboardFocus;
            AssociatedObject.KeyDown -= AssociatedObjectKeyDown;
            base.OnDetaching();
        }

        #endregion attach/detach

        #region event handlers

        void AssociatedObjectTextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrEmpty(RegexOptions))
            {
                return;
            }

            var text = AssociatedObject.Text;
            var newText = Regex.Replace(AssociatedObject.Text, RegexOptions, "");

            if(MaxStringLength > 0 && newText.Length > MaxStringLength)
            {
                newText = newText.Substring(0, MaxStringLength);
            }

            if(text.Length == newText.Length)
            {
                return;
            }

            var selectionStart = AssociatedObject.SelectionStart - (text.Length - newText.Length);
            AssociatedObject.Text = newText;
            AssociatedObject.SelectionStart = selectionStart;
        }

        #endregion event handlers
    }
}
