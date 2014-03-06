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

namespace Infragistics.Windows.DataPresenter
{
	#region FilterConditionEvaluationContextBase Class

	// AS - NA 11.2 Excel Style Filtering
	// Changed to a base class that doesn't have a datarecord for the current value.
	//
	//internal class FilterConditionEvaluationContext : ConditionEvaluationContext
	internal abstract class FilterConditionEvaluationContextBase : ConditionEvaluationContext
	{
		#region Nested Data Structures

		#region AllValuesEnumerable Class

		private class AllValuesEnumerable : IEnumerable<ValueEntry>
		{
			#region Enumerator Class

			private class Enumerator : IEnumerator<ValueEntry>
			{
				#region Member Vars

				private IEnumerator<DataRecord> _records;
				private Field _field;
				private FilterValueEntry _current;

				
				
				private FilterConditionEvaluationContextBase _context;

				#endregion // Member Vars

				#region Constructor

				
				
				
				internal Enumerator( FilterConditionEvaluationContextBase context, IEnumerator<DataRecord> records )
				{
					
					
					
					_context = context;
					_field = context._field;

					_records = records;

					this.Reset( );
				}

				#endregion // Constructor

				#region IEnumerator<ValueEntry> Members

				public ValueEntry Current
				{
					get
					{
						return _current;
					}
				}

				#endregion

				#region IDisposable Members

				public void Dispose( )
				{
				}

				#endregion

				#region IEnumerator Members

				object IEnumerator.Current
				{
					get
					{
						return this.Current;
					}
				}

				public bool MoveNext( )
				{
					if ( _records.MoveNext( ) )
					{
						// SSP 6/16/09 - TFS18467
						// If a combo editor is being used to map values then the filtering should be done on the
						// mapped values, not the underlying values, because the mapped values are what the end
						// user sees.
						// 
						//_current = new FilterValueEntry( _field, _records.Current );
						DataRecord dr = _records.Current;
						_current = new FilterValueEntry( _field, dr, _context.GetCellValueForFilterComparison( dr ) );

						return true;
					}

					_current = null;
					return false;
				}

				public void Reset( )
				{
					_current = null;
					_records.Reset( );
				}

				#endregion

				#region Clone

				internal Enumerator Clone( )
				{
					
					
					
					return new Enumerator( _context, _records );
				}

				#endregion // Clone
			}

			#endregion // Enumerator Class

			#region Member Vars

			private Enumerator _e;

			#endregion // Member Vars

			#region Constructor

			
			
			
			internal AllValuesEnumerable( FilterConditionEvaluationContextBase context, IEnumerable<DataRecord> records )
			{
				
				
				
				_e = new Enumerator( context, records.GetEnumerator( ) );
			}

			#endregion // Constructor

			#region IEnumerable<ValueEntry> Members

			public IEnumerator<ValueEntry> GetEnumerator( )
			{
				return _e.Clone( );
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator( )
			{
				return this.GetEnumerator( );
			}

			#endregion
		}

		#endregion // AllValuesEnumerable Class

		#region FilterValueEntryBase

		// AS - NA 11.2 Excel Style Filtering
		// Changed to a base class that doesn't reference a record.
		//
		//internal class FilterValueEntry : ValueEntry
		internal abstract class FilterValueEntryBase : ValueEntry
		{
			#region Member Vars

			private Field _field;
			// AS - NA 11.2 Excel Style Filtering
			//private DataRecord _record;

			
			
			
			private object _value;

			#endregion // Member Vars

			#region Constructor

			
			
			
			// AS - NA 11.2 Excel Style Filtering
			//internal FilterValueEntry( Field field, DataRecord record, object value )
			internal FilterValueEntryBase( Field field, object value )
			{
				GridUtilities.ValidateNotNull( field );
				// AS - NA 11.2 Excel Style Filtering
				//GridUtilities.ValidateNotNull( record );

				_field = field;
				// AS - NA 11.2 Excel Style Filtering
				//_record = record;

				
				
				_value = value;
			}

			#endregion // Constructor

			#region Properties

			#region Context

			// AS - NA 11.2 Excel Style Filtering
			//public override object Context
			//{
			//    get 
			//    {
			//        return _record.Cells[_field];
			//    }
			//}

			#endregion // Context

			#region Culture

			public override CultureInfo Culture 
			{
				get
				{
					return GetCulture( _field );
				}
			}

			#endregion // Culture

			// AS - NA 11.2 Excel Style Filtering
			#region Field
			public Field Field
			{
				get { return _field; }
			} 
			#endregion //Field

			#region Format

			public override string Format 
			{
				get
				{
					return GetFormat( _field );
				}
			}

			#endregion // Format

			#region Value

			public override object Value 
			{
				get
				{
					
					
					
					return _value;
					
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				}
			}

			#endregion // Value

			#endregion // Properties

			#region Methods

			#region GetCulture

			internal static CultureInfo GetCulture( Field field )
			{
				
				return GridUtilities.GetDefaultCulture( field );
			}

			#endregion // GetCulture

			#region GetFormat

			internal static string GetFormat( Field field )
			{
				
				return null;
			}

			#endregion // GetFormat

			#endregion // Methods
		}

		#endregion // FilterValueEntryBase

		#region FilterValueEntry
		// AS - NA 11.2 Excel Style Filtering
		// Moved the DataRecord specific part to this class.
		//
		internal class FilterValueEntry : FilterValueEntryBase
		{
			#region Member Vars

			private DataRecord _record;

			#endregion // Member Vars

			#region Constructor

			internal FilterValueEntry( Field field, DataRecord record, object value ) : base(field, value)
			{
				GridUtilities.ValidateNotNull( record );

				_record = record;
			}

			#endregion // Constructor

			#region Properties

			#region Context

			public override object Context
			{
				get 
				{
					return _record.Cells[this.Field];
				}
			}

			#endregion // Context

			#endregion // Properties
		}
		#endregion //FilterValueEntry

		#endregion // Nested Data Structures

		#region Member Vars

		private Field _field;
		// AS - NA 11.2 Excel Style Filtering
		//private DataRecord _record;
		private IEnumerable<DataRecord> _allValuesRecords;
		// AS - NA 11.2 Excel Style Filtering
		//private ValueEntry _currentValue;
		private IEnumerable<ValueEntry> _cachedAllValues;

		// SSP 6/16/09 - TFS18467
		// If a combo editor is being used to map values then the filtering should be done on the
		// mapped values, not the underlying values, because the mapped values are what the end
		// user sees.
		// 
		private object _currentCellValueForFilterComparison;

        // SSP 5/3/10 TFS25788
		// 
		private IComparer _filterComparer;

		// SSP 2/29/12 TFS89053
		// 
		private IFilterEvaluator _filterEvaluator;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterConditionEvaluationContext"/>.
		/// </summary>
		/// <param name="field">Values of this field are being evaluated.</param>
		/// <param name="allValuesRecords">Supplies the 'AllValues'.</param>
		// AS - NA 11.2 Excel Style Filtering
		//internal FilterConditionEvaluationContext( Field field, IEnumerable<DataRecord> allValuesRecords )
		protected FilterConditionEvaluationContextBase( Field field, IEnumerable<DataRecord> allValuesRecords )
		{
			GridUtilities.ValidateNotNull( field );
			_field = field;
			_allValuesRecords = allValuesRecords;

            // SSP 5/3/10 TFS25788
			// 
			_filterComparer = _field.FilterComparerResolved;

			// SSP 2/29/12 TFS89053
			// 
			_filterEvaluator = _field.FilterEvaluatorResolved;
		}

		#endregion // Constructor

		#region Methods

		#region Private/Internal Methods

		#region GetCellValueForFilterComparison

		// SSP 6/16/09 - TFS18467
		// If a combo editor is being used to map values then the filtering should be done on the
		// mapped values, not the underlying values, because the mapped values are what the end
		// user sees.
		// 

		internal object GetCellValueForFilterComparison( DataRecord dr )
		{
			CellTextConverterInfo info = CellTextConverterInfo.GetCachedConverter( _field );

			string discard;
			return GetCellValueForFilterComparisonHelper( info, dr, false, out discard );
		}

		internal static object GetCellValueForFilterComparisonHelper( 
			CellTextConverterInfo info, DataRecord dr, bool getText, out string cellText )
		{
			cellText = null;
			// SSP 7/17/09 TFS18466
			// Pass in true for useConverter.
			// 
			//object val = dr.GetCellValue( info.Field );
			// SSP 7/27/09 TFS19657
			// 
			//object val = dr.GetCellValue( info.Field, true );
			object val = dr.GetCellValue( info.Field, CellValueType.EditAsType );

			if ( info.CompareByText )
			{
				string text = info.ConvertCellValue( val );
				val = text;

				if ( getText )
					cellText = text;
			}
			else
			{
				// SSP 1/20/10 TFS33040
				// If the cells aren't displaying time portion then ignore the time portion when
				// evaluating filter conditions since the end user will be entering dates in the
				// filter criteria and any time portion of a cell value will fail to match it.
				// 
				if ( val is DateTime )
					val = info.GetVisiblePortion( (DateTime)val );

				if ( getText )
					cellText = info.ConvertCellValue( val );
			}

			return val;
		}

		#endregion // GetCellValueForFilterComparison

		#region GetValueTypeForQuantitativeFilterComparison

		// SSP 10/23/09 TFS23134
		// 
		internal static Type GetValueTypeForQuantitativeFilterComparison( Field field )
		{
			CellTextConverterInfo info = CellTextConverterInfo.GetCachedConverter( field );
			return null != info && info.CompareByText ? typeof( string ) : field.EditAsTypeResolved;
		}

		#endregion // GetValueTypeForQuantitativeFilterComparison

		#region Initialize

		// AS - NA 11.2 Excel Style Filtering
		// Moved to the derived class.
		//
		//internal void Initialize( DataRecord record )
		//{
		//    Debug.Assert( _field.Owner == record.FieldLayout );
		//
		//    _record = record;
		//    _currentValue = null;
		//
		//    // SSP 6/16/09 - TFS18467
		//    // If a combo editor is being used to map values then the filtering should be done on the
		//    // mapped values, not the underlying values, because the mapped values are what the end
		//    // user sees.
		//    // 
		//    _currentCellValueForFilterComparison = this.GetCellValueForFilterComparison( record );
		//}

		#endregion //SetCurrentValue

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region AllValues

		public override IEnumerable<ValueEntry> AllValues
		{
			get
			{
				if ( null == _cachedAllValues )
					
					
					
					_cachedAllValues = new AllValuesEnumerable( this, _allValuesRecords );

				return _cachedAllValues;
			}
		}

		#endregion // AllValues

		#region CurrentValue

		// AS - NA 11.2 Excel Style Filtering
		// Moved to the derived class
		//
		//public override ValueEntry CurrentValue 
		//{
		//    get
		//    {
		//        if ( null == _currentValue )
		//            //~ SSP 6/16/09 - TFS18467 - Optimizations
		//            //~ 
		//            //~_currentValue = new FilterValueEntry( _field, _record );
		//            _currentValue = new FilterValueEntry( _field, _record, _currentCellValueForFilterComparison );
		//
		//        return _currentValue;
		//    }
		//}

		#endregion // CurrentValue

		#region Comparer

        // SSP 5/3/10 TFS25788
		// Added FilterComparer property on the FieldSettings in data presenter.
		// 
		public override IComparer Comparer
		{
			get
			{
				return _filterComparer;
			}
		}

		#endregion // Comparer

		#region FilterEvaluator

		// SSP 2/29/12 TFS89053
		// 
		internal override IFilterEvaluator FilterEvaluator
		{
			get
			{
				return _filterEvaluator;
			}
		} 

		#endregion // FilterEvaluator

		// AS - NA 11.2 Excel Style Filtering
		#region Field
		public Field Field
		{
			get { return _field; }
		} 
		#endregion //Field

		#region IgnoreCase

		public override bool IgnoreCase
		{
			get 
			{
				return FieldSortComparisonType.CaseInsensitive == _field.FilterStringComparisonTypeResolved;
			}
		}

		#endregion // IgnoreCase

		#region PreferredComparisonDataType

		public override Type PreferredComparisonDataType 
		{
			get
			{
				// SSP 11/24/09 TFS24666
				// If values are mapped to display text (for example using combo editor) then
				// always compare by display texts which would be of string type.
				// 
				
				CellTextConverterInfo info = CellTextConverterInfo.GetCachedConverter( _field );
				if ( null != info && info.CompareByText )
					return typeof( string );
				

                // SSP 5/3/10 TFS25788
				// If field data type is string then let the filtering logic decide what is the
				// best comparison type. For example, if the string field actually contains 
				// numeric values as strings and a filter condition with a numeric compare value
				// has been specified then a numeric comparison will be done by the filtering
				// logic.
				// 
				
				//return _field.EditAsTypeResolved;
				Type type = _field.EditAsTypeResolved;
				if ( typeof( string ) == type )
					type = null;

				return type;
				
			}
		}

		#endregion // PreferredComparisonDataType

		#endregion // Public Properties

		#region Private/Internal Properties

		#region CurrentCellValueForFilterComparison

		// SSP 6/16/09 - TFS18467
		// If a combo editor is being used to map values then the filtering should be done on the
		// mapped values, not the underlying values, because the mapped values are what the end
		// user sees.
		// 
		internal object CurrentCellValueForFilterComparison
		{
			get
			{
				return _currentCellValueForFilterComparison;
			}
			// AS - NA 11.2 Excel Style Filtering
			set
			{
				_currentCellValueForFilterComparison = value;
			}
		}

		#endregion // CurrentCellValueForFilterComparison

		#region WasAllValuesUsed

		internal bool WasAllValuesUsed
		{
			get
			{
				return null != _cachedAllValues;
			}
		}

		#endregion // WasAllValuesUsed

		#endregion // Private/Internal Properties

		#endregion // Properties
	}

	#endregion // FilterConditionEvaluationContextBase Class

	#region FilterConditionEvaluationContext
	// AS - NA 11.2 Excel Style Filtering
	// Moved the portion of the old FilterConditionEvaluationContext that relied on the DataRecord
	// into this derived class.
	//
	internal class FilterConditionEvaluationContext : FilterConditionEvaluationContextBase
	{
		#region Member Variables

		private DataRecord _record;
		private ValueEntry _currentValue;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterConditionEvaluationContext"/>.
		/// </summary>
		/// <param name="field">Values of this field are being evaluated.</param>
		/// <param name="allValuesRecords">Supplies the 'AllValues'.</param>
		internal FilterConditionEvaluationContext( Field field, IEnumerable<DataRecord> allValuesRecords ) : base(field, allValuesRecords)
		{
		}

		#endregion // Constructor

		#region Base class overrides

		#region CurrentValue

		// AS - NA 11.2 Excel Style Filtering
		public override ValueEntry CurrentValue
		{
			get
			{
				if (null == _currentValue)
					
					
					
					_currentValue = new FilterValueEntry(this.Field, _record, this.CurrentCellValueForFilterComparison);

				return _currentValue;
			}
		}

		#endregion // CurrentValue 

		#endregion //Base class overrides

		#region Methods

		#region Initialize

		internal void Initialize(DataRecord record)
		{
			Debug.Assert(this.Field.Owner == record.FieldLayout);

			_record = record;
			_currentValue = null;

			// SSP 6/16/09 - TFS18467
			// If a combo editor is being used to map values then the filtering should be done on the
			// mapped values, not the underlying values, because the mapped values are what the end
			// user sees.
			// 
			this.CurrentCellValueForFilterComparison = this.GetCellValueForFilterComparison(record);
		}

		#endregion // Initialize

		#endregion //Methods
	}
	#endregion //FilterConditionEvaluationContext

	// AS - NA 11.2 Excel Style Filtering
	// Added evaluation context that the RecordFilterTreeControl could use when 
	// determining what is checked in the tree.
	//
	#region FilterDropDownConditionEvaluationContext
	internal class FilterDropDownConditionEvaluationContext : FilterConditionEvaluationContextBase
	{
		#region Member Variables

		private FilterDropDownItem _dropDownItem;
		private ValueEntry _currentValue;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterDropDownConditionEvaluationContext"/>.
		/// </summary>
		/// <param name="field">Values of this field are being evaluated.</param>
		/// <param name="allValuesRecords">Supplies the 'AllValues'.</param>
		internal FilterDropDownConditionEvaluationContext( Field field, IEnumerable<DataRecord> allValuesRecords ) : base(field, allValuesRecords)
		{
		}

		#endregion // Constructor

		#region Base class overrides

		#region CurrentValue

		// AS - NA 11.2 Excel Style Filtering
		public override ValueEntry CurrentValue
		{
			get
			{
				if (null == _currentValue)
					_currentValue = new DropDownItemValueEntry(this.Field, _dropDownItem, this.CurrentCellValueForFilterComparison);

				return _currentValue;
			}
		}

		#endregion // CurrentValue 

		#endregion //Base class overrides

		#region Methods

		#region Initialize

		internal void Initialize(FilterDropDownItem dropDownItem)
		{
			_dropDownItem = dropDownItem;
			_currentValue = null;

			
			this.CurrentCellValueForFilterComparison = dropDownItem.Value;
		}

		#endregion // Initialize

		#endregion //Methods

		#region DropDownItemValueEntry class
		private class DropDownItemValueEntry : FilterValueEntryBase
		{
			#region Member Variables

			private FilterDropDownItem _dropDownItem;

			#endregion //Member Variables

			#region Constructor
			internal DropDownItemValueEntry(Field field, FilterDropDownItem dropDownItem, object value)
				: base(field, value)
			{
				_dropDownItem = dropDownItem;
			}
			#endregion //Constructor

			#region Base class overrides

			#region Context
			public override object Context
			{
				get { return _dropDownItem; }
			}
			#endregion //Context

			#endregion //Base class overrides
		} 
		#endregion //DropDownItemValueEntry class
	}
	#endregion //FilterDropDownConditionEvaluationContext

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