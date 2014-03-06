using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Editors.Primitives;
using System.Linq;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// An object that describes generic class for the slider thumb
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    public class XamSliderThumb<T> : XamSliderThumbBase
    {
        #region Members

        private double _dragValue;
        private TrackFill _trackFill;
        private T originalValue1, originalValue2;
        private bool _quietlySetValue;
        private T _initialValue;
        private bool _initialValueSet;

        #endregion Members

        #region Events

        /// <summary>
        /// Occurs when the value of the <see cref="XamSliderThumb{T}"/> Value property is changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<T> ValueChanged;

        #endregion Events

        #region Properties

        #region Public

        #region IsSnapToTickEnabled

        /// <summary>
        /// Identifies the <see cref="IsSnapToTickEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSnapToTickEnabledProperty = DependencyProperty.Register("IsSnapToTickEnabled", typeof(bool), typeof(XamSliderThumb<T>), new PropertyMetadata(false, new PropertyChangedCallback(IsSnapToTickEnabledChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether this thumb instance is snap to tick enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is snap to tick enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsSnapToTickEnabled
        {
            get { return (bool)this.GetValue(IsSnapToTickEnabledProperty); }
            set { this.SetValue(IsSnapToTickEnabledProperty, value); }
        }

        private static void IsSnapToTickEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            //If we are enabling snap to tick, we need to update the value!!
            if ((bool)e.NewValue)
            {
                XamSliderThumb<T> thumb = obj as XamSliderThumb<T>;
                thumb.Value = thumb.PreviewCoerceValue(thumb.Value);
            }
        }

        #endregion IsSnapToTickEnabled

        #region IsTrackFillVisible

        /// <summary>
        /// Identifies the <see cref="IsTrackFillVisible"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTrackFillVisibleProperty = DependencyProperty.Register("IsTrackFillVisible", typeof(bool), typeof(XamSliderThumb<T>), new PropertyMetadata(true, IsTrackFillVisibleChanged));

        /// <summary>
        /// Gets or sets a value indicating whether TrackFill of the thumb is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is TrackFill visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsTrackFillVisible
        {
            get { return (bool)this.GetValue(IsTrackFillVisibleProperty); }
            set { this.SetValue(IsTrackFillVisibleProperty, value); }
        }

        private static void IsTrackFillVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderThumb<T> thumb = obj as XamSliderThumb<T>;
            if (thumb != null)
            {
                if (thumb.Owner != null)
                {
                    thumb.Owner.EnsureTrackFill();
                }
            }
        }

        #endregion // IsTrackFillVisible

        #region Owner

        internal new XamSliderBase<T> Owner
        {
            get
            {
                return base.Owner as XamSliderBase<T>;
            }

            set
            {
                base.Owner = value;
                this.TrackFill.Owner = value;
            }
        }

        #endregion // Owner

        #region TrackFillBrush

        /// <summary>
        /// Identifies the <see cref="TrackFillBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TrackFillBrushProperty = DependencyProperty.Register("TrackFillBrush", typeof(Brush), typeof(XamSliderThumb<T>), new PropertyMetadata(new PropertyChangedCallback(TrackFillBrushChanged)));

        /// <summary>
        /// Gets or sets the Brush, used for background of
        /// the <see cref="TrackFill"/> instance,
        /// assigned to the <see cref="XamSliderThumb&lt;T&gt;"/>.
        /// </summary>
        /// <value>The track fill brush.</value>
        public Brush TrackFillBrush
        {
            get { return (Brush)this.GetValue(TrackFillBrushProperty); }
            set { this.SetValue(TrackFillBrushProperty, value); }
        }

        private static void TrackFillBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderThumb<T> thumb = obj as XamSliderThumb<T>;
            if (thumb != null)
            {
                Brush brush = e.NewValue as Brush;
                thumb.TrackFill.Background = brush;
            }
        }

        #endregion // TrackFillBrush

        #region TrackFillStyle

        /// <summary>
        /// Identifies the <see cref="TrackFillStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TrackFillStyleProperty = DependencyProperty.Register("TrackFillStyle", typeof(Style), typeof(XamSliderThumb<T>), new PropertyMetadata(new PropertyChangedCallback(TrackFillStyleChanged)));

        /// <summary>
        /// Gets or sets the Style, used for background of
        /// the <see cref="TrackFill"/> instance,
        /// assigned to the <see cref="XamSliderThumb&lt;T&gt;"/>.
        /// </summary>
        /// <value>The track fill brush.</value>
        public Style TrackFillStyle
        {
            get { return (Style)this.GetValue(TrackFillStyleProperty); }
            set { this.SetValue(TrackFillStyleProperty, value); }
        }

        private static void TrackFillStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderThumb<T> thumb = obj as XamSliderThumb<T>;
            if (thumb != null)
            {
                Style style = e.NewValue as Style;
                thumb.TrackFill.Style = style;
            }
        }

        #endregion // TrackFillStyle

        #region Value

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(T), typeof(XamSliderThumb<T>), new PropertyMetadata(new PropertyChangedCallback(ValuePropertyChanged)));

        /// <summary>
        /// Gets or sets the generic value of
        /// the Value property.
        /// </summary>
        /// <value>The value.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual T Value
        {
            get { return (T)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void ValuePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderThumb<T> thumb = obj as XamSliderThumb<T>;
            if (thumb != null)
            {
                if (e.NewValue != e.OldValue)
                {
                    thumb.OnValueChanged((T)e.OldValue, (T)e.NewValue);
                }
            }
        }

        #endregion Value

        #endregion Public

        #region Internal

        #region IsLastMoved

        internal bool IsLastMoved { get; set; }

        #endregion IsLastMoved
        
        #region InitialValue

        internal T InitialValue
        {
            get { return _initialValue; }
            set { _initialValue = value; _initialValueSet = true; }
        }

        #endregion InitialValue

        #region PreviosIsSnapToTickEnabled

        internal bool PreviousIsSnapToTickEnabled { get; set; }

        #endregion PreviosIsSnapToTickEnabled

        #region TrackFill

        internal TrackFill TrackFill
        {
            get
            {
                if (this._trackFill == null)
                {
                    this._trackFill = new TrackFill();

                    this._trackFill.DragDelta += this.TrackFill_DragDelta;
                    this._trackFill.DragStarted += this.TrackFill_DragStarted;
                    this._trackFill.DragCompleted += this.TrackFill_DragCompleted;
                    this._trackFill.KeyDown += this.TrackFill_KeyDown;

                }

                return this._trackFill;
            }
        }

        #endregion TrackFill

        #endregion Internal

        #endregion Properties

        #region Overrides

        #region EnsureCurrentState

        /// <summary>
        /// Ensures VisualStateManager current state when
        /// the owner of the control is in Horizontal or
        /// Vertical state
        /// </summary>
        protected override void EnsureCurrentState()
        {
            base.EnsureCurrentState();
            if (this.Owner != null && this.IsActive)
            {
                if (this.Owner.ActiveThumb == null || !this.Owner.ActiveThumb.Equals(this))
                {
                    this.Owner.ActiveThumb = this;
                }
            }
        }

        #endregion EnsureCurrentState

        #region OnApplyTemplate

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.ThumbToolTip != null)
            {
                Binding b = new Binding("Value");
                b.Source = this;
                ThumbToolTip.SetBinding(ContentControl.ContentProperty, b);
            }
        }

        #endregion OnApplyTemplate

        #region OnCreateAutomationPeer

        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new XamSliderThumbAutomationPeer<T>(this);
        }

        #endregion OnCreateAutomationPeer

        #region OnDragDelta

        /// <summary>
        /// Called when DragDelta event is raised.
        /// </summary>
        /// <param name="horizontalChange">The horizontal change.</param>
        /// <param name="verticalChange">The verticsal change.</param>
        protected override void OnDragDelta(double horizontalChange, double verticalChange)
        {
            base.OnDragDelta(horizontalChange, verticalChange);
            if (this.Owner != null)
            {
                double offset;
                double length = (this.Owner.Orientation == Orientation.Horizontal)
                                    ? this.Owner.HorizontalTrack.ActualWidth
                                    : this.Owner.VerticalTrack.ActualHeight;
                double max = this.Owner.ToDouble(this.Owner.MaxValue);
                double min = this.Owner.ToDouble(this.Owner.MinValue);
                if (this.Owner.Orientation == Orientation.Horizontal)
                {



                    offset = (max - min) * (horizontalChange / (length - (this.ActualWidth / 2.0)));

                }
                else
                {



                    offset = -1 * (max - min) * (verticalChange / (length - (this.ActualHeight / 2.0)));

                }

                if (!this.Owner.IsDirectionReversed)
                    this._dragValue = this._dragValue + offset;
                else
                    this._dragValue = this._dragValue - offset;

                this.Value = this.PreviewCoerceValue(this.Owner.ToValue(this._dragValue));
            }
        }

        #endregion OnDragDelta

        #region OnDragStarted

        /// <summary>
        /// Called when DragStarted event is raised.
        /// </summary>
        /// <param name="horizontalOffset">The horizontal offset.</param>
        /// <param name="verticalOffset">The verticsal offset.</param>
        protected override void OnDragStarted(double horizontalOffset, double verticalOffset)
        {
            base.OnDragStarted(horizontalOffset, verticalOffset);
            if (this.Owner != null)
            {
                foreach (XamSliderThumb<T> thumb in this.Owner.Thumbs)
                {
                    thumb.InitDrag();
                }
            }
        }

        #endregion OnDragStarted

        #region OnGotFocus

        /// <summary>
        /// Raises the <see cref="OnGotFocus"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (this.Owner != null)
                this.IsActive = true;
        }

        #endregion OnGotFocus

        #region OnMouseLeftButtonUp
        /// <summary>
        /// Called before the <see cref="System.Windows.UIElement.MouseLeftButtonUp"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            e.Handled = true;
        }
        #endregion //OnMouseLeftButtonUp

        #region OnIsActiveChanged

        protected override void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            base.OnIsActiveChanged(oldValue, newValue);
            var slider = this.Owner as XamSliderBase<T>;

            if (slider != null)
            {
                if ((slider.ActiveThumb != null) && (slider.ActiveThumb != this))
                    slider.ActiveThumb.IsActive = false;

                if (newValue == true)
                    slider.ActiveThumb = this;
                else if (slider.ActiveThumb == this)
                    slider.ActiveThumb = null;
            }
        }

        #endregion // OnIsActiveChanged

        #endregion Overrides

        #region Methods

        #region Internal

        #region QuietlySetValue

        /// <summary>
        /// Changes the value of Value, but sets a ONE TIME usage flag to prevent it from invoking the callback handler
        /// This is dangerous and ugly, and should only be used by the slider.
        /// </summary>
        /// <param name="value">The value to set</param>
        internal void QuietlySetValue(T value)
        {
            _quietlySetValue = true;

            //There is a fun case where the thumb will be added to the slider and it's value hasn't been bound yet, 
            //this if check prevents that binding from breaking.
            if ((this.Value == null && value != null) || !this.Value.Equals(value))
                this.Value = value;

            _quietlySetValue = false;
        }

        #endregion

        #region PreviewCoerceValue

        internal T PreviewCoerceValue(T newValue)
        {
            return PreviewCoerceValue(newValue, this.Value);
        }

        internal T PreviewCoerceValue(T newValue, T originalValue)
        {
            T returnValue = newValue;
            if (this.Owner != null)
            {
                double value = this.Owner.ToDouble(newValue);
                double coersedValue = value;
                double min = this.Owner.ToDouble(this.Owner.MinValue);
                double max = this.Owner.ToDouble(this.Owner.MaxValue);

                if (coersedValue > max)
                    coersedValue = max;
                if (coersedValue < min)
                    coersedValue = min;

                coersedValue = this.CoerceSnapToTickMarks(coersedValue);

                returnValue = (T)this.Owner.ToValue(coersedValue);
            }

            return returnValue;
        }

        #endregion PreviewCoerceValue

        #region GetDelta

        /// <summary>
        /// Gets the delta, used to coerse the thumb interaction.
        /// </summary>
        /// <returns>delta as double </returns>
        protected virtual double GetDelta()
        {
            return 0.0001;
        }

        #endregion GetDelta

        #region InitDrag

        /// <summary>
        /// Inits parameters when start to drag the thumb.
        /// </summary>
        internal void InitDrag()
        {
            this.InitialValue = this.Value;
            this._dragValue = this.Owner.ToDouble(this.Value);
        }

        #endregion InitDrag

        #region ResolveTrackFill







        internal void ResolveTrackFill()
        {
            if (this.Owner != null)
            {
                if (this.Owner.VerticalTrack != null && this.Owner.HorizontalTrack != null)
                {
                    //Find the starting Value of the slider, Top or Left value.
                    double previousValue = this.GetTrackStartingValue();

                    //Get our index in the Slider
                    int currIndex = this.Owner.SortedThumbs.IndexOf(this);

                    //Determine if there is a thumb between us and the end of our track fill.
                    XamSliderThumb<T> thumb = this.GetPreviousEnabledThumbIndex(currIndex, false);

                    // To find the coverage area, find the length of the value range of the slider, find the amount that value covers, and translate that to the percent of the visual size.
                    double valueRange = this.Owner.ToDouble(this.Owner.MaxValue) - this.Owner.ToDouble(this.Owner.MinValue);

                    //Now we need to compensate the value for the bottom of the range.
                    double adjustedValue = this.Owner.ToDouble(this.Value) - this.Owner.ToDouble(this.Owner.MinValue);

                    //Find the size of the track
                    double trackSize = this.Owner.Orientation == Orientation.Horizontal ?
                        this.Owner.HorizontalTrack.ActualWidth : this.Owner.VerticalTrack.ActualHeight;

                    //If our track fill is going to hit a previous thumb, we need to accommodate for it.
                    if (thumb != null)
                    {
                        //Cache the Value of the previous thumb, and compensate it for the minimum of the slider range.
                        double previousThumbValue = this.Owner.ToDouble(thumb.Value) - this.Owner.ToDouble(this.Owner.MinValue);

                        //First adjust the value so it does not overlap the other track fill.
                        adjustedValue -= previousThumbValue;

                        //Find the pixels covered from the previous thumb to the end of the slider.
                        double previousCoverage = 0;

                        if (previousThumbValue > 0 && valueRange > 0)
                            previousCoverage = (previousThumbValue / valueRange) * trackSize;

                        //Then adjust the starting pointing of the track fill by offsetting it from the last track fill.
                        if (((this.Owner.Orientation == Orientation.Horizontal) && (this.Owner.IsDirectionReversed))
                            || ((this.Owner.Orientation == Orientation.Vertical) && (!this.Owner.IsDirectionReversed)))
                        {
                            previousValue -= previousCoverage;
                        }
                        else
                        {
                            previousValue += previousCoverage;
                        }
                    }

                    //Find the amount of space to cover
                    double currValue = 0;

                    if (adjustedValue > 0 && valueRange > 0)
                        currValue = (adjustedValue / valueRange) * trackSize;

                    //Render the TrackFill
                    this.EnsureTrackFill(currValue, previousValue);
                }
            }
        }

        #endregion ResolveTrackFill

        #region ResolveValue



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


        internal double ResolveValue()
        {
            if (this.Owner != null)
            {
                return this.Owner.ToDouble(this.Value);
            }

            return 0;
        }

        #endregion ResolveValue

        #region SnapRangeInRangeToTickMarks



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


        internal virtual void SnapRangeInRangeToTickMarks(XamSliderThumb<T> thumb)
        {
            double thisValue = this.ResolveValue();
            double thumbValue = thumb.ResolveValue();
            double delta = thisValue - thumbValue;
            thisValue = this.CoerceSnapToTickMarks(thisValue);
            thumbValue = thisValue - delta;
            this.Value = this.Owner.ToValue(thisValue);
            thumb.Value = this.Owner.ToValue(thumbValue);
        }

        #endregion SnapRangeInRangeToTickMarks

        #region UpdateThumbPosition

        /// <summary>
        /// Updates the thumb position.
        /// when MinValue or MaxValue of the slider is changed
        /// </summary>
        internal void UpdateThumbPosition()
        {
            XamSliderBase<T> owner = this.Owner;
            if (owner != null)
            {
                if (owner.ToDouble(this.Value) > owner.ToDouble(owner.MaxValue))
                    this.Value = owner.MaxValue;
                if (owner.ToDouble(this.Value) < owner.ToDouble(owner.MinValue))
                    this.Value = owner.MinValue;
            }
        }
        #endregion //UpdateThumbPosition

        #region IsThumbValid

        /// <summary>
        /// Checks to see if a thumb meets specific criteria to be considered valid, for right now, just enabled!
        /// </summary>
        /// <param name="thumb">The thumb to check</param>
        /// <returns>True or False</returns>
        internal bool IsThumbValid()
        {
            return this.IsEnabled;
        }
        #endregion

        #region IsThumbValidForInteraction

        /// <summary>
        /// Checks to see if the thumb is a Valid Interactable thumb... this means, it's Valid (see above), and NOT free!
        /// </summary>
        /// <param name="thumb">The thumb to look at</param>
        /// <returns>True or False</returns>
        internal bool IsThumbValidForInteraction()
        {
            return (this.InteractionMode != SliderThumbInteractionMode.Free) & (IsThumbValid());
        }

        #endregion

        #endregion Internal

        #region Protected

        #region OnValueChanged

        /// <summary>
        /// Called when value of the ValueChanged event is raised.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnValueChanged(T oldValue, T newValue)
        {
            if (this.Owner != null)
            {
                //If we are doing this quietly then DO NOT RECOERCE THE VALUE!!!
                //DO NOT REFACTOR the 'this.InitialValue = oldValue' out of the if!!  The _initialValueSet will break!!
                if ((!_quietlySetValue) && (this.Owner.SliderLoaded) && (_initialValueSet))
                {
                    this.InitialValue = oldValue;
                    this.Owner.MoveThumbTo(this, PreviewCoerceValue(newValue));
                }
                else
                {
                    this.Owner.MoveThumbTo(this, PreviewCoerceValue(newValue), false);
                    this.InitialValue = oldValue;
                }

                double updatedValue = this.Owner.ToDouble(this.Value);

                if (updatedValue != this.Owner.ToDouble(oldValue))
                {
                    if (this.Owner.OnThumbValueChanged(oldValue, this.Value, this))
                        return;

                    RoutedPropertyChangedEventHandler<T> valueChanged = this.ValueChanged;
                    if (valueChanged != null)
                    {
                        valueChanged(this, new RoutedPropertyChangedEventArgs<T>(oldValue, this.Value));
                    }
                }
            }
        }

        #endregion OnValueChanged

        #endregion Protected

        #region Private

        #region CoerceSnapToTickMarks

        /// <summary>
        /// Iterates through all of the tick marks in all tick mark collections to find the closest tick mark to the 
        /// position of new value.
        /// </summary>
        /// <param name="newValue">The value to find the nearest tick mark to</param>
        /// <returns>The value of the nearest tickmark (if IsSnapToTickEnabled = true, otherwise just passes through the value)</returns>
        protected internal double CoerceSnapToTickMarks(double newValue)
        {
            if (this.Owner == null | (!this.IsSnapToTickEnabled || this.Owner.TickMarks.Count == 0))
                return newValue;

            double min = this.Owner.ToDouble(this.Owner.MinValue);
            double max = this.Owner.ToDouble(this.Owner.MaxValue);

            //Loop through all of the tick marks and hone in on the closest min and max bound tick marks for this slider
            foreach (SliderTickMarks<T> tickmarks in this.Owner.TickMarks)
            {
                foreach (double tickmark in tickmarks.ResolvedTickMarks.Select(tickValue => this.Owner.ToDouble(tickValue)))
                {
                    if (tickmark == newValue)
                        return newValue;

                    if ((tickmark > newValue) && (tickmark < max))
                        max = tickmark;
                    if ((tickmark < newValue) && (tickmark > min))
                        min = tickmark;
                }
            }

            if ((max - newValue) > (newValue - min))
                return min;

            return max;
        }

        #endregion CoerceSnapToTickMarks

        #region EnsureRangeThumbs

        private void EnsureRangeThumbs(int index, double offset)
        {

            double newValue = this.Owner.IsDirectionReversed ? this.ResolveValue() - offset : this.ResolveValue() + offset;

            XamSliderThumb<T> thumb = this.GetPreviousEnabledThumbIndex(index);
            if (thumb != null)
            {
                double lastValue;
                double delta = this.ResolveValue() - thumb.ResolveValue();

                if ((offset > 0 && !this.Owner.IsDirectionReversed) ||
                    (offset < 0 && this.Owner.IsDirectionReversed))
                {
                    this.InitialValue = this.Value;
                    this._dragValue = newValue;

                    this.Value = this.PreviewCoerceValue(this.Owner.ToValue(newValue), this.Value);
                    lastValue = this.ResolveValue() - delta;
                    thumb.Value = this.Owner.ToValue(lastValue);
                }
                else if ((offset > 0 && this.Owner.IsDirectionReversed) ||
                         (offset < 0 && !this.Owner.IsDirectionReversed))
                {
                    thumb.InitialValue = thumb.Value;
                    thumb._dragValue = lastValue = newValue - delta;

                    thumb.Value = this.Owner.ToValue(lastValue);
                    newValue = thumb.ResolveValue() + delta;
                    this.Value = this.Owner.ToValue(newValue);
                }
            }
        }

        #endregion EnsureRangeThumbs

        #region EnsureTrackFill
        private void EnsureTrackFill(double currValue, double stopValue)
        {
            double offset = stopValue;

            if (((this.Owner.Orientation == Orientation.Horizontal) && (this.Owner.IsDirectionReversed))
                || ((this.Owner.Orientation == Orientation.Vertical) && (!this.Owner.IsDirectionReversed)))
            {
                offset -= currValue;
            }
            
            if (this.Owner.Orientation == Orientation.Horizontal)
            {
                this.TrackFill.Width = currValue;
                this.TrackFill.Height = this.Owner.HorizontalTrack.ActualHeight;
                Canvas.SetTop(this.TrackFill, 0);
                Canvas.SetLeft(this.TrackFill, offset);

            }
            else if (this.Owner.Orientation == Orientation.Vertical)
            {
                this.TrackFill.Height = currValue;
                this.TrackFill.Width = this.Owner.VerticalTrack.ActualWidth;
                Canvas.SetLeft(this.TrackFill, 0);
                Canvas.SetTop(this.TrackFill, offset);
            }
        }

        #endregion EnsureTrackFill

        #region GetPreviousEnabledThumbIndex

        private XamSliderThumb<T> GetPreviousEnabledThumbIndex(int currIndex)
        {
            return GetPreviousEnabledThumbIndex(currIndex, true);
        }

        private XamSliderThumb<T> GetPreviousEnabledThumbIndex(int currIndex, bool requireThumbDraggable)
        {
            if (currIndex > 0)
            {
                for (int i = currIndex - 1; i >= 0; i--)
                {
                    XamSliderThumb<T> thumb = this.Owner.SortedThumbs[i];
                    if (thumb.IsTrackFillVisible && (!requireThumbDraggable || (thumb.IsDragEnabled && thumb.IsEnabled)))
                    {
                        return thumb;
                    }
                }
            }

            return null;
        }
        #endregion //GetPreviousEnabledThumbIndex
                
        #region GetTrackSize






        private double GetTrackStartingValue()
        {
            double startingValue = 0;
            if (this.Owner.Orientation == Orientation.Vertical)
            {
                if (!this.Owner.IsDirectionReversed)
                {
                    startingValue = this.Owner.VerticalTrack.ActualHeight;
                }
            }
            else
            {
                if (this.Owner.IsDirectionReversed)
                {
                    startingValue = this.Owner.HorizontalTrack.ActualWidth;
                }
            }

            return startingValue;
        }

        #endregion GetTrackSize

        #endregion Private
        
        #endregion Methods

        #region Event Handlers



        #region TrackFill_DragDelta

        private void TrackFill_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            int index = this.Owner.SortedThumbs.IndexOf(this);
            TrackFill trackfill = sender as TrackFill;
            if (index > 0 && trackfill != null)
            {
                XamRangeSlider<T> multiSLider = this.Owner as XamRangeSlider<T>;
                if (multiSLider != null && multiSLider.IsSelectionRangeEnabled)
                {
                    double offset;
                    double length = (this.Owner.Orientation == Orientation.Horizontal)
                                        ? this.Owner.HorizontalTrack.ActualWidth
                                        : this.Owner.VerticalTrack.ActualHeight;
                    double max = this.Owner.ToDouble(this.Owner.MaxValue);
                    double min = this.Owner.ToDouble(this.Owner.MinValue);
                    if (this.Owner.Orientation == Orientation.Horizontal)
                    {
                        offset = (max - min) * (e.HorizontalChange / (length - (this.ActualWidth / 2.0)));
                    }
                    else
                    {
                        offset = -1 * (max - min) * (e.VerticalChange / (length - (this.ActualHeight / 2.0)));
                    }

                    this.EnsureRangeThumbs(index, offset);
                }
            }
        }

        #endregion TrackFill_DragDelta

        #region TrackFill_DragCompleted

        private void TrackFill_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            int index = this.Owner.SortedThumbs.IndexOf(this);
            TrackFill trackfill = sender as TrackFill;
            if (index > 0 && trackfill != null)
            {
                this.IsSnapToTickEnabled = this.PreviousIsSnapToTickEnabled;
                XamSliderThumb<T> thumb = this.TrackFill.Thumb as XamSliderThumb<T>;
                if (thumb != null)
                {
                    thumb.IsSnapToTickEnabled = thumb.PreviousIsSnapToTickEnabled;

                    this.InitialValue = originalValue1;
                    thumb.InitialValue = originalValue2;

                    if (this.IsSnapToTickEnabled)
                    {
                        this.SnapRangeInRangeToTickMarks(thumb);
                    }
                    else if (thumb.IsSnapToTickEnabled)
                    {
                        thumb.SnapRangeInRangeToTickMarks(this);
                    }

                    //The thumb on the TrackFill will not be me, hence the reason why we pass in this and trackfill.Thumb
                    if (this.Owner is XamRangeSlider<T>)
                        ((XamRangeSlider<T>)Owner).OnTrackFillDragCompleted(this, trackfill.Thumb as XamSliderThumb<T>);
                }
            }
        }

        #endregion TrackFill_DragCompleted

        #region TrackFill_DragStarted

        private void TrackFill_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            int index = this.Owner.SortedThumbs.IndexOf(this);
            TrackFill trackfill = sender as TrackFill;
            if (index > 0 && trackfill != null)
            {
                XamSliderThumb<T> thumb = this.GetPreviousEnabledThumbIndex(index);
                if (thumb != null)
                {
                    originalValue1 = this.Value;
                    originalValue2 = thumb.Value;

                    this.PreviousIsSnapToTickEnabled = this.IsSnapToTickEnabled;
                    thumb.PreviousIsSnapToTickEnabled = thumb.IsSnapToTickEnabled;

                    this.IsSnapToTickEnabled = false;
                    thumb.IsSnapToTickEnabled = false;
                }

                this.TrackFill.Thumb = thumb;
            }
        }

        #endregion TrackFill_DragStarted

        #region TrackFill_KeyDown

        private void TrackFill_KeyDown(object sender, KeyEventArgs e)
        {
            int index = this.Owner.SortedThumbs.IndexOf(this);
            TrackFill trackfill = sender as TrackFill;
            if (index > 0 && trackfill != null && this.Owner.EnableKeyboardNavigation)
            {
                XamRangeSlider<T> multiSLider = this.Owner as XamRangeSlider<T>;
                if (multiSLider != null && multiSLider.IsSelectionRangeEnabled)
                {
                    double offset = 0;

                    if (e.Key == Key.Right || e.Key == Key.Up)
                    {
                        offset = multiSLider.GetSmallChangeValue();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Left || e.Key == Key.Down)
                    {
                        offset = -multiSLider.GetSmallChangeValue();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.PageUp)
                    {
                        offset = multiSLider.GetLargeChangeValue();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.PageDown)
                    {
                        offset = -multiSLider.GetLargeChangeValue();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.End)
                    {
                        double startValue = this.ResolveValue();
                        double endValue = this.Owner.ToDouble(this.Owner.MaxValue);
                        offset = endValue - startValue;
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Home)
                    {
                        XamSliderThumb<T> thumb = this.GetPreviousEnabledThumbIndex(index);
                        if (thumb != null)
                        {
                            double thumbValue = thumb.ResolveValue();
                            double homeValue = this.Owner.ToDouble(this.Owner.MinValue);
                            offset = homeValue - thumbValue;
                            e.Handled = true;
                        }
                    }

                    if (offset != 0)
                    {
                        this.EnsureRangeThumbs(index, offset);
                    }
                }
            }
        }

        #endregion TrackFill_KeyDown



        #endregion Event Handlers
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