    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

namespace Warewolf.Studio.Views
{
        public static class AutoCompleteBehavior
        {
            private static readonly TextChangedEventHandler onTextChanged = new TextChangedEventHandler(OnTextChanged);
            private static readonly KeyEventHandler onKeyDown = new KeyEventHandler(OnPreviewKeyDown);
            public static readonly DependencyProperty AutoCompleteItemsSource =
                DependencyProperty.RegisterAttached
                (
                    "AutoCompleteItemsSource",
                    typeof(IEnumerable<String>),
                    typeof(AutoCompleteBehavior),
                    new UIPropertyMetadata(null, OnAutoCompleteItemsSource)
                );

            public static IEnumerable<String> GetAutoCompleteItemsSource(DependencyObject obj)
            {
                object objRtn = obj.GetValue(AutoCompleteItemsSource);
                if (objRtn is IEnumerable<String>)
                    return (objRtn as IEnumerable<String>);

                return null;
            }

            public static void SetAutoCompleteItemsSource(DependencyObject obj, IEnumerable<String> value)
            {
                obj.SetValue(AutoCompleteItemsSource, value);
            }

            private static void OnAutoCompleteItemsSource(object sender, DependencyPropertyChangedEventArgs e)
            {
                TextBox tb = sender as TextBox;
                if (sender == null)
                    return;

                if (e.NewValue == null)
                {
                    if(tb != null)
                    {
                        tb.TextChanged -= onTextChanged;
                        tb.PreviewKeyDown -= onKeyDown;
                    }
                }
                else
                {
                    if(tb != null)
                    {
                        tb.TextChanged += onTextChanged;
                        tb.PreviewKeyDown += onKeyDown;
                    }
                }

            }

            static void OnPreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key != Key.Enter)
                    return;

                TextBox tb = e.OriginalSource as TextBox;
                if (tb == null)
                    return;

                //If we pressed enter and if the selected text goes all the way to the end, move our caret position to the end
                if (tb.SelectionLength > 0 && (tb.SelectionStart + tb.SelectionLength == tb.Text.Length))
                {
                    tb.SelectionStart = tb.CaretIndex = tb.Text.Length;
                    tb.SelectionLength = 0;
                }
            }

            static void OnTextChanged(object sender, TextChangedEventArgs e)
            {
                if
                (
                    (from change in e.Changes where change.RemovedLength > 0 select change).Any() &&
                    !(from change in e.Changes where change.AddedLength > 0 select change).Any()
                )
                    return;

                TextBox tb = e.OriginalSource as TextBox;
                if (sender == null)
                    return;

                IEnumerable<String> values = GetAutoCompleteItemsSource(tb);
                //No reason to search if we don't have any values.
                if (values == null)
                    return;

                //No reason to search if there's nothing there.
                if (tb != null && String.IsNullOrEmpty(tb.Text))
                    return;

                if(tb != null)
                {
                    Int32 textLength = tb.Text.Length;

                    //Do search and changes here.
                    IEnumerable<String> matches = values.Where(subvalue => subvalue.Length >= textLength).Where(value => value.ToLower().Substring(0, textLength) == tb.Text.ToLower());

                    //Nothing.  Leave 'em alone
                    if (!matches.Any())
                    {
                        return;
                    }
                    

                    String match = matches.ElementAt(0);
                    //String remainder = match.Substring(textLength, (match.Length - textLength));
                    tb.TextChanged -= onTextChanged;
                    tb.Text = match;
                    tb.CaretIndex = textLength;
                    tb.SelectionStart = textLength;
                    tb.SelectionLength = (match.Length - textLength);
                }
                if(tb != null)
                {
                    tb.TextChanged += onTextChanged;
                }
            }
        }
    }
