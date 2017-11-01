/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls
{
    internal class PopupHelper
    {
#if SILVERLIGHT
        private bool _hasControlLoaded;
#endif
        public bool UsesClosingVisualState { get; private set; }
        
        private Control Parent { get; set; }

#if SILVERLIGHT
        private Canvas OutsidePopupCanvas { get; set; }
        
        private Canvas PopupChildCanvas { get; set; }
#endif
        
        public double MaxDropDownHeight { get; set; }
        
        public Popup Popup { get; private set; }
        
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Provided for completeness.")]
        public bool IsOpen
        {
            get { return Popup.IsOpen; }
            set { Popup.IsOpen = value; }
        }
        
        private FrameworkElement PopupChild { get; set; }
        
        public event EventHandler Closed;
        
        public event EventHandler FocusChanged;
        
        public event EventHandler UpdateVisualStates;
        
        public PopupHelper(Control parent)
        {
            Debug.Assert(parent != null, "Parent should not be null.");
            Parent = parent;
        }
        
        public PopupHelper(Control parent, Popup popup)
            : this(parent)
        {
            Popup = popup;
        }
        
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This try-catch pattern is used by other popup controls to keep the runtime up.")]
        public void Arrange()
        {
            if(Popup == null
                || PopupChild == null
#if SILVERLIGHT
 || OutsidePopupCanvas == null
#endif
 || Application.Current == null
)
            
            {
                return;
            }

#if SILVERLIGHT
            Content hostContent = Application.Current.Host.Content;
            double rootWidth = hostContent.ActualWidth;
            double rootHeight = hostContent.ActualHeight;
#else
            UIElement u = Parent;
            if(Application.Current.CheckAccess() && Application.Current.Windows.Count > 0)
            {                
                u = Application.Current.Windows[0];
            }
            while((!(u is Window)) && u != null)
            {
                u = VisualTreeHelper.GetParent(u) as UIElement;
            }
            Window w = u as Window;
            if(w == null)
            {
                return;
            }

            double rootWidth = w.ActualWidth;
            double rootHeight = w.ActualHeight;
#endif

            double popupContentWidth = PopupChild.ActualWidth;
            double popupContentHeight = PopupChild.ActualHeight;
                        
            if(rootHeight.Equals(0) || rootWidth.Equals(0) || popupContentWidth.Equals(0) || popupContentHeight.Equals(0))
            {
                return;
            }

            const double rootOffsetX = 0;
            const double rootOffsetY = 0;

            double myControlHeight = Parent.ActualHeight;
            double myControlWidth = Parent.ActualWidth;
            
            double popupMaxHeight = MaxDropDownHeight;
            if(double.IsInfinity(popupMaxHeight) || double.IsNaN(popupMaxHeight))
            {
                popupMaxHeight = (rootHeight - myControlHeight) * 3 / 5;
            }

            popupContentWidth = Math.Min(popupContentWidth, rootWidth);
            popupContentHeight = Math.Min(popupContentHeight, popupMaxHeight);
            popupContentWidth = Math.Max(myControlWidth, popupContentWidth);
            
            double popupX = rootOffsetX;
            if(rootWidth < popupX + popupContentWidth)
            {
                popupX = rootWidth - popupContentWidth;
                popupX = Math.Max(0, popupX);
            }
            
            bool below = true;
            double popupY = rootOffsetY + myControlHeight;
            if(rootHeight < popupY + popupContentHeight)
            {
                below = false;
                popupY = rootOffsetY - popupContentHeight;
                if(popupY < 0)
                {
                    if(rootOffsetY < (rootHeight - myControlHeight) / 2)
                    {
                        below = true;
                        popupY = rootOffsetY + myControlHeight;
                    }
                    else
                    {
                        popupY = rootOffsetY - popupContentHeight;
                    }
                }
            }
            popupMaxHeight = below ? Math.Min(rootHeight - popupY, popupMaxHeight) : Math.Min(rootOffsetY, popupMaxHeight);

            Popup.HorizontalOffset = 0;
            Popup.VerticalOffset = 0;

#if SILVERLIGHT
            OutsidePopupCanvas.Width = rootWidth;
            OutsidePopupCanvas.Height = rootHeight;

            // Transform the transparent canvas to the plugin's coordinate 
            // space origin.
            Matrix transformToRootMatrix = mt.Matrix;
            Matrix newMatrix;
            transformToRootMatrix.Invert(out newMatrix);
            mt.Matrix = newMatrix;

            OutsidePopupCanvas.RenderTransform = mt;
#endif
            PopupChild.MinWidth = myControlWidth;
            PopupChild.MaxWidth = rootWidth;
            PopupChild.MinHeight = 0;
            PopupChild.MaxHeight = Math.Max(0, popupMaxHeight);

            PopupChild.Width = popupContentWidth;
            PopupChild.HorizontalAlignment = HorizontalAlignment.Left;
            PopupChild.VerticalAlignment = VerticalAlignment.Top;
            
            Canvas.SetLeft(PopupChild, popupX - rootOffsetX);
            Canvas.SetTop(PopupChild, popupY - rootOffsetY);
        }
        
        private void OnClosed(EventArgs e)
        {
            EventHandler handler = Closed;
            handler?.Invoke(this, e);
        }
        
        private void OnPopupClosedStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if(e?.NewState != null && e.NewState.Name == VisualStates.StatePopupClosed)
            {
                if(Popup != null)
                {
                    Popup.IsOpen = false;
                }
                OnClosed(EventArgs.Empty);
            }
        }
        
        public void BeforeOnApplyTemplate()
        {
            if(UsesClosingVisualState)
            {
                VisualStateGroup groupPopupClosed = VisualStates.TryGetVisualStateGroup(Parent, VisualStates.GroupPopup);
                if(null != groupPopupClosed)
                {
                    groupPopupClosed.CurrentStateChanged -= OnPopupClosedStateChanged;
                    UsesClosingVisualState = false;
                }
            }

            if(Popup != null)
            {
                Popup.Closed -= Popup_Closed;
            }
        }
        
        public void AfterOnApplyTemplate()
        {
            if(Popup != null)
            {
                Popup.Closed += Popup_Closed;
            }

            VisualStateGroup groupPopupClosed = VisualStates.TryGetVisualStateGroup(Parent, VisualStates.GroupPopup);
            if(null != groupPopupClosed)
            {
                groupPopupClosed.CurrentStateChanged += OnPopupClosedStateChanged;
                UsesClosingVisualState = true;
            }
            if(Popup != null)
            {
                PopupChild = Popup.Child as FrameworkElement;

                if(PopupChild != null)
                {
#if SILVERLIGHT
                    // For Silverlight only, we just create the popup child with 
                    // canvas a single time.
                    if (!_hasControlLoaded)
                    {
                        _hasControlLoaded = true;

                        // Replace the poup child with a canvas
                        PopupChildCanvas = new Canvas();
                        Popup.Child = PopupChildCanvas;

                        OutsidePopupCanvas = new Canvas();
                        OutsidePopupCanvas.Background = new SolidColorBrush(Colors.Transparent);
                        OutsidePopupCanvas.MouseLeftButtonDown += OutsidePopup_MouseLeftButtonDown;

                        PopupChildCanvas.Children.Add(OutsidePopupCanvas);
                        PopupChildCanvas.Children.Add(PopupChild);
                    }
#endif

                    PopupChild.GotFocus += PopupChild_GotFocus;
                    PopupChild.LostFocus += PopupChild_LostFocus;
                    PopupChild.MouseEnter += PopupChild_MouseEnter;
                    PopupChild.MouseLeave += PopupChild_MouseLeave;
                    PopupChild.SizeChanged += PopupChild_SizeChanged;
                }
            }
        }
        
        private void PopupChild_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Arrange();
        }
        
        private void Popup_Closed(object sender, EventArgs e)
        {
            OnClosed(EventArgs.Empty);
        }
        
        private void OnFocusChanged(EventArgs e)
        {
            EventHandler handler = FocusChanged;
            handler?.Invoke(this, e);
        }
        
        private void OnUpdateVisualStates(EventArgs e)
        {
            EventHandler handler = UpdateVisualStates;
            handler?.Invoke(this, e);
        }
        
        private void PopupChild_GotFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChanged(EventArgs.Empty);
        }
        
        private void PopupChild_LostFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChanged(EventArgs.Empty);
        }
        
        private void PopupChild_MouseEnter(object sender, MouseEventArgs e)
        {
            OnUpdateVisualStates(EventArgs.Empty);
        }
        
        private void PopupChild_MouseLeave(object sender, MouseEventArgs e)
        {
            OnUpdateVisualStates(EventArgs.Empty);
        }
    }
}
