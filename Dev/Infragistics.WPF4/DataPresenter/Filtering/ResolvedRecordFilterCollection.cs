using System;
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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Infragistics.Collections;


using System.Linq;


namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A collection used for resolving record filters that are to be applied to a record. Since
	/// a record manager can have records with different field layouts, this collection is used
	/// for resolving the record filters and keeping track of changes in the source filter 
	/// collections for any changes.
	/// </summary>
	internal class ResolvedRecordFilterCollection : DependencyObject
	{
		#region Nested Data Structures

		#region DataItemFilterEvaluator Class

		// SSP 2/15/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		private class DataItemFilterEvaluator
		{
			#region Nested Data Structures

			#region EvaluationContext Class

			internal class EvaluationContext : ConditionEvaluationContext
			{
				#region ContextValueEntry Class

				internal class ContextValueEntry : ValueEntry
				{
					private EvaluationContext _context;
					private object _value;

					internal ContextValueEntry( object value, EvaluationContext context )
					{
						_context = context;
						_value = value;
					}

					public override object Context
					{
						get
						{
							return _context._currentDataItem;
						}
					}

					public override System.Globalization.CultureInfo Culture
					{
						get
						{
							return _context._culture;
						}
					}

					public override string Format
					{
						get
						{
							return null;
						}
					}

					public override object Value
					{
						get
						{
							return _value;
						}
					}
				}

				#endregion // ContextValueEntry Class

				internal readonly RecordManager _recordManager;
				internal readonly Field _field;
				internal PropertyDescriptor _propertyDescriptor;
				internal object _currentDataItem;
				internal CultureInfo _culture;
				private ContextValueEntry _currentValue;
				private Type _prefComparisonType;
				private bool _ignoreCase = true;

				internal EvaluationContext( RecordManager recordManager, Field field )
				{
					_recordManager = recordManager;
					_field = field;
					_culture = GridUtilities.GetDefaultCulture( field );

					// SSP 5/3/10 TFS25788
					// If field data type is string then let the filtering logic decide what is the
					// best comparison type. For example, if the string field actually contains 
					// numeric values as strings and a filter condition with a numeric compare value
					// has been specified then a numeric comparison will be done by the filtering
					// logic.
					// 
					Type type = _field.EditAsTypeResolved;
					if ( typeof( string ) == type )
						type = null;

					_prefComparisonType = type;
				}

				public override IEnumerable<ValueEntry> AllValues
				{
					get
					{
						IEnumerable allDataItems = VUtils.GetCollectionViewSourceItems( _recordManager );

						IEnumerable<object> typedItems = allDataItems as IEnumerable<object> ?? new TypedEnumerable<object>( allDataItems );


						return from dataItem in typedItems
							   select new ContextValueEntry( this.GetCellValueHelper( dataItem ), this );


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

					}
				}

				public override IComparer Comparer
				{
					get
					{
						return _field.FilterComparerResolved ?? base.Comparer;
					}
				}

				public override ValueEntry CurrentValue
				{
					get
					{
						return _currentValue;
					}
				}

				public override bool IgnoreCase
				{
					get
					{
						return _ignoreCase;
					}
				}

				public void SetIgnoreCase( bool val )
				{
					_ignoreCase = val;
				}

				public void InitializeCurrentValue( object dataItem, object value )
				{
					_currentDataItem = dataItem;
					_currentValue = new ContextValueEntry( value, this );
				}

				public override Type PreferredComparisonDataType
				{
					get
					{
						return _prefComparisonType;
					}
				}

				internal object GetCellValueHelper( object dataItem )
				{
					return _propertyDescriptor.GetValue( dataItem );
				}
			}

			#endregion // EvaluationContext Class

			#region WeakPredicateHelper Class

			internal class WeakPredicateHelper
			{
				private WeakReference _evaluator;

				internal WeakPredicateHelper( DataItemFilterEvaluator evaluator )
				{
					_evaluator = new WeakReference( evaluator );
				}

				internal bool FilterDataItem( object dataItem )
				{
					if ( null != _evaluator )
					{
						DataItemFilterEvaluator evaluator = (DataItemFilterEvaluator)Utilities.GetWeakReferenceTargetSafe( _evaluator );

						if ( null != evaluator )
							return evaluator.FilterDataItem( dataItem );
						else
							_evaluator = null;
					}

					return true;
				}
			}

			#endregion // WeakPredicateHelper Class 

			#endregion // Nested Data Structures

			#region Member Vars

			private readonly ResolvedRecordFilterCollection _coll;
			private readonly bool _populatingFilterDropDown;
			private WeakDictionary<Field, EvaluationContext> _evaluationContexts;
			private RecordFilter _ignoreFilter;
			private PropertyDescriptorProvider _cachedPropertyDescriptorProvider;

			internal readonly Predicate<object> _filterPredicate;

			#endregion // Member Vars

			#region Constructor

			internal DataItemFilterEvaluator( ResolvedRecordFilterCollection coll, bool populatingFilterDropDown, RecordFilter ignoreFilter )
			{
				_coll = coll;
				_populatingFilterDropDown = populatingFilterDropDown;
				_ignoreFilter = ignoreFilter;

				_filterPredicate = new WeakPredicateHelper( this ).FilterDataItem;
			} 

			#endregion // Constructor

			#region Methods

			#region EnsurePropertyDescriptor

			private void EnsurePropertyDescriptor( object dataItem, EvaluationContext context )
			{
				if ( null == context._propertyDescriptor )
				{
					ICollectionView view = VUtils.GetCollectionView( _coll._recordManager );
					PropertyDescriptorProvider provider = _cachedPropertyDescriptorProvider
						?? ( _cachedPropertyDescriptorProvider = PropertyDescriptorProvider.CreateProvider( dataItem, view ) );

					if ( null != provider )
					{
						PropertyDescriptorCollection props = provider.GetProperties( );
						context._propertyDescriptor = props[VUtils.GetPropertyName( context._field )];
					}

					Debug.Assert( null != context._propertyDescriptor );
				}
			}

			#endregion // EnsurePropertyDescriptor

			#region FilterDataItem

			internal bool FilterDataItem( object dataItem )
			{
				FieldLayout fl = _coll._recordManager.FieldLayout;
				RecordFilterCollection filters = null != fl ? _coll.GetRecordFilters( fl, false ) : null;
				if ( null != filters )
				{
					LogicalOperator logicalOperator = fl.RecordFiltersLogicalOperatorResolved;

					bool allMatched = true;
					int count = filters.Count;
					int evaluatedFilterCount = 0;

					for ( int i = 0; i < count; i++ )
					{
						RecordFilter filter = filters[i];
						if ( filter.HasConditions )
						{
							Field filterField = filter.Field;
							if ( _ignoreFilter != filter && fl == GridUtilities.GetFieldLayout( filterField ) )
							{
								EvaluationContext context = this.GetEvaluationContext( filterField );
								EnsurePropertyDescriptor( dataItem, context );

								object cellValue = context.GetCellValueHelper( dataItem );
								context.InitializeCurrentValue( dataItem, cellValue );

								bool isMatch = filter.Conditions.IsMatch( cellValue, context );
								evaluatedFilterCount++;

								// If logical operator is 'Or' then return true even if a single filter
								// matches the record.
								// 
								if ( isMatch && LogicalOperator.Or == logicalOperator )
									return true;

								// JJD 1/7/11 - Optimization
								// If logical operator is 'And' then return false even if a single filter
								// doesn't match the record.
								// 
								if ( isMatch == false && LogicalOperator.And == logicalOperator )
									return false;

								allMatched = allMatched && isMatch;
							}
						}
					}

					if ( evaluatedFilterCount > 0 )
						return allMatched;
				}

				return true;
			}

			#endregion // FilterDataItem

			#region GetEvaluationContext

			private EvaluationContext GetEvaluationContext( Field field )
			{
				if ( null == _evaluationContexts )
					_evaluationContexts = new WeakDictionary<Field, EvaluationContext>( true, false );

				EvaluationContext context;
				if ( !_evaluationContexts.TryGetValue( field, out context ) )
				{
					context = new EvaluationContext( _coll._recordManager, field );
					_evaluationContexts[field] = context;
				}

				return context;
			}

			#endregion // GetEvaluationContext 

			#endregion // Methods
		}

		#endregion // DataItemFilterEvaluator Class

		#region FieldFilterInfo Class

		internal class FieldFilterInfo : PropertyChangeNotifier
		{
			#region Nested Data Structures

			#region FilterDropDownItemComparer Class

			/// <summary>
			/// Used for sorting unique cell values in the filter drop-down.
			/// </summary>
			internal class FilterDropDownItemComparer : IComparer<FilterDropDownItem>
			{
				internal RecordManager.SameFieldRecordsSortComparer.FieldSortInfo _fieldSortInfo;

				internal FilterDropDownItemComparer( FieldFilterInfo info )
				{
					_fieldSortInfo = new RecordManager.SameFieldRecordsSortComparer.FieldSortInfo( info._field );
				}

				public int Compare( FilterDropDownItem x, FilterDropDownItem y )
				{
					IComparer comparer = _fieldSortInfo._comparer;
					if ( null != comparer )
					{
						return comparer.Compare( x.Value, y.Value );
					}

					// JJD 06/29/10 - TFS32174 - Optimization
					// Use cached CellInfo
					//RecordManager.SameFieldRecordsSortComparer.CellInfo xxCellInfo = _fieldSortInfo.GetCellInfo( (DataRecord)x.Tag );
					//RecordManager.SameFieldRecordsSortComparer.CellInfo yyCellInfo = _fieldSortInfo.GetCellInfo( (DataRecord)y.Tag );
					RecordManager.SameFieldRecordsSortComparer.CellInfo xxCellInfo = x._cellInfo;
					RecordManager.SameFieldRecordsSortComparer.CellInfo yyCellInfo = y._cellInfo;

					return RecordManager.SameFieldRecordsSortComparer.DefaultCompare(
						// SSP 5/29/09 - TFS17233 - Optimization
						// Now we are storing a reference to fieldSortInfo in the class so no need to pass along
						// the parameter.
						// 
						//xxCellInfo.GetCompareValue( _fieldSortInfo ),
						//yyCellInfo.GetCompareValue( _fieldSortInfo ),
						xxCellInfo.GetCompareValue( ),
						yyCellInfo.GetCompareValue( ),
						_fieldSortInfo._field );
				}
			}

			#endregion // FilterDropDownItemComparer Class

			#region MeetsCriteria_Filter Class

			/// <summary>
			/// Used for filtering data records for the purposes of populating the filter drop-down with
			/// unique cell values from the list.
			/// </summary>
			public class MeetsCriteria_Filter : GridUtilities.IMeetsCriteria
			{
				#region Member Vars

				private FieldFilterInfo _info;
				private bool _includeFilteredOutValues;
				private RecordFilter _ignoreFilter;

				#endregion // Member Vars

				#region Constructor

				/// <summary>
				/// Constructor.
				/// </summary>
				/// <param name="info">FieldFilterInfo backward pointer.</param>
				/// <param name="includeFilteredOutValues">Whether to include cell values from filtered out records.</param>
				/// <param name="ignoreFilter">Specifies the filter to ignore when evaluating whether a record is filtered out 
				/// or not. When a field with pre-existing filter criteria has its filter drop-down dropped down, the drop-down 
				/// should contain cell values with disregard to the field's filter criteria. Only used if 
				/// includeFilteredOutValues is false.</param>
				public MeetsCriteria_Filter( FieldFilterInfo info, bool includeFilteredOutValues, RecordFilter ignoreFilter )
				{
					_info = info;
					_includeFilteredOutValues = includeFilteredOutValues;
					_ignoreFilter = ignoreFilter;
				}

				#endregion // Constructor

				#region MeetsCriteria

				public bool MeetsCriteria( object recordObj )
				{
					DataRecord dr = recordObj as DataRecord;
					if ( null != dr )
					{
						// Don't include records that are explicitly hidden, even if 
						// includeFilteredOutValues is true.
						// 
						Visibility visibility = dr.VisibilityResolved_NoFiltering;
						if ( Visibility.Visible != visibility )
							return false;

						if ( _includeFilteredOutValues || !dr.InternalIsFilteredOut_Verify )
						{
							return true;
						}
						else if ( null != _ignoreFilter && _ignoreFilter.HasConditions )
						{
							// When RecordFilterScope is AllRecords, we can get records from different
							// record managers and therefore _info._filters is not always the record
							// filters that the dr should be evaluated with.
							// 
							//if ( _info._filters.MeetsCriteria( dr, _ignoreFilter, true ) )
							ResolvedRecordFilterCollection filters = dr.RecordManager.RecordFiltersResolved;
							if ( filters.MeetsCriteria( dr, _ignoreFilter, true ) ?? true )
								return true;
						}
					}

					return false;
				}

				#endregion // MeetsCriteria
			}

			#endregion // MeetsCriteria_Filter Class

			#endregion // Nested Data Structures

			#region Member Vars

			private ResolvedRecordFilterCollection _filters;
			private Field _field;
			private RecordManager _recordManager;
			private PropertyValueTracker _pvtList;

			private RecordFilter _cachedRecordFilter;
			private bool _cachedHasActiveFilters;

			// AS 8/4/09 NA 2009.2 Field Sizing
			private int _cachedFilterVersion;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="filters">Resolved record filter collection.</param>
			/// <param name="field">Field.</param>
			internal FieldFilterInfo( ResolvedRecordFilterCollection filters, Field field )
			{
				GridUtilities.ValidateNotNull( filters );
				GridUtilities.ValidateNotNull( field );

				_filters = filters;
				_field = field;
				_recordManager = filters._recordManager;

				PropertyValueTracker pvt;

				_pvtList = pvt = new PropertyValueTracker( filters,
					ResolvedRecordFilterCollection.VersionProperty, this.OnFiltersChanged );

				pvt = (PropertyValueTracker)pvt.Tag;
			}

			#endregion // Constructor

			#region Properties

			#region Public Properties

			#region HasActiveFilters

			public bool HasActiveFilters
			{
				get
				{
					this.VerifyHasActiveFilters( );

					return _cachedHasActiveFilters;
				}
			}

			#endregion // HasActiveFilters

			#region RecordFilter

			public RecordFilter RecordFilter
			{
				get
				{
					this.VerifyRecordFilter( );

					return _cachedRecordFilter;
				}
			}

			private void VerifyRecordFilter( )
			{
				RecordFilter oldFilter = _cachedRecordFilter;

				// SSP 5/10/12 TFS111127
				// If the field is removed from the fields collection then don't allocate a record filter for it.
				// 
				//_cachedRecordFilter = _filters.GetItem( _field, true );
				_cachedRecordFilter = _field.Index < 0 && null != oldFilter ? oldFilter : _filters.GetItem( _field, true );

				if ( oldFilter != _cachedRecordFilter )
				{
					this.RaisePropertyChangedEvent( "RecordFilter" );

					this.VerifyHasActiveFilters( );
				}

				// AS 8/4/09 NA 2009.2 Field Sizing
				// While the above would have handled the case where a new RecordFilter was 
				// associated with a field it wouldn't help when the record filter itself changed.
				// Usually we don't need to worry about that but with autosizing mode and a scope 
				// of all records, the filter record could be out of view and so we need to notify 
				// the filtercell so it can get remeasured. Since this method will be called whenever 
				// any filter is dirtied, we'll cache the recordfilter's version and only raise this 
				// when this recordfilter has been modified.
				//
				int version = _cachedRecordFilter != null ? _cachedRecordFilter.Version : -1;
				if (version != _cachedFilterVersion)
				{
					_cachedFilterVersion = version;
					this.RaisePropertyChangedEvent( "RecordFilterVersion" );
				}
			}

			#endregion // RecordFilter

			#endregion // Public Properties

			#region Private/Internal Properties

			#region Field

			internal Field Field
			{
				get
				{
					return _field;
				}
			}

			#endregion // Field

			// JJD 06/29/10 - TFS32174 - added
			#region Filters

			internal ResolvedRecordFilterCollection Filters { get { return this._filters; } }

			#endregion //Filters	
    
			#region RecordManager

			internal RecordManager RecordManager
			{
				get
				{
					return _recordManager;
				}
			}

			#endregion // RecordManager

			#endregion // Private/Internal Properties

			#endregion // Properties

			#region Methods

			#region Private/Internal Methods

    #region Old code
    
    		
#region Infragistics Source Cleanup (Region)



























































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


   	#endregion //Old code	
    
			#region OnFiltersChanged

			/// <summary>
			/// Called when a record filters are changed. We need to update RecordFilter property.
			/// </summary>
			private void OnFiltersChanged( )
			{
				this.VerifyRecordFilter( );
				this.VerifyHasActiveFilters( );
			}

			#endregion // OnFiltersChanged

			#region VerifyHasActiveFilters

			private void VerifyHasActiveFilters( )
			{
				bool oldValue = _cachedHasActiveFilters;

				RecordFilter recordFilter = this.RecordFilter;
				_cachedHasActiveFilters = null != recordFilter && recordFilter.HasConditions;

				if ( oldValue != _cachedHasActiveFilters )
				{
					this.RaisePropertyChangedEvent( "HasActiveFilters" );
				}
			}

			#endregion // VerifyHasActiveFilters

			#endregion // Private/Internal Methods

			#endregion // Methods
		}

		#endregion // FieldFilterInfo Class

		#region FilterEvaluationContextCache Class

		private class FilterEvaluationContextCache
		{
			private WeakDictionary<Field, FilterConditionEvaluationContext> _cachedEvaluationContexts;
			private ResolvedRecordFilterCollection _info;
			internal bool _hasAnyContextUsedAllValues = false;

			internal FilterEvaluationContextCache( ResolvedRecordFilterCollection info )
			{
				_info = info;
			}

			#region CreateEvaluationContext

			internal static FilterConditionEvaluationContext CreateEvaluationContext( RecordManager recordManager, Field field )
			{
				IEnumerable<DataRecord> allValuesRecords =
					GridUtilities.Filter<DataRecord>( recordManager.Unsorted, new GridUtilities.MeetsCriteria_RecordVisible( true ) );

				return new FilterConditionEvaluationContext( field, allValuesRecords );
			}

			#endregion // CreateEvaluationContext

			#region GetCachedEvaluationContext



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            internal FilterConditionEvaluationContext GetCachedEvaluationContext( Field field, bool create )
			{
				FilterConditionEvaluationContext ret = null;
				if ( null != _cachedEvaluationContexts && _cachedEvaluationContexts.TryGetValue( field, out ret ) )
					return ret;

				if ( create )
				{
					if ( null == _cachedEvaluationContexts )
						_cachedEvaluationContexts = new WeakDictionary<Field, FilterConditionEvaluationContext>( true, false );

					ret = CreateEvaluationContext( _info._recordManager, field );
					_cachedEvaluationContexts[field] = ret;
				}

				return ret;
			}

			#endregion // GetCachedEvaluationContext

			#region MaintainCacheForUsesAllValues

			internal void MaintainCacheForUsesAllValues( FilterConditionEvaluationContext context )
			{
				if ( context.WasAllValuesUsed )
					_hasAnyContextUsedAllValues = true;
			}

			#endregion // MaintainCacheForUsesAllValues
		}

		#endregion // FilterEvaluationContextCache Class

		// JJD 06/29/10 - TFS32174 - added
		#region FilterDropDownItemLoader internal class

		internal class FilterDropDownItemLoader
		{
			#region Member Variables

			private FieldFilterInfo _filterInfo;
			private IEnumerator<DataRecord> _recordEnumerator;
			private ObservableCollectionExtended<FilterDropDownItem> _items;
			private List<FilterDropDownItem> _phase1Items;
			private List<FilterDropDownItem> _phase2Items;
			private CellTextConverterInfo _cellTextConverter;
			private DispatcherTimer _timer;
			private CultureInfo _fieldConversionCulture;
			private int _asynchLoadDuration = RecordFilterDropDownPopulatingEventArgs.DefaultAsynchLoadDuration;
			private int _asynchLoadInterval		= RecordFilterDropDownPopulatingEventArgs.DefaultAsynchLoadInterval;
			private int _asynchLoadThreshhold	= RecordFilterDropDownPopulatingEventArgs.DefaultAsynchLoadThreshhold;
			private HashSet _uniqueCellValues = new HashSet();
			private Dictionary<FilterDropDownItem, string> _itemToOriginalTextMap = new Dictionary<FilterDropDownItem, string>();
			private bool _endReached;
			private bool _aborted;
			private bool _forCustomFilterSelectionControl;
			private bool _uniqueValuesLoadPending;
			private bool _includeFilteredOutValues;
			private XamComboEditor _editor;
			private FieldFilterInfo.FilterDropDownItemComparer _comparer;

			// AS - NA 11.2 Excel Style Filtering
			private FieldMenuDataItem _rootMenuItem;
			private bool _hasNullValues;

			#endregion //Member Variables	
    
			#region Constructor

			internal FilterDropDownItemLoader(XamComboEditor editor,
				FieldFilterInfo filterInfo,
				ObservableCollectionExtended<FilterDropDownItem> items,
				bool forCustomFilterSelectionControl)
			{
				Debug.Assert(null != filterInfo, "We need a filterinfo!");

				this._editor = editor;
				this._items = items;
				this._forCustomFilterSelectionControl = forCustomFilterSelectionControl;
				this._filterInfo = filterInfo;
				
				this._uniqueCellValues = new HashSet();

				// AS - NA 11.2 Excel Style Filtering
				if (items == null)
					_rootMenuItem = new FieldMenuDataItem();
			}

			#endregion //Constructor

			#region Events

			// AS - NA 11.2 Excel Style Filtering
			#region Phase1Completed
			internal event EventHandler Phase1Completed;

			private void RaisePhase1Completed()
			{
				var handler = this.Phase1Completed;

				if (null != handler)
					handler(this, EventArgs.Empty);
			}
			#endregion //Phase1Completed

			#region Phase2Completed

			internal event EventHandler Phase2Completed;

			private void RaisePhase2Completed()
			{
				// Null out _cellInfo of all the filter drop-down items that were used during loading.
				//
				int count = this._phase2Items != null ? this._phase2Items.Count : 0;

				for (int i = 0; i < count; i++)
					this._phase2Items[i]._cellInfo = null;

				if (this._timer != null)
				{
					this._timer.Stop();
					this._timer = null;

					if (this._editor != null)
						this._editor.ClearValue(XamComboEditor.PostDropDownAreaTemplateProperty);
				}

				if (this.Phase2Completed != null)
					this.Phase2Completed(this, EventArgs.Empty);
			}

			#endregion //Phase2Completed

			// AS - NA 11.2 Excel Style Filtering
			#region Phase2Updated
			internal event EventHandler Phase2Updated;

			private void RaisePhase2Updated()
			{
				var handler = this.Phase2Updated;

				if (null != handler)
					handler(this, EventArgs.Empty);
			} 
			#endregion //Phase2Updated

			#endregion //Events	
        
			#region Properties

			#region Aborted

			internal bool Aborted { get { return this._aborted; } }

			#endregion //Aborted
    
			#region Editor

			internal XamComboEditor Editor 
			{ 
				get { return this._editor; }

				set
				{
					if (value != this._editor)
					{
						if (this._editor != null)
							this._editor.ClearValue(XamComboEditor.PostDropDownAreaTemplateProperty);

						this._editor = value;
						
						if (this._editor != null && 
							this._endReached == false && 
							this._aborted == false)
							this._editor.SetResourceReference(XamComboEditor.PostDropDownAreaTemplateProperty, DataPresenterBase.FilterDropDownLoadingTemplateKey);
					}
				}
			}

			#endregion //EndReached
    
			#region EndReached

			internal bool EndReached { get { return this._endReached; } }

			#endregion //EndReached

			// AS - NA 11.2 Excel Style Filtering
			#region FilterInfo
			internal FieldFilterInfo FilterInfo
			{
				get { return _filterInfo; }
			} 
			#endregion //FilterInfo

			// AS - NA 11.2 Excel Style Filtering
			#region HasNullValues
			internal bool HasNullValues
			{
				get { return _hasNullValues; }
			} 
			#endregion //HasNullValues

			// AS - NA 11.2 Excel Style Filtering
			#region Phase2Items
			internal IList<FilterDropDownItem> Phase2Items
			{
				get { return _phase2Items; }
			} 
			#endregion //Phase2Items

			// AS - NA 11.2 Excel Style Filtering
			#region RootMenuItem
			internal FieldMenuDataItem RootMenuItem
			{
				get { return _rootMenuItem; }
			} 
			#endregion //RootMenuItem

			#region UniqueValuesLoadPending

			internal bool UniqueValuesLoadPending { get { return this._uniqueValuesLoadPending; } }

			#endregion //UniqueValuesLoadPending	

			#endregion //Properties	
        
			#region Methods

			#region Public Methods

			#region Abort

			internal void Abort()
			{
				this._aborted = true;

				this.RaisePhase2Completed();
			}

			#endregion //Abort
			
			#region PopulatePhase1

			internal void PopulatePhase1()
			{
				Debug.Assert ( this._phase1Items == null, "Phase 1 already loaded");

				// AS - NA 11.2 Excel Style Filtering
				// Added logic in case we are going to show a menu.
				//
				if (_rootMenuItem != null)
				{
					this.InitializeFilterMenuItems();
				}
				else
				{
					this._phase1Items = new List<FilterDropDownItem>();

					this.GetFilterDropDownSpecialItems(this._phase1Items, this._forCustomFilterSelectionControl);
				}

				FieldLayout fieldLayout = this._filterInfo.Field.Owner;
				DataPresenterBase dataPresenter = null != fieldLayout ? fieldLayout.DataPresenter : null;

				// AS - NA 11.2 Excel Style Filtering
				//RecordFilterDropDownPopulatingEventArgs eventArgs = new RecordFilterDropDownPopulatingEventArgs(this._filterInfo.Field, this._filterInfo.RecordManager, this._phase1Items, true, this._forCustomFilterSelectionControl);
				RecordFilterDropDownPopulatingEventArgs eventArgs;

				if (_rootMenuItem == null)
					eventArgs = new RecordFilterDropDownPopulatingEventArgs(this._filterInfo.Field, this._filterInfo.RecordManager, this._phase1Items, true, this._forCustomFilterSelectionControl);
				else
					eventArgs = new RecordFilterDropDownPopulatingEventArgs(_filterInfo.Field, _filterInfo.RecordManager, _rootMenuItem.Items, true, _forCustomFilterSelectionControl); 

				if (null != dataPresenter)
				{
					dataPresenter.RaiseRecordFilterDropDownPopulating(eventArgs);
					this._uniqueValuesLoadPending	= eventArgs.IncludeUniqueValues;
					this._asynchLoadDuration	= eventArgs.AsynchLoadDuration;
					this._asynchLoadInterval	= eventArgs.AsynchLoadInterval;
					this._asynchLoadThreshhold	= eventArgs.AsynchLoadThreshhold;

					// AS - NA 11.2 Excel Style Filtering
					if (!_uniqueValuesLoadPending && _rootMenuItem != null)
					{
						this.RemoveTreeMenuItem();
					}
				}
				else
					this._uniqueValuesLoadPending = true;

				// AS - NA 11.2 Excel Style Filtering
				// The items are optional.
				//
				//this._items.BeginUpdate();
				//this._items.Clear();
				//this._items.AddRange(this._phase1Items);
				//this._items.EndUpdate();
				if (_items != null && _phase1Items != null)
					_items.ReInitialize(_phase1Items);

				// AS - NA 11.2 Excel Style Filtering
				this.RaisePhase1Completed();
			}

			#endregion //PopulatePhase1	
			
			#region PopulatePhase2

			internal void PopulatePhase2(bool includeFilteredOutValues)
			{
				// AS - NA 11.2 Excel Style Filtering
				//if (this._phase1Items == null)
				if (this._phase1Items == null && this._rootMenuItem == null)
					this.PopulatePhase1();

				Debug.Assert(this._phase2Items == null, "Phase 2 already started");

				// SSP 2/15/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// Moved this here from below.
				// 
				this._cellTextConverter = CellTextConverterInfo.GetCachedConverter( this._filterInfo.Field );

				// SSP 2/15/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// 
				// ------------------------------------------------------------------------------------------------
				if ( _uniqueValuesLoadPending && null != _filterInfo && ! _filterInfo.Field.IsUnbound
					&& FilterEvaluationMode.UseCollectionView == _filterInfo.Field.Owner.FilterEvaluationModeResolved )
				{

					LinqQueryManager lqm = VUtils.CreateLinqQueryManager( _filterInfo.RecordManager, true );

					if ( null != lqm )
					{
						LinqQueryManager.LinqInstructionSelect select =
							new LinqQueryManager.LinqInstructionSelect( VUtils.GetPropertyName( _filterInfo.Field ), null );

						LinqQueryManager.LinqInstructionDistinct distinct = new LinqQueryManager.LinqInstructionDistinct( select );

						LinqQueryManager.LinqInstructionOrderBy orderedValues = new LinqQueryManager.LinqInstructionOrderBy( null, distinct, false );

						DataItemFilterEvaluator dataItemEvaluator = new DataItemFilterEvaluator( _filterInfo.Filters, true, _filterInfo.RecordFilter );
						IEnumerable list = lqm.List;
						if ( ! includeFilteredOutValues )
							list = ( list as IEnumerable<object> ?? new TypedEnumerable<object>( list ) ).Where( dataItemEvaluator.FilterDataItem );

						IEnumerable distinctValues = lqm.PerformQuery( list, orderedValues );

						_phase2Items = new List<FilterDropDownItem>( );

						foreach ( object ii in distinctValues )
						{
							FilterDropDownItem item = new FilterDropDownItem( ii, _cellTextConverter.ConvertCellValue( ii ), true );
							_phase2Items.Add( item );
						}

						_items.AddRange( _phase2Items );

						_uniqueValuesLoadPending = false;
					}



				}
				// ------------------------------------------------------------------------------------------------

				if (!this._uniqueValuesLoadPending)
				{
					this._endReached = true;
					this.RaisePhase2Completed();
					return;
				}
				
				this._comparer = new FieldFilterInfo.FilterDropDownItemComparer(this._filterInfo);
				this._fieldConversionCulture = GridUtilities.GetDefaultCulture(this._filterInfo.Field);
				this._includeFilteredOutValues = includeFilteredOutValues;

				// Get the data records from which we need to populate the filter drop-down with cell values.
				// 
				this._recordEnumerator = this.GetRecordsForFilterDropDownCellValues(this._includeFilteredOutValues, true).GetEnumerator();

				// AS - NA 11.2 Excel Style Filtering
				this._hasNullValues = false;

				// SSP 6/16/09 - TFS18467
				// If a combo editor is being used to map values then the filtering should be done on the
				// mapped values, not the underlying values, because the mapped values are what the end
				// user sees.
				// 
				// ----------------------------------------------------------------------------------------------
				// SSP 2/15/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// Moved this above.
				// 
				//this._cellTextConverter = CellTextConverterInfo.GetCachedConverter(this._filterInfo.Field);

				this._phase2Items = new List<FilterDropDownItem>();

				this.ProcessPhase2Items(this._asynchLoadThreshhold);
			}

			#endregion //PopulatePhase2

			#endregion //Public Methods	
        
			#region Private Methods

			#region AddFilterItemHelper

			private void AddFilterItemHelper(List<FilterDropDownItem> items, ICommand action, string srDisplayText)
			{
				// JJD 2/4/09 - TFS13465
				// Append a couple of spaces to the end of the string. The reason for this is
				// so the string shown in the combo's edit portion in a FilterCellValuePresenter
				// is less likely to match this string representing the command in the operands list.
				// If the string does match then when the user drops down the list and selects
				// the command we won't get a value change notification from the combo and
				// therefore we won't execute the command.
				//items.Add( new FilterDropDownItem( action, SR.GetString( srDisplayText ) ) );
				items.Add(new FilterDropDownItem(action, DataPresenterBase.GetString(srDisplayText) + "  "));
			}

			private void AddFilterItemHelper(List<FilterDropDownItem> items, string operandName)
			{
				SpecialFilterOperandBase operand = SpecialFilterOperands.GetRegisteredOperand(operandName);
				if (null != operand)
					this.AddFilterItemHelper(items, operand);
			}

			private void AddFilterItemHelper(List<FilterDropDownItem> items, SpecialFilterOperandBase operand)
			{
				if (null != operand)
				{
					// SSP 3/23/10 TFS29800
					// We need to use the Language to format operand month names. Changed the FilterDropDownItem
					// constructor that took in special operand to also take in the display text so we have to
					// provide it here.
					// 
					//items.Add( new FilterDropDownItem( operand ) );
					string operandDisplayText = GridUtilities.ToString(operand.DisplayContent, operand.Name, GridUtilities.GetDefaultCulture(this._filterInfo.Field));
					items.Add(new FilterDropDownItem(operand, operandDisplayText));
				}
			}

			#endregion // AddFilterItemHelper

			#region CheckForDuplicateDisplayText

			private bool CheckForDuplicateDisplayText(HashSet duplicateDisplayTexts, FilterDropDownItem item, FilterDropDownItem nextItem)
			{
				// Since the list is sorted by values, any duplicate display texts will be
				// next to each other.
				//
				string displayText = this.GetOriginalDisplayText(item);
				string nextItemDisplayText = this.GetOriginalDisplayText(nextItem);

				if (displayText == nextItemDisplayText)
				{
					duplicateDisplayTexts.Add(item);
					duplicateDisplayTexts.Add(nextItem);

					this._itemToOriginalTextMap[item] = displayText;
					this._itemToOriginalTextMap[nextItem] = displayText;

					return true;
				}

				return false;
			}

			#endregion //CheckForDuplicateDisplayText	
    
			#region GetFilterDropDownSpecialItems

			private void GetFilterDropDownSpecialItems(List<FilterDropDownItem> items, bool forCustomFilterSelectionControl)
			{
				FieldLayout fieldLayout = this._filterInfo.Field.Owner;
				bool forFilterRecord = null != fieldLayout && FilterUIType.FilterRecord == fieldLayout.FilterUITypeResolved;

				// Don't show (All) item when using filter record since there's a clear button for it.
				// 
				if (!forFilterRecord && !forCustomFilterSelectionControl )
					this.AddFilterItemHelper(items, new ClearFilterAction(this._filterInfo), "SR_FilterDropDownItem_All");

				if (!forCustomFilterSelectionControl)
					this.AddFilterItemHelper(items, new CustomFilterAction(this._filterInfo), "SR_FilterDropDownItem_Custom");

				foreach (SpecialFilterOperandBase operand in SpecialFilterOperands.GetRegisteredOperands(this._filterInfo.Field.EditAsTypeResolved))
				{
					this.AddFilterItemHelper(items, operand);
				}
			}

			#endregion // GetFilterDropDownSpecialItems

			#region GetOriginalDisplayText

			private string GetOriginalDisplayText(FilterDropDownItem item)
			{
				string originalText;
				if (this._itemToOriginalTextMap.TryGetValue(item, out originalText))
					return originalText;

				return item.DisplayText;
			}

			#endregion //GetOriginalDisplayText

			#region GetRecordsForFilterDropDownCellValues

			internal IEnumerable<DataRecord> GetRecordsForFilterDropDownCellValues(bool includeFilteredOutValues, bool ignoreFieldFilter)
			{
				FieldLayout fieldLayout = _filterInfo.Field.Owner;
				RecordFilterScope filterScope = _filterInfo.Filters.GetRecordFilterScopeResolved(fieldLayout);

				// Depending on the record filter scope, get the record manager's records or data presenter's
				// root records.
				// 
				RecordCollectionBase recordColl = RecordFilterScope.SiblingDataRecords == filterScope
					? this._filterInfo.RecordManager.Unsorted as RecordCollectionBase
					: this._filterInfo.RecordManager.DataPresenter.RecordManager.Unsorted as RecordCollectionBase;

				// Get all data records that are trivial descendants of the recordColl that belong to the field layout.
				// 
				IEnumerable<Record> records = recordColl.GetRecordEnumerator(RecordType.DataRecord, fieldLayout, fieldLayout, false);

				// Filter out records that not visible or are filtered out based on the includeFilteredOutValues and 
				// ignoreFieldFilter parameters.
				// 
				records = GridUtilities.Filter<Record>(records, new FieldFilterInfo.MeetsCriteria_Filter(this._filterInfo, includeFilteredOutValues, this._filterInfo.RecordFilter));

				// Return the data records from which the filter drop-down will be populated with unique cell values.
				// 
				return new TypedEnumerable<DataRecord>(records);
			}

			#endregion // GetRecordsForFilterDropDownCellValues

			// AS - NA 11.2 Excel Style Filtering
			#region InitializeFilterMenuItems
			private void InitializeFilterMenuItems()
			{
				_rootMenuItem.Items.Clear();

				FieldMenuItemProvider menuProvider = new FieldMenuItemProvider(_rootMenuItem, _filterInfo);

				// clear filter for the associated field
				menuProvider.AddClearFilterMenuItem(_rootMenuItem);

				// data type specific menu of filter options
				_rootMenuItem.Items.Add(menuProvider.CreateDataTypeFilterMenuItem(_filterInfo.Field.EditAsTypeResolved));

				FieldMenuItemProvider.AddSeparator(_rootMenuItem);

				// multi select custom filter list
				RecordFilterTreeControl tree = new RecordFilterTreeControl();
				tree.ValueLoader = this;
				tree.HeightInInfiniteContainers = 230d;
				tree.WidthInInfiniteContainers = 230d;
				_rootMenuItem.Items.Add(new FieldMenuDataItem { Header = tree, IsResizable = true, StaysOpenOnClick = true, IsControlHost = true  });
			}
			#endregion //InitializeFilterMenuItems

			#region OnTimerTick

			private void OnTimerTick(object sender, EventArgs e)
			{
				this._timer.Stop();
				this.ProcessPhase2Items(this._asynchLoadDuration);
			}

			#endregion //OnTimerTick

			#region ProcessPhase2Items

			private void ProcessPhase2Items(int maxMs)
			{
				if (this._aborted)
					return;

				// Store unique cell values and texts associated with them.
				// 
				
				
				

				int startTicks = Environment.TickCount;
				int recordProcessCount = 0;
				bool exception = false;
				List<FilterDropDownItem> newItems = new List<FilterDropDownItem>(2000);

				while (true)
				{
					#region Process next record

					if (recordProcessCount > 0 && startTicks + maxMs < Environment.TickCount)
						break;

					try
					{
						this._endReached = !this._recordEnumerator.MoveNext();
					}
					catch (Exception)
					{
						exception = true;
						break;
					}

					if (this._endReached)
					{
						break;
					}

					recordProcessCount++;

					DataRecord dr = this._recordEnumerator.Current;

					// SSP 6/16/09 - TFS18467
					// If a combo editor is being used to map values then the filtering should be done on the
					// mapped values, not the underlying values, because the mapped values are what the end
					// user sees.
					// 
					// ----------------------------------------------------------------------------------------------
					string cellText;

					// JJD 06/29/10 - TFS32174 - Optimization
					// Pass in false for getText param since we want to delay that
					// to see if the value is already in our uniqueCellValues cache below. 
					//object cellValue = FilterConditionEvaluationContext.GetCellValueForFilterComparisonHelper(
					//    this._cellTextConverter, dr, true, out cellText);
					object cellValue = FilterConditionEvaluationContext.GetCellValueForFilterComparisonHelper(
						this._cellTextConverter, dr, false, out cellText);
					
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

					// ----------------------------------------------------------------------------------------------

					// Skip null/dbnull values. For such blank values, we have (Blanks) and (NonBlanks) entries
					// in the filter drop-down.
					// 
					// JJD 06/29/10 - TFS32174 - Optimization
					// Only check the value since we haven't gotten the text yet 
					//if (GridUtilities.IsNullOrEmpty(cellValue) || GridUtilities.IsNullOrEmpty(cellText))
					if (GridUtilities.IsNullOrEmpty(cellValue))
					{
						_hasNullValues = true; // AS - NA 11.2 Excel Style Filtering
						continue;
					}

					if (!this._uniqueCellValues.Exists(cellValue))
					{
						// JJD 06/29/10 - TFS32174 - Optimization
						// Now that we know that the value hasn't been processed get
						// the text but only if CompareByText is false. Otherwise
						// the value should be the text already
						if (this._cellTextConverter.CompareByText)
							cellText = cellValue as string;
						else
							cellText = this._cellTextConverter.ConvertCellValue(cellValue);

						if (GridUtilities.IsNullOrEmpty(cellText))
							continue;

						this._uniqueCellValues.Add(cellValue);

						FilterDropDownItem item = new FilterDropDownItem(cellValue, cellText, true);
						newItems.Add(item);

						// Temporarily store the cellinfo on the item. This is used by the 
						// FilterDropDownItemComparer. We clear it later when processing is done.
						// 
						item._cellInfo = this._comparer._fieldSortInfo.GetCellInfo(dr);
					}

					#endregion //Process next record
				}

				int count = newItems.Count;

				if (count > 0)
				{
					#region Setup

					HashSet duplicateDisplayTexts = new HashSet();
					int lastCount = this._phase2Items.Count;


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


					#endregion //Setup	
    
					#region Sort the new items

					// Sort the cell values alphabetically.
					// 
					Utilities.SortMergeGeneric<FilterDropDownItem>(newItems, this._comparer);


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


					#endregion //Sort the new items	


					if (lastCount == 0)
					{
						#region Process initial batch

						this._phase2Items.AddRange(newItems);

						// This is for taking into account situations where different cell values could have
						// the same display texts. This can occur for example if there was formatting on DateTime 
						// field that truncated time portion. Two different DateTime instances with the same date 
						// but different times would result in the same display texts. Likewise with numeric values
						// where fraction portions are truncated to certain number of digits. In such cases, append
						// to the display text the value converted to text without formatting applied.
						// 
						int countMinusOne = count - 1;

						for (int i = 0; i < countMinusOne; i++)
						{
							FilterDropDownItem item = this._phase2Items[i];
							FilterDropDownItem nextItem = this._phase2Items[1 + i];

							this.CheckForDuplicateDisplayText(duplicateDisplayTexts, item, nextItem);
						}

						#endregion //Process initial batch
					}
					else
					{
						#region Find to insert locations

						// allocate an array to hold the insert locations
						int[] insertLocations = new int[count];
						int insertLocation = 0;

						for (int i = 0; i < count; i++)
						{
							FilterDropDownItem itemToInsert = newItems[i];

							#region Find the location via a binary search

							// call BinarySearch 
							insertLocation = this._phase2Items.BinarySearch(itemToInsert, this._comparer);

							// the item shouldn't be in the list in which case it returns the complement of
							// the index to insert at so we need to teke the complement again to get the
							// usable index
							if (insertLocation < 0)
								insertLocation = ~insertLocation;

							Debug.Assert(insertLocation >= 0 && insertLocation <= lastCount, "invalid insert location");

							if (insertLocation < 0)
								insertLocation = 0;
							else
							{
								if (insertLocation > lastCount)
									insertLocation = lastCount;
							}

							#endregion //Find the location via a binary search

							insertLocations[i] = insertLocation;
						}

						#endregion //Find to insert locations	
 
						// expand the _phase2Items list with empty slots to ahandle the new items
						this._phase2Items.AddRange(new FilterDropDownItem[count]);

						#region Merge the new items in by processing th expanded list backward

						int delta = count;
						int locationIndex = count - 1;

						// JJD 07/10/12 - TFS116586
						// The correct insert location is determined by adding delta minus 1
						//int nextInsertLocation = insertLocations[locationIndex] + delta;
						int nextInsertLocation = insertLocations[locationIndex] + delta - 1;

						for (int i = this._phase2Items.Count - 1; i >= 0; i--)
						{
							if (i <= nextInsertLocation)
							{
								this._phase2Items[i] = newItems[locationIndex];

								// update the insertLocations[i] entry with the exact
								// slot so we can look for duplicates below more
								// efficiently
								insertLocations[locationIndex] = i;
								locationIndex--;
								delta--;

								if (delta == 0 || locationIndex < 0)
									break;

								// JJD 07/10/12 - TFS116586
								// The correct insert location is determined by adding delta minus 1
								//nextInsertLocation = insertLocations[locationIndex] + delta;
								nextInsertLocation = insertLocations[locationIndex] + delta - 1;
							}
							else
							{
								this._phase2Items[i] = this._phase2Items[i - delta];
							}
						}

						#endregion //Merge the new items in by processing th expanded list backward	
 
						#region Check the newly merged items to make sure the neighboring items don't have the same display text

						for (int i = 0; i < count; i++)
						{
							int index = Math.Min( insertLocations[i], this._phase2Items.Count - 2);

							// see if the djacent items have the same display text
							for (int j = Math.Max(index - 1, 0); j <= index; j++)
							{
								FilterDropDownItem item = this._phase2Items[j];
								FilterDropDownItem nextItem = this._phase2Items[1 + j];

								this.CheckForDuplicateDisplayText(duplicateDisplayTexts, item, nextItem);
							}
						}

						#endregion //Check the newly merged items to make sure the neighboring items don't have the same display text
					}

					#region Make duplicate text strings unique



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


					foreach (FilterDropDownItem item in duplicateDisplayTexts)
					{
						string s = (string)Utilities.ConvertDataValue(item.Value, typeof(string), this._fieldConversionCulture, null);

						// append the value to make the display text unique
						if (!string.IsNullOrEmpty(s))
							item.DisplayText += " (" + s + ")";
					}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


					#endregion //Make duplicate text strings unique	
    
					#region Update the items collection

					// AS - NA 11.2 Excel Style Filtering
					// It's better to add the items in bulk than 1 at a time.
					//
					//this._items.BeginUpdate();
					//
					//int phase1Count = this._phase1Items != null ? this._phase1Items.Count : 0;;
					//int phase2Count = this._phase2Items.Count;
					//int overallCount = phase1Count + phase2Count;
					//
					//// expand the list so there are enough slots
					//if ( this._items.Count < overallCount )
					//    this._items.AddRange( new FilterDropDownItem[overallCount - this._items.Count]);
					//
					//int idx = 0;
					//
					//// insert phase 1 items
					//for (int i = 0; i < phase1Count; i++)
					//{
					//    this._items[idx] = this._phase1Items[i];
					//    idx++;
					//}
					//
					//// insert phase 2 items
					//for (int i = 0; i < phase2Count; i++)
					//{
					//    this._items[idx] = this._phase2Items[i];
					//    idx++;
					//}
					if (null != _items)
					{
						this._items.BeginUpdate();

						IEnumerable<FilterDropDownItem> aggregate;
						if (_phase1Items == null)
							aggregate = _phase2Items;
						else
							aggregate = new CoreUtilities.AggregateEnumerable<FilterDropDownItem>(_phase1Items, _phase2Items);

						_items.Clear();
						_items.InsertRange(0, aggregate);



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


						this._items.EndUpdate();
					}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


					#endregion //Update the items collection
				}

				if (this._endReached)
				{
					this.RaisePhase2Completed();
				}
				else
				{
					if (exception == true)
					{
						// Re-get the enumerator and clear the _phase2Items so we can start over
						// 
						this._recordEnumerator = this.GetRecordsForFilterDropDownCellValues(this._includeFilteredOutValues, true).GetEnumerator();
						this._phase2Items.Clear();
						
						
						// also clear the _uniqueCellValues hashset
						this._uniqueCellValues.Clear();
					}

					if (this._timer == null)
					{
						if (this._editor != null)
							this._editor.SetResourceReference(XamComboEditor.PostDropDownAreaTemplateProperty, DataPresenterBase.FilterDropDownLoadingTemplateKey);
						
						this._timer = new DispatcherTimer(TimeSpan.FromMilliseconds(this._asynchLoadInterval), DispatcherPriority.Background, new EventHandler(this.OnTimerTick), Dispatcher.CurrentDispatcher);
					}

					this._timer.Start();

					// AS - NA 11.2 Excel Style Filtering
					this.RaisePhase2Updated();
				}
			}

			#endregion //Process

			// AS - NA 11.2 Excel Style Filtering
			#region RemoveTreeMenuItem
			private void RemoveTreeMenuItem()
			{
				if (_rootMenuItem == null)
					return;

				Predicate<FieldMenuDataItem> findTreeCallback = delegate(FieldMenuDataItem mi)
				{
					return mi.Header is RecordFilterTreeControl;
				};
				var ancestors = new List<FieldMenuDataItem>();
				var treeMenuItem = _rootMenuItem.Find(findTreeCallback, true, ancestors);

				if (null == treeMenuItem)
					return;

				var temp = treeMenuItem;

				for (int i = 0, count = ancestors.Count; i < count; i++)
				{
					var ancestor = ancestors[i];
					var ancestorItems = ancestor.Items;
					int tempIndex = ancestorItems.IndexOf(temp);

					ancestor.Items.RemoveAt(tempIndex);

					if (ancestorItems.Count > 0)
					{
						if (tempIndex == 0)
						{
							// remove trailing separator unless it has a header to
							// decorate the subsequent items
							if (ancestorItems[tempIndex].IsSeparator && ancestorItems[tempIndex].Header == null)
								ancestorItems.RemoveAt(tempIndex);
						}
						else if (tempIndex == ancestorItems.Count)
						{
							// remove leading separator
							if (ancestorItems[tempIndex - 1].IsSeparator)
								ancestorItems.RemoveAt(tempIndex - 1);
						}
						else if (ancestorItems[tempIndex].IsSeparator && ancestorItems[tempIndex - 1].IsSeparator)
						{
							// 2 consecutive separators - remove the leading one
							ancestorItems.RemoveAt(tempIndex - 1);
						}

						if (ancestorItems.Count > 0)
							break;
					}

					temp = ancestor;
				}
			}
			#endregion //RemoveTreeMenuItem

			#endregion //Private Methods

			#endregion //Methods
		}

		#endregion //FilterDropDownItemLoader private class

		#endregion // Nested Data Structures

		#region Member Vars

		// Key is the object and the value is the tracker.
		private WeakDictionary<FieldLayout, PropertyValueTracker> _pvtTrackers = new WeakDictionary<FieldLayout, PropertyValueTracker>( true, false );

		private RecordManager _recordManager;
		private int _filtersVersion;
		private FilterEvaluationContextCache _evaluationContextCache;

		// SSP 2/17/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		private DataItemFilterEvaluator _dataItemFilterEvaluator;

		// SSP 5/10/12 TFS111127
		// 
		private bool _needToVerifyRecordFilterFields;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="recordManager">This collection will resolve record filters for this record manager.</param>
		internal ResolvedRecordFilterCollection( RecordManager recordManager )
		{
			GridUtilities.ValidateNotNull( recordManager );
			_recordManager = recordManager;
			_evaluationContextCache = new FilterEvaluationContextCache( this );

			FieldLayout fl = recordManager.FieldLayout;
			if ( null != fl )
				this.HookInto( fl );
		}

		#endregion // Constructor

		#region Methods

		#region Private/Internal Methods

		#region ApplyFiltersToCollectionViewHelper

		// SSP 2/17/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		private void ApplyFiltersToCollectionViewHelper( )
		{
			if ( FilterEvaluationMode.UseCollectionView == _recordManager.FilterEvaluationModeResolved )
			{
				ICollectionView cv = VUtils.GetCollectionView( _recordManager );
				if ( null != cv && cv.CanFilter )
				{
					if ( null == _dataItemFilterEvaluator )
						_dataItemFilterEvaluator = new DataItemFilterEvaluator( this, false, null );

					if ( CoreUtilities.Antirecursion.Enter( _dataItemFilterEvaluator, "SettingFilter", true ) )
					{
						// If we have filter conditions then set the filter. Also clear the filter if we had set 
						// it previously and we don't have any filter conditions anymore.
						// 
						if ( this.HasActiveFiltersInAnyFieldLayout( ) )
							cv.Filter = _dataItemFilterEvaluator._filterPredicate;
						else if ( cv.Filter == _dataItemFilterEvaluator._filterPredicate )
							cv.Filter = null;
						
						CoreUtilities.Antirecursion.Exit( _dataItemFilterEvaluator, "SettingFilter" );
					}
				}
			}
		}

		#endregion // ApplyFiltersToCollectionViewHelper

		#region ClearCachedEvaluationContexts

		internal void ClearCachedEvaluationContexts( )
		{
			_evaluationContextCache = new FilterEvaluationContextCache( this );
		}

		#endregion // ClearCachedEvaluationContexts

		#region CreateEvaluationContext

		internal static FilterConditionEvaluationContext CreateEvaluationContext( RecordManager recordManager, Field field )
		{
			return FilterEvaluationContextCache.CreateEvaluationContext( recordManager, field );
		}

		#endregion // CreateEvaluationContext

		#region DoesFilterCriteriaUseAllValues

		// SSP 5/24/11 TFS76271
		// Refactored. Added DoesFilterCriteriaUseAllValues where the code in it is moved from OnDataChanged_DirtyFiltersHelper.
		// 
		internal bool DoesFilterCriteriaUseAllValues( Field fieldContext, DataRecord recordContext )
		{
			if ( null != fieldContext )
				return this.UsesAllValues( fieldContext );
			else if ( null != recordContext )
				return this.UsesAllValues( recordContext );

			return false;
		}

		#endregion // DoesFilterCriteriaUseAllValues

		#region GetItem

		/// <summary>
		/// Gets the RecordFilter associated with the specified field.
		/// </summary>
		/// <param name="field">Field for which to get the RecordFilter.</param>
		/// <param name="create">Specifies whether to allocate the record fitler if it hasn't already been allocated.</param>
		/// <returns>Returns the record filter for the specified field. If create is false and no RecordFilter has been allocated for
		/// the field then returns null.</returns>
		internal RecordFilter GetItem( Field field, bool create )
		{
			FieldLayout fieldLayout = field.Owner;
			Debug.Assert( null != fieldLayout );
			if ( null == fieldLayout )
				return null;

			RecordFilterCollection filters = this.GetRecordFilters( fieldLayout, create );

			return filters.GetItem( field, create );
		}

		#endregion // GetItem

		#region GetRecordFilters

		/// <summary>
		/// Returns the RecordFilters of the record manager or the field layout depending on the RecordFilterScope property setting.
		/// </summary>
		/// <param name="fieldLayout">Field layout.</param>
		/// <param name="create">Specifies whether to allocate the record fitler collection if it hasn't already been allocated.</param>
		/// <returns>Returns the RecordFilters of the record manager or the field layout depending on the RecordFilterScope 
		/// property setting. If create is false and the RecordFilters
		/// hasn't been allocated then returns null. Otherwise allocates a new one.</returns>
		internal RecordFilterCollection GetRecordFilters( FieldLayout fieldLayout, bool create )
		{
			// SSP 12/21/11 TFS67264 - Optimizations
			// Since we are always hooked into the record manager's field-layout, which also would happen
			// to be the one most frequenty queried in typical setups, skip calling HookInto method which
			// requires a dictionary lookup.
			// 
			// ------------------------------------------------------------------------------------------
			//this.HookInto( fieldLayout );
			bool isAlreadyHookedInto = fieldLayout == _recordManager.FieldLayout;
			if ( !isAlreadyHookedInto )
				this.HookInto( fieldLayout );
			// ------------------------------------------------------------------------------------------

			RecordFilterScope filterScope = this.GetRecordFilterScopeResolved( fieldLayout );
			if ( RecordFilterScope.SiblingDataRecords == filterScope )
			{
				if ( create )
					return _recordManager.RecordFilters;
				else
					return _recordManager.RecordFiltersIfAllocated;
			}
			else
			{
				if ( create )
					return fieldLayout.RecordFilters;
				else
					return fieldLayout.RecordFiltersIfAllocated;
			}
		}

		#endregion // GetRecordFilters

		#region HasActiveFilters

		/// <summary>
		/// Checks to see if any field from the specified field layout has active filter conditions.
		/// </summary>
		/// <param name="fieldLayout">Fields of this field layout will be checked for active filter conditions.</param>
		/// <returns>True if any field in the field layout has active filters.</returns>
		internal bool HasActiveFilters( FieldLayout fieldLayout )
		{
			RecordFilterCollection filters = this.GetRecordFilters( fieldLayout, false );
			if ( null != filters )
			{
				FieldCollection fields = GridUtilities.GetFields( fieldLayout );
				if ( null != fields )
				{
					for ( int i = 0, count = fields.Count; i < count; i++ )
					{
						Field field = fields[i];

						RecordFilter rf = filters.GetItem( field, false );
						if ( null != rf && rf.HasConditions )
							return true;
					}
				}
			}

			return false;
		}

		#endregion // HasActiveFilters

		#region HasActiveFiltersInAnyFieldLayout

		/// <summary>
		/// Checks to see if there are active filter conditions in any field of any field layout
		/// that this record filter collection has been queried for filters (via GetFilters method).
		/// </summary>
		/// <returns></returns>
		internal bool HasActiveFiltersInAnyFieldLayout( )
		{
			foreach ( FieldLayout fieldLayout in _pvtTrackers.Keys )
			{
				if ( this.HasActiveFilters( fieldLayout ) )
					return true;
			}

			return false;
		}

		#endregion // HasActiveFiltersInAnyFieldLayout

		#region HookInto

		/// <summary>
		/// Hooks into the specified field layout to monitor for change in record filters so any 
		/// applicable changes can be reflected by this collection.
		/// </summary>
		/// <param name="fieldLayout">The field layout to hook into.</param>
		private void HookInto( FieldLayout fieldLayout )
		{
			if ( !_pvtTrackers.ContainsKey( fieldLayout ) )
			{
				PropertyValueTracker t1 = new PropertyValueTracker( fieldLayout, "RecordFilters.Version", this.OnSourceFiltersChanged );
				_pvtTrackers.Add( fieldLayout, t1 );

				// SSP 5/10/12 TFS111127
				// 
				t1.Tag = new PropertyValueTracker( fieldLayout, "Fields.Version", this.OnFieldLayoutFieldsChanged );

				
				// By default the this collection's Version property returns 0 so the ViewableRecordCollection 
				// doesn't go through all the records and evaluate filters on them even if there are no filters
				// to begin with. However if a field layout has filters when it's hooked into, then bump the 
				// version number so the viewable record collection does initially evaluate them. The records 
				// will evaluate their filter state without this. However with this the viewable record 
				// collection will evaluate fitlers on all records instead of individual record doing it lazily. 
				// 
				RecordFilterCollection filters = fieldLayout.RecordFiltersIfAllocated;
				if ( null != filters && filters.Version > 0 )
					this.BumpVersion( );
			}
		}

		#endregion // HookInto

		#region MeetsCriteria

		/// <summary>
		/// Checks to see if the specified data record meets all the filter conditions.
		/// </summary>
		/// <param name="record">Data record to check for filter criteria.</param>
		/// <returns>Returns true if the record meets filter criteria, false if it doesn't meet filter criteria
		/// and null if there is no filter criteria.</returns>
		internal bool? MeetsCriteria( DataRecord record )
		{
			return this.MeetsCriteria( record, null, false );
		}

		/// <summary>
		/// Checks to see if the specified data record meets all the filter conditions.
		/// </summary>
		/// <param name="record">Data record to check for filter criteria.</param>
		/// <param name="ignoreFilter">Optional. Specifies a RecordFilter to ingore. This record filter will not be 
		/// considered for criteria matching.</param>
		/// <param name="populatingFilterDropDown">Indicates whether this is called from logic that populates filter drop-down list.</param>
		/// <returns>Returns true if the record meets filter criteria, false if it doesn't meet filter criteria
		/// and null if there is no filter criteria.</returns>
		private bool? MeetsCriteria( DataRecord record, RecordFilter ignoreFilter, bool populatingFilterDropDown )
		{
			// SSP 5/10/12 TFS111127
			// 
			if ( _needToVerifyRecordFilterFields )
				this.VerifyRecordFilterFieldsHelper( );

			FieldLayout fl = record.FieldLayout;
			Debug.Assert( null != fl );
			Debug.Assert( record.RecordManager == _recordManager );

			RecordFilterCollection filters = null != fl ? this.GetRecordFilters( fl, false ) : null;
			if ( null != filters )
			{
				// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// 
				if ( FilterEvaluationMode.Auto != fl.FilterEvaluationModeResolved )
					return null;

				LogicalOperator logicalOperator = fl.RecordFiltersLogicalOperatorResolved;

				FilterEvaluationContextCache contextsCache = populatingFilterDropDown 
					? new FilterEvaluationContextCache( this )
					: _evaluationContextCache;

				bool allMatched = true;
				int count = filters.Count;
				int evaluatedFilterCount = 0;

				for ( int i = 0; i < count; i++ )
				{
					RecordFilter filter = filters[i];
					if ( filter.HasConditions )
					{
						Field filterField = filter.Field;
						if ( ignoreFilter != filter && fl == GridUtilities.GetFieldLayout( filterField ) )
						{
							FilterConditionEvaluationContext context = contextsCache.GetCachedEvaluationContext( filterField, true );
							bool isMatch = filter.MeetsCriteria( record, context );
							evaluatedFilterCount++;

							// Call CacheUsesAllValuesHelper method so it maintains necessary cache for its
							// UsesAllValues methods to return the correct value.
							// 
							contextsCache.MaintainCacheForUsesAllValues( context );

							// If logical operator is 'Or' then return true even if a single filter
							// matches the record.
							// 
							if ( isMatch && LogicalOperator.Or == logicalOperator )
								return true;

							// JJD 1/7/11 - Optimization
							// If logical operator is 'And' then return false even if a single filter
							// doesn't match the record.
							// 
							if ( isMatch == false && LogicalOperator.And == logicalOperator )
								return false;

							allMatched = allMatched && isMatch;
						}
					}
				}

				if ( evaluatedFilterCount > 0 )
					return allMatched;
			}

			// If no filter criteria then return true.
			// 
			return null;
		}

		#endregion // MeetsCriteria

		#region OnFieldLayoutAdded

		/// <summary>
		/// Called when a FieldLayout is created and associated with the record manager.
		/// </summary>
		/// <param name="fieldLayout">Field layout.</param>
		internal void OnFieldLayoutAdded( FieldLayout fieldLayout )
		{
			this.HookInto( fieldLayout );
		}

		#endregion // OnFieldLayoutAdded

		#region OnFieldLayoutFieldsChanged

		// SSP 5/10/12 TFS111127
		// 
		/// <summary>
		/// When fields are removed from fields collection, we need to remove any corresponding filters
		/// from the field-layout.
		/// </summary>
		private void OnFieldLayoutFieldsChanged( )
		{
			if ( !_needToVerifyRecordFilterFields )
			{
				_needToVerifyRecordFilterFields = true;
				this.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new GridUtilities.MethodDelegate( this.VerifyRecordFilterFieldsHelper ) );
			}
		}

		private void VerifyRecordFilterFieldsHelper( )
		{
			if ( _needToVerifyRecordFilterFields )
			{
				_needToVerifyRecordFilterFields = false;
				bool bumpVersion = false;

				if ( null != _pvtTrackers )
				{
					foreach ( FieldLayout ii in _pvtTrackers.Keys )
					{
						RecordFilterCollection filters = this.GetRecordFilters( ii, false );
						if ( null != filters )
						{
							if ( filters.VerifyFields( ) )
								bumpVersion = true;
						}
					}
				}

				if ( bumpVersion )
					this.BumpVersion( );
			}
		}

		#endregion // OnFieldLayoutFieldsChanged

		#region OnSourceFiltersChanged

		/// <summary>
		/// Called when one of the source record filter collections is modified. A source record 
		/// filter collection is either a FieldLayout's RecordFilters or the RecordManager's RecordFilters.
		/// </summary>
		internal void OnSourceFiltersChanged( )
		{
			this.BumpVersion( );
		}

		#endregion // OnSourceFiltersChanged

		#region UsesAllValues

		/// <summary>
		/// Returns true if a filter condition used all values to base its filter criteria.
		/// </summary>
		/// <param name="field">This field's record filter will be checked for whether it used all values or not.</param>
		/// <returns></returns>
		internal bool UsesAllValues( Field field )
		{
			if ( _evaluationContextCache._hasAnyContextUsedAllValues )
			{
				FilterConditionEvaluationContext context = _evaluationContextCache.GetCachedEvaluationContext( field, false );
				if ( null != context )
					return context.WasAllValuesUsed;
			}

			return false;
		}

		/// <summary>
		/// Returns true if a filter condition used all values to base its filter criteria.
		/// </summary>
		/// <param name="record">Checks to see if this record has an active filter that used all values or not.</param>
		/// <returns></returns>
		internal bool UsesAllValues( DataRecord record )
		{
			return this.UsesAllValues( record.FieldLayout );
		}

		/// <summary>
		/// Returns true if a filter condition used all values to base its filter criteria.
		/// </summary>
		/// <param name="fieldLayout">Checks to see if a field from this field layout has a filter that used all values or not.</param>
		/// <returns></returns>
		internal bool UsesAllValues( FieldLayout fieldLayout )
		{
			if ( _evaluationContextCache._hasAnyContextUsedAllValues )
			{
				FieldCollection fields = GridUtilities.GetFields( fieldLayout );
				if ( null != fields )
				{
					for ( int i = 0, count = fields.Count; i < count; i++ )
					{
						if ( this.UsesAllValues( fields[i] ) )
							return true;
					}
				}
			}

			return false;
		}

		#endregion // UsesAllValues

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region Properties

		#region Private/Internal Properties

		#region Indexer [Field]

		/// <summary>
		/// Returns the resolved RecordFilter for the specified field. Note that a new RecordFilter will be allocated
		/// if none exists for the field.
		/// </summary>
		/// <param name="field">RecordFilter for this field will be returned.</param>
		/// <returns>RecordFilter for the specified field.</returns>
		internal RecordFilter this[Field field]
		{
			get
			{
				return this.GetItem( field, true );
			}
		}

		#endregion // Indexer [Field]

		#region GetRecordFilterScopeResolved

		/// <summary>
		/// Returns the resolved RecordFilterScope for the associated record manager and the 
		/// specified field layout.
		/// </summary>
		/// <param name="fl">Field layout</param>
		/// <returns>Resolved record filter scope</returns>
		internal RecordFilterScope GetRecordFilterScopeResolved( FieldLayout fl )
		{
			// For the root records, always use FieldLayout's RecordFilters since it
			// doesn't matter.
			// 
			if ( _recordManager.IsRootManager )
				return RecordFilterScope.AllRecords;

			RecordFilterScope scope = fl.RecordFilterScopeResolvedDefault;
			if ( RecordFilterScope.Default == scope
				// SSP 2/15/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// When using collection view for filtering, the scope needs to be sibling as
				// its impractical to apply filters across all collection views in a child field-layout.
				// 
				|| FilterEvaluationMode.UseCollectionView == fl.FilterEvaluationModeResolved
				)
				scope = RecordFilterScope.SiblingDataRecords;

			return scope;
		}

		#endregion // GetRecordFilterScopeResolved

		#region Version

		/// <summary>
		/// Identifies the property key for read-only <see cref="SummaryRecord"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey VersionPropertyKey = DependencyProperty.RegisterReadOnly(
			"Version",
			typeof( int ),
			typeof( ResolvedRecordFilterCollection ),
			new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="Version"/> dependency property.
		/// </summary>
		internal static readonly DependencyProperty VersionProperty = VersionPropertyKey.DependencyProperty;

		/// <summary>
		/// This version number is bumped every time this collection changes.
		/// </summary>
		internal int Version
		{
			get
			{
				return _filtersVersion;
			}
		}

		internal void BumpVersion( )
		{
			_filtersVersion++;

			// Clear any cached FilterEvaluationContexts to we create new ones. This will cause us to
			// update UsesAllValues information that we use to determine whether we need to re-evaluate
			// filters on all records when a single data record's value changes (for conditions like
			// AboveAverage).
			// 
			this.ClearCachedEvaluationContexts( );

			this.SetValue( VersionPropertyKey, _filtersVersion );

			// SSP 2/17/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
			// 
			this.ApplyFiltersToCollectionViewHelper( );
		}

		#endregion // Version

		#endregion // Private/Internal Properties

		#endregion // Properties
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