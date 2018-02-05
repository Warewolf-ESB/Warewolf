/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections;
using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows.Controls
{    
    public class SelectorSelectionAdapter : ISelectionAdapter    
    {
        Selector _selector;
        bool IgnoringSelectionChanged { get; set; }
        public Selector SelectorControl
        {
            get { return _selector; }

            set
            {
                if(_selector != null)
                {
                    _selector.SelectionChanged -= OnSelectionChanged;
                    _selector.MouseLeftButtonUp -= OnSelectorMouseLeftButtonUp;
                }

                _selector = value;

                if(_selector != null)
                {
                    _selector.SelectionChanged += OnSelectionChanged;
                    _selector.MouseLeftButtonUp += OnSelectorMouseLeftButtonUp;
                }
            }
        }

        public event SelectionChangedEventHandler SelectionChanged;
        public event RoutedEventHandler Commit;
        public event RoutedEventHandler Cancel;

        public SelectorSelectionAdapter(Selector selector)
        {
            SelectorControl = selector;
        }
        
        public object SelectedItem
        {
            get
            {
                return SelectorControl?.SelectedItem;
            }

            set
            {
                IgnoringSelectionChanged = true;
                if(SelectorControl != null)
                {
                    SelectorControl.SelectedItem = value;
                }
                if(value == null)
                {
                    ResetScrollViewer();
                }

                IgnoringSelectionChanged = false;
            }
        }
        
        public IEnumerable ItemsSource
        {
            get
            {
                return SelectorControl?.ItemsSource;
            }
            set
            {
                if(SelectorControl != null)
                {
                    SelectorControl.ItemsSource = value;
                }
            }
        }

        void ResetScrollViewer()
        {
            var sv = SelectorControl?.GetLogicalChildrenBreadthFirst().OfType<ScrollViewer>().FirstOrDefault();
            sv?.ScrollToTop();
        }

        void OnSelectorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OnCommit();
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoringSelectionChanged)
            {
                return;
            }

            var handler = SelectionChanged;
            handler?.Invoke(sender, e);
        }

        protected void SelectedIndexIncrement()
        {
            if(SelectorControl != null)
            {
                SelectorControl.SelectedIndex = SelectorControl.SelectedIndex + 1 >= SelectorControl.Items.Count ? -1 : SelectorControl.SelectedIndex + 1;
            }
        }
        
        protected void SelectedIndexDecrement()
        {
            if(SelectorControl != null)
            {
                var index = SelectorControl.SelectedIndex;
                if (index >= 0)
                {
                    SelectorControl.SelectedIndex--;
                }
                else
                {
                    SelectorControl.SelectedIndex = SelectorControl.Items.Count - 1;
                }
            }
        }
        
        public void HandleKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    OnCommit();
                    e.Handled = true;
                    break;

                case Key.Up:
                    SelectedIndexDecrement();
                    e.Handled = true;
                    break;

                case Key.Down:
                    if ((ModifierKeys.Alt & Keyboard.Modifiers) == ModifierKeys.None)
                    {
                        SelectedIndexIncrement();
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    OnCancel();
                    e.Handled = true;
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }

        protected virtual void OnCommit()
        {
            OnCommit(this, new RoutedEventArgs());
        }

        void OnCommit(object sender, RoutedEventArgs e)
        {
            var handler = Commit;
            handler?.Invoke(sender, e);

            AfterAdapterAction();
        }

        protected virtual void OnCancel()
        {
            OnCancel(this, new RoutedEventArgs());
        }

        void OnCancel(object sender, RoutedEventArgs e)
        {
            var handler = Cancel;
            handler?.Invoke(sender, e);

            AfterAdapterAction();
        }

        void AfterAdapterAction()
        {
            IgnoringSelectionChanged = true;
            if (SelectorControl != null)
            {
                SelectorControl.SelectedItem = null;
                SelectorControl.SelectedIndex = -1;
            }
            IgnoringSelectionChanged = false;
        }

        public AutomationPeer CreateAutomationPeer() => _selector != null ? UIElementAutomationPeer.CreatePeerForElement(_selector) : null;
    }
}
