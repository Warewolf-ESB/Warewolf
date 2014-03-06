using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    ///  An object that describes base class for different types of sliders
    /// </summary>
    [TemplatePart(Name = "HorizontalTrack", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "VerticalTrack", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "HorizontalThumbs", Type = typeof(Canvas))]
    [TemplatePart(Name = "VerticalThumbs", Type = typeof(Canvas))]
    [TemplatePart(Name = "HorizontalTrackFills", Type = typeof(Canvas))]
    [TemplatePart(Name = "VerticalTrackFills", Type = typeof(Canvas))]
    [TemplatePart(Name = "HorizontalTickMarks", Type = typeof(Grid))]
    [TemplatePart(Name = "VerticalTickMarks", Type = typeof(Grid))]
    [TemplateVisualState(GroupName = "OrientationStates", Name = "Horizontal")]
    [TemplateVisualState(GroupName = "OrientationStates", Name = "Vertical")]
    [StyleTypedProperty(Property = "ThumbStyle", StyleTargetType = typeof(XamSliderThumbBase))]
    [StyleTypedProperty(Property = "TrackFillStyle", StyleTargetType = typeof(TrackFill))]
    public abstract class XamSliderBase : Control, ICommandTarget, INotifyPropertyChanged
    {
        #region Constructor


        static XamSliderBase()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamSliderBase), new FrameworkPropertyMetadata(typeof(XamSliderBase)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="XamSliderBase"/> class.
        /// </summary>
        protected XamSliderBase()
        {

			Infragistics.Windows.Utilities.ValidateLicense(typeof(XamSliderBase), this);


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(XamSliderBase_IsEnabledChanged);
        }

        #endregion Constructor

        #region Properties

        #region Public



        #region IncreaseButtonVisibility

        /// <summary>
        /// Identifies the <see cref="IncreaseButtonVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IncreaseButtonVisibilityProperty = DependencyProperty.Register("IncreaseButtonVisibility", typeof(Visibility), typeof(XamSliderBase), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the increase button visibility.
        /// It specifies the Visibility of the button in default template,
        /// that is used to increase with SmallChange value the value of the
        /// ActiveThumb
        /// </summary>
        /// <value>The increase button visibility.</value>
        public Visibility IncreaseButtonVisibility
        {
            get { return (Visibility)this.GetValue(IncreaseButtonVisibilityProperty); }
            set { this.SetValue(IncreaseButtonVisibilityProperty, value); }
        }

        #endregion IncreaseButtonVisibility

        #region DecreaseButtonVisibility

        /// <summary>
        /// Identifies the <see cref="DecreaseButtonVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DecreaseButtonVisibilityProperty = DependencyProperty.Register("DecreaseButtonVisibility", typeof(Visibility), typeof(XamSliderBase), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the decrease button visibility.
        /// It specifies the Visibility of the button in default template,
        /// that is used to decrease with SmallChange value the value of the
        /// ActiveThumb
        /// </summary>
        /// <value>The decrease button visibility.</value>
        public Visibility DecreaseButtonVisibility
        {
            get { return (Visibility)this.GetValue(DecreaseButtonVisibilityProperty); }
            set { this.SetValue(DecreaseButtonVisibilityProperty, value); }
        }

        #endregion DecreaseButtonVisibility





        #region EnableKeyboardNavigation

        /// <summary>
        /// Identifies the <see cref="EnableKeyboardNavigation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableKeyboardNavigationProperty = DependencyProperty.Register("EnableKeyboardNavigation", typeof(bool), typeof(XamSliderBase), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether is enable keyboard navigation.
        /// </summary>
        /// <value>
        /// <c>true</c> if keyboard navigation is enaled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableKeyboardNavigation
        {
            get { return (bool)this.GetValue(EnableKeyboardNavigationProperty); }
            set { this.SetValue(EnableKeyboardNavigationProperty, value); }
        }

        #endregion EnableKeyboardNavigation



        #region HorizontalTickMarksTemplate

        /// <summary>
        /// Identifies the <see cref="HorizontalTickMarksTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalTickMarksTemplateProperty = DependencyProperty.Register("HorizontalTickMarksTemplate", typeof(DataTemplate), typeof(XamSliderBase), new PropertyMetadata(new PropertyChangedCallback(HorizontalTickMarksTemplateChanged)));

        /// <summary>
        /// Gets or sets the DataTemplate for horizontal <see cref="SliderTickMarks&lt;T&gt;"/>.
        /// </summary>
        /// <value>The horizontal tick marks template.</value>
        public DataTemplate HorizontalTickMarksTemplate
        {
            get { return (DataTemplate)this.GetValue(HorizontalTickMarksTemplateProperty); }
            set { this.SetValue(HorizontalTickMarksTemplateProperty, value); }
        }

        private static void HorizontalTickMarksTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase owner = obj as XamSliderBase;
            if (owner != null)
            {
                DataTemplate newValue = e.NewValue as DataTemplate;
                owner.OnHorizontalTickMarksTemplateChanged(newValue);
            }
        }

        #endregion HorizontalTickMarksTemplate

        #region IsDirectionReversed

        /// <summary>
        /// Identifies the <see cref="IsDirectionReversed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDirectionReversedProperty = DependencyProperty.Register("IsDirectionReversed", typeof(bool), typeof(XamSliderBase), new PropertyMetadata(new PropertyChangedCallback(IsDirectionReversedChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether the direction
        /// form MinValue to MaxValue for this <see cref="XamSliderBase"/> instance is reversed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is direction reversed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectionReversed
        {
            get { return (bool)this.GetValue(IsDirectionReversedProperty); }
            set { this.SetValue(IsDirectionReversedProperty, value); }
        }

        private static void IsDirectionReversedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase slider = obj as XamSliderBase;
            if (slider != null)
            {
                slider.EnsureSliderElements();
            }
        }

        #endregion IsDirectionReversed



        #region IsMouseWheelEnabled

        /// <summary>
        /// Identifies the <see cref="IsMouseWheelEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsMouseWheelEnabledProperty = DependencyProperty.Register("IsMouseWheelEnabled", typeof(bool), typeof(XamSliderBase), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mouse wheel enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is mouse wheel enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsMouseWheelEnabled
        {
            get { return (bool)this.GetValue(IsMouseWheelEnabledProperty); }
            set { this.SetValue(IsMouseWheelEnabledProperty, value); }
        }

        #endregion IsMouseWheelEnabled



        #region Orientation

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(XamSliderBase), new PropertyMetadata(new PropertyChangedCallback(OrientationChanged)));

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.Controls.Orientation"/> orientation.
        /// </summary>
        /// <value>The orientation.</value>
        public Orientation Orientation
        {
            get { return (Orientation)this.GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        private static void OrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase slider = (XamSliderBase)obj;
            if (slider != null)
            {
                slider.EnsureCurrentState();
                slider.EnsureOrientation();
            }
        }

        #endregion Orientation

        #region ThumbStyle

        /// <summary>
        /// Identifies the <see cref="ThumbStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ThumbStyleProperty = DependencyProperty.Register("ThumbStyle", typeof(Style), typeof(XamSliderBase), new PropertyMetadata(new PropertyChangedCallback(ThumbStyleChanged)));

        /// <summary>
        /// Gets or sets the Style property for the <see cref="XamSliderBase"/>.
        /// </summary>
        /// <value>The thumb style.</value>
        public Style ThumbStyle
        {
            get { return (Style)this.GetValue(ThumbStyleProperty); }
            set { this.SetValue(ThumbStyleProperty, value); }
        }

        private static void ThumbStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase slider = obj as XamSliderBase;
            if (slider != null && e.NewValue != e.OldValue)
            {
                Style style = e.NewValue as Style;
                slider.OnThumbStyleChanged(style);
            }
        }

        #endregion ThumbStyle

        #region TrackFillBrush

        /// <summary>
        /// Identifies the <see cref="TrackFillBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TrackFillBrushProperty = DependencyProperty.Register("TrackFillBrush", typeof(Brush), typeof(XamSliderBase), new PropertyMetadata(new PropertyChangedCallback(TrackFillBrushChanged)));

        /// <summary>
        /// Gets or sets the Brush of the all thumb's trackfills if
        /// there is not explict set another style to thumb's trackfill.
        /// </summary>
        /// <value>The track fill style.</value>
        public Brush TrackFillBrush
        {
            get { return (Brush)this.GetValue(TrackFillBrushProperty); }
            set { this.SetValue(TrackFillBrushProperty, value); }
        }

        private static void TrackFillBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase slider = obj as XamSliderBase;
            if (slider != null && e.NewValue != e.OldValue)
            {
                Brush brush = e.NewValue as Brush;
                slider.OnTrackFillBrushChanged(brush);
            }
        }

        #endregion TrackFillBrush

        #region TrackFillStyle

        /// <summary>
        /// Identifies the <see cref="TrackFillStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TrackFillStyleProperty = DependencyProperty.Register("TrackFillStyle", typeof(Style), typeof(XamSliderBase), new PropertyMetadata(new PropertyChangedCallback(TrackFillStyleChanged)));

        /// <summary>
        /// Gets or sets the Style of the all thumbs if
        /// there is not explict set another style to thumb.
        /// </summary>
        /// <value>The track fill style.</value>
        public Style TrackFillStyle
        {
            get { return (Style)this.GetValue(TrackFillStyleProperty); }
            set { this.SetValue(TrackFillStyleProperty, value); }
        }

        private static void TrackFillStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase slider = obj as XamSliderBase;
            if (slider != null && e.NewValue != e.OldValue)
            {
                Style style = e.NewValue as Style;
                slider.OnTrackFillStyleChanged(style);
            }
        }

        #endregion TrackFillStyle

        #region TrackClickAction

        /// <summary>
        /// Identifies the <see cref="TrackClickAction"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TrackClickActionProperty = DependencyProperty.Register("TrackClickAction", typeof(SliderTrackClickAction), typeof(XamSliderBase), null);

        /// <summary>
        /// Gets or sets the action when click over slider track.
        /// It is possible to have no action, to change the ActiveThumb
        /// value with value of the LargeChange property or move the
        /// ActiveThumb to the position of the mouse cursor
        /// </summary>
        /// <value>The <see cref="SliderTrackClickAction"/>
        /// track click action.</value>
        public SliderTrackClickAction TrackClickAction
        {
            get { return (SliderTrackClickAction)this.GetValue(TrackClickActionProperty); }
            set { this.SetValue(TrackClickActionProperty, value); }
        }

        #endregion TrackClickAction

        #region VerticalTickMarksTemplate

        /// <summary>
        /// Identifies the <see cref="VerticalTickMarksTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalTickMarksTemplateProperty = DependencyProperty.Register("VerticalTickMarksTemplate", typeof(DataTemplate), typeof(XamSliderBase), new PropertyMetadata(new PropertyChangedCallback(VerticalTickMarksTemplateChanged)));

        /// <summary>
        /// Gets or sets the the DataTemplate for vertical <see cref="SliderTickMarks&lt;T&gt;"/>.
        /// </summary>
        /// <value>The vertical tick marks template.</value>
        public DataTemplate VerticalTickMarksTemplate
        {
            get { return (DataTemplate)this.GetValue(VerticalTickMarksTemplateProperty); }
            set { this.SetValue(VerticalTickMarksTemplateProperty, value); }
        }

        private static void VerticalTickMarksTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSliderBase owner = obj as XamSliderBase;
            if (owner != null)
            {
                DataTemplate newValue = e.NewValue as DataTemplate;
                owner.OnVerticalTickMarksTemplateChanged(newValue);
            }
        }

        #endregion VerticalTickMarksTemplate



#region Infragistics Source Cleanup (Region)




































































#endregion // Infragistics Source Cleanup (Region)


        #endregion Public

        #region Internal

        #region HorizontalTrack

        internal FrameworkElement HorizontalTrack
        {
            get;
            set;
        }

        #endregion HorizontalTrack

        #region VerticalTrack

        internal FrameworkElement VerticalTrack
        {
            get;
            set;
        }

        #endregion VerticalTrack

        #endregion Internal

        #region Protected

        #region HorizontalThumbsPanel

        /// <summary>
        /// Gets or sets the horizontal panel for the slider thumbs.
        /// </summary>
        /// <value>The horizontal thumbs panel.</value>
        protected Canvas HorizontalThumbsPanel
        {
            get;
            set;
        }

        #endregion HorizontalThumbsPanel

        #region HorizontalTrackFillsPanel

        /// <summary>
        /// Gets or sets the horizontal panel for the slider trackfills.
        /// </summary>
        /// <value>The horizontal trackfills panel.</value>
        protected Canvas HorizontalTrackFillsPanel
        {
            get;
            set;
        }

        #endregion HorizontalTrackFillsPanel

        #region HorizontalTickMarksContainer

        /// <summary>
        /// Gets or sets the horizontal container for  <see cref="TickMarksPanel&lt;T&gt;"/> .
        /// </summary>
        /// <value>The horizontal tick marks container.</value>
        protected Grid HorizontalTickMarksContainer
        {
            get;
            set;
        }

        #endregion HorizontalTickMarksContainer

        #region VerticalThumbsPanel

        /// <summary>
        /// Gets or sets the vertical panel for the slider thumbs.
        /// </summary>
        /// <value>The vertical thumbs panel.</value>
        protected Canvas VerticalThumbsPanel
        {
            get;
            set;
        }

        #endregion VerticalThumbsPanel

        #region VerticalTrackFillsPanel

        /// <summary>
        /// Gets or sets the vertical panel for the slider trackfills.
        /// </summary>
        /// <value>The vertical trackfills panel.</value>
        protected Canvas VerticalTrackFillsPanel
        {
            get;
            set;
        }

        #endregion VerticalTrackFillsPanel

        #region VerticalTickMarkslContainer

        /// <summary>
        /// Gets or sets the vertical container for  <see cref="TickMarksPanel&lt;T&gt;"/> .
        /// </summary>
        /// <value>The vertical tick marksl container.</value>
        protected Grid VerticalTickMarksContainer
        {
            get;
            set;
        }

        #endregion VerticalTickMarkslContainer

        #endregion Protected

        #endregion Properties

        #region Overrides

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.EnsureCurrentState();

            this.HorizontalThumbsPanel = this.GetTemplateChild("HorizontalThumbs") as Canvas;
            this.VerticalThumbsPanel = this.GetTemplateChild("VerticalThumbs") as Canvas;

            this.HorizontalTrackFillsPanel = this.GetTemplateChild("HorizontalTrackFills") as Canvas;
            this.VerticalTrackFillsPanel = this.GetTemplateChild("VerticalTrackFills") as Canvas;

            this.HorizontalTickMarksContainer = this.GetTemplateChild("HorizontalTickMarks") as Grid;

            this.VerticalTickMarksContainer = this.GetTemplateChild("VerticalTickMarks") as Grid;

            this.HorizontalTrack = this.GetTemplateChild("HorizontalTrack") as FrameworkElement;

            if (this.HorizontalTrack != null)
            {
                this.HorizontalTrack.SizeChanged -= this.HorizontalTrack_SizeChanged;
                this.HorizontalTrack.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.Track_MouseLeftButtonUp));

                this.HorizontalTrack.SizeChanged += this.HorizontalTrack_SizeChanged;
                this.HorizontalTrack.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.Track_MouseLeftButtonUp), true);
            }

            this.VerticalTrack = this.GetTemplateChild("VerticalTrack") as FrameworkElement;

            if (this.VerticalTrack != null)
            {
                this.VerticalTrack.SizeChanged -= this.VerticalTrack_SizeChanged;
                this.VerticalTrack.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.Track_MouseLeftButtonUp));

                this.VerticalTrack.SizeChanged += this.VerticalTrack_SizeChanged;
                this.VerticalTrack.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.Track_MouseLeftButtonUp), true);
            }


            this.EnsureDecreaseButtonVisibility();
            this.EnsureIncreaseButtonVisibility();

            this.EnsureOrientation();
            //Mihail Mateev: 2010.04.07 Removed ArrangeThumbs() Fix for Bug 30277




        }

        #endregion Overrides

        #region Methods

        #region Protected

        #region ArrangeThumbs

        /// <summary>
        /// Arranges the thumbs inside the slider layout.
        /// </summary>
        protected internal virtual void ArrangeThumbs()
        {
        }

        #endregion ArrangeThumbs

        #region EnsureCurrentState

        /// <summary>
        /// Ensures the state of the current <see cref="XamSliderBase"/> instance.
        /// </summary>
        protected virtual void EnsureCurrentState()
        {
            if (this.Orientation == Orientation.Horizontal)
            {
                VisualStateManager.GoToState(this, "Horizontal", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Vertical", false);
            }
            if (IsEnabled)
                VisualStateManager.GoToState(this, "Enabled", false);
            else
                VisualStateManager.GoToState(this, "Disabled", false);
        }

        #endregion EnsureCurrentState



        #region EnsureDecreaseButtonVisibility

        /// <summary>
        /// Ensures the visibility of  decrease small change button,
        /// depending on <see cref="DecreaseButtonVisibility"/> property
        /// </summary>
        protected virtual void EnsureDecreaseButtonVisibility()
        {
            if (this.DecreaseButtonVisibility == Visibility.Visible)
            {
                VisualStateManager.GoToState(this, "DecreaseButtonVisible", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "DecreaseButtonCollapsed", false);
            }
        }

        #endregion EnsureDecreaseButtonVisibility

        #region EnsureIncreaseButtonVisibility

        /// <summary>
        /// Ensures the visibility of  increase small change button,
        /// depending on <see cref="IncreaseButtonVisibility"/> property
        /// </summary>
        protected virtual void EnsureIncreaseButtonVisibility()
        {
            if (this.IncreaseButtonVisibility == Visibility.Visible)
            {
                VisualStateManager.GoToState(this, "IncreaseButtonVisible", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "IncreaseButtonCollapsed", true);
            }
        }

        #endregion EnsureIncreaseButtonVisibility



        #region EnsureOrientation

        /// <summary>
        /// Ensures the orientation of the slider.
        /// </summary>
        protected virtual void EnsureOrientation()
        {
        }

        #endregion EnsureOrientation

        #region EnsureSliderElements

        /// <summary>
        /// Ensures the slider elements -
        /// Thumbs and ThickMarks position.
        /// </summary>
        protected virtual void EnsureSliderElements()
        {
        }

        #endregion EnsureSliderElements

        #region EnsureTickMarks

        /// <summary>
        /// Ensures the tick marks generation.
        /// </summary>
        protected internal virtual void EnsureTickMarks()
        {
        }

        #endregion EnsureTickMarks



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


        #region OnHorizontalTickMarksTemplateChanged

        /// <summary>
        /// Called when HorizontalTickMarksTemplate value changed].
        /// </summary>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnHorizontalTickMarksTemplateChanged(DataTemplate newValue)
        {
        }

        #endregion OnHorizontalTickMarksTemplateChanged

        #region OnThumbStyleChanged

        /// <summary>
        /// Called when value of the ThumbStyle property is changed.
        /// </summary>
        /// <param name="style">The style.</param>
        protected virtual void OnThumbStyleChanged(Style style)
        {
        }

        #endregion OnThumbStyleChanged

        #region OnTrackFillBrushChanged

        /// <summary>
        /// Called when value of the TrackFillBrush property is changed.
        /// </summary>
        /// <param name="brush">The Brush.</param>
        protected virtual void OnTrackFillBrushChanged(Brush brush)
        {
        }

        #endregion OnTrackFillBrushChanged

        #region OnTrackFillStyleChanged

        /// <summary>
        /// Called when value of the TrackFillStyle property is changed.
        /// </summary>
        /// <param name="style">The style.</param>
        protected virtual void OnTrackFillStyleChanged(Style style)
        {
        }

        #endregion OnTrackFillStyleChanged

        #region OnTrackClick

        /// <summary>
        /// Raises the <see cref="XamSliderBase{T}.TrackClick"/> event.
        /// When click over the slider track.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        protected virtual void OnTrackClick(MouseButtonEventArgs e)
        {
        }

        #endregion OnTrackClick

        #region OnVerticalTickMarksTemplateChanged

        /// <summary>
        /// Called when VerticalTickMarksTemplate value is changed.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnVerticalTickMarksTemplateChanged(DataTemplate newValue)
        {
        }

        #endregion OnVerticalTickMarksTemplateChanged

        #region ProceedChange

        /// <summary>
        /// Change the value of the active thumb with specified Change value.
        /// </summary>
        /// <param name="isIncrease">if set to <c>true</c> value is increased.</param>
        /// <param name="isLargeChange">if set to <c>true</c> [is large change].</param>
        /// <param name="forceMoveOneTick">if set to <c>true</c> and Snap to Tick is True, the thumb will move to the next tick mark regardless of change size.</param>
        protected internal virtual void ProcessChanges(bool isIncrease, bool isLargeChange, bool forceMoveOneTick)
        {
        }

        #endregion ProceedChange

        #endregion Protected

        #region Private



#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)


        #endregion Private

        #endregion Methods

        #region EventHandlers

        private void VerticalTrack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ArrangeThumbs();
        }

        private void HorizontalTrack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ArrangeThumbs();
        }

        private void Track_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Grid track = sender as Grid;
            if (track != null && e.Handled == false)
            {
                this.OnTrackClick(e);
            }
        }

        /// <summary>
        /// If the IsEnabled State of our control changes, we need to update to the correct VisualState
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XamSliderBase_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            EnsureCurrentState();
        }



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


        #endregion EventHandlers

        #region ICommandTarget Members

        #region GetParameter

        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param name="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>
        /// The object necessary for the command to complete.
        /// </returns>
        protected virtual object GetParameter(CommandSource source)
        {
            if (source.Command is XamSliderBaseCommandBase)
            {
                return this;
            }

            return null;
        }

        #endregion GetParameter

        #region SupportsCommand

        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param name="command">The command to be validated.</param>
        /// <returns>
        /// True if the object recognizes the command as actionable against it.
        /// </returns>
        protected virtual bool SupportsCommand(ICommand command)
        {
            return command is XamSliderBaseCommandBase;
        }

        #endregion SupportsCommand



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return this.SupportsCommand(command);
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
        }

        #endregion ICommandTarget Members

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