using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Helpers;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using System.Windows.Threading;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	#region ViewableRecordCollection Class

	/// <summary>
	/// A read only collection of sibling records whose visibility is not set to 'Collapsed'. 
	/// </summary>
	/// <value>Contains all records whose <see cref="Record.Visibility"/> property is not set to 'Collapsed'.</value>
	/// <remarks>
	/// <para class="body">This collection contains all records whose <see cref="Record.Visibility"/> property is not set to 'Collapsed' as well as any special records (e.g. add records)</para>
	/// <para></para>
	/// <p class="note"><b>Note:</b>The records are ordered exactly as they are presented in the UI.</p>
	/// </remarks>
	/// <seealso cref="DataPresenterBase.ViewableRecords"/>
	/// <seealso cref="Record.ViewableChildRecords"/>
	public class ViewableRecordCollection : PropertyChangeNotifier, IList<Record>, IList, INotifyCollectionChanged
	{
		#region CachedInfo Class

		private class CachedInfo
		{
			#region Member Vars

			internal RecordCollectionBase _recordColl;
			internal RecordManager _recordManager;
			internal bool _isTopLevel;
			internal bool _isRootLevel;
			internal RecordType _recordsType;
			internal bool _isDataRecords;
			internal bool _isGroupByRecords;
			internal ViewableRecordCollection _vrc;
			internal Record[] _recordsToRecycle;
			internal FieldLayout _fieldLayout;
			internal DataPresenterBase _dataPresenter;
			internal ViewBase _view;

            // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing - added
            internal MainRecordSparseArray _sparseArray;

			
			
			
			
			
			
			
			
			
			
			internal bool _shouldAddSpecialRecords;

            // JJD 8/25/09 - NA 2009 Vol 2 - Record fixing
            // Cache whether record fixing is allowed for this island of records
            internal bool _isRecordFixingAllowed;

			// SSP 10/25/11 TFS91364
			// 
			/// <summary>
			/// This will be set to true when all the records are filtered out (and there are records).
			/// The purpose of this flag is to ensure that we continue showing headers with filter label
			/// icons so the records can be unfiltered in the child record collection.
			/// </summary>
			internal bool _addHeaderRecord;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="vrc"></param>
			/// <param name="recordsToRecycle"></param>
			internal CachedInfo( ViewableRecordCollection vrc, Record[] recordsToRecycle )
			{
				_vrc = vrc;
				_recordColl = vrc.RecordCollection;
				_recordManager = vrc.RecordManager;
				_isTopLevel = _recordColl.IsTopLevel;
				_isRootLevel = _recordColl.IsRootLevel;
				_recordsType = _recordColl.RecordsType;
				_isDataRecords = RecordType.DataRecord == _recordsType;
				_isGroupByRecords = RecordType.GroupByField == _recordsType;
				_recordsToRecycle = recordsToRecycle;

				// AS 8/26/09 TFS21417
				// We actually should prefer the FieldLayout of the ViewableRecordCollection 
				// since that is the class for which this class is dealing. I think this hasn't 
				// come up before because it would only be an issue if we were using heterogeneous 
				// data within the recordmanager and this class were created for the non-default 
				// field layout. That being said it is possible that the VRC doesn't have a field 
				// layout so in that case we'll fall back to using that of the RecordManager as 
				// we had previously.
				//
				//_fieldLayout = _recordManager.FieldLayout;
				_fieldLayout = vrc.FieldLayout ?? _recordManager.FieldLayout;

				_dataPresenter = _recordManager.DataPresenter;
				_view = _dataPresenter.CurrentViewInternal;
                
                // JJD 8/25/09 - NA 2009 Vol 2 - Record fixing
                // Cache whether record fixing is allowed for this island of records
                _isRecordFixingAllowed = (_view.IsFixedRecordsSupported == true &&
                                         (_view.IsFixingSupportedForNestedRecords == true || this._recordColl.ParentRecord == null));

				
				
				
				// SSP 10/25/11 TFS91364
				// 
				//_shouldAddSpecialRecords = this.ShouldAddSpecialRecords( );
				this.InitShouldAddSpecialRecords( );

                // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing - added
                _sparseArray = vrc.RecordCollectionSparseArray;

				Debug.Assert( null != _fieldLayout && null != _dataPresenter );
			}

			#endregion // Constructor

			#region AssignSeparatorVisibility

			/// <summary>
			/// Assigns SeparatorVisibility on the record based on whether it should display a separator
			/// on top or bottom. 'IsTopSpecialRecord' parameter indicates whether this is a record on
			/// top or bottom.
			/// </summary>
			/// <param name="record">Record to assign SeparatorVisibility of.</param>
			/// <param name="isTopSpecialRecord">Whether this is a record on top or bottom.</param>
			private void AssignSeparatorVisibility( Record record, bool isTopSpecialRecord )
			{
				RecordSeparatorLocation separatorLocation = null != _fieldLayout ? _fieldLayout.RecordSeparatorLocationResolved : RecordSeparatorLocation.None;

				RecordSeparatorVisibility val = RecordSeparatorVisibility.None;

				if ( record is SummaryRecord )
				{
					if ( 0 != ( RecordSeparatorLocation.SummaryRecord & separatorLocation ) )
						val = isTopSpecialRecord ? RecordSeparatorVisibility.After : RecordSeparatorVisibility.Before;
				}
				// SSP 12/11/08 - NAS9.1 Record Filtering
				// Added below else-if block.
				// 
				else if ( record is FilterRecord )
				{
					if ( 0 != ( RecordSeparatorLocation.FilterRecord & separatorLocation ) )
						val = isTopSpecialRecord ? RecordSeparatorVisibility.After : RecordSeparatorVisibility.Before;
				}
				else if ( typeof( DataRecord ) == record.GetType( ) ) // Only data record is add-record
				{
					if ( 0 != ( RecordSeparatorLocation.TemplateAddRecord & separatorLocation ) )
						val = isTopSpecialRecord ? RecordSeparatorVisibility.After : RecordSeparatorVisibility.Before;
				}

				record.SeparatorVisibility = val;
			}

			#endregion // AssignSeparatorVisibility

			#region ExtractRecord

			// SSP 12/11/08 - NAS9.1 Record Filtering
			// 
			/// <summary>
			/// Finds a record of recordType in the arr and returns it. It also nulls out the found 
			/// entry in the arr.
			/// </summary>
			/// <param name="arr"></param>
			/// <param name="recordType"></param>
			/// <returns></returns>
			internal Record ExtractRecord( Record[] arr, Type recordType )
			{
				if ( null != arr )
				{
					for ( int i = 0; i < arr.Length; i++ )
					{
						Record r = arr[i];
						// SSP 1/5/09
						// When data source is changed, we shouldn't reuse the filter record since the
						// record is still referencing the old data source' field layout.
						// 
						//if ( GridUtilities.IsObjectOfType( r, recordType ) )
						if ( GridUtilities.IsObjectOfType( r, recordType ) && _fieldLayout == r.FieldLayout )
						{
							arr[i] = null;
							return r;
						}
					}
				}

				return null;
			}

			#endregion // ExtractRecord

			#region GetAddRecordIfVisible

			
			
			
			
			private DataRecord GetAddRecordIfVisible( bool top )
			{
				if ( _isTopLevel && _isDataRecords )
				{
					AddNewRecordLocation loc = _recordManager.AddNewRecordLocation;
					bool addRecordVisibleFlag = top
						? ( AddNewRecordLocation.OnTop == loc || AddNewRecordLocation.OnTopFixed == loc )
						: ( AddNewRecordLocation.OnBottom == loc || AddNewRecordLocation.OnBottomFixed == loc );

					if ( addRecordVisibleFlag )
					{
						DataRecord addRecord = _recordManager.CurrentAddRecord;
						Debug.Assert( null != addRecord );
						if ( null != addRecord )
						{
							addRecord.FixedRecordLocationOverride = GridUtilities.GetFixedRecordLocation( top,
                                // JJD 4/8/09 - TFS6338/BR34948
                                // Only support fixed records on the bottom for root level rcds
                                //AddNewRecordLocation.OnTopFixed == loc || AddNewRecordLocation.OnBottomFixed == loc );
								AddNewRecordLocation.OnTopFixed == loc || (AddNewRecordLocation.OnBottomFixed == loc && this._isRootLevel ));

							return addRecord;
						}
					}
				}

				return null;
			}

			#endregion // GetAddRecordIfVisible

			#region GetFixedNonSpecialRecords

			/// <summary>
			/// Returns the fixed non-special (data records) on top or bottom based on 'top' parameter.
			/// </summary>
			/// <param name="list">The list to which to add the records.</param>
			/// <param name="top">If true gets the records to be displayed on top, otherwise gets the records
			/// to be displayed on the bottom.</param>
			internal void GetFixedNonSpecialRecords( List<Record> list, bool top )
			{
				int count = _recordColl.Count;
				int si = top ? 0 : count - 1;
				int step = top ? 1 : -1;

                // JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
                bool isNestedDataEnabled = this._dataPresenter.IsNestedDataDisplayEnabled && _view.SupportedDataDisplayMode != DataDisplayMode.Flat;

                // JJD 6/22/09 - NA 2009 Vol 2 - Record fixing
                // Cache the appropriate setting in a stack variable
                FixedRecordLocationInternal locationOverride = top ? FixedRecordLocationInternal.Top : this._isRootLevel ? FixedRecordLocationInternal.Bottom : FixedRecordLocationInternal.None;

                // JJD 8/25/09 - NA 2009 Vol 2 - Record fixing
                // Make sure record fixing is supported for this collection
                if (!this._isRecordFixingAllowed)
                {
                    locationOverride = FixedRecordLocationInternal.None;
                }

				// MD 6/1/10 - ChildRecordsDisplayOrder feature
				// Cache the value indicating whether child records come after the parent or not.
				bool areChildrenDisplayedAfterParent = 
					(_recordColl.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.AfterParent);

				Record lastRecord = null;
				for ( int i = si; i >= 0 && i < count; i += step )
				{
					// SSP 3/18/11 TFS26273
					// Don't allocate the record. If the record hasn't been allocated yet, then consider it not fixed.
					// 
					//Record record = _recordColl[i];
					Record record = _recordColl.SparseArray.GetItem( i, false );

                    // JJD 6/22/09 - NA 2009 Vol 2 - Record fixing
                    // If the record is expanded then treat it as non-fixed
					// SSP 3/18/11 TFS26273
					// Related to above change. Check for null.
					// 
					//if ( record.IsFixed && top == record.IsOnTopWhenFixed )
					if ( null != record && record.IsFixed && top == record.IsOnTopWhenFixed )
					{
						// JJD 11/16/11 - TFS23332
						// First reset the record's SeparatorVisibility in case the rcd was
						// fixed while inside a group of rcds and grouping has been changed/undone
						record.SeparatorVisibility = RecordSeparatorVisibility.None;

						// SSP 1/27/09
						// Enclosed the existing code into the if block. Add only visible records. Also
						// since the add-record is already added in GetSpecialRecords method, we should 
						// not add it again here.
						// 
						Visibility visibility = record.VisibilityResolved;
						if ( Visibility.Collapsed != visibility
							&& _recordManager.CurrentAddRecordInternal != record )
						{
							// MD 6/1/10 - ChildRecordsDisplayOrder feature
							// Cache whether the record is expanded with visible children because we will do this check in multiple places.
							bool hasVisibleChildren = (record.IsExpanded && record.HasVisibleChildren);

							// MD 6/1/10 - ChildRecordsDisplayOrder feature
							// Refactored: We want to un-fix any bottom fixed records which are expanded when child records come after the parent
							// or any top fixed records which are expanded when child record come before the parent, as well as any records after 
							// this record.
							//// JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
							//// Once we encounter an expanded record on bottom with children then it and
							//// all subsequent rcds should be treated as non-fixed
							//if (isNestedDataEnabled == true && top == false && locationOverride != FixedRecordLocationInternal.None)
							//{
							//    if (record.IsExpanded && record.HasVisibleChildren)
							//        locationOverride = FixedRecordLocationInternal.None;
							//}
							if (isNestedDataEnabled &&
								hasVisibleChildren && 
								locationOverride != FixedRecordLocationInternal.None )
							{
								// MD 12/2/10 - TFS36506
								// After discussing this with JoeM, we came to the conclusion that the only time a record should
								// remain fixed after it is expanded is when it is fixed to the top and it's children expand below it.
								// Keeping the record fixed in any other case either looks odd or causes the fixed record area to 
								// potentially grow too large.
								//if ((top == false && areChildrenDisplayedAfterParent) ||
								//    (top && areChildrenDisplayedAfterParent == false))
								if (top == false || areChildrenDisplayedAfterParent == false)
								{
									locationOverride = FixedRecordLocationInternal.None;
								}
							}

                            // JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
                            // Once we have encountered an expanded record (previous fixed record)
                            // then we shouldn't add subsequent rcds to the fixed list
                            if (locationOverride != FixedRecordLocationInternal.None)
                            {
                                list.Add(record);
                                lastRecord = record;
                            }

                            // JJD 4/8/09 - TFS6338/BR34948
                            // Only support fixed records on the bottom for root level rcds
                            //record.FixedRecordLocationOverride = top ? FixedRecordLocation.Top : FixedRecordLocation.Bottom;
                            // JJD 6/22/09 - NA 2009 Vol 2 - Record fixing
                            // Use the value cached above
							//record.FixedRecordLocationOverride = top ? FixedRecordLocationInternal.Top : this._isRootLevel ? FixedRecordLocationInternal.Bottom : FixedRecordLocationInternal.None;
                            record.FixedRecordLocationOverride = locationOverride;

							// MD 6/1/10 - ChildRecordsDisplayOrder feature
							// Refactored: If this record is expanded, unfix any subsequent fixed records if this is on the top and child records display after the parent
							// or this is on the bottom and child records display beofre the parent.
							//// JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
							//// Once we encounter an expanded record on top with children then all subsequent
							//// rcds should be treated as non-fixed
							//if (isNestedDataEnabled == true && top == true && locationOverride != FixedRecordLocationInternal.None)
							//{
							//    if (record.IsExpanded && record.HasVisibleChildren)
							//        locationOverride = FixedRecordLocationInternal.None;
							//}
							// JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
							// Once we encounter an expanded record on top with children then all subsequent
							// rcds should be treated as non-fixed
							if (isNestedDataEnabled &&
								hasVisibleChildren &&
								locationOverride != FixedRecordLocationInternal.None)
							{
								if ((top && areChildrenDisplayedAfterParent) ||
									(top == false && areChildrenDisplayedAfterParent == false))
								{
									locationOverride = FixedRecordLocationInternal.None;
								}
							}
						}
					}
					else
						break;
				}

				// Assign record separator visibility.
				// 
				if ( null != lastRecord )
				{
					RecordSeparatorLocation separatorLocation = null != _fieldLayout ? _fieldLayout.RecordSeparatorLocationResolved : RecordSeparatorLocation.None;
					if ( 0 != ( RecordSeparatorLocation.FixedRecords & separatorLocation ) )
						lastRecord.SeparatorVisibility = top ? RecordSeparatorVisibility.After : RecordSeparatorVisibility.Before;
				}
			}

			#endregion // GetFixedNonSpecialRecords

			#region GetOrderedSpecialRecordTypes

			/// <summary>
			/// Returns an array of types that specify the special records that are to be displayed on top 
			/// or bottom based on the value of 'top' parameter. The order of types is also the order of 
			/// records. The order is based on the special record order settings.
			/// </summary>
			/// <param name="top">If true then returns special records to be displayed on top. Otherwise
			/// returns special records to be displayed on the bottom.</param>
			/// <returns>An array of types that specify the special records.</returns>
			private Type[] GetOrderedSpecialRecordTypes( bool top )
			{
				long summaryRecord = -1;
				long addRecord = -1;
				// SSP 12/11/08 - NAS9.1 Record Filtering
				// 
				long filterRecord = -1;

				for ( int step = 0; step <= 1; step++ )
				{
					FieldLayoutSettings settings;
					if ( step == 0 )
						settings = null != _fieldLayout ? _fieldLayout.SettingsIfAllocated : null;
					else
						settings = null != _dataPresenter ? _dataPresenter.FieldLayoutSettingsIfAllocated : null;

					if ( null != settings )
					{
						SpecialRecordOrder order = settings.SpecialRecordOrder;
						if ( null != order )
						{
							if ( summaryRecord < 0 )
								summaryRecord = order.SummaryRecord;

							if ( addRecord < 0 )
								addRecord = order.AddRecord;

							// SSP 12/11/08 - NAS9.1 Record Filtering
							// 
							if ( filterRecord < 0 )
								filterRecord = order.FilterRecord;
						}
					}
				}

				// On top, summary record occurs before add-record. On bottom, add-record occurs
				// before summary record. Also multiplcation by 256 is to amplify and give higher
				// priority to values explicitly set by the user so the default order values that
				// we are adding do not take precedence.
				// 
				summaryRecord = 256 * summaryRecord + ( top ? -20 : 20 );
				// SSP 12/11/08 - NAS9.1 Record Filtering
				// Added filterRecord entry.
				filterRecord = 256 * filterRecord + ( top ? -15 : 15 );
				addRecord = 256 * addRecord + ( top ? -10 : 10 );

				Type[] recordTypes = new Type[] { 
					typeof( DataRecord ), // For add-record template
					// SSP 12/11/08 - NAS9.1 Record Filtering
					// Added filterRecord entry.
					typeof( FilterRecord ),
					typeof( SummaryRecord )
				};

				long[] sequence = new long[] { 
					addRecord,
					// SSP 12/11/08 - NAS9.1 Record Filtering
					// Added filterRecord entry.
					filterRecord,
					summaryRecord
				};

				// Sort by the sequence numbers so the rows are displayed in that order.
				//
				for ( int i = 0; i < sequence.Length; i++ )
				{
					for ( int j = 1 + i; j < sequence.Length; j++ )
					{
						if ( sequence[i] > sequence[j] )
						{
							GridUtilities.Swap( sequence, i, j );
							GridUtilities.Swap( recordTypes, i, j );
						}
					}
				}

				return recordTypes;
			}

			#endregion // GetOrderedSpecialRecordTypes

			#region GetSpecialRecordsHelper

			/// <summary>
			/// Gets the special records of specified record type that are to be displayed on top or bottom
			/// based on 'top' parameter. The records are added to the specified list.
			/// </summary>
			/// <param name="recordType"></param>
			/// <param name="list"></param>
			/// <param name="top"></param>
			internal void GetSpecialRecordsHelper( Type recordType, List<Record> list, bool top )
			{
				if ( typeof( DataRecord ) == recordType )
				{
					
					
					
					DataRecord addRecord = this.GetAddRecordIfVisible( top );
					if ( null != addRecord )
						list.Add( addRecord );

					return;
				}

				if ( typeof( SummaryRecord ) == recordType )
				{
					if ( ( _isDataRecords || _isGroupByRecords ) && null != _view && _view.IsSummaryRecordSupported
						
						
						
						&& _shouldAddSpecialRecords
						// JJD 10/24/11 - TFS86447
						// Make sure that the DataSource is not null before adding any summary rcds
						&& null != _recordManager.DataSource
						)
					{
						SummaryResultCollection summaryResults = _recordColl.SummaryResults;
						if ( null != summaryResults )
						{
							SummaryRecord sr;
                            // JJD 4/8/09 - TFS6338/BR34948
                            // Pass in isRootLevel flag
							//sr = summaryResults.GetSummaryRecord( top, top, _recordsToRecycle );
							sr = summaryResults.GetSummaryRecord( top, top, _recordsToRecycle, this._isRootLevel );
							if ( null != sr )
							{
								
								
								
								
                                
                                
                                
                                
                                
                                

								list.Add( sr );
							}

                            // JJD 4/8/09 - TFS6338/BR34948
                            // Pass in isRootLevel flag
							//sr = summaryResults.GetSummaryRecord( top, !top, _recordsToRecycle );
							sr = summaryResults.GetSummaryRecord( top, !top, _recordsToRecycle, this._isRootLevel );
							if ( null != sr )
							{
								
								
								
								
                                
                                
                                
                                
                                
                                

								list.Add( sr );
							}
						}
					}

					return;
				}

				// SSP 12/11/08 - NAS9.1 Record Filtering
				// 
				if ( typeof( FilterRecord ) == recordType )
				{
					FilterRecordLocation loc = _fieldLayout.FilterRecordLocationResolved;
					bool addFilterRecord = top
						? FilterRecordLocation.OnTop == loc || FilterRecordLocation.OnTopFixed == loc
						: FilterRecordLocation.OnBottom == loc || FilterRecordLocation.OnBottomFixed == loc;

					if ( addFilterRecord )
					{
						FilterRecord filterRecord = (FilterRecord)this.ExtractRecord( _recordsToRecycle, typeof( FilterRecord ) );

						if ( this.IsFilterRecordVisibleHelper( filterRecord ) )
						{
							if ( null == filterRecord )
								filterRecord = new FilterRecord( _fieldLayout, _recordColl );

                            // JJD 4/8/09 - TFS6338/BR34948
                            // Only support fixed records on the bottom for root level rcds
                            //filterRecord.FixedRecordLocationOverride = GridUtilities.GetFixedRecordLocation(top,
                            //        FilterRecordLocation.OnTopFixed == loc || FilterRecordLocation.OnBottomFixed == loc );
                            filterRecord.FixedRecordLocationOverride = GridUtilities.GetFixedRecordLocation(top,
									FilterRecordLocation.OnTopFixed == loc || (FilterRecordLocation.OnBottomFixed == loc && this._isRootLevel ) );

							
							
							
							
                            
                            
                            
                            
                            //if (!_isRootLevel)
								//filterRecord.FixedRecordLocationOverride = FixedRecordLocation.None;

							list.Add( filterRecord );
						}
					}

					return;
				}
			}

			/// <summary>
			/// Gets the special records to be displayed on top or bottom based on 'top' parameter. The
			/// records are added to the specified list.
			/// </summary>
			/// <param name="list">This is the list to which special records will be added.</param>
			/// <param name="top">If true gets the top special records. If false gets the bottom special records.</param>
			internal void GetSpecialRecordsHelper( List<Record> list, bool top )
			{
				Type[] recordTypes = this.GetOrderedSpecialRecordTypes( top );

				int origListCount = list.Count;

				for ( int i = 0; i < recordTypes.Length; i++ )
					this.GetSpecialRecordsHelper( recordTypes[i], list, top );

				// SSP 2/9/09 TFS13459
				// We decided to change the behavior so that now fixed records will be forced to appear after
				// the non-fixed records on top (and the other way around for bottom). Before this, we were forcing
				// records to be fixed if they were before any fixed records when on top (and the other way around 
				// for bottom).
				// 
				// I noticed this while fixing TFS13459. Ignore items in the list that pre-existed
				// before this method was called. Although this won't make any difference right now
				// since this method is always getting called with an empty list currently.
				//
				// ------------------------------------------------------------------------------------------------
				



				int newListCount = list.Count;
				if ( origListCount < newListCount )
				{
					for ( int i = 0; i < newListCount; i++ )
						this.AssignSeparatorVisibility( list[i], top );

					// The following code does a two pass. In first pass it adds fixed records to the list.
					// Then in the second pass, it adds scrolling records to the list. Then after the loop
					// it removes the records in the original order.
					// 
					for ( int pass = 0; pass < 2; pass++ )
					{
						for ( int i = origListCount; i < newListCount; i++ )
						{
							Record ii = list[i];
							bool isFixed = GridUtilities.IsFixed( ii.FixedRecordLocationOverride );

							// If top records then first add fixed records then scrolling records.
							// If bottom records then first add scrolling records and then fixed records.
							// 
							if ( ( 0 == pass ) == ( top == isFixed ) )
								list.Add( ii );
						}
					}

					list.RemoveRange( origListCount, newListCount - origListCount );
				}
				// ------------------------------------------------------------------------------------------------
			}

			#endregion // GetSpecialRecordsHelper

			#region IsAddRecordVisible

			
			
			private bool IsAddRecordVisible( )
			{
				return null != this.GetAddRecordIfVisible( true )
					|| null != this.GetAddRecordIfVisible( false );
			}

			#endregion // IsAddRecordVisible

			#region IsFilterRecordVisibleHelper

			/// <summary>
			/// Returns true if the filter record is visible.
			/// </summary>
			private bool IsFilterRecordVisibleHelper( FilterRecord currFilterRecord )
			{
				FieldLayout fieldLayout = _fieldLayout;

				bool hasVisibleFields;

				return _shouldAddSpecialRecords
					&& FilterUIType.FilterRecord == fieldLayout.FilterUITypeResolved
					&& ( null == currFilterRecord || Visibility.Visible == currFilterRecord.VisibilityResolvedBase )
					// In case of group-by rows, display the filter record with the
					// top level group-by rows only. Don't display the filter record
					// at every island since filter criteria is per record manager.
					//
					&& _isTopLevel
					// Expandable field records' viewable records shouldn't have filter record in it.
					// 
					&& ( _isDataRecords || _isGroupByRecords )
					// Card-view doesn't support filter record yet. Reporting shouldn't display the filter
					// record either.
					//
					&& _view.IsFilterRecordSupported
					// Filter record should be hidden if none of the visible fields allow filtering.
					// 
					// SSP 9/8/09 TFS21710
					// We should display the record filter if there are 0 visible fields and 
					// AllowRecordFiltering has been set to true on the field layout or the data presenter
					// field settings. This is to prevent the filter record from abruptly going away or
					// when the user hides all the fields via the field chooser, although it still can 
					// happen in some situations however the likeliness is greatly reduced.
					// --------------------------------------------------------------------------------------
					//&& !FilterRecord.AllFieldsDisallowFilteringHelper( fieldLayout )
					&& ( FilterRecord.DoesAnyFieldAllowFiltering( fieldLayout, out hasVisibleFields )
						|| ! hasVisibleFields && _fieldLayout.AllowRecordFilteringResolvedDefault )
					// --------------------------------------------------------------------------------------
					// This is to prevent the filter record's row selector from being displayed
					// even when the data presenter is not bound to any data.
					// 
					&& null != _recordManager.DataSource;
			}

			#endregion // IsFilterRecordVisibleHelper

            // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing - added
            #region ProcessPendingUnfixNonSpecialRecords

            internal bool ProcessPendingUnfixNonSpecialRecords(Queue<Record> queue)
            {
                if (queue == null || queue.Count == 0)
                    return true;

                bool rcdMoved = false;

                foreach (Record rcd in queue)
                {
                    // bypass records that have since been marked fixed
                    if (rcd.FixedLocation != FixedRecordLocation.Scrollable)
                        continue;

                    int oldIndex = this._sparseArray.IndexOf(rcd);

                    if (oldIndex >= 0)
                    {
                        this._vrc.EnsureCorrectSortPosition(rcd);
                        rcdMoved = true;
                    }
                }

                return !rcdMoved;
            }

            #endregion //ProcessPendingUnfixNonSpecialRecords

			#region ResetCachedSettings

			internal static void ResetCachedSettings( IEnumerable<Record> records )
			{
				if ( null != records )
				{
					foreach ( Record record in records )
					{
						record.FixedRecordLocationOverride = FixedRecordLocationInternal.Default;
						record.SeparatorVisibility = RecordSeparatorVisibility.None;
					}
				}
			}

			#endregion // ResetCachedSettings

			#region ShouldAddSpecialRecords

			
			
			
			/// <summary>
			/// Figures out if we should add special records, like the filter record or the summary record,
			/// to the viewable record.
			/// </summary>
			/// <returns></returns>
			// SSP 10/25/11 TFS91364
			// 
			//private bool ShouldAddSpecialRecords( )
			private void InitShouldAddSpecialRecords( )
			{
				bool shouldAddSpecialRecords = _isRootLevel;
				if ( ! shouldAddSpecialRecords )
				{
					SparseArray arr = _recordColl.SparseArray;
					int recordCount = null != arr ? arr.Count : 0;
					int visibleCount = arr is MainRecordSparseArray ? ( (MainRecordSparseArray)arr ).VisibleCount : recordCount;
					shouldAddSpecialRecords = visibleCount > 0 || this.IsAddRecordVisible( );

					if ( ! shouldAddSpecialRecords )
					{
						// If the user filters out all the records, then we should not hide the entire child record
						// collection because that will remove the filter UI as well, preventing the user from
						// unfiltering records.
						// 
						if ( recordCount > 0 )
						{
							shouldAddSpecialRecords = true;

							// SSP 10/25/11 TFS91364
							// When using label icons filter ui type, if all records of a child collection are filtered out,
							// we need to make sure we show at least the field labels so the user can un-filter the records.
							// The below logic adds a header record if there are no other visible records, and there are 
							// records. Initialize the new _addHeaderRecord flag.
							// 
							if ( _isTopLevel && ( _isDataRecords || _isGroupByRecords ) 
								&& FilterUIType.LabelIcons == _fieldLayout.FilterUITypeResolved 
								&& _dataPresenter.IsExportControl == false
								&& _dataPresenter.IsReportControl == false)
								_addHeaderRecord = true;
						}
					}
				}

				
				
				
				_shouldAddSpecialRecords = shouldAddSpecialRecords;
			}

			#endregion // ShouldAddSpecialRecords

            // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing - added
            #region VerifyFixedNonSpecialRecords

            internal bool VerifyFixedNonSpecialRecords(List<Record> list, bool onTop)
            {
                if (list == null || list.Count == 0)
                    return true;

                int listCount = list.Count;
                int overallRecordCount = this._sparseArray.Count;

                Debug.Assert(overallRecordCount >= listCount, "We can't have more fixed records than records");

                if ( overallRecordCount < listCount )
                    return false;

                int startIndex = onTop ? 0 : overallRecordCount - 1;
                int endIndex = onTop ? listCount - 1 : overallRecordCount - listCount;

                if (this._isDataRecords)
                {
                    DataRecord drFromSparseArray;
                    DataRecord drFromFixedListList;
                    
                    if (onTop)
                    {
                        drFromSparseArray = this._sparseArray[0] as DataRecord;
                        drFromFixedListList = list[0] as DataRecord;
                    }
                    else
                    {
                        drFromSparseArray = this._sparseArray[overallRecordCount - 1] as DataRecord;
                        drFromFixedListList = list[listCount - 1] as DataRecord;
                    }

                    Debug.Assert(drFromFixedListList != null, "Fixed record list should contain DataRecords");

                    if (drFromSparseArray != null &&
                        drFromFixedListList != drFromSparseArray &&
                         drFromSparseArray.IsAddRecord)
                    {
                        if ( onTop )
                        {
                            startIndex++;
                            endIndex++;
                        }
                        else
                        {
                            startIndex--;
                            endIndex--;
                        }
                    }
                }

                int listIndex = onTop ? 0 : listCount - 1;
                int step = onTop ? 1 : -1;

                bool recordPositionChanged = false;

                for (int i = startIndex; onTop ? i <= endIndex : i >= endIndex; i += step)
                {
                    Record rcdFromSparseArray = this._sparseArray[i] as Record;
                    Record rcdFromList = list[listIndex];

                    if (rcdFromList != rcdFromSparseArray)
                    {
                        int oldIndex = this._sparseArray.IndexOf(rcdFromList);

                        // If the record is now longer in the list just remove it.
                        // Otherwise, move it to the correct slot
                        if (oldIndex < 0)
                        {
                            // remove the record from the list of fixed records
                            list.RemoveAt(listIndex);

                            // adjust the endindex so we stop at the appropriate spot
                            endIndex -= step;

                            // make sure the index in the sparsearray doesn't move 
                            // for the next iteration by subtracting the step value
                            i -= step;

                            // if we are processing top fixed rcds then make
                            // sure the index in the list doesn't change for the
                            // next iteration
                            if ( onTop )
                                listIndex -= step;

                        }
                        else
                        {
                            // we need to move the fixed rcd to
                            // the proper position in the sparse array
                            this._sparseArray.RemoveAt(oldIndex);
                            this._sparseArray.Insert(i, rcdFromList);

                            recordPositionChanged = true;
                        }
                    }

                    listIndex += step;
                }

                return !recordPositionChanged;
            }

            #endregion //VerifyFixedNonSpecialRecords
        }

		#endregion // CachedInfo Class

		#region Private Members

		private RecordCollectionBase _recordCollection;
		private List<DataRecord> _recordsInsertedAtTop;
		private List<Record> _fixedRecordsOnTop;
		private List<Record> _fixedRecordsOnBottom;
		private List<Record> _specialRecordsOnTop;
		private List<Record> _specialRecordsOnBottom;

        // JJD 6/17/09 - NA 2009 Vol 2 - Record fixing
		private List<Record> _fixedNonSpecialRecordsOnTop;
		private List<Record> _fixedNonSpecialRecordsOnBottom;
		private Queue<Record> _pendingUnfixedNonSpecialRecords;

		// SSP 5/12/09 TFS16576
		// 
		//internal int _lastScrollCount;
		private int _lastScrollCount = -1;
		private DataRecord _lastAddRecord;

		private WeakReference _lastRecordList;

		private bool _isResetNotificationNeeded;
		private bool _bypassRecordCollectionNotifications;

		
		
		private bool _inVerifySpecialRecords;
		private int _verifiedSpecialRecordsVersion;
		private PropertyValueTracker _specialRecordsVersionTracker;

        // JJD 8/27/09 - TFS21513
        // Added constant to let us know that the fixed record order needs to be verified
        private readonly static int FixedRecordOrderIsDirty = -5;


        // JJD 9/23/08 - added support for printing
        // Lazily maintain a map between print records and their associated records
        // in the source ui datapresenter
        private Dictionary<SummaryDisplayAreaContext, SummaryRecord> _associatedRecordMap;
        private ViewableRecordCollection _associatedViewableRecordCollection;
        private int _reportVersion;


		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		private bool _inEnsureFiltersEvaluated;
		internal int _verifiedFiltersVersion;
		private PropertyValueTracker _filtersVersionTracker;
		private int _collectionChanged_RaiseResetCounter;

        // JJD 6/2/09 - TFS18108 - added
        private PropertyValueTracker _fieldsVersionTracker;

		#endregion //Private Members	

		#region Constructor

		internal ViewableRecordCollection(RecordCollectionBase recordCollection)
		{
			GridUtilities.ValidateNotNull( recordCollection );
			this._recordCollection = recordCollection;

			this._recordCollection.CollectionChanged += new NotifyCollectionChangedEventHandler( OnSorted_CollectionChanged );
			this._recordCollection.PropertyChanged += new PropertyChangedEventHandler( OnSorted_PropertyChanged );
		}

		#endregion //Constructor

		#region Base Overrides

		#region OnFirstListenerAdding

		// SSP 2/3/09 - NAS9.1 Record Filtering
		// Added OnFirstListenerAdding method to the PropertyChangeNotifier. ViewableRecordCollection 
		// uses this to verify itself before anyone hooks into it so it doesn't raise notifications 
		// after it's hooked into.
		// 
		/// <summary>
        /// Overridden. Called when the first listener is being added to the <see cref="Infragistics.PropertyChangeNotifier.PropertyChanged"/> event.
		/// </summary>
        /// <seealso cref="Infragistics.PropertyChangeNotifier.HasListeners"/>
        /// <seealso cref="Infragistics.PropertyChangeNotifier.PropertyChanged"/>
		/// <seealso cref="Infragistics.PropertyChangeNotifier.OnHasListenersChanged"/>
		protected override void OnFirstListenerAdding( )
		{
			base.OnFirstListenerAdding( );

			this.OnFirstListenerAddingInternal( );
		}

		#endregion // OnFirstListenerAdding

		#region OnHasListenersChanged

		
		
		/// <summary>
		/// Overridden. Called when value of HasListeners property chnages.
		/// </summary>
		protected override void OnHasListenersChanged( )
		{
			base.OnHasListenersChanged( );

			this.VerifyHasAnyListeners( );
		}

		#endregion // OnHasListenersChanged

		#endregion // Base Overrides

		#region Event handlers

		void OnSorted_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
		{
			if ( this._bypassRecordCollectionNotifications == false )
			{
				this.RaisePropertyChangedEvent( e.PropertyName );
				//this.DirtyScrollCounts();
				// SSP 2/12/09 TFS12467 - Optimizations
				// This shouldn't be necessary. Not only that it would cause performance issue
				// since it will synrhonously calculate the scroll count.
				// 
				//this.VerifyScrollCount( );
			}
		}

		void OnSorted_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			NotifyCollectionChangedEventArgs args = e;

			if ( e.Action != NotifyCollectionChangedAction.Reset )
			{
				// bypass notifications unless the flag is set
				if ( this._bypassRecordCollectionNotifications == true )
					return;

				switch ( e.Action )
				{
					// the Remove, Replace and Move notificatication can not be handled
					// here because we have on way of getting the old and new visible indexes.
					// this needs to be handled thru explicit method calls before the
					// underlying list is updated
					case NotifyCollectionChangedAction.Remove:
					case NotifyCollectionChangedAction.Replace:
					case NotifyCollectionChangedAction.Move:


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

						args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
						break;

					#region Add

					case NotifyCollectionChangedAction.Add:
						{
							IList items = e.NewItems;
							int startngIndex = e.NewStartingIndex;

							if ( items != null &&
								 items.Count == 1 )
							{
								Record record = items[0] as Record;

								// if the visibility is collapsed then we don't pass
								// then notification on since we only contain 
								// visible records
								if ( record == null ||
									record.Visibility == Visibility.Collapsed )
									return;

								DataRecord dr = record as DataRecord;

								// check for and special case add record add/remove
								// we can ignore it since it was already in our list
								// as a special record
								if ( dr != null && dr.IsAddRecord )
								{
									this.VerifyCachedRecordStatus( dr );
									return;
								}

								if ( dr == this._lastAddRecord )
									this._lastAddRecord = null;

								// get the visible index
								startngIndex = this.RecordCollectionSparseArray.VisibleIndexOf( record );

								// SSP 5/15/09 TFS16576
								// If there are record filters applied and the new record doesn't match the filter
								// conditions then it would be filtered out and its visible index would be -1.
								// In that case we should simply return.
								// 
								if ( startngIndex < 0 )
									return;

                                // JJD 6/23/09 - NA 2009 Vol 2 - Record fixing
                                #region See if the record's position intersects with a fixed area

                                bool isIndexInFixedArea = false;

                                if (this._fixedNonSpecialRecordsOnTop != null)
                                    isIndexInFixedArea = startngIndex < this._fixedNonSpecialRecordsOnTop.Count;

                                if (isIndexInFixedArea == false &&
                                    this._fixedNonSpecialRecordsOnBottom != null)
                                    isIndexInFixedArea = startngIndex >= this.RecordCollectionSparseArray.Count - this._fixedNonSpecialRecordsOnBottom.Count;

                                // JJD 6/23/09 - NA 2009 Vol 2 - Record fixing
                                // if the record is iin the fixed area then just dirty the special records
                                // and return since that will generate a reset noticifaction in this
                                // case
                                if (isIndexInFixedArea)
                                {
                                    this.DirtySpecialRecords(true);
                                    return;
                                }

                                #endregion //See if the record's position intersects with a fixed area	

								// adjust the index to account for special records on top

                                if (this._specialRecordsOnTop != null)
                                {
                                    // JJD 11/17/08 - TFS6743/BR35763 
                                    // Adjust the index don't set it
                                    //startngIndex = this._specialRecordsOnTop.Count;
                                    startngIndex += this._specialRecordsOnTop.Count;
                                }
    
								Debug.Assert( startngIndex >= 0 );

								if ( startngIndex < 0 )
								{
									args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
									break;
								}
							}
							else
							{
								args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
								break;
							}

							if ( e.Action == NotifyCollectionChangedAction.Add )
								args = new NotifyCollectionChangedEventArgs( e.Action, e.NewItems, startngIndex );
							else
								args = new NotifyCollectionChangedEventArgs( e.Action, e.OldItems, startngIndex );
						}
						break;

					#endregion //Add

				}
			}

			// clear the _isResetNotificationNeeded flag on a reset notification 
			if ( args.Action == NotifyCollectionChangedAction.Reset )
				this._isResetNotificationNeeded = false;

			
			
			
			
			this.RaiseCollectionChanged( args );

			// SSP 2/12/09 TFS12467 - Optimizations
			// This shouldn't be necessary. Not only that it would cause performance issue
			// since it will synrhonously calculate the scroll count. We are dirtying the
			// scrollcount when the sorted collection changes.
			// 
			//this.VerifyScrollCount( );

			// SSP 3/23/12 TFS99274
			// 
			if ( args.Action == NotifyCollectionChangedAction.Reset )
			{
				RecordManager recordManager = this.RecordManager;
				if ( null != recordManager )
					recordManager.NotifyCalcAdapterHelper( DataChangeType.Reset, null, null );
			}
		}

		#endregion //Event handlers
 
		#region Properties

			#region Public Properties

				#region CountOfFixedRecordsOnBottom

		/// <summary>
		/// The number of records at the end of the list that are fixed (read-only).
		/// </summary>
		/// <value>An integer value from 0 to the value of the <see cref="Count"/> property that indicates how many records at the end of the list are fixed.</value>
		/// <remarks>
		/// Fixed records don't scroll until all of their scrollable sibling records have been scrolled out of view. Root level fixed records never scroll if there is enough space to display them all.
		/// <para></para>
		/// <p class="note"><b>Note:</b>Not all views support the fixing of records visually. For those views that don't support fixing, e.g. carousel view, fixing a record will just determine its order in the list.</p>
		/// </remarks>
		/// <seealso cref="CountOfFixedRecordsOnTop"/>
		/// <seealso cref="Record.IsFixed"/>
		public int CountOfFixedRecordsOnBottom
		{
			get
			{
				this.Verify( );

				return null != _fixedRecordsOnBottom 
					? _fixedRecordsOnBottom.Count
					: 0;
			}
		}

				#endregion //CountOfFixedRecordsOnBottom	

				#region CountOfFixedRecordsOnTop

		/// <summary>
		/// The number of records at the beginning of the list that are fixed (read-only).
		/// </summary>
		/// <value>An integer value from 0 to the value of the <see cref="Count"/> property that indicates how many records at the begiining of the list are fixed.</value>
		/// <remarks>
		/// Fixed records don't scroll until all of their scrollable sibling records have been scrolled out of view. Root level fixed records never scroll if there is enough space to display them all.
		/// <para></para>
		/// <p class="note"><b>Note:</b>Not all views support the fixing of records visually. For those views that don't support fixing, e.g. carousel view, fixing a record will just determine its order in the list.</p>
		/// </remarks>
		/// <seealso cref="CountOfFixedRecordsOnBottom"/>
		/// <seealso cref="Record.IsFixed"/>
		public int CountOfFixedRecordsOnTop
		{
			get
			{
				this.Verify( );
                
				return null != _fixedRecordsOnTop 
					? _fixedRecordsOnTop.Count
					: 0;
			}
		}

				#endregion //CountOfFixedRecordsOnTop	

				#region IsSpecialRecordsDirty

		/// <summary>
		/// Indicates if the special records in the collection need to be verified.
		/// </summary>
		internal bool IsSpecialRecordsDirty
		{
			get
			{
				FieldLayout fl = this.FieldLayout;
				return null != fl && _verifiedSpecialRecordsVersion != fl.SpecialRecordsVersion;
			}
		}

				#endregion // IsSpecialRecordsDirty
    
			#endregion //Public Properties

			#region Internal Properties

				#region AreRecordFiltersDirty

		// SSP 12/03/10 TFS60379
		// 
		/// <summary>
		/// Indicates if the record filters version is different.
		/// </summary>
		internal bool AreRecordFiltersDirty
		{
			get
			{
				RecordManager rm = this.RecordManager;

				ResolvedRecordFilterCollection filters = null != rm ? rm.RecordFiltersResolved : null;
				return null != filters && _verifiedFiltersVersion != filters.Version;
			}
		} 

				#endregion // AreRecordFiltersDirty

                // JJD 4/3/08 - added support for printing
                #region AssociatedViewableRecordCollection


        // Lazily maintain a map between print records and their associated records
        // in the source ui datapresenter
        internal ViewableRecordCollection AssociatedViewableRecordCollection
        {
            get
            {
                // MBS 7/29/09 - NA9.2 Excel Exporting
                //DataPresenterReportControl dprc = this.RecordCollection.DataPresenter as DataPresenterReportControl;
                DataPresenterExportControlBase dprc = this.RecordCollection.DataPresenter as DataPresenterExportControlBase;

                Debug.Assert(dprc != null);

                if (dprc == null)
                    return null;

                if (dprc.ReportVersion != this._reportVersion)
                {
                    this._reportVersion = dprc.ReportVersion;
                    this._associatedViewableRecordCollection = null;
                }

                if ( this._associatedViewableRecordCollection == null )
                {
                    Record parentRecord = this.RecordCollection.ParentRecord;

                    if (parentRecord == null)
                        this._associatedViewableRecordCollection = dprc.ViewableRecords;
                    else
                    {
                        Record associatedParentRecord = parentRecord.GetAssociatedRecord();

                        Debug.Assert(associatedParentRecord != null);

                        if (associatedParentRecord == null)
                            return null;

                        this._associatedViewableRecordCollection = associatedParentRecord.ViewableChildRecords;
                    }
                }

                return this._associatedViewableRecordCollection;
            }
        }

                #endregion //AssociatedViewableRecordManager

				#region CountOfNonSpecialRecords

		// SSP 5/12/09 TFS16576
		// 
		/// <summary>
		/// Returns the count of non-special records (data records or group-by records)
		/// in this collection.
		/// </summary>
		internal int CountOfNonSpecialRecords
		{
			get
			{
				return this.RecordCollectionSparseArray.VisibleCount;
			}
		}

				#endregion // CountOfNonSpecialRecords

				#region CountOfSpecialRecordsOnBottom

		internal int CountOfSpecialRecordsOnBottom
		{
			get
			{
				this.Verify( );

				if (this._specialRecordsOnBottom != null)
					return this._specialRecordsOnBottom.Count;

				return 0;
			}
		}

				#endregion //CountOfSpecialRecordsOnBottom	

				#region CountOfSpecialRecordsOnTop

		internal int CountOfSpecialRecordsOnTop
		{
			get
			{
				this.Verify( );

				if (this._specialRecordsOnTop != null)
					return this._specialRecordsOnTop.Count;

				return 0;
			}
		}

				#endregion //CountOfSpecialRecordsOnTop	

				#region FieldLayout

		/// <summary>
		/// Returns the associated field layout.
		/// </summary>
		internal FieldLayout FieldLayout
		{
			get
			{
				return _recordCollection.FieldLayout;
			}
		}

				#endregion // FieldLayout

				#region HasAnyListeners

		
		
		/// <summary>
		/// Returns true if anyone's hooked into CollectionChanged or PropertyChanged events.
		/// </summary>
		internal bool HasAnyListeners
		{
			get
			{
				return null != _collectionChanged || this.HasListeners;
			}
		}

				#endregion // HasAnyListeners

				#region IsAddRecordOnTop

		internal bool IsAddRecordOnTop
		{
			get
			{
				return (this._recordCollection is DataRecordCollection) 
						&& this._recordCollection.ParentRecordManager.IsAddRecordOnTop;
			}
		}

				#endregion //IsAddRecordOnTop	

                #region VerificationNeeded

        // SSP 5/12/10 TFS32148
        // 
        internal bool VerificationNeeded
        {
            get
            {
                if ( this.IsSpecialRecordsDirty )
                    return true;

				
				
				
				if ( this.AreRecordFiltersDirty )
					return true;

                return false;
            }
        }

                #endregion // VerificationNeeded

				#region LastRecordList

		internal RecordListControl LastRecordList
		{
			get
			{
				if (this._lastRecordList == null)
					return null;

				RecordListControl rlc = Utilities.GetWeakReferenceTargetSafe(this._lastRecordList) as RecordListControl;

				if (rlc == null)
					return null;

				// verify the CellPresenter is still valid
				if (rlc.ItemsSource != this)
				{
					this._lastRecordList = null;
					return null;
				}

				return rlc;
			}
			set
			{
				if (value == null)
					this._lastRecordList = null;
				else if (value != this.LastRecordList)
					this._lastRecordList = new WeakReference(value);
			}
		}

				#endregion //LastRecordList

				#region RecordCollection

		internal RecordCollectionBase RecordCollection
		{
			get { return this._recordCollection; }
		}

				#endregion //RecordCollection

				#region RecordCollectionSparseArray

		internal MainRecordSparseArray RecordCollectionSparseArray
		{
			get { return this._recordCollection.SparseArray as MainRecordSparseArray; }
		}

				#endregion //RecordCollectionSparseArray

				#region RecordManager

		/// <summary>
		/// Gets the associated record manager.
		/// </summary>
		internal RecordManager RecordManager
		{
			get
			{
				return _recordCollection.ParentRecordManager;
			}
		}

				#endregion // RecordManager

				#region RecordsInsertedAtTop

		internal List<DataRecord> RecordsInsertedAtTop
		{
			get
			{
				if (this._recordsInsertedAtTop == null &&
					this._recordCollection is DataRecordCollection)
					this._recordsInsertedAtTop = new List<DataRecord>();

				return this._recordsInsertedAtTop;
			}
		}

				#endregion //RecordsInsertedAtTop

				#region ScrollCount



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal int ScrollCount
		{
			get
			{
				this.Verify( );

				// get the cached scroll count from the sparse array
				int scrollCount = this.RecordCollectionSparseArray.ScrollCount;

				// add in the count of any special records

				// JJD 2/14/11 - TFS66166 - Optimization
				// Instead of using the public properties (which will call Verify again)
				// use the members 
				//scrollCount += this.CountOfSpecialRecordsOnTop + this.CountOfSpecialRecordsOnBottom;
				if (this._specialRecordsOnTop != null)
					scrollCount += this._specialRecordsOnTop.Count;

				if (this._specialRecordsOnBottom != null)
					scrollCount += this._specialRecordsOnBottom.Count;

				// SSP 2/11/09 TFS12467
				// We should be raising ScrollCount change notification synchronously when 
				// it gets dirtied, not when it gets lazily calculated. Commented out the 
				// following code.
				// 
				
#region Infragistics Source Cleanup (Region)







































#endregion // Infragistics Source Cleanup (Region)


				return scrollCount;
			}
		}

				#endregion //ScrollCount

				#region ShouldRaiseCollectionNotifications

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Indicates whether this collection should currently raise collection change notifications.
		/// Notifications aren't necessary when nothing is hooked into this collection or when notifications
		/// are suspended.
		/// </summary>
		internal bool ShouldRaiseCollectionNotifications
		{
			get
			{
				if ( this.HasAnyListeners )
				{
					RecordManager rm = this.RecordManager;
					if ( null == rm || ! rm.IsUpdating || ! rm.BeginUpdateInfo.HasPendingResetNotification( this ) )
						return true;
				}

				return false;
			}
		}

				#endregion // ShouldRaiseCollectionNotifications

			#endregion //Internal Properties
    
		#endregion //Properties	
        
		#region Methods

			#region Public Methods

			#endregion //Public Methods	
        
			#region Internal Methods
    
                #region ClearCachedRecordsOnTop

        internal void ClearCachedRecordsOnTop()
		{
			if (this._recordsInsertedAtTop != null &&
				this._recordsInsertedAtTop.Count > 0)
			{
				this._recordsInsertedAtTop.Clear();
				this.RaiseChangeEvents(true);
			}
		}

				#endregion //ClearCachedRecordsOnTop	
    
                // JJD 6/23/09 - NA 2009 Vol 2 - Record fixing - added
                #region ClearFixedRecords

        internal void ClearFixedRecords()
		{
            bool hadFixedRcds = false;

            if (this._fixedNonSpecialRecordsOnTop != null)
            {
                hadFixedRcds = this._fixedNonSpecialRecordsOnTop.Count > 0;
                this._fixedNonSpecialRecordsOnTop = null;
            }

            if (this._fixedNonSpecialRecordsOnBottom != null)
            {
                hadFixedRcds = hadFixedRcds || this._fixedNonSpecialRecordsOnBottom.Count > 0;
                this._fixedNonSpecialRecordsOnBottom = null;
            }
            
            if ( hadFixedRcds )
                this.DirtySpecialRecords(false);
		}

				#endregion //ClearFixedRecords	
            
                // JJD 7/1/09 - NA 2009 Vol 2 - Record fixing
                #region CloneFixedRecords

        internal void CloneFixedRecords(ViewableRecordCollection source)
        {
			// JJD 1/12/12 - TFS23607
			// Access all of the records thru the sorted collection to make sure
			// we don't have any empty slots in the sparse arrays before
			// calling CloneFixedRecords below. Otherwise, the arrays can get out of sync
			foreach (Record rcd in _recordCollection.ParentRecordManager.Sorted) { }

			this.CloneFixedRecordsHelper(source, true);
            this.CloneFixedRecordsHelper(source, false);

            this.DirtySpecialRecords(false);
        }

        private void CloneFixedRecordsHelper(ViewableRecordCollection source, bool onTop)
        {
            List<Record> sourceList;

            if (onTop)
                sourceList = source._fixedNonSpecialRecordsOnTop;
            else
                sourceList = source._fixedNonSpecialRecordsOnBottom;

            if (sourceList == null ||
                 sourceList.Count == 0)
                return;

            RecordManager rm = this._recordCollection.ParentRecordManager;

            Debug.Assert(rm != null);

            if (rm == null)
                return;

            RecordSparseArrayBase unsortedArray = ((DataRecordCollection)(rm.Unsorted)).SparseArray;

            RecordManager rmSource = source._recordCollection.ParentRecordManager;

            Debug.Assert(rmSource != null);

            if (rmSource == null)
                return;

            RecordSparseArrayBase unsortedArraySource = ((DataRecordCollection)(rmSource.Unsorted)).SparseArray;

            List<Record> targetList = new List<Record>(sourceList.Count);

            foreach (Record sourceRcd in sourceList)
            {
                if (sourceRcd is DataRecord)
                {
                    int unsortedIndex = unsortedArraySource.IndexOf(sourceRcd);
                    if (unsortedIndex >= 0)
                    {
                        Record rcd = unsortedArray.GetItem(unsortedIndex, true);

						// JJD 1/12/12 - TFS23607
						// Copy over the _fixedLocation from the source rcd so the cloned rcd 
						// is consistent
						rcd._fixedLocation = sourceRcd._fixedLocation;

						rcd.SeparatorVisibility = sourceRcd.SeparatorVisibility;

                        targetList.Add(rcd);
                    }
                }
            }

            if (onTop)
                this._fixedNonSpecialRecordsOnTop = targetList;
            else
                this._fixedNonSpecialRecordsOnBottom = targetList;
        }

                #endregion //CloneFixedRecords	
    
                // JJD 8/27/09 - TFS21513
                // Added constant to let us know that the fixed record order needs to be verified
 				#region DirtyFixedRecordOrder

		internal void DirtyFixedRecordOrder()
		{
            _verifiedSpecialRecordsVersion = FixedRecordOrderIsDirty;
		}

				#endregion // DirtyFixedRecordOrder
    
				#region DirtySpecialRecords

		internal void DirtySpecialRecords( bool verify )
		{
            // JJD 8/27/09 - TFS21513
            // Added constant to let us know that the fixed record order needs to be verified
            // so make sure _verifiedSpecialRecordsVersion doesn't go less than -1
            if ( _verifiedSpecialRecordsVersion >= 0 )
    	        _verifiedSpecialRecordsVersion = -1;

			if ( verify )
				this.Verify( );
		}

				#endregion // DirtySpecialRecords

				#region EnsureCorrectSortPosition

		/// <summary>
		/// Moves the record into its correct sort position if it's not already.
		/// </summary>
		/// <param name="record">Record</param>
		internal void EnsureCorrectSortPosition( Record record )
		{
			RecordCollectionBase recordColl = this.RecordCollection;
			Debug.Assert( null != recordColl );
			if ( null == recordColl )
				return;

			MainRecordSparseArray sarr = this.RecordCollectionSparseArray;
			if ( null != sarr )
			{
				int oldIndex = sarr.IndexOf( record );

				// SSP 3/10/10 TFS25807
				
				
				
				
				
				
				//int newIndex = oldIndex >= 0 ? this.GetInsertLocation( sarr, record ) : -1;
                if (oldIndex < 0)
                    return;

				int newIndex = -1;
				if ( record is DataRecord )
					newIndex = this.GetDataRecordInsertLocation( sarr, (DataRecord)record );
				else if ( record is GroupByRecord )
					newIndex = this.GetGroupByRecordInsertLocation( (GroupByRecord)record, true );
				

				//Debug.Assert( newIndex >= 0 );
				if ( newIndex >= 0 && oldIndex != newIndex )
				{
					this.MoveRecord( record, newIndex, oldIndex );
				}
			}
		}

				#endregion // EnsureCorrectSortPosition

				#region EnsureFiltersEvaluated

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		internal bool EnsureFiltersEvaluated( )
		{
			RecordManager rm = this.RecordManager;

			// JJD 12/29/08
			// Return false if we are in the middle of verifying the filters.
			
			// Also don't apply filters if we are in the middle of processing Reset data 
			// source notification.
			// 
			
			// SSP 10/20/10 
			
			// If a reset notification is pending (yet to be processed) then don't evaluate filters
			// since they will be re-evaluated when we do process the pending reset. Furthermore
			// the issue this causes is that the below logic doesn't lazily synchronize the records
			// to the data source and therefore at this point the records are out of sync with the
			// data source.
			// 
			//if ( this._inEnsureFiltersEvaluated || null == rm || rm.IsInReset )
			if ( this._inEnsureFiltersEvaluated || null == rm || rm.IsInReset || rm.IsResetPending )
				return false;

			ResolvedRecordFilterCollection filters = null != rm ? rm.RecordFiltersResolved : null;
			Debug.Assert( null != filters );

			// If version numbers are the same then we've previously evaluated the filters and nothing
			// needs to be done now.
			// 
			if ( null == filters || _verifiedFiltersVersion == filters.Version )
				return true;

			_verifiedFiltersVersion = filters.Version;

			RecordCollectionBase recordColl = this.RecordCollection;

			// SSP 8/25/09 TFS18934
			// When there are group-by records and this method gets called on the 
			// RecordManager's Sorted collection, don't evaluate the filters here
			// and instead let the group-by records evaluate the filters. Otherwise
			// the filteredOutStateChangedCount will be 0 when the group-by records
			// processes this method and won't raise the notification when it needs 
			// to.
			// 
			// --------------------------------------------------------------------
			if ( recordColl.IsTopLevel && recordColl != rm.Current )
				return false;
			// --------------------------------------------------------------------

			// Make sure the record collection is synchronized with the data list and is sorted.
			// 
			recordColl.EnsureNotDirty( );

			_inEnsureFiltersEvaluated = true;

			// SSP 3/19/10 - Optimizations
			// Added a counter to keep track of number of records whose filtered out state changes.
			// 
			int filteredOutStateChangedCount = 0;

			// Call BeginUpdate which will suspend the collection from synchronously responding to
			// change notifications on individual records as they are filtered in below loop.
			// 
			recordColl.BeginUpdate( );
			try
			{
				// Only DataRecord's and GroupByRecord's need to be filtered.
				// 
				RecordType recordsType = recordColl.RecordsType;
				if ( RecordType.GroupByField == recordsType || RecordType.DataRecord == recordsType )
				{
					// Dirty the scroll count on the sparse array so it doesn't synchronously handle
					// visibility change notifications of multiple records as they are marked 
					// filtered out.
					// 
					MainRecordSparseArray array = this.RecordCollectionSparseArray;
					array.DirtyScrollCount( );

					// If there are active filters then allocate all records. Lazily allocating records
					// and evaluating filters will cause remove notifications to be raised from the
					// viewable record collection as records are being allocated and filtered out and 
					// the items control might not handle it very well if the notifications are being 
					// raised while it's accessing (for example, enumerating) the viewable record 
					// collection. 
					// We only have to do this for the top level since if there are multiple levels
					// then all records would have been allocated to begin with (like in group-by
					// situation).
					// 
					if ( _recordCollection.IsTopLevel && filters.HasActiveFiltersInAnyFieldLayout( ) )
						array.EnsureAllItemsCreated( );

					int filteredOutRecordCount = 0;
					int filterStateChangedCount = 0;

					// Evaluate filters on allocated data records only. Data records that haven't been
					// allocated in lazy-load mode will be assumed to be un-filtered from the point of
					// view of scroll-range calculation logic. They will be filtered when they are 
					// created (which happens as they are brought in view).
					// 
					foreach ( Record record in array.NonNullItems )
					{
						// SSP 4/10/09 TFS16485 TFS16490
						// It's not just the change in IsFilteredOut that we need to take into account,
						// but it's also the change in FilterAction setting that could cause the 
						// VisibilityResolved to be different even with the same value of IsFilteredOut,
						// that we need to take into account to determine if we need to raise Reset
						// notification.
						// 
						//bool oldIsFilteredOut = record.InternalIsFilteredOut_NoVerify;
						Record.FilterState oldFilterState = record._cachedFilterState;

						DataRecord dr = record as DataRecord;
						if ( null != dr )
						{
							dr.ApplyFiltersHelper( filters, false );
						}
						else
						{
							GroupByRecord gr = record as GroupByRecord;
							if ( null != gr )
								// SSP 5/15/09 TFS16576
								// Use the new ApplyFiltersHelper method instead.
								// 
								//gr.EnsureFiltersEvaluated( );
								gr.ApplyFiltersHelper( false );
						}

						// SSP 4/10/09 TFS16485 TFS16490
						// 
						
						Record.FilterState newFilterState = record._cachedFilterState;
						if ( 0 != ( Record.FilterState.FilteredOut & newFilterState ) )
							filteredOutRecordCount++;

						// SSP 3/19/10 - Optimizations
						// 
						//if ( oldFilterState != newFilterState )
						if ( ( Record.FILTER_IN_OUT_AND_ACTION_FLAGS & oldFilterState ) != ( Record.FILTER_IN_OUT_AND_ACTION_FLAGS & newFilterState ) )
							filterStateChangedCount++;

						// SSP 3/19/10 - Optimizations
						// Added a counter to keep track of number of records whose filtered out state changes.
						// 
						if ( ( Record.FilterState.FilteredOut & oldFilterState ) != ( Record.FilterState.FilteredOut & newFilterState ) )
							filteredOutStateChangedCount++;

						
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

						
					}

					// If all the child records of a group-by record are filtered out, set its
					// IsFilteredOut to true.
					// 
					GroupByRecord groupByRecord = recordColl.ParentRecord as GroupByRecord;
					
					
					
					if ( null != groupByRecord )
						groupByRecord.ApplyFiltersHelper( filteredOutRecordCount > 0 && filteredOutRecordCount == array.Count, false );
					
#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

					

					// Reset the version number again here in case it was changed because a 
					// RecordFilter synchronized its field name with field, which would have
					// caused filters.Version to get bumped. If this causes an issue, then
					// remove it and instead explicitly have field names synchronized before 
					// we set the version number above.
					// 
					_verifiedFiltersVersion = filters.Version;

					// Raise Reset notification on the collection so any listeners get notified
					// of potential change in the contents of the collection (as filtered out
					// items won't be part of this collection anymore).
					// 
                    if (filterStateChangedCount > 0)
                    {
                        // JJD 7/14/09 - TFS19373
                        // Make sure we dirty the special records so we can maintain the
                        // fixed record lists based on the new filter criteria
                        this.DirtySpecialRecords(false);

                        this.RaiseChangeEvents(true);
                    }

					// SSP 8/25/09 TFS18934
					// Since we are bailing out of this method for sorted collection,
					// raise the reset notification on the sorted collection when we
					// process this method for the groups collection.
					// 
					// ----------------------------------------------------------------
					if ( recordColl == rm.Groups )
					{
						DataRecordCollection sortedColl = rm.SortedInternal;
						MainRecordSparseArray sortedSparseArr = null != sortedColl ? sortedColl.SparseArray as MainRecordSparseArray : null;
						if ( null != sortedSparseArr )
						{
							sortedSparseArr.DirtyScrollCount( );
							sortedColl.RaiseChangeEvents( true );
						}
					}
					// ----------------------------------------------------------------
				}
			}
			finally
			{
				// SSP 8/25/09 TFS18934
				// Moved the setting of _inEnsureFiltersEvaluated from below after the call to 
				// EndUpdate and DirtySummaryResults. The EndUpdate call below can cause records
				// to be marked as needing to re-evaluate filters and subsequently to re-evaluate
				// filters on them however having the _inEnsureFiltersEvaluated flag set to true
				// will cause the records to bail out from their filter evaluation call. 
				// Therefore we should reset the _inEnsureFiltersEvaluated flag before calling
				// EndUpdate below otherwise records will be left marked as needing to 
				// re-evaluate filters with the possibility that nothing may trigger the 
				// re-evaluation on those records.
				// 
				_inEnsureFiltersEvaluated = false;

				// Resume update.
				// 
				recordColl.EndUpdate( false );

				FieldLayout fieldLayout = this.FieldLayout;
				// SSP 3/19/10 - Optimizations
				// Only dirty the summaries if the filter state of any record changed.
				// 
				//if ( null != fieldLayout && CalculationScope.FilteredSortedList == fieldLayout.CalculationScopeResolved )
				if ( filteredOutStateChangedCount > 0 && null != fieldLayout
					&& CalculationScope.FilteredSortedList == fieldLayout.CalculationScopeResolved )
				{
					RecordManager.DirtySummaryResults( null, null, null, rm );

					// SSP 4/23/12 TFS107881
					// 
					GridUtilities.NotifyCalcAdapter( GridUtilities.GetDataPresenter( _recordCollection ), this, "Reset", null );
				}
			}

			return true;
		}

				#endregion // EnsureFiltersEvaluated

				#region GetAssociatedFilterRecord

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// Added GetAssociatedFilterRecord method.
		// 



		// Lazily maintain a map between print records and their associated summary records
		// in the source ui datapresenter
		internal FilterRecord GetAssociatedFilterRecord( FilterRecord filterRecord )
		{
			
			return null;
		}


				#endregion // GetAssociatedFilterRecord

                // JJD 9/23/08 - added support for printing
                #region GetAssociatedSummaryRecord


        // Lazily maintain a map between print records and their associated summary records
        // in the source ui datapresenter
        internal SummaryRecord GetAssociatedSummaryRecord(SummaryRecord record)
        {
            // MBS 7/28/09 - NA9.2 Excel Exporting
            //DataPresenterReportControl dprc = this.RecordCollection.DataPresenter as DataPresenterReportControl;
            DataPresenterExportControlBase dprc = this.RecordCollection.DataPresenter as DataPresenterExportControlBase;

            Debug.Assert(dprc != null);

            if (dprc == null)
                return null;

            // if the report version has changed clear the cached map
            if (dprc.ReportVersion != this._reportVersion)
                this._associatedRecordMap = null;

            if (this._associatedRecordMap == null)
            {
                ViewableRecordCollection associatedVieableRecordCollection = this.AssociatedViewableRecordCollection;

                Debug.Assert(associatedVieableRecordCollection != null);

                if (associatedVieableRecordCollection == null)
                    return null;

                int allocation = 0;

                if (this._specialRecordsOnBottom != null)
                    allocation += this._specialRecordsOnBottom.Count;

                if (this._specialRecordsOnTop != null)
                    allocation += this._specialRecordsOnTop.Count;

                // allocate a new map
                this._associatedRecordMap = new Dictionary<SummaryDisplayAreaContext, SummaryRecord>(Math.Max(10, allocation * 2));

                // add summary records on bottom and top to map
                this.AddSummaryRecordsToMapHelper(this._specialRecordsOnBottom);
                this.AddSummaryRecordsToMapHelper(this._specialRecordsOnTop);
            }

            SummaryRecord associatedSummaryRecord;

            // Get the asscoiated record from the map
            bool rcdFoundInMap = this._associatedRecordMap.TryGetValue(record.SummaryDisplayAreaContext, out associatedSummaryRecord);

            Debug.Assert(rcdFoundInMap == true || this._associatedRecordMap.Count == 0);

            return associatedSummaryRecord;
        }

        private void AddSummaryRecordsToMapHelper(List<Record> list)
        {
            if (list != null)
            {
                int count = list.Count;

                for (int i = 0; i < count; i++)
                {
                    SummaryRecord sr = list[i] as SummaryRecord;

                    if (sr != null)
                        this._associatedRecordMap[sr.SummaryDisplayAreaContext] = sr;
                }
            }
         }

                #endregion //GetAssociatedSummaryRecord

				#region CreateFlatScrollRecordsCollection

		#region DEBUG

		/// <summary>
		/// Create a new instance of FlatScrollRecordsCollection and returns it.
		/// </summary>
		/// <param name="vrc"></param>
		/// <returns></returns>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public static IList<Record> CreateFlatScrollRecordsCollection( ViewableRecordCollection vrc )
		{
			return new FlatScrollRecordsCollection( vrc.RecordManager );
		}

		#endregion // DEBUG

				#endregion // CreateFlatScrollRecordsCollection

		#region GetDataRecordInsertLocation

		private int GetDataRecordInsertLocation( RecordSparseArrayBase sarr, DataRecord record )
		{
            // JJD 6/23/09
            // Since with the Record fixing functionality we may call this when we
            // are unsorted we need to pass in false only if we are sorted. Passing
            // in 'true' sorts by the original order in the underlying list
            //IComparer comparer = new RecordManager.RecordsSortComparer(false);
            RecordManager rm = this.RecordManager;
            IComparer comparer = new RecordManager.RecordsSortComparer(rm == null || !rm.IsSorted);

			int i = 0;
			int j = sarr.Count - 1;

			int currentIndex = sarr.IndexOf( record );

			while ( i <= j )
			{
				int m = ( i + j ) / 2;
				Record rr = sarr.GetItem( m, true );

				// Skip the record itself since it should not be assumed to be at correct
				// location in the sort order.
				// 
				if ( record == rr )
				{
					if ( i != j )
						rr = sarr.GetItem( ++m, true );
					else
						break;
				}

				int c = comparer.Compare( record, rr );

				if ( c < 0 )
					j = m - 1;
				else if ( c > 0 )
					i = m + 1;
				// If the record is where it should be then don't move it. That's what the
				// the below else if does.
				// 
				else if ( currentIndex >= 0 && currentIndex < m )
					j = m - 1;
				else
					i = m + 1;
			}

			return currentIndex >= 0 && currentIndex < i ? i - 1 : i;
		}

				#endregion // GetDataRecordInsertLocation

				#region GetItem

		internal object GetItem(int index)
		{
			if ( index < 0 )
				throw new ArgumentOutOfRangeException();
			
			int prefixRecordCount = 0;

			// get the count of any special records on top
			if (this._specialRecordsOnTop != null)
				prefixRecordCount = this._specialRecordsOnTop.Count;

			// check if index in range of the top special records
			if (index < prefixRecordCount)
				return this._specialRecordsOnTop[index];

			// adjust the index for the top special records
			index -= prefixRecordCount;

			// get the count of the visible records from the sparse array
			int normalRecordCount = this.RecordCollectionSparseArray.VisibleCount;

			bool throwExceptionIsNotFound = true;

			// check if the index is in range of the normal data records
			if (index < normalRecordCount)
			{
				Record record = this.RecordCollectionSparseArray.GetItemAtVisibleIndex(index) as Record;

				// JJD 4/17/07
				// The record can be null if the above call caused lazy creates and the Visibility
				// of the records that were created was set to 'Collapsed' in the 'InitializeRecord' event
				if (record != null)
					return record;

				// JJD 4/17/07
				// Set a flag so we don't throw an exception if we can't find an appropriate record below
				normalRecordCount = this.RecordCollectionSparseArray.VisibleCount;

				throwExceptionIsNotFound = false;
			}

			// adjust the index for the normal data records
			index -= normalRecordCount;

            // JJD 9/18/09 - TFS20567
            // Make sure the index is still valid before we try accessing bottom records
            if (index >= 0)
            {
                int suffixRecordCount = 0;

                // get the count of any special records on bottom
                if (this._specialRecordsOnBottom != null)
                    suffixRecordCount = this._specialRecordsOnBottom.Count;

                // check if index in range of the bottom special records
                if (index < suffixRecordCount)
                    return this._specialRecordsOnBottom[index];
            }

			// JJD 4/17/07
			// Don't throw the visible count
			if ( throwExceptionIsNotFound )
				throw new ArgumentOutOfRangeException();

			return null;

		}

				#endregion //GetItem

				#region GetRecordAtScrollPosition

		internal Record GetRecordAtScrollPosition(int index)
		{
			Debug.Assert(index >= 0);

			if (index < 0)
				return null;

			// MD 8/6/10 - TFS36611
			// If the style is BeforeParentHeadersAttached and the top record is expanded, its children will actually
			// display before the special records, since the special record go below the header, which is also below
			// children, so we need to look into the nested children first.
			// --------------------------------------------------
			MainRecordSparseArray sparseArr = this.RecordCollectionSparseArray;
			int scrollCount = sparseArr.ScrollCount;

			int firstRecordChildrenScrollCount = 0;
			
			// JJD 2/14/11 - TFS66166 - Optimization
			// Just call Verify once then use the appropriate members below instead of the corresponding
			// properties which would call Verify again
			this.Verify();

			// MD 12/3/10 - TFS36634
			// When there are records fixed on top, we should never show the first record's children above the headers, 
			// because there could be fixed data records above the expanded record, and the children should show below them.
			//if (scrollCount > 0)

			// JJD 2/14/11 - TFS66166 - Optimization
			// Since we already called Verify above use corresponding members instead
			//if (scrollCount > 0 && this.CountOfFixedRecordsOnTop == 0)
			if (scrollCount > 0 && (_fixedRecordsOnTop == null || _fixedRecordsOnTop.Count == 0))
			{
				Record firstRecord = sparseArr.GetItemAtVisibleIndex(0) as Record;

				if (firstRecord != null &&
					firstRecord.IsExpanded &&
					firstRecord.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.BeforeParentHeadersAttached)
				{
					firstRecordChildrenScrollCount = firstRecord.ViewableChildRecordScrollCount;
					if (index < firstRecordChildrenScrollCount)
						return firstRecord.ViewableChildRecords.GetRecordAtScrollPosition(index);
					else
						index -= firstRecordChildrenScrollCount;
				}
			}
			// ---------- End of TFS36611 Fix --------------------

			// get the count of any top special records
			// JJD 2/14/11 - TFS66166 - Optimization
			// Since we already called Verify above use corresponding members instead
			//int specialRrecordCount = this.CountOfSpecialRecordsOnTop;
			int specialRrecordCount = this._specialRecordsOnTop != null ? _specialRecordsOnTop.Count : 0;

			// see if the index is in the range of any top special records
			if ( specialRrecordCount > 0 )
			{
				if ( index < specialRrecordCount )
					return this._specialRecordsOnTop[index];

				// adjust the index to account for the top special records
				index -= specialRrecordCount;
			}

			// MD 8/6/10 - TFS36611
			// Since we may have already gone through the children of the first record above,
			// Calculate the remaining scroll count to see if the index is still in range of it. If so, 
			// index back into the sparse array, but add back in the count of nested children from the
			// first record because as far as the SparseArray is concerned, we didn't already walk over 
			// them.
			// --------------------------------------------------
			// get the cached scroll count from the sparse array
			//int scrollCount = this.RecordCollectionSparseArray.ScrollCount;
			//
			//// if the index is in the sparse array range then get the appropriate
			//// record from the sparse array
			//if (index < scrollCount )
			//    return this.RecordCollectionSparseArray.GetItemAtScrollIndex(index);
			//
			//// adjust the index to account for the sparse array items 
			//index -= scrollCount;
			int remainingScrollCount = scrollCount - firstRecordChildrenScrollCount;

			if (index < remainingScrollCount)
				return sparseArr.GetItemAtScrollIndex(index + firstRecordChildrenScrollCount);

			// adjust the index to account for the remaining sparse array items 
			index -= remainingScrollCount;
			// ---------- End of TFS36611 Fix --------------------
		
			// get the count of any bottom special records
			// JJD 2/14/11 - TFS66166 - Optimization
			// Since we already called Verify above use corresponding members instead
			//specialRrecordCount = this.CountOfSpecialRecordsOnBottom;
			specialRrecordCount = this._specialRecordsOnBottom != null ? _specialRecordsOnBottom.Count : 0;

			if ( index < specialRrecordCount )
				return this._specialRecordsOnBottom[index];

			return null;
		}

				#endregion //GetRecordAtScrollPosition

				#region GetRecordContainingScrollIndex

		// SSP 7/30/09 - NAS9.2 Enhanced grid view
		// Added GetItemContainingScrollIndex method.
		// 
		/// <summary>
		/// Gets the record from this collection that contains the specified scroll index. This method
		/// modifies the scrollIndex to be relative to the returned record.
		/// </summary>
		/// <param name="scrollIndex"></param>
		/// <returns></returns>
		internal Record GetRecordContainingScrollIndex( ref int scrollIndex )
		{
			// MD 8/6/10 - TFS36611
			// If the style is BeforeParentHeadersAttached and the top record is expanded, its children will actually
			// display before the special records, since the special record go below the header, which is also below
			// children, so we need to look into the nested children first.
			// --------------------------------------------------
			MainRecordSparseArray sparseArr = this.RecordCollectionSparseArray;
			int sparseArrCount = sparseArr.ScrollCount;

			int firstRecordScrollCount = 0;

			// MD 12/3/10 - TFS36634
			// When there are records fixed on top, we should never show the first record's children above the headers, 
			// because there could be fixed data records above the expanded record, and the children should show below them.
			//if (scrollCount > 0)
			if (sparseArrCount > 0 && this.CountOfFixedRecordsOnTop == 0)
			{
				Record firstRecord = sparseArr.GetItemAtVisibleIndex(0) as Record;

				if (firstRecord != null &&
					firstRecord.IsExpanded &&
					firstRecord.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.BeforeParentHeadersAttached)
				{
					firstRecordScrollCount = firstRecord.ViewableChildRecordScrollCount;
					if (scrollIndex < firstRecordScrollCount)
						return firstRecord;
					else
						scrollIndex -= firstRecordScrollCount;
				}
			}
			// ---------- End of TFS36611 Fix --------------------

			if ( null != _specialRecordsOnTop )
			{
				int count = _specialRecordsOnTop.Count;
				if ( scrollIndex < count )
				{
					Record record = _specialRecordsOnTop[scrollIndex];
					scrollIndex = 0;
					return record;
				}

				scrollIndex -= count;
			}

			// MD 8/6/10 - TFS36611
			// Since we may have already gone through the children of the first record above,
			// calculate the remaining scroll count to see if the index is still in range of it. If so, 
			// index back into the sparse array, but add back in the count of nested children from the
			// first record because as far as the SparseArray is concerned, we didn't already walk over 
			// them.
			//MainRecordSparseArray sparseArr = this.RecordCollectionSparseArray;
			//int sparseArrCount = sparseArr.ScrollCount;
			//if ( scrollIndex < sparseArrCount )
			//    return sparseArr.GetItemContainingScrollIndex( ref scrollIndex );
			//else
			//    scrollIndex -= sparseArrCount;
			int remainingScrollCount = sparseArrCount - firstRecordScrollCount;
			if (scrollIndex < remainingScrollCount)
			{
				scrollIndex += firstRecordScrollCount;
				return sparseArr.GetItemContainingScrollIndex(ref scrollIndex);
			}
			
			scrollIndex -= remainingScrollCount;

			if ( null != _specialRecordsOnBottom )
			{
				int count = _specialRecordsOnBottom.Count;
				if ( scrollIndex < count )
				{
					Record record = _specialRecordsOnBottom[scrollIndex];
					scrollIndex = 0;
					return record;
				}
			}

			return null;
		}

				#endregion // GetRecordContainingScrollIndex
		
				#region GetScopedScrollIndexOfRecord

        // JJD 05/06/10 - TFS27757 
        // Added ignoreHiddenItemState param
        //internal int GetScopedScrollIndexOfRecord(Record record)
		internal int GetScopedScrollIndexOfRecord(Record record, bool ignoreHiddenItemState)
		{
			// MD 8/6/10 - TFS36611
			// Moved all code to the new overload.
			return this.GetScopedScrollIndexOfRecord(record, ignoreHiddenItemState, false);
		}

		// MD 8/6/10 - TFS36611
		// Added a new overload to take the getMinOfRecordAndFirstChild parameter.
		internal int GetScopedScrollIndexOfRecord(Record record, bool ignoreHiddenItemState, bool getMinOfRecordAndFirstChild)
		{
			//Debug.Assert(record.ParentCollection == this._recordCollection);

			// MD 8/6/10 - TFS36611
			// If the style is BeforeParentHeadersAttached and the top record is expanded, its children will actually
			// display before the special records, since the special record go below the header, which is also below
			// children, so we need to get the scroll count for the nested children and add it to the scroll index for
			// the special records.
			// --------------------------------------------------
			MainRecordSparseArray sparseArr = this.RecordCollectionSparseArray;
			int sparseArrCount = sparseArr.ScrollCount;

			int firstRecordsChildrenBeforeCount = 0;
			Record firstRecord = null;
			// MD 12/3/10 - TFS36634
			// When there are records fixed on top, we should never show the first record's children above the headers, 
			// because there could be fixed data records above the expanded record, and the children should show below them.
			//if (sparseArrCount > 0)
			if (sparseArrCount > 0 && this.CountOfFixedRecordsOnTop == 0)
			{
				firstRecord = sparseArr.GetItemAtVisibleIndex(0) as Record;

				// JM 10-19-10 TFS 57336 - Check for firstRecord == null
				if (null != firstRecord)
				{
					if (firstRecord.IsExpanded &&
						firstRecord.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.BeforeParentHeadersAttached &&
						firstRecord.ViewableChildRecords != null)
					{
						firstRecordsChildrenBeforeCount = firstRecord.ViewableChildRecordScrollCount;
					}
				}
			}
			// ---------- End of TFS36611 Fix --------------------

			if (record.IsSpecialRecord)
			{
				if ( this._specialRecordsOnTop != null &&
					 this._specialRecordsOnTop.Contains(record) )
					// MD 8/6/10 - TFS36611
					// Add in the nested children count of the first record if they show above the special records.
					//return this._specialRecordsOnTop.IndexOf(record);
					return firstRecordsChildrenBeforeCount + this._specialRecordsOnTop.IndexOf(record);

				if ( this._specialRecordsOnBottom != null &&
					 this._specialRecordsOnBottom.Contains(record) )
					return this._specialRecordsOnBottom.IndexOf(record) + 
							this.CountOfSpecialRecordsOnTop +
							this.RecordCollectionSparseArray.ScrollCount;

				//Debug.Fail("Special records shoulb be in the to or bottom cached list");
				return -1;
			}

            // JJD 05/06/10 - TFS27757 
            // Pass along ignoreHiddenItemState param
            //int arrayScrollIndex = this.RecordCollectionSparseArray.ScrollIndexOf(record);
			int arrayScrollIndex = this.RecordCollectionSparseArray.ScrollIndexOf(record, ignoreHiddenItemState);

			// MD 8/6/10 - TFS36611
			// The SparseArray.ScrollIndexOf stops when it finds the record it is looking for. It has no notion that 
			// records might display child above. So the returned arrayScrollIndex will always reflect the min of the 
			// record's scroll index and the scroll index of its first child. So if children expand upwards, we may 
			// want to add in the scroll count of the children since the record will appear directly after them.
			if(record.AreChildrenAfterParent == false &&
				record.IsExpanded &&
				// JJD 09/22/11  - TFS84708 - Optimization
				// Use ViewableChildRecordsIfNeeded instead
				//record.ViewableChildRecords != null)
				record.ViewableChildRecordsIfNeeded != null)
			{
				if (getMinOfRecordAndFirstChild)
				{
					// If all we really need is the min of the record scroll index and it's first child's scroll index, 
					// we don't have to do anything nuless the parent is the first record and headers are attached. In 
					// that case, the first child has the min scroll index and it is above the special records, so it's 
					// scroll index is exactly the scroll index that the sparse array thinks the parent record has.
					if (record == firstRecord &&
						firstRecord.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.BeforeParentHeadersAttached)
					{
						return arrayScrollIndex;
					}
				}
				else
				{
					// Otherwise, if we need to get the actual scroll index of the record, add in the scroll count of 
					// the children.
					// JJD 09/22/11  - TFS84708 - Optimization
					// Use ViewableChildRecordsIfNeeded instead
					//arrayScrollIndex += record.ViewableChildRecords.ScrollCount;
					ViewableRecordCollection vcr = record.ViewableChildRecordsIfNeeded;

					if ( vcr != null )
						arrayScrollIndex += vcr.ScrollCount;
				}
			}

			// AS 3/16/07
			// ViewableRecords is not limited to datarecords
			//Debug.Assert(arrayScrollIndex >= 0 || ((DataRecord)record).IsDeleted);
			//Debug.Assert(arrayScrollIndex >= 0 || (record is DataRecord && ((DataRecord)record).IsDeleted));

			return arrayScrollIndex + this.CountOfSpecialRecordsOnTop;

		}
		
				#endregion //GetScopedScrollIndexOfRecord

				// AS 5/28/09 NA 2009.2 Undo/Redo
				#region GetSpecialRecord
		internal Record GetSpecialRecord(RecordType recordType)
		{
			this.Verify();

			if (null != _specialRecordsOnTop)
			{
				foreach (Record rcd in _specialRecordsOnTop)
				{
					if (rcd.RecordType == recordType)
						return rcd;
				}
			}

			if (null != _specialRecordsOnBottom)
			{
				foreach (Record rcd in _specialRecordsOnBottom)
				{
					if (rcd.RecordType == recordType)
						return rcd;
				}
			}

			return null;
		}
				#endregion //GetSpecialRecord

				// AS 6/25/09 NA 2009.2 Field Sizing
				#region GetSpecialRecords
		/// <summary>
		/// Helper method to return all records in the associated record collection as well as any special records on top/bottom.
		/// </summary>
		/// <returns></returns>
		internal IEnumerator<Record> GetSpecialRecords(bool onTop)
		{
			this.VerifySpecialRecords();

			List<Record> specialRecords = onTop ? _specialRecordsOnTop : _specialRecordsOnBottom;

			return null != specialRecords ? specialRecords.GetEnumerator() : (IEnumerator<Record>)null;
		} 
				#endregion //GetSpecialRecords

				#region GetSortedIndexFromUnsortedIndex

		internal int GetSortedIndexFromUnsortedIndex( int unsortedIndex )
		{
			if ( unsortedIndex < 0 )
				return unsortedIndex;

			Debug.Assert( this._recordCollection is DataRecordCollection );

			RecordManager rm = this._recordCollection.ParentRecordManager;

			Debug.Assert( rm != null );

			if ( rm == null )
				return unsortedIndex;

			RecordSparseArrayBase unsortedArray = ( (DataRecordCollection)( rm.Unsorted ) ).SparseArray;

			if ( rm.IsSorted )
			{
				// JJD 12/02/08 - TFS6743/BR35763
				// If the rm is inside VerifySort then
				// we clear the _recordsInsertedAtTop and return the unsorted index
				if ( rm.IsInReset )
				{
					if ( this._recordsInsertedAtTop != null && rm.IsInVerifySort )
						this._recordsInsertedAtTop.Clear( );

					return unsortedIndex;
				}

				// JJD 12/04/08 - TFS6743/BR35763
				// Make sure the index is less than the count. This can happen sometimes
				// during an add to the underlying data source
				//Record rcd = unsortedArray.GetItem(unsortedIndex, false);
				Record rcd = null;
				if ( unsortedIndex < unsortedArray.Count )
					rcd = unsortedArray.GetItem( unsortedIndex, false );

				if ( rcd != null )
				{
					// JJD 12/04/08 - TFS6743/BR35763
					// Make sure the index is valid
					//return rm.Sorted.SparseArray.IndexOf(rcd);
					int sortedIndex = rm.Sorted.SparseArray.IndexOf( rcd );

					if ( sortedIndex >= 0 )
						return sortedIndex;
				}
			}

			// JJD 6/23/09 - NA 2009 Vol 2 - Record fixing
			int topFixedRcds = 0;
			int topFixedRcdsWithHigherUnsortedIndices = 0;
			int bottomFixedRcds = 0;
			int bottomFixedRcdsWithHigherUnsortedIndices = 0;
			int rcdsInsertedAtTop = 0;
			int rcdsInsertedAtTopWithHigherUnsortedIndices = 0;

			// JJD 6/23/09 - NA 2009 Vol 2 - Record fixing
			// Check the fixed rcds on top to see if any rcd's 
			// unsorted index matches
			if ( this._fixedNonSpecialRecordsOnTop != null )
			{
				topFixedRcds = this._fixedNonSpecialRecordsOnTop.Count;

				int indexInList = this.GetListIndexFromUnsortedIndex( this._fixedNonSpecialRecordsOnTop, unsortedIndex, unsortedArray, ref topFixedRcdsWithHigherUnsortedIndices );

				if ( indexInList >= 0 )
					return indexInList;
			}

			// JJD 6/23/09 - NA 2009 Vol 2 - Record fixing
			// Check the fixed rcds on top to see if any rcd's 
			// unsorted index matches
			if ( this._fixedNonSpecialRecordsOnBottom != null )
			{
				bottomFixedRcds = this._fixedNonSpecialRecordsOnBottom.Count;

				int indexInList = this.GetListIndexFromUnsortedIndex( this._fixedNonSpecialRecordsOnBottom, unsortedIndex, unsortedArray, ref bottomFixedRcdsWithHigherUnsortedIndices );

				if ( indexInList >= 0 )
					return unsortedArray.Count + indexInList - bottomFixedRcds;
			}

			// JJD 6/23/09 - NA 2009 Vol 2 - Record fixing
			// Check the fixed rcds on top to see if any rcd's 
			// unsorted index matches
			if ( this._recordsInsertedAtTop != null )
			{
				rcdsInsertedAtTop = this._recordsInsertedAtTop.Count;

				int indexInList = this.GetListIndexFromUnsortedIndex( this._recordsInsertedAtTop, unsortedIndex, unsortedArray, ref rcdsInsertedAtTopWithHigherUnsortedIndices );

				if ( indexInList >= 0 )
					return indexInList + topFixedRcds;
			}

			// JJD 6/23/09 - NA 2009 Vol 2 - Record fixing
			// Adjust for the fixed rcds on top or bottom that have a lower index
			// as well as rcds that were inserted at top
			return unsortedIndex
				+ topFixedRcdsWithHigherUnsortedIndices
				+ rcdsInsertedAtTopWithHigherUnsortedIndices
				- ( bottomFixedRcds - bottomFixedRcdsWithHigherUnsortedIndices );

			
#region Infragistics Source Cleanup (Region)




















































#endregion // Infragistics Source Cleanup (Region)

		}

				#endregion //GetSortedIndexFromUnsortedIndex	
    
				#region GetUnsortedIndexFromSortedIndex

		internal int GetUnsortedIndexFromSortedIndex( int sortedIndex )
		{
			Debug.Assert( this._recordCollection is DataRecordCollection );

			RecordManager rm = this._recordCollection.ParentRecordManager;

			Debug.Assert( rm != null );

			if ( rm == null )
				return sortedIndex;

			RecordSparseArrayBase unsortedArray = ( (DataRecordCollection)( rm.Unsorted ) ).SparseArray;
			RecordSparseArrayBase sortedArray = rm.Sorted.SparseArray;

			if ( rm.IsSorted )
			{
				// JJD 12/02/08 - TFS6743/BR35763
				// If the rm is inside Reset then
				// we clear the _recordsInsertedAtTop and return the sorted index
				if ( rm.IsInReset )
				{
					if ( this._recordsInsertedAtTop != null && rm.IsInVerifySort )
						this._recordsInsertedAtTop.Clear( );

					return sortedIndex;
				}

				Record rcd = sortedArray.GetItem( sortedIndex, false );

				if ( rcd != null )
					return unsortedArray.IndexOf( rcd );
			}

			// SSP 6/24/09 - NAS9.2
			// Removed the original logic and added the new logic below. We need to take into account
			// the fact that in order to calculate the unsorted index that the specified sorted index
			// maps to, we need to take into account the fixed records and top and bottom whose 
			// unsorted indexes are greater or less than the target unsorted index. However as we take
			// into account the fixed records, the target unsorted index changes, invalidating the
			// fixed records that we've taken into account so far. Therefore we need to take into 
			// acount the fixed records in the order from smaller index to larger index in order to
			// make sure the change in the target unsorted index as we calculate doesn't invalidate
			// the fixed records that we've taken into account so far.
			// 
			// --------------------------------------------------------------------------------------
			List<Record> top1 = _fixedNonSpecialRecordsOnTop;
			List<DataRecord> top2 = _recordsInsertedAtTop;
			List<Record> bottom = _fixedNonSpecialRecordsOnBottom;

			int outOfPlaceRecordsOnTop = ( null != top1 ? top1.Count : 0 ) + ( null != top2 ? top2.Count : 0 );
			int outOfPlaceRecordsOnBottom = null != bottom ? bottom.Count : 0;

            // If we don't have any out of place records then return the sorted index
            if (outOfPlaceRecordsOnTop + outOfPlaceRecordsOnBottom == 0)
                return sortedIndex;

			List<int> outOfPlaceRecords = new List<int>( outOfPlaceRecordsOnTop + outOfPlaceRecordsOnBottom );

			Record recordAtSortedIndex = sortedArray.GetItem( sortedIndex, false );

			// AS 8/5/09 NA 2009.2
			// Changed AggregateEnumerable to a generic.
			//
			//foreach ( Record record in new GridUtilities.AggregateEnumerable( top1, top2, bottom ) )
			foreach ( Record record in new GridUtilities.AggregateEnumerable<Record>( top1, new TypedEnumerable<Record>(top2), bottom ) )
			{
				int recordUnsortedIndex = unsortedArray.IndexOf( record );
				Debug.Assert( recordUnsortedIndex >= 0 );
				if ( recordUnsortedIndex >= 0 )
					outOfPlaceRecords.Add( recordUnsortedIndex );

				if ( recordAtSortedIndex == record )
					return recordUnsortedIndex;
			}

			outOfPlaceRecords.Sort( );

			int resultIndex = sortedIndex - outOfPlaceRecordsOnTop;

			for ( int c = 0, count = outOfPlaceRecords.Count; c < count; c++ )
			{
				if ( outOfPlaceRecords[c] <= resultIndex )
					resultIndex++;
			}

			return resultIndex;
			// --------------------------------------------------------------------------------------

			
#region Infragistics Source Cleanup (Region)


















































#endregion // Infragistics Source Cleanup (Region)

		}

				#endregion //GetUnsortedIndexFromSortedIndex	

				#region GetRecordInThisCollectionFromDescendant

		internal Record GetRecordInThisCollectionFromDescendant(Record descendantRecord)
		{
			if (descendantRecord == null)
				return null;

			if (descendantRecord.ParentCollection == this._recordCollection)
			{
				if (this.Contains(descendantRecord))
					return descendantRecord;

				return null;
			}

			// walk up the parent chain
			Record parentRecord = descendantRecord.ParentRecord;

			if (parentRecord != null)
				return this.GetRecordInThisCollectionFromDescendant(parentRecord);

			return null;
		}

				#endregion //GetRecordInThisCollectionFromDescendant	
 
                // JJD 2/25/08 - BR30660 - added
                #region InsertDataRecordInGroupBySlot

        internal void InsertDataRecordInGroupBySlot(DataRecord dr)
        {
            FieldLayout fl = dr.FieldLayout;

            bool isRootGroupByCollection = this._recordCollection.ParentRecordManager.Groups == this.RecordCollection;
           
            GroupByRecord gbrToUse = null;
            bool gbrToUseIsNew = false;
            int insertAtIndex = -1;

			// SSP 8/19/09 TFS21078
			// 
			bool dontInsertRecord = false;

            // JJD 7/28/08 - BR33636
            // We need to process this even if the count is 0
            //            if (this._recordCollection.Count > 0)
            {
                // JJD 7/28/08 - BR33636
                // Check the count before trying to acces te indexer
                //GroupByRecord existingGroupByRecord = this._recordCollection[0] as GroupByRecord;
                //Debug.Assert(existingGroupByRecord != null, "Only GroupByRecords can be in this collection.");
                GroupByRecord existingGroupByRecord = this._recordCollection.Count > 0 ? this._recordCollection[0] as GroupByRecord : null;
 
                // JJD 7/28/08 - BR33636
                // We need to process this even if we don't have an existing rcd in the collecection
                //if (existingGroupByRecord != null)
                {
                    // JJD 7/28/08 - BR33636
                    // Check for non-null existingGroupByRecord
                    //if (isRootGroupByCollection)
                    if (isRootGroupByCollection && existingGroupByRecord != null)
                    {
                        #region Process Root Groupby Collection

                        // See if we have a layer of special GroupByFieldLayout records
                        if (existingGroupByRecord.RecordType == RecordType.GroupByFieldLayout)
                        {
                            #region Process existing special GroupByFieldLayout records

                            // Loop over the existing groupbyrecords and match on FieldLayout
                            foreach (GroupByRecord gbr in this._recordCollection)
                            {
                                if (gbr.FieldLayout == fl)
                                {
                                    gbrToUse = gbr;
                                    break;
                                }
                            }

                            // if we didn't find a match then create a new one here
                            if (gbrToUse == null)
                            {
                                gbrToUse = new GroupByRecord(fl, this._recordCollection);
                                gbrToUseIsNew = true;
                                insertAtIndex = this.GetGroupByRecordInsertLocation(gbrToUse, false);

                                Debug.Assert(insertAtIndex >= 0, "Index should never be negative here");

                                if (insertAtIndex < 0)
                                    insertAtIndex = 0;
                            }

                            #endregion //Process exiting special GroupByFieldLayout records
                        }
                        else
                        {
                            // See if the FieldLayout matchs the existing groupbyrecord's FieldLayout

                            if (existingGroupByRecord.FieldLayout != fl)
                            {
                                #region New FieldLayout and we need to create Layer of special FL GroupByRecords

                                // Create 2 new GroupBy rcds, one to hold the existing groupbyrcds and
                                // the other to hold the records from the new fieldLayout
                                GroupByRecord gbrExistingFieldLayout = new GroupByRecord(existingGroupByRecord.FieldLayout, this._recordCollection);
                                gbrToUse = new GroupByRecord(fl, this._recordCollection);
                                gbrToUseIsNew = true;

                                // Since the special FieldLayout GroupBy rcds are ordered based
                                // on their index in the FieldLayouts collection determine where to
                                // insert the new guy by comparing the FieldLayout indices.
                                if (fl.Index < gbrExistingFieldLayout.Index)
                                    insertAtIndex = 0;
                                else
                                    insertAtIndex = 1;

                                bool oldBypassValue = this._bypassRecordCollectionNotifications;
                                this._bypassRecordCollectionNotifications = true;

                                try
                                {
                                    // Move the existing rcds into gbrExistingFieldLayout created above 
                                    RecordCollection newParentCollection = gbrExistingFieldLayout.ChildRecords;

                                    // JJD 10/03/08
                                    // We can't add the groupbyrecords to the newParent's colection
                                    // before we call Clear on the old one because the sparsearray maintains
                                    // it's own node objects that are held as opaque data by the records
                                    // and calling SparseArray.Clear() below will just null those objects out.
                                    // Therefore we just need to copy them into a temporary list,
                                    // then call Clear and then insert them into the new parent's collection
                                    // from the templist
                                    //foreach (GroupByRecord gbr in this._recordCollection)
                                    //{
                                    //    gbr.InitializeParentCollection(newParentCollection);
                                    //    newParentCollection.SparseArray.Add(gbr);
                                    //}
                                    List<Record> tempList = new List<Record>(this._recordCollection);

                                    this._recordCollection.SparseArray.Clear();

                                    // JJD 10/03/08
                                    // Now that we have called Clear we can add then from the templist safely
                                    foreach (GroupByRecord gbr in tempList)
                                    {
                                        gbr.InitializeParentCollection(newParentCollection);
                                        newParentCollection.SparseArray.Add(gbr);
                                    }

                                    this._recordCollection.SparseArray.Add(gbrExistingFieldLayout);
                                }
                                finally
                                {
                                    this._bypassRecordCollectionNotifications = oldBypassValue;
                                }

                                gbrToUse.ChildRecords.ViewableRecords.InsertDataRecordInGroupBySlot(dr);

								// SSP 8/19/09 TFS21078
								// Set this new flag so we don't re-add the dr again to gbrToUse.ChildRecords.ViewableRecords.
								// 
								dontInsertRecord = true;

                                this.RaiseChangeEvents(true);

                                gbrExistingFieldLayout.FireInitializeRecord();

                                #endregion //New FieldLayout and we need to create Layer of special FL GroupByRecords
                            }
                        }

                        #endregion //Process Root Groupby Collection
                    }

                    if (gbrToUse == null)
                    {
                        #region Look for a match based on value in exiting groupby rcds

                        // JJD 7/28/08 - BR33636
                        // Check for null existingGroupByRecord
                        //Field field = existingGroupByRecord.GroupByField;


                        // JJD 7/30/08 - Fix for regression caused by BR33636 changes
                        // The GroupByField from the this._recordCollection can be null in the case where the
                        // parent record is a special GroupByFieldLayout type record
                        //Field field = existingGroupByRecord != null ? existingGroupByRecord.GroupByField : this._recordCollection.GroupByField;
                        Field field = null;

                        if (existingGroupByRecord != null)
                            field = existingGroupByRecord.GroupByField;
                        else
                        {
                            field = this._recordCollection.GroupByField;

                            // JJD 7/30/08 - Fix for regression caused by BR33636 changes
                            // The GroupByField from the this._recordCollection can be null in the case where the
                            // parent record is a special GroupByFieldLayout type record.
                            // Here we want to see if the FieldLayout has a GroupByField which we
                            // can use.
                            if (field == null &&
                                fl.SortedFields.Count > 0 &&
                                fl.SortedFields[0].IsGroupBy)
                            {
                                field = fl.SortedFields[0].Field;
                            }
                        }

                        // JJD 7/30/08 - Fix for regression caused by BR33636 changes
                        // The field can be null if the special GroupByFieldLayout record's
                        // FieldLayout does not contain a groupby field
                        if (field != null)
                        {
                            // JJD 5/29/09 - TFS18063 
                            // Use the new overload to GetCellValue which will return the value 
                            // converted into EditAsType
                            //GroupByRecord gbr = new GroupByRecord(field, this._recordCollection, dr.GetCellValue(field, true));
                            GroupByRecord gbr = new GroupByRecord(field, this._recordCollection, dr.GetCellValue(field, CellValueType.EditAsType));

                            IGroupByEvaluator evaluator = field.GroupByEvaluatorResolved;

                            if (evaluator != null)
                                gbr.InitializeCommonValue(evaluator.GetGroupByValue(gbr, dr));

                            insertAtIndex = this.GetGroupByRecordInsertLocation(gbr, true);

                            Debug.Assert(insertAtIndex >= 0, "Index should never be negative here");

                            if (insertAtIndex < 0)
                                insertAtIndex = 0;

                            if (insertAtIndex < this._recordCollection.Count)
                            {
                                gbrToUse = this._recordCollection[insertAtIndex] as GroupByRecord;

                                if (gbrToUse == null ||
                                     !gbrToUse.DoesRecordMatchGroup(dr))
                                {
                                    gbrToUse = gbr;
                                    gbrToUseIsNew = true;
                                }
                            }
                            else
                            {
                                gbrToUse = gbr;
                                gbrToUseIsNew = true;
                            }

							// JJD 6/27/11 - TFS34868
							// If the gbrToUseIsNew flag was set to true and 
							// a custom GroupByComparer was specified then we can't
							// trust the binary search done above to find a match.
							// Therefor, we need to do a linear search since the order
							// of the groupby rcds is such that we can't assume its order
							// making the binary serah result unreliable.
							if (gbrToUseIsNew && field.GroupByComparerResolved != null)
							{
								GroupByRecord gbrLinear = this.FindMatchingGroupLinear(dr, evaluator);

								if (gbrLinear != null)
								{
									gbrToUse = gbrLinear;
									gbrToUseIsNew = false;
								}
							}
						}

                        #endregion //Look for a match based on value in exiting groupby rcds
                    }

                    if (gbrToUse != null)
                    {
                        #region Check if more nested groupby and call this method on descendant groupbys or insert datarecord here

						// SSP 8/19/09 TFS21078
						// If the new dontInsertRecord flag is set then don't add the dr to gbrToUse.ChildRecords.ViewableRecords.
						// Enclosed the existing code into the if block.
						// 
						if ( ! dontInsertRecord )
						{
							if ( RecordCollectionBase.HasAdditionalGroupByFields( gbrToUse.FieldLayout, gbrToUse.GroupByField ) )
								gbrToUse.ChildRecords.ViewableRecords.InsertDataRecordInGroupBySlot( dr );
							else
							{
								Record childRcd = null;
								if ( gbrToUse.ChildRecords.Count > 0 )
									childRcd = gbrToUse.ChildRecords[0];

								if ( childRcd == null || childRcd is DataRecord )
								{
									// JJD 7/28/08 - BR33636 
									// Initialize the parent record collection to the groupybyrcd's children
									dr.InitializeParentCollection( gbrToUse.ChildRecords );

									// JM 11-3-08 TFS9964
									//gbrToUse.ChildRecords.SparseArray.Add(dr);
									gbrToUse.ChildRecords.AddRecord( dr );

									// JJD 7/28/08 - BR33636 
									// Notify the groupby rcd that something has changed so it can invalidate
									// its description which may contain its child count
									gbrToUse.OnGroupingChanged( );

								}
								else
								{
									Debug.Fail( "Should have DataRecords in the collection" );
									if ( childRcd is GroupByRecord )
										gbrToUse.ChildRecords.ViewableRecords.InsertDataRecordInGroupBySlot( dr );
								}
							}
						}

                        #endregion //Check if more nested groupby and call this method on descendant groupbys or insert datarecord here

                        if (gbrToUseIsNew)
                        {
                            #region Insert new GroupbyRecord into this collection

                            Debug.Assert(insertAtIndex >= 0, "The index show not be negative here");

                            if (insertAtIndex < 0)
                                insertAtIndex = 0;

                            // JJD 7/28/08 - BR33636 
                            // Initialize the parent record collection to the groupybyrcd's children
                            gbrToUse.InitializeParentCollection(this._recordCollection);

                            this._recordCollection.InsertRecord(insertAtIndex, gbrToUse);

                            gbrToUse.FireInitializeRecord();

                            // JJD 7/28/08 - BR33636 
                            // Notify the groupby rcd's parentthat something has changed so it can invalidate
                            // its description which may contain its child count
                            GroupByRecord gbrParent = gbrToUse.ParentRecord as GroupByRecord;

                            if (gbrParent != null)
                                gbrParent.OnGroupingChanged();

                            #endregion //Insert new GroupbyRecord into this collection
                        }
                    }
                    else
                    {
                        #region Since the groupby field was null just add the data record to this collection

                        // JJD 7/30/08 - Fix for regression caused by BR33636 changes
                        // The field can be null if the special GroupByFieldLayout record's
                        // FieldLayout does not contain a groupby field and its fieldlayout
                        // doesn't contain at least one groupby field.
                        // In this case we just add the datarecord to the record collection

                        // Initialize the parent record collection to the groupybyrcd's children
                        dr.InitializeParentCollection(this._recordCollection);

						// JM 11-3-08 TFS9964
						//this._recordCollection.SparseArray.Add(dr);
						this._recordCollection.AddRecord(dr);

                        // JJD 7/28/08 - BR33636 
                        // Notify the groupby rcd's parent that something has changed so it can invalidate
                        // its description which may contain its child count
                        GroupByRecord parent = dr.ParentRecord as GroupByRecord;

                        if (parent != null)
                            parent.OnGroupingChanged();

                        #endregion //Since the groupby field was null just add the data record to this collection
                    }
                }
             }
        }

               #endregion //InsertDataRecordInGroupBySlot	

				#region MoveRecord

		internal void MoveRecord( Record record, int newSortedIndex, int oldSortedIndex )
		{
            // SSP 5/18/10 TFS32148
            // 
            int resetCounter = _collectionChanged_RaiseResetCounter;

			// SSP 6/15/09 TFS17912
			// When we raise the change notification below, we need to pass along the visible
			// indexes into the notification event args.
			// 
			MainRecordSparseArray sparseArr = this.RecordCollectionSparseArray;
			bool hasListeners = this.HasAnyListeners;

            // SSP 5/18/10 TFS32148
            // 
			//bool resetPending = _isResetNotificationNeeded;
            bool raiseReset = hasListeners && ( _isResetNotificationNeeded || this.VerificationNeeded );

			int oldVisibleIndex = hasListeners 
                // SSP 5/18/10 TFS32148
                // 
                //&& !_isResetNotificationNeeded
				//? sparseArr.VisibleIndexOf( record )
                && ! raiseReset
                ? sparseArr.GetVisibleIndexOf( oldSortedIndex, false )
				: -1;
			int newVisibleIndex = -1;

			// set a flag to bypass the notifications generated below
			this._bypassRecordCollectionNotifications = true;

			try
			{
				// move the record in the underlying array
				
				
				
				
                // SSP 5/18/10 TFS32148
                // Instead of calling RemoveAt/Insert, call the new Move which performs
                // the operation atomically. Commented out the existing code and added
                // new code.
                // 
                // ------------------------------------------------------------------------
                
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


                sparseArr.Move( oldSortedIndex, newSortedIndex );

                if ( resetCounter != _collectionChanged_RaiseResetCounter || _isResetNotificationNeeded )
                    raiseReset = true;

                if ( ! raiseReset )
                {
                    newVisibleIndex = hasListeners 
                        ? sparseArr.GetVisibleIndexOf( newSortedIndex, false )
                        : -1;

                    if ( null == record )
                        record = sparseArr.GetItem( newSortedIndex, true );
                }
                // ------------------------------------------------------------------------
			}
			finally
			{
				// reset the flag
				this._bypassRecordCollectionNotifications = false;
			}

            // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing 
            // If we are verifying special records we don't need to raise
            // change notifications since that will be done automatically
            // at the end of the verify routine if necessary
            if (this._inVerifySpecialRecords)
                return;

			// SSP 6/15/09 TFS17912
			// Above we'll either get both indexes or neither.
			// 
			if ( oldVisibleIndex < 0 != newVisibleIndex < 0 )
			{
				//Debug.Assert( false );
				_isResetNotificationNeeded = true;
			}

            // SSP 5/18/10 TFS32148
            // 
			//if ( this._isResetNotificationNeeded == true )
            if ( raiseReset )
				this.RaiseChangeEvents( true );
			else
			{
				// SSP 6/15/09 TFS17912
				// When we raise the change notification, we need to pass along the visible
				// indexes into the notification event args.
				// 
				// --------------------------------------------------------------------------------
				if ( oldVisibleIndex >= 0 && newVisibleIndex >= 0 )
				{
					// get count of the special rcds on top
					int countOfSpecialRcdsOnTop = this.CountOfSpecialRecordsOnTop;

					// adjust the indices for the special rcd count
					newVisibleIndex += countOfSpecialRcdsOnTop;
					oldVisibleIndex += countOfSpecialRcdsOnTop;

					this.RaiseChangeEvents( new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Move, record, newVisibleIndex, oldVisibleIndex ) );
				}
				
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

				// --------------------------------------------------------------------------------
			}

		}

				#endregion //MoveRecord	

				#region OnFixedStatusChanged

		internal void OnFixedStatusChanged(Record record)
		{
            // JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
            this.RemoveFromFixedList(record);

            // JJD 6/17/09 - NA 2009 Vol 2 - Record fixing
            // Maintain fixed non-special record lists based on the 
            // new state of the record
            switch (record.FixedLocation)
            {
                case FixedRecordLocation.FixedToTop:
                    this.AddRecordToFixedList(true, record);
                    break;

                case FixedRecordLocation.FixedToBottom:
                    this.AddRecordToFixedList(false, record);
                    break;

                case FixedRecordLocation.Scrollable:
                default:
                    if (this._fixedNonSpecialRecordsOnBottom != null &&
                        this._fixedNonSpecialRecordsOnBottom.Contains(record))
                        this._fixedNonSpecialRecordsOnBottom.Remove(record);
                    else
                    if (this._fixedNonSpecialRecordsOnTop != null &&
                        this._fixedNonSpecialRecordsOnTop.Contains(record))
                        this._fixedNonSpecialRecordsOnTop.Remove(record);

                    if (this._pendingUnfixedNonSpecialRecords == null)
                        this._pendingUnfixedNonSpecialRecords = new Queue<Record>();

                    this._pendingUnfixedNonSpecialRecords.Enqueue(record);
                    break;
            }

            if (record.DataPresenter.IsInitializeRecordSuspendedFor(record))
            {
				this.DirtySpecialRecords( false );
                return;
            }

            // JJD 6/17/09 - NA 2009 Vol 2 - Record fixing
            // If we are in the middle of a reset or then just dirty the special records and return
            RecordManager rm = this.RecordManager;

            if (null == rm || rm.IsInReset)
            {
				this.DirtySpecialRecords( false );
				return;
            }

			int oldIndex = this.IndexOf(record);

            // JJD 6/25/09 - NA 2009 Vol 2 - Record fixing
            // If the oldIndex is -1 then we can just dirty and return. This can happen
            // if the record being fixed if its Visibility is not set to Visible
            // or if the record has been filetered out
            if (oldIndex < 0)
            {
                this.DirtySpecialRecords(false);
                return;
            }

			// JJD 12/6/10 - TFS25227
			// Set the dirty flag for fixed records so we verify thm on the next verify pass
			this.DirtyFixedRecordOrder();

			this.VerifyCachedRecordStatus(record);

			int newIndex = this.IndexOf(record);

			Debug.Assert(newIndex >= 0);

			if ( oldIndex == newIndex || !this.HasAnyListeners )
			{
				// SSP 5/7/08 - Summary Record/Record Separators
				// Fixing a data record can cause scrolling special records to get fixed (since you can't have
				// a scrolling record before a fixed record on top etc...), we need to verify special records
				// status.
				// 
				this.DirtySpecialRecords( true );

				return;
			}

			if (newIndex >= 0)
			{
				// if the record wasn't in the collection before then something is
				// really messed up so send a full Reset notification
				if (oldIndex < 0)
					this.RaiseChangeEvents(true);
				else
				{
					// if the index has changed send a move notification
					if (newIndex != oldIndex)
						
						
						
						this.RaiseCollectionChanged( NotifyCollectionChangedAction.Move, record, newIndex, oldIndex );
				}
			}
			else
				this.RaiseChangeEvents(true);

			// SSP 5/7/08 - Summary Record/Record Separators
			// Fixing a data record can cause scrolling special records to get fixed (since you can't have
			// a scrolling record before a fixed record on top etc...), we need to verify special records
			// status.
			// 
			this.DirtySpecialRecords( true );
		}

				#endregion //OnFixedStatusChanged

				#region OnScrollCountDirtiedHelper

		// SSP 2/11/09 TFS12467
		// 
		internal void OnScrollCountDirtiedHelper( )
		{
			if ( _recordCollection.IsUpdating )
			{
				_recordCollection.BeginUpdateInfo._scrollCountDirtied = true;
				return;
			}

			Record parentRecord = _recordCollection.ParentRecord;

			// SSP 5/18/09 TFS16576
			// Also if the sparse array is in the the process of calculating scroll count, 
			// then don't recursively call into its ScrollCount property which whill be
			// invalid and also will result in the sparse array to recursively start 
			// calculating the scroll count again.
			// 
			MainRecordSparseArray sparseArr = this.RecordCollectionSparseArray;
			if ( null != sparseArr && sparseArr.InCalculatingScrollCount )
			{
				
				// Mark the _cachedOverallScrollCountDirty dirty so the data presenter re-gets 
				// the new count.
				// 
				DataPresenterBase dp = _recordCollection.DataPresenter;
				if ( null != dp )
					dp._cachedOverallScrollCountDirty = true;
				
				return;
			}

			bool areAllAncestorsExpanded = this.AreAllAncestorRecordsExpanded;

			bool dirtyParentRecordScrollCount = false;
			bool invalidateParentRecordProps = false;
			bool raiseSelfScrollCountChanged = false;

			// If this viewable record collection is in view then synchronously calculate the
			// scroll count and raise notification only if the scroll count is different. This 
			// is what we used to do. Alternative would be to always raise notifications and 
			// lazily calculate the scroll count.
			// 
			if ( areAllAncestorsExpanded )
			{
				int scrollCount = this.ScrollCount;
				if ( _lastScrollCount != scrollCount )
				{
					_lastScrollCount = scrollCount;

					raiseSelfScrollCountChanged = true;
					dirtyParentRecordScrollCount = true;
					invalidateParentRecordProps = true;
				}
			}
			else
			{
                // JJD 5/11/10 - TFS32091
                // Reset the _lastScrollCount member so the next time the ancestors are
                // expanded we will force a dirty of the scroll count. This ensures
                // that the overall scroll count remains valid.
                this._lastScrollCount = -1;

				raiseSelfScrollCountChanged = true;

				if ( null != parentRecord && parentRecord.IsExpanded )
					dirtyParentRecordScrollCount = true;

				invalidateParentRecordProps = true;
			}

			if ( raiseSelfScrollCountChanged )
				this.RaisePropertyChangedEvent( "ScrollCount" );

			if ( dirtyParentRecordScrollCount || invalidateParentRecordProps )
			{
				while ( null != parentRecord )
				{
					if ( dirtyParentRecordScrollCount )
						parentRecord.DirtyScrollCount( );

					// invalidate its ExpansionIndicatorVisibility and HasChildren properties
					if ( invalidateParentRecordProps )
					{
						parentRecord.InvalidateExpansionIndicatorVisibility( );
						parentRecord.InvalidateHasChildren( );

						// SSP 5/12/09 TFS16576
						// 
						GroupByRecord parentGroupByRecord = parentRecord as GroupByRecord;
						if ( null != parentGroupByRecord )
							parentGroupByRecord.DirtyFilterState( areAllAncestorsExpanded );
					}

					// once we hit a DataRecord we can stop
					if ( parentRecord is DataRecord )
						break;

					// walk up its parent chain
					parentRecord = parentRecord.ParentRecord;
				}
			}
		}

				#endregion // OnScrollCountDirtiedHelper

				#region OnRecordCollectionFieldLayoutChanged

		
		
		internal void OnRecordCollectionFieldLayoutChanged( )
		{
			_specialRecordsVersionTracker = null;

			// SSP 1/21/09 - NAS9.1 Record Filtering
			// Reset the _verifiedSpecialRecordsVersion so we recreate 
			// special records based on the new field layout.
			// 
            // JJD 8/27/09 - TFS21513
            // Call DirtySpecialRecords instead so we don't step on the constant
            // to let us know that the fixed record order needs to be verified
            //_verifiedSpecialRecordsVersion = -1;
            this.DirtySpecialRecords(false);


			// If there are any listeners that are hooked into this vrc, make sure we 
			// hook into the new field layout and also verify special records so the listeners
			// get synchronously notified.
			// 
			this.VerifyHasAnyListeners( );
			if ( this.HasAnyListeners )
				this.Verify( );
		}

				#endregion // OnRecordCollectionFieldLayoutChanged

                // JJD 7/1/09 - NA 2009 Vol 2 - Record fixing
                #region OnRecordRefreshSortPosition

        internal void OnRecordRefreshSortPosition(Record record)
        {
            this.EnsureCorrectSortPosition(record);

            if (record.FixedLocation != FixedRecordLocation.Scrollable)
            {
                if (record.IsOnTopWhenFixed)
                    this.RefreshSortOrderOfFixedRecordsHelper(this._fixedNonSpecialRecordsOnTop);
                else
                    this.RefreshSortOrderOfFixedRecordsHelper(this._fixedNonSpecialRecordsOnBottom);
            }

        }

                #endregion //OnRecordRefreshSortPosition	
    
				// JJD 4/14/07 - added to support record Visibility property changes
				#region OnRecordVisibilityChanged

		internal void OnRecordVisibilityChanged(Record record, Visibility oldValue, int oldIndex)
		{
			// SSP 12/11/08 - NAS9.1 Record Filtering
			// 
			if ( !this.ShouldRaiseCollectionNotifications )
				return;

			bool wasCollapsed = oldValue == Visibility.Collapsed;
			bool isCollapsed = record.VisibilityResolved == Visibility.Collapsed;

			if (wasCollapsed == isCollapsed)
				return;

			// SSP 1/12/09 - NAS9.1 Record Filtering
			// 
			int resetNotificationCounter = _collectionChanged_RaiseResetCounter;

			// this will either add or remove the record from the cache
			this.VerifyCachedRecordStatus(record);
            
            // JJD 7/14/09 - TFS19373
            // Make sure we dirty the special records if this record is flagged
            // as fixed so we can maintain the fixed record lists properly
            if (record.FixedLocation != FixedRecordLocation.Scrollable)
                this.DirtySpecialRecords(false);

			// SSP 1/12/09 - NAS9.1 Record Filtering
			// Since Verify above raised the Reset notification, there's no need for 
			// us to raise any other notification. It may even cause probelms. So
			// skip raising Add/Remove notification if we raised Reset above.
			// Enclosed the existing code in the if block.
			// 
			if ( resetNotificationCounter == _collectionChanged_RaiseResetCounter )
			{
				// raise either an add or remove notification
				if ( isCollapsed )
				{
					//Debug.Assert( oldIndex >= 0 );

					if ( oldIndex >= 0 )
						this.RaiseChangeEvents( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, record, oldIndex ) );
				}
				else
				{
					int index = this.GetIndexOf( record, false );

					//Debug.Assert( index >= 0 );

					if ( index >= 0 )
						this.RaiseChangeEvents( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, record, index ) );
				}
			}
		}

				#endregion //OnRecordVisibilityChanged

                // JJD 6/22/09 - NA 2009 Vol 2 - Record Fixing - added
                #region RefreshSortOrderOfFixedRecords

        internal void RefreshSortOrderOfFixedRecords()
        {
            this.RefreshSortOrderOfFixedRecordsHelper(this._fixedNonSpecialRecordsOnTop);
            this.RefreshSortOrderOfFixedRecordsHelper(this._fixedNonSpecialRecordsOnBottom);
        }

        private void RefreshSortOrderOfFixedRecordsHelper(List<Record> list)
        {
            if (list == null || list.Count < 2)
                return;

            RecordManager rm = this.RecordManager;

            if (rm != null)
            {
                list.Sort(new RecordManager.RecordsSortComparer(!rm.IsSorted));
                this.DirtySpecialRecords(false);
            }

        }

                #endregion //RefreshSortOrderOfFixedRecords	
    
				#region RemoveRecord

		internal bool RemoveRecord(Record record, bool notifyListeners )
        {
			// get the index of the record in the collection
            // SSP 5/12/10 TFS32148
            // 
            // ------------------------------------------------------------------------
            //int index = this.IndexOf(record);
            int index = -2;
            bool raiseReset = false;
            if ( notifyListeners )
            {
                if ( this.VerificationNeeded )
                    raiseReset = true;
                else
                    index = this.IndexOf( record );
            }
            // ------------------------------------------------------------------------

            // JJD 2/6/09
            // It is possible that the record is not visible and therefore it would
            // not be in the viewable record collection but we still need to
            // remove it from the underlying collections sparse array
			//Debug.Assert(index >= 0);
            //if ( index < 0 )
            //    return false;
            // SSP 5/12/10 TFS32148
            // Commented out the assert since now with the change above index can be
            // negative even when the record is visible.
            // 
			//Debug.Assert(index >= 0 || record.VisibilityResolved == Visibility.Collapsed);

			bool recordWasRemoved = false;

			DataRecord dr = record as DataRecord;
			MainRecordSparseArray recordCollectionSparseArray = this.RecordCollectionSparseArray;
			int indexInSparseArray = recordCollectionSparseArray.IndexOf(record);

			// JJD 2/16/11 - TFS64006
			// Add a flag so we know if we should call DirtySpecialRecords below
			bool dirtySpecialRcds = false;
			
			if (dr != null)
			{
				// JJD 2/16/11 - TFS64006
				// Remove the datarecord from the list of records inserted at top
				if (_recordsInsertedAtTop != null)
					dirtySpecialRcds = _recordsInsertedAtTop.Remove(dr);
			}

			// SSP 2/19/09 - NAS9.1 Record Filtering
			// 
			int resetEventCounter = _collectionChanged_RaiseResetCounter;

			if ( indexInSparseArray >= 0 )
			{
                if (notifyListeners)
                {
                    try
                    {
                        this._bypassRecordCollectionNotifications = true;
                        this.RecordCollection.RemoveAt(indexInSparseArray);
                        recordWasRemoved = true;
                    }
                    finally
                    {
                        this._bypassRecordCollectionNotifications = false;
                    }
                }
                else
                {
                    recordCollectionSparseArray.RemoveAt(indexInSparseArray);

                    // JJD 2/25/08 - BR30660 
                    // set the stack flag so we know the record was removed
                    recordWasRemoved = true;
                }

				// SSP 2/19/09 - NAS9.1 Record Filtering
				// When the last record is removed, we need to re-generate list of special records
				// because visibility of some special records, like the filter record, depends on
				// whether there are any data records in the collection or not.
				// 
                // JJD 6/24/09 - NA 2009 Vol 2 - Record fixing
                // We also need to dirty the special records if the recor was fixed
				//if ( 0 == recordCollectionSparseArray.Count )
				if (0 == recordCollectionSparseArray.Count ||
					record.FixedLocation != FixedRecordLocation.Scrollable)
				{
					// JJD 2/16/11 - TFS64006
					// Set the flag instead of calling DirtySpecialRecords 
					// so we only call it once.
					//this.DirtySpecialRecords( true );
					dirtySpecialRcds = true;
				}
			}

			// JJD 2/16/11 - TFS64006
			// If the flag is set call DirtySpecialRecords 
			if ( dirtySpecialRcds )
				this.DirtySpecialRecords( true );

            // JJD 2/6/09
            // It is possible that the record is not visible and therefore it would
            // not be in the viewable record collection but we still need to
            // remove it from the underlying collections sparse array.
            // Now that it has been removed from the sparsearray we should not 
            // send out any notifications for the ViewableRecordsCollection
            // since its count has not been affected. 
            // SSP 5/12/10 TFS32148
            // Added the if block and enclosed the existing code into the else block.
            // 
            if ( raiseReset )
                _isResetNotificationNeeded = true;
            else if ( index < 0 )
                notifyListeners = false;

			// SSP 2/19/09 - NAS9.1 Record Filtering
			// Don't notify again if we have already raised reset notification as a result
			// of DirtySpecialRecords call above.
			// 
			//if (notifyListeners)
			if ( notifyListeners && resetEventCounter == _collectionChanged_RaiseResetCounter )
			{
				if (this._isResetNotificationNeeded == true)
					this.RaiseChangeEvents(true);
				else
				{
					if (recordWasRemoved)
						this.RaiseChangeEvents(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, record, index));
				}
			}

            // JJD 2/25/08 - BR30660 
            // If the record that was removed was a child of a GroupByRecord and that caused the 
            // parent GroupByrecord's child count to go to zero then
            // we should remove the parent groupByRecord as well.
            if (recordWasRemoved)
            {
                GroupByRecord parentGroupBy = this.RecordCollection.ParentRecord as GroupByRecord;

				if ( parentGroupBy != null )
				{
					if ( this.RecordCollection.SparseArray.Count == 0 &&
						parentGroupBy.ParentCollection != null )
					{
						parentGroupBy.ParentCollection.ViewableRecords.RemoveRecord( parentGroupBy, notifyListeners );
					}
					// SSP 7/11/08 BR33286
					// Added else block. Since the Count has changed which could affect the group-by record's
					// Description, raise Description property change notification on the group-by record.
					// 
					else
					{
						parentGroupBy.OnGroupingChanged( );
					}
				}
            }

			return recordWasRemoved;
        }

                #endregion //RemoveRecord
 
				#region RemoveRecordAtUnsortedIndex
    
    	internal bool RemoveRecordAtUnsortedIndex(int unsortedIndex, bool notifyListeners )
        {
			// get the index of the record in the collection
            int index = this.GetSortedIndexFromUnsortedIndex(unsortedIndex);

			Debug.Assert(index >= 0);

			if ( index < 0 )
				return false;

			if (notifyListeners)
			{
				try
				{
					this._bypassRecordCollectionNotifications = true;
					this.RecordCollection.RemoveAt(index);
					
				}
				finally
				{
					this._bypassRecordCollectionNotifications = false;
				}

				this.RaiseChangeEvents(true);
				//this.RaiseChangeEvents(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, null, index));
			}
			else
				this.RecordCollectionSparseArray.RemoveAt(index);

			return true;
        }

                #endregion //RemoveRecordAtUnsortedIndex

				#region RaiseChangeEvents

		internal void RaiseChangeEvents(bool fullReset)
		{
			if (fullReset)
				this.RaiseChangeEvents(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			else
			{
				this.RaisePropertyChangedEvent("Count");
				this.RaisePropertyChangedEvent("Item[]");

				// SSP 2/12/09 TFS12467
				// Don't synchronously calculate the scroll count. Instead call OnScrollCountDirtiedHelper 
				// which will raise the ScrollCount property change notification.
				// 
				//this.VerifyScrollCount();
				this.OnScrollCountDirtiedHelper( );
			}
		}

		internal void RaiseChangeEvents( NotifyCollectionChangedAction action, Record item, int newSortedIndex, int oldSortedIndex)
		{
			this.RaiseChangeEvents( new NotifyCollectionChangedEventArgs( action, item, newSortedIndex, oldSortedIndex ) );
		}

		internal void RaiseChangeEvents(NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Reset)
				this._isResetNotificationNeeded = false;

			// SSP 2/12/09 TFS12467
			// Dirtying scroll count shouldn't be necessary since any change in the sparse array
			// will automatically cause the sparse array to dirty scroll count.
			// 
			//this.DirtyScrollCounts();

			this.RaisePropertyChangedEvent("Count");
			this.RaisePropertyChangedEvent("Item[]");
			
			
			
			
			this.RaiseCollectionChanged( args );

			// SSP 2/12/09 TFS12467
			// Related to above change. Don't synchronously calculate the scroll count. 
			// Instead call OnScrollCountDirtiedHelper which will raise the ScrollCount 
			// property change notification.
			// 
			//this.VerifyScrollCount();
			this.OnScrollCountDirtiedHelper( );
		}

				#endregion //RaiseChangeEvents

				#region RaiseCollectionChanged

		
		
		
		private void RaiseCollectionChanged( NotifyCollectionChangedAction action, Record item, int newSortedIndex, int oldSortedIndex )
		{
			this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( action, item, newSortedIndex, oldSortedIndex ) );
		}

		internal void RaiseCollectionChanged( NotifyCollectionChangedEventArgs args )
		{
			// SSP 1/12/09 - NAS9.1 Record Filtering
			// This is used to figure out if an action caused Reset notification 
			// in which case any pending granular notifications (like add, remove)
			// don't get raised (which will potentially cause problems).
			// 
			if ( NotifyCollectionChangedAction.Reset == args.Action )
				_collectionChanged_RaiseResetCounter++;

			// SSP 2/11/09 TFS12467
			// 
			RecordManager rm = this.RecordManager;
			if ( null != rm && rm.IsUpdating )
			{
				rm.BeginUpdateInfo.EnqueNotification( this, args );
				return;
			}
			
			if ( null != _collectionChanged )
				_collectionChanged( this, args );

			// JM 6-28-10 TFS33366 - Added.
			if (this._recordCollection != null)
				this._recordCollection.BumpCollectionVersion();
		}

				#endregion // RaiseCollectionChanged

				#region RaisePropertyChangedEvent

		// SSP 2/11/09 TFS12467
		// 
		internal new void RaisePropertyChangedEvent( string propName )
		{
			RecordManager rm = this.RecordManager;
			if ( null != rm && rm.IsUpdating )
			{
				rm.BeginUpdateInfo.EnqueNotification( this, new PropertyChangedEventArgs( propName ) );
				return;
			}

			base.RaisePropertyChangedEvent( propName );
		}

				#endregion // RaisePropertyChangedEvent

				#region Verify

		
		
		internal void Verify( )
		{
			// SSP 10/20/10 
			
			// If a reset notification is pending (yet to be processed) then process it now to 
			// make sure the records are synchronized with the data source.
			// 
			RecordManager rm = _recordCollection.ParentRecordManager;
			if ( null != rm && rm.IsResetPending )
				rm.OnDelayedReset( );

			// SSP 8/10/09 - NAS9.2 Enhanced grid-view
			// Added _verifiedFieldsVersion so check for that. Also moved code that 
			// gets the field layout and fields here from below.
			// 
			ExpandableFieldRecordCollection expandableRecords = _recordCollection as ExpandableFieldRecordCollection;
			if ( null != expandableRecords )
				expandableRecords.VerifyChildren( );

			// SSP 12/12/08 - NAS9.1 Record Filtering
			// First make sure that records are filtered.
			// 
			this.EnsureFiltersEvaluated( );

			this.VerifySpecialRecords( );
		}

				#endregion // Verify

				// JJD 11/22/11 - TFS96310 - added
				#region VerifyFixedRecordLists

		// JJD 11/22/11 - TFS96310
		// Added VerifyFixedRecordLists which will make sure the lists of fixed records that the
		// vrc maintains pick up any records that were fixed from the group islands.
		internal void VerifyFixedRecordLists()
		{
			this.VerifyFixedRecordList(true);
			this.VerifyFixedRecordList(false);
		}

				#endregion //VerifyFixedRecordLists	
    
				#region VerifyScrollCount

		// SSP 2/11/09 TFS12467
		// Not used anymore. Commented out.
		// 
		
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


				#endregion //VerifyScrollCount

			#endregion //Internal Methods

			#region Private Methods

				#region AddFixedRecordsHelper

		
		
		/// <summary>
		/// Adds records from source to dest that have their FixedRecordLocationOverride set to Top or Bottom.
		/// </summary>
		/// <param name="source">The source list of records. Can be null.</param>
		/// <param name="dest">List to which to append the fixed records from the source list.</param>
		private void AddFixedRecordsHelper( List<Record> source, List<Record> dest )
		{
			if ( null != source )
			{
				for ( int i = 0, count = source.Count; i < count; i++ )
				{
					Record record = source[i];
					FixedRecordLocationInternal fixedLoc = record.FixedRecordLocationOverride;
					if ( FixedRecordLocationInternal.Top == fixedLoc || FixedRecordLocationInternal.Bottom == fixedLoc )
						dest.Add( record );
				}
			}
		}

				#endregion // AddFixedRecordsHelper

                // JJD 6/22/09 - NA 2009 Vol 2 - Record fixing
                #region AddRecordToFixedList

        private void AddRecordToFixedList(bool onTop, Record record)
        {

            List<Record> list;

            if (onTop)
            {
                if (this._fixedNonSpecialRecordsOnTop == null)
                    this._fixedNonSpecialRecordsOnTop = new List<Record>();

                list = this._fixedNonSpecialRecordsOnTop;
            }
            else
            {
                if (this._fixedNonSpecialRecordsOnBottom == null)
                    this._fixedNonSpecialRecordsOnBottom = new List<Record>();

                list = this._fixedNonSpecialRecordsOnBottom;
            }

            // insert the record at the beginning or the end
            if (onTop)
                list.Add(record);
            else
                list.Insert(0, record);

            int limit = record.FieldLayout.FixedRecordLimitResolved;
			
            // if there is no limit then return
			if (limit < 1)
				return;

			if (limit >= list.Count)
				return;

			Queue<Record> rcdsToBump = new Queue<Record>();

			int index = (onTop) ? 0 : list.Count - 1;
			int step = (onTop) ? 1 : -1;
			int remainingRcdsToBump = list.Count - limit;

			// see which rcds we need to bump until we get down to the limit
			while (remainingRcdsToBump > 0)
			{
				rcdsToBump.Enqueue(list[index]);
				index += step;
				remainingRcdsToBump--;
			}

			// Just set the location to scrollable which will call back into 
			// OnFixedStatusChanged and remove it from the list
			while (rcdsToBump.Count > 0)
			{
				rcdsToBump.Dequeue().FixedLocation = FixedRecordLocation.Scrollable;
			}
        }

                #endregion //AddRecordToFixedList	
    
				#region AreAllAncestorRecordsExpanded

		// SSP 2/11/09 TFS12467
		// 
		internal bool AreAllAncestorRecordsExpanded
		{
			get
			{
				Record record = this.RecordCollection.ParentRecord;
				return null == record || record.IsExpanded && record.IsAncestorChainExpanded;
			}
		}

				#endregion // AreAllAncestorRecordsExpanded

				#region DirtyScrollCounts

		// SSP 2/12/09 TFS12467
		// Reimplemented how we dirtied and verified scroll counts. Commented this method 
		// out since it's not used anymore.
		// 
		
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

				#endregion //DirtyScrollCounts	

				// JJD 6/27/11 - TFS34868 - added
				#region FindMatchingGroupLinear

		private GroupByRecord FindMatchingGroupLinear(DataRecord dr, IGroupByEvaluator evaluator)
		{
			int count = this._recordCollection.Count;

			// do a linear walk to find the group that the DataRecord belongs to.
			for (int i = 0; i < count; i++)
			{
				GroupByRecord gbr = this._recordCollection[i] as GroupByRecord;

				if (gbr != null && evaluator.DoesGroupContainRecord(gbr, dr))
					return gbr;
			}

			return null;
		}

				#endregion //FindMatchingGroupLinear	
    
				#region FixInterveningNonFixedRecords

		/// <summary>
		/// Sets FixedRecordLocationOverride on non-fixed records that are before other fixed records.
		/// </summary>
		/// <param name="top">Whether these are top fixed records.</param>
		/// <param name="specialRecords">Special records</param>		
		/// <param name="fixedNonSpecialRecords">Fixed non-special (data) records</param>
		private void FixInterveningNonFixedRecords( bool top, List<Record> specialRecords, List<Record> fixedNonSpecialRecords )
		{
			int specialRecordsCount = specialRecords.Count;
			int si = top ? 0 : specialRecordsCount - 1;
			int ei = top ? specialRecordsCount - 1 : 0;
			int step = top ? 1 : -1;

			// Find the last fixed record. We need to fix all the records before it or after 
			// it depending upon whether records are fixed on top or bottom.
			// 
			int lastFixedRecordIndex;
			if ( null == fixedNonSpecialRecords || 0 == fixedNonSpecialRecords.Count )
			{
				lastFixedRecordIndex = -1;
				for ( int i = si; i >= 0 && i < specialRecordsCount; i += step )
				{
					Record rr = specialRecords[i];
					if ( rr.IsFixed )
						lastFixedRecordIndex = i;
				}
			}
			else
			{
				// If there are non-special fixed records (data or group-by records that are fixed), then
				// all special records have to be fixed. In which case leave lastFixedRecordIndex to -1
				// which will cause us to fix all special records further below.
				// 
				lastFixedRecordIndex = ei;
			}

			if ( lastFixedRecordIndex >= 0 )
			{
				// Fix intervening special records that are not fixed.
				FixedRecordLocationInternal value = top ? FixedRecordLocationInternal.Top : FixedRecordLocationInternal.Bottom;
				for ( int i = si; i >= 0 && i < specialRecordsCount; i += step )
				{
					Record rr = specialRecords[i];
					rr.FixedRecordLocationOverride = value;
					if ( i == lastFixedRecordIndex )
						break;
				}
			}
		}

				#endregion // FixInterveningNonFixedRecords

                // JJD 8/27/09 - TFS21513
                #region GetFixedRecordsList

        private List<Record> GetFixedRecordsList(Record[] records, bool top)
        {
            List<Record> fixedRecords = null;

            int count = records.Length;
            int start = top ? 0 : count - 1;
            int step = top ? 1 : -1;
            int end = top ? count : -1;
            FixedRecordLocation fixedLocation = top ? FixedRecordLocation.FixedToTop : FixedRecordLocation.FixedToBottom;

            for (int i = start; i != end; i += step)
            {
                Record rcd = records[i];
                if (rcd.FixedLocation != fixedLocation)
                    break;

                if (fixedRecords == null)
                    fixedRecords = new List<Record>();

                fixedRecords.Add(rcd);
            }

            if (!top && fixedRecords != null && fixedRecords.Count > 1)
                fixedRecords.Reverse();

            return fixedRecords;

        }

                #endregion //GetFixedRecordsList	
        
				#region GetGroupByRecordInsertLocation



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        private int GetGroupByRecordInsertLocation(GroupByRecord groupByRecord, bool returnIndexOnMatch)
        {
            Field groupByField = groupByRecord.GroupByField;

            bool ascending = true;

            IComparer groupBySortComparer   = null;
            IComparer sortComparer          = null;
            DataRecord firstDescendantDataRecord = null;
            object firstDescendantDataRecordCellValue = null;
            bool caseInsenstive = false;

            if (groupByField == null)
            {
                if (groupByRecord.RecordType != RecordType.GroupByFieldLayout)
                {
                    Debug.Assert(false);
                    return -1;
                }
            }
            else
            {
                if (groupByRecord.ParentCollection != this._recordCollection ||
                     this._recordCollection.GroupByField != groupByField)
                {
                    Debug.Assert(false);
                    return -1;
                }

                groupBySortComparer = groupByField.GroupByComparerResolved;
                sortComparer = groupByField.SortComparerResolved;
                ascending = groupByField.SortStatus == SortStatus.Ascending;
                caseInsenstive = FieldSortComparisonType.CaseInsensitive == groupByField.SortComparisonTypeResolved;

                if (sortComparer != null)
                {
                    firstDescendantDataRecord = groupByRecord.GetFirstDescendantDataRecord();

                    if (firstDescendantDataRecord != null)
                    {
                        // JJD 5/29/09 - TFS18063 
                        // Use the new overload to GetCellValue which will return the value 
                        // converted into EditAsType
                        //firstDescendantDataRecordCellValue = firstDescendantDataRecord.GetCellValue(groupByField, true);
                        firstDescendantDataRecordCellValue = firstDescendantDataRecord.GetCellValue(groupByField, CellValueType.EditAsType);
                    }
                }
            }

            SparseArray sparseArr = this._recordCollection.SparseArray;
            RecordCollection childRecords = groupByRecord.ChildRecords;
            FieldLayout fl = groupByRecord.FieldLayout;

            int i = 0, j = sparseArr.Count - 1;
            int currIndex = sparseArr.IndexOf(groupByRecord);
            while (i <= j)
            {
                int m = (i + j) / 2;

                GroupByRecord iiGroupByRecord = (GroupByRecord)sparseArr[m];
                int r;
                // SSP 3/23/05
                //
                // ------------------------------------------------------------------
                //if ( iiGroupByRow == groupByRow )
                //	r = 0;
                //else 
                if (iiGroupByRecord == groupByRecord)
                {
                    if (i != j)
                        iiGroupByRecord = (GroupByRecord)sparseArr[++m];
                    else
                        break;
                }
                // ------------------------------------------------------------------

                if (groupByField == null)
                {
                    FieldLayout flToCompare = iiGroupByRecord.FieldLayout;
                    // JJD 2/23/07
                    // Compare fieldlayouts by their index which would normally
                    // be by the order that the records were encountered. This is
                    // also something that can be controlled by the application developer
                    if (fl == flToCompare)
                        break;
                    else
                        r = fl.Index < flToCompare.Index ? -1 : 1;
                }
                else
                {
                    if (null != groupBySortComparer)
                        r = groupBySortComparer.Compare(groupByRecord, iiGroupByRecord);
                    else
                    {
                        DataRecord dr = null;
                        if (null != sortComparer && firstDescendantDataRecord != null)
                            dr = iiGroupByRecord.GetFirstDescendantDataRecord();

                        if (dr != null)
                        {
                            r = sortComparer.Compare(
                                firstDescendantDataRecordCellValue,
                                // JJD 5/29/09 - TFS18063 
                                // Use the new overload to GetCellValue which will return the value 
                                // converted into EditAsType
                                //dr.GetCellValue(groupByField, true));
                                dr.GetCellValue(groupByField, CellValueType.EditAsType));
                        }
                        else
                            r = RecordManager.RecordsSortComparer.DefaultCompare(
                               groupByRecord.Value, iiGroupByRecord.Value, true, caseInsenstive);
                    }
                }

                if (!ascending)
                    r = -r;

                if (r < 0)
                    j = m - 1;
                else if (r > 0)
                    i = m + 1;
                // If the row is where it should be then don't move it. That's what the
                // the below else if does.
                //
                else if (returnIndexOnMatch)
                    return m;
                else if (currIndex >= 0 && currIndex < m)
                    j = m - 1;
                else
                    i = m + 1;
            }

			// SSP 3/10/10 TFS25807
			// Callers typically remove the record from its current location and insert it
			// at the location returned by this method. If the record happens to be before
			// the new insert location in the array, the removal of the record will cause
			// the returned insert location to be off by 1. Since GetDataRecordInsertLocation 
			// method does this check, for consistency do the same check here and return the 
			// adjusted index.
			// 			
            //return i;
			return currIndex >= 0 && currIndex < i ? i - 1 : i;
        }

				#endregion // GetGroupByRecordInsertLocation

				#region GetIndexOf

		internal int GetIndexOf( Record record, bool verify )
		{
			if (verify == true)
				this.Verify();

			int prefixRecordCount = 0;

			if (this._specialRecordsOnTop != null)
				prefixRecordCount = this._specialRecordsOnTop.Count;

			// loop over the top special records
			for (int i = 0; i < prefixRecordCount; i++)
			{
				if (this._specialRecordsOnTop[i] == record)
					return i;
			}

			int suffixRecordCount = 0;

			if (this._specialRecordsOnBottom != null)
				suffixRecordCount = this._specialRecordsOnBottom.Count;

			// loop over the bottom special records
			for (int i = 0; i < suffixRecordCount; i++)
			{
				if (this._specialRecordsOnBottom[i] == record)
					return this.RecordCollectionSparseArray.VisibleCount + prefixRecordCount + i;
			}

			// get the index in the sorted collection
			int sortedIndex = this.RecordCollectionSparseArray.VisibleIndexOf(record);

			if (sortedIndex < 0)
				return sortedIndex;

			// return the sorted index plus any top special records
			return sortedIndex + prefixRecordCount;
		}

				#endregion //GetIndexOf

                // JJD 6/23/09 - NA 2009 Vol 2 - Record fixing - added
                #region GetListIndexFromUnsortedIndex

        private int GetListIndexFromUnsortedIndex(IList list, int unsortedIndex, RecordSparseArrayBase unsortedArray, ref int rcdsWithHigherIndices)
        {
            int count = list.Count;
            int matchingIndex = -1;

            for (int i = 0; i < count; i++)
            {
                Record rcd = list[i] as Record;
                int index = unsortedArray.IndexOf(rcd);

                // If the rcd is no longer in the unsorteArray then remove it here and
                // adjust the index and count appropriately and continue
                if (index < 0)
                {
                    list.RemoveAt(i);
                    i--;
                    count--;
                    continue;
                }

                if (index == unsortedIndex)
                {
                    // since this is the matching rcd then save its position in the array so we can return it
                    matchingIndex = i;
                }
                else
                    if (index > unsortedIndex)
                    {
                        // bump the passed in counter that keeps tarck of the number of 
                        // rcds whose unsorted index was greater than the pass in index
                        rcdsWithHigherIndices++;
                    }
            }

            return matchingIndex;
        }

                #endregion //GetListIndexFromUnsortedIndex	
    
				#region OnFirstListenerAddingInternal

		// SSP 2/3/09 - NAS9.1 Record Filtering
		// 
		private void OnFirstListenerAddingInternal( )
		{
			// ViewableRecordCollection uses this to verify itself before anyone hooks into it so 
			// it doesn't raise notifications after it's hooked into.
			// 
			this.Verify( );

            // JJD 6/1/09 - TFS18108 
            // For expandable field collecions we need to verify the order of the rcds 
            // in case they add/remove or modify the order of expandablefields
            ExpandableFieldRecordCollection expandableFieldRcds = this.RecordCollection as ExpandableFieldRecordCollection;

            if (expandableFieldRcds != null)
                expandableFieldRcds.VerifyChildren();
        }

				#endregion // OnFirstListenerAddingInternal

                // JJD 12/29/08 - added
                #region OnRecordFilterCollectionVersionChanged

        private void OnRecordFilterCollectionVersionChanged()
        {
            this.EnsureFiltersEvaluated();
        }

                #endregion //OnRecordFilterCollectionVersionChanged	

                // JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
                #region RemoveFromFixedList

        private void RemoveFromFixedList(Record record)
        {
            int index = this._fixedNonSpecialRecordsOnTop == null
                ? -1
                : this._fixedNonSpecialRecordsOnTop.IndexOf(record);

            if (index >= 0)
            {
                this._fixedNonSpecialRecordsOnTop.RemoveAt(index);
                return;
            }

            if (this._fixedNonSpecialRecordsOnBottom != null)
            {
                index = this._fixedNonSpecialRecordsOnBottom.IndexOf(record);

                if (index >= 0)
                {
                    this._fixedNonSpecialRecordsOnBottom.RemoveAt(index);
                    return;
                }
            }
        }

                #endregion //RemoveRecordFromFixedList	
		
				// JJD 12/2/10 - TFS25227 - added
				#region RemoveFromRcdsInsertedAtTop

		private void RemoveFromRcdsInsertedAtTop(IList<Record> rcdsToRemove)
		{
			if (this._recordsInsertedAtTop == null ||
				this._recordsInsertedAtTop.Count == 0 ||
				rcdsToRemove == null ||
				rcdsToRemove.Count == 0)
			{
				return;
			}

			foreach (Record rcd in rcdsToRemove)
			{
				DataRecord dr = rcd as DataRecord;

				if (dr != null)
					this._recordsInsertedAtTop.Remove(dr);
			}
		}

				#endregion //RemoveFromRcdsInsertedAtTop	
       
				#region VerifyCachedRecordStatus

		private void VerifyCachedRecordStatus(Record record)
		{
			this.Verify( );

			//// JJD 4/14/07
			//// use VisibilityResolved instead
			////if (record.Visibility == Visibility.Collapsed)
			//if (record.VisibilityResolved == Visibility.Collapsed)
			//{
			//    this.DeleteRecordFromCache(record);
			//    return;
			//}

			//bool isFixed	= record.IsFixed;
			//bool isSpecial	= record.IsSpecialRecord;

			//if (isFixed || isSpecial)
			//{
			//    bool onTop = record.IsOnTopWhenFixed;

			//    IList<Record> cachedList = this.GetCachedList(isSpecial, onTop);

			//    if (!cachedList.Contains(record))
			//    {
			//        if (isSpecial)
			//            this.InsertSpecialRecordInCache(record, cachedList, onTop);
			//        else
			//        {
			//            // we always want the last fixed record to be closest to
			//            // the scrollable records so on top we append it to the
			//            // end and on bottom we insert it at index 0
			//            if (onTop)
			//                cachedList.Add(record);
			//            else
			//                cachedList.Insert(0, record);
			//        }
			//    }
			//}
			//else
			//{
			//    this.DeleteRecordFromCache(record);
			//}
		}

				#endregion //VerifyCachedRecordStatus	

				// JJD 11/22/11 - TFS96310 - added
				#region VerifyFixedRecordLimit

		// JJD 11/22/11 - TFS96310 
		// Ensure that the fixed record limt has not been exceeded
		private void VerifyFixedRecordLimit(bool top)
		{
			List<Record> fixedList = top ? _fixedNonSpecialRecordsOnTop : _fixedNonSpecialRecordsOnBottom;

			// if we have less that 2 fixed records then we can'r have exceeded the limit 
			// so just return
			if (fixedList == null || fixedList.Count < 2)
				return;

			int limit = fixedList[0].FieldLayout.FixedRecordLimitResolved;

			// if there is no limit or we haven't exceeded it yet then return
			if (limit < 1 || limit >= fixedList.Count)
				return;

			Queue<Record> rcdsToUnFix = new Queue<Record>();

			// remove enough records to staisfy the specified limit 
			while (limit < fixedList.Count)
			{
				int indexToRemove = top ? 0 : fixedList.Count - 1;

				Record rcdToUnfix = fixedList[indexToRemove];

				// we don't want to unfix the rcd while inside the loop since that may
				// possibly affect the list so enqueue the rcd instead
				rcdsToUnFix.Enqueue(rcdToUnfix);

				fixedList.RemoveAt(indexToRemove);
			}

			// unfix all rcds that were bumped above
			while (rcdsToUnFix != null && rcdsToUnFix.Count > 0)
				rcdsToUnFix.Dequeue().FixedLocation = FixedRecordLocation.Scrollable;
		}

				#endregion //VerifyFixedRecordLimit	
    		
				// JJD 11/22/11 - TFS96310 - added
				#region VerifyFixedRecordList

		// JJD 11/22/11 - TFS96310
		// Added VerifyFixedRecordLists which will make sure the lists of fixed records that the
		// vrc maintains pick up any records that were fixed from the group islands.
		private void VerifyFixedRecordList(bool top)
		{
			List<Record> fixedList = top ? _fixedNonSpecialRecordsOnTop : _fixedNonSpecialRecordsOnBottom;

			int count = _recordCollection.Count;
			int si = top ? 0 : count - 1;
			int step = top ? 1 : -1;

			// loop over the first/last records looking for fixed records that are not
			// yet in the appropriate fixed non-special record list and add them if they aren't
			for (int i = si; i >= 0 && i < count; i += step)
			{
				Record record = _recordCollection.SparseArray.GetItem(i, false);

				// check if the record is fixed and break out of the loop
				// when we encounter a rcd that is either not fixed or is fixed
				// at the other end of the record collection
				if (record != null &&
					record.FixedLocation != FixedRecordLocation.Scrollable &&
					record.IsOnTopWhenFixed == top)
				{
					if (fixedList == null)
					{
						if (top)
							fixedList = _fixedNonSpecialRecordsOnTop = new List<Record>();
						else
							fixedList = _fixedNonSpecialRecordsOnBottom = new List<Record>();
					}

					// if the record is not in the list then append it or insert it at
					// the beginiing of the list based on the 'top' parameter
					if (false == fixedList.Contains(record))
					{
						if (top)
							fixedList.Add(record);
						else
							fixedList.Insert(0, record);
					}

				}
				else
					break;
			}

			// Ensure that the fixed record limt has not been exceeded
			this.VerifyFixedRecordLimit(top);
		}

				#endregion //VerifyFixedRecordList	
    
				#region VerifyHasAnyListeners

		
		
		private void VerifyHasAnyListeners( )
		{
			if ( this.HasAnyListeners )
			{
				if ( null == _specialRecordsVersionTracker && null != this.FieldLayout )
					_specialRecordsVersionTracker = new PropertyValueTracker( this.FieldLayout, FieldLayout.SpecialRecordsVersionProperty, this.VerifySpecialRecords );

				// SSP 12/15/08 - NAS9.1 Record Filtering
				// 
				if ( null == _filtersVersionTracker )
				{
					RecordManager rm = this.RecordManager;
					ResolvedRecordFilterCollection filters = null != rm ? rm.RecordFiltersResolved : null;
					if ( null != filters )
					{
						_filtersVersionTracker = new PropertyValueTracker( filters,
							ResolvedRecordFilterCollection.VersionProperty, this.OnRecordFilterCollectionVersionChanged, true );

						// When typing in a filter cell with as-you-type filtering, Background seems to work better.
						// 
						_filtersVersionTracker.AsynchronousDispatcherPriority = System.Windows.Threading.DispatcherPriority.Background;
					}
				}

                // JJD 6/1/09 - TFS18108 
                // For expandable field collecions we need to track the Fields  version number
                // in case they add/remove or modify the order of expandablefields
                ExpandableFieldRecordCollection expandableFieldRcds = this.RecordCollection as ExpandableFieldRecordCollection;

                if (expandableFieldRcds != null)
                {
                    FieldLayout fl = expandableFieldRcds.FieldLayout;

                    if (this._fieldsVersionTracker == null && fl != null)
                        this._fieldsVersionTracker = new PropertyValueTracker(fl.Fields, "Version", expandableFieldRcds.VerifyChildren);
                }
			}
			else
			{
				_specialRecordsVersionTracker = null;

				// SSP 12/15/08 - NAS9.1 Record Filtering
				// 
				_filtersVersionTracker = null;

                // JJD 6/1/09 - TFS18108 
                // clear the fields version tracker
                this._fieldsVersionTracker = null;
			}
        }

				#endregion // VerifyHasAnyListeners

				#region VerifySpecialRecords





		private void VerifySpecialRecords( )
		{

			if ( !this.IsSpecialRecordsDirty )
				return;

			if ( _inVerifySpecialRecords )
				return;

            // JJD 5/23/08 - BR33317 
            // Don't do verifications while rm is processing a reset
            RecordManager rm = this.RecordManager;
            if (rm == null ||
                 rm.IsInReset)
                return;

            // JJD 8/27/09 - TFS21513
            // Check constant that let's us know that the fixed record order needs to be verified
            bool fixedRecordOrderDirty = FixedRecordOrderIsDirty == _verifiedSpecialRecordsVersion;

            bool wasBeginUpdateCalled = false;

			_inVerifySpecialRecords = true;
			FieldLayout fieldLayout = this.FieldLayout;
			Debug.Assert( null != fieldLayout );
			if ( null != fieldLayout )
				_verifiedSpecialRecordsVersion = fieldLayout.SpecialRecordsVersion;





			try
			{

                // JJD 8/27/09 - TFS21513
                // Check if fixed record order needs to be verified
                #region Verify fixed record positions

                if (fixedRecordOrderDirty )
                {
                    // first clear the old fixed records lists
                    this._fixedNonSpecialRecordsOnTop = null;
                    this._fixedNonSpecialRecordsOnBottom = null;

                    if (_recordCollection.Count > 0)
                    {
                        RecordSparseArrayBase sparseArray = _recordCollection.SparseArray;

                        Record[] records = sparseArray.ToArray(true);

                        // call MoveFixedRecords which will return true if any fixed records
                        // were encountered and therefore re-positioned in the array
                        if (GridUtilities.MoveFixedRecords(records))
                        {
                            // set a flag so we know the call EndUpdate in the finally block
                            wasBeginUpdateCalled = true;

                            // call BeginUpdate to prevent notifications from being raised
                            // until we call EndUpdate below
                            _recordCollection.BeginUpdate();

                            // clear and re-add the records in the updated order
                            sparseArray.Clear();
                            sparseArray.AddRange(records);

                            // populate the _fixedNonSpecialRecordsOnTop and _fixedNonSpecialRecordsOnBottom lists
                            this._fixedNonSpecialRecordsOnTop = GetFixedRecordsList(records, true);
                            this._fixedNonSpecialRecordsOnBottom = GetFixedRecordsList(records, false);

							// JJD 11/22/11 - TFS96310 
							// Ensure that the fixed record limt has not been exceeded
							this.VerifyFixedRecordLimit(true);
							this.VerifyFixedRecordLimit(false);
						}
						
                    }

					// JJD 12/2/10 - TFS25227 
					// Make sure there are no fixed rcds that are also in the _recordsInsertedAtTop.
					// This could cause the index mapping logic from unsorted to sorted (and vice versa)
					// to give erroneous results
					this.RemoveFromRcdsInsertedAtTop(_fixedNonSpecialRecordsOnTop);
					this.RemoveFromRcdsInsertedAtTop(_fixedNonSpecialRecordsOnBottom);
                }

                #endregion //Verify fixed record positions	
    
				// Store the old ones for determining whether they have changed so we can raise
				// reset notification at the end.
				// 
				List<Record> oldSpecialRecordsOnTop = _specialRecordsOnTop;
				List<Record> oldSpecialRecordsOnBottom = _specialRecordsOnBottom;
				List<Record> oldFixedRecordsOnTop = _fixedRecordsOnTop;
				List<Record> oldFixedRecordsOnBottom = _fixedRecordsOnBottom;

				// Reset FixedRecordLocationOverride on any previously set records.
				// 
				CachedInfo.ResetCachedSettings( oldSpecialRecordsOnTop );
				CachedInfo.ResetCachedSettings( oldSpecialRecordsOnBottom );
                CachedInfo.ResetCachedSettings( oldFixedRecordsOnTop );
                CachedInfo.ResetCachedSettings( oldFixedRecordsOnBottom );
                // JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
                // Reset the cached lists
				CachedInfo.ResetCachedSettings( this._fixedNonSpecialRecordsOnTop );
				CachedInfo.ResetCachedSettings( this._fixedNonSpecialRecordsOnBottom );

				// Recycle old special records rather than creating new ones.
				// 
				Record[] recordsToRecycle = GridUtilities.Aggregate( _specialRecordsOnTop, _specialRecordsOnBottom );

				// Create structure that caches various information about this vrc so it doesn't
				// have to reget it multiple times.
				// 
				CachedInfo info = new CachedInfo( this, recordsToRecycle );

				// Allocate new lists.
				// 
				_specialRecordsOnTop = new List<Record>( );
				_specialRecordsOnBottom = new List<Record>( );
				_fixedRecordsOnTop = new List<Record>( );
				_fixedRecordsOnBottom = new List<Record>( );

                // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing
                // keep a flag so we know whether we need to send a Reset notification below
                bool recordPositionsHaveChanged = false;

                // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing
                // Reslot any pending unfixed records into their correct positions
                if (!info.ProcessPendingUnfixNonSpecialRecords(this._pendingUnfixedNonSpecialRecords))
                    recordPositionsHaveChanged = true;
                
                this._pendingUnfixedNonSpecialRecords = null;

                // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing
                // Verify that top fixed non-special records are in the proper order in the sparse array
                if (!info.VerifyFixedNonSpecialRecords(this._fixedNonSpecialRecordsOnTop, true))
                    recordPositionsHaveChanged = true;

                // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing
                // Verify that top fixed non-special records are in the proper order in the sparse array
                if (!info.VerifyFixedNonSpecialRecords(this._fixedNonSpecialRecordsOnBottom, false))
                    recordPositionsHaveChanged = true;

				// Get the top and bottom special records.
				// 
				info.GetSpecialRecordsHelper( _specialRecordsOnTop, true );
				info.GetSpecialRecordsHelper( _specialRecordsOnBottom, false );

				// Get fixed non-special records (data or group-by records).
				// 
				List<Record> fixedNonSpecialRecordsOnTop = new List<Record>( );
				List<Record> fixedNonSpecialRecordsOnBottom = new List<Record>( );
				info.GetFixedNonSpecialRecords( fixedNonSpecialRecordsOnTop, true );
				info.GetFixedNonSpecialRecords( fixedNonSpecialRecordsOnBottom, false );

				// If there are fixed non-special records then all special records have to
				// fixed since special records always occur before non-special records in 
				// visible order.
				// 
				this.FixInterveningNonFixedRecords( true, _specialRecordsOnTop, fixedNonSpecialRecordsOnTop );
				this.FixInterveningNonFixedRecords( false, _specialRecordsOnBottom, fixedNonSpecialRecordsOnBottom );

				// Now populate the _fixedRecordsOnTop and _fixedRecordsOnBottom lists.
				// 
				this.AddFixedRecordsHelper( _specialRecordsOnTop, _fixedRecordsOnTop );

                // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing
                if (info._isRecordFixingAllowed)
                {
                    this.AddFixedRecordsHelper(fixedNonSpecialRecordsOnTop, _fixedRecordsOnTop);
                    this.AddFixedRecordsHelper(fixedNonSpecialRecordsOnBottom, _fixedRecordsOnBottom);
                }

				this.AddFixedRecordsHelper( _specialRecordsOnBottom, _fixedRecordsOnBottom );

				// SSP 10/25/11 TFS91364
				// When using label icons filter ui type, if all records of a child collection are filtered out,
				// we need to make sure we show at least the field labels so the user can unfilter the records.
				// The below logic adds a header record if there are no other visible records, and there are 
				// records.
				// 
				// ------------------------------------------------------------------------------------------------
				if (info._addHeaderRecord && 0 == _specialRecordsOnTop.Count + _specialRecordsOnBottom.Count)
				{
					HeaderRecord hr = (HeaderRecord)info.ExtractRecord(info._recordsToRecycle, typeof(HeaderRecord));
					if (null == hr)
					{
						hr = new HeaderRecord(info._fieldLayout, info._recordColl)
						{
							_vrcPlaceholderRecord = new TemplateDataRecord(info._fieldLayout, info._recordColl)
						};

						// JJD 11/11/11 - TFS91364
						// Create a dummy TemplateDataRecord to provide enough context to allow
						// the filter logic to work in both flat and nested grid views
						// This is necessary to show the header to allow the user to change/reset the filter criteria
						hr.AttachedToRecord = hr._vrcPlaceholderRecord;
					}

					_specialRecordsOnTop.Add(hr);
				}
				// ------------------------------------------------------------------------------------------------

                // JJD 8/27/09 - TFS21513
                // Only check whether we need to send a reset notification if we didn't call
                // BeginUpdate above since a reset will be sent when we then call EndUpdate 
                // below.
                if (!wasBeginUpdateCalled)
                {
                    // Compare to see if special and fixed records have changed. If so then raise Reset
                    // notification.
                    // 
                    // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing
                    // Only compare the lists if we think we may be able to avoid sending the reset
                    // notification
                    //bool specialRecordsChanged =
                    if (!recordPositionsHaveChanged)
                    {
                        recordPositionsHaveChanged = !GridUtilities.CompareLists(oldSpecialRecordsOnTop, _specialRecordsOnTop)
                            || !GridUtilities.CompareLists(oldSpecialRecordsOnBottom, _specialRecordsOnBottom);
                    }

                    // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing
                    // Only compare the lists if we think we may be able to avoid sending the reset
                    // notification
                    //bool fixedRecordsChanged =
                    if (!recordPositionsHaveChanged)
                    {
                        recordPositionsHaveChanged = !GridUtilities.CompareLists(oldFixedRecordsOnTop, _fixedRecordsOnTop)
                            || !GridUtilities.CompareLists(oldFixedRecordsOnBottom, _fixedRecordsOnBottom);
                    }

                    // JJD 6/18/09 - NA 2009 Vol 2 - Record Fixing
                    // use the new recordPositionsHaveChanged flag to determine if we need to send
                    // a Reset notification out
                    //if ( specialRecordsChanged || fixedRecordsChanged )
                    if (recordPositionsHaveChanged)
                        this.RaiseChangeEvents(true);
                }
			}
			finally
			{
                // JJD 8/27/09 - TFS21513
                // if we called begin update we need to call endupdate
                if (wasBeginUpdateCalled)
                {
                    this.RaiseChangeEvents(true);
                    _recordCollection.EndUpdate(false);
                }

				_inVerifySpecialRecords = false;
			}
		}

				#endregion // VerifySpecialRecords
    
			#endregion //Private Methods

		#endregion //Methods

		#region INotifyCollectionChanged Members

		
		
		
		private event NotifyCollectionChangedEventHandler _collectionChanged;

		/// <summary>
		/// Occurs when a changes are made to the colleciton, i.e. records are added, removed or the entire collection is reset.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add
			{
				if ( ! this.HasAnyListeners )
					this.OnFirstListenerAddingInternal( );

				_collectionChanged = Delegate.Combine( _collectionChanged, value ) as NotifyCollectionChangedEventHandler;
				this.VerifyHasAnyListeners( );
			}
			remove
			{
				_collectionChanged = System.Delegate.Remove( _collectionChanged, value ) as NotifyCollectionChangedEventHandler;
				this.VerifyHasAnyListeners( );
			}
		}

		#endregion

		#region IList Members

		int IList.Add(object value)
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		void IList.Clear()
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		bool IList.Contains(object value)
		{
			return ((IList)this).IndexOf(value) >= 0;
		}

		int IList.IndexOf(object value)
		{
			if (value is Record)
				return this.GetIndexOf(value as Record, true);

			return -1;
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		object IList.this[int index]
		{
			get
			{
				this.Verify();

				return this.GetItem(index);
			}
			set
			{
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
			}
		}

		#endregion

		#region ICollection Members

        /// <summary>
        /// Copies all the records in the collection into an array
        /// </summary>
		/// <param name="array">The target array to receive the records.</param>
		/// <param name="arrayIndex">The index in the target array to receive the first record.</param>
		public void CopyTo(Array array, int arrayIndex)
		{
			// calling Count will perform a verification first
			int count = this.Count;

			
			for (int i = 0; i < count; i++)
            {
                // JJD 0/17/08
                // Use i in the call to GetItem.
                // Otherwise, we will place the 1st record in every slot
                //array.SetValue(this.GetItem(arrayIndex), arrayIndex + i);
                array.SetValue(this.GetItem(i), arrayIndex + i);
            }
 		}

		/// <summary>
		/// The total number of records in the collection (read-only).
		/// </summary>
		public int Count
		{
			get 
			{
				this.Verify();

				// JJD 4/14/07
				// Moved logic into CountNoVerify property
				int count = this.CountNoVerify;

				return count;
			}
		}

		private int CountNoVerify
		{
			get
			{
				int count = this.RecordCollectionSparseArray.VisibleCount;

				if (this._specialRecordsOnTop != null)
					count += this._specialRecordsOnTop.Count;

				if (this._specialRecordsOnBottom != null)
					count += this._specialRecordsOnBottom.Count;

				return count;
			}
		}


		bool ICollection.IsSynchronized
		{
			get { return ((IList)(this._recordCollection)).IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return ((IList)(this._recordCollection)).SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			this.Verify();

			return new VisualRecordEnumerator(this);
		}

		#endregion

		#region IList<Record> Members

		/// <summary>
		/// Gets the zero based index of a specific record
		/// </summary>
		/// <param name="record">The record in question.</param>
		/// <returns>The zero based index of the record or -1 if the record is not in the collection.</returns>
		public int IndexOf(Record record)
		{
			return ((IList)this).IndexOf(record);
		}

		void IList<Record>.Insert(int index, Record item)
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		void IList<Record>.RemoveAt(int index)
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		/// <summary>
		/// Gets the record at the specified index (read-only)
		/// </summary>
		/// <param name="index">The zero based index of the desired record.</param>
		/// <returns>The <see cref="Record"/> at the specified index.</returns>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		public Record this[int index]
		{
			get
			{
				return ((IList)this)[index] as Record;
			}
			set
			{
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
			}
		}

		#endregion

		#region ICollection<Record> Members

		void ICollection<Record>.Add(Record item)
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		void ICollection<Record>.Clear()
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		bool ICollection<Record>.IsReadOnly
		{
			get { return true; }
		}

		/// <summary>
		/// Returns true if the record is in the collection
		/// </summary>
		public bool Contains(Record item)
		{
			return ((IList)this).Contains(item);
		}

		/// <summary>
		/// Copies all the records in the collection into an array
		/// </summary>
		/// <param name="array">The target array to receive the records.</param>
		/// <param name="arrayIndex">The index in the target array to receive the first record.</param>
		public void CopyTo(Record[] array, int arrayIndex)
		{
			this.CopyTo((Array)array, arrayIndex);
		}

		bool ICollection<Record>.Remove(Record item)
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		#endregion

		#region IEnumerable<Record> Members

		IEnumerator<Record> IEnumerable<Record>.GetEnumerator()
		{
			this.Verify();

			return new VisualRecordEnumerator(this);
		}

		#endregion

		#region VisualRecordEnumerator private class

		private class VisualRecordEnumerator : IEnumerator, IEnumerator<Record>
		{
			private ViewableRecordCollection _collection;
			private int _currentPosition;
			private object _currentItem;

			static object UnsetObjectMarker = new object();

			internal VisualRecordEnumerator(ViewableRecordCollection collection)
			{
				this._collection = collection;
				this._currentPosition = -1;
				this._currentItem = UnsetObjectMarker;
			}

			public void Dispose()
			{
				this.Reset();
			}

			#region IEnumerator Members

			public bool MoveNext()
			{
				int count = this._collection.Count;

				if (this._currentPosition < count - 1)
				{
					this._currentPosition++;
					this._currentItem = this._collection.GetItem(this._currentPosition);
					return true;
				}

				this._currentPosition = count;
				this._currentItem = UnsetObjectMarker;
				return false;
			}

			public void Reset()
			{
				this._currentPosition = -1;
				this._currentItem = UnsetObjectMarker;
			}

			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			#endregion

			#region IEnumerator<Record> Members

			public Record Current
			{
				get
				{
					if (this._currentItem == UnsetObjectMarker)
					{
						if (this._currentPosition == -1)
							throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_25" ) );
						else
							throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_26" ) );
					}

					return this._currentItem as Record;
				}
			}

			#endregion
		}

		#endregion //VisualRecordEnumerator
	}

	#endregion // ViewableRecordCollection Class

	#region Old ViewableRecordCollection Commented Out

	
#region Infragistics Source Cleanup (Region)


























































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


	#endregion // Old ViewableRecordCollection Commented Out
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