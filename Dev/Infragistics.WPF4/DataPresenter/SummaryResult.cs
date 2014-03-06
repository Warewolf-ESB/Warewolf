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
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{

	#region SummaryResult Class

	/// <summary>
	/// SummaryResult object holds result of a summary calculation.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryResult</b> object holds result of a summary calculation. 
	/// RecordCollection exposes <see cref="Infragistics.Windows.DataPresenter.RecordCollectionBase.SummaryResults"/> property 
	/// that returns a collection of all the summary calculation results associated with that 
	/// RecordCollection.
	/// </para>
	/// <para class="body">
	/// To specify summaries to calculate, use the FieldLayout's <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/>
	/// property.
	/// </para>
	/// </remarks>
	/// <seealso cref="Infragistics.Windows.DataPresenter.RecordCollectionBase.SummaryResults"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/>
	public class SummaryResult : PropertyChangeNotifier
	{
		#region Private Vars

		private object _tooltip;
		private object _calculatedValue;
		private SummaryDefinition _summaryDefinition;
		private SummaryResultCollection _parentCollection;
		private int _verifiedDataVersion = -1;
		private int _verifiedFieldsVersion = -1;

		private Field _cachedSourceField;
		private Field _cachedPositionFieldResolved;
		private string _cachedDisplayText;

		// SSP 8/2/09 - Summary Recalc Optimizations
		// Now the summary result maintains its own data version, in adition to using
		// the summary definition's data version.
		// 
		private int _dataVersion;
		private PropertyValueTracker _summaryCalcVersionTracker;

		// AS 7/31/09 NA 2009.2 Field Sizing
		private int _fieldAutoSizeVersion;

		// SSP 7/7/10 TFS34835
		// For printing and exporting calculate summaries synchronously.
		// 
		private bool _isAsyncOperationSupported;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryResult"/>.
		/// </summary>
		/// <param name="parentCollection">Parent collection that this summary result belongs to.</param>
		/// <param name="summaryDefinition">Associated summary definition object.</param>
		internal SummaryResult( SummaryResultCollection parentCollection, SummaryDefinition summaryDefinition )
		{
			GridUtilities.ValidateNotNull( parentCollection );
			GridUtilities.ValidateNotNull( summaryDefinition );			

			_parentCollection = parentCollection;
			_summaryDefinition = summaryDefinition;

			// SSP 7/7/10 TFS34835
			// For printing and exporting calculate summaries synchronously.
			// 
			DataPresenterBase dp = GridUtilities.GetDataPresenter( parentCollection.Records );
			_isAsyncOperationSupported = null != dp && !dp.IsReportControl && !dp.IsExportControl;
		}

		#endregion // Constructor

		#region Base Overrides

		#region OnHasListenersChanged

		// SSP 8/2/09 - Optimizations
		// 
		/// <summary>
		/// Overridden. Called when the value of HasListeners property changes.
		/// </summary>
		protected override void OnHasListenersChanged( )
		{
			base.OnHasListenersChanged( );

			this.VerifyHasAnyListeners( );
		}

		#endregion // OnHasListenersChanged

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region DisplayAreaResolved

		/// <summary>
		/// Determines if and where the summary result is displayed (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DisplayAreaResolved</b> returns the resolved value based on settings of 
		/// <see cref="Infragistics.Windows.DataPresenter.SummaryDefinition"/>'s <see cref="Infragistics.Windows.DataPresenter.SummaryDefinition.DisplayArea"/> property and 
		/// <see cref="Infragistics.Windows.DataPresenter.FieldSettings"/>' <see cref="Infragistics.Windows.DataPresenter.FieldSettings.SummaryDisplayArea"/> property. 
		/// <see cref="Infragistics.Windows.DataPresenter.FieldSettings"/>
		/// object is exposed on <b>DataGrid</b> (<see cref="DataPresenterBase.FieldSettings"/>), 
		/// FieldLayout (<see cref="Infragistics.Windows.DataPresenter.FieldLayout.FieldSettings"/>) and Field (<see cref="Infragistics.Windows.DataPresenter.Field.Settings"/>).
		/// </para>
		/// <para class="body">
		/// To actually specify summary display area, use the SummaryDefinition's <see cref="Infragistics.Windows.DataPresenter.SummaryDefinition.DisplayArea"/> 
		/// property or FieldSettings' <see cref="Infragistics.Windows.DataPresenter.FieldSettings.SummaryDisplayArea"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinition.DisplayArea"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.SummaryDisplayArea"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowSummaries"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.SummaryUIType"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		//[Description( "Determines if and where summaries are displayed (read-only)." )]
		[Bindable( true )]
		[ReadOnly( true )]
		public SummaryDisplayAreas DisplayAreaResolved
		{
			get
			{
				FieldLayout fl = this.FieldLayout;
				SummaryDefinition summaryDef = this.SummaryDefinition;

				// If the summary is to be displayed under a field and that field is not visible, 
				// then return None.
				// 
				Field posField = this.PositionFieldResolved;				
				if ( SummaryPosition.UseSummaryPositionField == this.PositionResolved
					// AS 8/24/09 TFS19532
					// To be consistent with how we handle label/cells we should show the placeholder 
					// when the visibility is hidden.
					//
					//&& ( null == posField || Visibility.Visible != posField.VisibilityResolved ) )
					&& ( null == posField || Visibility.Collapsed == posField.VisibilityResolved ) )
					return SummaryDisplayAreas.None;
				
				SummaryDisplayAreas? ret = summaryDef.DisplayArea;
				if ( ret.HasValue )
					return ret.Value;

				FieldSettings settings = null != posField ? posField.SettingsIfAllocated : null;
				if ( null != settings )
				{
					ret = settings.SummaryDisplayArea;
					if ( ret.HasValue )
						return ret.Value;
				}

				settings = null != fl ? fl.FieldSettingsIfAllocated : null;
				if ( null != settings )
				{
					ret = settings.SummaryDisplayArea;
					if ( ret.HasValue )
						return ret.Value;
				}

				DataPresenterBase dp = null != fl ? fl.DataPresenter : null;
				settings = null != dp ? dp.FieldSettingsIfAllocated : null;
				if ( null != settings )
				{
					ret = settings.SummaryDisplayArea;
					if ( ret.HasValue )
						return ret.Value;
				}

				return SummaryDisplayAreas.TopLevelOnly 
					| SummaryDisplayAreas.InGroupByRecords 
					| SummaryDisplayAreas.Bottom;
			}
		}

		#endregion // DisplayAreaResolved

		#region DisplayText

		/// <summary>
		/// Returns the display text of the summary result.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DisplayText</b> returns the string value that's actually displayed in the UI element
		/// representation of this summary result. It's derived using the SummaryDefinition's
		/// <see cref="Infragistics.Windows.DataPresenter.SummaryDefinition.StringFormat"/> property setting.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that <see cref="SummaryResult.Value"/> property returns the actual result
		/// of the calculation.
		/// </para>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryResult.Value"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinition.StringFormat"/>
		/// </remarks>
		public string DisplayText
		{
			get
			{
				this.EnsureCalculated( );

				// SSP 3/19/10
				// 
				//return _cachedDisplayText;
				return _cachedDisplayText ?? string.Empty;
			}
		}

		#endregion // DisplayText

		#region DisplayTextAsync

		// SSP 3/19/10 - Optimizations
		// Added DisplayTextAsync property.
		// 
		/// <summary>
		/// Returns the display text of the summary result.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DisplayText</b> returns the string value that's actually displayed in the UI element
		/// representation of this summary result. It's derived using the SummaryDefinition's
		/// <see cref="Infragistics.Windows.DataPresenter.SummaryDefinition.StringFormat"/> property setting.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that <see cref="SummaryResult.Value"/> property returns the actual result
		/// of the calculation.
		/// </para>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryResult.Value"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinition.StringFormat"/>
		/// </remarks>
		public string DisplayTextAsync
		{
			get
			{
				// SSP 7/7/10 TFS34835
				// For printing and exporting calculate summaries synchronously.
				// 
				if ( !_isAsyncOperationSupported )
					this.EnsureCalculated( );

				return _cachedDisplayText ?? string.Empty;
			}
		}

		#endregion // DisplayTextAsync

		#region ParentCollection

		/// <summary>
		/// Returns the parent collection this summary result object belongs to.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryResultCollection"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordCollectionBase.SummaryResults"/>
		public SummaryResultCollection ParentCollection
		{
			get
			{
				return _parentCollection;
			}
		}

		#endregion // ParentCollection

		#region PositionFieldResolved

		/// <summary>
		/// The summary will be aligned with this field in the summary record.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The summary result will be displayed in the summary record aligned with this
		/// field. You can specify the position field using SummaryDefinition's 
		/// <see cref="Infragistics.Windows.DataPresenter.SummaryDefinition.PositionFieldName"/> property. If PositionFieldName
		/// property has not been set, the source field is used.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinition.PositionFieldName"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinition.SourceFieldName"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryResult.SourceField"/>
		public Field PositionFieldResolved
		{
			get
			{
				this.VerifyFields( );
				return _cachedPositionFieldResolved;
			}
		}

		#endregion // PositionFieldResolved

		#region SourceField

		/// <summary>
		/// Returns the field whose values are being summarized.
		/// </summary>
		public Field SourceField
		{
			get
			{
				this.VerifyFields( );
				return _cachedSourceField;
			}
		}

		#endregion // SourceField

		#region SummaryDefinition

		/// <summary>
		/// Gets the associated <see cref="Infragistics.Windows.DataPresenter.SummaryDefinition"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property returns the associated <see cref="Infragistics.Windows.DataPresenter.SummaryDefinition"/> object.
		/// See <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/> property for how to actually specify summaries.
		/// </para>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinition"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/>
		/// </remarks>
		public SummaryDefinition SummaryDefinition
		{
			get
			{
				return _summaryDefinition;
			}
		}

		#endregion // SummaryDefinition

		#region ToolTip

		/// <summary>
		/// Specifies the tooltip to display when the mouse is hovered over the summary result.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ToolTip</b> specifies the tooltip to display when the mouse is hovered over the summary result. 
		/// Note that you can specify a string value as a value for this property.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinition.ToolTip"/>
		/// <seealso cref="ToolTipResolved"/>
		//[Description( "Tooltip to display over the summary result." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public object ToolTip
		{
			get
			{
				return _tooltip;
			}
			set
			{
				if ( value != _tooltip )
				{
					_tooltip = value;
					this.RaisePropertyChangedEvent( "ToolTip" );
					this.RaisePropertyChangedEvent( "ToolTipResolved" );
				}
			}
		}

		#endregion // ToolTip

		#region ToolTipResolved

		/// <summary>
		/// Returns the resolved tooltip that will be displayed when the mouse hovers the summary result.
		/// </summary>
		[Bindable( true )]
		[Browsable(false)]
		public object ToolTipResolved
		{
			get
			{
				return this.GetToolTipResolved( true );
			}
		}

		#endregion // ToolTipResolved

		#region Value

		/// <summary>
		/// Returns the result of the summary calculation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Value</b> returns the result of the summary calculation. The type of the returned value 
		/// varies depending upon the type of summary calculation being performed. Typically for numeric
		/// calculations, a numeric type like <i>decimal</i> is returned. For Maximum and Minimum summary
		/// calculations, the returned object is of field's data type.
		/// </para>
		/// <para class="body">
		/// <i>Value</i> essentially returns whatever object the underlying summary calculator returned 
		/// from the underlying <see cref="SummaryCalculator.EndCalculation"/> implemenetation.
		/// </para>
		/// <seealso cref="SummaryResult.DisplayText"/>
		/// <seealso cref="SummaryResult.Refresh"/>
		/// </remarks>
		public object Value
		{
			get
			{
				this.EnsureCalculated( );

				return _calculatedValue;
			}
		}

		#endregion // Value

		#region ValueAsync

		// SSP 3/19/10 - Optimizations
		// Added ValueAsync property.
		// 
		/// <summary>
		/// Returns the result of the summary calculation when it was calculated the last time. It doesn't force
		/// the summary to be calculated if it's marked dirty.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The difference between <see cref="Value"/> and <b>ValueAsync</b> is that the <i>Value</i> 
		/// property forces the summary to be re-calculated if it has been marked dirty. <i>ValueAsync</i>
		/// simply returns the last calculated result.
		/// </para>
		/// <seealso cref="SummaryResult.Value"/>
		/// <seealso cref="SummaryResult.DisplayText"/>
		/// <seealso cref="SummaryResult.DisplayTextAsync"/>
		/// </remarks>
		public object ValueAsync
		{
			get
			{
				// SSP 7/7/10 TFS34835
				// For printing and exporting calculate summaries synchronously.
				// 
				if ( ! _isAsyncOperationSupported )
					this.EnsureCalculated( );

				return _calculatedValue;
			}
		}

		#endregion // ValueAsync

		#endregion // Public Properties

		#region Private/Internal Properties

		#region DataVersion

		// SSP 8/2/09 - Optimizations
		// Made public.
		// 
		//private int DataVersion
		/// <summary>
		/// For internal use only. May get removed in future builds.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never ), Browsable( false ),
		 DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public int DataVersion
		{
			get
			{
				// SSP 8/2/09 - Optimizations
				// 
				//return this.SummaryDefinition.CalculationVersion;
				return _dataVersion + this.SummaryDefinition.CalculationVersion;
			}
		}

		#endregion // DataVersion

		// AS 7/31/09 NA 2009.2 Field Sizing
		#region FieldAutoSizeVersion
		internal int FieldAutoSizeVersion
		{
			get { return _fieldAutoSizeVersion; }
			set { _fieldAutoSizeVersion = value; }
		} 
		#endregion //FieldAutoSizeVersion

		#region FieldLayout

		internal FieldLayout FieldLayout
		{
			get
			{
				return _parentCollection.Records.FieldLayout;
			}
		}

		#endregion // FieldLayout

		#region IsDirty

		// SSP 4/13/12 TFS108549 - Optimizations
		// Changed the property into a method.
		// 
		internal bool IsDirty( bool cleanPendingDirtyAffectedSummaries )
		{
			if ( cleanPendingDirtyAffectedSummaries && null != _parentCollection )
				_parentCollection.CleanPendingDirtyAffectedSummaries( );

			return _verifiedDataVersion != this.DataVersion;
		}

		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		#endregion // IsDirty

		#region PositionResolved

		/// <summary>
		/// Returns the resolved summary position.
		/// </summary>
		internal SummaryPosition PositionResolved
		{
			get
			{
				SummaryPosition pos = _summaryDefinition.Position;
				if ( SummaryPosition.Default == pos )
					pos = SummaryPosition.UseSummaryPositionField;

				return pos;
			}
		}

		#endregion // PositionResolved

		#region StringFormatProviderResolved

		
		
		/// <summary>
		/// Resolved string format provider.
		/// </summary>
		internal IFormatProvider StringFormatProviderResolved
		{
			get
			{
				IFormatProvider ret = _summaryDefinition.StringFormatProvider;
				if ( null == ret )
					
					
					
					
					// SSP 9/30/11 Calc
					// For formula summaries SourceField may be null.
					// 
					//ret = GridUtilities.GetDefaultCulture( this.SourceField );
					ret = GridUtilities.GetDefaultCulture( this.SourceField, this.FieldLayout );

				return ret;
			}
		}

		#endregion // StringFormatProviderResolved

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Refresh

		/// <summary>
		/// Re-calculates the summary result.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Typically it's not necessary to call this method as summaries are recalculated 
		/// automatically whenever data changes.
		/// </para>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinition.Refresh"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinitionCollection.Refresh"/>
		/// </remarks>
		public void Refresh( )
		{
			_verifiedDataVersion--;
			_verifiedFieldsVersion--;

			this.ResultChanged_RaisePropChangeNotifications( );
		}

		#endregion // Refresh

		#endregion // Public Methods

		#region Private/Internal Methods

		#region DirtyCalculation

		// SSP 8/2/09 - Optimizations
		// Now the summary result maintains its own data version, in adition to using
		// the summary definition's data version.
		// 
		/// <summary>
		/// Bumps the data version number if not already dirty which will cause the summary result to re-calc itself.
		/// </summary>
		internal void DirtyCalculation( )
		{
			// SSP 4/13/12 TFS108549 - Optimizations
			// 
			//if ( ! this.IsDirty )
			if ( !this.IsDirty( false ) )
			{
				_dataVersion++;

				this.RaisePropertyChangedEvent( "DataVersion" );
			}
		}

		#endregion // DirtyCalculation

		#region EnsureCalculated

		internal void EnsureCalculated( )
		{
			// SSP 8/2/09 - Summary Recalc Optimizations
			// 
			
			
			
			
			if ( ! this.IsDirty( true ) )
				return;
			
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

			

			SummaryCalculator calculator = _summaryDefinition.Calculator;
			Field sourceField = this.SourceField;
			_verifiedDataVersion = this.DataVersion;
			if ( null != calculator && null != sourceField )
			{
				RecordCollectionBase records = _parentCollection.Records;
				FieldLayout fieldLayout = records.FieldLayout;
				CalculationScope calcScope = fieldLayout.CalculationScopeResolved;
				bool visibleRecordsOnly = CalculationScope.FilteredSortedList == calcScope;
				object newCalculatedValue;

				// SSP 10/31/11 TFS94847
				// Don't call Begin/End calculation on custom calculator if the summary result is associated
				// with template records. Added the if block and enclosed the existing code in the else block.
				// 
				if ( GridUtilities.IsTemplateRecords( records ) )
				{
					newCalculatedValue = null;
				}
				else
				{
					// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
					// 
					// ------------------------------------------------------------------------------------------
					bool performCalculation = true;
					newCalculatedValue = null;
					DataPresenterBase dataPresenter = fieldLayout.DataPresenter;
					if ( null != dataPresenter )
					{
						SummaryEvaluationMode evaluationMode = fieldLayout.SummaryEvaluationModeResolved;
						performCalculation = SummaryEvaluationMode.Manual != evaluationMode;

						// Raise the new QuerySummaryResult event and if the user provides a value via
						// the event args SetCalculatedValue method, use that and skip the normal summary 
						// calculation logic below.
						// 
						QuerySummaryResultEventArgs args = new QuerySummaryResultEventArgs( this );
						dataPresenter.RaiseQuerySummaryResult( args );
						if ( null != args.ProvidedValue )
						{
							newCalculatedValue = args.ProvidedValue;
							performCalculation = false;
						}
					}

					if ( performCalculation )
					{
						SummaryEvaluationMode summaryEvaluationMode = fieldLayout.SummaryEvaluationModeResolved;

						if ( SummaryEvaluationMode.UseLinq == summaryEvaluationMode && !sourceField.IsUnbound )
						{
							string summary = null;
							if ( typeof( AverageSummaryCalculator ) == calculator.GetType( ) )
								summary = "Average";
							else if ( typeof( SumSummaryCalculator ) == calculator.GetType( ) )
								summary = "Sum";
							else if ( typeof( CountSummaryCalculator ) == calculator.GetType( ) )
								summary = "Count";
							else if ( typeof( MinimumSummaryCalculator ) == calculator.GetType( ) )
								summary = "Min";
							else if ( typeof( MaximumSummaryCalculator ) == calculator.GetType( ) )
								summary = "Max";

							if ( null != summary )
							{
								RecordManager rm = records.ParentRecordManager;
								LinqQueryManager lqm = VUtils.CreateLinqQueryManager( rm, CalculationScope.FilteredSortedList != calcScope );

								if ( null != lqm )
								{
									LinqQueryManager.LinqInstructionSummary summaryQuery = new LinqQueryManager.LinqInstructionSummary(
										summary, VUtils.GetPropertyName( sourceField ), null );

									IEnumerable result = null;
									try
									{
										result = lqm.PerformQuery( summaryQuery );
									}
									catch
									{
										// When there are no items in the list, performing linq average, min or max will result
										// in InvalidOperationException.
										// 

										if ( "Average" == summary )
											result = new object[] { 0m };
									}

									object val = null != result ? CoreUtilities.GetFirstItem<object>( result, true ) : null;
									newCalculatedValue = val;
									performCalculation = false;
								}
							}

						}


						if ( SummaryEvaluationMode.Manual == summaryEvaluationMode )
						{
							return;
						}
					}
					// ------------------------------------------------------------------------------------------

					// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
					// Enclosed the existing code into the if block that checks for the performCalculation.
					// 
					if ( performCalculation )
					{
						// The enumerator below will return records in its sorted order. If 
						// calculation scope specifies that we need to aggregate data in unsorted 
						// order, then we need to resort the records returned by GetRecordEnumerator
						// into their unsorted order.
						// 
						bool sortNeeded = CalculationScope.FullUnsortedList == calcScope
							&& calculator.IsCalculationAffectedBySort;

						IEnumerable<DataRecord> dataRecords =
							new TypedEnumerable<DataRecord>(
								records.GetRecordEnumerator(
									RecordType.DataRecord, sourceField.Owner, sourceField.Owner, visibleRecordsOnly ) );

						if ( sortNeeded )
						{
							List<DataRecord> list = new List<DataRecord>( dataRecords );
							list.Sort( new SortComparer_UnsortedIndex( ) );
							dataRecords = list;
						}

						calculator.BeginCalculation( this );

						foreach ( DataRecord record in dataRecords )
						{
							// JJD 5/29/09 - TFS18063 
							// Use the new overload to GetCellValue which will return the value 
							// converted into EditAsType
							//object dataValue = record.GetCellValue( sourceField, true );
							object dataValue = record.GetCellValue( sourceField, CellValueType.EditAsType );
							calculator.Aggregate( dataValue, this, record );
						}

						newCalculatedValue = calculator.EndCalculation( this );
					}
				}

				// SSP 9/9/11 - Calc
				// Refactored. Moved into new InternalSetNewValue method.
				// 
				this.InternalSetNewValue( newCalculatedValue );
			}
		}

		private class SortComparer_UnsortedIndex : IComparer<DataRecord>
		{
			public int Compare( DataRecord x, DataRecord y )
			{
				int xxIndex = x.DataItemIndex;
				int yyIndex = y.DataItemIndex;

				return xxIndex.CompareTo( yyIndex );
			}
		}

		#endregion // EnsureCalculated

		#region GetDisplayTextHelper

		/// <summary>
		/// Returns the display text.
		/// </summary>
		/// <returns>Formatted display text</returns>
		private string GetDisplayTextHelper( )
		{
			object value = this.Value;

			SummaryDefinition summaryDef = this.SummaryDefinition;
			SummaryCalculator calculator = summaryDef.Calculator;
			Field sourceField = this.SourceField;

			if ( null != sourceField && null != calculator )
			{
				
				
				
				IFormatProvider formatProvider = this.StringFormatProviderResolved;

				string format = summaryDef.StringFormat;
				if ( string.IsNullOrEmpty( format ) )
					format = sourceField.GetSummaryStringFormat( calculator.Name );

				if ( null != format )
					// SSP 9/2/08 BR35879
					// 
					//return string.Format( formatProvider, format, value, calculator.Name, summaryDef.Key, summaryDef.SourceFieldName );
					return string.Format( formatProvider, format, value, calculator.DisplayName, summaryDef.Key, summaryDef.SourceFieldName );

				return calculator.ApplyDefaultFormat( value, this );
			}
				// SSP 9/29/11 Calc
				// For formula summaries there's no source field or calculator.
				// 
			else if ( null != value )
			{
				string format = summaryDef.StringFormat;
				if ( string.IsNullOrEmpty( format ) )
				{
					if ( string.IsNullOrEmpty( summaryDef.Key ) )
						format = "{0}";
					else
						format = "{1} = {0}";
				}

				return string.Format( this.StringFormatProviderResolved, format, value, summaryDef.Key );
			}

			return null;
		}

		#endregion // GetDisplayTextHelper

		#region GetToolTipResolved

		// SSP 3/19/10 - Optimizations
		// Refactored. Moved code from ToolTipResolved into new GetToolTipResolved method.
		// 
		private object GetToolTipResolved( bool async )
		{
			object ret = _tooltip;
			if ( null != ret )
				return ret;

			ret = this.SummaryDefinition.ToolTip;
			if ( null != ret )
				return ret;

			// SSP 9/30/11 Calc
			// 
			// --------------------------------------------------------------------------------
			//string sourceFieldName = this.SourceField.Name;
			//ret = sourceFieldName + ": " + ( async ? this.DisplayTextAsync : this.DisplayText );
			
			string displayText = async ? this.DisplayTextAsync : this.DisplayText;

			Field sourceField = this.SourceField;
			if ( null != sourceField )
			{
				string sourceFieldName = sourceField.Name;
				ret = sourceFieldName + ": " + displayText;
			}
				// There may not be a source field if there's a formula.
				// 
			else
			{
				return displayText;
			}
			// --------------------------------------------------------------------------------

			return ret;
		}

		#endregion // GetToolTipResolved

		#region InternalSetNewValue

		// SSP 9/9/11 - Calc
		// Refactored. Moved existing logic from EnsureCalculated into this new InternalSetNewValue method.
		// 
		internal void InternalSetNewValue( object newCalculatedValue )
		{
			bool calculatedValueChanged = !object.Equals( _calculatedValue, newCalculatedValue );
			_calculatedValue = newCalculatedValue;

			string newDisplayText = this.GetDisplayTextHelper( );

			FieldLayout fieldLayout = this.FieldLayout;
			if ( calculatedValueChanged || _cachedDisplayText != newDisplayText )
			{
				_cachedDisplayText = newDisplayText;
				this.ResultChanged_RaisePropChangeNotifications( );

				// AS 7/21/09 NA 2009.2 Field Sizing
				fieldLayout.AutoSizeInfo.OnSummaryChanged( _parentCollection.Records, this );
			}

			if ( calculatedValueChanged )
			{
				// Raise SummaryResultChanged event.
				DataPresenterBase dp = fieldLayout.DataPresenter;
				Debug.Assert( null != dp );
				if ( null != dp )
					dp.RaiseSummaryResultChanged( new SummaryResultChangedEventArgs( this ) );
			}
		}

		#endregion // InternalSetNewValue

		#region OnSummaryDefCalcVersionChanged

		// SSP 8/2/09 - Optimizations
		// 
		/// <summary>
		/// Called by _summaryCalcVersionTracker whenever the calc version of the associated
		/// summary definition changes.
		/// </summary>
		private void OnSummaryDefCalcVersionChanged( )
		{
			this.RaisePropertyChangedEvent( "DataVersion" );
		}

		#endregion // OnSummaryDefCalcVersionChanged

		#region ResultChanged_RaisePropChangeNotifications

		private void ResultChanged_RaisePropChangeNotifications( )
		{
			this.RaisePropertyChangedEvent( "Value" );
			this.RaisePropertyChangedEvent( "DisplayText" );
			this.RaisePropertyChangedEvent( "ToolTipResolved" );

			// SSP 3/19/10 - Optimizations
			// Added ValueAsync and DisplayTextAsync properties.
			// 
			this.RaisePropertyChangedEvent( "ValueAsync" );
			this.RaisePropertyChangedEvent( "DisplayTextAsync" );
		}

		#endregion ResultChanged_RaisePropChangeNotifications

		#region VerifyFields

		/// <summary>
		/// Verifies SourceField and PositionField property values.
		/// </summary>
		private void VerifyFields( )
		{
			SummaryDefinition summaryDef = this.SummaryDefinition;
			SummaryDefinitionCollection summaries = summaryDef.ParentCollection;
			int version = null != summaries ? summaries.SummariesVersion : 0;

			FieldCollection fields = GridUtilities.GetFields( this.FieldLayout );
			if ( null != fields )
				version += fields.Version;

			if ( _verifiedFieldsVersion != version )
			{
				Field oldSourceField = _cachedSourceField;
				Field oldPosField = _cachedPositionFieldResolved;


				
				// descendant field layout field and also situations where SourceFieldName is changed
				// on the summary definition.
				// 
				_cachedSourceField = GridUtilities.GetField( fields, summaryDef.SourceFieldName, false );

				string posFieldName = summaryDef.PositionFieldName;
				// If PositionFieldName is specified then use that otherwise resolved position field
				// to be the source field.
				if ( !string.IsNullOrEmpty( posFieldName ) )
					_cachedPositionFieldResolved = GridUtilities.GetField( fields, summaryDef.PositionFieldName, false );
				else
					_cachedPositionFieldResolved = _cachedSourceField;

				_verifiedFieldsVersion = version;

				if ( oldSourceField != _cachedSourceField )
				{
					_verifiedDataVersion--;

					// SSP 8/2/09 - Summary Recalc Optimizations
					// 
					if ( null != _parentCollection )
						_parentCollection.DirtySourceFieldMapCache( );

					this.RaisePropertyChangedEvent( "SourceField" );
				}

				if ( oldPosField != _cachedPositionFieldResolved )
				{
					this.RaisePropertyChangedEvent( "PositionFieldResolved" );
				}
			}
		}

		#endregion // VerifyFields

		#region VerifyHasAnyListeners

		// SSP 8/2/09 - Optimizations
		// 
		/// <summary>
		/// Allocates or releases trackers whenever HasListeners property changes.
		/// </summary>
		private void VerifyHasAnyListeners( )
		{
			bool hasListeners = this.HasListeners;

			if ( hasListeners )
			{
				if ( null == _summaryCalcVersionTracker )
				{
					_summaryCalcVersionTracker = new PropertyValueTracker( _summaryDefinition,
						SummaryDefinition.CalculationVersionProperty, this.OnSummaryDefCalcVersionChanged
						// SSP 3/19/10 - Optimizations
						// Pass true for callAsynchronously parameter to get called asynchronously.
						// 
						, true
					);
				}
			}
			else
			{
				_summaryCalcVersionTracker = null;
			}
		}

		#endregion // VerifyHasAnyListeners

		#endregion // Private/Internal Methods

		#endregion // Methods

	}

	#endregion // SummaryResult Class

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