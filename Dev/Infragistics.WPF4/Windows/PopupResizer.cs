using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Infragistics.Controls.Primitives
{
    /// <summary>
    /// A control that will resize a specified popup.
    /// </summary>

	[System.ComponentModel.DesignTimeVisible(false)]

    public class PopupResizer : Control
    {
        #region Members

 
        FrameworkElement _resizeElem;
        bool _isMouseDown, _isResizing;
        Point _offsetPoint;
        Size _originalSize;
        double _originalTop;

        #endregion // Members

        #region Constructor


        static PopupResizer()
        {
            Style style = new Style();
            style.Seal();
            Control.FocusVisualStyleProperty.OverrideMetadata(typeof(PopupResizer), new FrameworkPropertyMetadata(style));
        }


        /// <summary>
        /// Instantiates a new instance of the <see cref="PopupResizer"/>
        /// </summary>
        public PopupResizer()
        {
            base.DefaultStyleKey = typeof(PopupResizer);
            this.IsTabStop = false;
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region ResizeElement

        /// <summary>
        /// Identifies the <see cref="ResizeElement"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ResizeElementProperty = DependencyProperty.Register("ResizeElement", typeof(FrameworkElement), typeof(PopupResizer), new PropertyMetadata(new PropertyChangedCallback(ResizeElementChanged)));

        /// <summary>
        /// The FrameworkElement that should be resized.
        /// </summary>
        public FrameworkElement ResizeElement
        {
            get { return (FrameworkElement)this.GetValue(ResizeElementProperty); }
            set { this.SetValue(ResizeElementProperty, value); }
        }

        private static void ResizeElementChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // Popup 

        #region Popup

        /// <summary>
        /// Identifies the <see cref="Popup"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty PopupProperty = DependencyProperty.Register("Popup", typeof(Popup), typeof(PopupResizer), new PropertyMetadata(new PropertyChangedCallback(PopupChanged)));

        /// <summary>
        /// Gets/Sets the popup that is associated with the resizer
        /// </summary>
        /// <remarks>Only needed for Above style resizing</remarks>
        public Popup Popup
        {
            get { return (Popup)this.GetValue(PopupProperty); }
            set { this.SetValue(PopupProperty, value); }
        }

        private static void PopupChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // Popup 
		
        #region Position

        /// <summary>
        /// Identifies the <see cref="Position"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(ResizerPosition), typeof(PopupResizer), new PropertyMetadata(ResizerPosition.Below, new PropertyChangedCallback(PositionChanged)));

        /// <summary>
        /// Gets/Sets the position of the resizer. In other words, whether is above or below the content it's resizing.
        /// </summary>
        public ResizerPosition Position
        {
            get { return (ResizerPosition)this.GetValue(PositionProperty); }
            set { this.SetValue(PositionProperty, value); }
        }

        private static void PositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            PopupResizer resizer = (PopupResizer)obj;
            resizer.EnsureVisualStates();
        }

        #endregion // Position 
				
        #endregion // Public

        #endregion // Properties

        #region Methods

        #region Protected

        #region EndResize

        /// <summary>
        /// Ends the resizing.
        /// </summary>
        protected virtual void EndResize(bool cancel)
        {
            this._isMouseDown = false;
            this._isResizing = false;

            this._resizeElem.ReleaseMouseCapture();



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


            if (cancel)
            {
                if (this.ResizeElement != null)
                {
                    this.ResizeElement.Height = this._originalSize.Height;
                    this.ResizeElement.Width = this._originalSize.Width;
                }
            }
        }

        #endregion // EndResize

        #region EnsureVisualStates

        /// <summary>
        /// Ensures that GotToState is raised on all the property VisualStates.
        /// </summary>
        protected virtual void EnsureVisualStates()
        {
            bool above = (this.Position == ResizerPosition.Above);

            if (above)
                VisualStateManager.GoToState(this, "Above", false);
            else
                VisualStateManager.GoToState(this, "Below", false);

            if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight)
            {
                if (above)
                    VisualStateManager.GoToState(this, "AboveLTR", false);
                else
                    VisualStateManager.GoToState(this, "LTR", false);
            }
            else
            {
                if (above)
                    VisualStateManager.GoToState(this, "AboveRTL", false);
                else
                    VisualStateManager.GoToState(this, "RTL", false);
            }
        }
        #endregion // EnsureVisualStates

        #endregion // Protected

        #endregion // Methods

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Builds the tree the of the <see cref="PopupResizer"/>
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this._resizeElem != null)
            {
                this._resizeElem.LostMouseCapture -= ResizableElement_LostMouseCapture;
                this._resizeElem.MouseLeftButtonUp -= ResizableElement_MouseLeftButtonUp;
                this._resizeElem.MouseLeftButtonDown -= ResizableElement_MouseLeftButtonDown;
                this._resizeElem.MouseMove -= ResizableElement_MouseMove;
            }

            this._resizeElem = base.GetTemplateChild("ResizeElem") as FrameworkElement;
            if (this._resizeElem != null)
            {
                this._resizeElem.LostMouseCapture += new MouseEventHandler(ResizableElement_LostMouseCapture);
                this._resizeElem.MouseLeftButtonUp += new MouseButtonEventHandler(ResizableElement_MouseLeftButtonUp);
                this._resizeElem.MouseLeftButtonDown += new MouseButtonEventHandler(ResizableElement_MouseLeftButtonDown);
                this._resizeElem.MouseMove += new MouseEventHandler(ResizableElement_MouseMove);
            }

            this.EnsureVisualStates();
        }

        #endregion // OnApplyTemplate

        #endregion // Overrides

        #region EventHandlers

        private void ResizableElement_LostMouseCapture(object sender, MouseEventArgs e)
        {
            this.EndResize(false);
        }

        private void ResizableElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.EndResize(false);
        }

        private void ResizableElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.ResizeElement != null)
            {
                this._isMouseDown = ((UIElement)sender).CaptureMouse();
                e.Handled = this._isMouseDown;
            }
        }

        private void ResizableElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (this._isMouseDown)
            {
                if (!this._isResizing)
                {
                    double height = 0, width = 0; 

                    height = this.ResizeElement.ActualHeight;
                    width = this.ResizeElement.ActualWidth;

                    if (double.IsNaN(width) || double.IsNaN(height) || (width == 0 && height == 0))
                        return;

                    this._isResizing = true;

                    this._offsetPoint = e.GetPosition(PlatformProxy.GetRootVisual(this.ResizeElement));
                    this._originalSize = new Size(width, height);

                    if (this.Popup != null)
                    {
                        this._originalTop = this.Popup.VerticalOffset;
                    }
                }

                Point elemPoint = e.GetPosition(PlatformProxy.GetRootVisual(this.ResizeElement));

                double x = elemPoint.X - this._offsetPoint.X;
                double y = elemPoint.Y - this._offsetPoint.Y;

                if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                    x *= -1;

                Point diff = new Point(x, y);

                Size newSize = new Size();

                newSize.Width = Math.Max(this._originalSize.Width - diff.X, this.MinWidth);
                newSize.Width = Math.Min(newSize.Width, this.MaxWidth);

                newSize.Height = Math.Max(this._originalSize.Height - diff.Y, this.MinHeight);
                newSize.Height = Math.Min(newSize.Height, this.MaxHeight);

                Size invertedSize = new Size();

                invertedSize.Width = Math.Max(this._originalSize.Width + diff.X, this.MinWidth);
                invertedSize.Width = Math.Min(invertedSize.Width, this.MaxWidth);

                invertedSize.Height = Math.Max(this._originalSize.Height + diff.Y, this.MinHeight);
                invertedSize.Height = Math.Min(invertedSize.Height, this.MaxHeight);

                // Update Diff, now that we've updated for max and min.
                diff = new Point(this._originalSize.Width - newSize.Width, this._originalSize.Height - newSize.Height);

                Point newPoint = new Point();

                this.ResizeElement.Width = invertedSize.Width;

                if (this.Position == ResizerPosition.Above)
                {
                    if (this.Popup != null)
                    {
                        if (newSize.Height != this.ResizeElement.MinHeight)
                        {
                            newPoint.Y = this._originalTop + diff.Y;

                            if (newPoint.Y == 0)
                            {
                                newSize.Height = this.ResizeElement.ActualHeight;
                            }
                            else if (newSize.Height < this.ResizeElement.MinHeight)
                            {
                                newSize.Height = this.ResizeElement.MinHeight;
                                double difftoRemove = this.ResizeElement.MinHeight - (this._originalSize.Height - diff.Y);
                                newPoint.Y = (this._originalTop + diff.Y) - difftoRemove;
                            }
                        }
                        else
                        {
                            double difftoRemove = this.ResizeElement.MinHeight - (this._originalSize.Height - diff.Y);
                            newPoint.Y = (this._originalTop + diff.Y) - difftoRemove;
                        }
                    }

                    this.Popup.VerticalOffset = newPoint.Y;
                    this.ResizeElement.Height = newSize.Height;
                }
                else
                {
                    this.ResizeElement.Height = invertedSize.Height;
                }
            }
        }

        #endregion // EventHandlers

    }

    #region ResizerPosition

    /// <summary>
    /// Describes whether the Resizer is above or below the content its resizing.
    /// </summary>
    public enum ResizerPosition
    {
        /// <summary>
        /// The Resizer is above its content
        /// </summary>
        Above,

        /// <summary>
        /// The Resizer is below its content.
        /// </summary>
        Below
    }

    #endregion // ResizerPosition
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved