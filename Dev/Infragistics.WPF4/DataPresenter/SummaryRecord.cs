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

namespace Infragistics.Windows.DataPresenter
{
	#region SpecialRecordOrder Class

	/// <summary>
	/// Object used to specify the FieldLayoutSettings' <see cref="FieldLayoutSettings.SpecialRecordOrder"/> property.
	/// </summary>
	/// <remarks>
	/// <b>SpecialRecordOrder</b> is used to specify the order in which summary record and add record
	/// are displayed inside the data grid.
	/// </remarks>
	/// <seealso cref="FieldLayoutSettings.SpecialRecordOrder"/>
	public class SpecialRecordOrder : PropertyChangeNotifier
	{
		#region Private Vars

		private const int DEFAULT_ORDER_VALUE = -1;

		private int _addRecordOrder;
		private int _summaryRecordOrder;
		
		
		private int _filterRecordOrder;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SpecialRecordOrder"/>.
		/// </summary>
		public SpecialRecordOrder( )
		{
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region AddRecord

		/// <summary>
		/// Specifies the order of add-record in relation to other special records. Default value is -1.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>AddRecord</b> along with other properties of this object determine the order
		/// in which special records are displayed inside each record collection. Special 
		/// records with lower order value will be displayed before the special records 
		/// with higher order value.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.SpecialRecordOrder"/>
		/// <seealso cref="FieldLayoutSettings.AllowAddNew"/>
		/// <seealso cref="FieldLayoutSettings.AddNewRecordLocation"/>
		//[Description( "Specifies the order of add-record in relation to other special records." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public int AddRecord
		{
			get
			{
				return _addRecordOrder;
			}
			set
			{
				if ( _addRecordOrder != value )
				{
					_addRecordOrder = value;
					this.RaisePropertyChangedEvent( "AddRecord" );
				}
			}
		}

		/// <summary>
		/// Returns true if the AddRecord property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeAddRecord( )
		{
			return DEFAULT_ORDER_VALUE != _addRecordOrder;
		}

		/// <summary>
		/// Resets the AddRecord property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetAddRecord( )
		{
			this.AddRecord = DEFAULT_ORDER_VALUE;
		}

		#endregion // AddRecord

		#region FilterRecord

		
		
		/// <summary>
		/// Specifies the order of filter record in relation to other special records. Default value is -1.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterRecord</b> along with other properties of this object determine the order
		/// in which special records are displayed inside each record collection. Special 
		/// records with lower order value will be displayed before the special records 
		/// with higher order value.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="FieldLayoutSettings.FilterRecordLocation"/>
		//[Description( "Specifies the order of the filter record in relation to other special records." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public int FilterRecord
		{
			get
			{
				return _filterRecordOrder;
			}
			set
			{
				if ( _filterRecordOrder != value )
				{
					_filterRecordOrder = value;
					this.RaisePropertyChangedEvent( "FilterRecord" );
				}
			}
		}

		/// <summary>
		/// Returns true if the FilterRecord property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeFilterRecord( )
		{
			return DEFAULT_ORDER_VALUE != _filterRecordOrder;
		}

		/// <summary>
		/// Resets the FilterRecord property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetFilterRecord( )
		{
			this.FilterRecord = DEFAULT_ORDER_VALUE;
		}

		#endregion // FilterRecord

		#region SummaryRecord

		/// <summary>
		/// Specifies the order of summary record in relation to other special records. Default value is -1.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryRecord</b> along with other properties of this object determine the order
		/// in which special records are displayed inside each record collection. Special 
		/// records with lower order value will be displayed before the special records 
		/// with higher order value.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.SpecialRecordOrder"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="FieldSettings.SummaryDisplayArea"/>
		//[Description( "Specifies the order of add-record in relation to other special records." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public int SummaryRecord
		{
			get
			{
				return _summaryRecordOrder;
			}
			set
			{
				if ( _summaryRecordOrder != value )
				{
					_summaryRecordOrder = value;
					this.RaisePropertyChangedEvent( "SummaryRecord" );
				}
			}
		}

		/// <summary>
		/// Returns true if the SummaryRecord property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeSummaryRecord( )
		{
			return DEFAULT_ORDER_VALUE != _summaryRecordOrder;
		}

		/// <summary>
		/// Resets the SummaryRecord property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetSummaryRecord( )
		{
			this.SummaryRecord = DEFAULT_ORDER_VALUE;
		}

		#endregion // SummaryRecord

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // SpecialRecordOrder Class

	#region SummaryDisplayAreaContext Class

	/// <summary>
	/// Holds context information for summary record display area.
	/// </summary>
	internal class SummaryDisplayAreaContext : GridUtilities.IMeetsCriteria
	{
		#region Constants

		internal const SummaryDisplayAreas ALL_EXCEPT_GROUPBY_SUMMARIES =
			SummaryDisplayAreas.Bottom | SummaryDisplayAreas.BottomFixed
			| SummaryDisplayAreas.Top | SummaryDisplayAreas.TopFixed;

		#endregion // Constants

		#region Private Vars

		private SummaryDisplayAreas _matchAny;
		private SummaryDisplayAreas _matchAll;
		private SummaryDisplayAreas _matchNone;
		private SummaryDisplayAreas _displayArea;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Context will match any summary whose summary display area intersects with matchAny.
		/// </summary>
		/// <param name="matchAny"></param>
		private SummaryDisplayAreaContext( SummaryDisplayAreas matchAny )
			: this( matchAny, SummaryDisplayAreas.None, SummaryDisplayAreas.None, matchAny )
		{
		}

		/// <summary>
		/// Constructor. Context will match any summary whose summary display area intersects with matchAny 
		/// and is supertset of matchAll. A summary settings is said to match the 
		/// context if it's resolved summary display are doesn't have any bit from the matchNone.
		/// </summary>
		/// <param name="matchAny"></param>
		/// <param name="matchAll"></param>
		/// <param name="matchNone"></param>
		/// <param name="displayArea"></param>
		private SummaryDisplayAreaContext( SummaryDisplayAreas matchAny,
			SummaryDisplayAreas matchAll, SummaryDisplayAreas matchNone, SummaryDisplayAreas displayArea )
		{
			_matchAny = matchAny;
			_matchAll = matchAll;
			_matchNone = matchNone;
			_displayArea = displayArea;
		}

		#endregion // Constructor

		#region Base Overrides

		#region Equals

		/// <summary>
		/// Overridden. Returns true if the specified object equals this object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>True if objects are equal.</returns>
		public override bool Equals( object obj )
		{
			SummaryDisplayAreaContext context = obj as SummaryDisplayAreaContext;
			if ( null == context )
				return false;

			return _displayArea == context._displayArea
				&& _matchAll == context._matchAll
				&& _matchAny == context._matchAny
				&& _matchNone == context._matchNone;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Overridden. Returns the hash code.
		/// </summary>
		/// <returns>Returns the hash code.</returns>
		public override int GetHashCode( )
		{
			return (int)_displayArea ^ (int)_matchAll ^ (int)_matchAny ^ (int)_matchNone;
		}

		#endregion // GetHashCode

		#endregion // Base Overrides

		#region Properties

		#region Private/Internal Properties

		#region AllExceptGroupbySummaries

		internal static SummaryDisplayAreaContext AllExceptGroupbySummaries
		{
			get
			{
				return new SummaryDisplayAreaContext( SummaryDisplayAreaContext.ALL_EXCEPT_GROUPBY_SUMMARIES );
			}
		}

		#endregion // AllExceptGroupbySummaries

		#region BottomScrollingSummariesContext

		internal static SummaryDisplayAreaContext BottomScrollingSummariesContext
		{
			get
			{
				return new SummaryDisplayAreaContext( SummaryDisplayAreas.Bottom );
			}
		}

		#endregion // BottomScrollingSummariesContext

		#region DisplayArea

		internal SummaryDisplayAreas DisplayArea
		{
			get
			{
				return _displayArea;
			}
		}

		#endregion // DisplayArea

		#region InGroupByRecordsSummariesContext

		internal static SummaryDisplayAreaContext InGroupByRecordsSummariesContext
		{
			get
			{
				return new SummaryDisplayAreaContext( SummaryDisplayAreas.InGroupByRecords );
			}
		}

		#endregion // InGroupByRecordsSummariesContext

		#region IsDisplayAreaBottom

		/// <summary>
		/// Returns true if the DisplayArea is either Bottom or BottomFixed.
		/// </summary>
		internal bool IsDisplayAreaBottom
		{
			get
			{
				return SummaryDisplayAreas.Bottom == _displayArea
					|| SummaryDisplayAreas.BottomFixed == _displayArea;
			}
		}

		#endregion // IsDisplayAreaBottom

		#region IsDisplayAreaTop

		/// <summary>
		/// Returns true if the DisplayArea is either Top or TopFixed.
		/// </summary>
		internal bool IsDisplayAreaTop
		{
			get
			{
				return SummaryDisplayAreas.Top == _displayArea
					|| SummaryDisplayAreas.TopFixed == _displayArea;
			}
		}

		#endregion // IsDisplayAreaTop

		#region IsGroupByRecordSummaries

		/// <summary>
		/// Returns true if the summaries footer is contained in a group-by row.
		/// </summary>
		internal bool IsGroupByRecordSummaries
		{
			get
			{
				return SummaryDisplayAreas.InGroupByRecords == _matchAny;
			}
		}

		#endregion // IsGroupByRecordSummaries

		#region NoneSummariesContext

		internal static SummaryDisplayAreaContext NoneSummariesContext
		{
			get
			{
				return new SummaryDisplayAreaContext( SummaryDisplayAreas.None );
			}
		}

		#endregion // NoneSummariesContext

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region DoesSummaryMatch

		internal bool DoesSummaryMatch( SummaryResult summary )
		{
			SummaryDisplayAreas area = summary.DisplayAreaResolved;
			return ( 0 == _matchAny || 0 != ( _matchAny & area ) )
				&& _matchAll == ( _matchAll & area )
				&& 0 == ( _matchNone & area )
				&& SummaryDisplayAreas.None != area;
		}

		#endregion // DoesSummaryMatch

		#region Filter

		/// <summary>
		/// Returns filters results that match this display area context.
		/// </summary>
		/// <param name="results">Results to filter</param>
		/// <returns>Matching results</returns>
		internal IEnumerable<SummaryResult> Filter( IEnumerable<SummaryResult> results )
		{
			return GridUtilities.Filter<SummaryResult>( results, this, false );
		}

		#endregion // Filter

		#region GetContext

		internal static SummaryDisplayAreaContext GetContext( bool top, bool isFixed,
			SummaryResultCollection results )
		{
			RecordCollectionBase records = results.Records;

			SummaryDisplayAreas displayArea = top
				? ( isFixed ? SummaryDisplayAreas.TopFixed : SummaryDisplayAreas.Top )
				: ( isFixed ? SummaryDisplayAreas.BottomFixed : SummaryDisplayAreas.Bottom );

			SummaryDisplayAreas matchNone = SummaryDisplayAreas.None;
			if ( !records.IsTopLevel )
				matchNone |= SummaryDisplayAreas.TopLevelOnly;

			if ( RecordType.DataRecord != records.RecordsType )
				matchNone |= SummaryDisplayAreas.DataRecordsOnly;

			SummaryDisplayAreaContext context = new SummaryDisplayAreaContext( 
				SummaryDisplayAreas.None, displayArea, matchNone, displayArea );

			return context;
		}

		#endregion // GetContext

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region GridUtilities.IMeetsCriteria Interface Implementation

		#region IMeetsCriteria.MeetsCriteria

		bool GridUtilities.IMeetsCriteria.MeetsCriteria( object resultObj )
		{
			SummaryResult result = resultObj as SummaryResult;
			return null != result && this.DoesSummaryMatch( result );
		}

		#endregion // IMeetsCriteria.MeetsCriteria

		#endregion // GridUtilities.IMeetsCriteria Interface Implementation
	}

	#endregion // SummaryDisplayAreaContext Class

	#region SummaryPositionContext Class

	/// <summary>
	/// Holds context information for summary result positioning within a summary record.
	/// </summary>
	internal class SummaryPositionContext : GridUtilities.IMeetsCriteria
	{
		#region Private Vars

		private SummaryPosition _position;
		private Field _positionField;

		#endregion // Private Vars

		#region Constructor

		internal SummaryPositionContext( SummaryPosition position, Field positionField )
		{
			_position = position;
			_positionField = positionField;
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Position

		public SummaryPosition Position
		{
			get
			{
				return _position;
			}
		}

		#endregion // Position

		#region PositionField

		public Field PositionField
		{
			get
			{
				return _positionField;
			}
		}

		#endregion // PositionField

		#endregion // Properties

		#endregion // Public Properties

		#region Methods

		#region Public Methods

		#region DoesSummaryMatch

		public bool DoesSummaryMatch( SummaryResult result )
		{
			return _position == result.PositionResolved 
				&& ( SummaryPosition.UseSummaryPositionField != _position || _positionField == result.PositionFieldResolved );
		}

		#endregion // DoesSummaryMatch

		#endregion // Public Methods

		#region Private/Internal Methods

		#region Filter

		/// <summary>
		/// Returns filters results that match this summary position context.
		/// </summary>
		/// <param name="results">Results to filter</param>
		/// <returns>Matching results</returns>
		internal IEnumerable<SummaryResult> Filter( IEnumerable<SummaryResult> results )
		{
			return GridUtilities.Filter<SummaryResult>( results, this, false );
		}

		#endregion // Filter

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region GridUtilities.IMeetsCriteria Interface Implementation

		#region IMeetsCriteria.MeetsCriteria

		bool GridUtilities.IMeetsCriteria.MeetsCriteria( object resultObj )
		{
			SummaryResult result = resultObj as SummaryResult;
			return null != result && this.DoesSummaryMatch( result );
		}

		#endregion // IMeetsCriteria.MeetsCriteria

		#endregion // GridUtilities.IMeetsCriteria Interface Implementation
	}

	#endregion // SummaryPositionContext Class

	#region SummaryRecord Class

	/// <summary>
	/// Summary record displays summary results.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryRecord</b> displays summary results of a record collection. It's displayed
	/// either above or below the data records based on the <see cref="FieldSettings.SummaryDisplayArea"/>
	/// property setting.
	/// </para>
	/// <para class="body">
	/// RecordCollection's <see cref="RecordCollectionBase.SummaryResults"/> property can be
	/// used to access summary calculation results for that record collection. To actually
	/// add summaries in code, use the FieldLayout's <see cref="FieldLayout.SummaryDefinitions"/>
	/// property.
	/// </para>
	/// <para class="body">
	/// <b>Note</b> that SummaryRecord object is not involved in displaying summary results inside each 
	/// group-by record element.
	/// </para>
	/// </remarks>
	/// <seealso cref="RecordCollectionBase.SummaryResults"/>
	/// <seealso cref="FieldLayout.SummaryDefinitions"/>
	/// <seealso cref="FieldSettings.AllowSummaries"/>
	/// <seealso cref="FieldSettings.SummaryUIType"/>
	/// <seealso cref="FieldSettings.SummaryDisplayArea"/>
	/// <seealso cref="SummaryDefinition.DisplayArea"/>
	public class SummaryRecord : Record
	{
		#region Private Vars

		private SummaryDisplayAreaContext _summaryDisplayAreaContext;
		private IEnumerable<SummaryResult> _cachedSummaryResults;
		private object _cachedSummaryRecordHeaderResolved;
		private int _verifiedCachedSummaryResultsVersion;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryRecord"/>.
		/// </summary>
		/// <param name="fieldLayout">Field layout of the summary record</param>
		/// <param name="parentRecords">Parent record collection</param>
		/// <param name="summaryDisplayAreaContext">Single display area associated with this summary record</param>
		internal SummaryRecord( FieldLayout fieldLayout, RecordCollectionBase parentRecords, SummaryDisplayAreaContext summaryDisplayAreaContext ) 
			: base( fieldLayout, parentRecords )
		{
			_summaryDisplayAreaContext = summaryDisplayAreaContext;
		}

		#endregion // Constructor

		#region Base class overrides

		#region ChildRecordsInternal

		internal override RecordCollectionBase ChildRecordsInternal { get { return null; } }

		#endregion //ChildRecordsInternal

        // JJD 9/30/08 - added support for printing
        #region CloneAssociatedRecordSettings


        // MBS 7/28/09 - NA9.2 Excel Exporting
        //internal override void CloneAssociatedRecordSettings(Record associatedRecord, ReportViewBase reportView)
        internal override void CloneAssociatedRecordSettings(Record associatedRecord, IExportOptions options)
        {
            base.CloneAssociatedRecordSettings(associatedRecord, options);

            // nothing special has to be done for the summary record
        }

        #endregion //CloneAssociatedRecordSettings	

		#region CreateRecordPresenter

		/// <summary>
		/// Creates a new element to represent this record in a record list control.
		/// </summary>
		/// <returns>Returns a new element to be used for representing this record in a record list control.</returns>
		internal override RecordPresenter CreateRecordPresenter( )
		{
			return new SummaryRecordPresenter( );
		}

		#endregion // CreateRecordPresenter

		#region Description

		/// <summary>
		/// Gets/sets the description for the record.
		/// </summary>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public override string Description
		{
			get
			{
				string description = base.Description;
				if ( !string.IsNullOrEmpty( description ) )
					return description;

				string header = this.SummaryRecordHeaderResolved as string;
				return header ?? string.Empty;
			}
			set
			{
				base.Description = value;
			}
		}

		#endregion // Description

        // JJD 4/1/08 - added support for printing
        #region GetAssociatedRecord


        // JJD 11/24/09 - TFS25215 - made public 
        /// <summary>
        /// Returns the associated record from the UI <see cref="DataPresenterBase"/> during a print or export operation. 
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> during a print or export operation clones of records are made that are used only during the operation. This method returns the source record this record was cloned from.</para>
        /// </remarks>
        /// <returns>The associated record from the UI DataPresenter or null.</returns>
        //internal override Record GetAssociatedRecord()
        public override Record GetAssociatedRecord()
        {
            // JJD 9/23/08 - added support for printing
            ViewableRecordCollection vrc = this.ParentCollection.ViewableRecords;
            
            if ( vrc != null )
                return vrc.GetAssociatedSummaryRecord(this);

            return null;
        }

        #endregion //GetAssociatedRecord

        #region HasChildrenInternal

        internal override bool HasChildrenInternal
		{
			get
			{
				return false;
			}
		}

		#endregion //HasChildrenInternal

        // JJD 05/06/10 - TFS27757 added
        #region IsActivatable

        /// <summary>
        /// Property: Returns false since the record cannot be activated
        /// </summary>
        internal protected override bool IsActivatable
        {
            get
            {
                return false;
            }
        }

        #endregion // IsActivatable

		#region IsSelectable

		/// <summary>
		/// Overridden. Returns false since the summary record is not selectable.
		/// </summary>
		internal protected override bool IsSelectable
		{
			get
			{
				return false;
			}
		}

		#endregion // IsSelectable

		#region IsSpecialRecord

		// SSP 8/5/09 - NAS9.2 Enhanced grid view
		// Made public.
		// 
		//internal override bool IsSpecialRecord
		/// <summary>
		/// Overridden. Returns true since the summary record is a special record.
		/// </summary>
		public override bool IsSpecialRecord
		{
			get
			{
				return true;
			}
		}

		#endregion // IsSpecialRecord

		#region RecordType

		/// <summary>
		/// Returns the type of the record (read-only).
		/// </summary>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public override RecordType RecordType
		{
			get
			{
				return RecordType.SummaryRecord;
			}
		}

		#endregion //RecordType

		#region SortChildren

		internal override void SortChildren( )
		{
			// Do nothing since the summary record doesn't have any children.
			// 
		}

		#endregion //SortChildren

		#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString( )
		{
			return "SummaryRecord: " + this.Description;
		}

		#endregion //ToString

		#endregion Base class overrides

		#region Properties

		#region Public Properties

		#region SummaryRecordHeaderResolved

		/// <summary>
		/// Returns the resolved summary record header content.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryRecordHeaderResolved</b> returns the resolved summary record header text
		/// based on the settings of SummaryResultCollection's <see cref="SummaryResultCollection.SummaryRecordHeader"/>
		/// and FieldLayout's <see cref="FieldLayout.SummaryDescriptionMask"/> and
		/// <see cref="FieldLayout.SummaryDescriptionMaskInGroupBy"/> properties. See
		/// <see cref="FieldLayout.SummaryDescriptionMask"/> for more information on how
		/// this property's default value is resolved.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> To actually set the summary record header's contents, use 
		/// <see cref="FieldLayout.SummaryDescriptionMask"/> and <see cref="FieldLayout.SummaryDescriptionMaskInGroupBy"/>
		/// or <see cref="SummaryResultCollection.SummaryRecordHeader"/>
		/// properties. To hide the summary record header, set these properties to empty string (String.Empty).
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayout.SummaryDescriptionMask"/>
		/// <seealso cref="FieldLayout.SummaryDescriptionMaskInGroupBy"/>
		/// <seealso cref="SummaryResultCollection.SummaryRecordHeader"/>
		public object SummaryRecordHeaderResolved
		{
			get
			{
				if ( null == _cachedSummaryRecordHeaderResolved )
				{
					object mask = this.SummaryRecordHeaderMaskResolved;
					object newVal = this.ResolveSummaryRecordHeaderValue( mask );
					bool changed = !object.Equals( newVal, _cachedSummaryRecordHeaderResolved );
					_cachedSummaryRecordHeaderResolved = newVal;

					if ( changed )
						this.RaisePropertyChangedEvent( "SummaryRecordHeaderResolved" );
				}

				return _cachedSummaryRecordHeaderResolved;
			}
		}

		#endregion // SummaryRecordHeaderResolved

		#region SummaryResults

		/// <summary>
		/// Returns the summary results that will be displayed in this summary record.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryResults</b> property returns the summary calculation results that will
		/// be displayed in this summary record. To get all calculated summary results for a 
		/// particular record collection, use the RecordCollectionBase's 
		/// <see cref="RecordCollectionBase.SummaryResults"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		/// <seealso cref="SummaryResult.Value"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="FieldSettings.AllowSummaries"/>
		/// <seealso cref="FieldSettings.SummaryUIType"/>
		public IEnumerable<SummaryResult> SummaryResults
		{
			get
			{
				this.VerifySummaryResults( );
				return _cachedSummaryResults;
			}
		}

		#endregion // SummaryResults

		#endregion // Public Properties

		#region Private/Internal Properties

		#region SummaryDisplayAreaContext

		/// <summary>
		/// Returns the summary display area context for this summary record. It describes where
		/// this summary record is supposed to go - top, top-fixed, bottom, bottom-fixed.
		/// </summary>
		internal SummaryDisplayAreaContext SummaryDisplayAreaContext
		{
			get
			{
				return _summaryDisplayAreaContext;
			}
		}

		#endregion // SummaryDisplayAreaContext

		#region SummaryRecordHeaderMaskResolved

		/// <summary>
		/// Returns the resolved summary record header template without replacing the placeholder tokens 
		/// with their associated values.
		/// </summary>
		private object SummaryRecordHeaderMaskResolved
		{
			get
			{
				// AS 2/9/11 NA 2011.1 Word Writer
				// Moved to a helper routine so the exporter could get this value.
				//
				//RecordCollectionBase records = this.ParentCollection;
				//SummaryResultCollection summaryResults = records.SummaryResults;
				//object ret = null != summaryResults ? summaryResults.SummaryRecordHeader : null;
				//if ( null != ret )
				//    return ret;
				//
				//bool parentRecordIsGroupBy = records.ParentRecord is GroupByRecord;
				//
				//FieldLayout fl = records.FieldLayout;
				//if ( null != fl )
				//{
				//    ret = parentRecordIsGroupBy
				//        ? fl.SummaryDescriptionMaskInGroupBy
				//        : fl.SummaryDescriptionMask;
				//
				//    if ( null != ret )
				//        return ret;
				//}
				//
				//if ( records.IsRootLevel )
				//    // Root records
				//    return "Grand Summaries";
				//else if ( parentRecordIsGroupBy )
				//    // A GroupByRecord's children
				//    return "Summaries for [GROUP_BY_VALUE]";
				//else
				//    // Child records of a parent data record
				//    return "Summaries for [PRIMARY_FIELD]";
				return GetHeaderMaskResolved(this.ParentCollection);
			}
		}

		#endregion // SummaryRecordHeaderMaskResolved

		#region SummaryRecordHeaderVisibilityResolved

		/// <summary>
		/// Returns a resolved value indivating whether the summary record's header should be visible.
		/// </summary>
		internal Visibility SummaryRecordHeaderVisibilityResolved
		{
			get
			{
				FieldLayout fieldLayout = this.FieldLayout;
				Visibility ret = null != fieldLayout ? fieldLayout.SummaryDescriptionVisibilityResolved : Visibility.Collapsed;

				if ( Visibility.Visible == ret )
				{
					object header = this.SummaryRecordHeaderResolved;
					if ( GridUtilities.IsNullOrEmpty( header ) )
						ret = Visibility.Collapsed;
				}

				return ret;
			}
		}

		#endregion // SummaryRecordHeaderVisibilityResolved

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region GetFixedSummaryResults

		/// <summary>
		/// Gets the summary results that are displayed under the specified field.
		/// </summary>
		/// <param name="field">Position field.</param>
		/// <param name="returnNotifyingCollection">Whether to return a collection that implements
		/// INotifyCollectionChanged so whenever summaries are added/removed, the collection 
		/// sends out appropriate notifications.</param>
		/// <returns>Summaries displayed under the specified field.</returns>
		internal IEnumerable<SummaryResult> GetFixedSummaryResults( Field field, bool returnNotifyingCollection )
		{
			if ( returnNotifyingCollection )
			{
				GridUtilities.IMeetsCriteria filter = new GridUtilities.MeetsCriteriaChain(
					this.SummaryDisplayAreaContext,
					new SummaryPositionContext( SummaryPosition.UseSummaryPositionField, field ),
					false );


				GridUtilities.NotifyCollectionEnumerable<SummaryResult> coll =
					new GridUtilities.NotifyCollectionEnumerable<SummaryResult>( this.ParentCollection.SummaryResults, filter );

				coll.Tag = new PropertyValueTracker( this,
							"FieldLayout.SummaryDefinitions.SummariesVersion",
							coll.RaiseCollectionChanged,
							// MD 8/17/10
							// We should be handling the dirtying of this version asynchronously because 
							// it may happen multiple times in the a row.
							true );

				return coll;
			}

			return new SummaryPositionContext( SummaryPosition.UseSummaryPositionField, field ).Filter( this.SummaryResults );
		}

		#endregion // GetFixedSummaryResults

		#region GetFreeFormSummaryResults

		internal IEnumerable<SummaryResult> GetFreeFormSummaryResults( SummaryPosition pos )
		{
			Debug.Assert( SummaryPosition.Left == pos || SummaryPosition.Center == pos || SummaryPosition.Right == pos );
			return new SummaryPositionContext( pos, null ).Filter( this.SummaryResults );
		}

		#endregion // GetFreeFormSummaryResults

		// AS 2/9/11 NA 2011.1 Word Writer
		// Moved here from the SummaryRecordHeaderMaskResolved property.
		//
		#region GetHeaderMaskResolved
		internal static object GetHeaderMaskResolved(RecordCollectionBase parentCollection)
		{
			RecordCollectionBase records = parentCollection;
			SummaryResultCollection summaryResults = records.SummaryResults;
			object ret = null != summaryResults ? summaryResults.SummaryRecordHeader : null;
			if (null != ret)
				return ret;

			bool parentRecordIsGroupBy = records.ParentRecord is GroupByRecord;

			FieldLayout fl = records.FieldLayout;
			if (null != fl)
			{
				ret = parentRecordIsGroupBy
					? fl.SummaryDescriptionMaskInGroupBy
					: fl.SummaryDescriptionMask;

				if (null != ret)
					return ret;
			}

			if (records.IsRootLevel)
				// Root records
				return "Grand Summaries";
			else if (parentRecordIsGroupBy)
				// A GroupByRecord's children
				return "Summaries for [GROUP_BY_VALUE]";
			else
				// Child records of a parent data record
				return "Summaries for [PRIMARY_FIELD]";
		}
		#endregion //GetHeaderMaskResolved

		#region HasFixedSummaryResults

		// SSP 1/6/10 TFS25633
		// Added an overload that doesn't take field.
		// 
		internal bool HasFixedSummaryResults( )
		{
			IEnumerable<SummaryResult> results = this.SummaryResults;
			if ( null != results )
			{
				foreach ( SummaryResult ii in results )
				{
					if ( SummaryPosition.UseSummaryPositionField == ii.PositionResolved )
						return true;
				}
			}

			return false;
		}

		internal bool HasFixedSummaryResults( Field field )
		{
			return GridUtilities.HasItems( this.GetFixedSummaryResults( field, false ) );
		}

		#endregion // HasFixedSummaryResults

		#region ResolveSummaryRecordHeaderValue

		/// <summary>
		/// If the specified header is a template (not ControlTemplate, just a string with special
		/// tokens that are placeholders) then fills in the placeholders in the template and returns it.
		/// </summary>
		/// <param name="headerTemplate">Summary record header.</param>
		/// <returns>Returns the resolved header value.</returns>
		private object ResolveSummaryRecordHeaderValue( object headerTemplate )
		{
			// AS 2/9/11 NA 2011.1 Word Writer
			return ResolveSummaryRecordHeaderValue(this.ParentCollection, this.FieldLayout, headerTemplate);
		}

		// AS 2/9/11 NA 2011.1 Word Writer
		// Added static overload so I could use it from the word exporter.
		//
		internal static object ResolveSummaryRecordHeaderValue(RecordCollectionBase parentCollection, FieldLayout fieldLayout, object headerTemplate)
		{
			string captionStr = headerTemplate as string;
			if ( !string.IsNullOrEmpty( captionStr ) )
			{
				System.Text.StringBuilder sb = null;

				int index = 0;
				while ( index < captionStr.Length )
				{
					string substitutedStr = null;

					int bracketStartIndex = captionStr.IndexOf( '[', index );
					int bracketEndIndex = -1;
					if ( bracketStartIndex >= 0 )
					{
						bracketEndIndex = captionStr.IndexOf( ']', 1 + bracketStartIndex );

						if ( bracketEndIndex > 0 )
						{
							// AS 2/9/11 NA 2011.1 Word Writer
							//string tokenValue = this.GetSummaryRecordHeaderTokenValue( captionStr.Substring( 1 + bracketStartIndex, bracketEndIndex - bracketStartIndex - 1 ) );
							string tokenValue = GetSummaryRecordHeaderTokenValue( parentCollection, fieldLayout, captionStr.Substring( 1 + bracketStartIndex, bracketEndIndex - bracketStartIndex - 1 ) );
							substitutedStr = null != tokenValue ? tokenValue : string.Empty;
						}
					}

					if ( null != substitutedStr )
					{
						if ( null == sb )
							sb = new System.Text.StringBuilder( captionStr.Length + substitutedStr.Length );

						sb.Append( captionStr, index, bracketStartIndex - index );
						sb.Append( substitutedStr );
						index = 1 + bracketEndIndex;
					}
					else
						break;
				}

				if ( null != sb )
				{
					if ( index < captionStr.Length )
						sb.Append( captionStr, index, captionStr.Length - index );

					return sb.ToString( );
				}
			}

			return headerTemplate;
		}

		// AS 2/9/11 NA 2011.1 Word Writer
		// Changed to a static method so I could get the header in the exporter for 
		// groupby records without needing to create a summary record. The only 
		// instance member used was this.ParentCollection.
		//
		//private string GetParentDataRecordFieldValueHelper( string fieldName, bool primaryField, bool scrolltipField )
		private static string GetParentDataRecordFieldValueHelper( RecordCollectionBase records, string fieldName, bool primaryField, bool scrolltipField )
		{
			// AS 2/9/11 NA 2011.1 Word Writer
			//RecordCollectionBase records = this.ParentCollection;
			DataRecord parentDataRecord = records.ParentDataRecord;
			FieldLayout parentRecordFieldLayout = null != parentDataRecord ? parentDataRecord.FieldLayout : null;
			if ( null != parentRecordFieldLayout )
			{
				Field field;
				if ( primaryField )
					field = parentRecordFieldLayout.PrimaryField;
				else if ( scrolltipField )
					field = parentRecordFieldLayout.ScrollTipField;
				else
					field = GridUtilities.GetField( parentRecordFieldLayout, fieldName, false );

				if ( null != field )
				{
					string str = parentDataRecord.GetCellText( field );
					return null != str ? str : string.Empty;
				}
			}

			return null;
		}

		// AS 2/9/11 NA 2011.1 Word Writer
		// Changed to a static method so I could use this from the exporting as opposed to 
		// creating a summary record just to get the header. The only instance members used 
		// are the ParentCollection and the FieldLayout. As part of this I changed the 
		// calls to GetParentDataRecordFieldValueHelper within this method to pass along the 
		// records.
		//
		//private string GetSummaryRecordHeaderTokenValue(string token)
		private static string GetSummaryRecordHeaderTokenValue(RecordCollectionBase records, FieldLayout fieldLayout, string token)
		{
			// AS 2/9/11 NA 2011.1 Word Writer
			//RecordCollectionBase records = this.ParentCollection;
			//FieldLayout fieldLayout = this.FieldLayout;

			const string PRIMARY_FIELD = "PRIMARY_FIELD";
			const string SCROLLTIP_FIELD = "SCROLLTIP_FIELD";
			const string GROUP_BY_VALUE = "GROUP_BY_VALUE";
			const string GROUP_BY_VALUES = "GROUP_BY_VALUES";
			const string PARENT_FIELD_NAME = "PARENT_FIELD_NAME";

			switch ( token )
			{
				case PRIMARY_FIELD:
					{
						string str = GetParentDataRecordFieldValueHelper( records, null, true, false );
						if ( null != str )
							return str;

						break;
					}
				case SCROLLTIP_FIELD:
					{
						string str = GetParentDataRecordFieldValueHelper( records, null, false, true );
						if ( null != str )
							return str;

						break;
					}
				case GROUP_BY_VALUE:
				case GROUP_BY_VALUES:
					{
						GroupByRecord groupByRecord = records.ParentRecord as GroupByRecord;
						if ( null != groupByRecord )
						{
							bool onlyParentGroupByRecord = GROUP_BY_VALUE == token;
							StringBuilder sb = null;
							while ( null != groupByRecord )
							{
								object val = groupByRecord.Value;
								
								string valAsStr = null != val ? val.ToString( ) : string.Empty;

								if ( onlyParentGroupByRecord )
									return valAsStr;

								if ( null == sb )
									sb = new StringBuilder( valAsStr );
								else
									sb.Append( ", " ).Append( valAsStr );

								groupByRecord = groupByRecord.ParentRecord as GroupByRecord;
							}

							if ( null != sb )
								return sb.ToString( );
						}

						break;
					}
				case PARENT_FIELD_NAME:
					{
						RecordManager rm = records.ParentRecordManager;
						ExpandableFieldRecord parentExpandableFieldRecord = null != rm ? rm.ParentRecord : null;
						if ( null != parentExpandableFieldRecord )
						{
							string val = parentExpandableFieldRecord.Description;
							return null != val ? val : string.Empty;
						}

						break;
					}
				default:
					{
						// Anything other than above inside [] should be treated as a field name.
						// 
						string str = GetParentDataRecordFieldValueHelper( records, token, false, false );
						if ( null != str )
							return str;

						break;
					}
			}

			return null;
		}

		#endregion // ResolveSummaryRecordHeaderValue

		#region VerifySummaryResults

		internal void VerifySummaryResults( )
		{
			SummaryResultCollection results = this.ParentCollection.SummaryResults;
			if ( null != results && _verifiedCachedSummaryResultsVersion != results.SummariesVersion )
			{
				_verifiedCachedSummaryResultsVersion = results.SummariesVersion;
				_cachedSummaryResults = results.GetSummaryResults( _summaryDisplayAreaContext, null );
				this.RaisePropertyChangedEvent( "SummaryResults" );
			}
		}

		#endregion // VerifySummaryResults

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // SummaryRecord Class

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