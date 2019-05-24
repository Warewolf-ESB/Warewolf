#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Warewolf.Studio.CustomControls
{
    public class VariableTextBox : TextBox
    {
        private static DependencyProperty labelTextProperty =
            DependencyProperty.Register(
                "LabelText",
                typeof(string),
                typeof(VariableTextBox));

        private static DependencyProperty labelTextColorProperty =
            DependencyProperty.Register(
                "LabelTextColor",
                typeof(Brush),
                typeof(VariableTextBox));

        private static DependencyProperty deleteCommandProperty =
            DependencyProperty.Register(
                "DeleteCommand",
                typeof(ICommand),
                typeof(VariableTextBox));

        private static DependencyProperty viewComplexObjectsCommandProperty =
            DependencyProperty.Register(
                "ViewComplexObjectsCommand",
                typeof(ICommand),
                typeof(VariableTextBox));

        private static readonly DependencyPropertyKey HasTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasText",
                typeof(bool),
                typeof(VariableTextBox),
                new PropertyMetadata());

        private static DependencyProperty hasTextProperty = HasTextPropertyKey.DependencyProperty;

        private static DependencyProperty allowNotesProperty =
            DependencyProperty.Register(
                "AllowNotes",
                typeof(bool),
                typeof(VariableTextBox));

        private static DependencyProperty isUsedProperty =
            DependencyProperty.Register(
                "IsUsed",
                typeof(bool),
                typeof(VariableTextBox));

        private static DependencyProperty isComplexObjectProperty =
            DependencyProperty.Register(
                "IsComplexObject",
                typeof(bool),
                typeof(VariableTextBox));

        static VariableTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(VariableTextBox),
                new FrameworkPropertyMetadata(typeof(VariableTextBox)));
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            HasText = Text.Length != 0;
        }

        public string LabelText
        {
            get => (string)GetValue(LabelTextProperty);
            set => SetValue(LabelTextProperty, value);
        }

        public Brush LabelTextColor
        {
            get => (Brush)GetValue(LabelTextColorProperty);
            set => SetValue(LabelTextColorProperty, value);
        }

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set
            {
                if (IsUsed)
                {
                    SetValue(DeleteCommandProperty, value);
                }
            }
        }

        public ICommand ViewComplexObjectsCommand
        {
            get => (ICommand)GetValue(ViewComplexObjectsCommandProperty);
            set
            {
                if (IsComplexObject)
                {
                    SetValue(ViewComplexObjectsCommandProperty, value);
                }
            }
        }

        public bool HasText
        {
            get => (bool)GetValue(HasTextProperty);
            private set => SetValue(HasTextPropertyKey, value);
        }

        public bool AllowNotes
        {
            get => (bool)GetValue(AllowNotesProperty);
            set => SetValue(AllowNotesProperty, value);
        }

        public bool IsUsed
        {
            get => (bool)GetValue(IsUsedProperty);
            set => SetValue(IsUsedProperty, value);
        }

        public bool IsComplexObject
        {
            get => (bool)GetValue(IsComplexObjectProperty);
            set => SetValue(IsComplexObjectProperty, value);
        }

        public static DependencyProperty IsComplexObjectProperty
        {
            get => isComplexObjectProperty;
            set => isComplexObjectProperty = value;
        }

        public static DependencyProperty IsUsedProperty
        {
            get => isUsedProperty;
            set => isUsedProperty = value;
        }

        public static DependencyProperty AllowNotesProperty
        {
            get => allowNotesProperty;
            set => allowNotesProperty = value;
        }

        public static DependencyProperty HasTextProperty
        {
            get => hasTextProperty;
            set => hasTextProperty = value;
        }

        public static DependencyProperty ViewComplexObjectsCommandProperty
        {
            get => viewComplexObjectsCommandProperty;
            set => viewComplexObjectsCommandProperty = value;
        }

        public static DependencyProperty DeleteCommandProperty
        {
            get => deleteCommandProperty;
            set => deleteCommandProperty = value;
        }

        public static DependencyProperty LabelTextColorProperty
        {
            get => labelTextColorProperty;
            set => labelTextColorProperty = value;
        }

        public static DependencyProperty LabelTextProperty
        {
            get => labelTextProperty;
            set => labelTextProperty = value;
        }
    }
}