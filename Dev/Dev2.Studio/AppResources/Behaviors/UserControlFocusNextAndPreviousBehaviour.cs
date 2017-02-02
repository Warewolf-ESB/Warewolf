/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class UserControlFocusNextAndPreviousBehaviour : Behavior<UserControl>, IDisposable
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            routedEventArgs.Handled = true;
        }

        void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;
            if (e.Key == Key.Up)
            {
                if(keyboardFocus != null)
                {
                    try
                    {
                        keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));

                        UIElement newKeyboardFocusTreeItem = Keyboard.FocusedElement as TreeViewItem ?? (UIElement)(Keyboard.FocusedElement as CheckBox);
                        while(newKeyboardFocusTreeItem != null)
                        {
                            newKeyboardFocusTreeItem.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                            newKeyboardFocusTreeItem = Keyboard.FocusedElement as TreeViewItem ?? (UIElement)(Keyboard.FocusedElement as CheckBox);
                        }
                    }
                    catch(StackOverflowException)
                    {
                        //
                    }
                    finally
                    {
                        e.Handled = true;
                    }
                }
            }
            else if (e.Key == Key.Down)
            {
                if (keyboardFocus != null)
                {
                    try
                    {
                        keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));

                        UIElement newKeyboardFocusTreeItem = Keyboard.FocusedElement as TreeViewItem ?? (UIElement)(Keyboard.FocusedElement as CheckBox);
                        while(newKeyboardFocusTreeItem != null)
                        {
                            newKeyboardFocusTreeItem.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                            newKeyboardFocusTreeItem = Keyboard.FocusedElement as TreeViewItem ?? (UIElement)(Keyboard.FocusedElement as CheckBox);
                        }
                    }
                    catch(StackOverflowException)
                    {
                        //
                    }
                    finally
                    {
                        e.Handled = true;
                    }
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    UIElement newKeyboardFocusTreeItem = Keyboard.FocusedElement as TreeViewItem ?? (UIElement)(Keyboard.FocusedElement as CheckBox);
                    while (newKeyboardFocusTreeItem != null)
                    {
                        newKeyboardFocusTreeItem.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                        newKeyboardFocusTreeItem = Keyboard.FocusedElement as TreeViewItem ?? (UIElement)(Keyboard.FocusedElement as CheckBox);
                    }
                    e.Handled = true;
                }
            }
            else if (e.KeyboardDevice.IsKeyDown(Key.LeftShift) && e.KeyboardDevice.IsKeyDown(Key.Tab))
            {
                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                    UIElement newKeyboardFocus = Keyboard.FocusedElement as TreeViewItem;
                    while(newKeyboardFocus != null)
                    {
                        newKeyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                        newKeyboardFocus = Keyboard.FocusedElement as TreeViewItem;
                    }
                    e.Handled = true;
                }
            }
            else if(e.KeyboardDevice.IsKeyDown(Key.Tab))
            {
                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    UIElement newKeyboardFocus = Keyboard.FocusedElement as TreeViewItem;
                    while(newKeyboardFocus != null)
                    {
                        newKeyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        newKeyboardFocus = Keyboard.FocusedElement as TreeViewItem;
                    }
                    e.Handled = true;
                }
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            base.OnDetaching();
        }

        public void Dispose()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
        }

    }
}
