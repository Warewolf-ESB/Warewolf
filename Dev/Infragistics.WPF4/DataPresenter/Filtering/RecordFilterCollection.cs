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
using System.Xml;

namespace Infragistics.Windows.DataPresenter
{

	#region RecordFilterCollection Class

	/// <summary>
	/// A collection of <see cref="RecordFilter"/> objects.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// To specify record filters, use the FieldLayout's <see cref="Infragistics.Windows.DataPresenter.FieldLayout.RecordFilters"/> 
	/// or the RecordManager's <see cref="Infragistics.Windows.DataPresenter.RecordManager.RecordFilters"/> depending
	/// on the <see cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.RecordFilterScope"/> property setting.
	/// For the root records, FieldLayout's RecordFilters are used. For child records, by default the RecordManager's
	/// RecordFilters are used unless <b>RecordFilterScope</b> is set to <i>AllRecords</i>, in which case the FieldLayout's
	/// RecordFilters are used.
	/// </para>
	/// <para class="body">
	/// To enable the filtering UI, set the <see cref="FieldSettings.AllowRecordFiltering"/> and 
	/// <see cref="FieldLayoutSettings.FilterUIType"/> properties. With the UI enabled, when the 
	/// user enters filter criteria through the UI, the underlying RecordFilterCollection will be 
	/// modified to reflect the entered filter criteria.
	/// </para>
	/// </remarks>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.RecordFilters"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.RecordManager.RecordFilters"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.RecordFilterScope"/>
	public class RecordFilterCollection : ObservableCollection<RecordFilter>
	{
		#region Private/Internal Vars

		// Either the FieldLayout or the RecordManager that this collection belongs to.
		private object _owner; 
		private int _version;
		private int _versionNotificationsSuspendedCounter;

		#endregion // Private/Internal Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="RecordFilterCollection"/> object.
		/// </summary>
		/// <param name="owner">The FieldLayout or the RecordManager that this collection belongs to.</param>
		internal RecordFilterCollection( object owner )
		{
			Debug.Assert(owner is FieldLayout || owner is RecordManager);

			_owner = owner;
		}

		#endregion // Constructor

		#region Base Overrides

		#region ClearItems

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected override void ClearItems( )
		{
			foreach ( RecordFilter ii in this )
				this.OnItemBeingAddedOrRemoved( ii, false );

			base.ClearItems( );
		}

		#endregion // ClearItems

		#region InsertItem

		/// <summary>
		/// Inserts the specified item in the collection.
		/// </summary>
		/// <param name="index">Location in the collection at which to insert the item.</param>
		/// <param name="item">The item to insert.</param>
		protected override void InsertItem( int index, RecordFilter item )
		{
			this.OnItemBeingAddedOrRemoved( item, true );

			// Since OnItemBeingAddedOrRemoved could remove an existing filter, make sure
			// the index is within bounds.
			// 
			index = Math.Min( index, this.Count );

			base.InsertItem( index, item );
		}

		#endregion // InsertItem

		#region OnCollectionChanged

		/// <summary>
		/// Overridden. Called when the collection changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnCollectionChanged( System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			base.OnCollectionChanged( e );

			this.BumpVersion( );
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

		#region Indexer (Field field)

		/// <summary>
		/// Gets the RecordFilter associated with the specified field name. If none exists for the specified field,
		/// it creates one.
		/// </summary>
		/// <param name="field">Field object for which to get the RecordFilter.</param>
		/// <returns><see cref="RecordFilter"/> object associated with the specified field.</returns>
		public RecordFilter this[Field field]
		{
			get
			{
				return this.GetItemHelper( field, true );
			}
		}

		#endregion // Indexer (Field field)

		#region Indexer (string fieldName)

		/// <summary>
		/// Gets the RecordFilter associated with the specified field name. If none exists for the specified field,
		/// it creates one.
		/// </summary>
		/// <param name="fieldName">Name of the field for which to get the RecordFilter.</param>
		/// <returns><see cref="RecordFilter"/> object associated with the specified field name.</returns>
		public RecordFilter this[string fieldName]
		{
			get
			{
				return this.GetItemHelper( fieldName, true );
			}
		}

		#endregion // Indexer (string fieldName)

		#endregion // Indexers

		#region Properties

		#region Private/Internal Properties

		// AS 5/28/09 NA 2009.2 Undo/Redo
		#region DataPresenter
		internal DataPresenterBase DataPresenter
		{
			get
			{
				FieldLayout fl = _owner as FieldLayout;

				if (null != fl)
					return fl.DataPresenter;

				RecordManager rm = _owner as RecordManager;

				if (null != rm)
					return rm.DataPresenter;

				return null;
			}
		} 
		#endregion //DataPresenter

		#region IsVersionNotificationsSuspended

		/// <summary>
		/// Indicates if bumping Version number is suspended.
		/// </summary>
		internal bool IsVersionNotificationsSuspended
		{
			get
			{
				return _versionNotificationsSuspendedCounter > 0;
			}
		}

		#endregion // IsVersionNotificationsSuspended

		#region Owner

		/// <summary>
		/// Returns either the FieldLayout or the RecordManager that this collection belongs to.
		/// </summary>
		internal object Owner
		{
			get
			{
				return _owner;
			}
		}

		#endregion // Owner

		#endregion // Private/Internal Properties

		#region Public Properties

		#region Version

		// This version is incremented whenever the collection is changed. It's also incremented
		// whenever conditions of any record filter belonging to this collection are changed.
		/// <summary>
		/// For internal use only. May get removed in future builds.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never ), Browsable( false ),
		   DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public int Version
		{
			get
			{
				return _version;
			}
		}

		#endregion // Version

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds the specified RecordFilter to the collection. Any existing RecordFilter for the same
		/// field will be removed before adding the specified RecordFilter.
		/// </summary>
		/// <param name="item">RecordFilter to add to the collection.</param>
		/// <remarks>
		/// <para class="body">
		/// <b>Add</b> method adds the specified <see cref="RecordFilter"/> to the collection.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> If there's an existing RecordFilter associated with the same Field or FieldName 
		/// as the specified RecordFilter, the existing RecordFilter will be removed. In effect this will 
		/// perform a replace operation in such a case. This is because the collection does not allow
		/// multiple RecordFilter objects for the same field.
		/// </para>
		/// </remarks>
		public new void Add( RecordFilter item )
		{
			base.Add( item );
		}

		#endregion // Add

		#region Refresh

		/// <summary>
		/// Re-evaluates filters on all records.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Re-evaluates filters on all records. Typically there is no need to call this method as filters 
		/// automatically get re-evaluated whenever data changes.
		/// </para>
		/// </remarks>
		public void Refresh( )
		{
            this.ApplyPendingFilters();

			this.BumpVersion( );
		}

		#endregion // Refresh

		#endregion // Public Methods

		#region Private/Internal Methods

		#region AddImplicitlyCreatedRecordFilter

		// This is for preventing us from bumping the Version number. Since the record filter
		// being added is implicitly created (and empty) record filter, we don't need to take
		// any action like re-filter the records etc..., therefore we shouldn't bump version
		// number. 
		
		
		
		
		
		
		private void AddImplicitlyCreatedRecordFilter( RecordFilter filter )
		{
			this.SuspendVersionNotifications( );
			try
			{
				this.Add( filter );
			}
			finally
			{
				this.ResumeVersionNotifications( false );
			}
		}

		#endregion // AddImplicitlyCreatedRecordFilter

        #region ApplyPendingFilters






		internal void ApplyPendingFilters()
		{
			// AS 5/28/09 NA 2009.2 Undo/Redo
			// Moved to a new overload that can support undo of filter changes.
			//
			//for (int i = 0; i < this.Count; i++)
			//	this[i].ApplyPendingFilter();
			this.ApplyPendingFilters(true, false, null);
		}

		// AS 5/28/09 NA 2009.2 Undo/Redo
		// Added an overload to support undo of filter changes.
		//
		internal void ApplyPendingFilters(bool raiseEvents, bool addToUndo, FilterCell cell)
        {
			List<RecordFilter> undoFilters = new List<RecordFilter>();

			for (int i = 0; i < this.Count; i++)
			{
				RecordFilter undoFilter = this[i].ApplyPendingFilter(raiseEvents, false);

				if (null != undoFilter && addToUndo)
				{
					undoFilters.Add(undoFilter);
				}
			}

			if (undoFilters.Count > 0)
			{
				DataPresenterBase dp = this.DataPresenter;

				if (null != dp)
					dp.History.AddUndoActionInternal(new RecordFiltersAction(undoFilters.ToArray(), this.Owner, cell, dp));
			}
         }

        #endregion //ApplyPendingFilters

        #region BumpVersion

        internal void BumpVersion()
		{
			if ( ! this.IsVersionNotificationsSuspended )
			{
				_version++;

				// Notify the ResolvedRecordFilterCollection that the record manager's RecordFilters
				// has changed. ResolvedRecordFilterCollection doesn't hook into the RecordManager's 
				// RecordFilters for efficiency reasons since all we have to do notify 
				// ResolvedRecordFilterCollection here.
				//
				RecordManager rm = _owner as RecordManager;
				if ( null != rm )
				{
					ResolvedRecordFilterCollection resolvedRecordFilters = rm.RecordFiltersResolvedIfAllocated;
					if ( null != resolvedRecordFilters )
						resolvedRecordFilters.OnSourceFiltersChanged( );
				}

				this.RaisePropertyChanged( "Version" );
			}
		}

		#endregion // BumpVersion

        // JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
        #region CloneFrom

        internal void CloneFrom(RecordFilterCollection source)
        {
            bool wereFiltersCloned = false;

            foreach (RecordFilter filter in source)
            {
                if (filter.Conditions.Count > 0)
                {
                    RecordFilter clone = filter.Clone(true, this);

                    this.Add(clone);

                    wereFiltersCloned = true;
                }
            }

            if (wereFiltersCloned)
                this.BumpVersion();
        }

        #endregion //CloneFrom	
    
		#region GetItem

		internal RecordFilter GetItem( Field field, bool create )
		{
			return this.GetItemHelper( field, create );
		}

		#endregion // GetItem

		#region GetItemHelper

		/// <summary>
		/// Gets the RecordFilter by the specified key where the key is either the field object or a field name.
		/// </summary>
		/// <param name="keyObj">Either field or field name.</param>
		/// <param name="create">Specifies whether to allocate the record fitler if it hasn't already been allocated for the specified key.</param>
		/// <returns>RecordFilter object in this collection with the specified key or null if none found</returns>
		private RecordFilter GetItemHelper( object keyObj, bool create )
		{
			if ( keyObj is Field )
			{
				Field field = (Field)keyObj;

				for ( int i = 0, count = this.Count; i < count; i++ )
				{
					RecordFilter ii = this[i];
					if ( ii.Field == field )
						return ii;
				}

				if ( create )
				{
					RecordFilter filter = new RecordFilter( field );
					this.AddImplicitlyCreatedRecordFilter( filter );
					return filter;
				}
			}
			else if ( keyObj is string && ( (string)keyObj ).Length > 0 )
			{
				string fieldName = (string)keyObj;

				for ( int i = 0, count = this.Count; i < count; i++ )
				{
					RecordFilter ii = this[i];
					if ( GridUtilities.AreKeysEqual( ii.FieldName, fieldName ) )
						return ii;
				}

				if ( create )
				{
					RecordFilter filter = new RecordFilter( fieldName );
					this.AddImplicitlyCreatedRecordFilter( filter );
					return filter;
				}
			}

			return null;
		}

		#endregion // GetItemHelper

		#region OnItemBeingAddedOrRemoved

		/// <summary>
		/// Called when an item is added or removed from this collection.
		/// </summary>
		/// <param name="filter">Item that's added or removed.</param>
		/// <param name="added">If true the item was added, false otherwise.</param>
		private void OnItemBeingAddedOrRemoved( RecordFilter filter, bool added )
		{
			
			
			
			this.VerifyNotRootRecordManager( );

			if ( added )
			{
				// Null values cannot be added to the collection.
				// 
				GridUtilities.ValidateNotNull( filter );

				// First initialize the parent collection on the record filter. This will 
				// cause the record filter to validate that its not part of multiple 
				// record filter collections.
				// 
				filter.InitializeParentCollection( this );

				// Make sure there's no existing record filter for the same field. We can't have
				// more than on RecordFilter instance for a particular field in the same 
				// RecordFilterCollection. It was decided that instead of throwing an exception, 
				// we should remove the existing filter, in effect replacing the filter. This 
				// is because the data presenter creates RecordFilter objects for each field as
				// soon as the filter record is displayed. Therefore depending on the timing, 
				// when record filters are added, in window_load for example, there may already
				// be a record filter for that field in the collection. This would result in an 
				// exception unless we do a replace. We can't have multiple record filters for
				// the same field.
				// 
				object field = null != filter.Field ? (object)filter.Field : (object)filter.FieldName;
				RecordFilter existingFilter = this.GetItemHelper( field, false );
				if ( null != existingFilter )
				{
					this.SuspendVersionNotifications( );
					try
					{
						this.Remove( existingFilter );
					}
					finally
					{
						this.ResumeVersionNotifications( false );
					}
					
					//throw new ArgumentException( string.Format( "RecordFilter for the specified {0} field already exists in the collection.", field is Field ? ( (Field)field ).Name : (string)field ) );
				}
			}
		}

		#endregion // OnItemBeingAddedOrRemoved

		// JJD 03/29/12 - TFS106889 - added
		#region OnFieldLayoutInitialized

		internal void OnFieldLayoutInitialized()
		{
			// JJD 03/29/12 - TFS106889 
			// Call OnFieldLayoutInitialized on each RecordFilter in the collection
			// so they can initialize any pending conditions and raise
			// the RecordFilterDropDownItemInitializing event
			foreach (RecordFilter filter in this)
				filter.OnFieldLayoutInitialized();

		}

		#endregion //OnFieldLayoutInitialized	
    
		#region OnRecordFilterChanged

		/// <summary>
		/// Called when the conditions on the specified record filter belonging to this collection are changed.
		/// </summary>
		/// <param name="recordFilter">Changed record filter.</param>
		internal void OnRecordFilterChanged( RecordFilter recordFilter )
		{
			this.BumpVersion( );
		}

		#endregion // OnRecordFilterChanged

		#region RaisePropertyChanged

		private void RaisePropertyChanged( string propertyName )
		{
			this.OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion // RaisePropertyChanged

		#region ResumeVersionNotifications

		/// <summary>
		/// Resumes bumping version number.
		/// </summary>
		/// <param name="raiseNotification">Whether to bump Version number upon resumption.</param>
		internal void ResumeVersionNotifications( bool raiseNotification )
		{
			if ( this.IsVersionNotificationsSuspended )
			{
				_versionNotificationsSuspendedCounter--;

				if ( raiseNotification && ! this.IsVersionNotificationsSuspended )
					this.BumpVersion( );
			}
		}

		#endregion // ResumeVersionNotifications

		#region ShouldSerialize

		/// <summary>
		/// Returns if any of the RecordFilter instances in the collection have any filter conditions.
		/// </summary>
		/// <returns></returns>
		internal bool ShouldSerialize( )
		{
			for ( int i = 0, count = this.Count; i < count; i++ )
			{
				RecordFilter filter = this[i];
				if ( null != filter && filter.ShouldSerialize( ) )
					return true;
			}

			return false;
		}

		#endregion // ShouldSerialize

		#region SuspendVersionNotifications

		/// <summary>
		/// Suspends bumping Version number.
		/// </summary>
		internal void SuspendVersionNotifications( )
		{
			_versionNotificationsSuspendedCounter++;
		}

		#endregion // SuspendVersionNotifications

		#region VerifyFields

		// SSP 5/10/12 TFS111127
		// 
		internal bool VerifyFields( )
		{
			this.SuspendVersionNotifications( );
			bool itemRemoved = false;
			bool filterWithConditionsRemoved = false;

			try
			{
				for ( int i = this.Count - 1; i >= 0; i-- )
				{
					RecordFilter filter = this[i];
					Field field = filter.Field;
					if ( null != field && field.Index < 0 )
					{
						itemRemoved = true;
						filterWithConditionsRemoved = filterWithConditionsRemoved || filter.HasConditions;

						this.RemoveAt( i );
					}
				}
			}
			finally
			{
				this.ResumeVersionNotifications( itemRemoved );
			}

			return filterWithConditionsRemoved;
		} 

		#endregion // VerifyFields

		#region VerifyNotRootRecordManager

		// SSP 2/2/09
		// Root records always use field layout's record filters, even if RecordFilterScope is
		// set to SiblingRecords. Therefore we need to warn anyone trying to add filters to
		// the root record manager's RecordFilters that they will not work. This method is
		// for that.
		// 
		/// <summary>
		/// Raises InvalidOperationException if this filter collection belongs to the root
		/// record manager.
		/// </summary>
		private void VerifyNotRootRecordManager( )
		{
			RecordManager rm = _owner as RecordManager;
			DataPresenterBase dp = null != rm ? rm.DataPresenter : null;

			if ( null != rm && dp.RecordManagerIfAllocated == rm )
			{
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_28" ) );
			}
		}

		#endregion // VerifyNotRootRecordManager

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // RecordFilterCollection Class

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