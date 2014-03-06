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

	#region SummaryDefinitionCollection Class

	/// <summary>
	/// A collection of <see cref="SummaryDefinition"/> objects.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// To specify summaries, use the FieldLayout's <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/> property.
	/// </para>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowSummaries"/>
	/// </remarks>
	public class SummaryDefinitionCollection : ObservableCollection<SummaryDefinition>
	{
		#region Private/Internal Vars

		private int _summariesVersion;
		private int _collectionVersion;
		private FieldLayout _fieldLayout;

		#endregion // Private/Internal Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryDefinitionCollection"/> object.
		/// </summary>
		internal SummaryDefinitionCollection( FieldLayout fieldLayout )
		{
			GridUtilities.ValidateNotNull( fieldLayout );
			_fieldLayout = fieldLayout;
		}

		#endregion // Constructor

		#region Base Overrides

		#region ClearItems

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected override void ClearItems( )
		{
			foreach ( SummaryDefinition def in this )
				this.OnItemBeingAddedOrRemoved( def, false );

			base.ClearItems( );
		}

		#endregion // ClearItems

		#region InsertItem

		/// <summary>
		/// Inserts the specified <see cref="SummaryDefinition"/> to in the collection.
		/// </summary>
		/// <param name="index">Location in the collection at which to insert the item.</param>
		/// <param name="item">The item to insert.</param>
		protected override void InsertItem( int index, SummaryDefinition item )
		{
			this.OnItemBeingAddedOrRemoved( item, true );
			base.InsertItem( index, item );
		}

		#endregion // InsertItem

		#region OnCollectionChanged

		/// <summary>
		/// Overridden. Called when the collection changes.
		/// </summary>
		/// <param name="e">Event args</param>
		protected override void OnCollectionChanged( System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			this.BumpCollectionVersion( );

			base.OnCollectionChanged( e );
		}

		#endregion // OnCollectionChanged

		#region RemoveItem

		/// <summary>
		/// Removes the item at the specified index in the collection.
		/// </summary>
		/// <param name="index">Index of the item to remove.</param>
		protected override void RemoveItem( int index )
		{
			this.OnItemBeingAddedOrRemoved( this[index], false );

			base.RemoveItem( index );
		}

		#endregion // RemoveItem

		#endregion // Base Overrides

		#region Indexers

		#region Indexer (string key)

		/// <summary>
		/// Gets the summary with the specified key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns>SummaryDefinition object with the specified key.</returns>
		/// <remarks>
		/// <para class="body">
		/// You can associate a <see cref="SummaryDefinition"/> object with a key using
		/// its <see cref="SummaryDefinition.Key"/> property.
		/// </para>
		/// </remarks>
		public SummaryDefinition this[string key]
		{
			get
			{
				SummaryDefinition ret = this.GetItem( key );

				if ( null == ret )
					GridUtilities.RaiseKeyNotFound( );

				return ret;
			}
		}

		#endregion // Indexer (string key)

		#endregion // Indexers

		#region Properties

		#region Public Properties

		/// <summary>
		/// Gets the associated <see cref="FieldLayout"/> that this collection belongs to.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FieldLayout</b> property returns the associated <see cref="FieldLayout"/> that this 
		/// <see cref="SummaryDefinitionCollection"/> belongs to.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/>
		public FieldLayout FieldLayout
		{
			get
			{
				return _fieldLayout;
			}
		}

		#endregion // Public Properties

		#region Private/Internal Properties

		#region CollectionVersion

		/// <summary>
		/// This version is incremented whenever the collection is changed.
		/// </summary>
		internal int CollectionVersion
		{
			get
			{
				return _collectionVersion;
			}
		}

		#endregion // CollectionVersion

		#region SummariesVersion

		// This property is bound to by elements.
		// 
		/// <summary>
		/// For internal use only. May get removed in future builds.
		/// </summary>
		[ EditorBrowsable( EditorBrowsableState.Never ), Browsable( false ), 
			DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden ) ]
		public int SummariesVersion
		{
			get
			{
				return _summariesVersion;
			}
		}

		#endregion // SummariesVersion

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a new summary.
		/// </summary>
		/// <param name="summaryKey">The key of the summary which later can be used to identify the summary.</param>
		/// <param name="calculator">The calculator to use for summarizing data.</param>
		/// <param name="sourceFieldName">The field whose data will be summarized.</param>
		/// <returns>Returns the added summary.</returns>
		/// <seealso cref="SummaryDefinition"/>
		public SummaryDefinition Add( string summaryKey, SummaryCalculator calculator, string sourceFieldName )
		{
			this.ValidateKeyDoesntExist( summaryKey );

			SummaryDefinition summary = new SummaryDefinition( );
			summary.Key = summaryKey;
			summary.Calculator = calculator;
			summary.SourceFieldName = sourceFieldName;

			this.Add( summary );
			return summary;
		}

		/// <summary>
		/// Adds a new summary.
		/// </summary>
		/// <param name="calculator">The calculator to use for summarizing data.</param>
		/// <param name="sourceFieldName">The field whose data will be summarized.</param>
		/// <returns>Returns the added summary.</returns>
		/// <seealso cref="SummaryDefinition"/>
		public SummaryDefinition Add( SummaryCalculator calculator, string sourceFieldName )
		{
			return this.Add( null, calculator, sourceFieldName );
		}

		#endregion // Add

		#region Exists

		
		
		
		/// <summary>
		/// Returns true if a summary with the specified key exists in the collection.
		/// </summary>
		/// <param name="key">Key of the summary to check if it exists in the collection.</param>
		/// <returns>True if the summary exists, false otherwise.</returns>
		public bool Exists( string key )
		{
			SummaryDefinition summary = this.GetItem( key );
			return null != summary;
		}

		#endregion // Exists

		#region Refresh

		/// <summary>
		/// Recalculates summaries.
		/// </summary>
		public void Refresh( )
		{
			this.BumpDataVersion( );
		}

		#endregion // Refresh

		#endregion // Public Methods

		#region Private/Internal Methods

		#region BumpCollectionVersion

		private void BumpCollectionVersion( )
		{
			_collectionVersion++;
			this.BumpSummariesVersion( );
		}

		#endregion // BumpCollectionVersion

		#region BumpDataVersion

		internal void BumpDataVersion( )
		{
			
			
			
			for ( int i = 0; i < this.Count; i++ )
			{
				SummaryDefinition def = this[i];
				def.BumpCalculationVersion( );
			}
		}

		#endregion // BumpDataVersion

		#region BumpSummariesVersion

		internal void BumpSummariesVersion( )
		{
			_summariesVersion++;
			this.RaisePropertyChanged( "SummariesVersion" );
			this.FieldLayout.BumpSpecialRecordsVersion( );
		}

		#endregion // BumpSummariesVersion

		#region GetItem

		/// <summary>
		/// Gets the summary by that key.
		/// </summary>
		/// <param name="key">Key to search for</param>
		/// <returns>SummaryDefinition object in this collection with the specified key or null if none found</returns>
		internal SummaryDefinition GetItem( string key )
		{
			if ( ! string.IsNullOrEmpty( key ) )
			{
				foreach ( SummaryDefinition ii in this )
				{
					if ( GridUtilities.AreKeysEqual( ii.Key, key ) )
						return ii;
				}
			}

			return null;
		}

		#endregion // GetItem

		#region GetMatchingSummaries

		/// <summary>
		/// Returns the summaries that use the specified calculator and source field. Either parameter
		/// is optional in which case that criteria will not be used for match.
		/// </summary>
		/// <param name="calculator">Calculator to match - optional.</param>
		/// <param name="sourceField">Source field to match - optional.</param>
		/// <returns>Returns the matching summaries.</returns>
		internal IEnumerable<SummaryDefinition> GetMatchingSummaries( SummaryCalculator calculator, Field sourceField )
		{
			return GridUtilities.Filter<SummaryDefinition>( this,
				delegate( SummaryDefinition def )
				{
					// SSP 6/12/08 BR33865
					// Compare names instead of the calculator instances.
					// 
					//if ( null != calculator && calculator != def.Calculator )
					if ( null != calculator && ( null == def.Calculator || calculator.Name != def.Calculator.Name ) )
						return false;

					if ( null != sourceField )
					{
						Field defSourceField = GridUtilities.GetField( sourceField.Owner, def.SourceFieldName, false );
						if ( sourceField != defSourceField )
							return false;
					}

					return true;
				}
			);
		}

		#endregion // GetMatchingSummaries

		#region InternalSetFieldLayout

		internal void InternalSetFieldLayout( FieldLayout fieldLayout )
		{
			_fieldLayout = fieldLayout;
		}

		#endregion // InternalSetFieldLayout

		#region OnItemBeingAddedOrRemoved

		/// <summary>
		/// Called when an item is added or removed from this collection.
		/// </summary>
		/// <param name="item">Item that's added or removed.</param>
		/// <param name="added">If true the item was added, false otherwise.</param>
		private void OnItemBeingAddedOrRemoved( SummaryDefinition item, bool added )
		{
			if ( added )
			{
				GridUtilities.ValidateNotNull( item );
				if ( null != item.ParentCollection )
					throw new ArgumentException( DataPresenterBase.GetString("LE_SummaryDefinitionAlreadyInCollection", item) );

				item.InternalSetParentCollection( this );
			}
			else
			{
				item.InternalSetParentCollection( null );
			}
		}

		#endregion // OnItemBeingAddedOrRemoved

		#region RaisePropertyChanged

		internal void RaisePropertyChanged( string propertyName )
		{
			this.OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion // RaisePropertyChanged

		#region RefreshSummariesAffectedBySort

		
		
		/// <summary>
		/// Recalculates summaries with calculators that are affected by sort.
		/// </summary>
		internal void RefreshSummariesAffectedBySort( )
		{
			foreach ( SummaryDefinition summary in this )
			{
				SummaryCalculator calculator = summary.Calculator;
				if ( null != calculator && calculator.IsCalculationAffectedBySort )
					summary.Refresh( );
			}
		}

		#endregion // RefreshSummariesAffectedBySort

		#region ValidateKeyDoesntExist

		private void ValidateKeyDoesntExist( string key )
		{
			SummaryDefinition summary = this.GetItem( key );
			if ( null != summary )
				throw new ArgumentException(DataPresenterBase.GetString("LE_DuplicateSummaryDefinitionKey", key));
		}

		#endregion // ValidateKeyDoesntExist

		#endregion // Private/Internal Methods

		#endregion // Methods

	}

	#endregion // SummaryDefinitionCollection Class

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