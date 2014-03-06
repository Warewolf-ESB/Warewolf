using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A control that provides generic slider behavior.
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public abstract class XamSliderBase<T> : XamSliderBase
    {
        #region Members

        //Fixed Bug 33723 _ Mihail Mateev 06/11/2010 - added _isTrackClicked member
        internal bool IsTrackClicked;
        private ObservableCollection<XamSliderThumb<T>> _thumbs;
        private ObservableCollection<SliderTickMarks<T>> _tickmarks;
        private TickMarksPanel<T> _tickMarksPanel;
        //variable, used for fix for Bug 32211
        private bool _isOrientationChanged;

        #endregion Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XamSliderBase&lt;T&gt;"/> class.
        /// </summary>
        protected XamSliderBase()
        {
            this.SliderLoaded = false;
            this.LayoutUpdated += this.XamSliderBase_LayoutUpdated;
            this.Loaded += XamSliderBase_Loaded;
        }

        #endregion Constructor

        #region Overrides

        #region ArrangeThumbs

        /// <summary>
        /// Arranges the thumbs inside the slider layout.
        /// </summary>
        protected internal override void ArrangeThumbs()
        {
            if (this.DesiredSize.Height != 0 && this.DesiredSize.Width != 0)
            {
                foreach (XamSliderThumb<T> thumb in this.Thumbs)
                {
                    this.ArrangeThumb(thumb);
                }
            }
        }

        //Mihail Mateev: 2010.04.07 - Added ArrangeThumbs2 and ArrangeThumb(XamWebSliderThumb<T>, bool) - Fix for Bug 30277 ;
        /// <summary>
        /// Arranges the thumbs inside the slider layout.
        /// </summary>
        protected internal void ArrangeThumbs2()
        {
            if (this.DesiredSize.Height != 0 && this.DesiredSize.Width != 0)
            {
                foreach (XamSliderThumb<T> thumb in this.Thumbs)
                {
                    this.ArrangeThumb(thumb, true);
                }
            }
        }

        #endregion ArrangeThumbs

        #region ArrangeThumb

        /// <summary>
        /// Arranges the <see cref="XamSliderThumb&lt;T&gt;"/> instance .
        /// </summary>
        /// <param name="thumb">The thumb.</param>
        protected internal virtual void ArrangeThumb(XamSliderThumb<T> thumb)
        {
            this.ArrangeThumb(thumb, false);
        }

        #endregion ArrangeThumb

        #region ArrangeThumb -isFirstTime

        /// <summary>
        /// Arranges the thumb.
        /// </summary>
        /// <param name="thumb">The thumb.</param>
        /// <param name="isFirstTime">arrange for first time.</param>
        protected internal virtual void ArrangeThumb(XamSliderThumb<T> thumb, bool isFirstTime)
        {
            if (this.Orientation == Orientation.Horizontal)
            {
                if (this.HorizontalTrack != null)
                {
                    Canvas.SetTop(thumb, 0);
                    if (!this.IsDirectionReversed)
                    {
                        if (isFirstTime)
                        {
                            Canvas.SetLeft(thumb,
                                           this.ResolveThumbLocation(thumb.Value, this.HorizontalTrack.ActualWidth));
                        }
                        else
                        {
                            Canvas.SetTop(thumb, this.VerticalTrack.ActualHeight - this.ResolveThumbLocation(thumb.Value, this.VerticalTrack.ActualHeight) - (thumb.ActualHeight / 2));
                        }
                    }
                    else
                    {
                        if (isFirstTime)
                        {
                            Canvas.SetLeft(thumb, this.ResolveThumbLocation(thumb.Value, this.HorizontalTrack.ActualWidth));
                        }
                        else
                        {
                            Canvas.SetLeft(thumb, this.ResolveThumbLocation(thumb.Value, this.HorizontalTrack.ActualWidth) - (thumb.ActualWidth / 2));
                        }
                    }
                }
            }
            else
            {
                if (this.VerticalTrack != null)
                {
                    Canvas.SetLeft(thumb, 0);
                    if (isFirstTime)
                    {
                        Canvas.SetTop(thumb, this.VerticalTrack.ActualHeight - this.ResolveThumbLocation(thumb.Value, this.VerticalTrack.ActualHeight));
                    }
                    else
                    {
                    }
                }
            }
        }

        #endregion ArrangeThumb -isFirstTime

        #region EnsureOrientation

        /// <summary>
        /// Ensures the state and position of the
        /// <see cref="XamSliderBase&lt;T&gt;"/> elements,
        /// depending on orientation of the slider.
        /// </summary>
        protected override void EnsureOrientation()
        {
            if (this.Orientation == Orientation.Horizontal)
            {
                if (this.VerticalThumbsPanel != null)
                {
                    this.VerticalThumbsPanel.Children.Clear();
                }

                if (this.VerticalTrackFillsPanel != null)
                {
                    this.VerticalTrackFillsPanel.Children.Clear();
                }

                if (this.VerticalTickMarksContainer != null)
                {
                    this.VerticalTickMarksContainer.Children.Clear();
                }

                if (this.HorizontalTickMarksContainer != null)
                {
                    if (!this.HorizontalTickMarksContainer.Children.Contains(this.TickMarksPanel))
                    {
                        this.HorizontalTickMarksContainer.Children.Add(this.TickMarksPanel);
                    }

                    //Bug 33211 fix - Mihail Mateev 06/03/2010
                    _isOrientationChanged = true;
                }

                if (this.HorizontalTrackFillsPanel != null)
                {
                    foreach (XamSliderThumb<T> thumb in this.Thumbs)
                    {
                        if (!this.HorizontalTrackFillsPanel.Children.Contains(thumb.TrackFill))
                        {
                            this.HorizontalTrackFillsPanel.Children.Add(thumb.TrackFill);
                        }
                    }
                }

                if (this.HorizontalThumbsPanel != null)
                {
                    foreach (XamSliderThumb<T> thumb in this.Thumbs)
                    {
                        if (!this.HorizontalThumbsPanel.Children.Contains(thumb))
                        {

                            this.RemoveLogicalChild(thumb);

                            this.HorizontalThumbsPanel.Children.Add(thumb);
                        }
                    }
                }
            }
            else
            {
                if (this.HorizontalThumbsPanel != null)
                {
                    this.HorizontalThumbsPanel.Children.Clear();
                }

                if (this.HorizontalTrackFillsPanel != null)
                {
                    this.HorizontalTrackFillsPanel.Children.Clear();
                }

                if (this.HorizontalTickMarksContainer != null)
                {
                    this.HorizontalTickMarksContainer.Children.Clear();
                }

                if (this.VerticalTickMarksContainer != null)
                {
                    if (!this.VerticalTickMarksContainer.Children.Contains(this.TickMarksPanel))
                    {
                        this.VerticalTickMarksContainer.Children.Add(this.TickMarksPanel);
                    }

                    //Bug 33211 fix - Mihail Mateev 06/03/2010
                    _isOrientationChanged = true;
                }

                if (this.VerticalTrackFillsPanel != null)
                {
                    foreach (XamSliderThumb<T> thumb in this.Thumbs)
                    {
                        if (!this.VerticalTrackFillsPanel.Children.Contains(thumb.TrackFill))
                        {
                            this.VerticalTrackFillsPanel.Children.Add(thumb.TrackFill);
                        }
                    }
                }

                if (this.VerticalThumbsPanel != null)
                {
                    foreach (XamSliderThumb<T> thumb in this.Thumbs)
                    {
                        if (!this.VerticalThumbsPanel.Children.Contains(thumb))
                        {

                            this.RemoveLogicalChild(thumb);

                            this.VerticalThumbsPanel.Children.Add(thumb);
                        }
                    }
                }
            }

            this.ArrangeThumbs();
        }

        #endregion EnsureOrientation

        #region EnsureTrackFill

        internal void EnsureTrackFill()
        {
            if (this.Thumbs.Count > 0)
            {
                for (int i = 0; i < this.SortedThumbs.Count; i++)
                {
                    XamSliderThumb<T> thumb = this.SortedThumbs[i];
                    if (thumb.IsTrackFillVisible)
                    {
                        thumb.TrackFill.Visibility = Visibility.Visible;
                        thumb.ResolveTrackFill();
                    }
                    else
                    {
                        thumb.TrackFill.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #endregion EnsureTrackFill

        #region EnsureSliderElements

        /// <summary>
        /// Ensures the slider elements -
        /// Thumbs and ThickMarks position.
        /// </summary>
        protected override void EnsureSliderElements()
        {
            base.EnsureSliderElements();
            this.EnsureTickMarks();
            this.ArrangeThumbs();
        }

        #endregion EnsureSliderElements

        #region EnsureTickMarks

        /// <summary>
        /// Ensures the tick marks generation.
        /// </summary>
        protected internal override void EnsureTickMarks()
        {
            this.TickMarksPanel.InvalidateArrange();
        }

        #endregion EnsureTickMarks



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


        #region OnApplyTemplate

        /// <summary>
        /// Called when control template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this._tickMarksPanel != null)
            {
                if (this.HorizontalTickMarksContainer != null)
                {
                    if (this.HorizontalTickMarksContainer.Children.Contains(this._tickMarksPanel))
                    {
                        this.HorizontalTickMarksContainer.Children.Remove(this._tickMarksPanel);
                    }
                }

                if (this.VerticalTickMarksContainer != null)
                {
                    if (this.VerticalTickMarksContainer.Children.Contains(this._tickMarksPanel))
                    {
                        this.VerticalTickMarksContainer.Children.Remove(this._tickMarksPanel);
                    }
                }

                this._tickMarksPanel = null;
            }

            this.RemoveThumbsFromHorizontalThumbsPanel();
            this.RemoveThumbsFromVerticalThumbsPanel();
            this.RemoveTrackFillFromHorizontalTrackFillsPanel();
            this.RemoveTrackFillFromVerticalTrackFillsPanel();

            base.OnApplyTemplate();

            if (this.ActiveThumb == null && this.Thumbs.Count > 0)
                this.Thumbs[this.Thumbs.Count - 1].IsActive = true;
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
            return new XamSliderBaseAutomationPeer<T>(this);
        }

        #endregion OnCreateAutomationPeer



        #region OnKeyDown

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.KeyDown"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!this.EnableKeyboardNavigation || this.Thumbs.Count == 0 || this.ActiveThumb == null)
            {
                return;
            }

            if (e.Key == Key.Tab)
            {
                this.Focus();
                int index = this.SortedThumbs.IndexOf(this.ActiveThumb);
                e.Handled = true;
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                {
                    if (index > 0)
                    {
                        this.ActiveThumb = this.SortedThumbs[index - 1];
                    }
                    else
                    {
                        //Were currently at the first thumb, so let the tab fall through
                        e.Handled = false;
                    }
                }
                else
                {
                    if (index < (this.Thumbs.Count - 1))
                    {
                        this.ActiveThumb = this.SortedThumbs[index + 1];
                    }
                    else
                    {
                        //Were currently at the last thumb, so let the tab fall through
                        e.Handled = false;
                    }
                }

                return;
            }

            if (e.Key == Key.Right || e.Key == Key.Up)
            {
                this.ActiveThumb.Focus();
                e.Handled = true;
                this.ProcessChanges(true, false, false);
                return;
            }

            if (e.Key == Key.Left || e.Key == Key.Down)
            {
                this.ActiveThumb.Focus();
                e.Handled = true;
                this.ProcessChanges(false, false, false);
                return;
            }

            if (e.Key == Key.PageUp)
            {
                this.ActiveThumb.Focus();
                e.Handled = true;
                this.ProcessChanges(true, true, false);
                return;
            }

            if (e.Key == Key.PageDown)
            {
                this.ActiveThumb.Focus();
                e.Handled = true;
                this.ProcessChanges(false, true, false);
                return;
            }

            if (e.Key == Key.Home)
            {
                this.ActiveThumb.Focus();
                e.Handled = true;
                this.ActiveThumb.Value = this.MinValue;
                return;
            }

            if (e.Key == Key.End)
            {
                this.ActiveThumb.Focus();
                e.Handled = true;
                this.ActiveThumb.Value = this.MaxValue;
                return;
            }
        }

        #endregion OnKeyDown



        #region OnMouseLeftButtonDown

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.Focus();
        }

        #endregion OnMouseLeftButtonDown



        #region OnMouseWheel

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseWheel"/> event occurs to provide handling for the event in a derived class without attaching a delegate.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.MouseWheelEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (this.IsEnabled && this.IsMouseWheelEnabled)
            {
                this.MouseWheelMoved(e);
            }
        }

        #endregion OnMouseWheel



        #region OnHorizontalTickMarksTemplateChanged

        /// <summary>
        /// Called when the value of the  HorizontalTickMarksTemplate property is changed.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        protected override void OnHorizontalTickMarksTemplateChanged(DataTemplate newValue)
        {
            base.OnHorizontalTickMarksTemplateChanged(newValue);
            this.TickMarksPanel.InvalidateArrange();
        }

        #endregion OnHorizontalTickMarksTemplateChanged

        #region OnThumbStyleChanged

        /// <summary>
        /// Called when value of the ThumbStyle property is changed.
        /// </summary>
        /// <param name="style">The style.</param>
        protected override void OnThumbStyleChanged(Style style)
        {
            base.OnThumbStyleChanged(style);
            foreach (XamSliderThumb<T> thumb in this.Thumbs)
            {




                if (thumb.Style == null)
                {
                    thumb.Style = style;
                }

            }
        }

        #endregion OnThumbStyleChanged

        #region OnTrackClick

        /// <summary>
        /// Raises the <see cref="XamSliderBase{T}.TrackClick"/> event.
        /// When click over the slider track.
        /// Calculates the position of the active thumb when click over
        /// the slider track, depending on position of the mouse over the
        /// the track and value of the <see cref="XamSliderBase.TrackClickAction"/> property
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        protected override void OnTrackClick(MouseButtonEventArgs e)
        {
            bool ishorizontal = this.Orientation == Orientation.Horizontal;
            double minimum = this.ToDouble(this.MinValue);
            double maximum = this.ToDouble(this.MaxValue);
            Point p = ishorizontal ? e.GetPosition(this.HorizontalTrack) : e.GetPosition(this.VerticalTrack);
            double zoom = (ishorizontal ? this.HorizontalTrack.ActualWidth : this.VerticalTrack.ActualHeight) / (maximum - minimum);
            double value = ishorizontal ? p.X : this.VerticalTrack.ActualHeight - p.Y;
            if (this.IsDirectionReversed)
            {
                value = ishorizontal ? this.HorizontalTrack.ActualWidth - p.X : p.Y;
            }

            double clickedValue = minimum + (value / zoom);

            base.OnTrackClick(e);
            e.Handled = true;
            if (this.ActiveThumb != null && this.ActiveThumb.IsEnabled && this.TrackClickAction != SliderTrackClickAction.None)
            {
                double thumbValue = this.ToDouble(this.ActiveThumb.Value);
                double startValue = thumbValue;
                this.ActiveThumb.InitialValue = this.ActiveThumb.Value;

                if (this.TrackClickAction == SliderTrackClickAction.LargeChange)
                {
                    //In the event of a decrease largeChange will be Negative...
                    double largeChange = this.GetLargeChangeValue(startValue, clickedValue > startValue);
                    thumbValue += largeChange;
                }
                else if (this.TrackClickAction == SliderTrackClickAction.MoveToPoint)
                {
                    thumbValue = clickedValue;
                    //Fixed Bug 33723 Mihail Mateev 06/11/2010 set _isTrackCLicked = true
                    IsTrackClicked = true;
                }
                this.ActiveThumb.Value = this.ToValue(thumbValue);
            }

            if (this.TrackClick != null)
            {
                TrackClickEventArgs<T> args = new TrackClickEventArgs<T>
                {
                    Value = this.ToValue(clickedValue)
                };
                this.TrackClick(this, args);
            }
        }

        #endregion OnTrackClick

        #region OnTrackFillBrushChanged

        /// <summary>
        /// Called when value of the TrackFillBrush property is changed.
        /// </summary>
        /// <param name="brush">The Brush.</param>
        protected override void OnTrackFillBrushChanged(System.Windows.Media.Brush brush)
        {
            base.OnTrackFillBrushChanged(brush);
            foreach (XamSliderThumb<T> thumb in this.Thumbs)
            {
                if (thumb.TrackFillBrush == null)
                {
                    thumb.TrackFillBrush = brush;
                }
            }
        }

        #endregion OnTrackFillBrushChanged

        #region OnTrackFillStyleChanged

        /// <summary>
        /// Called when value of the ThumbStyle property is changed.
        /// </summary>
        /// <param name="style">The style.</param>
        protected override void OnTrackFillStyleChanged(Style style)
        {
            base.OnThumbStyleChanged(style);
            foreach (XamSliderThumb<T> thumb in this.Thumbs)
            {



                if (thumb.TrackFill.Style == null)
                {
                    thumb.TrackFill.Style = style;
                }

            }
        }

        #endregion OnTrackFillStyleChanged

        #region OnVerticalTickMarksTemplateChanged

        /// <summary>
        /// Called when the value of the VerticalTickMarksTemplate
        /// property is changed.
        /// </summary>
        /// <param name="newValue">The new value - DataTemplate.</param>
        protected override void OnVerticalTickMarksTemplateChanged(DataTemplate newValue)
        {
            base.OnVerticalTickMarksTemplateChanged(newValue);
            this.TickMarksPanel.InvalidateArrange();
        }

        #endregion OnVerticalTickMarksTemplateChanged

        #region ProcessChanges

        /// <summary>
        /// Change the value of the active thumb with specified LargeChange or SmallChange value.
        /// </summary>
        /// <param name="isIncrease">if set to <c>true</c> value is increased.</param>
        /// <param name="isLargeChange">if set to <c>true</c> LargeChange is used.</param>
        /// <param name="forceMoveOneTick">if set to <c>true</c> and Snap to Tick is True, the thumb will move to the next tick mark regardless of change size.</param>
        protected internal override void ProcessChanges(bool isIncrease, bool isLargeChange, bool forceMoveOneTick)
        {
            if (this.IsDirectionReversed)
                isIncrease = !isIncrease;

            var activeThumb = this.ActiveThumb;

            double change = isLargeChange ? this.GetLargeChangeValue() : this.GetSmallChangeValue(); ;

            if (activeThumb != null && activeThumb.IsEnabled)
            {
                double thumbValue = this.ToDouble(activeThumb.Value);
                double max = this.ToDouble(this.MaxValue);
                double min = this.ToDouble(this.MinValue);

                if ((isIncrease && thumbValue >= max) || (!isIncrease && thumbValue <= min))
                    return;

                activeThumb.InitialValue = activeThumb.Value;
                if (isIncrease)
                {
                    thumbValue += change;
                }
                else
                {
                    thumbValue -= change;
                }

                if (thumbValue > max)
                    thumbValue = max;
                if (thumbValue < min)
                    thumbValue = min;

                if (activeThumb.IsSnapToTickEnabled && forceMoveOneTick)
                {
                    thumbValue = NextTickMarkValue(thumbValue, isIncrease);
                }

                activeThumb.Value = activeThumb.PreviewCoerceValue(this.ToValue(thumbValue), activeThumb.InitialValue);
            }
        }

        #endregion ProcessChanges

        #endregion Overrides

        #region Properties

        #region Public

        #region LargeChange

        /// <summary>
        /// Identifies the <see cref="LargeChange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register("LargeChange", typeof(double), typeof(XamSliderBase<T>), new PropertyMetadata(new PropertyChangedCallback(LargeChangeChanged)));

        /// <summary>
        /// Gets or sets the value of the LargeChange property.
        /// </summary>
        /// <value>The large change.</value>
        public virtual double LargeChange
        {
            get { return (double)this.GetValue(LargeChangeProperty); }
            set { this.SetValue(LargeChangeProperty, value); }
        }

        private static void LargeChangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase<T> slider = obj as XamSliderBase<T>;
            if (slider != null)
            {
                slider.OnPropertyChanged("LargeChange");
            }
        }

        #endregion LargeChange

        #region MaxValue

        /// <summary>
        /// Identifies the <see cref="MaxValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(T), typeof(XamSliderBase<T>), new PropertyMetadata(new PropertyChangedCallback(MaxValueChanged)));

        /// <summary>
        /// Gets or sets the maximum allowable value for this slider's range.
        /// </summary>
        /// <value>The max value.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual T MaxValue
        {
            get { return (T)this.GetValue(MaxValueProperty); }
            set { this.SetValue(MaxValueProperty, value); }
        }

        private static void MaxValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase<T> slider = obj as XamSliderBase<T>;
            if (!slider.SliderLoaded)
                return;
            if (slider != null && e.NewValue != e.OldValue)
            {
                if (slider.CoerceMinMaxValue(false))
                {
                    slider.EnsureSliderElements();
                    foreach (XamSliderThumb<T> thumb in slider.Thumbs)
                    {
                        thumb.UpdateThumbPosition();
                    }
                }

                slider.TickMarksPanel.GenerateTicks();

                slider.OnPropertyChanged("MaxValue");
            }
        }

        #endregion MaxValue

        #region MinValue

        /// <summary>
        /// Identifies the <see cref="MinValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(T), typeof(XamSliderBase<T>), new PropertyMetadata(new PropertyChangedCallback(MinValueChanged)));

        /// <summary>
        /// Gets or sets the minimum allowable value for this slider's range.
        /// </summary>
        /// <value>The min value.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual T MinValue
        {
            get { return (T)this.GetValue(MinValueProperty); }
            set { this.SetValue(MinValueProperty, value); }
        }

        private static void MinValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase<T> slider = obj as XamSliderBase<T>;
            if (!slider.SliderLoaded)
                return;
            if (slider != null)
            {
                if (slider.CoerceMinMaxValue(true))
                {
                    slider.EnsureSliderElements();
                    foreach (XamSliderThumb<T> thumb in slider.Thumbs)
                    {
                        thumb.UpdateThumbPosition();
                    }
                }

                slider.TickMarksPanel.GenerateTicks();

                slider.OnPropertyChanged("MinValue");
            }
        }

        #endregion MinValue

        #region SmallChange



        /// <summary>
        /// Identifies the <see cref="SmallChange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(XamSliderBase<T>), new PropertyMetadata(new PropertyChangedCallback(SmallChangeChanged)));

        /// <summary>
        /// Gets or sets the value of the SmallChange property.
        /// </summary>
        /// <value>The small change.</value>
        public virtual double SmallChange
        {
            get { return (double)this.GetValue(SmallChangeProperty); }
            set { this.SetValue(SmallChangeProperty, value); }
        }

        private static void SmallChangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase<T> slider = obj as XamSliderBase<T>;
            if (slider != null)
            {
                slider.OnPropertyChanged("SmallChange");
            }
        }


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


        #endregion SmallChange

        #region TickMarks

        /// <summary>
        /// Gets the collection of tick marks - <see cref="SliderTickMarks&lt;T&gt;"/>
        /// instances.
        /// </summary>
        /// <value>The tick marks.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ObservableCollection<SliderTickMarks<T>> TickMarks
        {
            get
            {
                if (this._tickmarks == null)
                {
                    this._tickmarks = new ObservableCollection<SliderTickMarks<T>>();
                    this._tickmarks.CollectionChanged += this.TickMarks_CollectionChanged;
                }

                return this._tickmarks;
            }
        }

        #endregion TickMarks

        #endregion Public

        #region Internal

        #region ActiveThumb






        internal static readonly DependencyProperty ActiveThumbProperty = DependencyProperty.Register("ActiveThumb", typeof(XamSliderThumb<T>), typeof(XamSliderBase<T>), new PropertyMetadata(new PropertyChangedCallback(ActiveThumbChanged)));



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


        internal XamSliderThumb<T> ActiveThumb
        {
            get { return (XamSliderThumb<T>)this.GetValue(ActiveThumbProperty); }
            set { this.SetValue(ActiveThumbProperty, value); }
        }

        private static void ActiveThumbChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase<T> slider = obj as XamSliderBase<T>;
            if (slider != null)
                if (slider.ActiveThumb != null)
                {
                    if (e.OldValue != null)
                        ((XamSliderThumb<T>)e.OldValue).IsActive = false;
                    if (!((XamSliderThumb<T>)e.NewValue).IsActive)
                        ((XamSliderThumb<T>)e.NewValue).IsActive = true;

                    slider.BringToFront();
                }
        }

        #endregion ActiveThumb

        #region SortedThumbs

        internal List<XamSliderThumb<T>> SortedThumbs
        {
            get;
            set;
        }

        #endregion SortedThumbs

        #region SliderLoaded

        internal bool SliderLoaded
        { get; set; }

        #endregion

        #endregion Internal

        #region Protected

        #region Thumbs

        /// <summary>
        /// Gets the collection of thumbs - <see cref="XamSliderThumb&lt;T&gt;"/>
        /// instances.
        /// </summary>
        /// <value>The thumbs.</value>
        protected internal virtual ObservableCollection<XamSliderThumb<T>> Thumbs
        {
            get
            {
                if (this._thumbs == null)
                {
                    this._thumbs = new ObservableCollection<XamSliderThumb<T>>();
                    this._thumbs.CollectionChanged += this.Thumbs_CollectionChanged;
                }

                return this._thumbs;
            }
        }

        #endregion Thumbs

        #region TickMarksPanel

        /// <summary>
        /// Gets the tick marks panel that arranges the tick marks.
        /// </summary>
        /// <value>The tick marks panel.</value>
        protected TickMarksPanel<T> TickMarksPanel
        {
            get
            {
                if (this._tickMarksPanel == null)
                {
                    this._tickMarksPanel = new TickMarksPanel<T> { Owner = this };
                }

                return this._tickMarksPanel;
            }
        }

        #endregion TickMarksPanel

        #endregion Protected

        #endregion Properties

        #region Methods

        #region Internal Methods

        #region NextLeft Helper Functions

        /// <summary>
        /// Returns the first thumb to the left, or null, regardless of enabled, disable, etc.
        /// </summary>
        /// <param name="thumbIndex">The index of the thumb to look left from, not the index of the first thumb to the left!!</param>
        /// <returns>The first left thumb, or null</returns>
        internal XamSliderThumb<T> NextLeftThumb(int thumbIndex)
        {
            if (thumbIndex == 0)
                return null;

            return SortedThumbs[thumbIndex - 1];
        }

        /// <summary>
        /// Returns the first thumb to the left that is interactable, or null
        /// </summary>
        /// <param name="thumbIndex">The index of the thumb to look left from, not the index of the first thumb to the left!!</param>
        /// <returns>The first interactable thumb to the left, or null</returns>
        internal XamSliderThumb<T> NextLeftInteractableThumb(int thumbIndex)
        {
            while (thumbIndex > 0)
            {
                var thumb = NextLeftThumb(thumbIndex);
                if (thumb != null && thumb.IsThumbValidForInteraction())
                    return thumb;
                thumbIndex--;
            }

            return null;
        }

        /// <summary>
        /// Returns the first thumb to the left that has the interaction we are looking for, or null
        /// </summary>
        /// <param name="thumbIndex">The index of the thumb to look left from, not the index of the first thumb to the left!!</param>
        /// <param name="interactionMode">The desired interaction mode</param>
        /// <returns>The first interactable thumb of the specified mode, or null</returns>
        internal XamSliderThumb<T> NextLeftInteractableThumb(int thumbIndex, SliderThumbInteractionMode interactionMode)
        {
            var thumb = NextLeftThumb(thumbIndex);

            while (thumb != null)
            {
                //Is it valid and also the interaction mode we want?
                if (thumb.IsThumbValid() && thumb.InteractionMode == interactionMode)
                    return thumb;
                //Let's look at the next one!
                thumbIndex--;
                thumb = NextLeftThumb(thumbIndex);
            }

            return null;
        }

        #endregion NextLeft Helper Functions

        #region NextRight Helper Functions

        /// <summary>
        /// Returns the first thumb to the right, or null, regardless of enabled, disable, etc.
        /// </summary>
        /// <param name="thumbIndex">The index of the thumb to look right from, not the index of the first thumb to the right!!</param>
        /// <returns>The first right thumb, or null</returns>
        internal XamSliderThumb<T> NextRightThumb(int thumbIndex)
        {
            if (thumbIndex == SortedThumbs.Count - 1)
                return null;

            return SortedThumbs[thumbIndex + 1];
        }

        /// <summary>
        /// Returns the first thumb to the right that is interactable, or null
        /// </summary>
        /// <param name="thumbIndex">The index of the thumb to look right from, not the index of the first thumb to the right!!</param>
        /// <returns>The first interactable thumb to the right, or null</returns>
        internal XamSliderThumb<T> NextRightInteractableThumb(int thumbIndex)
        {
            while (thumbIndex < SortedThumbs.Count - 1)
            {
                var thumb = NextRightThumb(thumbIndex);
                if (thumb != null && thumb.IsThumbValidForInteraction())
                    return thumb;
                thumbIndex++;
            }

            return null;
        }

        /// <summary>
        /// Returns the first thumb to the left that has the interaction we are looking for, or null
        /// </summary>
        /// <param name="thumbIndex">The index of the thumb to look left from, not the index of the first thumb to the left!!</param>
        /// <param name="interactionMode">The desired interaction mode</param>
        /// <returns>The first interactable thumb of the specified mode, or null</returns>
        internal XamSliderThumb<T> NextRightInteractableThumb(int thumbIndex, SliderThumbInteractionMode interactionMode)
        {
            var thumb = NextRightThumb(thumbIndex);

            while (thumb != null)
            {
                //Is it valid and also the interaction mode we want?
                if (thumb.IsThumbValid() && thumb.InteractionMode == interactionMode)
                    return thumb;
                //Let's look at the next one!
                thumbIndex++;
                thumb = NextRightThumb(thumbIndex);
            }

            return null;
        }

        #endregion NextRight Helper Functions

        #region DriveThumb[Left/Right]

        /// <summary>
        /// Takes the given thumb and drives it the left aiming at the value specified (previewValue)
        /// This function handles the interactions with other thumbs, and potentially can change the landing value of the thumb passed in.
        /// That's why the previewValue is a preview, and not the actual value of the thumb!
        /// NOTE: Any changes to the logic here should probably be done on DriveThumbRight as well, these two function should stay pretty in sync!
        /// </summary>
        /// <param name="thumb">The thumb to move</param>
        /// <param name="previewValue">The value to try and reach</param>
        /// <returns>The result of moving the thumb, this could be different that the value passed in if we hit a lock!</returns>
        internal double DriveThumbLeft(XamSliderThumb<T> thumb, double previewValue)
        {
            //Find the first thumb to the left that is NOT free!
            var nextLeft = NextLeftInteractableThumb(thumb.SliderThumbIndex);

            //If there is no thumb to the left, then there is nothing to do!
            if (nextLeft == null)
                return previewValue;

            //Cache the value for later.
            var nextLeftValue = nextLeft.ResolveValue();

            //If the next left thumb is going to be passed by this thumb, we need to check for interactions!
            if (nextLeftValue > previewValue)
            {
                //If the next left thumb is locked we need to adjust back accordingly
                //By Calling Preview on this thumb passing in the value of the locking thumb, we can automatically find the appropriate place to land.
                //This will make sure to snap to tick if needed...
                //Additionally will over lap
                if (nextLeft.InteractionMode == SliderThumbInteractionMode.Lock)
                    return ToDouble(thumb.PreviewCoerceValue(nextLeft.Value, ToValue(previewValue)));

                //If the next left thumb is push we need to move the thumb accordingly, however
                //We call the MoveThumbTo function for the next thumb incase moving that thumb causes additional interactions!
                //However we need to make sure the pushed thumb doesn't hit a lock.  That's why we look down the slider for a locked thumb
                //If we find one, we need to modify our preview value so we don't break snap to tick if applied.
                if (nextLeft.InteractionMode == SliderThumbInteractionMode.Push)
                {
                    //Look for the first Locked thumb to the left of us
                    var lockedThumb = NextLeftInteractableThumb(thumb.SliderThumbIndex, SliderThumbInteractionMode.Lock);

                    //Ok, so there is a thumb, and we are going to hit it.
                    if (lockedThumb != null && lockedThumb.ResolveValue() > previewValue)
                    {
                        //We are going to hit a locked thumb during our push.
                        //We need to adjust our value accordingly.
                        previewValue = ToDouble(thumb.PreviewCoerceValue(lockedThumb.Value, ToValue(previewValue)));
                        //Make sure thumb being pushed is on top of the locked thumb, not underneath!
                        OverlapThumb(nextLeft, lockedThumb, previewValue);
                    }

                    //Move this left thumb to our preview value, if in-fact it's still to the left of it!
                    if (nextLeftValue > previewValue)
                        QuietlyMoveThumbTo(nextLeft, ToValue(previewValue));
                }
            }

            return previewValue;
        }

        /// <summary>
        /// Takes the given thumb and drives it the right aiming at the value specified (previewValue)
        /// This function handles the interactions with other thumbs, and potentially can change the landing value of the thumb passed in.
        /// That's why the previewValue is a preview, and not the actual value of the thumb!
        /// NOTE: Any changes to the logic here should probably be done on DriveThumbLeft as well, these two function should stay pretty in sync!
        /// </summary>
        /// <param name="thumb">The thumb to move</param>
        /// <param name="previewValue">The value to try and reach</param>
        /// <returns>The result of moving the thumb, this could be different that the value passed in if we hit a lock!</returns>
        internal double DriveThumbRight(XamSliderThumb<T> thumb, double previewValue)
        {
            //Find the first thumb to the right that is NOT free!
            var nextRight = NextRightInteractableThumb(thumb.SliderThumbIndex);

            //If there is no thumb to the right, then there is nothing to do!
            if (nextRight == null)
                return previewValue;

            //Cache the value for later.
            var nextRightValue = nextRight.ResolveValue();

            //If the next right thumb is going to be passed by this thumb, we need to check for interactions!
            if (nextRightValue < previewValue)
            {
                //If the next right thumb is locked we need to adjust back accordingly
                //By Calling Preview on this thumb passing in the value of the locking thumb, we can automatically find the appropriate place to land.
                //This will make sure to snap to tick if needed...
                if (nextRight.InteractionMode == SliderThumbInteractionMode.Lock)
                    return ToDouble(thumb.PreviewCoerceValue(nextRight.Value, ToValue(previewValue)));

                //If the next right thumb is push, we need to move the thumb accordingly, however
                //We call the MoveThumbTo function for the next thumb incase moving that thumb causes additional interactions!
                //However we need to make sure the pushed thumb doesn't hit a lock.  That's why we look down the slider for a locked thumb
                //If we find one, we need to modify our preview value so we don't break snap to tick if applied.
                if (nextRight.InteractionMode == SliderThumbInteractionMode.Push)
                {
                    //Look for the first Locked thumb to the left of us
                    var lockedThumb = NextRightInteractableThumb(thumb.SliderThumbIndex, SliderThumbInteractionMode.Lock);

                    //Ok, so there is a thumb, and we are going to hit it.
                    if (lockedThumb != null && lockedThumb.ResolveValue() < previewValue)
                    {
                        //We are going to hit a locked thumb during our push.
                        //We need to adjust our value accordingly.
                        previewValue = ToDouble(thumb.PreviewCoerceValue(lockedThumb.Value, ToValue(previewValue)));
                        //Make sure thumb being pushed is on top of the locked thumb, not underneath!
                        OverlapThumb(nextRight, lockedThumb, previewValue);
                    }

                    //Move this right thumb to our preview value, if in-fact it's still to the right of it!
                    if (nextRightValue < previewValue)
                        QuietlyMoveThumbTo(nextRight, ToValue(previewValue));
                }
            }

            return previewValue;
        }

        #endregion DriveThumb[Left/Right]

        #region QuietlyMoveThumbTo

        /// <summary>
        /// This private function moves a thumb to a given position and will interact with thumbs in it path.
        /// </summary>
        /// <param name="thumb">The thumb to move</param>
        /// <param name="previewValue">The value to aim for</param>
        private void QuietlyMoveThumbTo(XamSliderThumb<T> thumb, T previewValue)
        {
            QuietlyMoveThumbTo(thumb, previewValue, true);
        }

        /// <summary>
        /// This private function moves a thumb to a given position and will interact with thumbs in it path.
        /// </summary>
        /// <param name="thumb">The thumb to move</param>
        /// <param name="previewValue">The value to aim for</param>
        /// <param name="provideInteractions">Attempts to the move the thumb respecting the interaction with other thumbs</param>
        private void QuietlyMoveThumbTo(XamSliderThumb<T> thumb, T previewValue, bool provideInteractions)
        {
            double initialValue = ToDouble(thumb.InitialValue);
            double thumbValue = ToDouble(thumb.Value);
            double newValue = ToDouble(previewValue);

            if (provideInteractions)
            {

            //If we are WPF we only want to coerce the movement of the thumb after it's been loaded!
            if (thumb.IsLoaded)
            {


                //Determine the direction of the Thumb Movement
                // If is decreasing, else is increasing
                if (initialValue > newValue)
                    newValue = DriveThumbLeft(thumb, newValue);
                else
                    newValue = DriveThumbRight(thumb, newValue);


            }

            }

            //The value has changed since we set it, updated it, but quietly, we don't want to be called back here because we changed the value!
            if (thumbValue != newValue)
                thumb.QuietlySetValue(ToValue(newValue));
        }

        #endregion QuietlyMoveThumbTo

        #region MoveThumbTo

        /// <summary>
        /// This is the method called by the Thumb to move it to a new value.
        /// This guy should be the ONLY method to move thumbs since he knows how to ensure proper interaction between them
        /// </summary>
        /// <param name="thumb">The instance of the thumb to move</param>
        /// <param name="previewValue">The value to try to move the thumb to</param>
        internal void MoveThumbTo(XamSliderThumb<T> thumb, T previewValue)
        {
            MoveThumbTo(thumb, previewValue, true);
        }

        /// <summary>
        /// This is the method called by the Thumb to move it to a new value.
        /// This guy should be the ONLY method to move thumbs since he knows how to ensure proper interaction between them
        /// </summary>
        /// <param name="thumb">The instance of the thumb to move</param>
        /// <param name="previewValue">The value to try to move the thumb to</param>
        /// <param name="provideInteractions">Attempts to the move the thumb respecting the interaction with other thumbs</param>
        internal void MoveThumbTo(XamSliderThumb<T> thumb, T previewValue, bool provideInteractions)
        {
            //Actually move the thumb to the locating as well as interact with out thumbs
            QuietlyMoveThumbTo(thumb, previewValue, provideInteractions);

            //Make sure the value of the slider reflects the value of this thumb if the slider is not a range slider.
            XamSimpleSliderBase<T> slider = this as XamSimpleSliderBase<T>;
            if (slider != null)
            {
                double updatedValue = ToDouble(thumb.Value);
                double sliderValue = slider.ToDouble(slider.Value);

                //Update the value if they are different
                if (sliderValue != updatedValue)
                    slider.Value = ToValue(updatedValue);
            }

            //Ok, our values have potentially changed, lets re-sort!
            SortThumbs();

            
            this.ArrangeThumbs2();
            this.EnsureTrackFill();
        }

        #endregion MoveThumbTo

        #region SortThumbs

        /// <summary>
        /// Sorts thumbs by value, if the value is the same, will still know which one was originally left and originally right.
        /// </summary>
        internal void SortThumbs()
        {
            this.SortedThumbs = new List<XamSliderThumb<T>>(this.Thumbs);
            this.SortedThumbs.Sort(new ThumbComparer<T>());

            //Persist the newly sorted values
            for (int i = 0; i < this.SortedThumbs.Count; i++)
                this.SortedThumbs[i].SliderThumbIndex = i;
        }

        #endregion SortThumbs

        #region GenerateTickMarks







        protected internal void GenerateTickMarks()
        {
            this.TickMarksPanel.GenerateTicks();
        }

        #endregion GenerateTickMarks

        #region ToDouble

        internal double ToDouble(T value)
        {
            return this.ValueToDouble(value);
        }

        #endregion ToDouble

        #region ToValue

        internal T ToValue(double value)
        {
            return this.DoubleToValue(value);
        }

        #endregion ToValue

        #endregion Internal Methods

        #region Protected Methods

        #region CoerceMinMaxValue

        /// <summary>
        /// Coerces the min max values when change one of them.
        /// </summary>
        /// <param name="isMinValueChanged">if set to <c>true</c> [is min value changed].</param>
        /// <returns>true if values are correct, otherwise false</returns>
        protected internal virtual bool CoerceMinMaxValue(bool isMinValueChanged)
        {
            double delta = this.GetDelta();
            double min = this.ToDouble(this.MinValue);
            double max = this.ToDouble(this.MaxValue);
            if (max <= min && this.ActualHeight > 0 && this.ActualWidth > 0)
            {
                if (!isMinValueChanged)
                {
                    min = max - delta;
                    this.MinValue = this.ToValue(min);
                }
                else
                {
                    max = min + delta;
                    this.MaxValue = this.ToValue(max);
                }

                return false;
            }

            return true;
        }

        #endregion CoerceMinMaxValue

        #region DoubleToValue

        /// <summary>
        /// Converts value double type to specific generic type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>value from generic type</returns>
        protected virtual T DoubleToValue(double value)
        {
            object newValue = value;
            return (T)newValue;
        }

        #endregion DoubleToValue

        #region GetDelta

        /// <summary>
        /// Gets the delta, used to coerse the min max values.
        /// </summary>
        /// <returns>delta as double </returns>
        protected virtual double GetDelta()
        {
            return 1;
        }

        #endregion GetDelta

        #region GetLargeChangeValue

        /// <summary>
        /// Gets the LargeChange value in double.
        /// </summary>
        /// <returns>value in double</returns>
        protected internal virtual double GetLargeChangeValue()
        {
            return this.LargeChange;
        }

        /// <summary>
        /// Gets the LargeChange value in double, based off of a given starting point (but not including it, used for DateTime).
        /// </summary>
        /// <param name="baseValue">The starting point to add the large value to.</param>
        /// <param name="isIncreasing">Indicated whether the change is positive or negative.</param>
        /// <returns>value in double based of the starting point (but not including it, used for DateTime)</returns>
        protected internal virtual double GetLargeChangeValue(double baseValue, bool isIncreasing)
        {
            double value = GetLargeChangeValue();
            return isIncreasing ? value : value * -1;
        }

        #endregion GetLargeChangeValue

        #region GetSmallChangeValue

        /// <summary>
        /// Gets the SmallChange value in double.
        /// </summary>
        /// <returns>value in double</returns>
        protected internal virtual double GetSmallChangeValue()
        {
            return this.SmallChange;
        }

        #endregion GetSmallChangeValue

        #region OnThumbValueChanged

        /// <summary>
        /// Called when the value af the any slide rthumb is changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="thumb">The thumb.</param>
        /// <returns>true if event i cancelled</returns>
        protected internal virtual bool OnThumbValueChanged(T oldValue, T newValue, XamSliderThumb<T> thumb)
        {
            EventHandler<ThumbValueChangedEventArgs<T>> thumbValueChanged = this.ThumbValueChanged;
            if (thumbValueChanged != null)
            {
                ThumbValueChangedEventArgs<T> args = new ThumbValueChangedEventArgs<T>
                                                         {
                                                             OldValue = oldValue,
                                                             NewValue = newValue,
                                                             Thumb = thumb
                                                         };
                thumbValueChanged(this, args);
                return args.Cancel;
            }

            return false;
        }

        #endregion OnThumbValueChanged

        #region ValueToDouble

        /// <summary>
        /// Converts value from specific generic type to double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Double value</returns>
        protected virtual double ValueToDouble(T value)
        {
            return 0;
        }

        #endregion ValueToDouble

        #region ResolveThumbLocation

        /// <summary>
        /// Resolves the thumb location on the slider track.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="size">The size of the track.</param>
        /// <returns>Position of the thumb relative to track</returns>
        protected virtual double ResolveThumbLocation(T value, double size)
        {
            double diff = this.ToDouble(this.MaxValue) - this.ToDouble(this.MinValue);
            if (diff == 0)
            {
                return 0;
            }

            double min = this.ToDouble(this.MinValue);
            double thumbValue = this.ToDouble(value) - min;
            if (thumbValue < 0)
            {
                thumbValue = 0;
            }

            double percent = thumbValue / diff;
            if (percent > 1)
            {
                percent = 1;
            }

            if (this.IsDirectionReversed)
            {
                return size - (size * percent);
            }

            return size * percent;
        }

        #endregion ResolveThumbLocation

        /// <summary>
        /// Will iterate through all of the tick marks starting at the startingValue to find the next tick-mark either left or right depending on the searchRight
        /// </summary>
        /// <param name="startingValue">The value to start the search from.</param>
        /// <param name="searchRight">True = search to the right, increasing.  False = search to the left, decreasing.</param>
        /// <returns>The value of the next tick-mark.</returns>
        protected double NextTickMarkValue(double startingValue, bool searchRight)
        {
            double min = ToDouble(MinValue);
            double max = ToDouble(MaxValue);
            
            foreach (SliderTickMarks<T> tickmarks in TickMarks)
            {
                foreach (double tickmark in tickmarks.ResolvedTickMarks.Select(tickValue => ToDouble(tickValue)))
                {
                    if (tickmark == startingValue)
                        return startingValue;
                    
                    if ((tickmark > startingValue) && (tickmark < max))
                        max = tickmark;
                    if ((tickmark < startingValue) && (tickmark > min))
                        min = tickmark;
                }
            }

            if (searchRight)
                return max;

            return min;
        }

        #endregion // Protected Methods

        #region Private Methods

        #region BringToFront

        private void BringToFront()
        {
            Canvas panel = (this.Orientation == Orientation.Horizontal)
                               ? this.HorizontalThumbsPanel
                               : this.VerticalThumbsPanel;
            int zindex = 0;
            if (panel != null)
            {
                foreach (XamSliderThumb<T> thumb in this.Thumbs)
                {
                    zindex++;
                    if (thumb != this.ActiveThumb)
                    {
                        Canvas.SetZIndex(thumb, zindex);
                    }
                }

                Canvas.SetZIndex(this.ActiveThumb, zindex + this.Thumbs.Count);
            }
        }

        #endregion BringToFront

        #region OverlapThumb

        /// <summary>
        /// Take the "topThumb" and changes the ZIndex to be on top of "bottomThumb"
        /// </summary>
        /// <param name="topThumb">The thumb to be on top</param>
        /// <param name="bottomThumb">The thumb to be on bottom</param>
        private void OverlapThumb(XamSliderThumb<T> topThumb, XamSliderThumb<T> bottomThumb, double newValue)
        {
            //If the new value isn't equal to our thumb then we don't care!
            if (newValue != bottomThumb.ResolveValue())
                return;

            int thumbZindex = Canvas.GetZIndex(bottomThumb);
            int zindex = Canvas.GetZIndex(topThumb);
            if (thumbZindex > zindex)
            {
                Canvas.SetZIndex(topThumb, ++thumbZindex);
            }
        }

        #endregion OverlapThumb

        #region ClearThumb

        private void ClearThumb(XamSliderThumb<T> thumb)
        {
            if (this.VerticalThumbsPanel != null && this.VerticalThumbsPanel.Children.Contains(thumb))
            {
                this.VerticalThumbsPanel.Children.Remove(thumb);
            }

            if (this.VerticalTrackFillsPanel != null && this.VerticalTrackFillsPanel.Children.Contains(thumb.TrackFill))
            {
                this.VerticalTrackFillsPanel.Children.Remove(thumb.TrackFill);
            }

            if (this.HorizontalThumbsPanel != null && this.HorizontalThumbsPanel.Children.Contains(thumb))
            {
                this.HorizontalThumbsPanel.Children.Remove(thumb);
            }

            if (this.HorizontalTrackFillsPanel != null && this.HorizontalTrackFillsPanel.Children.Contains(thumb.TrackFill))
            {
                this.HorizontalTrackFillsPanel.Children.Remove(thumb.TrackFill);
            }
        }

        #endregion ClearThumb



        #region MouseWheelMoved

        private void MouseWheelMoved(MouseWheelEventArgs args)
        {
            double delta = args.Delta;
            args.Handled = true;
            if (this.ActiveThumb != null)
            {
                if (delta < 0.0)
                {
                    this.ProcessChanges(true, false, false);
                }
                else
                {
                    this.ProcessChanges(false, false, false);
                }
            }
        }

        #endregion MouseWheelMoved



        #region RemoveTrackFillFromHorizontalTrackFillsPanel

        private void RemoveTrackFillFromHorizontalTrackFillsPanel()
        {
            if (this.HorizontalTrackFillsPanel != null)
            {
                foreach (XamSliderThumb<T> thumb in this.Thumbs)
                {
                    if (this.HorizontalTrackFillsPanel.Children.Contains(thumb.TrackFill))
                    {
                        this.HorizontalTrackFillsPanel.Children.Remove(thumb.TrackFill);
                    }
                }
            }
        }

        #endregion RemoveTrackFillFromHorizontalTrackFillsPanel

        #region RemoveThumbsFromHorizontalThumbsPanel

        private void RemoveThumbsFromHorizontalThumbsPanel()
        {
            if (this.HorizontalThumbsPanel != null)
            {
                foreach (XamSliderThumb<T> thumb in this.Thumbs)
                {
                    if (this.HorizontalThumbsPanel.Children.Contains(thumb))
                    {

                            this.RemoveLogicalChild(thumb);

                        this.HorizontalThumbsPanel.Children.Remove(thumb);
                    }
                }
            }
        }

        #endregion RemoveThumbsFromHorizontalThumbsPanel

        #region RemoveTrackFillFromVerticalTrackFillsPanel

        private void RemoveTrackFillFromVerticalTrackFillsPanel()
        {
            if (this.VerticalTrackFillsPanel != null)
            {
                foreach (XamSliderThumb<T> thumb in this.Thumbs)
                {
                    if (this.VerticalTrackFillsPanel.Children.Contains(thumb.TrackFill))
                    {
                        this.VerticalTrackFillsPanel.Children.Remove(thumb.TrackFill);
                    }
                }
            }
        }

        #endregion RemoveTrackFillFromVerticalTrackFillsPanel

        #region RemoveThumbsFromVerticalThumbsPanel

        private void RemoveThumbsFromVerticalThumbsPanel()
        {
            if (this.VerticalThumbsPanel != null)
            {
                foreach (XamSliderThumb<T> thumb in this.Thumbs)
                {
                    if (this.VerticalThumbsPanel.Children.Contains(thumb))
                    {

                            this.RemoveLogicalChild(thumb);

                        this.VerticalThumbsPanel.Children.Remove(thumb);
                    }
                }
            }
        }

        #endregion RemoveThumbsFromVerticalThumbsPanel

        #endregion Private Methods

        #endregion Methods

        #region Events

        /// <summary>
        /// Occurs when value of the any item from Thumbs collection is changed.
        /// </summary>
        public event EventHandler<ThumbValueChangedEventArgs<T>> ThumbValueChanged;

        /// <summary>
        /// Occurs when there is mouse click over the track of the XamSlider.
        /// </summary>
        public event EventHandler<TrackClickEventArgs<T>> TrackClick;

        #endregion Events

        #region EventHandlers

        #region Thumbs_CollectionChanged

        private void Thumbs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (XamSliderThumb<T> thumb in e.NewItems)
                {
                    thumb.Owner = this;
                    if (thumb.TrackFillBrush == null)
                        thumb.TrackFillBrush = this.TrackFillBrush;

                    AddLogicalChild(thumb);

                    Debug.WriteLine("Thumb Added!");
                    //This prevents the user from throwing in thumbs with invalid values!
                    if (this.SliderLoaded)
                        thumb.QuietlySetValue(thumb.PreviewCoerceValue(thumb.Value));
                }
            }

            if (e.OldItems != null)
            {
                foreach (XamSliderThumb<T> thumb in e.OldItems)
                {
                    if (thumb.IsActive)
                    {
                        this.ActiveThumb = null;
                        thumb.IsActive = false;
                    }

                    thumb.Owner = null;

                    this.ClearThumb(thumb);
                }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset && this.SortedThumbs != null)
            {
                foreach (XamSliderThumb<T> thumb in this.SortedThumbs)
                {
                    if (thumb.IsActive)
                    {
                        this.ActiveThumb = null;
                        thumb.IsActive = false;
                    }

                    thumb.Owner = null;
                    this.ClearThumb(thumb);
                }
            }

            SortThumbs();

            this.EnsureOrientation();
        }

        #endregion Thumbs_CollectionChanged

        #region TickMarks_CollectionChanged

        private void TickMarks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SliderTickMarks<T> tickmarks in e.NewItems)
                {
                    tickmarks.Owner = this;
                }
            }

            this.EnsureOrientation();
            this.EnsureTickMarks();
        }

        #endregion TickMarks_CollectionChanged

        #region XamSliderBase_LayoutUpdated

        private void XamSliderBase_LayoutUpdated(object sender, EventArgs e)
        {
            this.ArrangeThumbs2();
            this.EnsureTrackFill();

            //Bug 33211 fix - Mihail Mateev 06/03/2010
            if (this._isOrientationChanged)
            {
                this.TickMarksPanel.GenerateTicks();
                this._isOrientationChanged = false;
            }
        }

        #endregion XamSliderBase_LayoutUpdated

        #region XamSliderBase_Loaded

        private void XamSliderBase_Loaded(object sender, RoutedEventArgs e)
        {
            this.SliderLoaded = true;
            if (CoerceMinMaxValue(false))
            {
                EnsureSliderElements();
                foreach (XamSliderThumb<T> thumb in Thumbs)
                {
                    thumb.UpdateThumbPosition();
                }
            }
            
            TickMarksPanel.GenerateTicks();

            foreach (XamSliderThumb<T> thumb in Thumbs)
            {
                thumb.QuietlySetValue(thumb.PreviewCoerceValue(thumb.Value));
            }

            //In Wpf there is a chance that the thumbs may not be sorted properly due to the delayed binding.  Lets force sort really quick.
            this.SortThumbs();
        }

        #endregion
                
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