using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter.Events;
using System.Windows.Data;
using System.Reflection;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A <see cref="ViewBase"/> dervied class that implements a custom card style view.
	/// </summary>
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
	public class CardView : ViewBase
	{
		#region Member Variables

		private CardViewSettings						_viewSettings = null;

		internal static DependencyProperty				s_ItemStringFormatProperty;
		internal static DependencyProperty				s_ContentStringFormatProperty;
		internal static DependencyProperty				s_HeaderStringFormatProperty;
		internal static PropertyInfo					s_BindingStringFormatInfo;

		internal DataPresenterBase						_dataPresenter;

		#endregion //Member Variables

		#region Constructor

		static CardView()
		{
			// Cache the DependencyPropertys for ItemStringFormat and ContentStringFormat since
			// these properties were only introduced in 3.5 we can't access these properties 
			// directly. Otherwise we will blow up when running on an earlier versions of the framework
			DependencyPropertyDescriptor pd = DependencyPropertyDescriptor.FromName("ItemStringFormat", typeof(ItemsControl), typeof(RecordListControl));
			if (pd != null)
			{
				s_ItemStringFormatProperty = pd.DependencyProperty;

				pd = DependencyPropertyDescriptor.FromName("ContentStringFormat", typeof(ContentControl), typeof(CardViewCard));

				if (pd != null)
					s_ContentStringFormatProperty = pd.DependencyProperty;

				pd = DependencyPropertyDescriptor.FromName("HeaderStringFormat", typeof(HeaderedContentControl), typeof(CardViewCard));

				if (pd != null)
					s_HeaderStringFormatProperty = pd.DependencyProperty;

				s_BindingStringFormatInfo = typeof(BindingBase).GetProperty("StringFormat");
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CardView"/> class
		/// </summary>
		public CardView()
		{
			this.SetValue(CardView.ViewSettingsProperty, this.ViewSettings);
		}

		#endregion //Constructor	
    
		#region Constants

		private const double							DEFAULT_ITEM_WIDTH = 200;
		private const double							DEFAULT_ITEM_HEIGHT = 200;

		#endregion //Constants

		#region Base Class Overrides

			#region Properties

				#region CellPresentation

		/// <summary>
		/// Returns the type of <see cref="CellPresentation"/> used by the view which determines the default manner in which the cells within each row are laid out by the <see cref="FieldLayoutTemplateGenerator"/>.
		/// </summary>
		/// <seealso cref="CellPresentation"/>
		/// <seealso cref="FieldLayoutTemplateGenerator"/>
		internal protected override CellPresentation CellPresentation
		{
			get { return CellPresentation.CardView; }
		}

				#endregion //CellPresentation	

				#region DefaultAutoArrangeCells

		/// <summary>
		/// Returns the default value for <see cref="AutoArrangeCells"/> for field layout templates generated on behalf of the View.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The base implementation returns <see cref="AutoArrangeCells"/>.LeftToRight.
		/// </p>
		/// </remarks>
		/// <seealso cref="AutoArrangeCells"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AutoArrangeCells"/>
		internal protected override AutoArrangeCells DefaultAutoArrangeCells
		{
			get { return AutoArrangeCells.TopToBottom; }
		}

				#endregion //DefaultAutoArrangeCells	

				#region DefaultAutoArrangeMaxRows

		/// <summary>
		/// Returns the default maximum number of rows of cells to auto-generate in the field layout templates.
		/// </summary>
		/// <remarks>
		/// The base implementation returns 0 which causes as many rows as necessary to be generated.
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.ViewBase.DefaultAutoArrangeCells"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.ViewBase.DefaultAutoArrangeMaxRows"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AutoArrangeMaxColumns"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AutoArrangeMaxRows"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AutoArrangeCells"/>
		internal protected override int DefaultAutoArrangeMaxRows
		{
			get { return 0; }
		}

				#endregion //DefaultAutoArrangeMaxRows

				#region DefaultAutoFit

		/// <summary>
		/// Returns the default value for the <see cref="DataPresenterBase.AutoFit"/> property if that property is set to null (Nothing in VB), its default value.
		/// </summary>
		/// <remarks>
		/// Returns true since the <see cref="Field"/>s should be sized to fill the Card.
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AutoArrangeCells"/>
		internal protected override bool DefaultAutoFit
		{
			get { return true; }
		}

				#endregion // DefaultAutoFit

				#region DefaultCellContentAlignment

        /// <summary>
        /// Returns the default <see cref="CellContentAlignment"/> for <see cref="Cell"/> content in the View.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="CellContentAlignment"/>.Default.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.CellContentAlignment"/>
        /// <seealso cref="CellContentAlignment"/>
		internal protected override CellContentAlignment DefaultCellContentAlignment
		{
			get { return CellContentAlignment.LabelLeftOfValueAlignMiddle; }
		}

				#endregion //DefaultLabelLocation

				// JM 02-23-10 TFS28229 - Added.
				#region DefaultDataRecordSizingMode

		/// <summary>
		/// Returns the default <see cref="DataRecordSizingMode"/> for data records in the View.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="DataRecordSizingMode"/>.Default.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataRecordSizingMode"/>
		/// <seealso cref="FieldLayoutSettings.DataRecordSizingMode"/>
		internal protected override DataRecordSizingMode DefaultDataRecordSizingMode
		{
			get { return DataRecordSizingMode.SizableSynchronized; }
		}

				#endregion //DefaultDataRecordSizingMode	

				#region DefaultLabelClickAction

        /// <summary>
        /// Returns the default <see cref="LabelClickAction"/> for cells in the View.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The base implementation returns LabelClickAction.Default.  This will ultimately cause <see cref="Field.DefaultLabelClickAction"/> to be used.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.LabelClickAction"/>
        /// <seealso cref="Field.DefaultLabelClickAction"/>
        internal protected override LabelClickAction DefaultLabelClickAction
		{
			// JM 01-18-10 TFS26093
			//get { return LabelClickAction.SelectField; }
			get { return LabelClickAction.Nothing; }
		}

				#endregion //DefaultLabelClickAction	

				#region DefaultLabelLocation

		/// <summary>
		/// Returns the default <see cref="LabelLocation"/> for <see cref="Field"/> Labels in the View.
		/// </summary>
		/// <seealso cref="Field"/>
		/// <seealso cref="LabelLocation"/>
		internal protected override LabelLocation DefaultLabelLocation
		{
			get { return LabelLocation.InCells; }
		}

				#endregion //DefaultLabelLocation

				#region DefaultShouldCollapseCards

		/// <summary>
		/// Returns a default value for whether the View should collapse Cards.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default implementation returns the current ViewSettings value for ShouldCollapseCards.
		/// </p>
		/// </remarks>
		internal protected override bool DefaultShouldCollapseCards
		{
			get { return this.ViewSettings.ShouldCollapseCards; }
		}

				#endregion //DefaultShouldCollapseCards

				#region DefaultShouldCollapseEmptyCells

		/// <summary>
		/// Returns a default value for whether the View should collapse Cells with empty values.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default implementation returns the current ViewSettings value for ShouldCollapseEmptyCells.
		/// </p>
		/// </remarks>
		internal protected override bool DefaultShouldCollapseEmptyCells
		{
			get { return this.ViewSettings.ShouldCollapseEmptyCells; }
		}

				#endregion //DefaultShouldCollapseEmptyCells

				#region HasLogicalOrientation

		/// <summary>
		/// Returns a value that indicates whether this View arranges its descendants in a particular dimension.
		/// </summary>
		internal protected override bool HasLogicalOrientation
		{
			get { return false; }
		}

				#endregion //HasLogicalOrientation	
		
				// JJD 4/28/11 - TFS73523 - added 
				#region InterRecordSpacingX

		/// <summary>
		/// Returns the amount of space between records in the X dimension (used for resizing logic).
		/// </summary>
		public override double InterRecordSpacingX
		{
			get
			{
				if (this._viewSettings != null && this._viewSettings.AllowCardWidthResizing)
					return this._viewSettings.InterCardSpacingX;

				return base.InterRecordSpacingX;
			}
		}

				#endregion //InterRecordSpacingX	
    
				// JJD 4/28/11 - TFS73523 - added 
				#region InterRecordSpacingY

		/// <summary>
		/// Returns the amount of space between records in the Y dimension (used for resizing logic).
		/// </summary>
		public override double InterRecordSpacingY
		{
			get
			{
				if (this._viewSettings != null && this._viewSettings.AllowCardHeightResizing)
					return this._viewSettings.InterCardSpacingY;

				return base.InterRecordSpacingY;
			}
		}

				#endregion //InterRecordSpacingY	
    
				#region IsGroupBySupported

		/// <summary>
		/// Returns true if the <see cref="DataPresenterBase"/> should display the <see cref="GroupByArea"/> by default and allow programmatic grouping of records.
		/// </summary>
		internal protected override bool IsGroupBySupported
		{
			get	{ return false; }
		}

				#endregion //IsGroupBySupported	

				#region IsAutoFitWidthSupported

		/// <summary>
		/// Returns true if the width of the cells within in each row should be adjusted so that all cells will fit within the horizontal space available for the row.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default implementation returns false.
		/// </p>
		/// </remarks>
		internal protected override bool IsAutoFitWidthSupported
		{
			get { return false == double.IsNaN(this.ViewSettings.CardWidth); }
		}

   				#endregion //IsAutoFitWidthSupported

				#region IsEmptyCellCollapsingSupported

		/// <summary>
		/// Returns true if the View supports the collapsing of Cells with empty values.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default implementation returns true.
		/// </p>
		/// </remarks>
		internal protected override bool IsEmptyCellCollapsingSupported
		{
			get { return true; }
		}

				#endregion //IsEmptyCellCollapsingSupported

				#region IsFilterRecordSupported

		/// <summary>
		/// Returns true if the <see cref="DataPresenterBase"/> should display filter records.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The base implementation returns false.
		/// </p>
		/// </remarks>
		/// <seealso cref="DataPresenterBase"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		internal protected override bool IsFilterRecordSupported
		{
			get { return true; }
		}

				#endregion // IsFilterRecordSupported

				#region IsLogicalFieldHeightResizingAllowed

		/// <summary>
		/// Returns true if the DataPresenter UI should allow logical field heights in this View to be resized.
		/// </summary>
		internal protected override bool IsLogicalFieldHeightResizingAllowed
		{
			// AS 1/22/10
			// If we want to allow the end user to resize the field when the card
			// is sized then we must return true here.
			//
			get { return true; }
		}

				#endregion //IsLogicalFieldHeightResizingAllowed	
    
				#region IsLogicalFieldWidthResizingAllowed

		/// <summary>
		/// Returns true if the DataPresenter UI should allow logical field widths in this View to be resized.
		/// </summary>
		internal protected override bool IsLogicalFieldWidthResizingAllowed
		{
			get	{ return true; }
		}

				#endregion //IsLogicalFieldWidthResizingAllowed	

				#region IsNestedPanelsSupported

		/// <summary>
		/// Returns true if the View supports nested panels to display hierarchical data.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns true.
        /// </p>
        /// </remarks>
		internal protected override bool IsNestedPanelsSupported
		{
			get { return false; }
		}

				#endregion //IsNestedPanelsSupported


				// JJD 2/22/12 - TFS101199 - Touch support
				#region IsPanningModeSupported

		/// <summary>
		/// Returns true if the RecordListControl should coerce its ScrollViewer.PanningMode property to enable standard flick scrolling on touch enabled systems.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// This implementation returns true.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase"/>
		internal protected override bool IsPanningModeSupported
		{
			get { return true; }
		}

				#endregion //IsPanningModeSupported


				#region IsSummaryRecordSupported

		/// <summary>
		/// Returns false since carousel view doesn't support displaying summary records.
		/// </summary>
		/// <seealso cref="DataPresenterBase"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="FieldSettings.AllowSummaries"/>
		internal protected override bool IsSummaryRecordSupported
		{
			get { return false; }
		}

				#endregion // IsSummaryRecordSupported
        
				#region ItemsPanelType

		/// <summary>
		/// Returns the type of the ItemsPanel used by the view to layout items in the list.
		/// </summary>
		internal protected override Type ItemsPanelType
		{
			get { return typeof(CardViewPanel); }
		}

				#endregion //ItemsPanelType	

				#region RecordPresenterContainerType

		/// <summary>
		/// Returns the type used as the container (if any) for RecordPresenters in the View.
		/// </summary>
		/// <remarks>
		/// <p class="body">The base implementation returns null.</p>
		/// <p class="note"><b>Note:</b> If this property is overridden then the <see cref="GetContainerForRecordPresenter(Panel)"/> method should also be overridden to return a container of the same type.</p>
		/// </remarks>
		/// <seealso cref="GetContainerForRecordPresenter(Panel)"/>
		internal protected override Type RecordPresenterContainerType
		{
			get { return typeof(CardViewCard); }
		}

				#endregion //RecordPresenterContainerType

				#region ShouldDisplayRecordSelectors

		/// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> should generate and display a record selector for each record in the View.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The base implementation returns true.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase"/>
        internal protected override bool ShouldDisplayRecordSelectors
		{
			get { return false; }
		}

				#endregion //ShouldDisplayRecordSelectors	

			#endregion //Properties

			#region Methods

				#region GetContainerForRecordPresenter

		// SSP 4/22/08 - Summaries Feature
		// Took out RecordPresenter parameter from GetContainerForRecordPresenter because in GetContainerForItemOverride
		// we don't have the item and thus don't know which record presenter to create, and thus we cannot pass that
		// along into this method. Instead we added PrepareContainerForRecordPresenter method that will allow the view
		// to associated its wrapper to the record presenter.
		// 
		/// <summary>
		/// Returns an element that will wrap a <see cref="RecordPresenter"/> inside it. If no wrapper is required, the method should return <b>null</b>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method, along with the <see cref="PrepareContainerForRecordPresenter"/>, only need to be overridden if the 
		/// view requires a specialized wrapper element around each <see cref="RecordPresenter"/> element. You would create
		/// a wrapper element in this method and associate it with a record presenter in <i>PrepareContainerForRecordPresenter</i> method.
		/// </p>
		/// </remarks>
		/// <param name="panel">The <see cref="System.Windows.Controls.Panel"/> derived element that will contain the returned wrapper element.</param>
		/// <returns>The container object to use to wrap a <see cref="RecordPresenter"/>.</returns>
		/// <seealso cref="PrepareContainerForRecordPresenter"/>
		internal protected override DependencyObject GetContainerForRecordPresenter( Panel panel )
		{
			return new CardViewCard( );
		}
		
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetContainerForRecordPresenter	

                #region GetFieldLayoutTemplateGenerator

        /// <summary>
        /// Gets a FieldLayoutTemplateGenerator derived class for generating an appropriate template for the specified layout in the current View.
        /// </summary>
        /// <param name="fieldLayout">The specified layout</param>
		internal protected override FieldLayoutTemplateGenerator GetFieldLayoutTemplateGenerator(FieldLayout fieldLayout)
        {
            return new CardViewFieldLayoutTemplateGenerator(fieldLayout, this.ViewSettings.Orientation, true);
        }

                #endregion //GetFieldLayoutTemplateGenerator

				#region OnPropertyChanged

		/// <summary>
		/// Called when the value of a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

            if (e.Property == ViewSettingsProperty)
            {
                // Unhook from old
                if (this._viewSettings != null)
                {
                    this._viewSettings.PropertyChanged -= new PropertyChangedEventHandler(this.OnViewSettingsPropertyChanged);
                    
                    // Reset our CachedHeaderPath
                    this.CachedHeaderPath = string.Empty;
                }


                // Set our ViewSettings member variable.
                this._viewSettings = e.NewValue as CardViewSettings;


                // Hook new change event
                if (this._viewSettings != null)
                {
                    this._viewSettings.PropertyChanged += new PropertyChangedEventHandler(this.OnViewSettingsPropertyChanged);

                    // UPdate our CachedHeaderPath
                    this.CachedHeaderPath = this._viewSettings.HeaderPath;
                }


                // Raise a ViewStateChanged event.
                this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateMeasure));
			}
		}

				#endregion //OnPropertyChanged	

				#region PrepareContainerForRecordPresenter

		// SSP 4/22/08 - Summaries Feature
		// Took out RecordPresenter parameter from GetContainerForRecordPresenter because in GetContainerForItemOverride
		// we don't have the item and thus don't know which record presenter to create, and thus we cannot pass that
		// along into this method. Instead we added PrepareContainerForRecordPresenter method that will allow the view
		// to associated its wrapper to the record presenter.
		// 
		/// <summary>
		/// Associates the wrapper element returned from <see cref="GetContainerForRecordPresenter"/> with 
		/// the specified <see cref="RecordPresenter"/> element.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method, along with the <see cref="GetContainerForRecordPresenter"/>, only need to be overridden if the 
		/// view requires a specialized wrapper element around each <see cref="RecordPresenter"/> element. You would create
		/// a wrapper element in <see cref="GetContainerForRecordPresenter"/> and associate it with a record presenter in 
		/// this method.
		/// </p>
		/// </remarks>
		/// <param name="panel">The <see cref="System.Windows.Controls.Panel"/> derived element that contains the wrapper element.</param>
		/// <param name="wrapper">The wrapper element that was returned from the <see cref="GetContainerForRecordPresenter"/>.</param>
		/// <param name="recordPresenter">The <see cref="RecordPresenter"/> to associate with the wrapper.</param>
		/// <seealso cref="GetContainerForRecordPresenter"/>
		internal protected override void PrepareContainerForRecordPresenter( Panel panel, DependencyObject wrapper, RecordPresenter recordPresenter )
		{
			if (recordPresenter != null)
				this._dataPresenter = recordPresenter.DataPresenter;

			// Create a CardViewCard and set its Content to the RecordPresenter
			CardViewCard card = wrapper as CardViewCard;
			card.Content	= recordPresenter;
			card.View		= this;
			card.Panel		= recordPresenter.DataPresenter != null ? recordPresenter.DataPresenter.CurrentPanel as CardViewPanel : null;

			// JM 01-14-10 TFS25927
			card.IsCollapsed= recordPresenter.Record.IsContainingCardCollapsedResolved;

			// JM 02--5-10 TFS27237
			card.Initialize(recordPresenter);

			// Setup the content to be used for the header
			this.SetupHeaderContent(card, recordPresenter);


			// Bind the HeaderVisibility property of the Card to our ViewSettings.HeaderVisibility
			Binding binding = new Binding();
			binding.Source	= this;
			binding.Path	= new PropertyPath("ViewSettings.HeaderVisibility", new object[0]);
			binding.Mode	= BindingMode.OneWay;
			card.SetBinding(CardViewCard.HeaderVisibilityProperty, binding);

			binding			= new Binding();
			binding.Source	= recordPresenter.Record;
			binding.Path	= new PropertyPath("ShouldCollapseEmptyCellsResolved", new object[0]);
			binding.Mode	= BindingMode.OneWay;
			card.SetBinding(CardViewCard.ShouldCollapseEmptyCellsProperty, binding);

			binding			= new Binding();
			binding.Source	= recordPresenter.Record;
			binding.Path	= new PropertyPath("IsActive", new object[0]);
			binding.Mode	= BindingMode.OneWay;
			card.SetBinding(CardViewCard.IsActiveProperty, binding);

			binding			= new Binding();
			binding.Source	= recordPresenter.Record;
			binding.Path	= new PropertyPath("IsSelected", new object[0]);
			binding.Mode	= BindingMode.OneWay;
			card.SetBinding(CardViewCard.IsSelectedProperty, binding);

			binding			= new Binding();
			binding.Source	= this;
			binding.Path = new PropertyPath("ViewSettings.CollapseCardButtonVisibility", new object[0]);
			binding.Mode	= BindingMode.OneWay;
			card.SetBinding(CardViewCard.CollapseCardButtonVisibilityProperty, binding);

			binding			= new Binding();
			binding.Source	= this;
			binding.Path	= new PropertyPath("ViewSettings.CollapseEmptyCellsButtonVisibility", new object[0]);
			binding.Mode	= BindingMode.OneWay;
			card.SetBinding(CardViewCard.CollapseEmptyCellsButtonVisibilityProperty, binding);

			// Calling InvalidateRequerySuggested here because in the case where the setting of the card.Content property above
			// is setting the Content property to the same RecordPresenter, the CollapseCard button was showing disabled because
			// its associated ToggleCardCollapsedState command was returning CanExecute = false.
			System.Windows.Input.CommandManager.InvalidateRequerySuggested();
		}

				#endregion //PrepareContainerForRecordPresenter
		
			#endregion //Methods

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region ViewSettings

		/// <summary>
		/// Identifies the <see cref="ViewSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ViewSettingsProperty = DependencyProperty.Register("ViewSettings",
			typeof(CardViewSettings), typeof(CardView), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceViewSettings)));

		private static object OnCoerceViewSettings(DependencyObject d, object value)
		{
			if (!(value is CardViewSettings))
				return new CardViewSettings();

			return value;
		}

		/// <summary>
        /// Returns/sets the <see cref="CardViewSettings"/> object for this CardView.
		/// </summary>
        /// <remarks>
        /// <p class="body"><see cref="CardViewSettings"/> exposes properties that let you control features supported by the CardView.  
        /// Refer to <see cref="CardViewSettings"/> object for detailed information on these properties.</p>
        /// </remarks>
		/// <seealso cref="ViewSettingsProperty"/>
		/// <seealso cref="CardViewSettings"/>
		//[Description("Returns/sets the CardViewSettings object for this CardView.")]
		//[Category("Appearance")]
		[Bindable(true)]
        public CardViewSettings ViewSettings
		{
			get
			{
				if (this._viewSettings == null)
					this._viewSettings = new CardViewSettings();

				return this._viewSettings;
			}
			set
			{
				this.SetValue(CardView.ViewSettingsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ViewSettings"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeViewSettings()
		{
			return this.ViewSettings != (CardViewSettings)CardView.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ViewSettings"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetViewSettings()
		{
			this.ClearValue(CardView.ViewSettingsProperty);
		}

				#endregion //ViewSettings

			#endregion //Public Properties

			#region Internal Properties

				#region CachedHeaderPath

		/// <summary>
		/// Identifies the <see cref="CachedHeaderPath"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty CachedHeaderPathProperty = DependencyProperty.Register("CachedHeaderPath",
			typeof(string), typeof(CardView), new FrameworkPropertyMetadata(string.Empty));

		internal string CachedHeaderPath
		{
			get
			{
				return (string)this.GetValue(CardView.CachedHeaderPathProperty);
			}
			set
			{
				this.SetValue(CardView.CachedHeaderPathProperty, value);
			}
		}

				#endregion //CachedHeaderPath

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region OnViewSettingsPropertyChanged

		private void OnViewSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				// JJD 4/28/11 - TFS73523 - added 
				case "AllowCardWidthResizing":
				case "AllowCardHeightResizing":
					this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateRender));
					break;

				case "HeaderPath":
					this.CachedHeaderPath = this.ViewSettings.HeaderPath;
					break;

				case "AutoFitCards":
				// JM 04-26-11 TFS66510 Move below into a separate group and change the ViewStateChangedAction to InvalidateMeasure.
				//case "CardHeight":
				//case "CardWidth":
				case "CollapseCardButtonVisibility":
				case "CollapseEmptyCellsButtonVisibility":
				case "InterCardSpacingX":
				case "InterCardSpacingY":
				case "HeaderVisibility":
				case "MaxCardCols":
				case "MaxCardRows":
				case "Orientation":
				case "Padding":
					this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateFieldLayouts));
					break;

				// JM 04-26-11 TFS66510 Moved from above and changed the ViewStateChangedAction to InvalidateMeasure.
				case "CardHeight":
				case "CardWidth":
					this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateMeasure));
					break;

				case "ShouldCollapseCards":
					this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateFieldLayouts));

					if (this._dataPresenter != null)
					{
						foreach (Record record in this._dataPresenter.ViewableRecords)
							record.RaisePropertyChangedEventInternal("IsContainingCardCollapsedResolved");
					}

					break;

				case "ShouldCollapseEmptyCells":
					this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateFieldLayouts));

					if (this._dataPresenter != null)
					{
						foreach (Record record in this._dataPresenter.ViewableRecords)
							record.RaisePropertyChangedEventInternal("ShouldCollapseEmptyCellsResolved");
					}

					break;

				default:
					break;
			}
		}

			#endregion //OnViewSettingsPropertyChanged

			#region SetupHeaderContent

		private void SetupHeaderContent(CardViewCard card, RecordPresenter recordPresenter)
		{
			// Bind the Card's Header to the value of the Property specified by ViewSettings.HeaderPath if set, or bind it to the value of the PrimaryField.
			//
			// Use the cached DependencyPropertys for ItemStringFormat and ContentStringFormat since
			// these properties were only introduced in 3.5 we can't access these properties 
			// directly. Otherwise we will blow up when running on an earlier versions of the framework
			string stringFormat = null;
			if (s_ItemStringFormatProperty		!= null &&
				s_ContentStringFormatProperty	!= null &&
				s_HeaderStringFormatProperty	!= null)
			{
				stringFormat = this.GetValue(s_ItemStringFormatProperty) as string;

				if (stringFormat != null)
				{
					card.SetValue(s_ContentStringFormatProperty, stringFormat);
					card.SetValue(s_HeaderStringFormatProperty, stringFormat);
				}
			}

			card.ResolveHeader();
		}

			#endregion //SetupHeaderContent

		#endregion //Methods
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