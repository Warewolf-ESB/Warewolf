using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;







namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// An object that describes base class for the slider thumb
    /// </summary>
    [TemplateVisualState(GroupName = "OrientationStates", Name = "Horizontal")]
    [TemplateVisualState(GroupName = "OrientationStates", Name = "Vertical")]
    [TemplateVisualState(GroupName = "FocusStates", Name = "Focused")]
    [TemplateVisualState(GroupName = "FocusStates", Name = "Unfocused")]
    [TemplateVisualState(GroupName = "ActiveStates", Name = "Active")]
    [TemplateVisualState(GroupName = "ActiveStates", Name = "Inactive")]
    [TemplateVisualState(GroupName = "HoverStates", Name = "NoHover")]
    [TemplateVisualState(GroupName = "HoverStates", Name = "Hover")]






    public abstract class XamSliderThumbBase : Control, INotifyPropertyChanged
    {
        #region Members

        private Point _origin;
        private Point _previousPosition;
        private Point _startPosition;

        #endregion Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XamSliderThumbBase"/> class.
        /// </summary>
        protected XamSliderThumbBase()
        {
            this.DefaultStyleKey = typeof(XamSliderThumbBase);
            this.LayoutUpdated += this.XamSliderThumbBase_LayoutUpdated;
            this.IsRestricted = false;
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(XamSliderThumbBase_IsEnabledChanged);



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion Constructor

        #region Events

        /// <summary>
        /// Occurs when mouse is start to drag the <see cref="XamSliderThumbBase"/> object.
        /// </summary>
        public event DragStartedEventHandler DragStarted;

        /// <summary>
        /// Occurs when mouse is dragging the <see cref="XamSliderThumbBase"/> object.
        /// </summary>
        public event DragDeltaEventHandler DragDelta;

        /// <summary>
        /// Occurs when mouse is complete to drag the <see cref="XamSliderThumbBase"/> object.
        /// </summary>
        public event DragCompletedEventHandler DragCompleted;

        #endregion Events

        #region Properties

        #region Public

        #region InteractionMode

        /// <summary>
        /// Identifies the <see cref="InteractionMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InteractionModeProperty = DependencyProperty.Register("InteractionMode", typeof(SliderThumbInteractionMode), typeof(XamSliderThumbBase), new PropertyMetadata(SliderThumbInteractionMode.Free));

        /// <summary>
        /// Gets or sets the interaction mode
        /// of the <see cref="XamSliderThumbBase"/>.
        /// </summary>
        /// <value>The interaction mode.</value>
        public SliderThumbInteractionMode InteractionMode
        {
            get { return (SliderThumbInteractionMode)this.GetValue(InteractionModeProperty); }
            set { this.SetValue(InteractionModeProperty, value); }
        }

        #endregion InteractionMode

        #region IsActive

        /// <summary>
        /// Identifies the <see cref="IsActive"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(XamSliderThumbBase), new PropertyMetadata(new PropertyChangedCallback(IsActiveChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether this instance
        /// is in active state.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return (bool)this.GetValue(IsActiveProperty); }
            set { this.SetValue(IsActiveProperty, value); }
        }

        private static void IsActiveChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderThumbBase thumb = obj as XamSliderThumbBase;
            if (thumb != null)
            {
                thumb.OnIsActiveChanged((bool)e.OldValue, (bool)e.NewValue);
                thumb.EnsureCurrentState();
            }
        }

        #endregion IsActive

        #region IsDragging

        /// <summary>
        /// Gets or sets a value indicating whether this instance is dragging.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is dragging; otherwise, <c>false</c>.
        /// </value>
        internal bool IsDragging
        {
            get;
            set;
        }

        #endregion IsDragging

        #region IsRestricted

        /// <summary>
        /// Gets or sets a value indicating whether this instance is restricted
        /// with a thumb wirth InteractionMode.Lock.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is restricted; otherwise, <c>false</c>.
        /// </value>
        internal bool IsRestricted
        {
            get;
            set;
        }

        #endregion IsRestricted

        #region Owner

        /// <summary>
        /// Gets or sets the <see cref="XamSliderBase"/>, that is owner
        /// of the thumb
        /// </summary>
        /// <value>The owner.</value>
        public XamSliderBase Owner
        {
            get;
            protected internal set;
        }

        #endregion Owner

        #endregion Public

        #region Internal

        #region SliderThumbIndex

        internal int SliderThumbIndex { get; set; }

        #endregion // SliderThumbIndex

        #endregion Internal

        #region Protected

        /// <summary>
        /// Gets or sets the thumb default ToolTip.
        /// </summary>
        /// <value>The thumb tool tip.</value>
        protected ToolTip ThumbToolTip { get; set; }

        /// <summary>
        /// Gets / sets if the mouse is currently over the thumb.
        /// </summary>
        protected bool IsOver { get; set; }



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


        #endregion Protected

        #endregion Properties

        #region Methods

        #region Public

        /// <summary>
        /// Cancels the dragging.
        /// </summary>
        public void CancelDrag()
        {
            if (this.IsDragging)
            {
                this.IsDragging = false;
                this.RaiseDragCompleted(true);
            }
        }

        #region IsDragEnabled

        /// <summary>
        /// Identifies the <see cref="IsDragEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDragEnabledProperty = DependencyProperty.Register("IsDragEnabled", typeof(bool), typeof(XamSliderThumbBase), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether this instance is draggable or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is drag enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDragEnabled
        {
            get { return (bool)this.GetValue(IsDragEnabledProperty); }
            set { this.SetValue(IsDragEnabledProperty, value); }
        }

        #endregion IsDragEnabled

        #region ToolTipTemplate

        /// <summary>
        /// Identifies the <see cref="ToolTipTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolTipTemplateProperty = DependencyProperty.Register("ToolTipTemplate", typeof(DataTemplate), typeof(XamSliderThumbBase), null);

        /// <summary>
        /// Gets or sets the ContentTemplate of the ToolTip .
        /// </summary>
        /// <value>The tool tip template.</value>
        public DataTemplate ToolTipTemplate
        {
            get { return (DataTemplate)this.GetValue(ToolTipTemplateProperty); }
            set { this.SetValue(ToolTipTemplateProperty, value); }
        }

        #endregion ToolTipTemplate

        #region ToolTipVisibility

        /// <summary>
        /// Identifies the <see cref="ToolTipVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolTipVisibilityProperty = DependencyProperty.Register("ToolTipVisibility", typeof(Visibility), typeof(XamSliderThumbBase), new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets or sets the Visibility of the ToolTip .
        /// </summary>
        /// <value>The tool tip Visibility.</value>
        public Visibility ToolTipVisibility
        {
            get { return (Visibility)this.GetValue(ToolTipVisibilityProperty); }
            set { this.SetValue(ToolTipVisibilityProperty, value); }
        }

        #endregion ToolTipVisibility

        #endregion Public

        #region Protected

        #region EnsureCurrentState

        /// <summary>
        /// Ensures VisualStateManager current state when
        /// the owner of the control is in Horizontal or
        /// Vertical state
        /// </summary>
        protected virtual void EnsureCurrentState()
        {
            if (this.Owner != null)
            {
                if (this.Owner.Orientation == Orientation.Horizontal)
                {
                    VisualStateManager.GoToState(this, "Horizontal", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Vertical", false);
                }

                if (this.IsActive)
                {
                    VisualStateManager.GoToState(this, "Active", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Inactive", false);
                }

                if (this.IsOver && this.IsEnabled)
                {
                    VisualStateManager.GoToState(this, "Hover", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "NoHover", false);
                }

                if (IsEnabled)
                {
                    VisualStateManager.GoToState(this, "Enabled", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Disabled", false);
                }
            }
        }

        #endregion EnsureCurrentState

        #region RaiseDragCompleted

        /// <summary>
        /// Raises the DragCompleted event.
        /// </summary>
        /// <param name="canceled">if set to <c>true</c> [canceled].</param>
        protected void RaiseDragCompleted(bool canceled)
        {
            this.OnDragCompleted(this._previousPosition.X - this._origin.X, this._previousPosition.Y - this._origin.Y, canceled);
        }

        #endregion RaiseDragCompleted

        #region OnDragCompleted

        /// <summary>
        /// Called when DragCompleted event is raised.
        /// </summary>
        /// <param name="horizontalChange">The horizontal change.</param>
        /// <param name="verticalChange">The vertical change.</param>
        /// <param name="canceled">if set to <c>true</c> [cancelled].</param>
        protected virtual void OnDragCompleted(double horizontalChange, double verticalChange, bool canceled)
        {
            DragCompletedEventHandler dragCompleted = this.DragCompleted;
            if (dragCompleted != null)
            {
                DragCompletedEventArgs e = new DragCompletedEventArgs(horizontalChange, verticalChange, canceled);
                dragCompleted(this, e);
            }
        }

        #endregion OnDragCompleted

        #region OnDragDelta

        /// <summary>
        /// Called when DragDelta event is raised.
        /// </summary>
        /// <param name="horizontalChange">The horizontal change.</param>
        /// <param name="verticalChange">The verticsal change.</param>
        protected virtual void OnDragDelta(double horizontalChange, double verticalChange)
        {
            DragDeltaEventHandler dragDelta = this.DragDelta;
            if (dragDelta != null)
            {
                dragDelta(this, new DragDeltaEventArgs(horizontalChange, verticalChange));
            }
        }

        #endregion OnDragDelta

        #region OnDragStarted

        /// <summary>
        /// Called when DragStarted event is raised.
        /// </summary>
        /// <param name="horizontalOffset">The horizontal offset.</param>
        /// <param name="verticalOffset">The verticsal offset.</param>
        protected virtual void OnDragStarted(double horizontalOffset, double verticalOffset)
        {
            DragStartedEventHandler dragStarted = this.DragStarted;
            if (dragStarted != null)
            {
                dragStarted(this, new DragStartedEventArgs(horizontalOffset, verticalOffset));
            }
        }

        #endregion OnDragStarted

        #region OnIsActiveChanged

        /// <summary>
        /// Called when IsActive property value is changed.
        /// </summary>
        /// <param name="oldValue">if set to <c>true</c> The old value.</param>
        /// <param name="newValue">if set to <c>true</c> The new value.</param>
        protected virtual void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            this.OnPropertyChanged("IsActive");
        }

        #endregion OnIsActiveChanged

        #endregion Protected

        #region Private

        #region CaptureMouse

        private void CaptureMouse(Point position)
        {
            if (this.IsRestricted)
            {
                double minleft = Canvas.GetLeft(this);
                double maxleft = minleft + this.ActualWidth;
                double mintop = Canvas.GetTop(this);
                double maxtop = mintop + this.ActualHeight;
                if (Owner.Orientation == Orientation.Horizontal)
                {
                    if (position.X < maxleft && position.X > minleft)
                    {
                        this.IsRestricted = false;
                        _previousPosition.X = (minleft + maxleft) / 2.0;
                    }
                }
                else if (Owner.Orientation == Orientation.Vertical)
                {
                    if (position.Y < maxtop && position.Y > mintop)
                    {
                        this.IsRestricted = false;
                        _previousPosition.Y = (mintop + maxtop) / 2.0;
                    }
                }
            }
        }

        #endregion CaptureMouse

        #region GotFocusImplementation

        private void GotFocusImplementation()
        {




            VisualStateManager.GoToState(this, "Focused", true);
        }

        #endregion GotFocusImplementation

        #region LostFocusImplementation

        private void LostFocusImplementation()
        {
            VisualStateManager.GoToState(this, "Unfocused", true);
        }

        #endregion LostFocusImplementation

        #region MouseEnterImplementation

        private void MouseEnterImplementation(Point position)
        {
            if (!this.IsDragging)
                this.IsOver = true;

            if (this.IsRestricted)
            {
                this._origin = this._previousPosition = this._startPosition = position;
                this.IsRestricted = false;
            }

            if (this.IsEnabled)
            {
                this.EnsureCurrentState();
            }
        }

        #endregion MouseEnterImplementation

        #region MouseLeftButtonDownImplementation

        private bool MouseLeftButtonDownImplementation(Point position, bool handled)
        {

            this.IsOver = false;
            this.Focus();

            if (!handled && this.IsDragEnabled && (!this.IsDragging && this.IsEnabled))
            {
                handled = true;
                this.CaptureMouse();
                this.IsDragging = true;
                this._origin = this._previousPosition = this._startPosition = position;
                bool flag = false;
                try
                {
                    this.OnDragStarted(this._origin.X, this._origin.Y);
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.CancelDrag();
                    }
                }
            }
            return handled;
        }

        #endregion MouseLeftButtonDownImplementation

        #region MouseLeftButtonUpImplementation

        private bool MouseLeftButtonUpImplementation(Point position, bool handled)
        {
            this.IsOver = true;
            if (!handled && this.IsDragEnabled && (this.IsDragging && this.IsEnabled))
            {
                this._origin = position;
                if (!this._origin.Equals(this._startPosition))
                {
                    handled = true;
                }

                this.IsDragging = false;
                this.ReleaseMouseCapture();
                this.RaiseDragCompleted(false);
            }
            return handled;
        }

        #endregion MouseLeftButtonUpImplementation

        #region MouseMoveImplementation

        private void MouseMoveImplementation(Point position)
        {
            if (this.Owner != null)
            {
                this.CaptureMouse(position);
                if (!this.IsRestricted && this.IsDragEnabled && this.IsDragging)
                {
                    if (position != this._previousPosition)
                    {
                        this.OnDragDelta(position.X - this._previousPosition.X, position.Y - this._previousPosition.Y);
                        this._previousPosition = position;
                    }
                }
            }
        }

        #endregion MouseMoveImplementation

        #region MouseLeaveImplementation

        private void MouseLeaveImplementation()
        {
            this.IsOver = false;
            if (this.IsEnabled)
            {
                this.EnsureCurrentState();
            }
        }

        #endregion MouseLeaveImplementation

        #endregion Private

        #region Internal



#region Infragistics Source Cleanup (Region)




















































#endregion // Infragistics Source Cleanup (Region)


        #endregion

        #endregion Methods

        #region Overrides

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.ThumbToolTip = this.GetTemplateChild("ThumbToolTip") as ToolTip;



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        }



        /// <summary>
        /// Called before the <see cref="System.Windows.UIElement.GotFocus"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            GotFocusImplementation();
        }

        /// <summary>
        /// Called before the <see cref="System.Windows.UIElement.LostFocus"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            LostFocusImplementation();
        }

        /// <summary>
        /// Called before the <see cref="System.Windows.UIElement.MouseEnter"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            Point position = e.GetPosition((UIElement)this.Parent);
            MouseEnterImplementation(position);
        }

        /// <summary>
        /// Called before the <see cref="System.Windows.UIElement.MouseLeftButtonDown"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Point position = e.GetPosition((UIElement)this.Parent);
            e.Handled = MouseLeftButtonDownImplementation(position, e.Handled);
        }

        /// <summary>
        /// Called before the <see cref="System.Windows.UIElement.MouseLeftButtonUp"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!e.Handled && this.IsDragEnabled && (this.IsDragging && this.IsEnabled))
            {
                this._origin = e.GetPosition((UIElement)this.Parent);
                this.IsDragging = false;
                this.ReleaseMouseCapture();
                this.RaiseDragCompleted(false);
            }
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseMove"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point position = e.GetPosition((UIElement)this.Parent);
            MouseMoveImplementation(position);
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseLeave"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            MouseLeaveImplementation();
        }



        #endregion Overrides

        #region EventHandlers

        private void XamSliderThumbBase_LayoutUpdated(object sender, EventArgs e)
        {
            this.EnsureCurrentState();
        }

        /// <summary>
        /// If the IsEnabled State of our control changes, we need to update to the correct VisualState
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XamSliderThumbBase_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.EnsureCurrentState();
        }



#region Infragistics Source Cleanup (Region)

































































































































#endregion // Infragistics Source Cleanup (Region)


        #endregion EventHandlers

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Members
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