using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.DataPresenter;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Media;
using Infragistics.Windows.Internal;
using System.Windows.Data;
using Infragistics.Windows.Resizing;
using Infragistics.Shared;
using System.Globalization;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using System.Windows.Input;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Content control derived class used by the <see cref="XamDataCards"/> (and <see cref="XamDataPresenter"/>'s <see cref="CardView"/>) to serve as a container (wrapper) for each item in the list.
	/// </summary>
	/// <remarks>
	/// <p class="body">A <see cref="CardViewPanel"/> is used within a <see cref="XamDataCards"/> (and <see cref="XamDataPresenter"/>'s <see cref="CardView"/>) to arrange items in the list. 
	/// The <see cref="CardViewPanel"/> wraps each of its child items in a CardViewCard element.  The wrapper serves as a convenient
	/// place to store state (required by the <see cref="CardViewPanel"/>) for each of its child items.  You will not normally need to interact with this element but you should
	/// be aware of its existence in case you have code that needs to traverse the control's visual tree.</p>
	/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamDataCards"/> (or <see cref="XamDataPresenter"/>'s <see cref="CardView"/> 
	/// when needed.  You do not ordinarily need to create an instance of this class directly.</p>
	/// </remarks>
	/// <seealso cref="XamDataCards"/>
	/// <seealso cref="XamDataPresenter"/>
	/// <seealso cref="XamDataPresenter.View"/>
	/// <seealso cref="CardView"/>
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]

    // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,           GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,        GroupName = VisualStateUtilities.GroupCommon)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateActive,           GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,         GroupName = VisualStateUtilities.GroupActive)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateChanged,          GroupName = VisualStateUtilities.GroupChange)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnchanged,        GroupName = VisualStateUtilities.GroupChange)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateExpanded,         GroupName = VisualStateUtilities.GroupExpansion)]
    [TemplateVisualState(Name = VisualStateUtilities.StateCollapsed,        GroupName = VisualStateUtilities.GroupExpansion)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateDataRecord,       GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFilterRecord,     GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAddRecord,        GroupName = VisualStateUtilities.GroupRecord)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateSelected,         GroupName = VisualStateUtilities.GroupSelection)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnselected,       GroupName = VisualStateUtilities.GroupSelection)]

	[DesignTimeVisible(false)]
	public sealed class CardViewCard : ContentControl,
									   IRecordPresenterContainer,
									   IResizableElement
	{
		#region Member Variables

		private TransformGroup						_explicitTransformGroupOriginal;
		private TransformGroup						_explicitTransformGroupClones;
		private TransformGroup						_internalTransformGroup;
		private TranslateTransform					_translateTransform;
		private PropertyValueTracker				_layoutVersionTracker;
		private PropertyValueTracker				_layoutSortVersionTracker;
		private PropertyValueTracker				_isCardCollapsedTracker;
		private PropertyValueTracker				_primaryFieldTracker;
		private PropertyValueTracker				_headerPathTracker;
		private PropertyValueTracker				_recordDataTracker;
		private PropertyValueTracker				_overallSortVersionTracker;

		private static CultureInfo					s_culture;

		private CardView							_view;
		private CardViewPanel						_panel;

		// JM 03-29-10 TFS29659 - Cached reference to our contained scrollviewer (set in Initialized).
		private ScrollViewer						_scrollViewer;

        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        // Keep track of the cardHeaderPresenter; so we can let it know to Update its visual state 
        private WeakReference _cardHeaderPresenter;


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		// JM 01-18-11 TFS32131
		private int									_lastCollapseTickCount;

		#endregion //Member Variables

		#region Constructor

		static CardViewCard()
		{
			// JM 06-10-10 TFS34292 - Should be looking at (and AddOwnering) the s_HeaderStringFormatProperty on
			// CardView - NOT CardViewCard.
			//if (CardViewCard.s_HeaderStringFormatProperty != null)
			//    s_HeaderStringFormatProperty = CardViewCard.s_HeaderStringFormatProperty.AddOwner(typeof(CardViewCard));
			if (CardView.s_HeaderStringFormatProperty != null)
				s_HeaderStringFormatProperty = CardView.s_HeaderStringFormatProperty.AddOwner(typeof(CardViewCard));
			else
				s_HeaderStringFormatProperty = DependencyProperty.Register("HeaderStringFormat",
					typeof(string), typeof(CardViewCard), new FrameworkPropertyMetadata(null));

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
				typeof(CardViewCard), new FrameworkPropertyMetadata(typeof(CardViewCard)));

			FrameworkElement.RenderTransformProperty.OverrideMetadata(
				typeof(CardViewCard), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceRenderTransform)));

			// JM 03-02-10 TFS28793
			EventManager.RegisterClassHandler(typeof(CardViewCard), FrameworkElement.MouseWheelEvent, new RoutedEventHandler(OnMouseWheelEvent), true);

			// JM 01-18-11 TFS32131.
			EventManager.RegisterClassHandler(typeof(CardViewCard), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView), true);
		}

		/// <summary>
		/// Constructor provided to allow creation in design tools for template and style editing.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamDataCards"/> (or <see cref="XamDataPresenter"/>'s <see cref="CardView"/> when needed.  
		/// You do not ordinarily need to create an instance of this class directly.</p>
		/// </remarks>
		/// <seealso cref="XamDataCards"/>
		/// <seealso cref="XamDataPresenter"/>
		/// <seealso cref="XamDataPresenter.View"/>
		/// <seealso cref="CardView"/>
		public CardViewCard()
		{
		}

		#endregion //Constructor

		#region IRecordPresenterContainer Members

		RecordPresenter IRecordPresenterContainer.RecordPresenter
		{
			get { return this.Content as RecordPresenter; }
		}

		#endregion //IRecordPresenterContainer Members

		#region Base Class Overrides 

			// AS 2/25/10 TFS28544
			#region HandlesScrolling

		/// <summary>
		/// Returns a value that indicates whether the control handles scrolling.
		/// </summary>
		/// <returns>True to indicate that scrolling will be handled by the containing DataPresenterBase control.</returns>
		protected override bool HandlesScrolling
		{
			get { return true; }
		}

			#endregion //HandlesScrolling

			#region HitTestCore

		/// <summary>
		/// Override to make sure the card gets mouse messages regardless of whether its background is transparent or not.
		/// </summary>
		/// <param name="hitTestParameters"></param>
		/// <returns></returns>
		/// <remarks>
		/// <p class="body">
		/// This method is overridden on this class to make sure the card gets mouse messages
		/// regardless of whether its background is transparent or not.
		/// </p>
		/// </remarks>
		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			Rect rect = new Rect(new Point(), this.RenderSize);
			if (rect.Contains(hitTestParameters.HitPoint))
				return new PointHitTestResult(this, hitTestParameters.HitPoint);

			return base.HitTestCore(hitTestParameters);
		}

			#endregion // HitTestCore

            #region LogicalChildren
        /// <summary>
        /// Gets an enumerator that can iterate the logical child elements of this element.
        /// </summary>
        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
				// AS 2/22/10 TFS27019
				// The CarouselView sets the RecordPresenter as the content. When that 
				// happens, the base ContentControl OnContentChanged implementation makes 
				// that a logical child of this element. However, when the ContentControl's 
				// PrepareContentControl is called by the PrepareContainerForItemOverride 
				// it sets its ContentIsNotLogical which causes its LogicalChildren to 
				// not return the content because it assumes that the DataContext and 
				// Content are the same which in the case of the CarouselItem it is not.
				//
				DependencyObject content = this.Content as DependencyObject;
				bool isContentLogical = content != null && LogicalTreeHelper.GetParent(content) == this;

				if (this.HasHeader)
				{
					if (isContentLogical)
					{
						return new MultiSourceEnumerator(
							new SingleItemEnumerator(content),
							new SingleItemEnumerator(this.Header));
					}

					return new SingleItemEnumerator(this.Header);
				}

				if (isContentLogical)
					return new SingleItemEnumerator(content);

				return EmptyEnumerator.Instance;
            }
        } 
            #endregion //LogicalChildren

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			Size currentMeasureSize = base.MeasureOverride(constraint);

			// If the card is collapsed and we have cached a width from the last uncollapsed measure,
			// construct and return a desired size that uses the cached width so that the card retains its uncollapsed 
			// width even if the header area is narrower than the collapsed content.
			if (constraint.Width != 0 && constraint.Height != 0)
			{
				if (this.IsCollapsed)
				{
					if (this._panel != null)
					{
						if (double.IsNaN(this._panel.UncollapsedCardWidth) == false)
						{
							if (this.View != null)
							{
								if (double.IsNaN(this.View.ViewSettings.CardWidth))
									return new Size(this._panel.UncollapsedCardWidth, currentMeasureSize.Height);
							}
						}
					}
				}
				else
				{
					if (this._panel != null)
					{
						this._panel.UncollapsedCardWidth = currentMeasureSize.Width;

						if (double.IsNaN(this.View.ViewSettings.CardWidth))
							return new Size(this._panel.UncollapsedCardWidth, currentMeasureSize.Height);
					}
				}
			}

			return currentMeasureSize;
		}

			#endregion //MeasureOverride

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
        /// it to get the focus site from the control template whenever template gets applied to the control.
        /// </p>
        /// </remarks>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

            #endregion //OnApplyTemplate	

			#region OnCreateAutomationPeer

		/// <summary>
		/// Returns an automation peer that exposes the <see cref="CardViewCard"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.CardViewCardAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new CardViewCardAutomationPeer(this);
		}

			#endregion //OnCreateAutomationPeer
    
            #region OnMouseEnter
        /// <summary>
        /// Invoked when the mouse is moved within the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
            #endregion //OnMouseEnter

            #region OnMouseLeave
        /// <summary>
        /// Invoked when the mouse is moved outside the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
            #endregion //OnMouseLeave

			#region OnPropertyChanged

		/// <summary>
		/// Called when the value of a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == CardViewCard.RenderTransformProperty && e.NewValue == null)
			{
				this._explicitTransformGroupClones		= null;
				this._explicitTransformGroupOriginal	= null;
				this.CoerceValue(CardViewCard.RenderTransformProperty);
			}
			else
			// JM 01-18-11 TFS32131 - Added.
			if (e.Property == CardViewCard.IsCollapsedProperty)
			{
				if ((bool)e.NewValue == true)
					this._lastCollapseTickCount = Environment.TickCount;
			}
		}

			#endregion //OnPropertyChanged

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region CollapseCardButtonVisibility

		/// <summary>
		/// Identifies the <see cref="CollapseCardButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CollapseCardButtonVisibilityProperty = DependencyProperty.Register("CollapseCardButtonVisibility",
			typeof(Visibility), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

		/// <summary>
		/// Returns/sets the Visibility of a button in the Card header which can be clicked by the end user to collapse a Card so it shows its header only. 
		/// </summary>
		/// <seealso cref="CollapseCardButtonVisibilityProperty"/>
		/// <seealso cref="CardView"/>
		/// <seealso cref="CardViewSettings.CollapseCardButtonVisibility"/>
		/// <seealso cref="CardViewSettings.ShouldCollapseCards"/>
		//[Description("Returns/sets the Visibility of a button in the Card header which can be clicked by the end user to collapse a Card so it shows its header only. ")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Visibility CollapseCardButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CardViewCard.CollapseCardButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(CardViewCard.CollapseCardButtonVisibilityProperty, value);
			}
		}

			#endregion //CollapseCardButtonVisibility

				#region CollapseEmptyCellsButtonVisibility

		/// <summary>
		/// Identifies the <see cref="CollapseEmptyCellsButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CollapseEmptyCellsButtonVisibilityProperty = DependencyProperty.Register("CollapseEmptyCellsButtonVisibility",
			typeof(Visibility), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

		/// <summary>
		/// Returns/sets the Visibility of a button in the Card header which can be clicked by the end user to collapse cells that contain empty values (e.g. empty string/null for string, null for nullable types, and 0 for numeric types).
		/// </summary>
		/// <seealso cref="CollapseEmptyCellsButtonVisibilityProperty"/>
		/// <seealso cref="CardView"/>
		/// <seealso cref="CardViewSettings.CollapseEmptyCellsButtonVisibility"/>
		/// <seealso cref="CardViewSettings.ShouldCollapseEmptyCells"/>
		//[Description("Returns/sets the Visibility of a button in the Card header which can be clicked by the end user to collapse cells that contain empty values (e.g. empty string/null for string, null for nullable types, and 0 for numeric types).")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Visibility CollapseEmptyCellsButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CardViewCard.CollapseEmptyCellsButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(CardViewCard.CollapseEmptyCellsButtonVisibilityProperty, value);
			}
		}

			#endregion //CollapseEmptyCellsButtonVisibility

                #region HasHeader

        private static readonly DependencyPropertyKey HasHeaderPropertyKey =
			DependencyProperty.RegisterReadOnly("HasHeader",
			typeof(bool), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasHeader"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasHeaderProperty =
			HasHeaderPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the <see cref="Header"/> property has been set.
		/// </summary>
		/// <seealso cref="HasHeaderProperty"/>
		/// <seealso cref="Header"/>
		//[Description("Returns a boolean indicating if the Header property has been set.")]
		//[Category("Appearance")] 
		[Bindable(true)]
		[ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HasHeader
		{
			get
			{
				return (bool)this.GetValue(CardViewCard.HasHeaderProperty);
			}
		}

		        #endregion //HasHeader

                #region Header

        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property
        /// </summary>
		public static readonly DependencyProperty HeaderProperty = HeaderedContentControl.HeaderProperty.AddOwner(typeof(CardViewCard),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnHeaderChanged)));

        private static void OnHeaderChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
			CardViewCard card = target as CardViewCard;

            card.SetValue(HasHeaderPropertyKey, KnownBoxes.FromValue(e.NewValue != null));
            
            card.OnHeaderChanged(e.OldValue, e.NewValue);
        }

        /// <summary>
        /// Gets/sets an object that will appear in the header of this card.
        /// </summary>
        /// <value>A header object or the default value of null.</value>
        /// <seealso cref="HeaderProperty"/>
        //[Description("Gets/sets an object that will appear in the header of this card.")]
        //[Category("Content")]
        [Bindable(true)]
        [Localizability(LocalizationCategory.Label)]
        public object Header
        {
            get
            {
				return (object)this.GetValue(CardViewCard.HeaderProperty);
            }
            set
            {
				this.SetValue(CardViewCard.HeaderProperty, value);
            }
        }

                #endregion //Header

                #region HeaderStringFormat

        private static DependencyProperty s_HeaderStringFormatProperty;

        /// <summary>
        /// Identifies the <see cref="HeaderStringFormat"/> dependency property
        /// </summary>
        public static DependencyProperty HeaderStringFormatProperty { get { return s_HeaderStringFormatProperty; } }


        /// <summary>
        /// Gets/sets the format of the card header
        /// </summary>
        /// <seealso cref="HeaderStringFormatProperty"/>
        //[Description("Gets/sets the format of the card header")]
        //[Category("Control")]
        [Bindable(true)]
        public string HeaderStringFormat
        {
            get
            {
				return (string)this.GetValue(CardViewCard.HeaderStringFormatProperty);
            }
            set
            {
				this.SetValue(CardViewCard.HeaderStringFormatProperty, value);
            }
        }

                #endregion //HeaderStringFormat
	
                #region HeaderTemplate

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> dependency property
        /// </summary>
		public static readonly DependencyProperty HeaderTemplateProperty = HeaderedContentControl.HeaderTemplateProperty.AddOwner(typeof(CardViewCard));

        /// <summary>
        /// Gets/sets the template used to display the content of the card's header.
        /// </summary>
        /// <seealso cref="HeaderTemplateProperty"/>
        //[Description("Gets/sets the template used to display the content of the card's header.")]
        //[Category("Content")]
        [Bindable(true)]
        public DataTemplate HeaderTemplate
        {
            get
            {
				return (DataTemplate)this.GetValue(CardViewCard.HeaderTemplateProperty);
            }
            set
            {
				this.SetValue(CardViewCard.HeaderTemplateProperty, value);
            }
        }

                #endregion //HeaderTemplate

                #region HeaderTemplateSelector

        /// <summary>
        /// Identifies the <see cref="HeaderTemplateSelector"/> dependency property
        /// </summary>
		public static readonly DependencyProperty HeaderTemplateSelectorProperty = HeaderedContentControl.HeaderTemplateSelectorProperty.AddOwner(typeof(CardViewCard));

        /// <summary>
        /// Gets/sets a selector to allow the application writer to assign different header templates for each card.
        /// </summary>
        /// <value>A DataTemplateSelector object or the default value of null.</value>
        /// <seealso cref="HeaderTemplateSelectorProperty"/>
        //[Description("Gets/sets a selector to allow the application writer to assign different header templates for each card.")]
        //[Category("Content")]
        [Bindable(true)]
        public DataTemplateSelector HeaderTemplateSelector
        {
            get
            {
				return (DataTemplateSelector)this.GetValue(CardViewCard.HeaderTemplateSelectorProperty);
            }
            set
            {
                this.SetValue(CardViewCard.HeaderTemplateSelectorProperty, value);
            }
        }

                #endregion //HeaderTemplateSelector

				#region HeaderVisibility

		/// <summary>
		/// Identifies the <see cref="HeaderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderVisibilityProperty = DependencyProperty.Register("HeaderVisibility",
			typeof(Visibility), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

		/// <summary>
		/// Returns/sets the Visibiolity of the Card's Header.
		/// </summary>
		/// <seealso cref="HeaderVisibilityProperty"/>
		//[Description("Returns/sets the Visibiolity of the Card's Header.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Visibility HeaderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CardViewCard.HeaderVisibilityProperty);
			}
			set
			{
				this.SetValue(CardViewCard.HeaderVisibilityProperty, value);
			}
		}

			#endregion //HeaderVisibility

				#region IsAddRecord

		private static readonly DependencyPropertyKey IsAddRecordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsAddRecord",
			typeof(bool), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsAddRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAddRecordProperty =
			IsAddRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the associated <see cref="Record"/> is an Add record. (read only)
		/// </summary>
		/// <seealso cref="IsAddRecordProperty"/>
		//[Description("Returns true if the associated Record is an Add record. (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsAddRecord
		{
			get
			{
				return (bool)this.GetValue(CardViewCard.IsAddRecordProperty);
			}
		}

				#endregion //IsAddRecord

				#region IsActive

		/// <summary>
		/// Identifies the <see cref="IsActive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
			typeof(bool), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Returns/sets whether the associated <see cref="Record"/> is active.
		/// </summary>
		/// <seealso cref="IsActiveProperty"/>
		//[Description("Returns/sets whether the associated record is active.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsActive
		{
			get
			{
				return (bool)this.GetValue(CardViewCard.IsActiveProperty);
			}
			set
			{
				this.SetValue(CardViewCard.IsActiveProperty, value);
			}
		}

				#endregion //IsActive

				#region IsCollapsed

		/// <summary>
		/// Identifies the <see cref="IsCollapsed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed",
			typeof(bool), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsParentMeasure	| 
																								   FrameworkPropertyMetadataOptions.AffectsMeasure, 

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            new PropertyChangedCallback(OnVisualStatePropertyChanged), new CoerceValueCallback(CoerceIsCollapsed)));





		private static object CoerceIsCollapsed(DependencyObject target, object value)
		{
			CardViewCard card = target as CardViewCard;

			if (card != null)
			{
				bool			isCollapsed = false;
				RecordPresenter rp			= card.Content as RecordPresenter;
				if (rp != null)
				{
					Record record = rp.Record;

					if (record != null)
						isCollapsed = record.IsContainingCardCollapsedResolved;
				}

				return isCollapsed;
			}

			return value;
		}

		/// <summary>
		/// Returns/sets whether the CardViewCard is collapsed to show only its header and not its content.
		/// </summary>
		/// <seealso cref="IsCollapsedProperty"/>
		/// <seealso cref="CardView"/>
		/// <seealso cref="CardViewSettings.CollapseCardButtonVisibility"/>
		/// <seealso cref="CardViewSettings.ShouldCollapseCards"/>
		//[Description("Returns/sets whether the CardViewCard is collapsed to show only its header and not its content.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public bool IsCollapsed
		{
			get
			{
				return (bool)this.GetValue(CardViewCard.IsCollapsedProperty);
			}
			set
			{
				this.SetValue(CardViewCard.IsCollapsedProperty, value);
			}
		}

			#endregion //IsCollapsed

				#region IsFilterRecord

		private static readonly DependencyPropertyKey IsFilterRecordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsFilterRecord",
			typeof(bool), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsFilterRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFilterRecordProperty =
			IsFilterRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the associated <see cref="Record"/> is a Filter record. (read only)
		/// </summary>
		/// <seealso cref="IsFilterRecordProperty"/>
		//[Description("Returns true if the associated Record is a Filter record. (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsFilterRecord
		{
			get
			{
				return (bool)this.GetValue(CardViewCard.IsFilterRecordProperty);
			}
		}

				#endregion //IsFilterRecord

				#region IsSelected

		/// <summary>
		/// Identifies the <see cref="IsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
			typeof(bool), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Returns/sets whether the associated <see cref="Record"/> is selected.
		/// </summary>
		/// <seealso cref="IsSelectedProperty"/>
		//[Description("Returns/sets whether the associated record is selected.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsSelected
		{
			get
			{
				return (bool)this.GetValue(CardViewCard.IsSelectedProperty);
			}
			set
			{
				this.SetValue(CardViewCard.IsSelectedProperty, value);
			}
		}

				#endregion //IsSelected

				#region ShouldCollapseEmptyCells

		/// <summary>
		/// Identifies the <see cref="ShouldCollapseEmptyCells"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldCollapseEmptyCellsProperty = DependencyProperty.Register("ShouldCollapseEmptyCells",
			typeof(bool), typeof(CardViewCard), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsParentMeasure |
																								   FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets whether <see cref="Cell"/>s contained in this <see cref="CardViewCard"/> should be collapsed if they contain empty values.
		/// </summary>
		/// <seealso cref="ShouldCollapseEmptyCellsProperty"/>
		//[Description("Returns/sets whether Cells contained in this CardViewCard should be collapsed if they contain empty values.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool ShouldCollapseEmptyCells
		{
			get
			{
				return (bool)this.GetValue(CardViewCard.ShouldCollapseEmptyCellsProperty);
			}
			set
			{
				this.SetValue(CardViewCard.ShouldCollapseEmptyCellsProperty, value);
			}
		}

			#endregion //ShouldCollapseEmptyCells

			#endregion //Public Properties

			#region Internal Properties

                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                #region HeaderPresenter

        // Keep track of the paneHeader so we can let it know to Update its visuall state 
        internal CardHeaderPresenter HeaderPresenter
        {
            get
            {
                if (this._cardHeaderPresenter == null)
                    return null;

                return Utilities.GetWeakReferenceTargetSafe(this._cardHeaderPresenter) as CardHeaderPresenter;
            }
        }

                #endregion //HeaderPresenter	

				#region Panel

		internal CardViewPanel Panel
		{
			get { return this._panel; }
			set { this._panel = value; }
		}

				#endregion //Panel

				#region View

		internal CardView View
		{
			get { return this._view; }
			set { this._view = value; }
		}

				#endregion //View

			#endregion //Internal Properties

			#region Private Properties

				// JM 03-29-10 TFS29659 - Added.
				#region ScrollViewer

		private ScrollViewer ScrollViewer
		{
			get
			{
				if (this._scrollViewer != null)
				{
					if (VisualTreeHelper.GetParent(this._scrollViewer)	== null  ||
						this._scrollViewer.IsLoaded						== false)
						this._scrollViewer = null;
				}

				if (this._scrollViewer == null)
					this._scrollViewer = Utilities.GetDescendantFromType(this, typeof(ScrollViewer), true) as ScrollViewer;

				return this._scrollViewer;
			}
		}

				#endregion ScrollViewer

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region BindPathProperty

		private bool BindPathProperty(object item, CardViewCard card, string sourcePropertyName, DependencyProperty dpTarget, string stringFormat)
        {
            if (sourcePropertyName != null && sourcePropertyName.Length > 0)
            {
                Binding binding = new Binding();
				binding.Source	= item;
				binding.Mode	= BindingMode.OneWay;

                if (GridUtilities.IsXmlNodeOptimized(item))
                {
                    binding.XPath = sourcePropertyName;
                }
                else
                {
                    binding.Path = new PropertyPath(sourcePropertyName, new object[0]);
                }

                if (stringFormat != null && CardView.s_BindingStringFormatInfo != null)
					CardView.s_BindingStringFormatInfo.SetValue(binding, stringFormat, null);

                card.SetBinding(dpTarget, binding);

                return true;
            }

            return false;
        }

                #endregion //BindPathProperty	

                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                #region ClearHeader

        // Keep track of the paneHeader so we can let it know to Update its visual state
        internal void ClearHeader(CardHeaderPresenter header)
        {
            if ( this.Header == header )
                this._cardHeaderPresenter = null;
        }

                #endregion //ClearHeader

				#region GetCulture

		private CultureInfo GetCulture(DataPresenterBase dp)
		{
			if (CardViewCard.s_culture == null)
			{
				CardViewCard.s_culture = (dp != null) ? dp.DefaultConverterCulture : null;

				if (CardViewCard.s_culture == null)
				{
					CardViewCard.s_culture = System.Threading.Thread.CurrentThread.CurrentCulture;

					if (CardViewCard.s_culture == null)
						CardViewCard.s_culture = CultureInfo.InvariantCulture;
				}
			}

			return CardViewCard.s_culture;
		}

				#endregion //GetCulture

                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                #region InitializeHeader

        // Keep track of the paneHeader so we can let it know to Update its visual state
        internal void InitializeHeader(CardHeaderPresenter header)
        {
            this._cardHeaderPresenter = new WeakReference(header);
        }

                #endregion //InitializeHeader

				// JM 03-02-10 TFS28793 - Added.
				#region OnMouseWheelEvent
		// ScrollViewers automatically mark the MouseWheel event handled even if they don't actually perform a scroll.
		// This creates a problem for us since the XamDataCards control won't scroll the Cards if the mousepointer
		// is over a Card when the mousewheel is turned, because the default Template for our Cards contains a ScrollViewer.
		// That's OK if the ScrollViewer in the Card actually scrolled the card's contents, but is not OK if the ScrollViewer
		// did not perform a scroll.
		//
		// To allow the MouseWheel messages to bubble up to the XamDataCards control in this scenario, this routine will 
		// look at all handled MouseWheel messages and walk up the tree from the original sender of the message looking 
		// for ScrollViewers that are active (i.e., it's ScrollBars are visible). If it does not find one, it will set
		// handled = false to allow the message to continue to bubble up.
		private static void OnMouseWheelEvent(object sender, RoutedEventArgs e)
		{
			// We are only interested in handled events.
			if (e.Handled == false)
				return;

			DependencyObject originalSource = e.OriginalSource as DependencyObject;

			// Make sure the originalSource is not a ContentElement
			while (originalSource is ContentElement)
				originalSource = LogicalTreeHelper.GetParent(originalSource);

			if (originalSource == null)
				return;

			// The sender should be the Card.
			CardViewCard card = sender as CardViewCard;
			if (card == null)
				return;

			DependencyObject	parent					= VisualTreeHelper.GetParent(originalSource);
			bool				activeScrollViewerFound = false;
			ScrollViewer		scrollViewer			= null;
			bool				cardFound				= false;	// JM 07-22-11 TFS82776

			// JM 07-22-11 TFS82776 - Also check for parent == null
			while (parent != null && parent != card)
			{
				scrollViewer = parent as ScrollViewer;
				if (scrollViewer != null &&
					(scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible ||
					 scrollViewer.ComputedVerticalScrollBarVisibility	== Visibility.Visible))
				{
					activeScrollViewerFound = true;
					break;
				}

				cardFound = parent == card;	// JM 07-22-11 TFS82776

				parent = VisualTreeHelper.GetParent(parent);
			}

			// JM 07-22-11 TFS82776 - If we walked all the way up to the point where parent == null and we did not find
			// the card, then the mouse is over a popup (e.g., a cell's combo editor popup) and we shouldn't let the message bubble up.
			if (parent == null && false == cardFound)
				e.Handled = true;
			else
			// If we did not find an active ScrollViewer, set Handled to False so the message continues to bubble.
			if (activeScrollViewerFound == false)
				e.Handled = false;
		}
				#endregion //OnMouseWheelEvent

				// JM 01-18-11 TFS32131 - Added.
				#region OnRequestBringIntoView
		static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			CardViewCard card = e.TargetObject as CardViewCard;

			if (card != null && card.IsCollapsed)
			{
				int ticksSinceLastCollapse = Environment.TickCount - card._lastCollapseTickCount;

				if (ticksSinceLastCollapse < 200)
					e.Handled = true;
			}
		}
				#endregion //OnRequestBringIntoView

				#region ResolveIsCollapsed

		private void ResolveIsCollapsed()
		{
			this.InvalidateProperty(CardViewCard.IsCollapsedProperty);
		}

				#endregion //ResolveIsCollapsed

			#endregion PrivateMethods

			#region Internal Methods 

				// JM 02--5-10 TFS27237 - Created this method from code moved from OnPropertyChanged (content property changed)
				#region Initialize

		internal void Initialize(RecordPresenter recordPresenter)
		{
			// JM 03-29-10 TFS29659 - Reset the scroll offsets (if necessary) of our contained scrollviewer.
			if (this.ScrollViewer != null)
			{
				if (this.ScrollViewer.VerticalOffset > 0)
					this.ScrollViewer.ScrollToTop();
				
				if (this.ScrollViewer.HorizontalOffset > 0)
					this.ScrollViewer.ScrollToLeftEnd();
			}

			if (this._panel != null)
			{
				if (recordPresenter != null && recordPresenter.FieldLayout != null)
					this._panel.SetupSpecialRecordsVersionTracker(recordPresenter.FieldLayout);
			}

			if (recordPresenter != null)
			{
				if (recordPresenter.FieldLayout != null)
				{
					this._layoutVersionTracker			= new PropertyValueTracker(recordPresenter.FieldLayout, "SpecialRecordsVersion", this.ResolveIsCollapsed);
					this._layoutSortVersionTracker		= new PropertyValueTracker(recordPresenter.FieldLayout, "SortOperationVersion", this.ResolveIsCollapsed);
					this._primaryFieldTracker			= new PropertyValueTracker(recordPresenter.FieldLayout, "PrimaryField", this.ResolveHeader);

					if (recordPresenter.FieldLayout.DataPresenter != null)
					{
						if (recordPresenter.FieldLayout.DataPresenter.CurrentViewInternal is CardView)
							this._headerPathTracker		= new PropertyValueTracker(recordPresenter.FieldLayout.DataPresenter.CurrentViewInternal, "CachedHeaderPath", this.ResolveHeader);

						this._overallSortVersionTracker = new PropertyValueTracker(recordPresenter.FieldLayout.DataPresenter, "OverallSortVersion", this.ResolveHeader, true);
					}
				}

				if (recordPresenter.Record != null)
				{
					this._isCardCollapsedTracker	= new PropertyValueTracker(recordPresenter.Record, "IsContainingCardCollapsedResolved", this.ResolveIsCollapsed);
					this._recordDataTracker			= new PropertyValueTracker(recordPresenter.Record, "IsDataChanged", this.ResolveHeader);

					this.ResolveAddNewAndFilterRecordStatus(recordPresenter.Record);
				}
			}

			this.ResolveIsCollapsed();
		}

				#endregion //Initialize

				#region ResolveAddNewAndFilterRecordStatus

		internal void ResolveAddNewAndFilterRecordStatus(Record record)
		{
			if (record is DataRecord && ((DataRecord)record).IsAddRecord)
				this.SetValue(CardViewCard.IsAddRecordPropertyKey, KnownBoxes.TrueBox);
			else
				this.SetValue(CardViewCard.IsAddRecordPropertyKey, KnownBoxes.FalseBox);

			if (record.RecordType == RecordType.FilterRecord)
				this.SetValue(CardViewCard.IsFilterRecordPropertyKey, KnownBoxes.TrueBox);
			else
				this.SetValue(CardViewCard.IsFilterRecordPropertyKey, KnownBoxes.FalseBox);
		}

				#endregion //ResolveAddNewAndFilterRecordStatus

				#region ResolveHeader

		internal void ResolveHeader()
		{
            try
            {
                RecordPresenter rp = this.Content as RecordPresenter;
                Record record = rp != null ? rp.Record : null;
                if (rp != null &&
                    record != null &&
                    (record.IsDataRecord || ((DataRecord)record).IsAddRecord || record.RecordType == RecordType.FilterRecord))
                {
                    if (((DataRecord)rp.Record).IsAddRecord)
                    {
                        this.SetValue(CardViewCard.HeaderProperty, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Card_AddNewRecordTitle", new object[] { }, this.GetCulture(rp.DataPresenter)));
                        return;
                    }

                    if (rp.Record.RecordType == RecordType.FilterRecord)
                    {
                        this.SetValue(CardViewCard.HeaderProperty, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Card_FilterRecordTitle", new object[] { }, this.GetCulture(rp.DataPresenter)));
                        return;
                    }

				// JM 06-10-10 TFS34292 - Should be looking at the s_HeaderStringFormatProperty on CardViewCard NOT CardView
				//string stringFormat = (string)this.GetValue(CardView.s_HeaderStringFormatProperty);
				string		stringFormat= (string)this.GetValue(CardViewCard.s_HeaderStringFormatProperty);

                    bool headerSet = false;
                    CardView cardView = this.View;
                    if (cardView != null && false == string.IsNullOrEmpty(this.View.CachedHeaderPath))
                        headerSet = this.BindPathProperty(((DataRecord)rp.Record).DataItem, this, cardView.CachedHeaderPath, CardViewCard.HeaderProperty, stringFormat);


                    // JM 02-04-10 TFS26812 - Check for PrimaryField = null.  Also, if the binding cannot be set, clear any previous binding.
                    if (false == headerSet)
                    {
                        if (rp.FieldLayout != null && rp.FieldLayout.PrimaryField != null)
                            this.BindPathProperty(((DataRecord)rp.Record).DataItem, this, rp.FieldLayout.PrimaryField.Name, CardViewCard.HeaderProperty, stringFormat);
                        else
                        {
                            BindingOperations.ClearBinding(this, CardViewCard.HeaderProperty);
                            this.ClearValue(CardViewCard.HeaderProperty);
                        }
                    }

                    this.ResolveAddNewAndFilterRecordStatus(rp.Record);
                }
                else
                {
                    BindingOperations.ClearBinding(this, CardViewCard.HeaderProperty);
                    this.ClearValue(CardViewCard.HeaderProperty);
                }
            }
            finally
            {

                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                this.UpdateVisualStates();

            }


		}

				#endregion //ResolveHeader

				#region UpdateTransform

		internal void UpdateTransform(Rect original, Rect current, Rect target, Vector offset, bool calledFromArrange)
        {
			// JM 05-19-10 TFS32481 
			//if (this._translateTransform == null)
			if (this._translateTransform == null || this._translateTransform.IsSealed)
			{
				this._translateTransform = new TranslateTransform();
				this.CoerceValue(CardViewCard.RenderTransformProperty);
			}

            Rect rect					= Rect.Offset(current, offset);
            this._translateTransform.X	= rect.X;
            this._translateTransform.Y	= rect.Y;
        }

                #endregion //UpdateTransform

			#endregion //Internal Methods

			#region Private Methods

				#region AreTransformGroupsEqual

		private static bool AreTransformGroupsEqual(TransformCollection group1Children, TransformCollection group2Children)
		{
			if (group1Children.Count == group2Children.Count)
			{
				for (int i = 0; i < group1Children.Count; i++)
				{
					if (false == group1Children[i].Equals(group2Children[i]))
						return false;
				}

				return true;
			}
				
			return false;
		}

				#endregion //AreTransformGroupsEqual

				#region CoerceRenderTransform

		private static object CoerceRenderTransform(DependencyObject target, object value)
        {
			CardViewCard card				= target as CardViewCard;
			Transform	 explicitTransform	= value as Transform;

			// If there is not explicitly set tranform then return our internal TranslateTransform.
			if (explicitTransform == null)
				return card._translateTransform;

			// If the explicitly set transform is a TransformGroup then we should return that with our internal TranslateTransform
			// contained within.
			if (explicitTransform is TransformGroup)
			{
				TransformGroup explicitTransformGroup = explicitTransform as TransformGroup;

				// Since we need to create a TransformGroup that contains the transforms from the explicitly set TransformGroup in their 
				// original order as well as our internal TranslateTransform.  To do this we create a new TransformGroup by copying 
				// the transforms in the explicitly set TransformGroup (if we have never copied it or if it does not contain the same
				// entries as the last explicitTransform we copied) and then add in our internal TranslateTransform.
				if (card._explicitTransformGroupClones == null ||
					false == AreTransformGroupsEqual(explicitTransformGroup.Children, card._explicitTransformGroupOriginal.Children))
				{
					// Create 2 new TransformGroups - one to hold clones of the transforms in the explicitly set TransformGroup and the other to hold
					// the original Transforms in the explicitly set TransformGroup. We need the original copies so that we can call AreTransformGroupsEqual
					// above to see if a subsequently set explicit TransformGroup is the same as the one we have already created a cloned group for.
					card._explicitTransformGroupClones		= new TransformGroup();
					card._explicitTransformGroupOriginal	= new TransformGroup();
					
					foreach (Transform t in ((TransformGroup)explicitTransform).Children)
					{
						if (t.IsFrozen)
							card._explicitTransformGroupClones.Children.Add(t.Clone());
						else
							card._explicitTransformGroupClones.Children.Add(t);

						// Save the original
						card._explicitTransformGroupOriginal.Children.Add(t);
					}

					if (card._translateTransform != null)
					{
						card._explicitTransformGroupClones.Children.Add(card._translateTransform);
						card._explicitTransformGroupOriginal.Children.Add(card._translateTransform);
					}
					else
					{
						// Add a placeholder TranslateTransform to the groups because we could get in here before 
						// the internal TranslateTransform has been created in the UpdateTransform method.  Doing this ensures that
						// the call to AreTransformGroupsEqual above will return false once the internal TranslateTransform is created
						card._explicitTransformGroupClones.Children.Add(new TranslateTransform());		
						card._explicitTransformGroupOriginal.Children.Add(new TranslateTransform());	
					}
				}

				return card._explicitTransformGroupClones;
			}
			else  // The explicitly set transform is not a TransformGroup
			{
				// If we don't have a TranslateTransform yet just return the explicitly set transform.
				if (card._translateTransform == null)
					return explicitTransform;

				// At this point we have both an explicitly set transform as well as our internal TranslateTransform and we
				// need to put them into a TransformGroup and return that.
				if (null	== card._internalTransformGroup												||
					false	== card._internalTransformGroup.Children.Contains(card._translateTransform) ||
					false	== card._internalTransformGroup.Children.Contains(explicitTransform))
				{
					card._internalTransformGroup = new TransformGroup();
					card._internalTransformGroup.Children.Add(card._translateTransform);
					card._internalTransformGroup.Children.Add(explicitTransform);
				}
			}

			return card._internalTransformGroup;
		}

                #endregion //CoerceRenderTransform

				#region OnHeaderChanged

		/// <summary>
		/// Called when the <see cref="Header"/> property value changes.
		/// </summary>
		/// <param name="oldHeader">The old header value</param>
		/// <param name="newHeader">The new header value</param>
		private void OnHeaderChanged(object oldHeader, object newHeader)
		{
			base.RemoveLogicalChild(oldHeader);
			base.AddLogicalChild(newHeader);
		}

				#endregion //OnHeaderChanged

			#endregion //Private Methods

			#region Protected Methods

               #region VisualState... Methods


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager






        private void SetVisualState(bool useTransitions)
        {
            RecordPresenter rp = this.Content as RecordPresenter;
            Record record = rp != null ? rp.Record : null;

            if (record == null)
                return;

            if (this.IsMouseOver)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

            CardViewCard.SetVisualStateHelper(this, this, useTransitions);
        }
        
        internal static void SetVisualStateHelper(CardViewCard card, Control ctrl, bool useTransitions)
        {
            RecordPresenter rp = card.Content as RecordPresenter;
            Record record = rp != null ? rp.Record : null;

            if (record == null)
                return;

            // set active state
            if (card.IsActive)
                VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateActive, useTransitions);
            else
                VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateInactive, useTransitions);

            DataRecord dr = record as DataRecord;

            // set change state
            if (dr != null && dr.IsDataChanged)
                VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateChanged, useTransitions);
            else
                VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateUnchanged, useTransitions);

            // set expansion state
            if (card.IsCollapsed)
                VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateCollapsed, useTransitions);
            else
                VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateExpanded, useTransitions);

            // set record state
            if (card.IsFilterRecord)
                VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateFilterRecord, useTransitions);
            else
            {
                if (card.IsAddRecord)
                    VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateAddRecord, useTransitions);
                else
                    VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateDataRecord, useTransitions);
            }

            // set selection state
            if (card.IsSelected)
                VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateSelected, useTransitions);
            else
                VisualStateManager.GoToState(ctrl, VisualStateUtilities.StateUnselected, useTransitions);
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            CardViewCard card = target as CardViewCard;

            if ( card != null )
                card.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        private void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        private void UpdateVisualStates(bool useTransitions)
        {
            
            
            if (!this.IsLoaded)
                useTransitions = false;

            if (this._hasVisualStateGroups)
                this.SetVisualState(useTransitions);

            CardHeaderPresenter chp = this.HeaderPresenter;

            if (chp != null)
                chp.UpdateVisualStates(useTransitions);
        }



                #endregion //VisualState... Methods	

			#endregion //Protected Methods	

		#endregion //Methods

		#region IResizableElement Members

		object IResizableElement.ResizeContext
		{
			get { return this.Content; }
		}

		#endregion
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