/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System.Collections;
using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows.Controls
{
    /// <summary>
    /// Represents the selection adapter contained in the drop-down portion of
    /// an <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    
    public class SelectorSelectionAdapter : ISelectionAdapter
    
    {
        /// <summary>
        /// The Selector instance.
        /// </summary>
        private Selector _selector;

        /// <summary>
        /// Gets or sets a value indicating whether the selection change event 
        /// should not be fired.
        /// </summary>
        private bool IgnoringSelectionChanged { get; set; }

        /// <summary>
        /// Gets or sets the underlying
        /// <see cref="T:System.Windows.Controls.Primitives.Selector" />
        /// control.
        /// </summary>
        /// <value>The underlying
        /// <see cref="T:System.Windows.Controls.Primitives.Selector" />
        /// control.</value>
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

        /// <summary>
        /// Occurs when the
        /// <see cref="P:System.Windows.Controls.SelectorSelectionAdapter.SelectedItem" />
        /// property value changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Occurs when an item is selected and is committed to the underlying
        /// <see cref="T:System.Windows.Controls.Primitives.Selector" />
        /// control.
        /// </summary>
        public event RoutedEventHandler Commit;

        /// <summary>
        /// Occurs when a selection is canceled before it is committed.
        /// </summary>
        public event RoutedEventHandler Cancel;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:System.Windows.Controls.SelectorSelectionAdapter" />
        /// class.
        /// </summary>
        public SelectorSelectionAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:System.Windows.Controls.SelectorSelectionAdapter" />
        /// class with the specified
        /// <see cref="T:System.Windows.Controls.Primitives.Selector" />
        /// control.
        /// </summary>
        /// <param name="selector">The
        /// <see cref="T:System.Windows.Controls.Primitives.Selector" /> control
        /// to wrap as a
        /// <see cref="T:System.Windows.Controls.SelectorSelectionAdapter" />.</param>
        public SelectorSelectionAdapter(Selector selector)
        {
            SelectorControl = selector;
        }

        /// <summary>
        /// Gets or sets the selected item of the selection adapter.
        /// </summary>
        /// <value>The selected item of the underlying selection adapter.</value>
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

                // Attempt to reset the scroll viewer's position
                if(value == null)
                {
                    ResetScrollViewer();
                }

                IgnoringSelectionChanged = false;
            }
        }

        /// <summary>
        /// Gets or sets a collection that is used to generate the content of
        /// the selection adapter.
        /// </summary>
        /// <value>The collection used to generate content for the selection
        /// adapter.</value>
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

        /// <summary>
        /// If the control contains a ScrollViewer, this will reset the viewer 
        /// to be scrolled to the top.
        /// </summary>
        private void ResetScrollViewer()
        {
            ScrollViewer sv = SelectorControl?.GetLogicalChildrenBreadthFirst().OfType<ScrollViewer>().FirstOrDefault();
            sv?.ScrollToTop();
        }

        /// <summary>
        /// Handles the mouse left button up event on the selector control.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnSelectorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OnCommit();
        }

        /// <summary>
        /// Handles the SelectionChanged event on the Selector control.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The selection changed event data.</param>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(IgnoringSelectionChanged)
            {
                return;
            }

            SelectionChangedEventHandler handler = SelectionChanged;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        /// Increments the
        /// <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedIndex" />
        /// property of the underlying
        /// <see cref="T:System.Windows.Controls.Primitives.Selector" />
        /// control.
        /// </summary>
        protected void SelectedIndexIncrement()
        {
            if(SelectorControl != null)
            {
                SelectorControl.SelectedIndex = SelectorControl.SelectedIndex + 1 >= SelectorControl.Items.Count ? -1 : SelectorControl.SelectedIndex + 1;
            }
        }

        /// <summary>
        /// Decrements the
        /// <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedIndex" />
        /// property of the underlying
        /// <see cref="T:System.Windows.Controls.Primitives.Selector" />
        /// control.
        /// </summary>
        protected void SelectedIndexDecrement()
        {
            if(SelectorControl != null)
            {
                int index = SelectorControl.SelectedIndex;
                if(index >= 0)
                {
                    SelectorControl.SelectedIndex--;
                }
                else if(index == -1)
                {
                    SelectorControl.SelectedIndex = SelectorControl.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Provides handling for the
        /// <see cref="E:System.Windows.UIElement.KeyDown" /> event that occurs
        /// when a key is pressed while the drop-down portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> has focus.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.KeyEventArgs" />
        /// that contains data about the
        /// <see cref="E:System.Windows.UIElement.KeyDown" /> event.</param>
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
                case Key.None:
                    break;
                case Key.Cancel:
                    break;
                case Key.Back:
                    break;
                case Key.Tab:
                    break;
                case Key.LineFeed:
                    break;
                case Key.Clear:
                    break;
                case Key.Pause:
                    break;
                case Key.Capital:
                    break;
                case Key.KanaMode:
                    break;
                case Key.JunjaMode:
                    break;
                case Key.FinalMode:
                    break;
                case Key.HanjaMode:
                    break;
                case Key.ImeConvert:
                    break;
                case Key.ImeNonConvert:
                    break;
                case Key.ImeAccept:
                    break;
                case Key.ImeModeChange:
                    break;
                case Key.Space:
                    break;
                case Key.Prior:
                    break;
                case Key.Next:
                    break;
                case Key.End:
                    break;
                case Key.Home:
                    break;
                case Key.Left:
                    break;
                case Key.Right:
                    break;
                case Key.Select:
                    break;
                case Key.Print:
                    break;
                case Key.Execute:
                    break;
                case Key.Snapshot:
                    break;
                case Key.Insert:
                    break;
                case Key.Delete:
                    break;
                case Key.Help:
                    break;
                case Key.D0:
                    break;
                case Key.D1:
                    break;
                case Key.D2:
                    break;
                case Key.D3:
                    break;
                case Key.D4:
                    break;
                case Key.D5:
                    break;
                case Key.D6:
                    break;
                case Key.D7:
                    break;
                case Key.D8:
                    break;
                case Key.D9:
                    break;
                case Key.A:
                    break;
                case Key.B:
                    break;
                case Key.C:
                    break;
                case Key.D:
                    break;
                case Key.E:
                    break;
                case Key.F:
                    break;
                case Key.G:
                    break;
                case Key.H:
                    break;
                case Key.I:
                    break;
                case Key.J:
                    break;
                case Key.K:
                    break;
                case Key.L:
                    break;
                case Key.M:
                    break;
                case Key.N:
                    break;
                case Key.O:
                    break;
                case Key.P:
                    break;
                case Key.Q:
                    break;
                case Key.R:
                    break;
                case Key.S:
                    break;
                case Key.T:
                    break;
                case Key.U:
                    break;
                case Key.V:
                    break;
                case Key.W:
                    break;
                case Key.X:
                    break;
                case Key.Y:
                    break;
                case Key.Z:
                    break;
                case Key.LWin:
                    break;
                case Key.RWin:
                    break;
                case Key.Apps:
                    break;
                case Key.Sleep:
                    break;
                case Key.NumPad0:
                    break;
                case Key.NumPad1:
                    break;
                case Key.NumPad2:
                    break;
                case Key.NumPad3:
                    break;
                case Key.NumPad4:
                    break;
                case Key.NumPad5:
                    break;
                case Key.NumPad6:
                    break;
                case Key.NumPad7:
                    break;
                case Key.NumPad8:
                    break;
                case Key.NumPad9:
                    break;
                case Key.Multiply:
                    break;
                case Key.Add:
                    break;
                case Key.Separator:
                    break;
                case Key.Subtract:
                    break;
                case Key.Decimal:
                    break;
                case Key.Divide:
                    break;
                case Key.F1:
                    break;
                case Key.F2:
                    break;
                case Key.F3:
                    break;
                case Key.F4:
                    break;
                case Key.F5:
                    break;
                case Key.F6:
                    break;
                case Key.F7:
                    break;
                case Key.F8:
                    break;
                case Key.F9:
                    break;
                case Key.F10:
                    break;
                case Key.F11:
                    break;
                case Key.F12:
                    break;
                case Key.F13:
                    break;
                case Key.F14:
                    break;
                case Key.F15:
                    break;
                case Key.F16:
                    break;
                case Key.F17:
                    break;
                case Key.F18:
                    break;
                case Key.F19:
                    break;
                case Key.F20:
                    break;
                case Key.F21:
                    break;
                case Key.F22:
                    break;
                case Key.F23:
                    break;
                case Key.F24:
                    break;
                case Key.NumLock:
                    break;
                case Key.Scroll:
                    break;
                case Key.LeftShift:
                    break;
                case Key.RightShift:
                    break;
                case Key.LeftCtrl:
                    break;
                case Key.RightCtrl:
                    break;
                case Key.LeftAlt:
                    break;
                case Key.RightAlt:
                    break;
                case Key.BrowserBack:
                    break;
                case Key.BrowserForward:
                    break;
                case Key.BrowserRefresh:
                    break;
                case Key.BrowserStop:
                    break;
                case Key.BrowserSearch:
                    break;
                case Key.BrowserFavorites:
                    break;
                case Key.BrowserHome:
                    break;
                case Key.VolumeMute:
                    break;
                case Key.VolumeDown:
                    break;
                case Key.VolumeUp:
                    break;
                case Key.MediaNextTrack:
                    break;
                case Key.MediaPreviousTrack:
                    break;
                case Key.MediaStop:
                    break;
                case Key.MediaPlayPause:
                    break;
                case Key.LaunchMail:
                    break;
                case Key.SelectMedia:
                    break;
                case Key.LaunchApplication1:
                    break;
                case Key.LaunchApplication2:
                    break;
                case Key.Oem1:
                    break;
                case Key.OemPlus:
                    break;
                case Key.OemComma:
                    break;
                case Key.OemMinus:
                    break;
                case Key.OemPeriod:
                    break;
                case Key.Oem2:
                    break;
                case Key.Oem3:
                    break;
                case Key.AbntC1:
                    break;
                case Key.AbntC2:
                    break;
                case Key.Oem4:
                    break;
                case Key.Oem5:
                    break;
                case Key.Oem6:
                    break;
                case Key.Oem7:
                    break;
                case Key.Oem8:
                    break;
                case Key.Oem102:
                    break;
                case Key.ImeProcessed:
                    break;
                case Key.System:
                    break;
                case Key.OemAttn:
                    break;
                case Key.OemFinish:
                    break;
                case Key.OemCopy:
                    break;
                case Key.OemAuto:
                    break;
                case Key.OemEnlw:
                    break;
                case Key.OemBackTab:
                    break;
                case Key.Attn:
                    break;
                case Key.CrSel:
                    break;
                case Key.ExSel:
                    break;
                case Key.EraseEof:
                    break;
                case Key.Play:
                    break;
                case Key.Zoom:
                    break;
                case Key.NoName:
                    break;
                case Key.Pa1:
                    break;
                case Key.OemClear:
                    break;
                case Key.DeadCharProcessed:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.SelectorSelectionAdapter.Commit" />
        /// event.
        /// </summary>
        protected virtual void OnCommit()
        {
            OnCommit(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Fires the Commit event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnCommit(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler handler = Commit;
            handler?.Invoke(sender, e);

            AfterAdapterAction();
        }

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.SelectorSelectionAdapter.Cancel" />
        /// event.
        /// </summary>
        protected virtual void OnCancel()
        {
            OnCancel(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Fires the Cancel event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnCancel(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler handler = Cancel;
            handler?.Invoke(sender, e);

            AfterAdapterAction();
        }

        /// <summary>
        /// Change the selection after the actions are complete.
        /// </summary>
        private void AfterAdapterAction()
        {
            IgnoringSelectionChanged = true;
            if(SelectorControl != null)
            {
                SelectorControl.SelectedItem = null;
                SelectorControl.SelectedIndex = -1;
            }
            IgnoringSelectionChanged = false;
        }

        /// <summary>
        /// Returns an automation peer for the underlying
        /// <see cref="T:System.Windows.Controls.Primitives.Selector" />
        /// control, for use by the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>An automation peer for use by the Silverlight automation
        /// infrastructure.</returns>
        public AutomationPeer CreateAutomationPeer()
        {
            return _selector != null ? UIElementAutomationPeer.CreatePeerForElement(_selector) : null;
        }
    }
}
