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
    public enum SearchMode
    {
        Instant,
        Delayed,
    }

    public class SearchTextBox : TextBox
    {
        static DependencyProperty labelTextProperty =
            DependencyProperty.Register(
                "LabelText",
                typeof(string),
                typeof(SearchTextBox));

        static DependencyProperty labelTextColorProperty =
            DependencyProperty.Register(
                "LabelTextColor",
                typeof(Brush),
                typeof(SearchTextBox));

        static DependencyProperty clearSearchCommandProperty =
            DependencyProperty.Register(
                "ClearSearchCommand",
                typeof(ICommand),
                typeof(SearchTextBox));

        static DependencyProperty searchModeProperty =
            DependencyProperty.Register(
                "SearchMode",
                typeof(SearchMode),
                typeof(SearchTextBox),
                new PropertyMetadata(SearchMode.Instant));

        static DependencyPropertyKey HasTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasText",
                typeof(bool),
                typeof(SearchTextBox),
                new PropertyMetadata());

        static DependencyProperty hasTextProperty = HasTextPropertyKey.DependencyProperty;

        public static readonly RoutedEvent SearchEvent =
            EventManager.RegisterRoutedEvent(
                "Search",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(SearchTextBox));

        public static readonly DependencyProperty ClearSearchToolTipProperty =
            DependencyProperty.Register("ClearSearchToolTip",
            typeof(string),
            typeof(SearchTextBox));

        static SearchTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SearchTextBox),
                new FrameworkPropertyMetadata(typeof(SearchTextBox)));
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            HasText = Text.Length != 0;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            HasText = Text.Length != 0;
            if (!HasText)
            {
                Focus();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && SearchMode == SearchMode.Instant)
            {
                Text = "";
            }
            else if ((e.Key == Key.Return || e.Key == Key.Enter) &&
                SearchMode == SearchMode.Delayed)
            {
                RaiseSearchEvent();
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        void RaiseSearchEvent()
        {
            var args = new RoutedEventArgs(SearchEvent);
            RaiseEvent(args);
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

        public ICommand ClearSearchCommand
        {
            get => (ICommand)GetValue(ClearSearchCommandProperty);
            set => SetValue(ClearSearchCommandProperty, value);
        }

        public SearchMode SearchMode
        {
            get => (SearchMode)GetValue(SearchModeProperty);
            set => SetValue(SearchModeProperty, value);
        }

        public bool HasText
        {
            get => (bool)GetValue(HasTextProperty);
            private set => SetValue(HasTextPropertyKey, value);
        }

        public string ClearSearchToolTip
        {
            get => (string)GetValue(ClearSearchToolTipProperty);
            set => SetValue(ClearSearchToolTipProperty, value);
        }

        public static DependencyProperty HasTextProperty
        {
            get => hasTextProperty;
            set => hasTextProperty = value;
        }

        public static DependencyProperty SearchModeProperty
        {
            get => searchModeProperty;
            set => searchModeProperty = value;
        }

        public static DependencyProperty ClearSearchCommandProperty
        {
            get => clearSearchCommandProperty;
            set => clearSearchCommandProperty = value;
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

        public event RoutedEventHandler Search
        {
            add { AddHandler(SearchEvent, value); }
            remove { RemoveHandler(SearchEvent, value); }
        }
    }
}