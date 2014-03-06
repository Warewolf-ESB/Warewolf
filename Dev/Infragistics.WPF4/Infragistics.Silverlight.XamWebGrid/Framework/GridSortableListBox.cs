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
using System.Collections;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// A <see cref="ListBox"/> made of <see cref="GridSortableListBoxItem"/> objects, that can be reorderd.
    /// </summary>
    public class GridSortableListBox : ListBox, ISupportScrollHelper
    {
        #region Members

        ScrollViewer _scrollViewer;
        TouchScrollHelper _scrollHelper;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GridSortableListBox"/>
        /// </summary>
        public GridSortableListBox()
        {
            base.DefaultStyleKey = typeof(GridSortableListBox);
        }

        #endregion // Constructor

        #region Propertes

        #region Public

        #region IsSortable

        /// <summary>
        /// Identifies the <see cref="IsSortable"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsSortableProperty = DependencyProperty.Register("IsSortable", typeof(bool), typeof(GridSortableListBox), new PropertyMetadata(true, new PropertyChangedCallback(IsSortableChanged)));

        /// <summary>
        /// Gets/Sets whether items in the <see cref="GridSortableListBox"/> can be re-ordered via the UI.
        /// </summary>
        public bool IsSortable
        {
            get { return (bool)this.GetValue(IsSortableProperty); }
            set { this.SetValue(IsSortableProperty, value); }
        }

        private static void IsSortableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            GridSortableListBox gslb = (GridSortableListBox)obj;
            int count = gslb.Items.Count;

            for (int i = 0; i < count; i++)
            {
                GridSortableListBoxItem item = gslb.ItemContainerGenerator.ContainerFromIndex(i) as GridSortableListBoxItem;
                if (item != null)
                    item.IsMovable = gslb.IsSortable;
            }
        }

        #endregion // IsSortable

        #endregion // Public

        #region Internal

        internal GridSortableListBoxItem CurrentDragItem
        {
            get;
            set;
        }

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Internal

        internal void IncreaseScrollPosition()
        {
            if (this._scrollViewer != null)
                this._scrollViewer.ScrollToVerticalOffset(this._scrollViewer.VerticalOffset + 1);
        }

        internal void DecreaseScrollPosition()
        {
            if (this._scrollViewer != null)
                this._scrollViewer.ScrollToVerticalOffset(this._scrollViewer.VerticalOffset - 1);
        }

        #endregion // Internal

        #endregion // Methods

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Retrieves the ScrollViewer TemplatePart for scrolling logic.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._scrollViewer = base.GetTemplateChild("ScrollViewer") as ScrollViewer;

            this._scrollHelper = new TouchScrollHelper(this, this);


            this.SetCurrentValue(IsManipulationEnabledProperty, true);

        }
        #endregion // OnApplyTemplate

        #region GetContainerForItemOverride

        /// <summary>
        /// Returns a new <see cref="GridSortableListBoxItem"/>
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            if (this.CurrentDragItem == null)
            {
                return new GridSortableListBoxItem() { IsMovable = this.IsSortable };
            }
            else
            {
                return this.CurrentDragItem;
            }
        }
        #endregion // GetContainerForItemOverride

        #endregion // Overrides   
     
        #region ISupportScrollHelper Members

        ScrollType ISupportScrollHelper.VerticalScrollType
        {
            get { return ScrollType.Pixel; }
        }

        ScrollType ISupportScrollHelper.HorizontalScrollType
        {
            get { return ScrollType.Pixel; }
        }

        double ISupportScrollHelper.GetFirstItemHeight()
        {

            if (this._scrollViewer != null && this.Items.Count > 0)
            {
                GridSortableListBoxItem item = this.ItemContainerGenerator.ContainerFromIndex(0) as GridSortableListBoxItem;

                if (item != null)
                {
                    return item.DesiredSize.Height;
                }
            }

            return 0;
        }

        double ISupportScrollHelper.GetFirstItemWidth()
        {
            if (this._scrollViewer != null && this.Items.Count > 0)
            {
                GridSortableListBoxItem item = this.ItemContainerGenerator.ContainerFromIndex(0) as GridSortableListBoxItem;

                if (item != null)
                {
                    return item.DesiredSize.Width;
                }
            }

            return 0;
        }

        void ISupportScrollHelper.InvalidateScrollLayout()
        {
            
        }

        double ISupportScrollHelper.VerticalValue
        {
            get
            {                
                if (this._scrollViewer != null)
                    return this._scrollViewer.VerticalOffset;

                return 0;
            }
            set
            {
                if (this._scrollViewer != null)
                    this._scrollViewer.ScrollToVerticalOffset( value);
            }
        }

        double ISupportScrollHelper.VerticalMax
        {
            get
            {
                if (this._scrollViewer != null)
                    return this._scrollViewer.ExtentHeight;

                return 0;
            }
        }

        double ISupportScrollHelper.HorizontalValue
        {
            get
            {
                if (this._scrollViewer != null)
                    return this._scrollViewer.HorizontalOffset;

                return 0;
            }
            set
            {
                if (this._scrollViewer != null)
                    this._scrollViewer.ScrollToHorizontalOffset(  value);
            }
        }

        double ISupportScrollHelper.HorizontalMax
        {
            get
            {
                if (this._scrollViewer != null)
                    return this._scrollViewer.ScrollableWidth;

                return 0;
            }
        }

        TouchScrollMode ISupportScrollHelper.GetScrollModeFromPoint(Point point, UIElement elementDirectlyOver)
        {        
            return TouchScrollMode.Both;
        }

        void ISupportScrollHelper.OnStateChanged(TouchState newState, TouchState oldState)
        {
//#if SILVERLIGHT
//            switch (newState)
//            {
//                case TouchState.Pending:
//                    this._deferMouseClicks = true;
//                    break;
//                case TouchState.Holding:
//                    {
//                        break;
//                    }
//                case TouchState.NotDown:
//                    this._deferMouseClicks = false;
//                    break;
//                case TouchState.Scrolling:
//                    break;
//            }
//#endif
        }

        void ISupportScrollHelper.OnPanComplete()
        {
        }

        #endregion        
    }

    /// <summary>
    /// An draggable item generated in the <see cref="GridSortableListBox"/>
    /// </summary>
    public class GridSortableListBoxItem : ListBoxItem
    {
        #region Members

        FrameworkElement _dragHandle;
        bool _mouseDown = false, _isDragging = false;
        Point _offsetPoint, _mousePosition;
        Popup _popup;
        GridSortableListBox _parent;
        IList _itemsSource;
        DispatcherTimer _scrollListBoxTimer;
        GridSortableListBoxItem _dragItem;
        int _originalIndex;
        Cursor _originalCursor;
        Point _lastPoint;
        double _lastTrueItemWidth;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GridSortableListBoxItem"/>
        /// </summary>
        public GridSortableListBoxItem()
        {
            this._popup = new Popup();

            this._popup.Placement = PlacementMode.Relative;
            this._popup.AllowsTransparency = true;

            base.DefaultStyleKey = typeof(GridSortableListBoxItem);
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region IsMovable

        /// <summary>
        /// Identifies the <see cref="IsMovable"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsMovableProperty = DependencyProperty.Register("IsMovable", typeof(bool), typeof(GridSortableListBoxItem), new PropertyMetadata(new PropertyChangedCallback(IsMovableChanged)));

        /// <summary>
        /// Gets/Sets whether the particular item can be re-ordered.
        /// </summary>
        public bool IsMovable
        {
            get { return (bool)this.GetValue(IsMovableProperty); }
            set { this.SetValue(IsMovableProperty, value); }
        }

        private static void IsMovableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            GridSortableListBoxItem gslbi = (GridSortableListBoxItem)obj;

            if (gslbi._dragHandle != null)
            {
                if (gslbi.IsMovable)
                {
                    gslbi._dragHandle.Cursor = gslbi._originalCursor;
                }
                else
                {
                    gslbi._dragHandle.Cursor = Cursors.Arrow;
                }
            }
        }

        #endregion // IsMovable 				

        #endregion // Public

        #endregion // Properties

        #region Methods

        #region Private

        private void EndDrag()
        {
            if (this._mouseDown)
            {
                this._mouseDown = false;
                this._isDragging = false;
                this.Opacity = 1;
                this._popup.IsOpen = false;
                this.ReleaseMouseCapture();
                this._parent.CurrentDragItem = null;

                if (this._scrollListBoxTimer != null)
                    this._scrollListBoxTimer.Stop();

                if(this._dragItem != null)
                    this._dragItem.KeyDown -= GridSortableListBoxItem_KeyDown;
            }
            this.ReleaseMouseCapture();
        }

        #endregion // Private

        #endregion // Methods

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Retrieves the DragHandle element for a <see cref="GridSortableListBoxItem"/>
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this._dragHandle != null)
            {
                this._dragHandle.RemoveHandler(ListBoxItem.MouseLeftButtonDownEvent, new MouseButtonEventHandler(SortableListBoxItem_MouseLeftButtonDown));
            }

            this._dragHandle = base.GetTemplateChild("DragHandle") as FrameworkElement;

            if (this._dragHandle != null)
            {
                this._originalCursor = this._dragHandle.Cursor;
                this._dragHandle.AddHandler(ListBoxItem.MouseLeftButtonDownEvent, new MouseButtonEventHandler(SortableListBoxItem_MouseLeftButtonDown), true);

                if (!this.IsMovable)
                    this._dragHandle.Cursor = Cursors.Arrow;
            }
        }
        #endregion // OnApplyTemplate

        #region OnMouseLeftButtonUp

        /// <summary>
        /// Raised before the MouseLeftButtonUp event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            this.EndDrag();
        }
        #endregion // OnMouseLeftButtonUp

        #region OnMouseMove

        /// <summary>
        /// Raised before the MouseMove event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this._mouseDown)
            {
                this._mousePosition = e.GetPosition(null);

                if (!this._isDragging)
                {
                    this.CaptureMouse();
                    this._offsetPoint = e.GetPosition(this);
                    this._isDragging = true;

                    this.Opacity = 0;

                    this._dragItem = new GridSortableListBoxItem();
                    this._dragItem.Content = this.Content;
                    this._dragItem.ContentTemplate = this.ContentTemplate;

                    this._popup.Child = this._dragItem;

                    this._popup.IsOpen = true;

                    this._dragItem.Focus();

                    this._dragItem.KeyDown += new KeyEventHandler(GridSortableListBoxItem_KeyDown);

                    this._originalIndex = this._itemsSource.IndexOf(this.Content);

                    this._dragItem.FlowDirection = this.FlowDirection;

                    if (this._dragItem.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        this._offsetPoint.X *= -1;

                    this._lastPoint = e.GetPosition(this._parent);


                    double offsetX = this._offsetPoint.X;
                    if (SystemParameters.IsMenuDropRightAligned)
                    {
                        if (this._dragItem.ActualWidth != 0)
                        {
                            this._lastTrueItemWidth = this._dragItem.ActualWidth;
                        }
                        this._offsetPoint.X -= (this._lastTrueItemWidth);                        
                    }                    

                    this._popup.HorizontalOffset = (this._mousePosition.X - offsetX);



                }
                else
                {
                    bool insideListBox = false;

                    Point p = this._mousePosition;

                    p = e.GetPosition(this._parent);

                    if (p == this._lastPoint)
                        return;
                    else
                        this._lastPoint = p;

                    IEnumerable<UIElement> elements = PlatformProxy.GetElementsFromPoint(p, this._parent);
                    foreach (UIElement element in elements)
                    {
                        GridSortableListBoxItem target = element as GridSortableListBoxItem;
                        if (target != null && target != this && target != this._popup.Child)
                        {
                            object sourceObj = this.Content;
                            int sourceIndex = this._itemsSource.IndexOf(sourceObj);
                            int targetIndex = this._itemsSource.IndexOf(target.Content);
                            if (sourceIndex != -1 && targetIndex != -1)
                            {
                                ColumnBase col = sourceObj as ColumnBase;

                                if (col != null)
                                {
                                    col.IsMoving = true;
                                    col.ColumnLayout.Grid.SuspendConditionalFormatUpdates = true;
                                }

                                this._itemsSource.RemoveAt(sourceIndex);
                                this._itemsSource.Insert(targetIndex, sourceObj);

                                if (col != null)
                                {
                                    col.IsMoving = false;
                                    col.ColumnLayout.Grid.SuspendConditionalFormatUpdates = false;
                                }

                                insideListBox = true;
                                break;
                            }
                        }

                        if (element == this)
                        {
                            insideListBox = true;
                        }

                    }

                    if (!insideListBox)
                    {
                        if (this._scrollListBoxTimer == null)
                        {
                            this._scrollListBoxTimer = new DispatcherTimer();
                            this._scrollListBoxTimer.Tick += new EventHandler(ScrollListBoxTimer_Tick);
                            this._scrollListBoxTimer.Interval = TimeSpan.FromMilliseconds(0);
                        }

                        this._scrollListBoxTimer.Start();
                    }
                    else
                    {
                        if (this._scrollListBoxTimer != null)
                            this._scrollListBoxTimer.Stop();
                    }

                    this._popup.HorizontalOffset = (this._mousePosition.X - this._offsetPoint.X);
                }

                
                this._popup.VerticalOffset = (this._mousePosition.Y - this._offsetPoint.Y);
            }
        }
               
        #endregion // OnMouseMove

        #region OnLostMouseCapture
        /// <summary>
        /// Called before the LostMouseCapture event is raised.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            // So, if a user was to right click, and the SL menu is invoked
            // it could steal focus from the mouse capture, and thus, we might not get notified of 
            // the mouse up, so we get stuck in limbo where we think the drag is still occuring, but its not. 




            base.OnLostMouseCapture(e);
        }
        #endregion // OnLostMouseCapture

        #endregion // Overrides

        #region EventHandlers

        #region SortableListBoxItem_MouseLeftButtonDown

        void SortableListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this._popup != null && this.IsMovable)
            {
                if (this._parent == null)
                {
                    DependencyObject parent = this;
                    while (parent != null && this._parent == null)
                    {
                        parent = VisualTreeHelper.GetParent(parent);

                        this._parent = parent as GridSortableListBox;
                    }

                    if (this._parent != null)
                    {
                        this._itemsSource = this._parent.ItemsSource as IList;

                        UIElement rootParent = this._parent;
                        DependencyObject temp = rootParent;
                        while (temp != null)
                        {
                            UIElement elem = temp as UIElement;
                            if(elem != null)
                                rootParent = elem;

                            temp = PlatformProxy.GetParent(temp, false);
                        }

                        this._popup.PlacementTarget = rootParent;

                    }
                }

                if (this._itemsSource != null)
                {
                    this._mouseDown = this.CaptureMouse();

                    if (this._mouseDown)
                        this._parent.CurrentDragItem = this;
                }
            }
        }


        #endregion // SortableListBoxItem_MouseLeftButtonDown

        #region ScrollListBoxTimer_Tick
        private void ScrollListBoxTimer_Tick(object sender, EventArgs e)
        {
            try
            {





                Rect r = LayoutInformation.GetLayoutSlot(this._parent);


                double zoom = Infragistics.PlatformProxy.GetZoomFactor();

                double y = this._mousePosition.Y * zoom;

                if (y < r.Y)
                {
                    this._parent.DecreaseScrollPosition();
                }
                else if (y > (r.Y + r.Height))
                {
                    this._parent.IncreaseScrollPosition();
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion //ScrollListBoxTimer_Tick

        #region GridSortableListBoxItem_KeyDown
        void GridSortableListBoxItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (this._isDragging && e.Key == Key.Escape)
            {
                object sourceObj = this.Content;
                int sourceIndex = this._itemsSource.IndexOf(sourceObj);
                int targetIndex = this._originalIndex;
                if (sourceIndex != -1 && targetIndex != -1 && sourceIndex != targetIndex)
                {
                    ColumnBase col = sourceObj as ColumnBase;

                    if (col != null)
                        col.IsMoving = true;

                    this._itemsSource.RemoveAt(sourceIndex);
                    this._itemsSource.Insert(targetIndex, sourceObj);

                    if (col != null)
                        col.IsMoving = false;
                }

                this.EndDrag();
            }
        }
        #endregion // GridSortableListBoxItem_KeyDown

        #endregion // EventHandlers
    }
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