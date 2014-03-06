using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.DataPresenter.Internal;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using Infragistics.Windows.Reporting;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	#region GroupBySummariesPresenter Class

	/// <summary>
	/// Element used for displaying summaries inside group-by records.
	/// </summary>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupBySummariesPresenter : Control
	{
		#region Private Vars

		private PropertyValueTracker _summariesVersionTracker;
		private ArrayList _propertyChangeTrackers;

        // JJD 1/19/09 - NA 2009 vol 1 - no longer needed
        //private PropertyValueTracker _internalVersionTracker;
		//private SummaryRecordPresenter.ContentAreaMarginsManager _contentAreaMarginsManager;

		#endregion // Private Vars

		#region Constructor

		static GroupBySummariesPresenter( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( GroupBySummariesPresenter ), new FrameworkPropertyMetadata( typeof( GroupBySummariesPresenter ) ) );
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( GroupBySummariesPresenter ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="GroupBySummariesPresenter"/>.
		/// </summary>
		public GroupBySummariesPresenter( )
		{
            // JJD 1/19/09 - NA 2009 vol 1 - no longer needed
            //_contentAreaMarginsManager = new SummaryRecordPresenter.ContentAreaMarginsManager( this );

			// Whenever summaries are added/removed, we need to reposition the summaries.
			// 
			_summariesVersionTracker = new PropertyValueTracker( this,
				"GroupByRecord.ChildRecords.SummaryResults.SummaryDefinitions.SummariesVersion",
				this.VerifySummaries,
				// MD 8/17/10
				// We should be handling the dirtying of this version asynchronously because 
				// it may happen multiple times in the a row.
				true);

            // JJD 1/19/09 - NA 2009 vol 1 - no longer needed
            //// Whenever group-by hierarchy is changed, the offset of summaries in the group-by record 
            //// may change so re-calculate the content area margins.
            //// 
            //_internalVersionTracker = new GridUtilities.PropertyValueTracker( this,
            //    "GroupByRecord.FieldLayout.InternalVersion",
            //    _contentAreaMarginsManager.InitContentAreaMargins );
		}

		#endregion // Constructor

		#region Base Overrides
        
            // JJD 1/19/09 - NA 2009 vol 1 
            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // JJD 1/19/09 - NA 2009 vol 1 - no longer needed
            // Initialize content area margin bindings
            SummaryRecordPresenter.InitializeContentAreaMarginBinding(this, this.GroupByRecord, this.GetType());
        }

            #endregion //OnApplyTemplate

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region ContentAreaMargins

		/// <summary>
		/// Identifies the <see cref="ContentAreaMargins"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentAreaMarginsProperty = 
			SummaryRecordPresenter.ContentAreaMarginsProperty.AddOwner( typeof( GroupBySummariesPresenter ) );

		/// <summary>
		/// Returns the margins for the contents inside the element. This is the amount
		/// the summary cells need to be offset by to align them with the field labels.
		/// </summary>
		//[Description( "Returns the margins for the contents inside the element." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Thickness ContentAreaMargins
		{
			get
			{
				return (Thickness)this.GetValue( ContentAreaMarginsProperty );
			}
			set
			{
				this.SetValue( ContentAreaMarginsProperty, value );
			}
		}

		#endregion // ContentAreaMargins

		#region DisplaySummariesAsCells

		private static readonly DependencyPropertyKey DisplaySummariesAsCellsPropertyKey = DependencyProperty.RegisterReadOnly(
				"DisplaySummariesAsCells",
				typeof( bool ),
				typeof( GroupBySummariesPresenter ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="DisplaySummariesAsCells"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisplaySummariesAsCellsProperty = DisplaySummariesAsCellsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if summary results are to be displayed as cells, false if they are to be displayed as plain text.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// By default summary results are displayed inside group-by record as summary cells that are aligned with
		/// their associated fields. You can change this behavior by setting the <see cref="FieldLayoutSettings.GroupBySummaryDisplayMode"/>
		/// property to <i>Text</i>, in which case summary results will be displayed as part of the group-by record's description as plain 
		/// text. <b>DisplaySummariesAsCells</b> property returns <i>true</i> if summary results are
		/// to be displayed as summary cells and returns <i>false</i> if they are to be displayed as plain text.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.GroupBySummaryDisplayMode"/>
		//[Description( "Indicates whether summaries are displayed in the group-by record as cells." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool DisplaySummariesAsCells
		{
			get
			{
				return (bool)this.GetValue( DisplaySummariesAsCellsProperty );
			}
		}

		#endregion // DisplaySummariesAsCells

		#region GroupByRecord

		/// <summary>
		/// Identifies the <see cref="GroupByRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupByRecordProperty = DependencyProperty.Register(
				"GroupByRecord",
				typeof( GroupByRecord ),
				typeof( GroupBySummariesPresenter ),
				new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
					new PropertyChangedCallback( OnGroupByRecordChanged ) )
			);

		/// <summary>
		/// Gets or sets the associated GroupByRecord.
		/// </summary>
		//[Description( "Associated GroupByRecord" )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public GroupByRecord GroupByRecord
		{
			get
			{
				return (GroupByRecord)this.GetValue( GroupByRecordProperty );
			}
			set
			{
				this.SetValue( GroupByRecordProperty, value );
			}
		}

		private static void OnGroupByRecordChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			GroupBySummariesPresenter presenter = (GroupBySummariesPresenter)dependencyObject;

			// Since there's a SummaryRecordCellArea in the element hierarchy of the group-by summaries
			// presenter, set the DataContext of this element to a SummaryRecord that has the summary
			// results to be displayed in the summary record cell area.
			// 
			SummaryRecord summaryRecord = null;
			GroupByRecord groupByRecord = e.NewValue as GroupByRecord;
			if ( null != groupByRecord )
			{
				// AS 2/9/11 NA 2011.1 Word Writer
				//summaryRecord = new SummaryRecord( groupByRecord.FieldLayout, groupByRecord.ChildRecords,
				//	SummaryDisplayAreaContext.InGroupByRecordsSummariesContext );
				summaryRecord = groupByRecord.CreateSummaryRecord();
			}

			presenter.DataContext = summaryRecord;

            // JJD 1/19/09 - NA 2009 vol 1 - no longer needed
            //presenter._contentAreaMarginsManager.InitRecord( groupByRecord );

            // JJD 1/19/09 - NA 2009 vol 1 
            // Initialize content area margin bindings
            SummaryRecordPresenter.InitializeContentAreaMarginBinding(presenter, presenter.GroupByRecord, presenter.GetType());

            presenter.VerifySummaries();
		}

		#endregion // GroupByRecord

		#region HasSummaries

		private static readonly DependencyPropertyKey HasSummariesPropertyKey = DependencyProperty.RegisterReadOnly(
				"HasSummaries",
				typeof( bool ),
				typeof( GroupBySummariesPresenter ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="HasSummaries"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasSummariesProperty = HasSummariesPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates whether there are summaries to be displayed in this presenter.
		/// </summary>
		//[Description( "Indicates whether there are summaries to be displayed in this presenter." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool HasSummaries
		{
			get
			{
				return (bool)this.GetValue( HasSummariesProperty );
			}
		}

		#endregion // HasSummaries

		#region SummaryResults

		private static readonly DependencyPropertyKey SummaryResultsPropertyKey = DependencyProperty.RegisterReadOnly(
				"SummaryResults",
				typeof( IEnumerable<SummaryResult> ),
				typeof( GroupBySummariesPresenter ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="SummaryResults"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryResultsProperty = SummaryResultsPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the summary results that will be displayed inside this element.
		/// </summary>
		//[Description( "Gets the summary results that will be displayed inside this element." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IEnumerable<SummaryResult> SummaryResults
		{
			get
			{
				return (IEnumerable<SummaryResult>)this.GetValue( SummaryResultsProperty );
			}
		}

		#endregion // SummaryResults

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#endregion // Public Methods

		#region Private/Internal Methods

		#region VerifySummaries

		private void VerifySummaries( )
		{
			GroupByRecord groupByRecord = this.GroupByRecord;
			IEnumerable<SummaryResult> results = null != groupByRecord ? groupByRecord.GetSummaryResults( ) : null;

			bool displaySummariesAsCells = false;
			FieldLayout fl = null != groupByRecord ? groupByRecord.FieldLayout : null;
			if ( null != fl )
			{
				GroupBySummaryDisplayMode gbrSummaryDisplayStyle = fl.GroupBySummaryDisplayModeResolved;
				if ( //GroupBySummaryDisplayMode.SummaryCells == gbrSummaryDisplayStyle || 
					GroupBySummaryDisplayMode.SummaryCellsAlwaysBelowDescription == gbrSummaryDisplayStyle )
					displaySummariesAsCells = true;
			}

			_propertyChangeTrackers = new ArrayList( );

			bool hasSummaries = false;
			if ( null != results )
			{
				foreach ( SummaryResult result in results )
				{
					hasSummaries = true;

					// SSP 8/2/09 - Summary Recalc Optimizations
					// 
					
					PropertyValueTracker pvt = new PropertyValueTracker( result,
						"DataVersion", this.VerifySummaryCalculations, true );

					pvt.ThrottleTime = SummaryResultPresenter.THROTTLE_TIME;

					// SSP 4/10/12 TFS108549 - Optimizations
					// 
					pvt.ThrottleFirstInvoke = true;

					_propertyChangeTrackers.Add( pvt );

					if ( null != groupByRecord )
					{
						pvt = new PropertyValueTracker( result,
							"DisplayText", groupByRecord.DirtyDescriptionWithSummaries, true );

						pvt.ThrottleTime = SummaryResultPresenter.THROTTLE_TIME;

						// SSP 4/10/12 TFS108549 - Optimizations
						// 
						pvt.ThrottleFirstInvoke = true;

						_propertyChangeTrackers.Add( pvt );

						// SSP 3/22/10 TFS27718
						// When a field is hidden, we need to re-get the group-by description text because
						// the summary for the hidden field will be excluded from it.
						// 
						// --------------------------------------------------------------------------------
						pvt = new PropertyValueTracker( result, "PositionFieldResolved.VisibilityResolved",
							groupByRecord.DirtyDescriptionWithSummaries, true );

						pvt.ThrottleTime = SummaryResultPresenter.THROTTLE_TIME;

						// SSP 4/10/12 TFS108549 - Optimizations
						// 
						pvt.ThrottleFirstInvoke = true;

						_propertyChangeTrackers.Add( pvt );
						// --------------------------------------------------------------------------------
					}

					
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

					
				}
			}

			this.SetValue( SummaryResultsPropertyKey, results );
			this.SetValue( HasSummariesPropertyKey, hasSummaries );
			this.SetValue( DisplaySummariesAsCellsPropertyKey, displaySummariesAsCells );

			this.VerifySummaryCalculations( );

			// MD 8/13/10
			// VerifySummaryCalculations already calls DirtyDescriptionWithSummaries and considering it is realtively 
			// slow, we should only call it once.
			//if ( null != groupByRecord )
			//    groupByRecord.DirtyDescriptionWithSummaries( );

			// Also refresh ShouldDisplaySummaryCells property on group-by record presenter.
			// 
			GroupByRecordPresenter grp = (GroupByRecordPresenter)Utilities.GetAncestorFromType( this, typeof( GroupByRecordPresenter ), true );
			if ( null != grp )
				grp.UpdateShouldDisplaySummaries( );
		}

		#endregion // VerifySummaries

		#region VerifySummaryCalculations

		private void VerifySummaryCalculations( )
		{
			GroupByRecord groupByRecord = this.GroupByRecord;
			IEnumerable<SummaryResult> results = this.SummaryResults;
			if ( null != results )
			{
				foreach ( SummaryResult result in results )
					result.EnsureCalculated( );
			}

			if ( null != groupByRecord )
				groupByRecord.DirtyDescriptionWithSummaries( );
		}

		#endregion // VerifySummaryCalculations

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // GroupBySummariesPresenter Class

	#region SummaryButton Class

	/// <summary>
	/// Used for displaying summary button inside each field's label. When a summary button is clicked upon, a user interface
	/// for selecting summary calculations for the field is displayed.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// To actually show summary buttons inside field labels, set the FieldSettings's <see cref="FieldSettings.AllowSummaries"/>
	/// property.
	/// </para>
	/// </remarks>
	/// <seealso cref="FieldSettings.AllowSummaries"/>
	/// <seealso cref="FieldSettings.SummaryUIType"/>
	/// <seealso cref="SummaryCalculatorSelectionControl"/>
	/// <seealso cref="DataPresenterBase.SummarySelectionControlOpening"/>
	/// <seealso cref="DataPresenterBase.SummarySelectionControlClosed"/>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryButton : ToggleButton
	{
		#region Private Vars

		private bool _isCanceled;

		#endregion // Private Vars

		#region Constructor

		static SummaryButton( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryButton ), new FrameworkPropertyMetadata( typeof( SummaryButton ) ) );
            
            // JJD 3/5/09 - TFS5971/BR32606 
            // The summary button should not accespt focus by default
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( SummaryButton ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );

			ToggleButton.IsCheckedProperty.OverrideMetadata( typeof( SummaryButton ),
				new FrameworkPropertyMetadata( null, new CoerceValueCallback( OnCoerceIsChecked ) ) );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryButton"/>.
		/// </summary>
		public SummaryButton( )
		{
			this.AddHandler( Button.ClickEvent, new RoutedEventHandler( this.OnButtonClick ) );

			// SSP 11/10/11 TFS95707
			// 
			CommandManager.AddPreviewCanExecuteHandler( this, this.OnPreviewCanExecuteCommandHandler );
		}

		#endregion // Constructor

        #region Base class overrides

            // JJD 3/5/09 - TFS5971/BR32606 - added
            #region OnMouseLeftButtonDown

        /// <summary>
        /// Caled when the left mouse button is pressed
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(this);

            Rect rect = new Rect(this.RenderSize);

            if (!rect.Contains(pt))
            {
                e.Handled = true;
                return;
            }

            base.OnMouseLeftButtonDown(e);
        }

            #endregion //OnMouseLeftButtonDown

        #endregion //Base class overrides	
        
		#region Properties

		#region Public Properties

		#region Field

		/// <summary>
		/// Identifies the <see cref="Field"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldProperty = DependencyProperty.Register(
			"Field",
			typeof( Field ),
			typeof( SummaryButton ),
			new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Gets the associated field whose label is displaying this summary button inside it.
		/// </summary>
		//[Description( "Associated field." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Field Field
		{
			get
			{
				return (Field)this.GetValue( FieldProperty );
			}
			set
			{
				this.SetValue( FieldProperty, value );
			}
		}

		#endregion // Field

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region OnCoerceIsChecked

		private static object OnCoerceIsChecked( DependencyObject dependencyObject, object valueAsObject )
		{
			SummaryButton button = (SummaryButton)dependencyObject;
			bool newVal = (bool?)valueAsObject ?? false;
			bool oldVal = button.IsChecked ?? false;

			if ( newVal != oldVal )
			{
				SummaryCalculatorSelectionControl control = (SummaryCalculatorSelectionControl)GridUtilities.GetTrivialDescendantOfType( button, typeof( SummaryCalculatorSelectionControl ), true );
				if ( null != control )
				{
					if ( newVal )
					{
						// When opening the popup, reset _isCanceled flag to false.						
						button._isCanceled = false;

						if ( !control.OnOpenning( ) )
							return false;
					}
					else
					{
						control.OnClosed( button._isCanceled );
					}
				}
			}

			return valueAsObject;
		}

		#endregion // OnCoerceIsChecked

		#endregion // Public Methods

		#region Private/Internal Methods

		#region OnButtonClick

		private void OnButtonClick( object sender, RoutedEventArgs e )
		{
			Button button = e.OriginalSource as Button;
			if ( null != button )
			{
				_isCanceled = false;
				// SSP 5/12/09 TFS16824
				// 
				//if ( button.Name == "RowSummaryCancelButton" )
				if ( button.IsCancel || button.Name == "RowSummaryCancelButton" )
				{
					_isCanceled = true;
					this.IsChecked = false;
				}
				// SSP 5/12/09 TFS16824
				// 
				//else if ( button.Name == "RowSummaryOkButton" )
				else if ( button.IsDefault || button.Name == "RowSummaryOkButton" )
				{
					_isCanceled = false;
					this.IsChecked = false;
				}
			}
		}

		#endregion // OnButtonClick

		#region OnPreviewCanExecuteCommandHandler

		// SSP 11/10/11 TFS95707
		// 
		private void OnPreviewCanExecuteCommandHandler( object sender, CanExecuteRoutedEventArgs e )
		{
			RoutedCommand routedCommand = e.Command as RoutedCommand;
			if ( null != routedCommand && "DialogCancel" == routedCommand.Name )
			{
				e.CanExecute = false;
				e.ContinueRouting = false;
				e.Handled = true;
			}
		}

		#endregion // OnPreviewCanExecuteCommandHandler

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // SummaryButton Class

	#region SummaryCalculatorHolder

	/// <summary>
	/// This class is used by the <see cref="SummaryCalculatorSelectionControl"/>.
	/// </summary>
	/// <seealso cref="SummaryCalculatorSelectionControl"/>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryCalculatorHolder : DependencyObject
	{
		#region Private Vars

		private SummaryCalculatorSelectionControl _selectionControl;
		private SummaryCalculator _calculator;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryCalculatorHolder"/>.
		/// </summary>
		/// <param name="selectionControl">The associated summary calculator selection control.</param>
		/// <param name="calculator">The associated summary calculator.</param>
		/// <param name="isChecked">Whether it's currenty selected in the SummaryCalculatorSelectionControl.</param>
		internal SummaryCalculatorHolder( SummaryCalculatorSelectionControl selectionControl, 
			SummaryCalculator calculator, bool isChecked )
		{
			GridUtilities.ValidateNotNull( selectionControl );
			GridUtilities.ValidateNotNull( calculator );

			_selectionControl = selectionControl;
			_calculator = calculator;
			this.IsChecked = isChecked;
		}

		#endregion // Constructor

		#region Calculator

		/// <summary>
		/// Gets the associated summary calculator.
		/// </summary>
		/// <seealso cref="SummaryCalculator"/>
		public SummaryCalculator Calculator
		{
			get
			{
				return _calculator;
			}
		}

		#endregion // Calculator

		#region IsChecked

		/// <summary>
		/// Identifies the <see cref="IsChecked"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
				"IsChecked",
				typeof( bool ),
				typeof( SummaryCalculatorHolder ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback( OnIsCheckedChanged ) )
			);

		/// <summary>
		/// Specifies whether the this calculator will be used to summarize field data.
		/// </summary>
		//[Description( "Specifies whether the this calculator will be used to summarize field data." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsChecked
		{
			get
			{
				return (bool)this.GetValue( IsCheckedProperty );
			}
			set
			{
				this.SetValue( IsCheckedProperty, value );
			}
		}

		private static void OnIsCheckedChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			SummaryCalculatorHolder holder = (SummaryCalculatorHolder)dependencyObject;
			bool newVal = (bool)e.NewValue;

			// If multiple summaries aren't allowed, deselect other summaries.
			// 
			if ( newVal )
			{
				SummaryCalculatorSelectionControl control = holder.SelectionControl;
				if ( null != control && ! control.AllowMultipleSummaries )
				{
					IEnumerable<SummaryCalculatorHolder> calculators = control.SummaryCalculatorHolders;
					if ( null != calculators )
					{
						foreach ( SummaryCalculatorHolder hh in calculators )
						{
							if ( hh != holder )
								hh.IsChecked = false;
						}
					}
				}
			}
		}

		#endregion // IsChecked

		#region SelectionControl

		/// <summary>
		/// Returns the associated summary calculator selection control.
		/// </summary>
		/// <seealso cref="SummaryCalculatorSelectionControl"/>
		public SummaryCalculatorSelectionControl SelectionControl
		{
			get
			{
				return _selectionControl;
			}
		}

		#endregion // SelectionControl
	}

	#endregion // SummaryCalculatorHolder

	#region SummaryCalculatorSelectionControl Class

	/// <summary>
	/// Control for displaying the user interface for selecting summary calculation type.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// Summary selection user interface consists of a summary icon on the field label which 
	/// when clicked upon displays a popup that contains an instance of this control.
	/// The control displays a list of available summary calculators that can be selected
	/// for summarizing the field data. It will allow multiple selection based on 
	/// <see cref="FieldSettings.SummaryUIType"/> property setting.
	/// </para>
	/// <para class="body">
	/// To actually show summary buttons inside field labels, set the FieldSettings's <see cref="FieldSettings.AllowSummaries"/>
	/// property.
	/// </para>
	/// </remarks>
	/// <seealso cref="FieldSettings.SummaryUIType"/>
	/// <seealso cref="FieldLayout.SummaryDefinitions"/>
	/// <seealso cref="RecordCollectionBase.SummaryResults"/>
	/// <seealso cref="DataPresenterBase.SummarySelectionControlOpening"/>
	/// <seealso cref="DataPresenterBase.SummarySelectionControlClosed"/>
	/// <seealso cref="DataPresenterBase.SummaryResultChanged"/>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryCalculatorSelectionControl : Control
	{
		#region NonePlaceHolder Class

		// SSP 3/31/09 TFS15634 
		// When summary ui type is single select, we need to display "None" radio option so the
		// user can unselect an existing summary. NonePlaceHolder is a dummy calculator 
		// implementation that represents that None entry.
		// 
		/// <summary>
		/// Represents "None" radio button entry in the selection control to allow the user to
		/// be able to unselect any existing summary if the summary ui type is single select.
		/// </summary>
		private class NonePlaceHolder : SummaryCalculator
		{
			public override string Description
			{
				get 
				{
					return DataPresenterBase.GetString("SummarySelectionControl_None_Entry_Tooltip");
				}
			}

			public override string DisplayName
			{
				get 
				{
					return DataPresenterBase.GetString("SummarySelectionControl_None_Entry_Text");
				}
			}

			public override string Name
			{
				get 
				{
					return "None";
				}
			}

			public override void Aggregate( object dataValue, SummaryResult summaryResult, Record record )
			{
			}

			public override void BeginCalculation( SummaryResult summaryResult )
			{
			}

			public override bool CanProcessDataType( Type dataType )
			{
				return false;
			}

			public override object EndCalculation( SummaryResult summaryResult )
			{
				return null;
			}
		}

		#endregion // NonePlaceHolder Class

		#region Private Vars

		// AS 6/2/09
		// We don't need to maintain this state. We just need it when 
		// updating the summaries.
		//
		//private bool _summariesChanged;

		#endregion // Private Vars

		#region Constructors

		/// <summary>
		/// Static constructor.
		/// </summary>
		static SummaryCalculatorSelectionControl( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryCalculatorSelectionControl ),
				new FrameworkPropertyMetadata( typeof( SummaryCalculatorSelectionControl ) ) );

            // JJD 3/4/09 - TFS5971/BR32606 
            UIElement.FocusableProperty.OverrideMetadata(typeof(SummaryCalculatorSelectionControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

            EventManager.RegisterClassHandler(typeof(SummaryCalculatorSelectionControl), Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnPreviousLostKeyboardFocus));
        }

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryCalculatorSelectionControl"/>.
		/// </summary>
		public SummaryCalculatorSelectionControl( )
		{
		}

		#endregion // Constructors

		#region Properties

		#region Public Properties

		#region AllowMultipleSummaries

		/// <summary>
		/// Identifies the <see cref="AllowMultipleSummaries"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMultipleSummariesProperty = DependencyProperty.Register(
				"AllowMultipleSummaries",
				typeof( bool ),
				typeof( SummaryCalculatorSelectionControl ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox )
			);

		/// <summary>
		/// Specifies whether the user is allowed to select multiple summaries or only a single summary for the field.
		/// </summary>
		//[Description( "Specifies whether the user is allowed to select multiple summaries for the field." )]
		//[Category( "Behaviorp" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool AllowMultipleSummaries
		{
			get
			{
				return (bool)this.GetValue( AllowMultipleSummariesProperty );
			}
			set
			{
				this.SetValue( AllowMultipleSummariesProperty, value );
			}
		}

		#endregion // AllowMultipleSummaries

		#region Field

		/// <summary>
		/// Identifies the <see cref="Field"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldProperty = DependencyProperty.Register(
  				"Field",
				typeof( Field ),
				typeof( SummaryCalculatorSelectionControl ),
				new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
					new PropertyChangedCallback( OnFieldChanged ) ) 
			);

		/// <summary>
		/// Field for which summaries are being selected.
		/// </summary>
		//[Description( "Field for which summaries are being selected." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Field Field
		{
			get
			{
				return (Field)this.GetValue( FieldProperty );
			}
			set
			{
				this.SetValue( FieldProperty, value );
			}
		}

		private static void OnFieldChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			SummaryCalculatorSelectionControl ctrl = (SummaryCalculatorSelectionControl)dependencyObject;
			ctrl.UpdateSettings( );
		}

		#endregion // Field

		#region SummaryCalculatorHolders

		/// <summary>
		/// Identifies the <see cref="SummaryCalculatorHolders"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryCalculatorHoldersProperty = DependencyProperty.Register(
				"SummaryCalculatorHolders",
				typeof( IEnumerable<SummaryCalculatorHolder> ),
				typeof( SummaryCalculatorSelectionControl ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Returns a list of summary calculators that are valie for the associated field, based on the field's
		/// data type.
		/// </summary>
		//[Description( "Summary calculators that are valid for the associated field." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IEnumerable<SummaryCalculatorHolder> SummaryCalculatorHolders
		{
			get
			{
				return (IEnumerable<SummaryCalculatorHolder>)this.GetValue( SummaryCalculatorHoldersProperty );
			}
			set
			{
				this.SetValue( SummaryCalculatorHoldersProperty, value );
			}
		}

		#endregion // SummaryCalculatorHolders

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private Methods

		#region GetApplicableSummaryCalculators

		private IEnumerable<SummaryCalculatorHolder> GetApplicableSummaryCalculators( Field field )
		{
			List<SummaryCalculatorHolder> list = new List<SummaryCalculatorHolder>( );

			if ( null == field )
				return list;

			FieldLayout fieldLayout = field.Owner;
			SummaryDefinitionCollection currentSummaries = null != fieldLayout ? fieldLayout.SummaryDefinitionsIfAllocated : null;

			// SSP 3/31/09 TFS15634
			bool anySummarySelected = false;

			foreach ( SummaryCalculator calculator in SummaryCalculator.RegisteredCalculators )
			{
				
				
				
				
				if ( calculator.CanProcessDataType( field.EditAsTypeResolved ) )
				{
					IEnumerable<SummaryDefinition> matchingSummaries = null == currentSummaries ? null
						: currentSummaries.GetMatchingSummaries( calculator, field );
					bool hasExistingSummary = GridUtilities.HasItems( matchingSummaries );

					// SSP 3/31/09 TFS15634
					anySummarySelected = anySummarySelected || hasExistingSummary;

					list.Add( new SummaryCalculatorHolder( this, calculator, hasExistingSummary ) );
				}
			}

			// SSP 3/31/09 TFS15634
			// We need to display "None" radio button so the user can unselect existing summary when
			// AllowMultipleSummaries is false.
			// 
			// --------------------------------------------------------------------------------------
			if ( ! this.AllowMultipleSummaries )
			{
				SummaryCalculatorHolder noneEntry = new SummaryCalculatorHolder( 
					this, new NonePlaceHolder( ), !anySummarySelected );
				list.Insert( 0, noneEntry );
			}
			// --------------------------------------------------------------------------------------

			return list;
		}

		#endregion // GetApplicableSummaryCalculators

		#region OnClosed

		internal void OnClosed( bool canceled )
		{
			this.OnOpenedClosedHelper( false, canceled );
		}

		#endregion // OnClosed

		#region OnOpenedClosedHelper

		private bool OnOpenedClosedHelper( bool opened, bool closeCanceled )
		{
			if ( opened )
				this.UpdateSettings( );

			// AS 6/2/09
			// We don't need to maintain this state. We just need to calculate it for the closed.
			//
			//// Reset the _summariesChanged flag every time the control is opened or closed.
			//// 
			//bool summariesChanged = _summariesChanged;
			//_summariesChanged = false;

			Field field = this.Field;
			FieldLayout fl = null != field ? field.Owner : null;
			DataPresenterBase dp = null != fl ? fl.DataPresenter : null;
			Debug.Assert( null != dp );
			if ( null != dp )
			{
				if ( opened )
				{
					SummarySelectionControlOpeningEventArgs args = new SummarySelectionControlOpeningEventArgs( field, this );
					dp.RaiseSummarySelectionControlOpening( args );
					if ( args.Cancel )
						return false;

					IEnumerable<SummaryCalculatorHolder> holders = this.SummaryCalculatorHolders;
					if ( null != holders )
					{
						foreach ( SummaryCalculatorHolder holder in holders )
							this.SyncHolderWithSummaries( field, holder );
					}

                    // JJD 3/4/09 - TFS5971/BR32606 
                    // Move the focus on the first focuable element in the control
                    DispatcherOperationCallback callback = delegate(object param)
                    {
                        if (this.IsVisible && Keyboard.FocusedElement == param)
                        {
                            this.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                         }

                        return null;
                    };
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Render, callback, Keyboard.FocusedElement);
                }
				else
				{
					// AS 6/2/09
					bool summariesChanged = false;

					if ( !closeCanceled )
					{
						IEnumerable<SummaryCalculatorHolder> holders = this.SummaryCalculatorHolders;
						if ( null != holders )
						{
							// AS 6/2/09 NA 2009.2 Undo/Redo
							// In order to handle the undo of this change, we need to keep a copy of the 
							// SummaryDefinitions list as they existed before we start adding/removing so 
							// we can readd the removed definitions in the original order.
							//
							IList<SummaryDefinition> oldDefinitionList = new List<SummaryDefinition>(fl.SummaryDefinitions);
							List<SummaryDefinition> definitionsRemoved = new List<SummaryDefinition>();
							List<SummaryDefinition> definitionsAdded = new List<SummaryDefinition>();

							foreach ( SummaryCalculatorHolder holder in holders )
							{
								// AS 6/2/09 NA 2009.2 Undo/Redo
								// Pass along the lists to update as items are added/removed.
								// Also we should track whether changes were made.
								//
								//// AS 6/2/09
								//// We need to maintain the dirty state for the closed args.
								////
								////this.SyncSummariesWithHolder( field, holder );
								//if (this.SyncSummariesWithHolder(field, holder))
								if (this.SyncSummariesWithHolder(field, holder, definitionsRemoved, definitionsAdded))
									summariesChanged = true;
							}

							// AS 6/2/09 NA 2009.2 Undo/Redo
							SummaryAction undoAction = SummaryAction.Create(field, oldDefinitionList, definitionsRemoved, definitionsAdded);

							if (null != undoAction && dp.IsUndoEnabled)
								dp.History.AddUndoActionInternal(undoAction);
						}
					}

					SummarySelectionControlClosedEventArgs args = new SummarySelectionControlClosedEventArgs( field, summariesChanged );
					dp.RaiseSummarySelectionControlClosed( args );
				}
			}

			return true;
		}

		#endregion // OnOpenedClosedHelper

		#region OnOpenning

		internal bool OnOpenning( )
		{
			return this.OnOpenedClosedHelper( true, false );
		}

		#endregion // OnOpenning

        // JJD 3/5/09 - TFS5971/BR32606 - added
        #region OnPreviousLostKeyboardFocus
        private static void OnPreviousLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            SummaryCalculatorSelectionControl summaryControl = sender as SummaryCalculatorSelectionControl;

            // when elements (like a menu item) call Keyboard.Focus(null) to try and put
            // focus back to the default element, they are causing focus to go to the main
            // browser window when in an xbap. really what should happen is that the last
            // focused element in that window should get focus. well in the case of an xbap,
            // this is the containing window and therefore we should take the focus and 
            // put it into our focused element.
            // AS 2/19/09 TFS13715
            // Do not manipulate the focus if it is another window that is getting focus.
            //
            //if (e.NewFocus is Window && VisualTreeHelper.GetParent((Window)e.NewFocus) == null)
            if (e.NewFocus is Window &&
                VisualTreeHelper.GetParent((Window)e.NewFocus) == null &&
                Window.GetWindow(summaryControl) == e.NewFocus)
            {
                e.Handled = true;
            }
        }
        #endregion //OnPreviousLostKeyboardFocus

		#region SyncHolderWithSummaries

		/// <summary>
		/// Sets IsChecked property of the holder based on whether there's currently a matching summary 
		/// in the field layout's summaries collection.
		/// </summary>
		/// <param name="field">The field for which summaries are being added or removed.</param>
		/// <param name="holder">This holder's IsChecked will be set.</param>
		internal void SyncHolderWithSummaries( Field field, SummaryCalculatorHolder holder )
		{
			FieldLayout fieldLayout = null != field ? field.Owner : null;
			bool isChecked = false;
			if ( null != fieldLayout )
			{
				SummaryDefinitionCollection summaries = fieldLayout.SummaryDefinitionsIfAllocated;
				SummaryDefinition existingSummary = null == summaries ? null
					: GridUtilities.GetFirstItem<SummaryDefinition>( summaries.GetMatchingSummaries( holder.Calculator, field ) );

				if ( null != existingSummary )
					isChecked = true;
			}

			holder.IsChecked = isChecked;
		}

		#endregion // SyncHolderWithSummaries

		#region SyncSummariesWithHolder

		/// <summary>
		/// Adds or removes summary in accordance with the checked state of the specified summary calculator holder.
		/// </summary>
		/// <param name="field">Field for which summaries are being added or removed.</param>
		/// <param name="holder">Calculator holder whose check state is used to determine whether to add or remove summary.</param>
		/// <param name="definitionsRemoved"></param>
		/// <param name="definitionsAdded"></param>
		/// <returns>Returns true summary was added or removed.</returns>
		// AS 6/2/09 NA 2009.2 Undo/Redo
		//private bool SyncSummariesWithHolder( Field field, SummaryCalculatorHolder holder )
		private bool SyncSummariesWithHolder(Field field, SummaryCalculatorHolder holder, List<SummaryDefinition> definitionsRemoved, List<SummaryDefinition> definitionsAdded)
		{
			FieldLayout fieldLayout = null != field ? field.Owner : null;
			Debug.Assert( null != fieldLayout );
			if ( null != fieldLayout )
			{
				SummaryDefinitionCollection summaries = fieldLayout.SummaryDefinitionsIfAllocated;
				SummaryDefinition existingSummary = null == summaries ? null
					: GridUtilities.GetFirstItem<SummaryDefinition>( summaries.GetMatchingSummaries( holder.Calculator, field ) );

				if ( holder.IsChecked )
				{
					// SSP 3/31/09 TFS15634
					// Added the if block and enclosed the existing code into the else block.
					// 
					if ( holder.Calculator is NonePlaceHolder )
					{
						if ( null != summaries )
						{
							// SSP 5/12/09 TFS17115
							// 
							// ------------------------------------------------------------------------------
							//summaries.Clear( );
							foreach ( SummaryDefinition ii in GridUtilities.ToArray<SummaryDefinition>( summaries.GetMatchingSummaries( null, field ) ) )
							{
								// AS 6/2/09 NA 2009.2 Undo/Redo
								definitionsRemoved.Add( ii );

								summaries.Remove( ii );
							}
							// ------------------------------------------------------------------------------
							
							return true;
						}
					}
					// Add a summary if it doesn't already exist.
					else if ( null == existingSummary )
					{
						// Cause summary collection to be allocated if not already.
						summaries = fieldLayout.SummaryDefinitions;

						SummaryDefinition definition = summaries.Add( holder.Calculator, field.Name );

						// AS 6/2/09 NA 2009.2 Undo/Redo
						definitionsAdded.Add( definition );

						return true;
					}
				}
				else
				{
					// Remove the existing summary if any.
					if ( null != existingSummary )
					{
						Debug.Assert( null != summaries );
						if ( null != summaries )
						{
							// AS 6/2/09 NA 2009.2 Undo/Redo
							definitionsRemoved.Add( existingSummary );

							summaries.Remove( existingSummary );
						}

						return true;
					}
				}
			}

			return false;
		}

		#endregion // SyncSummariesWithHolder

		#region UpdateSettings

		private void UpdateSettings( )
		{
			Field field = this.Field;

			SummaryUIType summaryUIType = null != field ? field.SummaryUITypeResolved : SummaryUIType.Default;
			this.AllowMultipleSummaries = SummaryUIType.MultiSelect == summaryUIType
				|| SummaryUIType.MultiSelectForNumericsOnly == summaryUIType;
			this.SummaryCalculatorHolders = this.GetApplicableSummaryCalculators( field );
		}

		#endregion // UpdateSettings

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // SummaryCalculatorSelectionControl Class

	#region SummaryCellPresenter Class

	/// <summary>
	/// Represents a summary cell inside a <see cref="SummaryRecordPresenter"/>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryCellPresenter</b> element displays one or more <see cref="SummaryResult"/>s by containing one or more 
	/// <see cref="SummaryResultPresenter"/> elements.
	/// </para>
	/// <para class="body">
	/// Each <i>SummaryCellPresenter</i> is associated with a single field inside a <see cref="SummaryRecordPresenter"/>. 
	/// A <i>SummaryCellPresenter</i> can contain one or more instances of <i>SummaryResultPresenter</i> elements. 
	/// This can happen if the field has more than one summary (for example the same field has <i>Sum</i> and 
	/// <i>Average</i> summaries, the associated <i>SummaryCellPresenter</i> for the field will contain two 
	/// <i>SummaryResultPresenter</i> elements).
	/// </para>
	/// </remarks>
	/// <seealso cref="SummaryResult"/>
	/// <seealso cref="SummaryResultPresenter"/>
	/// <seealso cref="SummaryRecordPresenter"/>
	//[ToolboxItem( false )]
    // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
    // I needed a common base class for the CellPresenter and SummaryCellPresenter
    //
    //public class SummaryCellPresenter : Control
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryCellPresenter : CellPresenterBase
	{
		#region Private Vars

		private Field _cachedField;

		#endregion // Private Vars

		#region Constructor

		static SummaryCellPresenter( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryCellPresenter ), new FrameworkPropertyMetadata( typeof( SummaryCellPresenter ) ) );

			// AS 8/24/09 TFS19532
			UIElement.VisibilityProperty.OverrideMetadata(typeof(SummaryCellPresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(GridUtilities.CoerceFieldElementVisibility)));
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryCellPresenter"/>.
		/// </summary>
		public SummaryCellPresenter( )
		{
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Field

		/// <summary>
		/// Identifies the 'Field' dependency property
		/// </summary>
		public static readonly DependencyProperty FieldProperty = DataItemPresenter.FieldProperty.AddOwner(
			typeof( SummaryCellPresenter ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnFieldChanged ) )
		);

		private static void OnFieldChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			SummaryCellPresenter cellPresenter = (SummaryCellPresenter)dependencyObject;
			Field newVal = (Field)e.NewValue;

			cellPresenter._cachedField = newVal;

			// AS 8/24/09 TFS19532
			// See GridUtilities.CoerceFieldElementVisibility for details.
			//
			if (newVal != null)
			{
				// JJD 3/9/11 - TFS67970 - Optimization - use the cached binding
				//cellPresenter.SetBinding(GridUtilities.FieldVisibilityProperty, Utilities.CreateBindingObject("VisibilityResolved", BindingMode.OneWay, newVal));
				cellPresenter.SetBinding(GridUtilities.FieldVisibilityProperty, newVal.VisibilityBinding);
			}
			else
				BindingOperations.ClearBinding(cellPresenter, GridUtilities.FieldVisibilityProperty);

            // AS 1/26/09 TFS13026
            cellPresenter.InvalidateLayoutElement();
        }

		/// <summary>
		/// The associated field.
		/// </summary>
        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
		//public Field Field
		public override Field Field
		{
			get
			{
				return this._cachedField;
			}
			set
			{
				this.SetValue( CellPresenter.FieldProperty, value );
			}
		}

		#endregion //Field

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // SummaryCellPresenter Class

	#region SummaryRecordCellArea Class

	/// <summary>
	/// Element inside <see cref="SummaryRecordContentArea"/> that contains summaries that are aligned with fields.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryRecordCellArea</b> element is positioned inside a <see cref="SummaryRecordContentArea"/>.
	/// It contains summaries that are aligned with fields.
	/// </para>
	/// </remarks>
	/// <seealso cref="SummaryRecordPresenter"/>
	/// <seealso cref="SummaryRecordContentArea"/>
	/// <seealso cref="SummaryResultsPresenter"/>
	/// <seealso cref="SummaryResultPresenter"/>
	//[ToolboxItem( false )]
    // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
    //public class SummaryRecordCellArea : ContentControl
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryRecordCellArea : RecordCellAreaBase
	{
		#region Private Vars

		#endregion // Private Vars

		#region Constructor

		static SummaryRecordCellArea( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryRecordCellArea ), new FrameworkPropertyMetadata( typeof( SummaryRecordCellArea ) ) );
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( SummaryRecordCellArea ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryRecordCellArea"/>.
		/// </summary>
		public SummaryRecordCellArea( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		/// <summary>
		/// Overridden. Called when a property is changed.
		/// </summary>
		protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnPropertyChanged( e );

			if ( DataContextProperty == e.Property )
				this.UpdateContentAndTemplate( );
		}

		#endregion // Base Overrides

		#region Methods

		#region Private Methods

		private void UpdateContentAndTemplate( )
		{
			SummaryRecord summaryRecord = this.DataContext as SummaryRecord;

			DataTemplate contentTemplate = null;
			if ( null != summaryRecord )
			{
                
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				contentTemplate = summaryRecord.FieldLayout.StyleGenerator.GeneratedSummaryVirtualCellAreaTemplate;
			}

			this.Content = summaryRecord;
			this.ContentTemplate = contentTemplate;			
		}

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // SummaryRecordCellArea Class

	#region SummaryRecordContentArea Class

	/// <summary>
	/// Element inside <see cref="SummaryRecordPresenter"/> that contains <see cref="SummaryRecordHeaderPresenter"/>,
	/// <see cref="SummaryRecordCellArea"/> and also free-form summaries.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryRecordContentArea</b> element is part of <see cref="SummaryRecordPresenter"/> element. It contains
	/// <see cref="SummaryRecordHeaderPresenter"/>, <see cref="SummaryRecordCellArea"/> and also free-form summaries.
	/// Free-form summaries are those summaries with their <see cref="SummaryDefinition.Position"/> property 
	/// set to one of Left, Center or Right.
	/// </para>
	/// </remarks>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryRecordContentArea : Control
	{
		#region Private Vars

		private PropertyValueTracker _summaryDefinitionsTracker;

		// SSP 4/10/12 TFS108549 - Optimizations
		// 
		private int _lastSummaryDefinitionCollection_Version;

		#endregion // Private Vars

		#region Constructor

		static SummaryRecordContentArea( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryRecordContentArea ), new FrameworkPropertyMetadata( typeof( SummaryRecordContentArea ) ) );
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( SummaryRecordContentArea ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );

			// AS 11/8/11 TFS88111 - Added Stretch HorizontalContentAlignment
			Control.HorizontalContentAlignmentProperty.OverrideMetadata( typeof( SummaryRecordContentArea ), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox ) );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryRecordContentArea"/>.
		/// </summary>
		public SummaryRecordContentArea( )
		{
			// Hook to get notified for changes in summary definitions.
			// 
			_summaryDefinitionsTracker = new PropertyValueTracker( this,
				"SummaryRecord.FieldLayout.SummaryDefinitions.SummariesVersion",
				this.OnSummariesChanged,
				// MD 8/17/10
				// We should be handling the dirtying of this version asynchronously because 
				// it may happen multiple times in the a row.
				true);
		}

		#endregion // Constructor

		#region Base Overrides

		#region OnPropertyChanged

		/// <summary>
		/// Overridden. Called when a property is changed.
		/// </summary>
		protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnPropertyChanged( e );

			DependencyProperty prop = e.Property;
			if ( DataContextProperty == prop )
			{
				this.SetValue( SummaryRecordPropertyKey, e.NewValue as SummaryRecord );

				// JJD 2/9/11 - TFS58961
				// If we are in a context, e.g. a report that doesn't support async operatines
				// then we need to call OnSummariesChanged so the HasSummariesInCellArea
				// gets synchronously initialized
				// SSP 6/20/11 TFS78929
				// Always do this so the HasSummariesInCellAreaProperty gets initialized to the correct value
				// synchronously. Prior to this change it does get initialized however asynchronously which
				// causes TFS78929. Commented out the condition so OnSummariesChanged always gets called.
				// 
				//if (!Utilities.AllowsAsyncOperations(this))
					this.OnSummariesChanged();
			}
		}

		#endregion // OnPropertyChanged

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region SummaryRecord

		private static readonly DependencyPropertyKey SummaryRecordPropertyKey = DependencyProperty.RegisterReadOnly(
				"SummaryRecord",
				typeof( SummaryRecord ),
				typeof( SummaryRecordContentArea ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="SummaryRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryRecordProperty = SummaryRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the associated <seealso cref="SummaryRecord"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property returns the associated <seealso cref="SummaryRecord"/> that this SummaryRecordContentArea is displaying the contents of.
		/// </para>
		/// </remarks>
		/// <seealso cref="SummaryRecord"/>
		/// <seealso cref="SummaryRecordPresenter"/>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="SummaryRecordPresenter"/>
		/// <seealso cref="SummaryCellPresenter"/>
		//[Description( "Associated SummaryRecord object." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public SummaryRecord SummaryRecord
		{
			get
			{
				return (SummaryRecord)this.GetValue( SummaryRecordProperty );
			}
		}

		#endregion // SummaryRecord

		#region HasSummariesInCellArea

		// SSP 1/6/10 TFS25633
		// Added HasSummariesInCellArea. We should not display the summary record cell area
		// if there are no summaries displayed inside it - that is all the visible summaries are
		// free-form (left, center and right) summaries.
		// 

		/// <summary>
		/// Identifies the <see cref="HasSummariesInCellArea"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasSummariesInCellAreaProperty = DependencyProperty.Register(
			"HasSummariesInCellArea",
			typeof( bool ),
			typeof( SummaryRecordContentArea ),
			new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// Returns a value indivating whether the cell area element has any summaries.
		/// </summary>
		//[Description( "Indicates whether the cell are element has any summaries." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool HasSummariesInCellArea
		{
			get
			{
				return (bool)this.GetValue( HasSummariesInCellAreaProperty );
			}
			set
			{
				this.SetValue( HasSummariesInCellAreaProperty, value );
			}
		}

		#endregion // HasSummariesInCellArea

		#region SummaryRecordHeaderVisibility

		/// <summary>
		/// Identifies the <see cref="SummaryRecordHeaderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryRecordHeaderVisibilityProperty = DependencyProperty.Register(
				"SummaryRecordHeaderVisibility",
				typeof( Visibility ),
				typeof( SummaryRecordContentArea ),
				new FrameworkPropertyMetadata( Visibility.Collapsed )
			);

		/// <summary>
		/// Returns a value indivating whether the summary record's header should be visible.
		/// </summary>
		//[Description( "Indicates whether the summary record header should be visible." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Visibility SummaryRecordHeaderVisibility
		{
			get
			{
				return (Visibility)this.GetValue( SummaryRecordHeaderVisibilityProperty );
			}
			set
			{
				this.SetValue( SummaryRecordHeaderVisibilityProperty, value );
			}
		}

		#endregion // SummaryRecordHeaderVisibility

		#region SummaryResultsCenter

		private static readonly DependencyPropertyKey SummaryResultsCenterPropertyKey = DependencyProperty.RegisterReadOnly(
				"SummaryResultsCenter",
				typeof( IEnumerable<SummaryResult> ),
				typeof( SummaryRecordContentArea ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="SummaryResultsCenter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryResultsCenterProperty = SummaryResultsCenterPropertyKey.DependencyProperty;

		/// <summary>
		/// Summary results that will be displayed in the center of the summary record content area.
		/// </summary>
		//[Description( "Summary results that will be displayed in the center of the summary record content area." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IEnumerable<SummaryResult> SummaryResultsCenter
		{
			get
			{
				return (IEnumerable<SummaryResult>)this.GetValue( SummaryResultsCenterProperty );
			}
		}

		#endregion // SummaryResultsCenter

		#region SummaryResultsLeft

		private static readonly DependencyPropertyKey SummaryResultsLeftPropertyKey = DependencyProperty.RegisterReadOnly(
				"SummaryResultsLeft",
				typeof( IEnumerable<SummaryResult> ),
				typeof( SummaryRecordContentArea ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="SummaryResultsLeft"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryResultsLeftProperty = SummaryResultsLeftPropertyKey.DependencyProperty;

		/// <summary>
		/// Summary results that will be displayed in the left of the summary record content area.
		/// </summary>
		//[Description( "Summary results that will be displayed in the center of the summary record content area." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IEnumerable<SummaryResult> SummaryResultsLeft
		{
			get
			{
				return (IEnumerable<SummaryResult>)this.GetValue( SummaryResultsLeftProperty );
			}
		}

		#endregion // SummaryResultsLeft

		#region SummaryResultsRight

		private static readonly DependencyPropertyKey SummaryResultsRightPropertyKey = DependencyProperty.RegisterReadOnly(
				"SummaryResultsRight",
				typeof( IEnumerable<SummaryResult> ),
				typeof( SummaryRecordContentArea ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="SummaryResultsRight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryResultsRightProperty = SummaryResultsRightPropertyKey.DependencyProperty;

		/// <summary>
		/// Summary results that will be displayed in the right of the summary record content area.
		/// </summary>
		//[Description( "Summary results that will be displayed in the center of the summary record content area." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IEnumerable<SummaryResult> SummaryResultsRight
		{
			get
			{
				return (IEnumerable<SummaryResult>)this.GetValue( SummaryResultsRightProperty );
			}
		}

		#endregion // SummaryResultsRight

		#endregion // Public Properties

		#region Private/Internal Properties

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region OnSummariesChanged

		private void OnSummariesChanged( )
		{
			SummaryRecord record = this.SummaryRecord;
			if ( null != record )
			{
				record.VerifySummaryResults( );

				this.SetValue( SummaryResultsLeftPropertyKey, record.GetFreeFormSummaryResults( SummaryPosition.Left ) );
				this.SetValue( SummaryResultsCenterPropertyKey, record.GetFreeFormSummaryResults( SummaryPosition.Center ) );
				this.SetValue( SummaryResultsRightPropertyKey, record.GetFreeFormSummaryResults( SummaryPosition.Right ) );
				this.SetValue( SummaryRecordHeaderVisibilityProperty, record.SummaryRecordHeaderVisibilityResolved );

				// SSP 1/6/10 TFS25633
				// See comment above SummaryRecordCellAreaVisibilityProperty definition.
				// 
				this.SetValue( HasSummariesInCellAreaProperty, record.HasFixedSummaryResults( ) );

				// Clear size to content manager so height gets recalculated since summaries have changed.
				record.ClearSizeToContentManager( );
			}
			else
			{
				this.ClearValue( SummaryResultsLeftPropertyKey );
				this.ClearValue( SummaryResultsCenterPropertyKey );
				this.ClearValue( SummaryResultsRightPropertyKey );
				this.ClearValue( SummaryRecordHeaderVisibilityProperty );

				// SSP 1/6/10 TFS25633
				// See comment above SummaryRecordCellAreaVisibilityProperty definition.
				// 
				this.ClearValue( HasSummariesInCellAreaProperty );
			}

			// SSP 4/10/12 TFS108549 - Optimizations
			// Enclosed the existing code into the if block.
			// 
			object versionObj = _summaryDefinitionsTracker.Target;
			int version = versionObj is int ? (int)versionObj : -1;
			if ( _lastSummaryDefinitionCollection_Version != version )
			{
				_lastSummaryDefinitionCollection_Version = version;

				VirtualizingSummaryCellPanel cellPanel = (VirtualizingSummaryCellPanel)Utilities.GetDescendantFromType(
					this, typeof( VirtualizingSummaryCellPanel ), true );
				if ( null != cellPanel )
					cellPanel.DirtyCellElements( true );
			}
		}

		#endregion // OnSummariesChanged

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // SummaryRecordContentArea Class

	#region SummaryRecordHeaderPresenter Class

	/// <summary>
	/// Element that represents the header of a <see cref="SummaryRecord"/>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// If a <see cref="SummaryRecord"/> has a header (set via <see cref="FieldLayout.SummaryDescriptionMask"/>),
	/// the associated <see cref="SummaryRecordPresenter"/> will contain a <b>SummaryRecordHeaderPresenter</b> 
	/// in its top area to display this header.
	/// </para>
	/// </remarks>
	/// <seealso cref="FieldLayout.SummaryDescriptionMask"/>
	/// <seealso cref="FieldLayout.SummaryDescriptionMaskInGroupBy"/>
	/// <seealso cref="SummaryResultCollection.SummaryRecordHeader"/>
	/// <seealso cref="SummaryRecordPresenter"/>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryRecordHeaderPresenter : Control
	{
		#region Constructor

		static SummaryRecordHeaderPresenter( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryRecordHeaderPresenter ), new FrameworkPropertyMetadata( typeof( SummaryRecordHeaderPresenter ) ) );
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( SummaryRecordHeaderPresenter ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryRecordHeaderPresenter"/>.
		/// </summary>
		public SummaryRecordHeaderPresenter( )
		{
		}

		#endregion // Constructor

		#region SummaryRecord

		/// <summary>
		/// Identifies the <see cref="SummaryRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryRecordProperty = DependencyProperty.Register(
				"SummaryRecord",
				typeof( SummaryRecord ),
				typeof( SummaryRecordHeaderPresenter ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Gets the associated summary record object.
		/// </summary>
		//[Description( "Associated summary record object." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public SummaryRecord SummaryRecord
		{
			get
			{
				return (SummaryRecord)this.GetValue( SummaryRecordProperty );
			}
			set
			{
				this.SetValue( SummaryRecordProperty, value );
			}
		}

		#endregion // SummaryRecord
	}

	#endregion // SummaryRecordHeaderPresenter Class

	#region SummaryRecordPrefixArea Class

	/// <summary>
	/// A control that is placed in the summary record and sized to match the dimensions of the RecordSelectors so that the summary results line up with the cells in data records.
	/// </summary>
	//[Description( "A control that is placed in the summary record and sized to match the dimensions of the RecordSelectors so that the summary results line up with the cells in data records." )]
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryRecordPrefixArea : RecordPrefixArea
	{
		#region Constructors

		static SummaryRecordPrefixArea( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryRecordPrefixArea ), new FrameworkPropertyMetadata( typeof( SummaryRecordPrefixArea ) ) );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryRecordPrefixArea"/> class
		/// </summary>
		public SummaryRecordPrefixArea( )
		{
		}

		#endregion Constructors

		#region Base class overrides

		#region CreateStyleSelectorHelper

		
		
		/// <summary>
		/// Creates an instance of StyleSelectorHelperBase derived class for the element.
		/// </summary>
		/// <returns>The created style selector helper.</returns>
		internal override StyleSelectorHelperBase CreateStyleSelectorHelper( )
		{
			return new StyleSelectorHelper( this );
		}

		#endregion // CreateStyleSelectorHelper

		#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString( )
		{
			StringBuilder sb = new StringBuilder( );

			sb.Append( "SummaryRecordPrefixArea: " );

			Record record = this.Record;
			if ( null != record )
				sb.Append( record.ToString( ) );

			return sb.ToString( );
		}

		#endregion //ToString

		#endregion //Base class overrides

		#region StyleSelectorHelper private class

		private class StyleSelectorHelper : StyleSelectorHelperBase
		{
			private SummaryRecordPrefixArea _rs;

			internal StyleSelectorHelper( SummaryRecordPrefixArea rs )
				: base( rs )
			{
				this._rs = rs;
			}

			/// <summary>
			/// The style to be used as the source of a binding (read-only)
			/// </summary>
			public override Style Style
			{
				get
				{
					if ( this._rs == null )
						return null;

					FieldLayout fl = this._rs.FieldLayout;

					if ( fl != null )
					{
						DataPresenterBase dp = fl.DataPresenter;

						if ( dp != null )
							
							//return dp.InternalSummaryRecordPrefixAreaStyleSelector.SelectStyle( this._rs.DataContext, this._rs );
							return null;
					}

					return null;
				}
			}
		}

		#endregion // StyleSelectorHelper private class
	}

	#endregion // SummaryRecordPrefixArea Class

	#region SummaryRecordPresenter Class

	/// <summary>
	/// Element that represents a <see cref="SummaryRecord"/>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryRecordPresenter</b> represents a <see cref="SummaryRecord"/>. It contains one or more
	/// <see cref="SummaryCellPresenter"/> elements for each field that has summaries that need to be displayed
	/// in the summary record. Each <i>SummaryCellPresenter</i> in turn can contain one or more 
	/// <see cref="SummaryResultPresenter"/> to represent summaries associated with the field.
	/// </para>
	/// </remarks>
	/// <seealso cref="SummaryRecord"/>
	/// <seealso cref="SummaryCellPresenter"/>
	/// <seealso cref="SummaryResultPresenter"/>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryRecordPresenter : RecordPresenter
	{

#region Infragistics Source Cleanup (Region)




































































































































































































































#endregion // Infragistics Source Cleanup (Region)

		#region Member Vars

        // JJD 1/19/09 - NA 2009 vol 1 - no longer needed
        //private ContentAreaMarginsManager _contentAreaMarginsManager;
		//private GridUtilities.PropertyValueTracker _internalVersionTracker;

		
		
		private SummaryRecord _cachedSummaryRecord;

		#endregion // Member Vars

		#region Constructor

		static SummaryRecordPresenter( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryRecordPresenter ), new FrameworkPropertyMetadata( typeof( SummaryRecordPresenter ) ) );
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( SummaryRecordPresenter ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryRecordPresenter"/>.
		/// </summary>
		public SummaryRecordPresenter( )
		{
            
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion // Constructor

		#region Base Overrides

		#region GetDefaultTemplateProperty

		private static bool s_DefaultTemplatesCached = false;
		private static ControlTemplate s_DefaultTemplate = null;
		private static ControlTemplate s_DefaultTemplateTabluar = null;
		private static ControlTemplate s_DefaultTemplateCardView = null;

		internal override ControlTemplate GetDefaultTemplateProperty( DependencyProperty templateProperty )
		{
			if ( s_DefaultTemplatesCached == false )
			{
				lock ( typeof( SummaryRecordPresenter ) )
				{
					if ( s_DefaultTemplatesCached == false )
					{
						s_DefaultTemplatesCached = true;

						Style style = Infragistics.Windows.Themes.DataPresenterGeneric.SummaryRecordPresenter;
						Debug.Assert( style != null );
						if ( style != null )
						{
							s_DefaultTemplate = Utilities.GetPropertyValueFromStyle( style, TemplateProperty, true, false ) as ControlTemplate;
							s_DefaultTemplateTabluar = Utilities.GetPropertyValueFromStyle( style, TemplateGridViewProperty, true, false ) as ControlTemplate;
							s_DefaultTemplateCardView = Utilities.GetPropertyValueFromStyle( style, TemplateCardViewProperty, true, false ) as ControlTemplate;
						}
					}
				}
			}

			if ( templateProperty == TemplateProperty )
				return s_DefaultTemplate;

			if ( templateProperty == TemplateGridViewProperty )
				return s_DefaultTemplateTabluar;

			if ( templateProperty == TemplateCardViewProperty )
				return s_DefaultTemplateCardView;

			Debug.Fail( "What are WeakEventManager doing here" );
			return null;
		}

		#endregion //GetDefaultTemplateProperty

		#region GetRecordContentAreaTemplate

		// SSP 4/7/08 - Summaries Functionality
		// Added GetRecordContentAreaTemplate method to RecordPresenter and overrode it here.
		// 
		/// <summary>
		/// Gets the record content area template from the generator. For summary presenter, this returns
		/// a template that creates summary cell area.
		/// </summary>
		/// <param name="generator">Field layout generator from which to get the template.</param>
		/// <returns>The data template</returns>
		internal override DataTemplate GetRecordContentAreaTemplate( FieldLayoutTemplateGenerator generator )
		{
			return generator.GeneratedSummaryRecordContentAreaTemplate;
		}

		#endregion // GetRecordContentAreaTemplate

		#region MeasureOverride

#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

		#endregion // MeasureOverride

        // JJD 1/19/09 - NA 2009 vol 1 
        #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // JJD 1/19/09 - NA 2009 vol 1 
            // Initialize content area margin bindings
            SummaryRecordPresenter.InitializeContentAreaMarginBinding(this, this.Record, this.GetType());
        }

        #endregion //OnApplyTemplate

		#region OnRecordChanged

		
		
		
		/// <summary>
		/// Called when Record property's value has changed.
		/// </summary>
		/// <param name="oldRecord">Old record if any.</param>
		/// <param name="newRecord">New record if any.</param>
		protected override void OnRecordChanged( Record oldRecord, Record newRecord )
		{
			base.OnRecordChanged( oldRecord, newRecord );

			
			
			
			_cachedSummaryRecord = newRecord as SummaryRecord;

			this.SetValue( SummaryRecordPropertyKey, newRecord );

            // JJD 1/19/09 - NA 2009 vol 1 - no longer needed
            //_contentAreaMarginsManager.InitRecord( newRecord );

            // JJD 1/19/09 - NA 2009 vol 1 
            // Initialize content area margin bindings
            SummaryRecordPresenter.InitializeContentAreaMarginBinding(this, this.Record, this.GetType());
		}

		#endregion // OnRecordChanged

		#region UpdateAutoFitProperties

		internal override void UpdateAutoFitProperties( )
		{
			// JJD 3/27/07
			// Get out if we aren't arranged in view
			if ( !this.IsArrangedInView )
			{
				DataRecordPresenter.ClearAutoFitCellAreaProperties( this );
				return;
			}

			// JJD 3/27/07
			// call the base implementation
			base.UpdateAutoFitProperties( );

			DataRecordPresenter.UpdateAutoFitPropertiesHelper( this );
		}

		#endregion // UpdateAutoFitProperties

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region ContentAreaMargins

		/// <summary>
		/// Identifies the <see cref="ContentAreaMargins"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentAreaMarginsProperty = DependencyProperty.Register(
				"ContentAreaMargins",
				typeof( Thickness ),
				typeof( SummaryRecordPresenter ),
                // JJD 1/20/09 - NA 2009 vol 1 - added AffectsMeasure option
				new FrameworkPropertyMetadata( new Thickness( ), FrameworkPropertyMetadataOptions.AffectsMeasure )
			);

		/// <summary>
		/// Returns the margins for the contents inside the element. This is the amount
		/// the summary cells need to be offset by to align them with the field labels.
		/// </summary>
		//[Description( "Returns the margins for the contents inside the element." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Thickness ContentAreaMargins
		{
			get
			{
				return (Thickness)this.GetValue( ContentAreaMarginsProperty );
			}
			set
			{
				this.SetValue( ContentAreaMarginsProperty, value );
			}
		}

		#endregion // ContentAreaMargins

		#region HeaderAreaBackground

		/// <summary>
		/// Identifies the <see cref="HeaderAreaBackground"/> dependency property
		/// </summary>		
		public static readonly DependencyProperty HeaderAreaBackgroundProperty = 
			DataRecordPresenter.HeaderAreaBackgroundProperty.AddOwner( typeof( SummaryRecordPresenter ) );

		/// <summary>
		/// The brush applied by default templates as the background in the HeaderArea. This is the area behind the LabelPresenters.
		/// </summary>
		/// <seealso cref="HeaderAreaBackgroundProperty"/>	
		//[Description("The brush applied by default templates as the background in the HeaderArea. This is the area behind the LabelPresenters ")]
		//[Category("Brushes")]
		public Brush HeaderAreaBackground
		{
			get
			{
				return (Brush)this.GetValue( DataRecordPresenter.HeaderAreaBackgroundProperty );
			}
			set
			{
				this.SetValue( DataRecordPresenter.HeaderAreaBackgroundProperty, value );
			}
		}

		#endregion HeaderAreaBackground	

		#region SummaryRecord

		private static readonly DependencyPropertyKey SummaryRecordPropertyKey = DependencyProperty.RegisterReadOnly(
			"SummaryRecord",
			typeof( SummaryRecord ),
			typeof( SummaryRecordPresenter ),
			new FrameworkPropertyMetadata( null )
		);

		/// <summary>
		/// Identifies the Read-Only <see cref="SummaryRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryRecordProperty = SummaryRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the associated <seealso cref="SummaryRecord"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property returns the associated <seealso cref="SummaryRecord"/> that this SummaryRecordPresenter is displaying.
		/// </para>
		/// </remarks>
		/// <seealso cref="SummaryRecord"/>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="SummaryRecordPresenter"/>
		/// <seealso cref="SummaryCellPresenter"/>
		//[Description( "Associated SummaryRecord object." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public SummaryRecord SummaryRecord
		{
			get
			{
				
				
				
				return _cachedSummaryRecord;
			}
		}

		#endregion // SummaryRecord

		#endregion // Public Properties

		#region Private/Internal Properties

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

        // JJD 1/19/09 - NA 2009 vol 1 
        #region InitializeContentAreaMarginBinding

        internal static void InitializeContentAreaMarginBinding(Control control, Record record, Type targetType)
        {
            if (record == null)
                return;

            ValueSource vs = DependencyPropertyHelper.GetValueSource(control, SummaryRecordPresenter.ContentAreaMarginsProperty);

            if (vs.BaseValueSource < BaseValueSource.Style)
                control.SetBinding(SummaryRecordPresenter.ContentAreaMarginsProperty, GridUtilities.CreateRecordContentMarginBinding(record.FieldLayout, false, targetType));
        }

        #endregion //InitializeContentAreaMarginBinding	
    
		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // SummaryRecordPresenter Class

	#region SummaryResultEntry Class

	// SSP 4/10/12 TFS108549 - Optimizations
	// 
	/// <summary>
	/// Used by the <see cref="SummaryResultsPresenter.SummaryResultEntries"/> property.
	/// </summary>
	public class SummaryResultEntry : PropertyChangeNotifier
	{
		private SummaryResult _summaryResult;

		/// <summary>
		/// Gets or sets the associated <see cref="SummaryResult"/> object.
		/// </summary>
		public SummaryResult SummaryResult
		{
			get
			{
				return _summaryResult;
			}
			set
			{
				if ( _summaryResult != value )
				{
					_summaryResult = value;
					this.RaisePropertyChangedEvent( "SummaryResult" );
				}
			}
		}
	} 

	#endregion // SummaryResultEntry Class

	#region SummaryResultPresenter Class

	/// <summary>
	/// Represents a summary result inside a <see cref="SummaryCellPresenter"/>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryResultPresenter</b> element displays the value of a <see cref="SummaryResult"/>.
	/// </para>
	/// <para class="body">
	/// One or more instances of <i>SummaryResultPresenter</i> will be contained inside <see cref="SummaryCellPresenter"/>
	/// element. Each <i>SummaryCellPresenter</i> is associated with a single field inside a <see cref="SummaryRecordPresenter"/>. 
	/// Each <i>SummaryResultPresenter</i> is associated with a single instance of a <i>SummaryResult</i>. A 
	/// <i>SummaryCellPresenter</i> can contain more than one <i>SummaryResultPresenter</i> elements if the field has more 
	/// than one summary (for example the same field has <i>Sum</i> and <i>Average</i> summaries, the associated 
	/// <i>SummaryCellPresenter</i> for the field will contain two <i>SummaryResultPresenter</i> elements).
	/// </para>
	/// </remarks>
	/// <seealso cref="SummaryResult"/>
	/// <seealso cref="SummaryCellPresenter"/>
	/// <seealso cref="SummaryRecordPresenter"/>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryResultPresenter : Control
	{
		#region Private Vars

		private PropertyValueTracker _summaryDataVersionTracker;

		// SSP 4/10/12 TFS108549 - Optimizations
		// 
		internal const int THROTTLE_TIME = 250;

		// SSP 4/13/12 TFS108549 - Optimizations
		// Skip summary re-calculation for summary record presenter that's de-activated or while scrolling.
		// 
		private bool _summaryRecalcBypassed_recordDeactivated;
		private bool _summaryRecalcBypassed_scrolling;
		private PropertyValueTracker _summaryRecalcBypassed_scrolling_pvt;
		private SummaryResult _cachedSummaryResult;

		#endregion // Private Vars

		#region Constructor

		static SummaryResultPresenter( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryResultPresenter ), new FrameworkPropertyMetadata( typeof( SummaryResultPresenter ) ) );
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( SummaryResultPresenter ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryResultPresenter"/>.
		/// </summary>
		public SummaryResultPresenter( )
		{
			this.EnsureSummaryDataVersionTracker( );
		}

		#endregion // Constructor

		#region Base Overrides

		#region OnCreateAutomationPeer

		// SSP 10/16/09 TFS19535
		// 
		/// <summary>
		/// Overridden. Creates a new instance of <see cref="SummaryResultPresenterAutomationPeer"/> for
		/// this SummaryResultPresenter and returns it.
		/// </summary>
		/// <returns>A new instance of <see cref="SummaryResultPresenterAutomationPeer"/>.</returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer( )
		{
			return new SummaryResultPresenterAutomationPeer( this );
		}

		#endregion // OnCreateAutomationPeer 

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region IsCalculationPending

		private static readonly DependencyPropertyKey IsCalculationPendingPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"IsCalculationPending",
			typeof( bool ),
			typeof( SummaryResultPresenter ),
			KnownBoxes.FalseBox,
			null
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsCalculationPending"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsCalculationPendingProperty = IsCalculationPendingPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates whether summary calculation is pending.
		/// </summary>
		/// <seealso cref="IsCalculationPendingProperty"/>
		public bool IsCalculationPending
		{
			get
			{
				return (bool)this.GetValue( IsCalculationPendingProperty );
			}
			internal set
			{
				this.SetValue( IsCalculationPendingPropertyKey, value );
			}
		}

		#endregion // IsCalculationPending

		#region SummaryResult

		/// <summary>
		/// Identifies the <see cref="SummaryResult"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryResultProperty = DependencyProperty.Register(
				"SummaryResult",
				typeof( SummaryResult ),
				typeof( SummaryResultPresenter ),
				// SSP 4/18/12 TFS108549 - Optimizations
				// 
				//new FrameworkPropertyMetadata( null )
				new FrameworkPropertyMetadata( new PropertyChangedCallback( OnSummaryResultChanged ) )
			);

		// SSP 4/18/12 TFS108549 - Optimizations
		// 
		private static void OnSummaryResultChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
		{
			SummaryResultPresenter item = (SummaryResultPresenter)obj;
			item._cachedSummaryResult = (SummaryResult)e.NewValue;
			item.VerifyIsCalculationPending( );
		}

		/// <summary>
		/// Gets or sets the associated SummaryResult object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property returns the associated <see cref="SummaryResult"/> object.
		/// </para>
		/// </remarks>
		/// <seealso cref="SummaryResult"/>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="SummaryRecordPresenter"/>
		/// <seealso cref="SummaryCellPresenter"/>
		//[Description( "Associated SummaryResult object" )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public SummaryResult SummaryResult
		{
			get
			{
				// SSP 4/18/12 TFS108549 - Optimizations
				// 
				//return (SummaryResult)this.GetValue( SummaryResultProperty );
				return _cachedSummaryResult;
			}
			set
			{
				this.SetValue( SummaryResultProperty, value );
			}
		}

		#endregion // SummaryResult

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region ClearSummaryDataVersionTracker

		// SSP 4/13/12 TFS108549 - Optimizations
		// 
		private void ClearSummaryDataVersionTracker( )
		{
			if ( null != _summaryDataVersionTracker )
			{
				_summaryDataVersionTracker.Deactivate( );
				_summaryDataVersionTracker = null;
			}
		} 

		#endregion // ClearSummaryDataVersionTracker

		#region EnsureSummaryDataVersionTracker

		// SSP 4/13/12 TFS108549 - Optimizations
		// Refactored. Moved the logic for creating the summary data version tracker into a separate method.
		// 
		private void EnsureSummaryDataVersionTracker( )
		{
			if ( null == _summaryDataVersionTracker )
			{
				// SSP 8/2/09 - Summary Recalc Optimizations
				// Use the new DataVersion of the SummaryResult instead of the CalculationVersion of the
				// SummaryDefinition.
				// 
				//_summaryDataVersionTracker = new PropertyValueTracker(
				//	this, "SummaryResult.SummaryDefinition.CalculationVersion", this.RecalculateSummary, true );
				PropertyValueTracker tracker = new PropertyValueTracker(
					this, "SummaryResult.DataVersion", this.RecalculateSummary, true );

				// SSP 4/10/12 TFS108549 - Optimizations
				// 
				tracker.ThrottleTime = THROTTLE_TIME;
				tracker.ThrottleFirstInvoke = true;

				// SSP 12/21/11 TFS73767 - Optimizations
				// 
				tracker.AsynchronousDispatcherPriority = DispatcherPriority.Input;

				_summaryDataVersionTracker = tracker;
			}
		}

		#endregion // EnsureSummaryDataVersionTracker

		#region OnIsScrollingChanged

		/// <summary>
		/// We bypass summary calculation while we are scrolling. This is the handler for when the data presenter's
		/// IsScrolling property is changed so we can resume summary calculation.
		/// </summary>
		private void OnIsScrollingChanged( )
		{
			if ( _summaryRecalcBypassed_scrolling )
			{
				_summaryRecalcBypassed_scrolling = false;
				_summaryRecalcBypassed_scrolling_pvt.Deactivate( );
				_summaryRecalcBypassed_scrolling_pvt = null;
				this.RecalculateSummary( );
			}
		}

		#endregion // OnIsScrollingChanged

		#region OnIsVisiblePropertyChanged

		// SSP 4/13/12 TFS108549 - Optimizations
		// 
		/// <summary>
		/// Event handler for IsVisibleChanged event. Note that this is conditionally hooked into.
		/// </summary>
		private void OnIsVisibleChangedHandler( object sender, DependencyPropertyChangedEventArgs e )
		{
			SummaryResultPresenter item = (SummaryResultPresenter)sender;
			if ( item._summaryRecalcBypassed_recordDeactivated && (bool)e.NewValue )
			{
				item._summaryRecalcBypassed_recordDeactivated = false;
				item.IsVisibleChanged -= new DependencyPropertyChangedEventHandler( this.OnIsVisibleChangedHandler );
				item.RecalculateSummary( );
			}
		}

		#endregion // OnIsVisiblePropertyChanged

		#region RecalculateSummary

		private void RecalculateSummary( )
		{
			// SSP 4/13/12 TFS108549 - Optimizations
			// 
			if ( _summaryRecalcBypassed_recordDeactivated || _summaryRecalcBypassed_scrolling )
				return;

			// AS 8/4/09 NA 2009.2 Field Sizing
			// If this is an element created during an autosize operation 
			// then do not force a calculation of the summary.
			//
			if ( AutoSizeFieldHelper.GetIsAutoSizeElement( this ) )
				return;

			SummaryResult result = this.SummaryResult;
			if ( null != result )
			{
				// SSP 4/13/12 TFS108549 - Optimizations
				// 
				// ----------------------------------------------------------------------------
				//result.EnsureCalculated( );
				if ( result.IsDirty( false ) )
				{
					SummaryRecord summaryRecord = this.DataContext as SummaryRecord;
					SummaryRecordPresenter summaryRP = null != summaryRecord
						? summaryRecord.AssociatedRecordPresenter as SummaryRecordPresenter : null;

					if ( null != summaryRP && summaryRP.IsDeactivated )
					{
						_summaryRecalcBypassed_recordDeactivated = true;
						this.IsVisibleChanged += new DependencyPropertyChangedEventHandler( OnIsVisibleChangedHandler );
					}

					DataPresenterBase dp = GridUtilities.GetDataPresenter( result.FieldLayout );
					if ( null != dp && dp.IsScrolling )
					{
						_summaryRecalcBypassed_scrolling = true;
						_summaryRecalcBypassed_scrolling_pvt = new PropertyValueTracker(
							dp, DataPresenterBase.IsScrollingProperty, this.OnIsScrollingChanged );
					}

					if ( _summaryRecalcBypassed_recordDeactivated || _summaryRecalcBypassed_scrolling )
					{
						this.ClearSummaryDataVersionTracker( );
						this.IsCalculationPending = true;
						return;
					}
				}

				result.EnsureCalculated( );
				this.IsCalculationPending = false;
				// ----------------------------------------------------------------------------
			}

			this.EnsureSummaryDataVersionTracker( );
		}

		#endregion // RecalculateSummary

		#region VerifyIsCalculationPending

		// SSP 4/18/12 TFS108549 - Optimizations
		// 
		private void VerifyIsCalculationPending( )
		{
			SummaryResult result = this.SummaryResult;
			if ( null != result && result.IsDirty( false ) )
			{
				this.IsCalculationPending = true;
			}
			else
			{
				this.ClearValue( IsCalculationPendingPropertyKey );
			}
		} 

		#endregion // VerifyIsCalculationPending

		#endregion // Methods
	}

	#endregion // SummaryResultPresenter Class

	#region SummaryResultsPresenter Class

	/// <summary>
	/// SummaryResultsPresenter displays one or more summary results.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryResultsPresenter</b> is used for displaying one or more summary results. It's used by 
	/// the <see cref="SummaryCellPresenter"/> to display summary results of the associated field. It's also
	/// used to display free-form summaries (summaries with their <see cref="SummaryDefinition.Position"/>
	/// property set to <i>Left</i>, <i>Center</i> or <i>Right</i>).
	/// </para>
	/// </remarks>
	/// <seealso cref="SummaryCellPresenter"/>
	/// <seealso cref="SummaryResultPresenter"/>
	/// <seealso cref="SummaryRecordPresenter"/>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SummaryResultsPresenter : Control
	{
		#region Member Vars

		// SSP 4/10/12 TFS108549 - Optimizations
		// 
		private ObservableCollectionExtended<SummaryResultEntry> _cachedSummaryResultEntries;

		#endregion // Member Vars

		#region Constructor

		static SummaryResultsPresenter( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SummaryResultsPresenter ), new FrameworkPropertyMetadata( typeof( SummaryResultsPresenter ) ) );
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( SummaryResultsPresenter ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );

            // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
            UIElement.ClipProperty.OverrideMetadata(typeof(SummaryResultsPresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(VirtualizingDataRecordCellPanel.CoerceCellClip)));

			// AS 8/24/09 TFS19532
			UIElement.VisibilityProperty.OverrideMetadata(typeof(SummaryResultsPresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(GridUtilities.CoerceFieldElementVisibility)));
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryResultsPresenter"/>.
		/// </summary>
		public SummaryResultsPresenter( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region OnPropertyChanged

		// SSP 4/10/12 TFS108549 - Optimizations
		// 
		/// <summary>
		/// Called when a property has changed.
		/// </summary>
		/// <param name="e">Information about the property that was changed.</param>
		protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnPropertyChanged( e );

			bool refreshSummaryResults = false;

			if ( DataContextProperty == e.Property )
			{
				refreshSummaryResults = true;
			}
			else if ( DataItemPresenter.FieldProperty == e.Property )
			{
				refreshSummaryResults = true;
			}
			
			if ( refreshSummaryResults )
			{
				Field field = (Field)this.GetValue( DataItemPresenter.FieldProperty );
				if ( null != field )
				{
					SummaryRecord summaryRecord = this.DataContext as SummaryRecord;
					Debug.Assert( null != summaryRecord || null == this.DataContext );

					IEnumerable<SummaryResult> summaryResults = null != summaryRecord
						? summaryRecord.GetFixedSummaryResults( field, true )
						: null;

					this.SummaryResults = summaryResults;
				}
			}
		}

		#endregion // OnPropertyChanged 

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region SummaryResults

		/// <summary>
		/// Identifies the <see cref="SummaryResults"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryResultsProperty = DependencyProperty.Register(
			"SummaryResults",
			typeof( IEnumerable<SummaryResult> ),
			typeof( SummaryResultsPresenter ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.AffectsMeasure 
				// SSP 4/10/12 TFS108549 - Optimizations
				// Added property change callback.
				// 
				, new PropertyChangedCallback( OnSummaryResultsChanged )
			)
		);

		// SSP 4/10/12 TFS108549 - Optimizations
		// Added property change callback.
		// 
		private static void OnSummaryResultsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
		{
			SummaryResultsPresenter item = (SummaryResultsPresenter)sender;

			IEnumerable<SummaryResult> summaryResults = item.SummaryResults;
			ObservableCollectionExtended<SummaryResultEntry> entries = item._cachedSummaryResultEntries;
			if ( null == entries )
			{
				item._cachedSummaryResultEntries = entries = new ObservableCollectionExtended<SummaryResultEntry>( );
				item.SetValue( SummaryResultEntriesPropertyKey, entries );
			}

			bool beginUpdateCalled = false;

			try
			{
				int i = 0;

				if ( null != summaryResults )
				{
					foreach ( SummaryResult ii in summaryResults )
					{
						// Add entries as needed.
						// 
						if ( i == entries.Count )
						{
							if ( !beginUpdateCalled )
							{
								entries.BeginUpdate( );
								beginUpdateCalled = true;
							}

							entries.Add( new SummaryResultEntry( ) );
						}

						entries[i].SummaryResult = ii;
						i++;
					}
				}

				// Remove any extra items in the entries collection.
				// 
				if ( i < entries.Count )
				{
					if ( !beginUpdateCalled )
					{
						entries.BeginUpdate( );
						beginUpdateCalled = true;
					}

					entries.RemoveRange( i, entries.Count - i );
				}
			}
			finally
			{
				if ( beginUpdateCalled )
					entries.EndUpdate( );
			}
		}

		/// <summary>
		/// Gets or sets the summary results to display.
		/// </summary>
		//[Description( "Specifies the summary results to display." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IEnumerable<SummaryResult> SummaryResults
		{
			get
			{
				return (IEnumerable<SummaryResult>)this.GetValue( SummaryResultsProperty );
			}
			set
			{
				this.SetValue( SummaryResultsProperty, value );
			}
		}

		#endregion // SummaryResults

		#region SummaryResultEntries

		// SSP 4/10/12 TFS108549 - Optimizations
		// 
		private static readonly DependencyPropertyKey SummaryResultEntriesPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"SummaryResultEntries", typeof( IEnumerable<SummaryResultEntry> ), typeof( SummaryResultsPresenter ), null, null
		);

		/// <summary>
		/// Identifies the read-only <see cref="SummaryResultEntries"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryResultEntriesProperty = SummaryResultEntriesPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a collection of <see cref="SummaryResultEntries"/> objects. The collection has the same
		/// number or entries as <see cref="SummaryResults"/> collection and furthermore each entry in this 
		/// collection is for a corresponding SummaryResult object in the <i>SummaryResults</i> collection.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryResultEntries</b> is used by the data template for ths SummaryResultsPresenter to 
		/// display the summary results.
		/// </para>
		/// </remarks>
		/// <seealso cref="SummaryResultEntriesProperty"/>
		/// <seealso cref="SummaryResults"/>
		public IEnumerable<SummaryResultEntry> SummaryResultEntries
		{
			get
			{
				return _cachedSummaryResultEntries;
			}
		}

		#endregion // SummaryResultEntries

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#endregion // Public Methods

		#endregion // Methods
	}

	#endregion // SummaryResultPresenter Class

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