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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Xml;
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
using Infragistics.Collections;




namespace Infragistics.Windows.DataPresenter
{


	#region FieldDragManager Class

	/// <summary>
	/// Class with the logic for starting and managing dragging and dropping of a field.
	/// </summary>
	internal class FieldDragManager
	{
		#region Nested Data Structures

		#region DropInfo Class

		/// <summary>
		/// Contains information about a specific drop. It contains info on the new layout.
		/// </summary>
		private class DropInfo
		{
			#region Nested Data Structures

			#region MeetsCriteria_IntersectingItems Class

			private class MeetsCriteria_IntersectingItems : GridUtilities.IMeetsCriteria
			{
				private int _col, _row, _colSpan, _rowSpan;
				private bool _fullyContainedItemsOnly;

				public MeetsCriteria_IntersectingItems( int col, int row, int colSpan, int rowSpan, bool fullyContainedItemsOnly )
				{
					_col = col;
					_row = row;
					_colSpan = colSpan;
					_rowSpan = rowSpan;
					_fullyContainedItemsOnly = fullyContainedItemsOnly;
				}

				public bool MeetsCriteria( object itemObj )
				{
					ItemLayoutInfo item = (ItemLayoutInfo)itemObj;

					if ( _fullyContainedItemsOnly )
						return item.Column >= _col && item.Column + item.ColumnSpan <= _col + _colSpan
								&& item.Row >= _row && item.Row + item.RowSpan <= _row + _rowSpan;
					else
						return LayoutInfo.Intersects( _col, _colSpan, item.Column, item.ColumnSpan )
								&& LayoutInfo.Intersects( _row, _rowSpan, item.Row, item.RowSpan );
				}
			}

			#endregion // MeetsCriteria_IntersectingItems Class

			#endregion // Nested Data Structures

			#region Member Vars

			private FieldDragManager _dragManager;
			private LayoutContainer _layoutContainer;
			private LayoutItem _dropTargetItem;
			private DropLocation _dropLocation;
			private RectInfo _dropTargetRect;
			private ItemLayoutInfo _newDragItemLayoutInfo;

			private LayoutInfo _origLayoutInfo, _newLayoutInfo;

			private bool _isProcessed;
			private bool _isDropValid;
			private bool _isDropNOOP;
			private bool _isDragAreaInvalid;

			// JM 04-17-09 - CrossBand grouping feature
			private GroupByAreaMultiDropType	_groupByAreaMultiDropType;
			private int							_groupByAreaMultiInsertAtIndex;

			// SSP 6/26/09 - NAS9.2 Field Chooser
			// 
			internal FieldChooserDropType _fieldChooserDropType = FieldChooserDropType.None;

			#endregion // Member Vars

			#region Constructor

			internal DropInfo( LayoutContainer layoutContainer, DropLocation dropLocation, LayoutItem dropTargetItem )
			{
				if ( DropLocation.AboveTarget != dropLocation && DropLocation.BelowTarget != dropLocation
					&& DropLocation.LeftOfTarget != dropLocation && DropLocation.RightOfTarget != dropLocation )
				{
					Debug.Assert( false );
					throw new ArgumentException( );
				}

				GridUtilities.ValidateNotNull( layoutContainer );
				GridUtilities.ValidateNotNull( dropTargetItem );

				this.Initialize( layoutContainer.DragManager, layoutContainer, dropLocation, dropTargetItem, dropTargetItem.Rect );

				ItemLayoutInfo dropTargetItemInfo = _dragManager.GetItemLayoutInfo( _dropTargetItem.Field );
				_newDragItemLayoutInfo = null != dropTargetItemInfo 
					? this.GetNewDragLayoutInfo( dropTargetItemInfo, _dropLocation ) : null;
			}

			internal DropInfo( LayoutContainer layoutContainer, int newRow, int newColumn )
			{
				GridUtilities.ValidateNotNull( layoutContainer );

				FieldDragManager dm = layoutContainer.DragManager;
				FrameworkElement dragElem = dm._dragElement;

				ItemLayoutInfo newDragItemLayoutInfo = dm._dragItemLayoutInfo.Clone( );
				newDragItemLayoutInfo.Row = newRow;
				newDragItemLayoutInfo.Column = newColumn;

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                // We're storing the fixed location on the item so we need to 
                // get the resulting fixed location from the container.
                //
                newDragItemLayoutInfo._fixedLocation = layoutContainer.GetFixedLocation(newRow, newColumn);

				// SSP 7/2/09 - NAS9.2 Field Chooser
				// When a hidden field from a field chooser is dropped, we need to unhide it.
				// 
				newDragItemLayoutInfo.Visibility = Visibility.Visible;

				RectInfo dropTargetRect = layoutContainer.GetRect(
						newDragItemLayoutInfo.Row,
						newDragItemLayoutInfo.Column,
						newDragItemLayoutInfo.RowSpan,
						newDragItemLayoutInfo.ColumnSpan,
						null != dragElem
							? new Size( dragElem.ActualWidth, dragElem.ActualHeight )
							: Size.Empty
					);

				this.Initialize( layoutContainer.DragManager, layoutContainer, DropLocation.OverTarget, null, dropTargetRect );

                _newDragItemLayoutInfo = newDragItemLayoutInfo;
			}

            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
            // This overload is used for the splitter drop handling.
            //
            internal DropInfo(LayoutContainer layoutContainer, DropLocation dropLocation, RectInfo rect, int newRow, int newCol, FixedFieldLocation fixedLocation)
            {
                FieldDragManager dm = layoutContainer.DragManager;
                
                this.Initialize(dm, layoutContainer, dropLocation, null, rect);

                ItemLayoutInfo newDragItemLayoutInfo = dm._dragItemLayoutInfo.Clone();
                newDragItemLayoutInfo.Row = newRow;
                newDragItemLayoutInfo.Column = newCol;
                newDragItemLayoutInfo._fixedLocation = fixedLocation;

                _newDragItemLayoutInfo = newDragItemLayoutInfo;
            }

			/// <summary>
			/// Constructs an invalid drop info. Used for figuring out the drag indicator location.
			/// </summary>
			/// <param name="dragManager"></param>
			/// <param name="layoutContainer"></param>
			/// <param name="isDragAreaInvalid"></param>
			internal DropInfo( FieldDragManager dragManager, LayoutContainer layoutContainer, bool isDragAreaInvalid )
			{
				_isDragAreaInvalid = isDragAreaInvalid;
				this.Initialize( dragManager, layoutContainer, DropLocation.None, null, null );
			}

			// JM 04-17-09 - CrossBand grouping feature
			// This overload is used for the GroupByAreaMulti drop handling.
			internal DropInfo(FieldDragManager dragManager, GroupByAreaMultiDropType groupByAreaMultiDropType, RectInfo targetRect, DropLocation dropLocation, int groupByAreaMultiInsertAtIndex, bool isDropNOOP)
			{
				_dragManager					= dragManager;
				_groupByAreaMultiDropType		= groupByAreaMultiDropType;
				_dropTargetRect					= targetRect;
				_dropLocation					= dropLocation;
				_groupByAreaMultiInsertAtIndex	= groupByAreaMultiInsertAtIndex;
				_isDropNOOP						= isDropNOOP;
			}

			// SSP 6/26/09 - NAS9.2 Field Chooser
			// 
			/// <summary>
			/// This constructor is used when the user drags a field from the data presenter and drops it either
			/// outside of the data presenter or over a field chooser to hide the field and it's also used when
			/// the user drags a field from the field chooser and drops it over the data presenter to show the
			/// field. Note that when the field is dropped over a field-layout area, (for example in relation 
			/// to another field or over an empty logical cell of the field-layout), then this constructor will
			/// note be used. In that case the relative drop-location drop-type is used.
			/// </summary>
			/// <param name="dragManager">Drag manager.</param>
			/// <param name="fieldChooserDropType"></param>
			internal DropInfo( FieldDragManager dragManager, FieldChooserDropType fieldChooserDropType )
			{
				_dragManager = dragManager;
				_fieldChooserDropType = fieldChooserDropType;

				if ( FieldChooserDropType.None != fieldChooserDropType )
				{
					ItemLayoutInfo newDragItemLayoutInfo = dragManager._dragItemLayoutInfo.Clone( );
					newDragItemLayoutInfo.Visibility = FieldChooserDropType.HideField == fieldChooserDropType
						? Visibility.Collapsed : Visibility.Visible;

					_newDragItemLayoutInfo = newDragItemLayoutInfo;
				}
			}

			#endregion // Constructor

			#region Methods

			#region Private/Internal Methods

			#region ApplyDrop

			internal bool ApplyDrop( )
			{
				this.EnsureProcessed( );

				// JM 04-17-09 - CrossBand grouping feature
				// SSP 6/29/09 - NAS9.2 Field Chooser
				// Refactored the code that was here into the new ApplyDrop_ProcessGroupBy helper method
				// and added code to ungroup when dropping a field from group-by area onto the data 
				// presenter.
				// 
				// --------------------------------------------------------------------------------------
				//return this.ApplyDrop_ProcessGroupBy( _groupByAreaMultiDropType, _groupByAreaMultiInsertAtIndex );
				GroupByAreaMultiDropType groupByDropType = _groupByAreaMultiDropType;

				if ( GroupByAreaMultiDropType.None == groupByDropType 
					&& _dragManager._isDraggingFromGroupByArea && _dragManager._dragField.AllowGroupByResolved )
					groupByDropType = GroupByAreaMultiDropType.Ungrouping;

				if ( GroupByAreaMultiDropType.None != groupByDropType )
				{
					if ( !this.ApplyDrop_ProcessGroupBy( groupByDropType, _groupByAreaMultiInsertAtIndex ) )
						return false;
				}
				// --------------------------------------------------------------------------------------

				//Debug.Assert( null != _newLayoutInfo );

				if ( null != _newLayoutInfo )
				{
					DataPresenterBase dp = _dragManager._dataPresenter;

                    // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                    // We need to pass in the appropriate change reason based on whether 
                    // we are repositioning within a fixed area or not.
                    //
                    ItemLayoutInfo oldItem = _dragManager._dragItemLayoutInfo;
                    ItemLayoutInfo newItem = _newLayoutInfo[_dragManager._dragField];
					FieldPositionChangeReason changeReason = FieldPositionChangeReason.Moved;
                    FixedFieldLocation fixedLocation = _dragManager._dragField.FixedLocation;

                    if (newItem != null && newItem._fixedLocation != oldItem._fixedLocation)
                    {
                        fixedLocation = newItem._fixedLocation;

                        if (newItem._fixedLocation == FixedFieldLocation.Scrollable)
                            changeReason = FieldPositionChangeReason.Unfixed;
                        else
                            changeReason = FieldPositionChangeReason.Fixed;
                    }

					// SSP 6/29/09 - NAS9.2 Field Chooser
					// 
					
					if ( null != oldItem && null != newItem && oldItem.Visibility != newItem.Visibility )
					{
						changeReason = Visibility.Visible == newItem.Visibility
							? FieldPositionChangeReason.Displayed : FieldPositionChangeReason.Hidden;
					}
					

					if ( null != dp )
					{
						// Raise the before event.
                        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
						//FieldPositionChangingEventArgs eventArgs = new FieldPositionChangingEventArgs( _dragManager._dragField );
						FieldPositionChangingEventArgs eventArgs = new FieldPositionChangingEventArgs( _dragManager._dragField, changeReason );
						dp.RaiseFieldPositionChanging( eventArgs );
						if ( eventArgs.Cancel )
							return false;
					}

					// SSP 1/12/10 TFS25122
					// 
					new LayoutInfo.CollapsedItemsPositionManager( _newLayoutInfo, _dragManager._dragField, FieldChooserDropType.None != _fieldChooserDropType ).OffsetCollapsedItems( );

					// AS 6/4/09 NA 2009.2 Undo/Redo
					if (null != dp && dp.IsUndoEnabled)
					{
						FieldPositionAction action = new FieldPositionAction(
							new Field[] { _dragManager._dragField },
							// SSP 6/26/09 - NAS9.2 Field Chooser
							// We want to take the snapshot of the current field layout if 
							// _dragFieldLayoutInfo hasn't been created yet (the user hasn't 
							// moved any fields yet).
							// 
							//_dragManager._fieldLayout._dragFieldLayoutInfo, 
							_dragManager._fieldLayout.GetFieldLayoutInfo( true, true ), 
							FieldPositionAction.GetUndoReason(changeReason), 
							oldItem._fixedLocation);

						dp.History.AddUndoActionInternal(action);
					}

					// SSP 1/6/09 TFS11860
					// When a new field is added or an existing collapsed field is made visible, we need
					// to make sure it doesn't overlap with other fields. Also if the field was collapsed,
					// it should appear close to where it was when it was collapsed.
					// 
					
					
					// SSP 6/26/09 - NAS9.2 Field Chooser
					// Moved the logic into the new SetFieldLayoutInfo method on the field layout.
					// 
					_dragManager._fieldLayout.SetFieldLayoutInfo( _newLayoutInfo, true, true );
					
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

					

                    // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                    // Update the fixed location of the field based on the new fixed area.
                    //
                    // AS 3/3/09 Optimization
                    // Use a helper method so we can skip the global invalidation since we
                    // are going to explicitly validate below.
                    //
                    //_dragManager._dragField.FixedLocation = fixedLocation;
                    _dragManager._dragField.SetFixedLocation(fixedLocation);

					if ( null != dp )
					{
                        // AS 3/3/09 Optimization
                        // Only invalidate this one field layout and don't recreate the 
                        // templates.
                        //
                        //dp.InvalidateGeneratedStyles( true, false );
						// AS 7/7/09 TFS19145/Optimization
						// We shouldn't need to bump the internal version for a position change just 
						// like we don't need to when the field visibility changes.
						//
						//_dragManager._fieldLayout.InvalidateGeneratedStyles(true, false);
						_dragManager._fieldLayout.InvalidateGeneratedStyles(false, false);

						// Raise the After event.
                        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
						//dp.RaiseFieldPositionChanged( new FieldPositionChangedEventArgs( _dragManager._dragField ) );
						dp.RaiseFieldPositionChanged( new FieldPositionChangedEventArgs( _dragManager._dragField, changeReason ) );
					}

					return true;
				}

				return false;
			}

			#endregion // ApplyDrop

			#region ApplyDrop_ProcessGroupBy

			// SSP 6/29/09 - NAS9.2 Field Chooser
			// Refactored - code in this method was originally moved from ApplyDrop method below.
			// 
			private bool ApplyDrop_ProcessGroupBy( GroupByAreaMultiDropType groupByDropType, int groupByAreaMultiInsertAtIndex )
			{
				bool rtn = false;

				// AS 6/1/09 NA 2009.2 Undo/Redo
				//GroupingHelper groupingHelper = new GroupingHelper(_dragManager.DataPresenter.GroupByAreaMulti, _dragManager._fieldLayout);
				GroupingHelper groupingHelper = new GroupingHelper( _dragManager._fieldLayout );
				switch ( groupByDropType )
				{
					case GroupByAreaMultiDropType.Grouping:
						if ( groupingHelper.ProcessTryGrouping( _dragManager.DragField, groupByAreaMultiInsertAtIndex ) )
						{
							groupingHelper.ProcessApplyGrouping( _dragManager.DragField, groupByAreaMultiInsertAtIndex );
							rtn = true;
						}

						break;
					case GroupByAreaMultiDropType.Ungrouping:
						if ( groupingHelper.ProcessTryUnGroup( _dragManager.DragField ) )
						{
							groupingHelper.ProcessApplyUnGroup( _dragManager.DragField );
							rtn = true;
						}

						break;
					case GroupByAreaMultiDropType.Regrouping:
						if ( groupingHelper.ProcessTryReGroup( _dragManager.DragField, groupByAreaMultiInsertAtIndex ) )
						{
							groupingHelper.ProcessApplyReGroup( _dragManager.DragField, groupByAreaMultiInsertAtIndex );
							rtn = true;
						}

						break;
					default:
						Debug.Assert( false, "Should not be here - unexpected GroupByAreaMultiDropType" );
						break;
				}

				// JJD 5/27/09 - TFS17941
				// Raise the grouped event if the grouping event wasn't cancelled
				if ( rtn == true )
					// AS 6/1/09 NA 2009.2 Undo/Redo
					//groupingHelper.GroupByArea.RaiseGroupedEventHelper(groupingHelper.FieldLayout, groupingHelper.Groups);
					groupingHelper.RaiseGroupedEventHelper( );

				return rtn;
			}

			#endregion // ApplyDrop_ProcessGroupBy

			#region EnsureProcessed

			private void EnsureProcessed( )
			{
				if ( !_isProcessed )
				{
					this.ProcessHelper( );
					_isProcessed = true;
				}
			}

			#endregion // EnsureProcessed

			#region GetDropIndicator

			internal static DropIndicator GetDropIndicator( DragToolWindow indicatorWindow )
			{
				DropIndicator dropIndicator = (DropIndicator)Utilities.GetDescendantFromType( indicatorWindow, typeof( DropIndicator ), true );

				// SSP 10/23/09 TFS22643
				// If templates haven't been applied then drop indicator won't be in visual tree.
				// 
				if ( null == dropIndicator )
					dropIndicator = indicatorWindow.Content as DropIndicator;

				return dropIndicator;
			}

			#endregion // GetDropIndicator

			#region GetNewDragLayoutInfo

			private ItemLayoutInfo GetNewDragLayoutInfo( ItemLayoutInfo targetLayoutInfo, DropLocation dropLocation )
			{
				int newCol = targetLayoutInfo.Column;
				int newRow = targetLayoutInfo.Row;

				if ( DropLocation.RightOfTarget == dropLocation )
					newCol += targetLayoutInfo.ColumnSpan;

				if ( DropLocation.BelowTarget == dropLocation )
					newRow += targetLayoutInfo.RowSpan;

				ItemLayoutInfo newItem = new ItemLayoutInfo( newCol, newRow, 
					_dragManager._dragItemLayoutInfo.ColumnSpan, 
					_dragManager._dragItemLayoutInfo.RowSpan );

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                // Use the fixed location of the relative element.
                //
                newItem._fixedLocation = targetLayoutInfo._fixedLocation;

                return newItem;
			}

			#endregion // GetNewDragLayoutInfo

			#region HasLogicalColumnChanged

			private static bool HasLogicalColumnChanged(
				LayoutInfo origInfo,
				LayoutInfo newInfo )
			{
				bool logicalRowChanged, logicalColumnChanged;

				origInfo.HasLogicalColumnOrRowChanged( newInfo, out logicalColumnChanged, out logicalRowChanged );

				return logicalColumnChanged;
			}

			#endregion // HasLogicalColumnChanged

			#region HasLogicalRowChanged

			private static bool HasLogicalRowChanged(
				LayoutInfo origInfo,
				LayoutInfo newInfo )
			{
				bool logicalRowChanged, logicalColumnChanged;

				origInfo.HasLogicalColumnOrRowChanged( newInfo, out logicalColumnChanged, out logicalRowChanged );

				return logicalRowChanged;
			}

			#endregion // HasLogicalRowChanged

			#region Initialize

			private void Initialize( 
				FieldDragManager dragManager,
				LayoutContainer layoutContainer,
				DropLocation dropLocation,
				LayoutItem dropTargetItem,
				RectInfo dropTargetRect )
			{
				_dragManager = dragManager;
				_layoutContainer = layoutContainer;
				_dropLocation = dropLocation;
				_dropTargetItem = dropTargetItem;
				_dropTargetRect = dropTargetRect;
			}

			#endregion // Initialize

			#region InitializeDropIndicator

			// SSP 6/29/09 - NAS9.2 Field Chooser
			// Certain drop types don't show indicators, like dragging a field outside of the
			// data presenter to hide it. Changed the return type from void to bool to 
			// return a value indicating whether the drop indicator should be shown or not.
			// 
			//internal void InitializeDropIndicator( DragToolWindow indicatorWindow )
			/// <summary>
			/// Initializes the drop indicator. If the drop indicator should not be
			/// shown then returns false.
			/// </summary>
			/// <param name="indicatorWindow">Drop indicator window.</param>
			/// <returns>A value indicating whether the drop indicator should be shown.</returns>
			internal bool InitializeDropIndicator( DragToolWindow indicatorWindow )
			{
				DropIndicator dropIndicator = GetDropIndicator( indicatorWindow );

				// SSP 6/29/09 - NAS9.2 Field Chooser
				// See related change above.
				// 
				if ( null == _dropTargetRect )
					return false;					

				Rect rectInScreen = _dropTargetRect.GetRectInScreen( );

				if ( null != dropIndicator )
				{
					dropIndicator.DropLocation = _dropLocation;
					dropIndicator.DropTargetWidth = rectInScreen.Width;
					dropIndicator.DropTargetHeight = rectInScreen.Height;

					indicatorWindow.UpdateLayout( );

					UIElement elem = (UIElement)Utilities.GetDescendantFromName( indicatorWindow, "PART_Offset" );
					if ( null != elem )
					{
						Point offset = elem.TranslatePoint( new Point( 0, 0 ), indicatorWindow );
						rectInScreen.Offset( -offset.X, -offset.Y );

                        // AS 1/29/09 TFS13199
                        // We only want to shift to the right when the drop location is the physical
                        // right edge which when the flow direction is RightToLeft would only be 
                        // when the DropLocation is LeftOfTarget.
                        //
                        //if ( DropLocation.RightOfTarget == _dropLocation )
                        FlowDirection flowDirection = _dragManager.DataPresenter.FlowDirection;
						if (( DropLocation.RightOfTarget == _dropLocation && flowDirection == FlowDirection.LeftToRight) ||
                            ( DropLocation.LeftOfTarget == _dropLocation && flowDirection == FlowDirection.RightToLeft))
							rectInScreen.Offset( rectInScreen.Width, 0 );
						else if ( DropLocation.BelowTarget == _dropLocation )
							rectInScreen.Offset( 0, rectInScreen.Height );
					}
				}

                // AS 2/12/09 TFS11410
                //indicatorWindow.Left = rectInScreen.Left;
				//indicatorWindow.Top = rectInScreen.Top;
                this._dragManager.PositionToolWindow(indicatorWindow, rectInScreen.Location);

				// SSP 6/29/09 - NAS9.2 Field Chooser
				// See related change above.
				// 
				return true;
			}

			#endregion // InitializeDropIndicator

			#region IsDropValidHelper

			internal static bool IsDropValidHelper( FieldDragManager dragManager,
                // AS 1/23/09
                // Added an out parameter. In discussing this with Sandip it seemed
                // better to have the No cursor shown when over a fixed area for which 
                // the dragged item cannot be dropped rather than showing nothing. To 
                // show the no cursor the IsDragAreaInvalid must be true.
                //
				//LayoutInfo origLayoutInfo, LayoutInfo newLayoutInfo )
				LayoutInfo origLayoutInfo, LayoutInfo newLayoutInfo, ref bool isDragAreaInvalid )
			{
				Field dragField = dragManager._dragField;

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                // Ensure that the field is allowed to be put into the new fixed area.
                //
                ItemLayoutInfo origItem = dragManager._dragItemLayoutInfo;
                ItemLayoutInfo newItem = newLayoutInfo[dragField];

                if (newItem != null)
                {
                    if (newItem._fixedLocation != origItem._fixedLocation &&
                        !dragField.IsFixedLocationAllowed(newItem._fixedLocation))
                    {
                        // AS 1/23/09
                        // I moved this up since we need to initialize the isDragAreaInvalid
                        // so that the No cursor is shown when over an invalid drop area.
                        //
                        isDragAreaInvalid = true;

                        return false;
                    }
                }

				FieldLayout fieldLayout = dragManager._fieldLayout;
				AllowFieldMoving allowFieldMoving = fieldLayout.AllowFieldMovingResolved;
				switch ( allowFieldMoving )
				{
					case AllowFieldMoving.WithinLogicalColumn:
						if ( HasLogicalColumnChanged( origLayoutInfo, newLayoutInfo ) )
							return false;

						break;
					case AllowFieldMoving.WithinLogicalRow:
						if ( HasLogicalRowChanged( origLayoutInfo, newLayoutInfo ) )
							return false;

						break;
					case AllowFieldMoving.No:
						// SSP 6/26/09 - NAS9.2 Field Chooser
						// Since now we could be changing the visibility of fields, we need to
						// explicitly compare only the positions.
						// 
						//if ( ! origLayoutInfo.IsSame( newLayoutInfo ) )
						//	return false;
						if ( origLayoutInfo.HasLogicalColumnOrRowChanged( newLayoutInfo ) )
							return false;

						break;
				}


				int maxLogicalCols = fieldLayout.FieldMovingMaxColumnsResolved;
				int maxLogicalRows = fieldLayout.FieldMovingMaxRowsResolved;
				if ( maxLogicalCols > 0 || maxLogicalRows > 0 )
				{
					int newLogicalColumnCount, newLogicalRowCount, oldLogicalColumnCount, oldLogicalRowCount;
					origLayoutInfo.GetLogicalRowAndColumnCount( out oldLogicalColumnCount, out oldLogicalRowCount );
					newLayoutInfo.GetLogicalRowAndColumnCount( out newLogicalColumnCount, out newLogicalRowCount );

					// If the new logical column count is greater than the max allowed then return false.
					// Also if the new logical column count is the same as the old, then allow the drop
					// since we aren't increasing the logical column count.
					// 
					// SSP 9/19/08 TFS7443
					// MaxLogicalCols of 0 means no limit.
					// 
					//if ( newLogicalColumnCount > maxLogicalCols && newLogicalColumnCount > oldLogicalColumnCount )
					if ( maxLogicalCols > 0 && newLogicalColumnCount > maxLogicalCols && newLogicalColumnCount > oldLogicalColumnCount )
						return false;

					// Do the same with rows.
					// 
					// SSP 9/19/08 TFS7443
					// MaxLogicalRows of 0 means no limit.
					// 
					//if ( newLogicalRowCount > maxLogicalRows && newLogicalRowCount > oldLogicalRowCount )
					if ( maxLogicalRows > 0 && newLogicalRowCount > maxLogicalRows && newLogicalRowCount > oldLogicalRowCount )
						return false;
				}

				// SSP 7/2/09 - NAS9.2 Field Chooser
				// 
				// --------------------------------------------------------------------------------------------
				Debug.Assert( null != origItem && null != newItem );
				if ( null != origItem && null != newItem
					&& origItem.Visibility != newItem.Visibility )
				{
					AllowFieldHiding allowHiding = dragField.AllowHidingResolved;
					bool isMouseOverFieldChooser = null != dragManager._lastFieldChooserWithMouseOver;

					// If both the AllowHiding is set to Never then the user shouldn't be allowed to 
					// change the visibility of the field (neither hide nor show).
					// 
					if ( AllowFieldHiding.Never == allowHiding )
						return false;

					// If AllowHiding is set to ViaFieldChooserOnly then the user should not be allowed 
					// to drag a field from the data presenter and drop it outside of the data presenter 
					// to hide it. However the user can drop it over a field chooser to hide it.
					// 
					if ( Visibility.Visible != newItem.Visibility && AllowFieldHiding.ViaFieldChooserOnly == allowHiding && ! isMouseOverFieldChooser )
						return false;
				}
				// --------------------------------------------------------------------------------------------

				return true;
			}

			#endregion // IsDropValidHelper

			#region OffsetItems

			internal static void OffsetItems( IEnumerable<ItemLayoutInfo> items, int colDelta, int rowDelta )
			{
				foreach ( ItemLayoutInfo ii in items )
				{
					if ( ii.Row + rowDelta < 0 || ii.Column + colDelta < 0 )
					{
						Debug.Assert( false );
						return;
					}
				}

				foreach ( ItemLayoutInfo ii in items )
				{
					ii.Column += colDelta;
					ii.Row += rowDelta;
				}
			}

			#endregion // OffsetItems

			#region ProcessHelper

			private void ProcessHelper( )
			{
				// JM 04-17-09 - CrossBand grouping feature
				if (this._groupByAreaMultiDropType != GroupByAreaMultiDropType.None)
				{
					_isDropValid = true;

					return;
				}

				LayoutInfo origLayoutInfo = null, newLayoutInfo = null;
				bool isDropValid, isDropNOOP;

				bool canProcess = null != _newDragItemLayoutInfo;

				// AS 3/23/10 TFS29701
				// If there is an invalid field sort description that creates a group by 
				// label then we don't want to blow up when dragging it - just don't allow 
				// it to be dropped within the field or group by area but only to be removed.
				//
				if (_dragManager._dragField.Index < 0)
					canProcess = false;

				// If drop target item is from a different field layout then the drop can't be processed
				// and is considered invalid.
				// 
				if ( canProcess && null != _dropTargetItem )
				{
					Field dropTargetField = _dropTargetItem.Field;
					if ( null == dropTargetField || dropTargetField.Owner != _dragManager._fieldLayout )
						canProcess = false;
				}

				if ( canProcess )
				{
					// SSP 6/26/09 - NAS9.2 Field Chooser
					// 
					bool positionChanged = !ItemLayoutInfo.HasSamePosition( _newDragItemLayoutInfo, _dragManager._dragItemLayoutInfo );

					origLayoutInfo = _dragManager._layoutInfo.Clone( );

					// SSP 6/26/09 - NAS9.2 Field Chooser
					// Moved the call below. See below for the reason.
					// 
					//origLayoutInfo.PackLayout( );

					newLayoutInfo = origLayoutInfo.Clone( );

					newLayoutInfo[_dragManager._dragField] = _newDragItemLayoutInfo;

					AllowFieldMoving allowFieldMoving = _dragManager._fieldLayout.AllowFieldMovingResolved;

					// SSP 6/26/09 - NAS9.2 Field Chooser
					// If only changing the visibility then don't shift up or left. Simply ensure that
					// the item is not overlapping with any other item. Enclosed the existing call to 
					// ShiftUpLeftHelper into the if block.
					// 
					if ( positionChanged )
					{
						// If the item is dropped within the same logical column or row, then shift up or left
						// respectively to fill the space originally occupied by the drag item.
						// 
						this.ShiftUpLeftHelper( origLayoutInfo, newLayoutInfo );
					}

					// SSP 2/24/10 TFS28070
					// In card-view, we should shift overlapping items below since the layout is typically vertical.
					// 
					bool shiftBelowFirst;
					if ( DropLocation.AboveTarget == _dropLocation || DropLocation.BelowTarget == _dropLocation )
						shiftBelowFirst = true;
					else if ( DropLocation.LeftOfTarget == _dropLocation || DropLocation.RightOfTarget == _dropLocation )
						shiftBelowFirst = false;
					else
						shiftBelowFirst = AutoArrangeCells.TopToBottom == _dragManager._fieldLayout.AutoArrangeCellsResolved;

					newLayoutInfo.EnsureItemDoesntOverlap( _newDragItemLayoutInfo,
						AllowFieldMoving.WithinLogicalColumn != allowFieldMoving,
						AllowFieldMoving.WithinLogicalRow != allowFieldMoving,
						// SSP 2/24/10 TFS28070
						// 
						//DropLocation.AboveTarget == _dropLocation || DropLocation.BelowTarget == _dropLocation 
						shiftBelowFirst
					);

					// SSP 6/26/09 - NAS9.2 Field Chooser
					// Don't pack the layout if only changing the visibility. Enclosed the existing
					// call to PackLayout into the if block.
					// 
					if ( positionChanged )
					{
						newLayoutInfo.PackLayout( );

						// SSP 6/26/09 - NAS9.2 Field Chooser
						// Moved the call to origLayoutInfo.PackLayout( ) (commented out) from above
						// before we clone the origLayoutInfo into the newLayoutInfo. The reason for
						// the change is that the _newDragItemLayoutInfo is calculated based on the
						// original unpacked layout info. It's row/col values are relative to the
						// unpacked layout. Therefore we should process the drop and then pack the new
						// layout info. The only reason why we need to pack the origLayoutInfo is to
						// figure out if the drop is NOOP below.
						// 
						origLayoutInfo.PackLayout( );
					}

					isDropNOOP = newLayoutInfo.IsSame( origLayoutInfo );

                    newLayoutInfo.DebugFixedLocations();

                    // AS 1/23/09
                    // See the changes on the method signature for details.
                    //
					//isDropValid = IsDropValidHelper( _dragManager, origLayoutInfo, newLayoutInfo );
                    isDropValid = IsDropValidHelper(_dragManager, origLayoutInfo, newLayoutInfo, ref _isDragAreaInvalid);
                }
				else
				{
					isDropValid = false;
					isDropNOOP = false;
				}

				_origLayoutInfo = origLayoutInfo;
				_newLayoutInfo = newLayoutInfo;
				_isDropNOOP = isDropNOOP;
				_isDropValid = isDropValid;				
			}

			#endregion // ProcessHelper

			#region ShiftUpLeftHelper

			/// <summary>
			/// When an item is moved below or right in the same logical column or row, 
			/// the space previously occupied by the item will become empty. We need to
			/// shift items up or left respectively to fill that gap. This method does that.
			/// </summary>
			/// <param name="origLayoutInfo"></param>
			/// <param name="newLayoutInfo"></param>
			private void ShiftUpLeftHelper(
				LayoutInfo origLayoutInfo,
				LayoutInfo newLayoutInfo )
			{
				Field dragField = _dragManager._dragField;
				ItemLayoutInfo newDragItemLayoutInfo = newLayoutInfo[dragField];
				ItemLayoutInfo origDragItemLayoutInfo = origLayoutInfo[dragField];

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                // We do not want to be shifting items from one area into another.
                //
                Debug.Assert(null != newDragItemLayoutInfo && null != origDragItemLayoutInfo);
                if (null != newDragItemLayoutInfo && null != origDragItemLayoutInfo &&
                    newDragItemLayoutInfo._fixedLocation != origDragItemLayoutInfo._fixedLocation)
                    return;

				bool isDragItemVisible = Visibility.Visible == dragField.VisibilityResolved;
				bool didDragItemOverlap = origLayoutInfo.DoesItemOverlap( origDragItemLayoutInfo, null );
				if ( isDragItemVisible && !didDragItemOverlap )
				{
					int row = -1, col = -1, rowSpan = -1, colSpan = -1;
					int rowDelta = 0, colDelta = 0;
					bool performShift = false;

					if ( DropLocation.AboveTarget == _dropLocation || DropLocation.BelowTarget == _dropLocation )
					{
						if ( origDragItemLayoutInfo.Column == newDragItemLayoutInfo.Column
							&& origDragItemLayoutInfo.ColumnSpan == newDragItemLayoutInfo.ColumnSpan
							// This tests to see if the item is being moved down in the logical column. Otherwise
							// there is no need to shift up.
							&& origDragItemLayoutInfo.Row < newDragItemLayoutInfo.Row )
						{
							col = newDragItemLayoutInfo.Column;
							colSpan = newDragItemLayoutInfo.ColumnSpan;
							row = origDragItemLayoutInfo.Row;
							rowSpan = newDragItemLayoutInfo.Row - row;
							rowDelta = -origDragItemLayoutInfo.RowSpan;
							performShift = true;
						}
					}
					else if ( DropLocation.LeftOfTarget == _dropLocation || DropLocation.RightOfTarget == _dropLocation )
					{
						if ( origDragItemLayoutInfo.Row == newDragItemLayoutInfo.Row
							&& origDragItemLayoutInfo.RowSpan == newDragItemLayoutInfo.RowSpan
							// This tests to see if the item is being moved right in the logical row. Otherwise
							// there is no need to shift left.
							&& origDragItemLayoutInfo.Column < newDragItemLayoutInfo.Column )
						{
							row = newDragItemLayoutInfo.Row;
							rowSpan = newDragItemLayoutInfo.RowSpan;
							col = origDragItemLayoutInfo.Column;
							colSpan = newDragItemLayoutInfo.Column - col;
							colDelta = -origDragItemLayoutInfo.ColumnSpan;
							performShift = true;
						}
					}

					if ( performShift )
					{
						// We can only perform the shift operation if there are no other items that protrude
						// into the logical column or row from other logical columns or rows otherwise they
						// will end up overlapping with each other. The following code for comparing the
						// contained items with intersecting items is for that. That effectively checks for
						// that.

						HashSet intersectingItems = GridUtilities.ToSet( GridUtilities.Filter<ItemLayoutInfo>(
							newLayoutInfo.Values, new MeetsCriteria_IntersectingItems( col, row, colSpan, rowSpan, false ) ) );

						HashSet containedItems = GridUtilities.ToSet( GridUtilities.Filter<ItemLayoutInfo>(
							newLayoutInfo.Values, new MeetsCriteria_IntersectingItems( col, row, colSpan, rowSpan, true ) ) );

						if ( HashSet.AreEqual( containedItems, intersectingItems ) )
						{
							// We need to shift the drag item as well.
							containedItems.Add( newDragItemLayoutInfo );

							OffsetItems( new TypedEnumerable<ItemLayoutInfo>( containedItems ), colDelta, rowDelta );
						}
					}
				}
			}

			#endregion // ShiftUpLeftHelper

			#endregion // Private/Internal Methods

			#endregion // Methods

			#region Properties

			#region Private/Internal Properties

			// JM 05-20-09 TFS 17824 - Added
			#region GroupByAreaMultiDropType

			internal GroupByAreaMultiDropType GroupByAreaMultiDropType
			{
				get { return this._groupByAreaMultiDropType; }
			}

			#endregion //GroupByAreaMultiDropType

			#region IsDragAreaInvalid

			/// <summary>
			/// Indicates if the item is currently being dragged outside of valid drop area.
			/// For example, if the item is 
			/// </summary>
			internal bool IsDragAreaInvalid
			{
				get
				{
					return _isDragAreaInvalid;
				}
			}

			#endregion // IsDragAreaInvalid

			#region IsDropNOOP

			internal bool IsDropNOOP
			{
				get
				{
					this.EnsureProcessed( );

					return _isDropNOOP;
				}
			}

			#endregion // IsDropNOOP

			#region IsDropValid

			internal bool IsDropValid
			{
				get
				{
					this.EnsureProcessed( );

					return _isDropValid;
				}
			}

			#endregion // IsDropValid

			#endregion // Private/Internal Properties

			#endregion // Properties
		}

		#endregion // DropInfo Class

		#region LayoutContainer Class

		/// <summary>
		/// Contains information regarding layout container.
		/// </summary>
		private class LayoutContainer
		{
			#region Member Vars

            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
			//private FrameworkElement _layoutContainerElem; // DataRecordCellArea
			private VirtualizingDataRecordCellPanel _layoutContainerElem;

			private FieldDragManager _dragManager;

			private double[] _logicalColumnWidths;
			private double[] _logicalRowHeights;
			private double _offsetFromContainerX;
			private double _offsetFromContainerY;

			// SSP 10/26/10 TFS35100
			// 
            //List<VirtualCellInfo> _cells;

			#endregion // Member Vars

			#region Constructor

            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
			//internal LayoutContainer( FieldDragManager dragManager, FrameworkElement layoutContainerElem )
			internal LayoutContainer( FieldDragManager dragManager, VirtualizingDataRecordCellPanel layoutContainerElem )
			{
				GridUtilities.ValidateNotNull( dragManager );
				GridUtilities.ValidateNotNull( layoutContainerElem );

				_dragManager = dragManager;
				_layoutContainerElem = layoutContainerElem;

				this.InitializeLogicalRowColDims( );

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
				// SSP 10/26/10 TFS35100
				// Commented this out. We need to reget the cell info every time the mouse moves
				// because that could have cuased the cell panel to scroll, and thus the cell info
				// would be invalid.
				// 
                //_cells = _layoutContainerElem.GetCellInfo(false);
			}

			#endregion // Constructor

			#region Methods

			#region Private/Internal Methods

            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
            #region AdjustCellRect
            /// <summary>
            /// Helper method to  adjust a rect for a given cell row/col based on its fixed location.
            /// </summary>
            private void AdjustCellRect(int row, int col, ref Rect rect)
            {
                FixedFieldLocation location = GetFixedLocation(row, col);

                Vector offset = _layoutContainerElem.GetFixedAreaOffset(location, false);
                rect.Offset(offset);
            } 
            #endregion //AdjustCellRect

            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
            #region GetFixedLocation
            /// <summary>
            /// Calculates the fixed location for a given origin.
            /// </summary>
            internal FixedFieldLocation GetFixedLocation(int row, int col)
            {
                FieldLayout fl = _dragManager._fieldLayout;
                bool isHorz = fl.IsHorizontal;

                // the other orientation doesn't play a role
                if (isHorz)
                    col = 0;
                else
                    row = 0;

				// AS 2/19/10 TFS27943
				// Added If check. We should not even check for the fixed areas if the control doesn't support fixing.
				//
				if (fl.DataPresenter != null && fl.DataPresenter.IsFixedFieldsSupportedResolved)
				{
					// check the near area first
					FieldPosition areaPos = _layoutContainerElem.GetFixedArea(FixedFieldLocation.FixedToNearEdge);

					if (areaPos.Contains(row, col))
						return FixedFieldLocation.FixedToNearEdge;

					areaPos = _layoutContainerElem.GetFixedArea(FixedFieldLocation.FixedToFarEdge);

					if (areaPos.Contains(row, col) ||
						(isHorz && row >= areaPos.Row) ||
						(!isHorz && col >= areaPos.Column))
						return FixedFieldLocation.FixedToFarEdge;
				}

                return FixedFieldLocation.Scrollable;
            }
            #endregion //GetFixedLocation

			#region GetItemFromPoint

			internal LayoutItem GetItemFromPoint( PointInfo point )
			{
                
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

                Point pt = point.GetPosition(_layoutContainerElem);

				// SSP 10/26/10 TFS35100
				// We need to reget the cell info every time the mouse moves
				// because that could have cuased the cell panel to scroll, and thus the cell info
				// would be invalid.
				// 
				//List<VirtualCellInfo> cells = _cells;
				List<VirtualCellInfo> cells = _layoutContainerElem.GetCellInfo( false );

                for (int i = 0, count = cells.Count; i < count; i++)
                {
					VirtualCellInfo cell = cells[i];

                    if (cell.ClipRect.Contains(pt))
                    {
						if (null != cell.Element)
						{
							// AS 7/9/09 TFS19237
							// I found this while debugging TFS19237. If the cell element is hidden then it may not 
							// have any height so the insertion marks will not be correct. To get around this we'll 
							// provide the rect. Since the ElmentRect is relative to the VDRCP, we'll pass in that 
							// as the element. Since the Field was gotten from the element we need to pass it in 
							// explicitly now.
							//
							//return new LayoutItem(_dragManager, _cells[i].Element);
							return new LayoutItem(_dragManager, _layoutContainerElem, cell.ElementRect, cell.Field);
						}
                    }
                }

                return null;
			}

            #endregion // GetItemFromPoint

            #region GetLayoutAreaRect

            internal RectInfo GetLayoutAreaRect()
			{
                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                // There are now spacer columns/rows around each actual logical row/column
                // so we want the logical columns in the specified range.
                //
                //return this.GetRect( 0, 0, _logicalRowHeights.Length, _logicalColumnWidths.Length, new Size( 0, 0 ) );
                Rect rect = GetRectHelper(0, 0, _logicalRowHeights.Length, _logicalColumnWidths.Length, new Size( 0, 0 ) );
                return new RectInfo(_dragManager, rect, _layoutContainerElem);
			}

			#endregion // GetLayoutAreaRect

			#region GetLogicalCellFromPoint

			internal void GetLogicalCellFromPoint( PointInfo point, out int row, out int col )
			{
				Point p = point.GetPosition( _layoutContainerElem );

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                // We may need to massage the given relative point because the elements
                // at that point may have been shifted into view (i.e. fixed). Rather than 
                // make lots of changes in this class to worry about the origins of the 
                // row/col dims, we will fixed up the point relative to where it would be
                // if the elements the mouse is over were not fixed. I.e. if we're over 
                // the fixed near cell area then we will shift the point back to the 
                // left by the fixed near offset.
                //
                _layoutContainerElem.AdjustDragPoint(ref p);

				row = this.GetLogicalRow( p.Y );
				col = this.GetLogicalColumn( p.X );
			}

			#endregion // GetLogicalCellFromPoint

			#region GetLogicalColumn

			private int GetLogicalColumn( double x )
			{
				return GetSlotContainingPoint( _logicalColumnWidths, x - _offsetFromContainerX );
			}

			#endregion // GetLogicalColumn

			#region GetLogicalRow

			private int GetLogicalRow( double y )
			{
				return GetSlotContainingPoint( _logicalRowHeights, y - _offsetFromContainerY );
			}

			#endregion // GetLogicalRow

			#region GetRectHelper

			/// <summary>
			/// Returns the rect relative to the layout container element.
			/// </summary>
			/// <param name="row"></param>
			/// <param name="col"></param>
			/// <param name="rowSpan"></param>
			/// <param name="colSpan"></param>
			/// <param name="defSize"></param>
			/// <returns></returns>
			private Rect GetRectHelper( int row, int col, int rowSpan, int colSpan, Size defSize )
			{
				//Debug.Assert( row >= 0 && col >= 0
				//    && col + colSpan <= _logicalColumnWidths.Length
				//    && row + rowSpan <= _logicalRowHeights.Length );

				double x = GridUtilities.ArrSum( _logicalColumnWidths, 0, Math.Min( col, _logicalColumnWidths.Length ) );
				double y = GridUtilities.ArrSum( _logicalRowHeights, 0, Math.Min( row, _logicalRowHeights.Length ) );

				double width = col + colSpan <= _logicalColumnWidths.Length
					? GridUtilities.ArrSum( _logicalColumnWidths, col, colSpan )
					: defSize.Width;

				double height = row + rowSpan <= _logicalRowHeights.Length
					? GridUtilities.ArrSum( _logicalRowHeights, row, rowSpan )
					: defSize.Height;

				return new Rect( x + _offsetFromContainerX, y + _offsetFromContainerY, width, height );
			}

			#endregion // GetRectHelper

			#region GetRect

			internal RectInfo GetRect( int row, int col, int rowSpan, int colSpan, Size defSize )
			{
                // AS 1/7/09 NA 2009 Vol 1 - Fixed Fields
                // There are now spacer columns/rows around each actual logical row/column
                // so we want the logical columns in the specified range.
                //
				//Rect rect = this.GetRectHelper( row, col, rowSpan, colSpan, defSize );
				Rect rect = this.GetRectHelper( row * 2 + 1, col * 2 + 1, rowSpan * 2 - 1, colSpan * 2 - 1, defSize );

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                // The cell may be fixed near/far so we need to be able to adjust the rect.
                //
                AdjustCellRect(row, col, ref rect);

				return new RectInfo( _dragManager, rect, _layoutContainerElem );
			}

			#endregion // GetRect

			#region GetSlotContainingPoint
			private static int GetSlotContainingPoint( double[] extents, double p )
			{
                // AS 1/7/09 NA 2009 Vol 1 - Fixed Fields
                // Added helper method since we now have spacer columns/rows around
                // the actual rows/columns.
                //
                int index = GetSlotContainingPointImpl(extents, p);

                return index / 2;
            }

            private static int GetSlotContainingPointImpl(double[] extents, double p)
            {
                double c = 0;

				int i;
				for ( i = 0; i < extents.Length; i++ )
				{
					c += extents[i];

					if ( p < c )
						return i;
				}

				return i;
			}

			#endregion // GetSlotContainingPoint

			// AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
            #region GetSplitterFromPoint
            internal FixedFieldSplitter GetSplitterFromPoint(PointInfo point)
            {
                HitTestResult hr = VisualTreeHelper.HitTest(_layoutContainerElem, point.GetPosition(_layoutContainerElem));
                FixedFieldSplitter splitter = null;

                if (null != hr && null != hr.VisualHit)
                {
                    splitter = hr.VisualHit as FixedFieldSplitter;

                    if (null == splitter)
                        splitter = Utilities.GetAncestorFromType(hr.VisualHit, typeof(FixedFieldSplitter), true) as FixedFieldSplitter;
                }

                return splitter;
            }
            #endregion //GetSplitterFromPoint

			#region HasItemAtLogicalCell

			internal bool HasItemAtLogicalCell( int row, int col, bool ignoreDragItem )
			{
				LayoutInfo layoutInfo = _dragManager._layoutInfo;
				if ( null != layoutInfo )
				{
					foreach ( Field key in layoutInfo.Keys )
					{
						ItemLayoutInfo iiInfo = layoutInfo[key];

						if ( iiInfo.Column == col && iiInfo.Row == row
							&& ( ! ignoreDragItem || key != _dragManager._dragField ) )
							return true;
					}
				}

				return false;
			}

			#endregion // HasItemAtLogicalCell

			#region InitializeLogicalRowColDims

			private void InitializeLogicalRowColDims( )
			{
                #region Refactored
                
#region Infragistics Source Cleanup (Region)



















































#endregion // Infragistics Source Cleanup (Region)

                #endregion //Refactored
                List<double> rowDims = new List<double>();
                List<double> colDims = new List<double>();

                _layoutContainerElem.GetLogicalCellDimensions(rowDims, colDims);

				_logicalColumnWidths = colDims.ToArray( );
				_logicalRowHeights = rowDims.ToArray( );
				_offsetFromContainerX = _offsetFromContainerY = 0;
			}

			#endregion // InitializeLogicalRowColDims

			#endregion // Private/Internal Methods

			#endregion // Methods

			#region Properties

			#region Private/Internal Properties

			#region DragManager

			internal FieldDragManager DragManager
			{
				get
				{
					return _dragManager;
				}
			}

			#endregion // DragManager

			#endregion // Private/Internal Properties

			#endregion // Properties

        }

		#endregion // LayoutContainer Class

		#region LayoutItem Class

		/// <summary>
		/// Contains info on a layout item.
		/// </summary>
		private class LayoutItem
		{
			#region Member Vars

			private FrameworkElement _element;
			private FieldDragManager _dragManager;

			// AS 7/9/09 TFS19237
			private Rect _elementRect;
			private Field _field;

			#endregion // Member Vars

			#region Constructor

			// AS 7/9/09 TFS19237
			//internal LayoutItem( FieldDragManager dragManager, FrameworkElement element )
			internal LayoutItem( FieldDragManager dragManager, FrameworkElement element, Rect elementRect, Field field )
			{
				GridUtilities.ValidateNotNull( element );
				GridUtilities.ValidateNotNull( dragManager );

				_element = element;
				_dragManager = dragManager;

				// AS 7/9/09 TFS19237
				_elementRect = elementRect;
				_field = field;
			}

			#endregion // Constructor

			internal double ActualWidth
			{
				get
				{
					// AS 7/9/09 TFS19237
					//return _element.ActualWidth;
					return _elementRect.Width;
				}
			}

			internal double ActualHeight
			{
				get
				{
					// AS 7/9/09 TFS19237
					//return _element.ActualHeight;
					return _elementRect.Height;
				}
			}

			/// <summary>
			/// Rect of the element.
			/// </summary>
			internal RectInfo Rect
			{
				get
				{
					// AS 7/9/09 TFS19237
					//Rect r = new Rect( 0, 0, this.ActualWidth, this.ActualHeight );
					Rect r = _elementRect;

					return new RectInfo( _dragManager, r, _element );
				}
			}

			internal Field Field
			{
				get
				{
					
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

					return _field;
				}
			}
		}

		#endregion // LayoutItem Class

		#region RectInfo Class

		/// <summary>
		/// Contains rect information and the element its relative to.
		/// </summary>
		internal class RectInfo
		{
			private FieldDragManager _dragManager;
			private Rect _rect;
			private FrameworkElement _relativeTo;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="dragManager">Drag manager</param>
			/// <param name="rect">Rectangle</param>
			/// <param name="relativeTo">This is the element that the specified rectangle is relative to. If null then
			/// the rect is taken to be relative to the screen.</param>
			internal RectInfo( FieldDragManager dragManager, Rect rect, FrameworkElement relativeTo )
			{
				GridUtilities.ValidateNotNull( dragManager );

				_dragManager = dragManager;
				_rect = rect;
				_relativeTo = relativeTo;
			}

			internal Rect GetRectInScreen( )
			{
				// Pass in null which means convert to screen coordinates.
				// 
				return this.GetRect( null );
			}

			/// <summary>
			/// Gets the rect relative to data presenter.
			/// </summary>
			/// <returns>Rect relative to data presenter.</returns>
			internal Rect GetRect( )
			{
				return this.GetRect( _dragManager._dataPresenter );
			}

			/// <summary>
			/// Gets the rect relative to specified element. If null then gets the rect in screen coordinates.
			/// </summary>
			/// <param name="relativeTo"></param>
			/// <returns></returns>
			internal Rect GetRect( UIElement relativeTo )
			{
                
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

                Point topLeft = PointInfo.TranslatePointHelper(_rect.TopLeft, _relativeTo, relativeTo);
                Point bottomRight = PointInfo.TranslatePointHelper(_rect.BottomRight, _relativeTo, relativeTo);

                return Utilities.RectFromPoints(topLeft, bottomRight);
            }

			/// <summary>
			/// Checks to see if the rect contains specified point.
			/// </summary>
			/// <param name="point">Point to check.</param>
			/// <returns>True if the rect contains the point, false otherwise.</returns>
			internal bool Contains( PointInfo point )
			{
				Rect r = this.GetRect( );
				Point p = point.GetPosition( );

				return r.Contains( p );
			}
		}

		#endregion // RectInfo Class

		#region PointInfo Class

		/// <summary>
		/// Contains point info and which element its relative to.
		/// </summary>
		internal class PointInfo
		{
			private FieldDragManager _dragManager;
			private Point _point;
			private FrameworkElement _relativeTo;

			internal PointInfo( FieldDragManager dragManager, Point point, FrameworkElement relativeTo )
			{
				GridUtilities.ValidateNotNull( dragManager );

				_dragManager = dragManager;
				_point = point;
				_relativeTo = relativeTo;
			}

			/// <summary>
			/// Returns the mouse position relative to data presenter.
			/// </summary>
			/// <returns></returns>
			internal Point GetPosition( )
			{
				return this.GetPosition( _dragManager._dataPresenter );
			}

			internal Point GetPositionInScreen( )
			{
				return this.GetPosition( null );
			}

			internal Point GetPosition( UIElement relativeTo )
			{
				return TranslatePointHelper( _point, _relativeTo, relativeTo );
			}

			internal static Point TranslatePointHelper( Point p, UIElement currentRelativeTo, UIElement relativeTo )
			{
				// If relativeTo parameter is null then return rect in screen coordinates.
				// 
				if ( currentRelativeTo != relativeTo )
				{
					if ( null == relativeTo )
					{
						// SSP 5/25/11 TFS35313
						// Enclosed the existing code in try-catch because PointToScreenSafe can throw an exception
						// if the element is out of visual tree.
						// 
						try
						{
							p = Utilities.PointToScreenSafe( currentRelativeTo, p );
						}
						catch
						{
							p = new Point( double.MinValue, double.MinValue );
						}
					}
					else
					{
						if ( null != currentRelativeTo )
							p = currentRelativeTo.TranslatePoint( p, relativeTo );
						else // Convert from screen to element.
							p = Utilities.PointFromScreenSafe( relativeTo, p );
					}
				}

				return p;
			}

			internal bool IsInsideDataPresenter( )
			{
				return GridUtilities.DoesElementContainPoint( _dragManager._dataPresenter, this.GetPosition( ) );
			}

			#region IsMouseOverFieldChooser

			// SSP 6/29/09 - NAS9.2 Field Chooser
			// 

			internal bool IsMouseOverFieldChooser( )
			{
				FieldChooser fc;
				return this.IsMouseOverFieldChooser( out fc );
			}

			internal bool IsMouseOverFieldChooser( out FieldChooser fieldChooser )
			{
				DataPresenterBase dataPresenter = _dragManager._dataPresenter;

				WeakList<FieldChooser> registeredFieldChoosers = dataPresenter._registeredFieldChoosers;
				if ( null != registeredFieldChoosers )
				{
					foreach ( FieldChooser ii in registeredFieldChoosers )
					{
						// SSP 7/6/09 TFS19070
						// For some reason if the control is not visible, the HitTest still returns
						// a result. Enclosed the existing code in the if block.
						// 
						if ( ii.IsVisible )
						{
							HitTestResult hr = VisualTreeHelper.HitTest( ii, this.GetPosition( ii ) );
							if ( null != hr && null != hr.VisualHit )
							{
								FieldChooser tmp = (FieldChooser)Utilities.GetAncestorFromType( hr.VisualHit, typeof( FieldChooser ), true );
								if ( tmp == ii )
								{
									fieldChooser = tmp;
									return true;
								}
							}
						}
					}
				}

				fieldChooser = null;
				return false;
			}

			#endregion // IsMouseOverFieldChooser

			// SSP 7/2/09 - NAS9.2 Field Chooser
			// Moved this from the FieldDragManager to here since this class already has IsInsideDataPresenter.
			// 
			// JM 04-17-09 - CrossBand grouping feature
			#region IsMouseOverGroupByAreaMulti
			internal bool IsMouseOverGroupByAreaMulti( )
			{
				DataPresenterBase dataPresenter = _dragManager._dataPresenter;

				// SSP 7/6/09 TFS19070
				// For some reason if the control is not visible, the HitTest still returns
				// a result.
				// 
				//if ( dataPresenter.GroupByAreaMulti == null )
				GroupByAreaMulti groupByAreaMulti = dataPresenter.GroupByAreaMulti;
				if ( groupByAreaMulti == null || ! groupByAreaMulti.IsVisible )
					return false;

				HitTestResult hr = VisualTreeHelper.HitTest( groupByAreaMulti, this.GetPosition( groupByAreaMulti ) );
				GroupByAreaMulti gbam = null;

				if ( null != hr && null != hr.VisualHit )
				{
					gbam = hr.VisualHit as GroupByAreaMulti;
					if ( null == gbam )
						gbam = Utilities.GetAncestorFromType( hr.VisualHit, typeof( GroupByAreaMulti ), true ) as GroupByAreaMulti;
				}

				return gbam == groupByAreaMulti;
			}
			#endregion //IsMouseOverGroupByAreaMulti
		}

		#endregion // PointInfo Class

		#endregion // Nested Data Structures

		#region Member Vars

		private DataPresenterBase _dataPresenter;
		private Field _dragField;
		private ItemLayoutInfo _dragItemLayoutInfo;
		private FieldLayout _fieldLayout;
		private FrameworkElement _dragElement;

		private DragToolWindow _dragIndicatorWindow;
		private Point _dragIndicatorWindow_OffsetFromMouse;
		private DragToolWindow _dropIndicatorWindow;

		private LayoutInfo _layoutInfo;

		private bool _dropIndicatorShown;
		private bool _dragIndicatorShown;

		private LayoutContainer _layoutContainerToUse;
		private DispatcherTimer _scrollTimer;

		// JM 04-17-09 - CrossBand grouping feature
		private bool				_isGroupByMultiEnabled;

		// SSP 5/20/09 TFS17816
		// ISelectionHost.OnDragEnd doesn't get a mouse event args passed into it so
		// the event args that was passed into the last ISelectionHost.OnDragMove needs
		// to be stored so we can use that in ISelectionHost.OnDragEnd to pass it along
		// into the call to the drag manager's OnDragEnd.
		// 
		internal MouseEventArgs _selectionHost_LastDragMoveEventArgs;

		// SSP 6/24/09 - NAS9.2 Field Chooser
		// If a field is being dragged from the field chooser, this member will point 
		// to that field chooser.
		// 
		private FieldChooser _sourceFieldChooser;
		private bool _isDraggingFromFieldChooser;
		private bool _isDraggingFromGroupByArea;

		// _lastFieldChooserWithMouseOver is used to manage FieldChooser's
		// IsDragItemOver property.
		// 
		private FieldChooser _lastFieldChooserWithMouseOver;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		// SSP 6/24/09 - NAS9.2 Field Chooser
		// Added sourceFieldChooser parameter. Specified if the field is being dragged from a field chooser.
		// 
		//internal FieldDragManager( Field dragField, FrameworkElement dragElement )
		internal FieldDragManager( Field dragField, FrameworkElement dragElement, FieldChooser sourceFieldChooser )
		{
			GridUtilities.ValidateNotNull( dragField );

			_dragField = dragField;
			_fieldLayout = _dragField.Owner;
			_dataPresenter = dragField.DataPresenter;
			_dragElement = dragElement;

			Debug.Assert( null != _fieldLayout && null != _dataPresenter );

			// SSP 6/29/09 - NAS9.2 Field Chooser
			// Added sourceFieldChooser parameter. Also added _isDraggingFromGroupByArea flag.
			// 
			_sourceFieldChooser = sourceFieldChooser;
			_isDraggingFromFieldChooser = null != sourceFieldChooser;
			_isDraggingFromGroupByArea = dragElement is LabelPresenter && ( (LabelPresenter)dragElement ).IsInGroupByArea;
			Debug.Assert( !( _isDraggingFromFieldChooser && _isDraggingFromGroupByArea ), "Label can only come from one place." );

			// SSP 8/21/09 - TFS19187, TFS19273
			// By default field's grid position is initialized to 0, 0. This causes fields
			// to overlap and the resultant layout will contain those overlapped fields. We
			// need to ensure the auto-generation logic has initialized grid positions on
			// the fields before we proceed to create the layout information below.
			// 
			if ( null == _fieldLayout.GetFieldLayoutInfo( true, false ) )
				_fieldLayout.VerifyStyleGeneratorTemplates( );

			_layoutInfo = new LayoutInfo( _fieldLayout );

			foreach ( Field ii in _fieldLayout.Fields )
			{
				bool visible = ii.IsVisibleInCellArea;
				if ( ! visible && _dragField != ii )
					continue;

				// SSP 6/29/09 - NAS9.2 Field Chooser
				// 
				// --------------------------------------------------------------------
				FieldPosition gridPos = ii.ActualPosition;

				ItemLayoutInfo info = new ItemLayoutInfo(
					gridPos.Column,
					gridPos.Row,
					gridPos.ColumnSpan,
					gridPos.RowSpan );

				info.Visibility = ii.VisibilityInCellArea;
				
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

				// --------------------------------------------------------------------

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                info._fixedLocation = ii.FixedLocation;

				_layoutInfo.Add( ii, info );

				if ( _dragField == ii )
					_dragItemLayoutInfo = info;
			}

			Debug.Assert(null != _dragItemLayoutInfo || (_isDraggingFromGroupByArea && _dragField.Index < 0));

			if ( null == _dragItemLayoutInfo )
            {
				_dragItemLayoutInfo = new ItemLayoutInfo( 0, 0, 1, 1 );

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                _dragItemLayoutInfo._fixedLocation = dragField.FixedLocation;
            }

			// JM 04-17-09 - CrossBand grouping feature
			_isGroupByMultiEnabled = 
				(_dataPresenter != null && (_dataPresenter.GroupByAreaMode == GroupByAreaMode.MultipleFieldLayoutsCompact) ||
										   (_dataPresenter.GroupByAreaMode == GroupByAreaMode.MultipleFieldLayoutsFull)) ? true : false;
		}

		#endregion // Constructor

		#region Properties

		#region Private/Internal Properties

		#region DataPresenter

		/// <summary>
		/// Returns the associated data presenter.
		/// </summary>
		internal DataPresenterBase DataPresenter
		{
			get
			{
				return _dataPresenter;
			}
		}

		#endregion // DataPresenter

		#region DragField

		/// <summary>
		/// Returns the field being dragged.
		/// </summary>
		internal Field DragField
		{
			get
			{
				return _dragField;
			}
		}

		#endregion // DragField
		
		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		// JM 04-09 CrossBandGrouping feature.
		#region AdjustGroupByAreaMultiInsertAtIndex

		private int AdjustGroupByAreaMultiInsertAtIndex(int insertAtIndex, GroupByAreaMultiDropType dropType, FieldSortDescriptionCollection sortedFields)
		{
			if (dropType == GroupByAreaMultiDropType.Regrouping)
			{
				int indexOfFieldBeingDragged = sortedFields.IndexOf(this._dragField);
				if (indexOfFieldBeingDragged < insertAtIndex)
					insertAtIndex--;
			}
			
			return insertAtIndex;
		}

		#endregion //AdjustGroupByAreaMultiInsertAtIndex

		#region AdjustMouseLocationHelper

		private LayoutContainer AdjustMouseLocationHelper( ref PointInfo mouseLoc, out Point dragIndicatorLocation )
		{
			// If AllowFieldMoving is WithinLogicalRow/WithinLogicalColumn, since we are moving the drag
			// indicator within its logical row/col, we need to also make sure the drag item is allowed to
			// be dropped in the same record as it was dragged from. So always use the same layout container
			// once the drag operation starts.
			// 
			LayoutContainer layoutContainer = null != _layoutContainerToUse ? _layoutContainerToUse
				: this.GetLayoutContainerFromPoint( mouseLoc );

			dragIndicatorLocation = mouseLoc.GetPositionInScreen( );
			dragIndicatorLocation.Offset( -_dragIndicatorWindow_OffsetFromMouse.X, -_dragIndicatorWindow_OffsetFromMouse.Y );

			// SSP 6/29/09 - NAS9.2 Field Chooser
			// 
			bool isMouseOverLayoutContainer = null != layoutContainer && layoutContainer.GetLayoutAreaRect( ).Contains( mouseLoc );
			bool isMouseOverGroupByArea = mouseLoc.IsMouseOverGroupByAreaMulti( );
			bool isMouseOverFieldChooser = mouseLoc.IsMouseOverFieldChooser( );
			bool isMouseOverDataPresenter = mouseLoc.IsInsideDataPresenter( );

			// If field moving is within logical row or column then move the drag indicator
			// within the logical row/column. This also means that allow dragging and dropping
			// within the same layout container.
			// 
			AllowFieldMoving allowFieldMoving = _fieldLayout.AllowFieldMovingResolved;
			bool stayWithinLogicalRow = AllowFieldMoving.WithinLogicalRow == allowFieldMoving;
			bool stayWithinLogicalColumn = AllowFieldMoving.WithinLogicalColumn == allowFieldMoving;

			// SSP 6/29/09 - NAS9.2 Field Chooser & TFS17884
			// When the mouse is moved to the group-by area or to a field chooser then the drag indicator
			// shouldn't be locked into the logical row/column in the layout area anymore since the drop
			// onto the group-by area and the field chooser is valid.
			// 
			// --------------------------------------------------------------------------------------------
			if ( stayWithinLogicalRow || stayWithinLogicalColumn )
			{
				// If dragging the field from a field chooser or group-by area then the drag indicator should
				// not be forced to stay in the logical row/column in the field layout area.
				// 
				bool dontStayWithinLogicalRowColOverride = _isDraggingFromFieldChooser || _isDraggingFromGroupByArea;

				// If the mouse is dragged over the group-by area to group by a field then the drag indicator
				// should snap out of the logical row/column and move freely withing the group-by area since
				// it's valid to drop the field over the group-by area to group records by it.
				// 
				if ( isMouseOverGroupByArea && _dragField.AllowGroupByResolved )
					dontStayWithinLogicalRowColOverride = true;

				// Likewise if the mouse is dragged over a field chooser then we shouldn't lock the drag 
				// indicator in the logical row/column in the field layout area since it's valid to drop
				// the field over the field chooser to hide it.
				// 
				if ( isMouseOverFieldChooser && ( AllowFieldHiding.Never != _dragField.AllowHidingResolved ) )
					dontStayWithinLogicalRowColOverride = true;

				// Likewise if the mouse is dragged outside of the data presenter for the purposes of
				// hiding the field then the drag indicator should snap out of being locked inside the
				// logical row/column in the field layout area.
				// 
				if ( !isMouseOverDataPresenter && AllowFieldHiding.Always == _dragField.AllowHidingResolved )
					dontStayWithinLogicalRowColOverride = true;

				if ( dontStayWithinLogicalRowColOverride )
				{
					stayWithinLogicalColumn = stayWithinLogicalRow = false;

					// SSP 7/6/09 TFS17884
					// Since we aren't forcing the drag field to be within the logical row/column
					// anymore, don't return the layout container if the mouse is not over it. When
					// forcing the drag field to be within logical row/column, we use the same 
					// container from which the drag started for the purposes of keeping the drag
					// field in the same logical row/column.
					// 
					if ( ! isMouseOverLayoutContainer )
						return null;
				}
			}
			// --------------------------------------------------------------------------------------------

			if ( stayWithinLogicalRow || stayWithinLogicalColumn )
			{
				// Setting the member variable will cause us to use only that container from now
				// on as a potential drop target.
				// 
				_layoutContainerToUse = layoutContainer;

				RectInfo logicalCellRectInfo = null != layoutContainer
                    ? layoutContainer.GetRect( _dragItemLayoutInfo.Row, _dragItemLayoutInfo.Column, 1, 1, new Size( 20, 20 ) )
                    : null;

				if ( null != logicalCellRectInfo )
				{
					Rect logicalCellRect = logicalCellRectInfo.GetRect( );
					Point adjustedMouseLoc = mouseLoc.GetPosition( );
					RectInfo layoutAreaRectInfo = layoutContainer.GetLayoutAreaRect( );
					Rect layoutAreaRect = layoutAreaRectInfo.GetRect( );
					Rect layoutAreaRectInScreen = layoutAreaRectInfo.GetRectInScreen( );

					if ( stayWithinLogicalRow )
					{
						// Drag target is based on the X location of the mouse. The Y location is assumed to be within
						// the logical row of the item. Therefore adjust the Y location of the mouseLoc, which is what
						// will get used to find the drop target, to be in the logical row.
						// 
						adjustedMouseLoc.Y = ( logicalCellRect.Y + logicalCellRect.Bottom ) / 2;

						// The drag indicator should also be maintained inside the logical row. Therefore adjust
						// its Y as well.
						// 
						dragIndicatorLocation.Y = logicalCellRectInfo.GetRectInScreen( ).Y;

						// If the mouse is moved beyond the layout area then find drop target as if the
						// mouse were just left of the layout area or just right of the layout area.
						// If this causes a problem, take it out as it's only for ui pruposes.
						// 
						if ( adjustedMouseLoc.X < layoutAreaRect.X )
							adjustedMouseLoc.X = layoutAreaRect.X + 1;
						else if ( adjustedMouseLoc.X > layoutAreaRect.Right )
							adjustedMouseLoc.X = layoutAreaRect.Right - 1;

						// If the user drags the mouse way beyond the layout area then don't drag the drag
						// indicator with the mouse. Stop it just before or after the layout area. Again
						// if this causes a probelm, take it out as it's only for ui purposes.
						// 
						// SSP 8/13/09 TFS20642
						// Check for _dragIndicatorWindow being null. It can be null since now we call this
						// method before creating the drag indicator. Enclosed the existing code in the
						// if block.
						// 
						if ( null != _dragIndicatorWindow )
						{
							double dragIndicatorExtent = _dragIndicatorWindow.ActualWidth;
							if ( dragIndicatorLocation.X + dragIndicatorExtent < layoutAreaRectInScreen.X )
								dragIndicatorLocation.X = layoutAreaRectInScreen.X - dragIndicatorExtent;
							else if ( dragIndicatorLocation.X > layoutAreaRectInScreen.Right )
								dragIndicatorLocation.X = layoutAreaRectInScreen.Right;
						}
					}
					else if ( stayWithinLogicalColumn )
					{
						adjustedMouseLoc.X = ( logicalCellRect.X + logicalCellRect.Right ) / 2;

						dragIndicatorLocation.X = logicalCellRectInfo.GetRectInScreen( ).X;

						if ( adjustedMouseLoc.Y < layoutAreaRect.Y )
							adjustedMouseLoc.Y = layoutAreaRect.Y + 1;
						else if ( adjustedMouseLoc.Y > layoutAreaRect.Bottom )
							adjustedMouseLoc.Y = layoutAreaRect.Bottom - 1;

						// SSP 8/13/09 TFS20642
						// Check for _dragIndicatorWindow being null. It can be null since now we call this
						// method before creating the drag indicator. Enclosed the existing code in the
						// if block.
						// 
						if ( null != _dragIndicatorWindow )
						{
							double dragIndicatorExtent = _dragIndicatorWindow.ActualHeight;
							if ( dragIndicatorLocation.Y + dragIndicatorExtent < layoutAreaRectInScreen.Y )
								dragIndicatorLocation.Y = layoutAreaRectInScreen.Y - dragIndicatorExtent;
							else if ( dragIndicatorLocation.Y > layoutAreaRectInScreen.Bottom )
								dragIndicatorLocation.Y = layoutAreaRectInScreen.Bottom;
						}
					}

					mouseLoc = new PointInfo( this, adjustedMouseLoc, _dataPresenter );
				}
			}
			// If cells and labels are togather in records, then when a field is dragged, only
			// allow it to be dropped within the same record. This is because the drag indicator 
			// contains cell along with the label and the cell has the cell value in it.
			// 
			else if ( _fieldLayout.UseCellPresenters )
			{
				// Setting the member variable will cause us to use only that container from now
				// on as a potential drop target.
				// 
				_layoutContainerToUse = layoutContainer;

				// SSP 6/29/09 - NAS9.2 Field Chooser - Optimization
				// This is just an optimization. Now we are calculating whether the mouse is over the layout
				// container above so reuse the same info here.
				// 
				//if ( null != layoutContainer && !layoutContainer.GetLayoutAreaRect( ).Contains( mouseLoc ) )
				if ( ! isMouseOverLayoutContainer )
					return null;
			}

			return layoutContainer;
		}

		#endregion // AdjustMouseLocationHelper

		#region CalcScrollSpeed

		private ScrollSpeed CalcScrollSpeed( double distance )
		{
			double range = 200;

			int minScrollSpeed = (int)ScrollSpeed.Slowest;
			int maxScrollSpeed = (int)ScrollSpeed.Fastest;

			int scrollSpeed = Math.Max( minScrollSpeed,
				Math.Min( maxScrollSpeed, (int)( maxScrollSpeed * ( distance / range ) ) ) );

			return (ScrollSpeed)scrollSpeed;
		}

		#endregion // CalcScrollSpeed

		#region ChangeFieldVisibilityViaFieldChooser

		// SSP 6/30/09 - NAS9.2 Field Chooser
		// 
		/// <summary>
		/// Used by the FieldChooser to change the visibility of a field when the checkbox next to a field
		/// is checked/unchecked in the field chooser. The logic for raising events as well as managing
		/// the undo/redo for the action is already part of the DropInfo and therefore the field chooser
		/// is utilizing the same logic.
		/// </summary>
		/// <param name="field">Field whose visibility is being changed in the field chooser.</param>
		/// <param name="newVisibility">New visibility of the field.</param>
		internal static void ChangeFieldVisibilityViaFieldChooser( Field field, bool newVisibility )
		{
			FieldDragManager dm = new FieldDragManager( field, null, null );

			FieldChooserDropType dropType = newVisibility ? FieldChooserDropType.ShowField : FieldChooserDropType.HideField;

			DropInfo dropInfo = new DropInfo( dm, dropType );

			dropInfo.ApplyDrop( );
		}

		#endregion // ChangeFieldVisibilityViaFieldChooser

		// JM 04-09 CrossBandGrouping feature.
		#region CompareLabelPresenters

		private static int CompareLabelPresenters(LabelPresenter lp1, LabelPresenter lp2)
		{
			GroupByAreaMulti groupByAreaMulti = Utilities.GetAncestorFromType(lp1, typeof(GroupByAreaMulti), false) as GroupByAreaMulti;
			if (groupByAreaMulti == null)
				return 0;

			Point p1 = lp1.TranslatePoint(new Point(0, 0), groupByAreaMulti);
			Point p2 = lp2.TranslatePoint(new Point(0, 0), groupByAreaMulti);

			return p1.X < p2.X ? -1 : (p1.X == p2.X ? 0 : 1);
		}

		#endregion //CompareLabelPresenters

		#region DragHelper

		private void DragHelper( PointInfo mouseLoc, bool apply )
		{
			// If the mouse is outside the grid then we need to scroll.
			// 
			// SSP 2/4/09 TFS11475
			// Don't perform any scrolling when the drag operation is being ended.
			// 
			//this.ManagerScrollTimerHelper( );
			if ( ! apply )
				// SSP 7/2/09 - NAS9.2 Field Chooser
				// Pass along mouseLoc so we don't have to reget it.
				// 
				//this.ManagerScrollTimerHelper( );
				this.ManagerScrollTimerHelper( mouseLoc );

			// JM 07-07-09 - Move this code here from below the following 'if statement
			// so we can pass along the dragIndicatorLocation to the ShowDragIndicator Method.  
			Point dragIndicatorLocation;
			DropInfo dropInfo = this.DragHelper_GetDropInfoHelper(mouseLoc, out dragIndicatorLocation);

			if ( !_dragIndicatorShown )
			{
				// Set the offset of the cursor within the drag element.
				// 
				_dragIndicatorWindow_OffsetFromMouse = null != _dragElement
					? GridUtilities.EnsureInElementBounds( mouseLoc.GetPosition( _dragElement ), _dragElement )
					: new Point( 0, 0 );

				// JM 07-07-09 - Reset the dragIndicatorLocation now that we have the _dragIndicatorWindow_OffsetFromMouse
				dropInfo = this.DragHelper_GetDropInfoHelper(mouseLoc, out dragIndicatorLocation);

				// JM 07-07-09 - Pass the drag indicator location to the ShowDragIndicator method so it can position 
				// the drag indicator before showing it.
				//this.ShowDragIndicator();
				this.ShowDragIndicator(dragIndicatorLocation);
			}

			// JM 07-07-09 - Move this code above the previous 'if' statement
			//Point dragIndicatorLocation;
			//DropInfo dropInfo = this.DragHelper_GetDropInfoHelper( mouseLoc, out dragIndicatorLocation );

			// Move the drag indicator window to new drop location.
			// 
			if ( null != _dragIndicatorWindow )
			{
                // AS 2/12/09 TFS11410
                //_dragIndicatorWindow.Left = dragIndicatorLocation.X;
				//_dragIndicatorWindow.Top = dragIndicatorLocation.Y;
                this.PositionToolWindow(_dragIndicatorWindow, dragIndicatorLocation);
			}

			bool isDropNoop = null != dropInfo && dropInfo.IsDropNOOP;
			bool isDropValid = null != dropInfo && dropInfo.IsDropValid;
			bool isDragAreaInvalid = null != dropInfo && dropInfo.IsDragAreaInvalid;

			Debug.WriteLine( string.Format( "Drop Info = {0} {1} {2}",
				null == dropInfo ? "null" : "",
				isDropNoop ? "NOOP" : "",
				null != dropInfo && !isDropValid ? "Invalid" : "Valid" ) );


			// JM 06-02-09 TFS 17989
			if (apply && this._dataPresenter.GroupByAreaMulti != null)
				this._dataPresenter.GroupByAreaMulti.SetValue(GroupByAreaBase.FieldLabelDragInProgressPropertyKey, KnownBoxes.FalseBox);


			// If the item is dragged outside of a valid drop area (outside 
			// of the layout container), then display appropriate cursor.
			// 
			if ( isDragAreaInvalid )
				Mouse.OverrideCursor = Cursors.No;
			else
			// JM 05-20-09 TFS 17824 - Show a 'valid drop' cursor if we are ungrouping.
			if (dropInfo.GroupByAreaMultiDropType == GroupByAreaMultiDropType.Ungrouping && isDropValid)
				Mouse.OverrideCursor = Utilities.LoadCursor(typeof(GroupByAreaMulti), "Cursors/Ungroup.cur") ?? Cursors.UpArrow;
			// SSP 7/2/09 - NAS9.2 Field Chooser
			// When the user drags a field outside of the data presenter to hide it, show a cursor to indicate 
			// that. Note that when the mouse is dragged over a field chooser we shouldn't show the cursor.
			// 
			else if ( isDropValid && ! isDropNoop && FieldChooserDropType.HideField == dropInfo._fieldChooserDropType 
				&& ! mouseLoc.IsMouseOverFieldChooser( ) )
				// SSP 8/31/09
				// 
				//Mouse.OverrideCursor = Cursors.Cross;
				Mouse.OverrideCursor = Utilities.LoadCursor( typeof( DataPresenterBase ), "Cursors/HideField.cur" ) ?? Cursors.UpArrow;
			else
				Mouse.OverrideCursor = null;

			// JM 06-03-09 TFS18144 - Also hide the drop indicator if we are ungrouping and not applying.
			//if ( null == dropInfo || isDropNoop || !isDropValid || isDragAreaInvalid )
			if (null == dropInfo || isDropNoop || !isDropValid || isDragAreaInvalid ||
				(dropInfo.GroupByAreaMultiDropType == GroupByAreaMultiDropType.Ungrouping && isDropValid && false == apply))
			{
				this.HideDropIndicator( );
				return;
			}

			if ( !isDropNoop && isDropValid )
			{
				// JM 07-07-09 TFS18144 - Don't show the drop indicator if we are ungrouping in the GroupByAreaMulti.
				if (dropInfo.GroupByAreaMultiDropType != GroupByAreaMultiDropType.Ungrouping)
					this.ShowDropIndicator(dropInfo);

				if ( apply )
				{
					dropInfo.ApplyDrop( );
				}
			}
		}

		#endregion // DragHelper

		#region DragHelper_GetDropInfoHelper

		// SSP 7/2/09 - NAS9.2 Field Chooser
		// Refactored the code in the DragHelper_GetDropInfoHelper method. It's mostly the same
		// code except the field chooser related logic was added to it.
		// 
		
		/// <summary>
		/// Gets the drop info and also displays/moves the drag indicator.
		/// </summary>
		/// <param name="mouseLoc"></param>
		/// <param name="dragIndicatorLocation"></param>
		/// <returns></returns>
		private DropInfo DragHelper_GetDropInfoHelper( PointInfo mouseLoc, out Point dragIndicatorLocation )
		{
			// This method will keep the drag header within the logical row or column if the
			// AllowFieldMoving is WithinLogicalRow or WithinLogicalColumn.
			// 
			LayoutContainer layoutContainer = this.AdjustMouseLocationHelper( ref mouseLoc, out dragIndicatorLocation );

			// SSP 6/29/09 - NAS9.2 Field Chooser
			// 
			// ------------------------------------------------------------------------------------------
			// Set/clear appropriate FieldChooser's IsDragItemOver property.
			// 
			this.ManageFieldChooserIsDragItemOver( mouseLoc );

			DropInfo returnValue = null;

			bool isMouseOverFieldChooser = mouseLoc.IsMouseOverFieldChooser( );
			bool isMouseOverGroupByAreaMulti = ! isMouseOverFieldChooser && mouseLoc.IsMouseOverGroupByAreaMulti( );
			bool isMouseOverDataPresenter = ! isMouseOverFieldChooser && mouseLoc.IsInsideDataPresenter( );

			// If a group-by field from the group-by area is dragged and its not allowed to be grouped/ungrouped
			// then return a drop info that represents invalid drop area as the field can't be dropped anywhere.
			// 
            // JJD 8/19/09 - NA 2009 Vol 2 - Cross Band grouping
            
            //if ( _isDraggingFromGroupByArea && !_dragField.AllowGroupByResolved )
			if ( _isDraggingFromGroupByArea && !_dragField.AllowUnGrouping )
			{
				return new DropInfo( this, null, true );
			}

			if ( ( isMouseOverFieldChooser || ! isMouseOverDataPresenter ) 
				// If dragging a field from group-by area and dropping it outside of the data presenter,
				// the logic below for the group-by drag-n-drop will take care of ungrouping. Here we 
				// are simply concerned about hiding a field by dragging it outside of the data presenter.
				// 
				&& ! _isDraggingFromGroupByArea
				&& ! _isDraggingFromFieldChooser )
			{
				returnValue = new DropInfo( this, FieldChooserDropType.HideField );
			}
			// ------------------------------------------------------------------------------------------
			

			if ( ( null == returnValue || ! returnValue.IsDropValid ) && null != layoutContainer && isMouseOverDataPresenter )
			{
				// AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
				// If the mouse is over the field splitter then we need separate logic
				// to calculate the drop info.
				//
				FixedFieldSplitter splitter = layoutContainer.GetSplitterFromPoint( mouseLoc );

				if ( null != splitter )
					returnValue = this.GetSplitterDropInfo( layoutContainer, mouseLoc, splitter );
			}

			if ( ( null == returnValue || ! returnValue.IsDropValid ) && null != layoutContainer && isMouseOverDataPresenter )
			{
				LayoutItem targetItem = layoutContainer.GetItemFromPoint( mouseLoc );
				if ( null != targetItem )
				{
					DropLocation dropLoc = this.GetRelativeDropLocation( targetItem.Rect.GetRect( ), mouseLoc.GetPosition( ) );
					//Debug.Assert( DropLocation.None != dropLoc, "If mouse is in item then drop location should be one of left, right, above, below." );
					if ( DropLocation.None != dropLoc )
						returnValue = new DropInfo( layoutContainer, dropLoc, targetItem );
				}
				else
				{
					RectInfo dropRect = null;
					int row, col;
					layoutContainer.GetLogicalCellFromPoint( mouseLoc, out row, out col );
					if ( row >= 0 && col >= 0 && !layoutContainer.HasItemAtLogicalCell( row, col, true ) )
					{
						dropRect = layoutContainer.GetRect( row, col,
								_dragItemLayoutInfo.ColumnSpan, _dragItemLayoutInfo.RowSpan,
								null != _dragElement ? new Size( _dragElement.ActualWidth, _dragElement.ActualHeight ) : Size.Empty
							);
					}

					if ( null != dropRect && !dropRect.GetRect( ).IsEmpty )
						returnValue = new DropInfo( layoutContainer, row, col );
				}
			}

			if ( null == returnValue || ! returnValue.IsDropValid )
			{
				// JM 04-17-09 - CrossBand grouping feature
				// If the mouse is over the GroupByArea and the GroupByAreaMulti is being used and it is not expanded,
				// expand it now and return an appropriate DropInfo.
				// JM 6-01-09 TFS17948 - Move the check for AllowGroupByResolved several lines down where we are checking to see
				//						 if we should set the dropType to Grouping or Regrouping.
				// JM 5-11-09 TFS17520
				//if (_isGroupByMultiEnabled)
				//if (_isGroupByMultiEnabled && this._dragField.AllowGroupByResolved == true)
				if ( _isGroupByMultiEnabled )
				{
					if ( isMouseOverGroupByAreaMulti )
						this.ExpandGroupByAreaMultiIfNecessary( );

					GroupByAreaMultiDropType dropType = GroupByAreaMultiDropType.None;
					// JM 6-01-09 TFS17948 - Moved the check for AllowGroupByResolved here from the enclosing 'if' statement above.
					//if (isMouseOverGroupByAreaMulti)
					if ( isMouseOverGroupByAreaMulti && this._dragField.AllowGroupByResolved == true )
						dropType = _dragField.IsGroupBy ? GroupByAreaMultiDropType.Regrouping :
															GroupByAreaMultiDropType.Grouping;
					// JM 05-20-09 TFS 17824
					//else if (_dragField.IsGroupBy)
					else if ( _dragField.IsGroupBy && this._dragElement is LabelPresenter && ( (LabelPresenter)this._dragElement ).IsInGroupByArea )
						dropType = GroupByAreaMultiDropType.Ungrouping;

					// JM 08-18-09 TFS20898 - Make sure we have a GroupByAreaMulti before proceeeding
					if (this.DataPresenter.GroupByAreaMulti != null)
					{
						if (dropType != GroupByAreaMultiDropType.None)
						{
							// JM 06-02-09 TFS 17989 - Set the FieldLabelDragInProgress property on the GroupByAreaBase to true if we
							//						   are over the GroupByAreaMulti or if we are Ungrouping.
							if (dropType == GroupByAreaMultiDropType.Ungrouping || isMouseOverGroupByAreaMulti)
								this._dataPresenter.GroupByAreaMulti.SetValue(GroupByAreaBase.FieldLabelDragInProgressPropertyKey, KnownBoxes.TrueBox);
							else
								this._dataPresenter.GroupByAreaMulti.SetValue(GroupByAreaBase.FieldLabelDragInProgressPropertyKey, KnownBoxes.FalseBox);

							returnValue = this.GetGroupByAreaMultiDropInfo(mouseLoc, dropType);
						}
						else
							this._dataPresenter.GroupByAreaMulti.SetValue(GroupByAreaBase.FieldLabelDragInProgressPropertyKey, KnownBoxes.FalseBox);
					}
				}
			}

			// SSP 7/2/09 - NAS9.2 Field Chooser
			// If a field is dragged from a field chooser and dropped over the data presenter then 
			// we need to unhide it if it's hidden in the data presenter.
			// 
			if ( ( null == returnValue || !returnValue.IsDropValid ) && _isDraggingFromFieldChooser && isMouseOverDataPresenter )
			{
				returnValue = new DropInfo( this, FieldChooserDropType.ShowField );
			}

			if ( null == returnValue )
			{
				// SSP 2/4/09 TFS11475
				// I noticed this while debugging TFS11475. If the mouse is outside the
				// data presenter, then consider the drop invalid. Otherwise the drop 
				// indicator will appear outside of the grid since the drop target would
				// be scolled out of view.
				// 
				//if ( null == layoutContainer )
				if ( null == layoutContainer || !isMouseOverDataPresenter )
				{
					
					// Return an drop info instance that represents invalid drop.
					// 
					// SSP 7/2/09 - NAS9.2 Field Chooser
					// If the field is being dragged from a field chooser then
					// don't consider any area outside of the data presenter 
					// invalid. Without this change as soon as you start dragging
					// from the field chooser you start seeing the invalid area
					// cursor.
					// ------------------------------------------------------------
					//returnValue = new DropInfo( this, null, true );
					bool isDragAreaInvalid = true;
					if ( _isDraggingFromFieldChooser )
						isDragAreaInvalid = false;

					returnValue = new DropInfo( this, null, isDragAreaInvalid );
					// ------------------------------------------------------------
				}
			}

			if ( null == returnValue )
				// Return an drop info instance that represents invalid drop.
				// 
				returnValue = new DropInfo( this, layoutContainer, false );

			return returnValue;
		}

		///// <summary>
		///// Gets the drop info and also displays/moves the drag indicator.
		///// </summary>
		///// <param name="mouseLoc"></param>
		///// <param name="dragIndicatorLocation"></param>
		///// <returns></returns>
		//private DropInfo DragHelper_GetDropInfoHelper( PointInfo mouseLoc, out Point dragIndicatorLocation )
		//{
		//    // This method will keep the drag header within the logical row or column if the
		//    // AllowFieldMoving is WithinLogicalRow or WithinLogicalColumn.
		//    // 
		//    LayoutContainer layoutContainer = this.AdjustMouseLocationHelper( ref mouseLoc, out dragIndicatorLocation );

		//    // SSP 2/4/09 TFS11475
		//    // I noticed this while debugging TFS11475. If the mouse is outside the
		//    // data presenter, then consider the drop invalid. Otherwise the drop 
		//    // indicator will appear outside of the grid since the drop target would
		//    // be scolled out of view.
		//    // 
		//    //if ( null == layoutContainer )
		//    // JM 04-09 CrossBandGrouping feature.
		//    // Since we now allow the drag to start when AllowFieldMoving resolves to false in order to support
		//    // CrossBandGrouping, we need to add a check here to see if FieldMoving is allowed.
		//    //if (null == layoutContainer || !mouseLoc.IsInsideDataPresenter())
		//    if ( null == layoutContainer ||
		//        !mouseLoc.IsInsideDataPresenter( ) ||
		//        ( this._dragField != null &&
		//         this._dragField.Owner != null &&
		//         AllowFieldMoving.No == this._dragField.Owner.AllowFieldMovingResolved ) ) // JM 6-01-09 Add null checks for _dragField and _dragField.Owner
		//    {
		//        // JM 04-17-09 - CrossBand grouping feature
		//        // If the mouse is over the GroupByArea and the GroupByAreaMulti is being used and it is not expanded,
		//        // expand it now and return an appropriate DropInfo.
		//        // JM 6-01-09 TFS17948 - Move the check for AllowGroupByResolved several lines down where we are checking to see
		//        //						 if we should set the dropType to Grouping or Regrouping.
		//        // JM 5-11-09 TFS17520
		//        //if (_isGroupByMultiEnabled)
		//        //if (_isGroupByMultiEnabled && this._dragField.AllowGroupByResolved == true)
		//        if ( _isGroupByMultiEnabled )
		//        {
		//            bool isMouseOverGroupByAreaMulti = this.IsMouseOverGroupByAreaMulti( mouseLoc );
		//            if ( isMouseOverGroupByAreaMulti )
		//                this.ExpandGroupByAreaMultiIfNecessary( );

		//            GroupByAreaMultiDropType dropType = GroupByAreaMultiDropType.None;
		//            // JM 6-01-09 TFS17948 - Moved the check for AllowGroupByResolved here from the enclosing 'if' statement above.
		//            //if (isMouseOverGroupByAreaMulti)
		//            if ( isMouseOverGroupByAreaMulti && this._dragField.AllowGroupByResolved == true )
		//                dropType = _dragField.IsGroupBy ? GroupByAreaMultiDropType.Regrouping :
		//                                                    GroupByAreaMultiDropType.Grouping;
		//            // JM 05-20-09 TFS 17824
		//            //else if (_dragField.IsGroupBy)
		//            else if ( _dragField.IsGroupBy && this._dragElement is LabelPresenter && ( (LabelPresenter)this._dragElement ).IsInGroupByArea )
		//                dropType = GroupByAreaMultiDropType.Ungrouping;

		//            if ( dropType != GroupByAreaMultiDropType.None )
		//            {
		//                // JM 06-02-09 TFS 17989 - Set the FieldLabelDragInProgress property on the GroupByAreaBase to true if we
		//                //						   are over the GroupByAreaMulti or if we are Ungrouping.
		//                if ( dropType == GroupByAreaMultiDropType.Ungrouping || isMouseOverGroupByAreaMulti )
		//                    this._dataPresenter.GroupByAreaMulti.SetValue( GroupByAreaBase.FieldLabelDragInProgressPropertyKey, KnownBoxes.TrueBox );
		//                else
		//                    this._dataPresenter.GroupByAreaMulti.SetValue( GroupByAreaBase.FieldLabelDragInProgressPropertyKey, KnownBoxes.FalseBox );

		//                return this.GetGroupByAreaMultiDropInfo( mouseLoc, dropType );
		//            }
		//            else
		//                this._dataPresenter.GroupByAreaMulti.SetValue( GroupByAreaBase.FieldLabelDragInProgressPropertyKey, KnownBoxes.FalseBox );
		//        }

		//        // Return an drop info instance that represents invalid drop.
		//        // 
		//        return new DropInfo( this, null, true );
		//    }

		//    // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
		//    // If the mouse is over the field splitter then we need separate logic
		//    // to calculate the drop info.
		//    //
		//    FixedFieldSplitter splitter = layoutContainer.GetSplitterFromPoint( mouseLoc );

		//    if ( null != splitter )
		//        return this.GetSplitterDropInfo( layoutContainer, mouseLoc, splitter );

		//    LayoutItem targetItem = layoutContainer.GetItemFromPoint( mouseLoc );
		//    if ( null != targetItem )
		//    {
		//        DropLocation dropLoc = this.GetRelativeDropLocation( targetItem.Rect.GetRect( ), mouseLoc.GetPosition( ) );
		//        //Debug.Assert( DropLocation.None != dropLoc, "If mouse is in item then drop location should be one of left, right, above, below." );
		//        if ( DropLocation.None != dropLoc )
		//            return new DropInfo( layoutContainer, dropLoc, targetItem );
		//    }
		//    else
		//    {
		//        RectInfo dropRect = null;
		//        int row, col;
		//        layoutContainer.GetLogicalCellFromPoint( mouseLoc, out row, out col );
		//        if ( row >= 0 && col >= 0 && !layoutContainer.HasItemAtLogicalCell( row, col, true ) )
		//        {
		//            dropRect = layoutContainer.GetRect( row, col,
		//                    _dragItemLayoutInfo.ColumnSpan, _dragItemLayoutInfo.RowSpan,
		//                    null != _dragElement ? new Size( _dragElement.ActualWidth, _dragElement.ActualHeight ) : Size.Empty
		//                );
		//        }

		//        if ( null != dropRect && !dropRect.GetRect( ).IsEmpty )
		//            return new DropInfo( layoutContainer, row, col );
		//    }

		//    // Return an drop info instance that represents invalid drop.
		//    // 
		//    return new DropInfo( this, layoutContainer, false );
		//}

		#endregion // DragHelper_GetDropInfoHelper

		// JM 04-17-09 - CrossBand grouping feature
		#region ExpandGroupByAreaMultiIfNecessary

		private void ExpandGroupByAreaMultiIfNecessary()
		{
			if (this._isGroupByMultiEnabled == false)
				return;

			GroupByAreaMulti gbam = _dataPresenter.GroupByAreaMulti;
			if (gbam == null)
				return;

			if (gbam.IsExpanded == false)
				gbam.IsExpanded = true;
		}

		#endregion //ExpandGroupByAreaMultiIfNecessary

		#region GetDescendantElementContainingPoint

		internal static FrameworkElement GetDescendantElementContainingPoint( 
			PointInfo point, FrameworkElement ancestor, Type frameworkElementType, bool getLowestLevelElement )
		{
			FrameworkElement lastMatchingElem = null;

			IEnumerable layoutAreas = GridUtilities.GetTrivialDescendantsOfType( ancestor, frameworkElementType, true );
			foreach ( object i in layoutAreas )
			{
				FrameworkElement ii = i as FrameworkElement;

				// Skip hidden elements.
				// 
				if ( null == ii || !ii.IsVisible )
					continue;

				Point p = point.GetPosition( ii );
				if ( new Rect( 0, 0, ii.ActualWidth, ii.ActualHeight ).Contains( p ) )
				{
					if ( ! getLowestLevelElement )
						return ii;

					lastMatchingElem = ii;
				}
			}

			return lastMatchingElem;
		}

		#endregion // GetDescendantElementContainingPoint

		// JM 04-17-09 - CrossBand grouping feature
		#region GetGroupByAreaMultiDropInfo

		private DropInfo GetGroupByAreaMultiDropInfo(PointInfo mouseLoc, GroupByAreaMultiDropType dropType)
		{
			// We don't have to worry about drop locations or targets in these cases...
			GroupByAreaMulti groupByAreaMulti = null;
			if (this.DataPresenter != null)
				groupByAreaMulti = this.DataPresenter.GroupByAreaMulti;

			if (dropType	== GroupByAreaMultiDropType.Ungrouping	||
				dropType	== GroupByAreaMultiDropType.None		||
				null		== groupByAreaMulti)
			{
				return new DropInfo(this,
									dropType,
									new RectInfo(this,
												 new Rect(new Size(0, 0)),
												 this.DataPresenter.GroupByAreaMulti),
									DropLocation.None,
									-1,
									false);
			}


			// Since we are over the GroupByAreaMulti, we need to figure out the drop targets and rects.
			// Get a list of all the LabelPresenters currently in the GroupByAreaMulti that belong to the 
			// same FieldLayout as the Field being dragged.  If we encounter a LabelPresenter for the Field
			// being dragged (i.e., the Field being dragged is already grouped) do NOT include it in the list.
			List<LabelPresenter> labelPresenters = new List<LabelPresenter>();

			Utilities.DependencyObjectSearchCallback<LabelPresenter> callback = new Utilities.DependencyObjectSearchCallback<LabelPresenter>(delegate(LabelPresenter labelPresenter)
			{
				if (labelPresenter.Field.Owner	== this._fieldLayout &&
					labelPresenter.Field		!= this._dragField)
					labelPresenters.Add(labelPresenter);

				return false;
			});
			LabelPresenter lp = Utilities.GetDescendantFromType<LabelPresenter>(groupByAreaMulti, true, callback);


			// If there are no LabelPresenters from the same FieldLayout as the Field being dragged, then look for an ItemsControl
			// whose DataContext is a FieldLayoutGroupByInfo object that represents the FieldLayout that owns the Field being dragged 
			// and use that as the target for the drop (this is the itemsControl that will hold the grouped Fields for the FieldLayout). 
			// If no such ItemsControl is found, consider the entire GroupByArea to be the drop target, unless the Field being dragged 
			// is grouped.  In that case, consider this a NOOP.  
			FrameworkElement	target			= null;
			DropLocation		dropLocation	= DropLocation.None;
			int					insertAtIndex	= 0;
			if (labelPresenters.Count == 0)
			{
				// Look for an ItemsControl that has a FieldLayoutGroupByInfo for the current FieldLayout as its DataContext, and where the ItemsControl.ItemsSource 
				// is bound to the GroupByFields property of that FieldLayoutGroupByInfo.
				List<ItemsControl> itemsControls = new List<ItemsControl>();

				Utilities.DependencyObjectSearchCallback<ItemsControl> itemsControlCallback = new Utilities.DependencyObjectSearchCallback<ItemsControl>(delegate(ItemsControl itemsControl)
				{
					if (itemsControl.DataContext is FieldLayoutGroupByInfo	&&
						((FieldLayoutGroupByInfo)itemsControl.DataContext).FieldLayout == this._fieldLayout &&
						itemsControl.ItemsSource == ((FieldLayoutGroupByInfo)itemsControl.DataContext).GroupByFields)
						itemsControls.Add(itemsControl);

					return false;
				});
				Utilities.GetDescendantFromType<ItemsControl>(groupByAreaMulti, true, itemsControlCallback);
				if (itemsControls.Count == 1)
				{
					target			= itemsControls[0];
					dropLocation	= DropLocation.RightOfTarget;
					insertAtIndex	= 0;
				}
				else
				{
					bool isNoop = this._dragField.IsGroupBy;

					return new DropInfo(this,
										dropType,
										new RectInfo(this,
													 new Rect(new Size(this.DataPresenter.GroupByAreaMulti.ActualWidth,
																	   this.DataPresenter.GroupByAreaMulti.ActualHeight)),
													 this.DataPresenter.GroupByAreaMulti),
										DropLocation.OverTarget,
										0,
										isNoop);
				}
			}


			// If the drop target is still null and we found LabelPresenters, figure out which one of them should be
			// a drop target.
			if (labelPresenters.Count > 0 && target == null)
			{
				// Sort the LabelPresenters based on their Horizontal position.
				labelPresenters.Sort(CompareLabelPresenters);

				Point	mousePos			= mouseLoc.GetPosition(groupByAreaMulti);
				Rect	labelPresenterRect	= Rect.Empty;
				FieldSortDescriptionCollection
						sortedFields		= this._fieldLayout.SortedFields;

				// See if we are to the left of any of the LabelPresenters.
				foreach (LabelPresenter labPres in labelPresenters)
				{
					labelPresenterRect = new Rect(labPres.TranslatePoint(new Point(0, 0), groupByAreaMulti),
													new Size(labPres.ActualWidth, labPres.ActualHeight));

					if (mousePos.X < (labelPresenterRect.Left + (labelPresenterRect.Width / 2)))
					{
						target = labPres;
						dropLocation	= DropLocation.LeftOfTarget;

						insertAtIndex	= sortedFields.IndexOf(labPres.Field);
						insertAtIndex	= this.AdjustGroupByAreaMultiInsertAtIndex(insertAtIndex, dropType, sortedFields);

						break;
					}
				}

				// If we are not to the left of any of the LabelPresenters then assume we are positioned to the 
				// right of the last LabelPresenter.
				if (target == null)
				{
					target			= labelPresenters[labelPresenters.Count - 1];
					dropLocation	= DropLocation.RightOfTarget;

					insertAtIndex	= sortedFields.IndexOf(labelPresenters[labelPresenters.Count - 1].Field) + 1;
					insertAtIndex	= this.AdjustGroupByAreaMultiInsertAtIndex(insertAtIndex, dropType, sortedFields);
				}
			}

			return new DropInfo(this,
								dropType,
								new RectInfo(this,
											new Rect(target.TranslatePoint(new Point(0, 0), groupByAreaMulti),
												new Size(target.ActualWidth, target.ActualHeight)), 
											groupByAreaMulti),
								dropLocation,
								insertAtIndex,
								false);
		}

		#endregion //GetGroupByAreaMultiDropInfo

		#region GetItemLayoutInfo

		private ItemLayoutInfo GetItemLayoutInfo( Field field )
		{
			return _layoutInfo[ field ];
		}

		#endregion // GetItemLayoutInfo

		#region GetLayoutContainerFromPoint

		private UIElement GetLayoutContainerFromPointHelper( PointInfo point, Type containerElemType )
		{
			FrameworkElement containerElem = null;

			// If the point is outside of the data presenter then return null.
			// 
			if ( ! point.IsInsideDataPresenter( ) )
				return null;

			// First try to get the cell panel from adorner layer (in some modes, field labels are
			// in adorner layer).
			// 
			AdornerLayer adl = AdornerLayer.GetAdornerLayer( _dataPresenter.CurrentPanel );
			if ( null != adl )
				containerElem = GetDescendantElementContainingPoint( point, adl, containerElemType, true );

			if ( null == containerElem )
				containerElem = GetDescendantElementContainingPoint( point, _dataPresenter, containerElemType, true );

			if ( null == containerElem )
			{
				// If adorner layer doesn't have any cell panel at the current mouse location, get it
				// from the hit test.
				//
				
				
				
				




				DependencyObject htr = _dataPresenter.InputHitTest( point.GetPosition( ) ) as DependencyObject;
				if ( null != htr )
					containerElem = Utilities.GetAncestorFromType( htr, containerElemType, true ) as FrameworkElement;
			}

			return containerElem;
		}

		private LayoutContainer GetLayoutContainerFromPoint( PointInfo point )
		{
			VirtualizingDataRecordCellPanel cellPanel = this.GetLayoutContainerFromPointHelper( point, typeof( VirtualizingDataRecordCellPanel ) ) as VirtualizingDataRecordCellPanel;
			if ( null != cellPanel )
			{
				RecordPresenter rp = (RecordPresenter)Utilities.GetAncestorFromType( cellPanel, typeof( RecordPresenter ), true );
				Debug.Assert( null != rp );
				if ( null == rp )
					return null;

				if ( null != cellPanel
					// Make sure the field layouts match. You can't drop an field from one field layout to another.
					// 
					&& cellPanel.FieldLayout == _fieldLayout
					// If labels are separate from cells, then only layout panels that contain headers 
					// should be returned.
					// 
					
					
					
					
					
					
					&& ( !_fieldLayout.HasSeparateHeader || cellPanel.IsHeaderArea ) )
					return new LayoutContainer( this, cellPanel );
			}
			else
			{
                
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			}

			return null;
		}

		#endregion // GetLayoutContainerFromPoint

		#region GetRelativeDropLocation

		private DropLocation GetRelativeDropLocation( Rect rect, Point mousePos )
		{
			AllowFieldMoving allowFieldMoving = _fieldLayout.AllowFieldMovingResolved;
			bool stayWithinLogicalRow = AllowFieldMoving.WithinLogicalRow == allowFieldMoving;
			bool stayWithinLogicalColumn = AllowFieldMoving.WithinLogicalColumn == allowFieldMoving;

			DropLocation dropLocation = DropLocation.None;
			if ( rect.Contains( mousePos ) )
			{
				Point midPoint = new Point( rect.X + rect.Width / 2, rect.Y + rect.Height / 2 );

				double d = ! stayWithinLogicalColumn && ! stayWithinLogicalRow ? 0.25 : 0.50;

				if ( rect.Width > rect.Height )
				{
					if ( ! stayWithinLogicalColumn && mousePos.X < rect.X + rect.Width * d )
						dropLocation = DropLocation.LeftOfTarget;
					else if ( ! stayWithinLogicalColumn && mousePos.X >= rect.Right - rect.Width * d )
						dropLocation = DropLocation.RightOfTarget;
					else if ( ! stayWithinLogicalRow && mousePos.Y < midPoint.Y )
						dropLocation = DropLocation.AboveTarget;
					else if ( ! stayWithinLogicalRow )
						dropLocation = DropLocation.BelowTarget;
				}
				else
				{
					if ( ! stayWithinLogicalRow && mousePos.Y < rect.Y + rect.Height * d )
						dropLocation = DropLocation.AboveTarget;
					else if ( ! stayWithinLogicalRow && mousePos.Y >= rect.Bottom - rect.Height * d )
						dropLocation = DropLocation.BelowTarget;
					else if ( ! stayWithinLogicalColumn && mousePos.X < midPoint.X )
						dropLocation = DropLocation.LeftOfTarget;
					else if ( ! stayWithinLogicalColumn )
						dropLocation = DropLocation.RightOfTarget;
				}
			}

			return dropLocation;
		}

		#endregion // GetRelativeDropLocation

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region GetSplitterDropInfo
        private DropInfo GetSplitterDropInfo(LayoutContainer layoutContainer, PointInfo mouseLoc, FixedFieldSplitter splitter)
        {
            AllowFieldFixing allowFixing = _dragField.AllowFixingResolved;
            FixedFieldSplitterType splitterType = splitter.SplitterType;

            FixedFieldLocation fixedLocation = splitterType == FixedFieldSplitterType.Far
                ? FixedFieldLocation.FixedToFarEdge
                : FixedFieldLocation.FixedToNearEdge;

            // if the field already exists within that location then
            // treat a drop on the splitter as moving to the 
            // scrollable area. otherwise you can get into a situation where 
            // there is nothing in the scrollable area
            if (fixedLocation == _dragField.FixedLocation)
                fixedLocation = FixedFieldLocation.Scrollable;

            // if the field cannot be dragged into this area then consider it an 
            // invalid drop. i don't think we want to be considering the cell
            // that would be under the splitter
            if (!_dragField.IsFixedLocationAllowed(fixedLocation))
                return new DropInfo(this, layoutContainer, true);

            bool isVerticalSplitter = splitter.Orientation == Orientation.Vertical;
            DropLocation dropLocation;

            // figure out the "row" over which the mouse exists
            int row, col;
            layoutContainer.GetLogicalCellFromPoint(mouseLoc, out row, out col);

            // based on the splitter type & orientation we will be 
            // positioning the item just inside the splitter bar
            if (isVerticalSplitter)
            {
                // AS 1/21/09
                // I decided not to do this since we don't when dragging over an empty spot.
                //// restrict the row offset based on the span of the item
                //row = Math.Min(row, _layoutInfo.GetLogicalRowCount() - _dragItemLayoutInfo.RowSpan);

                if (splitterType == FixedFieldSplitterType.Near ^ fixedLocation == FixedFieldLocation.Scrollable)
                    dropLocation = DropLocation.LeftOfTarget;
                else
                    dropLocation = DropLocation.RightOfTarget;
            }
            else
            {
                // AS 1/21/09
                // I decided not to do this since we don't when dragging over an empty spot.
                //// restrict the column offset based on the span of the item
                //col = Math.Min(col, _layoutInfo.GetLogicalRowCount() - _dragItemLayoutInfo.ColumnSpan);

                if (splitterType == FixedFieldSplitterType.Near ^
                    fixedLocation == FixedFieldLocation.Scrollable)
                    dropLocation = DropLocation.AboveTarget;
                else
                    dropLocation = DropLocation.BelowTarget;
            }

            // see if there is a hole into which the drag item can be fit
            // on the inside of the splitter
            int extent = isVerticalSplitter ? _dragItemLayoutInfo.ColumnSpan : _dragItemLayoutInfo.RowSpan;

            for (int i = 0; i < extent; i++)
            {
                int tempRow;
                int tempCol;

                switch (dropLocation)
                {
                    default:
                        tempCol = tempRow = -1;
                        break;
                    case DropLocation.LeftOfTarget:
                        tempCol = col - (extent - i);
                        tempRow = row;
                        break;
                    case DropLocation.RightOfTarget:
                        tempCol = col - i;
                        tempRow = row;
                        break;
                    case DropLocation.AboveTarget:
                        tempRow = row - (extent - i);
                        tempCol = col;
                        break;
                    case DropLocation.BelowTarget:
                        tempRow = row - i;
                        tempCol = col;
                        break;
                }

                if ( tempCol >= 0 && tempRow >= 0 && !_layoutInfo.HasOverlappingItem(tempCol, tempRow, _dragItemLayoutInfo.ColumnSpan, _dragItemLayoutInfo.RowSpan, _dragItemLayoutInfo, fixedLocation) )
                {
                    return new DropInfo(layoutContainer, tempRow, tempCol);
                }
            }

            Rect rect = new Rect(splitter.RenderSize);

            // we need to get the offset and height of the logical row
            RectInfo rowRectInfo = layoutContainer.GetRect(row, col, _dragItemLayoutInfo.RowSpan, _dragItemLayoutInfo.ColumnSpan, new Size());
            Rect rowRect = rowRectInfo.GetRect(splitter);

            if (isVerticalSplitter)
            {
                rect.Y = rowRect.Y;
                rect.Height = rowRect.Height;
            }
            else
            {
                rect.X = rowRect.X;
                rect.Width = rowRect.Width;
            }

            RectInfo rectInfo = new RectInfo(layoutContainer.DragManager, rect, splitter);

            return new DropInfo(layoutContainer, dropLocation, rectInfo, row, col, fixedLocation);
        }
        #endregion //GetSplitterDropInfo

		#region HideDragIndicator

		internal void HideDragIndicator( )
		{
			if ( null != _dragIndicatorWindow )
			{
				_dragIndicatorWindow.Close( );
				_dragIndicatorShown = false;
			}
		}

		#endregion // HideDragIndicator

		#region HideDropIndicator

		private void HideDropIndicator( )
		{
			if ( null != _dropIndicatorWindow )
			{
				// Set the DropLocation on the drop indicator to None for animation triggerred 
				// off of DropLocation to work properly.
				// 
				DropIndicator dropIndicatorCtrl = DropInfo.GetDropIndicator( _dropIndicatorWindow );
				if ( null != dropIndicatorCtrl )
					dropIndicatorCtrl.DropLocation = DropLocation.None;

				_dropIndicatorWindow.Close( );

				// SSP 3/23/10 TFS24355
				// 
				_dropIndicatorWindow.Tag = null;

				_dropIndicatorShown = false;
			}
		}

		#endregion // HideDropIndicator

		#region ManageFieldChooserIsDragItemOver

		// SSP 7/2/09 - NAS9.2 Field Chooser
		// 
		/// <summary>
		/// Sets/clears appropriate FieldChooser's IsDragItemOver property. IsDragItemOver indicates
		/// if any field being dragged from the data presenter is over that field chooser.
		/// </summary>
		/// <param name="mouseLoc"></param>
		internal void ManageFieldChooserIsDragItemOver( PointInfo mouseLoc )
		{
			FieldChooser fieldChooser = null;
			if ( null != mouseLoc )
				mouseLoc.IsMouseOverFieldChooser( out fieldChooser );

			if ( _lastFieldChooserWithMouseOver != fieldChooser )
			{
				if ( null != _lastFieldChooserWithMouseOver )
					_lastFieldChooserWithMouseOver.InternalSetIsDragItemOver( false );

				_lastFieldChooserWithMouseOver = fieldChooser;

				if ( null != fieldChooser && ! _isDraggingFromFieldChooser )
					fieldChooser.InternalSetIsDragItemOver( true );
			}
		}

		#endregion // ManageFieldChooserIsDragItemOver

		#region ManagerScrollTimerHelper

		// SSP 7/2/09 - NAS9.2 Field Chooser
		// 
		private bool _scrollTimer_hasMouseBeenInDataPresenter;

		// SSP 7/2/09 - NAS9.2 Field Chooser
		// Pass along mouseLoc so we don't have to reget it.
		// 
		//private void ManagerScrollTimerHelper( )
		private void ManagerScrollTimerHelper( PointInfo mouseLoc )
		{
			// SSP 7/2/09 - NAS9.2 Field Chooser
			// When a field is dragged from a field chooser, don't scroll the data presenter unless the 
			// mouse is moved in the data presenter first. Otherwise as soon as the user starts dragging
			// a field from the field chooser, the data presenter starts scrolling.
			// 
			// ----------------------------------------------------------------------------------------------
			//Point mouseLoc = Mouse.GetPosition( _dataPresenter );
			//if ( GridUtilities.DoesElementContainPoint( _dataPresenter, mouseLoc ) )
			bool isMouseInDataPresenter = mouseLoc.IsInsideDataPresenter( );
			_scrollTimer_hasMouseBeenInDataPresenter = _scrollTimer_hasMouseBeenInDataPresenter || isMouseInDataPresenter;

			if ( isMouseInDataPresenter || ( _isDraggingFromFieldChooser && ! _scrollTimer_hasMouseBeenInDataPresenter ) )
			// ----------------------------------------------------------------------------------------------
			{
				if ( null != _scrollTimer )
					_scrollTimer.Stop( );
			}
			else
			{
				if ( null == _scrollTimer )
				{
					TimeSpan interval = TimeSpan.FromMilliseconds( 100 );
					_scrollTimer = new DispatcherTimer( interval, DispatcherPriority.Input, OnScrollTimer_Tick, _dataPresenter.Dispatcher );
				}

				_scrollTimer.Start( );
			}
		}

		#endregion // ManagerScrollTimerHelper

		#region OnDragEnd

		/// <summary>
		/// Called to indicate that the drag operation has ended. Drop will be processed if 'cancel' parameter
		/// is false.
		/// </summary>
		/// <param name="mouseEventArgs">Mouse event args associated with the drag end.</param>
		/// <param name="cancel">Indicates whether to process the drop operation.</param>
		internal void OnDragEnd( MouseEventArgs mouseEventArgs, bool cancel )
		{
			try
			{
				this.OnDragEndHelper( mouseEventArgs, cancel );
			}
			finally
			{
				// SSP 6/24/09 - NAS9.2 Field Chooser
				// Before this change the _fieldDragManager member on the data presenter was being
				// set in the LabelPresenter's StartDragHelper and cleared in the data presenter
				// after call to OnDragEnd. Moved that logic here in the StartDrag and OnDragEnd
				// methods. Reason for the change is that now we need to initialize the 
				// _fieldDragManager member of the data presenter OR the field chooser based on 
				// where the label presenter that the user started dragging was in.
				// 
				// --------------------------------------------------------------------------------
				if ( _isDraggingFromFieldChooser )
					_sourceFieldChooser.SetFieldDragManagerHelper( null );
				else
					_dataPresenter.SetFieldDragManagerHelper( null );
				// --------------------------------------------------------------------------------
			}

			Debug.WriteLine( "Drag End" );
		}

		private void OnDragEndHelper( MouseEventArgs mouseEventArgs, bool cancel )
		{
			// SSP 2/4/09 TFS11475
			// Prevent any pending timer ticks from being processed since the drag has ended.
			// For that null out _scrollTimer - we check for it being null in OnScrollTimer_Tick.
			// 
			if ( null != _scrollTimer )
			{
				DispatcherTimer tmpTimer = _scrollTimer;
				_scrollTimer = null;
				tmpTimer.Stop( );
			}

			// Reset the IsDragSource property back.
			// 
			LabelPresenter labelPresenter = _dragElement as LabelPresenter;
			if ( null != labelPresenter )
				labelPresenter.IsDragSource = false;

			if ( !cancel )
			{
				PointInfo mouseLoc = new PointInfo( this, mouseEventArgs.GetPosition( _dataPresenter ), _dataPresenter );
				this.DragHelper( mouseLoc, true );
			}

			// We set the OverrideCursor when the item is dragged outside of a valid 
			// drop area (outside of the layout container), in which case we have to 
			// reset it here since the drag operation is being ended.
			// 
			Mouse.OverrideCursor = null;

			this.HideDragIndicator( );
			this.HideDropIndicator( );

			if ( null != _dragIndicatorWindow )
			{
				_dragIndicatorWindow.Dispose( );
				_dragIndicatorWindow = null;
			}

			if ( null != _dropIndicatorWindow )
			{
				_dropIndicatorWindow.Dispose( );
				_dropIndicatorWindow = null;
			}
		}

		#endregion // OnDragEnd

		#region OnMouseMove

		/// <summary>
		/// Called whenever mouse is moved while drag operation is in progress.
		/// </summary>
		internal void OnMouseMove( MouseEventArgs mouseEventArgs )
		{
			PointInfo mouseLoc = new PointInfo( this, mouseEventArgs.GetPosition( _dataPresenter ), _dataPresenter );

			this.DragHelper( mouseLoc, false );
		}

		#endregion // OnMouseMove

		#region OnScrollTimer_Tick

		private void OnScrollTimer_Tick( object sender, EventArgs e )
		{
			// SSP 2/4/09 TFS11475
			// If drag has ended (where we null out _scrollTimer), then don't process
			// the tick.
			// 
			if ( null == _scrollTimer )
				return;

			DataPresenterBase dp = _dataPresenter;

			Point mouseLoc = Mouse.GetPosition( _dataPresenter );
			Rect dpRect = new Rect( 0, 0, dp.ActualWidth, dp.ActualHeight );

			if ( dpRect.Contains( mouseLoc ) )
				return;

			double dx = 0;
			double dy = 0;

			if ( mouseLoc.X < dpRect.X )
				dx = mouseLoc.X - dpRect.X;
			else if ( mouseLoc.X > dpRect.Right )
				dx = mouseLoc.X - dpRect.Right;

			if ( mouseLoc.Y < dpRect.Y )
				dy = mouseLoc.Y - dpRect.Y;
			else if ( mouseLoc.Y > dpRect.Bottom )
				dy = mouseLoc.Y - dpRect.Bottom;

			ViewBase view = dp.CurrentViewInternal;
			Orientation dpOrientation = null != view ? view.LogicalOrientation : Orientation.Vertical;
			bool shouldScrollHorizontal = Orientation.Vertical == dpOrientation;
			bool shouldScrollVertical = Orientation.Horizontal == dpOrientation;

			// SSP 2/4/09 TFS11475
			// If we scroll then we need to re-evaluate the target item, drop location etc... since
			// the elements would have moved as a result of scrolling.
			// 
			bool scrolled = false;

			if ( dx != 0 && shouldScrollHorizontal )
			{
				( (ISelectionHost)dp ).DoAutoScrollHorizontal( null,
					dx < 0 ? ScrollDirection.Decrement : ScrollDirection.Increment,
					this.CalcScrollSpeed( Math.Abs( dx ) ) );

				// SSP 2/4/09 TFS11475
				scrolled = true;
			}

			if ( dy != 0 && shouldScrollVertical )
			{
				
				
				
				
				( (ISelectionHost)dp ).DoAutoScrollVertical( null,
					dy < 0 ? ScrollDirection.Decrement : ScrollDirection.Increment,
					this.CalcScrollSpeed( Math.Abs( dy ) ) );
				
				
				
				

				// SSP 2/4/09 TFS11475
				scrolled = true;
			}

			// SSP 2/4/09 TFS11475
			// 
			if ( scrolled )
				this.DragHelper( new PointInfo( this, mouseLoc, _dataPresenter ), false );
		}

		#endregion // OnScrollTimer_Tick

        #region PositionToolWindow
        // AS 2/12/09 TFS11410
        // The top/left are relative to the logical screen used by the toolwindow
        // which may be within the adorner layer.
        //
        private void PositionToolWindow(ToolWindow toolWindow, Point screenPoint)
        {
            Point relativePoint = Utilities.PointFromScreenSafe(this._dataPresenter, screenPoint);
            FrameworkElement owner = toolWindow.Owner ?? this._dataPresenter;
            relativePoint = ToolWindow.GetScreenPoint(owner, relativePoint, this._dataPresenter);
            toolWindow.Left = relativePoint.X;
            toolWindow.Top = relativePoint.Y;
        } 
        #endregion //PositionToolWindow

		#region ShowDragIndicator

		// JM 07-07-09 - Change the method signature to take a point that represents the DragIndicator location
		// so we can position the indicator before showing it.
		//internal void ShowDragIndicator( )
		internal void ShowDragIndicator(Point dragIndicatorLocation)
		{
			DragToolWindow window = new DragToolWindow( _dataPresenter );

			FieldDragIndicator dragIndicator = new FieldDragIndicator( );
			dragIndicator.Field = _dragField;

			// SSP 6/29/09 - NAS9.2 Field Chooser/CrossBand Grouping
			// Initialize the new IsSourceFromFieldChooser and IsSourceFromGroupByArea properties
			// of the drag indicator.
			// 
			dragIndicator.IsSourceFromFieldChooser = _isDraggingFromFieldChooser;
			dragIndicator.IsSourceFromGroupByArea = _isDraggingFromGroupByArea;

            // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
            CellPresenterBase sourceCp = null;

			// Ensure that the drag indicator matches the dimensions of the label being dragged.
			// 
			if ( null != _dragElement )
			{
				// SSP 10/23/08
				// 
				// ----------------------------------------------------------------------
				




				// Make sure the CellPresenter element in the drag indicator (see xaml) gets proper
				// DataContext of a RecordPresenter. Otherwise it will throw an exception.
				// 
				RecordPresenter rp = Utilities.GetAncestorFromType( _dragElement, typeof( RecordPresenter ), true ) as RecordPresenter;
				if ( null != rp )
					dragIndicator.DataContext = rp.DataContext;

				// Display cell in the drag indicator if labels and cells are togather in each record.
                //
                // JJD 5/28/09 
                // Don't include the cell if we don't have a RecordPresenter to set as the DataContext
                //dragIndicator.IncludeCell = _fieldLayout.UseCellPresenters;
                dragIndicator.IncludeCell = rp != null && _fieldLayout.UseCellPresenters;

                // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
                // If we're dragging a label presenter that was within a cellprsenter then we
                // should get the actual width/height of the cell presenter itself.
                //
                sourceCp = _dragElement as CellPresenterBase ?? Utilities.GetAncestorFromType(_dragElement, typeof(CellPresenterBase), true) as CellPresenterBase;
                FrameworkElement dragSource = sourceCp ?? _dragElement;

				// Make sure the drag indicator is not smaller than the label that's actually in the grid.
				// 
                // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
                //dragIndicator.MinWidth = _dragElement.ActualWidth;
				//dragIndicator.MinHeight = _dragElement.ActualHeight;
				dragIndicator.MinWidth = dragSource.ActualWidth;
				dragIndicator.MinHeight = dragSource.ActualHeight;
				// ----------------------------------------------------------------------
			}

			window.Content = dragIndicator;

			_dragIndicatorWindow = window;

            // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
            // The DragIndicator could have a cell presenter in it and we want 
            // it to use the same sizing information that the cell in the 
            // display is using.
            //
            if (null != sourceCp)
            {
                VirtualizingDataRecordCellPanel.ApplyTemplateRecursively(dragIndicator);
                CellPresenterBase cp = Utilities.GetDescendantFromType(dragIndicator, typeof(CellPresenterBase), true) as CellPresenterBase;

                if (null != cp)
                {
                    cp.CellRect = sourceCp.CellRect;
                    cp.LabelRect = sourceCp.LabelRect;
                }
            }

			Debug.WriteLine( "Showing drag indicator window" );

			// Make the drag indicator visible.
			// 
			// JM 07-07-09 - Position the drag indicator before showing it.
			this.PositionToolWindow(_dragIndicatorWindow, dragIndicatorLocation);

			_dragIndicatorWindow.Show( _dataPresenter, false );
			_dragIndicatorShown = true;
		}

		#endregion // ShowDragIndicator

		#region ShowDropIndicator

		private void ShowDropIndicator( DropInfo dropInfo )
		{
			if ( null == _dropIndicatorWindow )
			{
				DragToolWindow window = new DragToolWindow( _dataPresenter );
				DropIndicator dropIndicator = new DropIndicator( );
				window.Content = dropIndicator;
				//window.Content = "DROP INDICATOR";

				_dropIndicatorWindow = window;
			}

			// SSP 6/29/09 - NAS9.2 Field Chooser
			// Certain drop types don't show indicators, like dragging a field outside of the
			// data presenter to hide it. Changed the return type from void to bool to 
			// return a value indicating whether the drop indicator should be shown or not.
			// 
			//dropInfo.InitializeDropIndicator( _dropIndicatorWindow );
			bool dropIndicatorVisibility = dropInfo.InitializeDropIndicator( _dropIndicatorWindow );

			// SSP 6/29/09 - NAS9.2 Field Chooser
			// Related to above. Enclosed the existing code into the if block and added the 
			// else block.
			// 
			if ( dropIndicatorVisibility )
			{
				if ( !_dropIndicatorShown )
				{
					_dropIndicatorWindow.Show( _dataPresenter, false );

					// SSP 10/23/09 TFS22643
					// 
					if ( null == _dropIndicatorWindow.Tag )
					{
						dropInfo.InitializeDropIndicator( _dropIndicatorWindow );
						_dropIndicatorWindow.Tag = new object( );
					}

					_dropIndicatorShown = true;
				}
			}
			else
			{
				this.HideDropIndicator( );
			}
		}

		#endregion // ShowDropIndicator

		#region StartDrag

		/// <summary>
		/// Starts dragging operation. It displays the drag indicator to indicate that dragging operation is in progress.
		/// </summary>
		internal void StartDrag( MouseEventArgs mouseEventArgs )
		{
			// SSP 6/24/09 - NAS9.2 Field Chooser
			// Before this change the _fieldDragManager member on the data presenter was being
			// set in the LabelPresenter's StartDragHelper and cleared in the data presenter
			// after call to OnDragEnd. Moved that logic here in the StartDrag and OnDragEnd
			// methods. Reason for the change is that now we need to initialize the 
			// _fieldDragManager member of the data presenter OR the field chooser based on 
			// where the label presenter that the user started dragging was in.
			// 
			// --------------------------------------------------------------------------------
			if ( _isDraggingFromFieldChooser )
				_sourceFieldChooser.SetFieldDragManagerHelper( this );
			else
				_dataPresenter.SetFieldDragManagerHelper( this );
			// --------------------------------------------------------------------------------

			PointInfo mouseLoc = new PointInfo( this, mouseEventArgs.GetPosition( _dataPresenter ), _dataPresenter );

			// Set IsDragSource so the label in the grid that's being dragged can be styled differently.
			// 
			LabelPresenter labelPresenter = _dragElement as LabelPresenter;
			if ( null != labelPresenter )
				labelPresenter.IsDragSource = true;

			Debug.WriteLine( "StartDrag" );

			// Call DragHelper which will display drag indicator window.
			// 
			this.DragHelper( mouseLoc, false );
		}

		#endregion // StartDrag

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // FieldDragManager Class

	#region DragToolWindow Class

	/// <summary>
	/// Tool window derived class for displaying drag indicators.
	/// </summary>
	/// <remarks>
	/// <b>Note</b> that there is no need for you to instantiate this directly. DataPresenter 
	/// creates this control when the user starts a drag operation.
	/// </remarks>
	/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
	internal class DragToolWindow : ToolWindow
	{
		#region Variables

		private DataPresenterBase _dataPresenter;

		private static ControlTemplate g_indicatorToolWindowTemplate;

		#endregion // Variables

		#region Constructor

		static DragToolWindow( )
		{
			FrameworkElementFactory fefRoot = new FrameworkElementFactory(typeof(ContentPresenter));
			fefRoot.Name = "PART_Content";
			ControlTemplate template = new ControlTemplate(typeof(ToolWindow));
			template.VisualTree = fefRoot;
			template.Seal();
			
			g_indicatorToolWindowTemplate = template;
		}

		public DragToolWindow( DataPresenterBase dataPresenter )
		{
			_dataPresenter = dataPresenter;

			this.ResizeMode = ResizeMode.NoResize;
			this.UseOSNonClientArea = false;
			this.Topmost = true;
			// moved this up from below so its available when the template is set and
			// before the content is set.
			this.VerticalAlignmentMode = ToolWindowAlignmentMode.Manual;
			this.HorizontalAlignmentMode = ToolWindowAlignmentMode.Manual;
			this.Template = g_indicatorToolWindowTemplate;
			this.Focusable = false;
			this.IsHitTestVisible = false;
			//this.HorizontalAlignment = HorizontalAlignment.Left;
			//this.VerticalAlignment = VerticalAlignment.Top;

			// AS 5/21/08
			// Instead of binding the theme property, we will add the resources of the 
			// dockmanager to the window's resources. That will pick up the theme property
			// resources and any other resources they put in.
			//
			//this.SetBinding(ToolWindow.ThemeProperty, Utilities.CreateBindingObject(XamDockManager.ThemeProperty, System.Windows.Data.BindingMode.OneWay, this._dockManager));
			this.Resources.MergedDictionaries.Add( dataPresenter.Resources );
		}

		#endregion // Constructor

		#region Base class overrides

		// AS 8/4/11 TFS83465/TFS83469
		#region KeepOnScreen
		internal override bool KeepOnScreen
		{
			get
			{
				return false;
			}
		}
		#endregion //KeepOnScreen 

		#endregion //Base class overrides

		#region Dispose

		internal void Dispose( )
		{
			this.Resources.MergedDictionaries.Clear( );

            // JJD 9/14/09
            // In case the DragToolWindow gets rooted make sure
            // that it won't root the dataPresenter
            this._dataPresenter = null;
		}

		#endregion // Dispose
	}

	#endregion // DragToolWindow Class

	#region FieldDragIndicator Class

	/// <summary>
	/// This control is used for displaying drag indicator when a field is being dragged.
	/// </summary>
	/// <remarks>
	/// <b>Note</b> that there is no need for you to instantiate this directly. DataPresenter 
	/// creates this control when the user starts a drag operation.
	/// </remarks>
	/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class FieldDragIndicator : Control
	{
		#region Constructor

		static FieldDragIndicator( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( FieldDragIndicator ), new FrameworkPropertyMetadata( typeof( FieldDragIndicator ) ) );
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( FieldDragIndicator ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FieldDragIndicator"/> class.
		/// </summary>
		public FieldDragIndicator( )
		{
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Field

		/// <summary>
		/// Identifies the <see cref="Field"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldProperty = DependencyProperty.Register(
				"Field",
				typeof( Field ),
				typeof( FieldDragIndicator ),
				new FrameworkPropertyMetadata( null,
					new PropertyChangedCallback( OnFieldChanged ) )
			);

		/// <summary>
		/// Gets or sets the field being dragged. The field's label is displayed inside the drag indicator.
		/// </summary>
		//[Description( "Specifies the associated field being dragged." )]
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
			Field newVal = (Field)e.NewValue;
		}

		#endregion // Field

		#region IncludeCell

		/// <summary>
		/// Identifies the <see cref="IncludeCell"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IncludeCellProperty = DependencyProperty.Register(
				"IncludeCell",
				typeof( bool ),
				typeof( FieldDragIndicator ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox )
			);

		/// <summary>
		/// Specifies whether the drag field's cell is also displayed in addition to its label.
		/// </summary>
		//[Description( "Specifies whether cell is also displayed in addition to the field label.")]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IncludeCell
		{
			get
			{
				return (bool)this.GetValue( IncludeCellProperty );
			}
			set
			{
				this.SetValue( IncludeCellProperty, value );
			}
		}

		#endregion // IncludeCell

		#region IsSourceFromFieldChooser

		
		

		/// <summary>
		/// Identifies the <see cref="IsSourceFromFieldChooser"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty IsSourceFromFieldChooserProperty = DependencyProperty.Register(
			"IsSourceFromFieldChooser",
			typeof( bool ),
			typeof( FieldDragIndicator ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Indicates if the field being dragged is from a FieldChooser.
		/// </summary>
		//[Description( "Indicates if the field being dragged is from a FieldChooser." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public bool IsSourceFromFieldChooser
		{
			get
			{
				return (bool)this.GetValue( IsSourceFromFieldChooserProperty );
			}
			set
			{
				this.SetValue( IsSourceFromFieldChooserProperty, value );
			}
		}

		#endregion // IsSourceFromFieldChooser

		#region IsSourceFromGroupByArea

		
		

		/// <summary>
		/// Identifies the <see cref="IsSourceFromGroupByArea"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty IsSourceFromGroupByAreaProperty = DependencyProperty.Register(
			"IsSourceFromGroupByArea",
			typeof( bool ),
			typeof( FieldDragIndicator ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Indicates if the field being dragged is from the group-by area.
		/// </summary>
		//[Description( "Indicates if the field being dragged is from the group-by area." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2 )]
		public bool IsSourceFromGroupByArea
		{
			get
			{
				return (bool)this.GetValue( IsSourceFromGroupByAreaProperty );
			}
			set
			{
				this.SetValue( IsSourceFromGroupByAreaProperty, value );
			}
		}

		#endregion // IsSourceFromGroupByArea

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // FieldDragIndicator Class



	#region ItemLayoutInfo Class

	
	
	
	/// <summary>
	/// Contains information on how an item is to be laid out by the layout manager.
	/// </summary>
	internal class ItemLayoutInfo
	{
		#region Member Vars

		private int _row;
		private int _column;
		private int _rowSpan;
		private int _columnSpan;

		// SSP 1/6/09 TFS11860
		// When a new field is added or an existing collapsed field is made visible, we need
		// to make sure it doesn't overlap with other fields. Also if the field was collapsed,
		// it should appear close to where it was when it was collapsed.
		// 
		// SSP 8/24/09 - NAS9.2 Field chooser - TFS19140
		// Replaced _isCollapsed with _visibility since undo/redo history needs
		// to store the hidden vs. collapsed state of a field because now the
		// field chooser allows one to unhide a Visibility.Hidden field.
		// 
		//internal bool _isCollapsed;
		private Visibility _visibility = Visibility.Visible;

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        // We need to know the fixed location for a layout item so we know whether its state 
        // is changed and so that the layoutinfo can appropriately handle adjusting the 
        // offsets when pushing items over.
        //
        internal FixedFieldLocation _fixedLocation;

		// SSP 9/2/09 TFS17893
		// _explicitSetPositionTimeStamp is updated whenever field's ActualPosition property is set.
		// 
		private static int g_explicitSetPositionTimeStampCounter;
		internal int _explicitSetPositionTimeStamp;

		#endregion // Member Vars

		#region Constructor

		internal ItemLayoutInfo( )
			: this( 0, 0 )
		{
		}

		internal ItemLayoutInfo( int column, int row )
			: this( column, row, 1, 1 )
		{
		}

		internal ItemLayoutInfo( int column, int row, int columnSpan, int rowSpan )
		{
			this.Column = column;
			this.Row = row;
			this.ColumnSpan = columnSpan;
			this.RowSpan = rowSpan;
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Row

		public int Row
		{
			get
			{
				return _row;
			}
			set
			{
				GridUtilities.ValidateNonNegative( value );

				_row = value;
			}
		}

		#endregion // Row

		#region Column

		public int Column
		{
			get
			{
				return _column;
			}
			set
			{
				GridUtilities.ValidateNonNegative( value );

				_column = value;
			}
		}

		#endregion // Column

		#region RowSpan

		public int RowSpan
		{
			get
			{
				return _rowSpan;
			}
			set
			{
				GridUtilities.ValidateNonNegative( value );

				_rowSpan = value;
			}
		}

		#endregion // RowSpan

		#region ColumnSpan

		public int ColumnSpan
		{
			get
			{
				return _columnSpan;
			}
			set
			{
				GridUtilities.ValidateNonNegative( value );

				_columnSpan = value;
			}
		}

		#endregion // ColumnSpan

		#region RowBottom

		
		
		/// <summary>
		/// Returns the bottom most logical row.
		/// </summary>
		internal int RowBottom
		{
			get
			{
				return _row + _rowSpan - 1;
			}
		}

		#endregion // RowBottom

		#region ColumnRight

		
		
		/// <summary>
		/// Returns the right most logical column.
		/// </summary>
		internal int ColumnRight
		{
			get
			{
				return _column + _columnSpan - 1;
			}
		}

		#endregion // ColumnRight

		#region IsCollapsed

		// SSP 8/24/09 - NAS9.2 Field chooser - TFS19140
		// Replaced _isCollapsed with _visibility since undo/redo history needs
		// to store the hidden vs. collapsed state of a field because now the
		// field chooser allows one to unhide a Visibility.Hidden field.
		// 
		/// <summary>
		/// Returns true if the Visibility property is not Collapsed. Flase otherwise.
		/// </summary>
		internal bool IsCollapsed
		{
			get
			{
				return Visibility.Collapsed == _visibility;
			}
			set
			{
				if ( this.IsCollapsed != value )
				{
					_visibility = value ? Visibility.Collapsed : Visibility.Visible;
				}
			}
		}

		#endregion // IsCollapsed

		#region Visibility

		// SSP 8/24/09 - NAS9.2 Field chooser - TFS19140
		// Replaced _isCollapsed with _visibility since undo/redo history needs
		// to store the hidden vs. collapsed state of a field because now the
		// field chooser allows one to unhide a Visibility.Hidden field.
		// 
		/// <summary>
		/// Gets or sets the Visibility.
		/// </summary>
		internal Visibility Visibility
		{
			get
			{
				return _visibility;
			}
			set
			{
				_visibility = value;
			}
		}

		#endregion // Visibility

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Base Overrides

		#region Equals

		public static bool AreEqual( ItemLayoutInfo x, ItemLayoutInfo y )
		{
			return x == y || null != x && null != y
				&& x._column == y._column && x._row == y._row
				&& x._columnSpan == y._columnSpan && x._rowSpan == y._rowSpan 
                // AS 1/21/09 NA 2009 Vol 1 - Fixed Fields
                // We should also consider the fixed location and collapsed state.
                //
				// SSP 8/24/09 - NAS9.2 Field chooser - TFS19140
				// 
                //&& x._fixedLocation == y._fixedLocation && x._isCollapsed == y._isCollapsed;
				&& x._fixedLocation == y._fixedLocation && x.Visibility == y.Visibility;
		}

		#endregion // Equals

		#region ToString


#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

		#endregion //ToString

		#endregion // Base Overrides

		#region Public Methods

		#region Clone

		public ItemLayoutInfo Clone( )
		{
			return (ItemLayoutInfo)this.MemberwiseClone( );
		}

		#endregion // Clone

		#region Contains

		
		
		public bool Contains( int column, int row )
		{
			return LayoutInfo.Intersects( _column, _columnSpan, column, 1 )
				&& LayoutInfo.Intersects( _row, _rowSpan, row, 1 );
		}

		#endregion // Contains

		#region Create

		// SSP 9/2/09 TFS17893
		// Added Create method.
		// 
		/// <summary>
		/// Creates a new instance of ItemLayoutInfo with the specified position and 
		/// field's visibility and fixed location property values.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		public static ItemLayoutInfo Create( Field field, FieldPosition pos )
		{
			ItemLayoutInfo info = new ItemLayoutInfo( pos.Column, pos.Row, pos.ColumnSpan, pos.RowSpan );
			info.Visibility = field.VisibilityInCellArea;
			info._fixedLocation = field.FixedLocation;

			return info;
		}

		#endregion // Create

		#region HasSamePosition

		// SSP 6/29/09 - NAS9.2 Field Chooser
		// 

		/// <summary>
		/// Returns true if the column, row, colum span and row span are the same in
		/// the specified ItemLayoutInfo instances.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static bool HasSamePosition( ItemLayoutInfo x, ItemLayoutInfo y )
		{
			return x._column == y._column && x._row == y._row
				&& x._columnSpan == y._columnSpan && x._rowSpan == y._rowSpan;
		}

		// AS 3/15/11 TFS65358
		public bool HasSamePosition(Field.FieldGridPosition position)
		{
			return position != null &&
				position.Column == _column &&
				position.Row == _row &&
				position.ColumnSpan == _columnSpan &&
				position.RowSpan == _rowSpan;
		}
		#endregion // HasSamePosition

		#region SetPosition

		// SSP 9/2/09 TFS17893
		// Added Create method.
		// 
		public void SetPosition( FieldPosition position )
		{
			this.Column = position.Column;
			this.Row = position.Row;
			this.ColumnSpan = position.ColumnSpan;
			this.RowSpan = position.RowSpan;
		}

		#endregion // SetPosition

		#endregion // Public Methods

		#region Private/Internal Methods

		#region HasSameRowColumn

		// SSP 1/12/10 TFS25122
		// 
		internal static bool HasSameRowColumn( ItemLayoutInfo x, ItemLayoutInfo y )
		{
			return null != x && null != y && x._column == y._column && x._row == y._row;
		}

		#endregion // HasSameRowColumn

		#region UpdateExplicitSetPositionTimeStamp

		// SSP 9/2/09 TFS17893
		// 
		internal void UpdateExplicitSetPositionTimeStamp( )
		{
			_explicitSetPositionTimeStamp = ++g_explicitSetPositionTimeStampCounter;
		}

		#endregion // UpdateExplicitSetPositionTimeStamp

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // ItemLayoutInfo Class

	#region LayoutInfo

	
	
	
	
	
	internal class LayoutInfo
	{
		#region Nested Data Structures

		#region CollapsedItemsPositionManager Class

		// SSP 1/12/10 TFS25122
		// Added CollapsedItemsPositionManager class.
		// 
		internal class CollapsedItemsPositionManager
		{
			#region Nested Data Structures

			#region ItemInfo Class

			private class ItemInfo
			{
				#region Member Vars

				internal ItemLayoutInfo _origItemInfo;
				internal ItemLayoutInfo _newItemInfo;
				internal Field _field;

				#endregion // Member Vars

				#region Constructor

				internal ItemInfo( ItemLayoutInfo origItemInfo, ItemLayoutInfo newItemInfo, Field field )
				{
					_origItemInfo = origItemInfo;
					_newItemInfo = newItemInfo;
					_field = field;
				}

				#endregion // Constructor

				#region HasPositionChanged

				internal bool HasPositionChanged
				{
					get
					{
						return !ItemLayoutInfo.HasSamePosition( _origItemInfo, _newItemInfo );
					}
				}

				#endregion // HasPositionChanged

				#region DeltaColumn

				internal int DeltaColumn
				{
					get
					{
						return _origItemInfo.Column - _newItemInfo.Column;
					}
				}

				#endregion // DeltaColumn

				#region DeltaRow

				internal int DeltaRow
				{
					get
					{
						return _origItemInfo.Row - _newItemInfo.Row;
					}
				}

				#endregion // DeltaRow
			}

			#endregion // ItemInfo Class

			#endregion // Nested Data Structures

			#region Member Vars

			private List<ItemInfo> _items = new List<ItemInfo>( );
			private ItemInfo _dragItemInfo;
			private bool _onlyVisibilityChanged = false;
			// SSP 2/24/10 TFS28070
			// 
			private LayoutInfo _newLayoutInfo;
			private bool _verticalLayout;

			#endregion // Member Vars

			#region Constructor

			internal CollapsedItemsPositionManager( LayoutInfo newLayoutInfo, Field dragField, bool onlyVisibilityChanged )
			{
				FieldLayout fieldLayout = null != newLayoutInfo ? newLayoutInfo._fieldLayout : null;
				LayoutInfo origLayoutInfo = null != fieldLayout ? fieldLayout.GetFieldLayoutInfo( true, false ) : null;
				_onlyVisibilityChanged = onlyVisibilityChanged;

				// SSP 2/24/10 TFS28070
				// When in card-view, we need to position unhidden fields below, and not to the right.
				// 
				_newLayoutInfo = newLayoutInfo;
				_verticalLayout = null != newLayoutInfo
					&& AutoArrangeCells.TopToBottom == newLayoutInfo._fieldLayout.AutoArrangeCellsResolved;

				if ( null != newLayoutInfo && null != origLayoutInfo )
				{
					foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in origLayoutInfo._info )
					{
						Field field = ii.Key;
						ItemLayoutInfo origItemInfo = ii.Value;
						ItemLayoutInfo newItemInfo = newLayoutInfo[field];
						if ( null == newItemInfo )
						{
							newItemInfo = origItemInfo.Clone( );
							newItemInfo.IsCollapsed = true;
							newLayoutInfo[field] = newItemInfo;
						}

						ItemInfo info = new ItemInfo( origItemInfo, newItemInfo, field );
						_items.Add( info );

						if ( field == dragField )
							_dragItemInfo = info;
					}
				}
			}

			#endregion // Constructor

			#region GetItemPrevNext

			/// <summary>
			/// Gets the previous or next item in the logical column or row based on the _verticalLayout member var.
			/// </summary>
			/// <returns>Index of the item.</returns>
			private int GetItemPrevNext( List<ItemInfo> list, int index, bool prev )
			{
				int count = list.Count;
				ItemInfo itemAtIndex = list[index];
				int step = prev ? -1 : 1;

				index += step;
				while ( index >= 0 && index < count )
				{
					ItemInfo ii = list[index];

					// SSP 2/24/10 TFS28070
					// 
					//if ( ii._origItemInfo.Row != itemAtIndex._origItemInfo.Row )
					if ( _verticalLayout
						? ii._origItemInfo.Column != itemAtIndex._origItemInfo.Column
						: ii._origItemInfo.Row != itemAtIndex._origItemInfo.Row )
						break;

					if ( !ii._origItemInfo.IsCollapsed && ii != _dragItemInfo )
						return index;

					index += step;
				}

				return -1;
			}

			#endregion // GetItemPrevNext

			#region OffsetCollapsedItems

			internal void OffsetCollapsedItems( )
			{
				Debug.Assert( null != _dragItemInfo );
				if ( null != _dragItemInfo )
				{
					List<ItemInfo> list = _items;

					// Sort the list by original Row and then Column values of items.
					// 
					Utilities.SortMergeGeneric<ItemInfo>( list,
						Utilities.CreateComparer<ItemInfo>( new Comparison<ItemInfo>(
								delegate( ItemInfo x, ItemInfo y )
								{
									int r;

									// SSP 2/24/10 TFS28070
									// In card-view, use the row instead of column values. Added if block and
									// and put the existing condition in the else block.
									// 
									if ( _verticalLayout )
									{
										r = x._origItemInfo.Column.CompareTo( y._origItemInfo.Column );
										if ( 0 == r )
											r = x._origItemInfo.Row.CompareTo( y._origItemInfo.Row );
									}
									else
									{
										r = x._origItemInfo.Row.CompareTo( y._origItemInfo.Row );
										if ( 0 == r )
											r = x._origItemInfo.Column.CompareTo( y._origItemInfo.Column );
									}

									if ( 0 == r )
										r = x._field.Index.CompareTo( y._field.Index );

									return r;
								}
						) )
					);

					int dragItemIndex = list.IndexOf( _dragItemInfo );

					// Adjust the position of each collapsed item according to the movement of the 
					// item left of or the right of the collapsed item.
					// 
					for ( int i = 0, count = list.Count; i < count; i++ )
					{
						ItemInfo ii = list[i];
						if ( ii != _dragItemInfo && ii._origItemInfo.IsCollapsed
							&& ItemLayoutInfo.AreEqual( ii._origItemInfo, ii._newItemInfo ) )
						{
							bool newPositionApplied = false;

							int itemBeforeIndex = GetItemPrevNext( list, i, true );
							int itemAfterIndex = GetItemPrevNext( list, i, false );
							ItemInfo itemBefore = itemBeforeIndex >= 0 ? list[itemBeforeIndex] : null;
							ItemInfo itemAfter = itemAfterIndex >= 0 ? list[itemAfterIndex] : null;

							// If an item was simply made visible and not moved then any other collapsed
							// items occuping the same position have to be adjusted based on whether they
							// occur before the item made visible or after.
							// 
							if ( _onlyVisibilityChanged )
							{
								// If the collapsed item was at the same location as the item that was made
								// visible.
								// 
								if ( ItemLayoutInfo.HasSamePosition( _dragItemInfo._origItemInfo, ii._origItemInfo ) )
								{
									if ( ( itemBeforeIndex < 0 || dragItemIndex >= itemBeforeIndex )
										&& ( itemAfterIndex < 0 || dragItemIndex <= itemAfterIndex ) )
									{
										// SSP 2/24/10 TFS28070
										// When in card-view, we need to position unhidden fields below, and not to the right.
										// 
										// ------------------------------------------------------------------------------------
										if ( _verticalLayout )
										{
											int newLocation = i > dragItemIndex ? _dragItemInfo._newItemInfo.RowBottom + 1 : _dragItemInfo._newItemInfo.Row;
											ii._newItemInfo.Row = Math.Max( 0, newLocation );
										}
										else
										{
											int newLocation = i > dragItemIndex ? _dragItemInfo._newItemInfo.ColumnRight + 1 : _dragItemInfo._newItemInfo.Column;
											ii._newItemInfo.Column = Math.Max( 0, newLocation );
										}
										
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

										// ------------------------------------------------------------------------------------

										newPositionApplied = true;
									}
								}

								if ( !newPositionApplied )
								{
									int deltaColumn = 0, deltaRow = 0;

									if ( null != itemBefore && ItemLayoutInfo.HasSamePosition( ii._origItemInfo, itemBefore._origItemInfo ) )
									{
										deltaColumn = itemBefore.DeltaColumn;
										deltaRow = itemBefore.DeltaRow;
									}
									else if ( null != itemAfter && ItemLayoutInfo.HasSamePosition( ii._origItemInfo, itemAfter._origItemInfo ) )
									{
										deltaColumn = itemAfter.DeltaColumn;
										deltaRow = itemAfter.DeltaRow;
									}
									else if ( null != itemBefore && null != itemAfter
										&& itemBefore.DeltaRow == itemAfter.DeltaRow
										&& itemBefore.DeltaColumn == itemAfter.DeltaColumn
										&&
										(
											// SSP 2/24/10 TFS28070
											// In card-view, use the row instead of column values. Added if block and
											// and put the existing condition in the else block.
											// 
											_verticalLayout
											? Math.Abs( itemBefore._origItemInfo.Row - itemAfter._origItemInfo.Row )
												<= Math.Abs( itemBefore._newItemInfo.Row - itemAfter._newItemInfo.Row )
											: Math.Abs( itemBefore._origItemInfo.Column - itemAfter._origItemInfo.Column )
												<= Math.Abs( itemBefore._newItemInfo.Column - itemAfter._newItemInfo.Column )
										) )
									{
										deltaColumn = itemBefore.DeltaColumn;
										deltaRow = itemBefore.DeltaRow;
									}
									else 
									{
										ItemInfo tmp = null != itemAfter ? itemAfter : itemBefore;
										if ( null != itemBefore && null != itemAfter )
										{
											if (
												// SSP 2/24/10 TFS28070
												// In card-view, use the row instead of column values. Added if block and
												// and put the existing condition in the else block.
												// 
												_verticalLayout
												? Math.Abs( ii._origItemInfo.Row - itemBefore._origItemInfo.Row ) < Math.Abs( itemAfter._origItemInfo.Row - ii._origItemInfo.Row )
												: Math.Abs( ii._origItemInfo.Column - itemBefore._origItemInfo.Column ) < Math.Abs( itemAfter._origItemInfo.Column - ii._origItemInfo.Column ) 
												)
												tmp = itemBefore;
										}

										// SSP 2/24/10 TFS28070
										// In card-view, use the row instead of column values. Added if block and
										// and enclosed the existing condition in the else block.
										// 
										if ( _verticalLayout )
										{
											if ( null != tmp && tmp.DeltaColumn == ii.DeltaColumn )
												deltaRow = tmp.DeltaRow;
										}
										else
										{
											if ( null != tmp && tmp.DeltaRow == ii.DeltaRow )
												deltaColumn = tmp.DeltaColumn;
										}
									}

									ii._newItemInfo.Column = Math.Max( 0, ii._origItemInfo.Column - deltaColumn );
									ii._newItemInfo.Row = Math.Max( 0, ii._origItemInfo.Row - deltaRow );
								}
							}
						}
					}
				}
			}

			#endregion // OffsetCollapsedItems
		}

		#endregion // CollapsedItemsPositionManager Class

		#region OriginComparer Class

		internal class OriginComparer : IComparer<ItemLayoutInfo>, IComparer
		{
			private bool _rowWise;
			private ItemLayoutInfo _dragItem;

			public OriginComparer( bool rowWise, ItemLayoutInfo dragItem )
			{
				_rowWise = rowWise;
				_dragItem = dragItem;
			}

			public int Compare( object x, object y )
			{
				return this.Compare( (ItemLayoutInfo)x, (ItemLayoutInfo)y );
			}

			public int Compare( ItemLayoutInfo x, ItemLayoutInfo y )
			{
				int r = _rowWise
					? x.Row.CompareTo( y.Row )
					: x.Column.CompareTo( y.Column );

				// This makes sure that the drag item is before other items.
				// 
				if ( 0 == r && x != y )
				{
					if ( x == _dragItem )
						r = -1;
					else if ( y == _dragItem )
						r = 1;
					else
						r = _rowWise
							? x.RowSpan.CompareTo( y.RowSpan )
							: x.ColumnSpan.CompareTo( y.ColumnSpan );
				}

				return r;
			}
		}

		#endregion // OriginComparer Class

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region FixedLocationComparer
        private class FixedLocationComparer : IComparer<ItemLayoutInfo>
        {
            private bool _byRow;

            internal FixedLocationComparer(bool byRow)
            {
                _byRow = byRow;
            }

            #region IComparer<ItemLayoutInfo> Members

            int IComparer<ItemLayoutInfo>.Compare(ItemLayoutInfo x, ItemLayoutInfo y)
            {
                // sort by fixed locations first
                int comparison = GridUtilities.Compare(x._fixedLocation, y._fixedLocation);

                if (0 == comparison)
                {
                    // then sort by column/row
                    comparison = _byRow
                        ? x.Row.CompareTo(y.Row)
                        : x.Column.CompareTo(y.Column);

                    if (0 == comparison)
                    {
                        comparison = _byRow
                            ? x.RowSpan.CompareTo(y.RowSpan)
                            : x.ColumnSpan.CompareTo(y.ColumnSpan);
                    }
                }

                return comparison;
            }

            #endregion
        } 
        #endregion //FixedLocationComparer

		#endregion // Nested Data Structures

		#region Member Vars

		private FieldLayout _fieldLayout;
		private Dictionary<Field, ItemLayoutInfo> _info;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fieldLayout"></param>
		internal LayoutInfo( FieldLayout fieldLayout )
		{
			GridUtilities.ValidateNotNull( fieldLayout );

			_fieldLayout = fieldLayout;
			_info = new Dictionary<Field, ItemLayoutInfo>( );
		}

		#endregion // Constructor

		#region Methods

		#region Private/Internal Methods

		#region CalculatePositionOfNewField

		private static void SetMaxValue( int[] arr, int startIndex, int count, int value )
		{
			for ( int i = startIndex, endIndex = startIndex + count - 1; i <= endIndex; i++ )
				arr[i] = Math.Max( arr[i], value );
		}

		/// <summary>
		/// Figures out where to position any new field that gets added.
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		private ItemLayoutInfo CalculatePositionOfNewField( Field field )
		{
			int colCount, rowCount;
			this.GetLogicalRowAndColumnCount( out colCount, out rowCount );

			int[] highestRow = new int[colCount];
			int[] highestCol = new int[rowCount];

			foreach ( ItemLayoutInfo ii in _info.Values )
			{
				SetMaxValue( highestRow, ii.Column, ii.ColumnSpan, ii.Row + ii.RowSpan - 1 );
				SetMaxValue( highestCol, ii.Row, ii.RowSpan, ii.Column + ii.ColumnSpan - 1 );
			}

			for ( int row = 0; row < rowCount; row++ )
			{
				for ( int col = 0; col < colCount; col++ )
				{
					if ( col > highestCol[row] && row > highestRow[col] )
						return new ItemLayoutInfo( col, row );
				}
			}

			return new ItemLayoutInfo( colCount, 0 );
		}

		#endregion // CalculatePositionOfNewField

		#region Clone

		internal LayoutInfo Clone( )
		{
			return this.Clone( false );
		}

		// SSP 6/26/09 - NAS9.2 Field Chooser
		// Added an overload that takes in excludeCollapsedEntries parameter.
		// 
		internal LayoutInfo Clone( bool excludeCollapsedEntries )
		{
			LayoutInfo clone = new LayoutInfo( _fieldLayout );

			foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in _info )
			{
				// SSP 6/26/09 - NAS9.2 Field Chooser
				// Added an overload that takes in excludeCollapsedEntries parameter.
				// 
				// AS 8/11/09 NA 2009.2
				// We should also exclude any entries that are not visible in the layout.
				// This is needed because the layout info may contain autogenerated positions 
				// in which case it may include items not in the layout but not visible 
				// either - e.g. expandableresolved = true
				//
				//if ( excludeCollapsedEntries && ii.Value._isCollapsed )
				// SSP 8/24/09 - TFS19187, TFS19273
				// With the fix that I did for these two bugs where now we are ensuring that 
				// _isCollapsed is set to the correct value based on field's IsVisibleInCellArea
				// (which we weren't doing some cases), I'm pulling this change out. The reason
				// is that we could be cloning a LayoutInfo instance from a point in history (for
				// undo/redo stuff) and therefore it's possible for the _isCollapsed to be different
				// from the current IsVisibleInCellArea state of the field.
				// 
				//if ( excludeCollapsedEntries && (ii.Value._isCollapsed || !ii.Key.IsVisibleInCellArea) )
				if ( excludeCollapsedEntries && ii.Value.IsCollapsed )
					continue;

				clone._info.Add( ii.Key, ii.Value.Clone( ) );
			}

			return clone;
		}

		#endregion // Clone

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region Create
        internal static LayoutInfo Create(FieldLayout fieldLayout)
        {
            LayoutInfo layoutInfo = new LayoutInfo(fieldLayout);

            foreach (Field field in fieldLayout.Fields)
            {
                if (!field.IsVisibleInCellArea)
                    continue;

                Field.FieldGridPosition gridPos = field.GridPosition;

                ItemLayoutInfo info = new ItemLayoutInfo(
                    gridPos.Column,
                    gridPos.Row,
                    gridPos.ColumnSpan,
                    gridPos.RowSpan);

                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                info._fixedLocation = field.FixedLocation;

                layoutInfo.Add(field, info);
            }

            return layoutInfo;
        } 
        #endregion //Create

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region DebugFixedLocations
        [Conditional("DEBUG")]
        internal void DebugFixedLocations()
        {
            int rowCount, colCount;
            this.GetLogicalRowAndColumnCount(out colCount, out rowCount);
            ItemLayoutInfo[,] cells = new ItemLayoutInfo[colCount, rowCount];

            foreach (KeyValuePair<Field, ItemLayoutInfo> item in _info)
            {
                ItemLayoutInfo layoutItem = item.Value;

                if (layoutItem.IsCollapsed)
                    continue;

                for (int c = layoutItem.Column, lastC = c + layoutItem.ColumnSpan; c < lastC; c++)
                {
                    for (int r = layoutItem.Row, lastR = r + layoutItem.RowSpan; r < lastR; r++)
                    {
                        Debug.Assert(cells[c, r] == null, string.Format("Another field exists at the specified spot. Field:{0}, Row:{1}, Col:{2}", item.Key, r, c));
                        cells[c, r] = layoutItem;
                    }
                }
            }

            // make sure we don't have overlapping fixed locations
            bool isHorz = _fieldLayout.IsHorizontal;

            int outerCount = cells.GetLength(isHorz ? 1 : 0);
            int innerCount = cells.GetLength(isHorz ? 0 : 1);

            for (int i = 0; i < outerCount; i++)
            {
                ItemLayoutInfo firstField = null;

                for (int j = 0; j < innerCount; j++)
                {
                    ItemLayoutInfo fld = isHorz ? cells[j, i] : cells[i, j];

                    if (fld == firstField)
                        continue;
                    else if (firstField == null)
                        firstField = fld;
                    else if (fld != null)
                        Debug.Assert(fld._fixedLocation == firstField._fixedLocation, "The layout items should have the same fixed location");
                }
            }
        }
        #endregion //DebugFixedLocations

		#region DoesItemOverlap

		/// <summary>
		/// Checks to see if the specified item overlaps with any other item in the items collection.
		/// </summary>
		/// <param name="item">Item to check if it overlaps with any other item.</param>
		/// <param name="ignoreItem">This item will be ignored and not checked for overlap.</param>
		/// <returns></returns>
		internal bool DoesItemOverlap( ItemLayoutInfo item, ItemLayoutInfo ignoreItem )
		{
			return DoesItemOverlap( item, _info.Values, ignoreItem );
		}

		/// <summary>
		/// Checks to see if the specified item overlaps with any other item in the items collection.
		/// </summary>
		/// <param name="item">Item to check if it overlaps with any other item.</param>
		/// <param name="items">Collection of items.</param>
		/// <param name="ignoreItem">This item will be ignored and not checked for overlap.</param>
		/// <returns></returns>
		private static bool DoesItemOverlap( ItemLayoutInfo item, IEnumerable<ItemLayoutInfo> items, ItemLayoutInfo ignoreItem )
		{
			foreach ( ItemLayoutInfo ii in items )
			{
				// Skip the item itself and ignoreItem.
				// 
				if ( ii == item || ii == ignoreItem )
					continue;

				if ( DoItemsOverlap( ii, item ) )
					return true;
			}

			return false;
		}

		#endregion // DoesItemOverlap

		#region DoItemsOverlap

		private static bool DoItemsOverlap( ItemLayoutInfo xx, ItemLayoutInfo yy )
		{
			return
				xx != null && yy != null && // AS 3/23/10 TFS29701
				Intersects( xx.Column, xx.ColumnSpan, yy.Column, yy.ColumnSpan ) &&
				Intersects( xx.Row, xx.RowSpan, yy.Row, yy.RowSpan );
		}

		#endregion // DoItemsOverlap

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region EnsureFixedLocationsDontOverlap
        /// <summary>
        /// Helper routine to adjust the origins of items such that items from one fixed 
        /// area do not overlap with that of another.
        /// </summary>
		internal void EnsureFixedLocationsDontOverlap()
        {
            bool byRow = _fieldLayout.IsHorizontal;
            List<ItemLayoutInfo> items = new List<ItemLayoutInfo>(_info.Values);
            items.Sort(new FixedLocationComparer(byRow));

            int offset = 0;
            int nextOrigin = -1;
            FixedFieldLocation fixedLocation = FixedFieldLocation.FixedToNearEdge;

            foreach (ItemLayoutInfo item in items)
            {
                int origin = byRow ? item.Row : item.Column;

                if (fixedLocation != item._fixedLocation)
                {
                    fixedLocation = item._fixedLocation;

                    if (origin < nextOrigin)
                    {
                        fixedLocation = item._fixedLocation;
                        offset += nextOrigin - origin;
                    }
                }

                if (byRow)
                    item.Row += offset;
                else
                    item.Column += offset;

                nextOrigin = Math.Max(nextOrigin, byRow ? item.Row + item.RowSpan : item.Column + item.ColumnSpan);
            }
        }
        #endregion //EnsureFixedLocationsDontOverlap

        // AS 1/21/09 NA 2009 Vol 1 - Fixed Fields
        #region EnsureFixedLocationsInSync
        internal void EnsureFixedLocationsInSync()
        {
            List<Field> changedItems = new List<Field>();

            foreach (KeyValuePair<Field, ItemLayoutInfo> pair in _info)
            {
                if (pair.Key.FixedLocation != pair.Value._fixedLocation)
                    changedItems.Add(pair.Key);
            }

            if (changedItems.Count > 0)
            {
                Array positions = Enum.GetValues(typeof(FixedFieldLocation));
                List<Field> fields = new List<Field>();

                for (int oldIdx = 0; oldIdx < positions.Length; oldIdx++)
                {
                    for (int newIdx = 0; newIdx < positions.Length; newIdx++)
                    {
                        if (newIdx != oldIdx)
                        {
                            FixedFieldLocation oldLocation = (FixedFieldLocation)positions.GetValue(oldIdx);
                            FixedFieldLocation newLocation = (FixedFieldLocation)positions.GetValue(newIdx);
                            fields.Clear();

                            for (int i = changedItems.Count - 1; i >= 0; i--)
                            {
                                Field field = changedItems[i];

                                if (field.FixedLocation == newLocation)
                                {
                                    if (_info[field]._fixedLocation == oldLocation)
                                    {
                                        // remove it from the list so we have less to process in the next iteration
                                        changedItems.RemoveAt(i);

                                        fields.Add(field);
                                    }
                                }
                            }

                            if (fields.Count > 0)
                                UpdateFixedLocation(oldLocation, newLocation, fields);
                        }
                    }
                }
            }
        }
        #endregion //EnsureFixedLocationsInSync

        #region EnsureItemDoesntOverlap

		
		
		
		internal void EnsureItemDoesntOverlap( Field field )
		{
			this.EnsureItemDoesntOverlap( this[field], true, true, false );
		}

        internal void EnsureItemDoesntOverlap(
                ItemLayoutInfo item,
                bool shiftRight,
                bool shiftBelow,
                bool shiftBelowFirst
            )
        {
            EnsureItemDoesntOverlapImpl(item, shiftRight, shiftBelow, shiftBelowFirst);

            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
            EnsureFixedLocationsDontOverlap();
        }

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        // Added a layer in between so we can do fix ups for the fixed locations.
        //
		private void EnsureItemDoesntOverlapImpl(
				ItemLayoutInfo item,
				bool shiftRight,
				bool shiftBelow,
				bool shiftBelowFirst
			)
		{
			if ( shiftRight && shiftBelow && shiftBelowFirst )
			{
				this.EnsureItemDoesntOverlap( item, false, true, false );
				this.EnsureItemDoesntOverlap( item, true, false, false );
				return;
			}

			List<ItemLayoutInfo> items = new List<ItemLayoutInfo>( _info.Values );

			if ( shiftRight )
			{
				items.Sort( new OriginComparer( false, item ) );

				int row = item.Row;
				int rowSpan = item.RowSpan;

				int delta = 0;
				int itemIndex = items.IndexOf( item );

				for ( int i = 1 + itemIndex; i < items.Count; i++ )
				{
					ItemLayoutInfo ii = items[i];

					if ( DoItemsOverlap( item, ii ) )
					{
						delta = Math.Max( delta, item.Column + item.ColumnSpan - ii.Column );
					}
				}

				// Shift by no more than the drag item's span.
				// 
				delta = Math.Min( delta, item.ColumnSpan );

				if ( delta > 0 )
				{
					for ( int i = 1 + itemIndex; i < items.Count; i++ )
					{
						ItemLayoutInfo ii = items[i];

                        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                        // Do not shift over items in another area. Instead we will do
                        // a final pass after this routine to account for this.
                        //
                        if (ii._fixedLocation != item._fixedLocation)
                            continue;

						if ( !DoesItemOverlap( ii, items, null ) )
							continue;

						if ( Intersects( row, rowSpan, ii.Row, ii.RowSpan ) )
						{
							int newRow = Math.Min( row, ii.Row );
							rowSpan = Math.Max( row + rowSpan, ii.Row + ii.RowSpan ) - newRow;
							row = newRow;

							ii.Column += delta;
						}
					}
				}
			}

			if ( shiftBelow )
			{
				items.Sort( new OriginComparer( true, item ) );

				int col = item.Column;
				int colSpan = item.ColumnSpan;

				int delta = 0;
				int itemIndex = items.IndexOf( item );

				for ( int i = 1 + itemIndex; i < items.Count; i++ )
				{
					ItemLayoutInfo ii = items[i];

					if ( DoItemsOverlap( item, ii ) )
					{
						delta = Math.Max( delta, item.Row + item.RowSpan - ii.Row );
					}
				}

				// Shift by no more than the drag item's span.
				// 
				delta = Math.Min( delta, item.RowSpan );

				if ( delta > 0 )
				{
					for ( int i = 1 + itemIndex; i < items.Count; i++ )
					{
						ItemLayoutInfo ii = items[i];

                        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                        // Do not shift over items in another area. Instead we will do
                        // a final pass after this routine to account for this.
                        //
                        if (ii._fixedLocation != item._fixedLocation)
                            continue;

                        if (!DoesItemOverlap(ii, items, null))
							continue;

						if ( Intersects( col, colSpan, ii.Column, ii.ColumnSpan ) )
						{
							int newCol = Math.Min( col, ii.Column );
							colSpan = Math.Max( col + colSpan, ii.Column + ii.ColumnSpan ) - newCol;
							col = newCol;

							ii.Row += delta;
						}
					}
				}
			}
		}

		#endregion // EnsureItemDoesntOverlap

		#region EnsureItemsDontOverlap

		
		
		
		/// <summary>
		/// Ensures items don't overlap. Returns true if the layout was changed.
		/// </summary>
		/// <returns>True if any items were changed, false otherwise.</returns>
		internal bool EnsureItemsDontOverlap( )
		{
			Dictionary<Field, ItemLayoutInfo> originalPositions = new Dictionary<Field, ItemLayoutInfo>( );

			foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in _info )
				// SSP 12/23/09 TFS25122
				// We need to clone the values.
				// 
				//originalPositions[ii.Key] = ii.Value;
				originalPositions[ii.Key] = ii.Value.Clone( );

			ItemLayoutInfo[] itemsArr = GridUtilities.ToArray<ItemLayoutInfo>( _info.Values );
			Array.Sort<ItemLayoutInfo>( itemsArr, new Comparison<ItemLayoutInfo>(  
				delegate ( ItemLayoutInfo x, ItemLayoutInfo y )
				{
					return - x._explicitSetPositionTimeStamp.CompareTo( y._explicitSetPositionTimeStamp );
				}
			) );

			this.EnsureItemsDontOverlap( itemsArr, true, true, false );

			if ( originalPositions.Count != _info.Count )
				return true;
			
			foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in _info )
			{
				ItemLayoutInfo jj;
				if ( !originalPositions.TryGetValue( ii.Key, out jj )
					|| !ItemLayoutInfo.AreEqual( jj, ii.Value ) )
					return true;
			}

			return false;
		}

		internal void EnsureItemsDontOverlap(
				IEnumerable<ItemLayoutInfo> items,
				bool shiftRight,
				bool shiftBelow,
				bool shiftBelowFirst
			)
		{
			// Sort the items based on their x origins and/or y origins based on shift parameters.
			// 
			ItemLayoutInfo[] itemsArr = GridUtilities.ToArray<ItemLayoutInfo>( items );

			IComparer<ItemLayoutInfo>[] arr = new IComparer<ItemLayoutInfo>[]
					{
						new OriginComparer( false, null ),
						new OriginComparer( true, null )
					};

			if ( shiftBelowFirst )
				GridUtilities.Swap( arr, 0, 1 );

			Utilities.SortMergeGeneric( itemsArr, new GridUtilities.AggregateComparer<ItemLayoutInfo>( arr ) );

			// Make sure none of the items overlap with any other items.
			// 
			for ( int i = 0; i < itemsArr.Length; i++ )
				this.EnsureItemDoesntOverlap( itemsArr[i], shiftRight, shiftBelow, shiftBelowFirst );
		}

		#endregion // EnsureItemsDontOverlap

		#region EnsureNewItemsDontOverlapHelper

		/// <summary>
		/// Makes sure any new field that is added or an existing collapsed field that is made
		/// visible doesn't overlap with other fields. This also ensures that if such a field
		/// was previously collapsed, it appears where it was previously.
		/// </summary>
		internal void EnsureNewItemsDontOverlapHelper( )
		{
			// First make sure that any fields that were removed from the field layout are also removed
			// from this layout info data structure.
			// 
			this.RemoveDeletedFields( );

			HashSet itemsMadeVisible = new HashSet( );
			HashSet newFields = new HashSet( );

			if ( null != _fieldLayout._autoGeneratedPositions )
			{
				foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in _fieldLayout._autoGeneratedPositions._info )
				{
					if ( !_info.ContainsKey( ii.Key ) )
					{
						ItemLayoutInfo iiInfo = ii.Value.Clone( );
						iiInfo.IsCollapsed = true;
						_info[ii.Key] = iiInfo;
					}
				}

				_fieldLayout._autoGeneratedPositions = null;
			}

			// Figure out which fields were made visible and which new fields were added.
			// 
			FieldCollection fields = GridUtilities.GetFields( _fieldLayout );
			for ( int i = 0, count = fields.Count; i < count; i++ )
			{
				Field field = fields[i];
				ItemLayoutInfo itemInfo;
				_info.TryGetValue( field, out itemInfo );

				if ( field.IsVisibleInCellArea )
				{
					// If there's no ItemLayoutInfo for the field then it's a new field that was
					// added.
					// 
					if ( null == itemInfo )
						newFields.Add( field );
						// Otherwise the field was previously hidden.
						// 
					else if ( itemInfo.IsCollapsed )
						itemsMadeVisible.Add( itemInfo );
				}
			}

			if ( itemsMadeVisible.Count > 0 || newFields.Count > 0 )
			{
				LayoutInfo workLayoutInfo = new LayoutInfo( _fieldLayout );

				// Create a new LayoutInfo object that we can work with and modify.
				// 
				foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in _info )
				{
					Field field = ii.Key;
					ItemLayoutInfo iiInfo = ii.Value;

					// Copy items that are visible to the work layout info.
					// 
					if ( !iiInfo.IsCollapsed || itemsMadeVisible.Exists( iiInfo ) )
						workLayoutInfo[ field ] = iiInfo;
				}

				// Add entries for the newly added fields to the workLayoutInfo.
				// 
				foreach ( Field field in newFields )
				{
					ItemLayoutInfo fieldInfo = workLayoutInfo.CalculatePositionOfNewField( field );

                    // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                    fieldInfo._fixedLocation = field.FixedLocation;

					workLayoutInfo[ field ] = fieldInfo;
					itemsMadeVisible.Add( fieldInfo );
				}





				// Make sure that fields that were made visible, including new fields, don't overlap
				// with any other fields.
				// 
				workLayoutInfo.EnsureItemsDontOverlap(
					itemsMadeVisible.ToArray<ItemLayoutInfo>( ), true, true, false );

				// Copy over changes from workLayoutInfo.
				// 
				this.EnsureNewItemsDontOverlapHelper_CopyFromNewLayout( workLayoutInfo );
			}
		}

		internal void EnsureNewItemsDontOverlapHelper_CopyFromNewLayout( LayoutInfo newLayoutInfo )
		{
			// All the items in the new layout info are visible and therefore should be marked
			// _isCollapsed = false.
			// 
			foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in newLayoutInfo._info )
			{
				_info[ii.Key] = ii.Value;
				ii.Value.IsCollapsed = false;
			}

			// Items that are not in the new layout info are collapsed and therefore should be
			// marked as _isCollapsed = true.
			// 
			foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in _info )
			{
				if ( ! newLayoutInfo.ContainsKey( ii.Key ) )
					ii.Value.IsCollapsed = true;
			}

            this.DebugFixedLocations();
		}

		#endregion // EnsureNewItemsDontOverlapHelper

		#region GetItemAtLogicalCell

		private Field GetItemAtLogicalCell( int column, int row )
		{
			foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in _info )
			{
				ItemLayoutInfo iiInfo = ii.Value;
				if ( iiInfo.Contains( column, row ) )
					return ii.Key;
			}

			return null;
		}

		#endregion // GetItemAtLogicalCell

		#region GetLogicalColumnCount

		internal int GetLogicalColumnCount( )
		{
			int cols, rows;
			this.GetLogicalRowAndColumnCount( out cols, out rows );

			return cols;
		}

		#endregion // GetLogicalColumnCount

		#region GetLogicalRowAndColumnCount

		internal void GetLogicalRowAndColumnCount( out int columnCount, out int rowCount )
		{
			int maxCols = 0, maxRows = 0;

			foreach ( ItemLayoutInfo ii in _info.Values )
			{
				maxCols = Math.Max( maxCols, ii.Column + ii.ColumnSpan );
				maxRows = Math.Max( maxRows, ii.Row + ii.RowSpan );
			}

			columnCount = maxCols;
			rowCount = maxRows;
		}

		#endregion // GetLogicalRowAndColumnCount

		#region GetLogicalRowCount

		internal int GetLogicalRowCount( )
		{
			int cols, rows;
			this.GetLogicalRowAndColumnCount( out cols, out rows );

			return rows;
		}

		#endregion // GetLogicalRowCount

		#region HasItemAtLogicalCell

		private bool HasItemAtLogicalCell( int column, int row )
		{
			return null != this.GetItemAtLogicalCell( column, row );
		}

		#endregion // HasItemAtLogicalCell

        // AS 1/21/09 NA 2009 Vol 1 - Fixed Fields
        #region HasOverlappingItem
        internal bool HasOverlappingItem(int column, int row, int columnSpan, int rowSpan, ItemLayoutInfo itemToIgnore, FixedFieldLocation? location)
        {
            foreach (KeyValuePair<Field, ItemLayoutInfo> ii in _info)
            {
                ItemLayoutInfo iiInfo = ii.Value;

                if (iiInfo.IsCollapsed || iiInfo == itemToIgnore)
                    continue;

                if (location != null && location.Value != iiInfo._fixedLocation)
                    continue;

                if (Intersects(iiInfo.Column, iiInfo.ColumnSpan, column, columnSpan) &&
                    Intersects(iiInfo.Row, iiInfo.RowSpan, row, rowSpan))
                    return true;
            }

            return false;
        } 
        #endregion //HasOverlappingItem

        #region HasLogicalColumnOrRowChanged

		// SSP 6/26/09 - NAS9.2 Field Chooser
		// Added the overload that doesn't take in any parameter however instead returns a boolean
		// value.
		// 
		/// <summary>
		/// Checks to see if any of the items in the newInfo have different logical row or column value
		/// than the corresponding item in this layout info. Any items that exist in the new layout info
		/// but not in this layout info are ignored.
		/// </summary>
		/// <param name="newInfo"></param>
		internal bool HasLogicalColumnOrRowChanged( LayoutInfo newInfo )
		{
			bool logicalColumnChanged, logicalRowChanged;
			this.HasLogicalColumnOrRowChanged( newInfo, out logicalColumnChanged, out logicalRowChanged );

			return logicalColumnChanged || logicalRowChanged;
		}

		/// <summary>
		/// Checks to see if any of the items in the newInfo have different logical row or column value
		/// than the corresponding item in this layout info. Any items that exist in the new layout info
		/// but not in this layout info are ignored.
		/// </summary>
		/// <param name="newInfo"></param>
		/// <param name="logicalColumnChanged"></param>
		/// <param name="logicalRowChanged"></param>
        internal void HasLogicalColumnOrRowChanged(
			LayoutInfo newInfo,
			out bool logicalColumnChanged,
			out bool logicalRowChanged )
		{
			logicalColumnChanged = logicalRowChanged = false;

			foreach ( KeyValuePair<Field, ItemLayoutInfo> entry in _info )
			{
				ItemLayoutInfo o = entry.Value;
				// SSP 6/26/09 - NAS9.2 Field Chooser
				// Now we allow for this layout info and the newInfo to contain different 
				// set of entries. Only check the intersecting items.
				// 
				// ------------------------------------------------------------------------
				//ItemLayoutInfo n = newInfo[entry.Key];
				ItemLayoutInfo n;
				newInfo.TryGetValue( entry.Key, out n );
				if ( null == n )
					continue;
				// ------------------------------------------------------------------------

				if ( o.Row != n.Row )
					logicalRowChanged = true;

				if ( o.Column != n.Column )
					logicalColumnChanged = true;

				if ( logicalRowChanged && logicalColumnChanged )
					break;
			}
		}

		#endregion // HasLogicalColumnOrRowChanged

		// AS 3/15/11 TFS65358
		#region HasOverlappingFixedAreas
		/// <summary>
		/// Helper method to check all the non-collapsed items to ensure that the fixed areas don't overlap each other. This does not check if items within the areas overlap.
		/// </summary>
		/// <returns></returns>
		private bool HasOverlappingFixedAreas()
		{
			// this will be an array of 3 arrays of ints. the inner 
			// bound will be 2 - one for the earliest column and the 
			// other for the latest
			int[][] fixedAreaPositions = new int[3][];

			bool isHorizontal = _fieldLayout.IsHorizontal;

			foreach (var pair in _info)
			{
				var pos = pair.Value;

				if (pos.IsCollapsed)
					continue;

				// the order that we use isn't relevant since we just 
				// want to make sure the areas don't overlap so check 
				// an int range
				int index = (int)pair.Key.FixedLocation;
				var fixedArea = fixedAreaPositions[index];

				int start = isHorizontal ? pos.Row : pos.Column;
				int end = isHorizontal ? pos.RowBottom : pos.ColumnRight;

				if (fixedArea == null)
				{
					// if this is the first item in the area then just use the 
					// start/end for the item
					fixedArea = fixedAreaPositions[index] = new int[] { start, end };
				}
				else
				{
					// otherwise keep the earliest start and latest extent
					fixedArea[0] = Math.Min(fixedArea[0], start);
					fixedArea[1] = Math.Max(fixedArea[1], end);
				}
			}

			// now that we have the areas we can just see if they overlap
			for (int i = 0; i < fixedAreaPositions.Length - 1; i++)
			{
				var fixedArea = fixedAreaPositions[i];

				if (fixedArea != null)
				{
					for (int j = i + 1; j < fixedAreaPositions.Length; j++)
					{
						var nextArea = fixedAreaPositions[j];

						if (nextArea != null)
						{
							// if it ends before the other starts or starts after the other end
							// then it doesn't overlap. if that's not the case then they do 
							// overlap and we have to let the previous fix up logic occur
							if (!(nextArea[1] < fixedArea[0] || nextArea[0] > fixedArea[1]))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}
		#endregion //HasOverlappingFixedAreas

		// AS 3/15/11 TFS65358
		#region HasOverlappingItems
		private bool HasOverlappingItems()
		{
			int minColumn = int.MaxValue;
			int minRow = int.MaxValue;
			int maxColumn = int.MinValue;
			int maxRow = int.MinValue;

			foreach (var pos in this.Values)
			{
				if (pos.IsCollapsed)
					continue;

				minColumn = Math.Min(minColumn, pos.Column);
				maxColumn = Math.Max(maxColumn, pos.ColumnRight);
				minRow = Math.Min(minRow, pos.Row);
				maxRow = Math.Max(maxRow, pos.RowBottom);
			}

			// since we just care if something overlaps we can just create a 
			// simple array with bits indicating if its occupied or not
			bool[,] positions = new bool[1 + maxColumn - minColumn, 1 + maxRow - minRow];

			foreach (var pos in this.Values)
			{
				if (pos.IsCollapsed)
					continue;

				for (int c = pos.Column - minColumn, endCol = pos.ColumnRight - minColumn; c <= endCol; c++)
				{
					for (int r = pos.Row - minRow, endRow = pos.RowBottom - minRow; r <= endRow; r++)
					{
						if (positions[c, r])
							return true;

						positions[c, r] = true;
					}
				}
			}

			return false;
		}
		#endregion //HasOverlappingItems

		// AS 3/15/11 TFS65358
		#region HasPositionsChanged
		private bool HasPositionsChanged()
		{
			foreach (var pair in _info)
			{
				var pos = pair.Value;

				if (pos.IsCollapsed)
					continue;

				if (!pos.HasSamePosition(pair.Key.GridPosition))
					return true;
			}

			return false;
		}
		#endregion //HasPositionsChanged

		#region Intersects

		internal static bool Intersects( int x1, int w1, int x2, int w2 )
		{
			if ( x1 > x2 && x1 < x2 + w2 || x1 + w1 > x2 && x1 + w1 < x2 + w2 ||
				 x2 > x1 && x2 < x1 + w1 || x2 + w2 > x1 && x2 + w2 < x1 + w1 ||
				 x1 == x2 && w1 == w2 )
				return true;

			return false;
		}

		#endregion // Intersects

		#region IsSame

		internal bool IsSame( LayoutInfo layoutInfo )
		{
			return AreSame( _info, layoutInfo._info );
		}

		private static bool AreSame( Dictionary<Field, ItemLayoutInfo> xxInfo, Dictionary<Field, ItemLayoutInfo> yyInfo )
		{
			if ( xxInfo.Count != yyInfo.Count )
				return false;

			foreach ( Field key in xxInfo.Keys )
			{
				if ( !yyInfo.ContainsKey( key ) || !ItemLayoutInfo.AreEqual( xxInfo[key], yyInfo[key] ) )
					return false;
			}

			return true;
		}

		#endregion // IsSame

		#region Merge

		// SSP 6/29/09 - NAS9.2 Field Chooser
		// 
		/// <summary>
		/// Copies entries from the source into this layout info.
		/// </summary>
		/// <param name="source">Source layout info from which to copy information to this layout info.</param>
		/// <param name="markCollapsedFieldsNotInSource">If true sets the IsCollapsed to true
		/// on fields in this layout info that don't exist in the source.</param>
		// SSP 10/16/09 TFS23582
		// Added markCollapsedFieldsNotInSource parameter.
		// 
		//internal void Merge( LayoutInfo source )
		internal void Merge( LayoutInfo source, bool markCollapsedFieldsNotInSource )
		{
			foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in source._info )
				_info[ii.Key] = ii.Value;

			// SSP 10/16/09 TFS23582
			// Added markCollapsedFieldsNotInSource parameter. Added the following block.
			// 
			// ----------------------------------------------------------------------------
			if ( markCollapsedFieldsNotInSource )
			{
				foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in _info )
				{
					if ( ! source._info.ContainsKey( ii.Key ) )
						ii.Value.IsCollapsed = true;
				}
			}
			// ----------------------------------------------------------------------------
		}

		#endregion // Merge

		#region PackLayout

		internal void PackLayout( )
		{
            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
            // Added overloads to allow only packing one orientation.
            //
            PackLayout(true, true);
        }

        internal void PackLayout(bool packColumns, bool packRows)
        {
			int nRows = 0, nCols = 0;
			foreach ( ItemLayoutInfo ii in _info.Values )
			{
				nRows = Math.Max( nRows, ii.Row + ii.RowSpan );
				nCols = Math.Max( nCols, ii.Column + ii.ColumnSpan );
			}

            if (nRows == 0 || nCols == 0)
                return;

			bool[] rows = new bool[nRows];
			bool[] cols = new bool[nCols];

			// Flag rows and columns through which one or more item passes.
			// 
			foreach ( ItemLayoutInfo ii in _info.Values )
			{
				GridUtilities.SetValue( rows, ii.Row, ii.RowSpan, true );
				GridUtilities.SetValue( cols, ii.Column, ii.ColumnSpan, true );
			}

			int[] rowDelta = new int[rows.Length];
			int[] colDelta = new int[cols.Length];

            if (packRows)
            {
                for (int i = 0, delta = 0; i < rows.Length; i++)
                {
                    if (!rows[i])
                        ++delta;

                    rowDelta[i] = delta;
                }
            }

            if (packColumns)
            {
                for (int i = 0, delta = 0; i < cols.Length; i++)
                {
                    if (!cols[i])
                        ++delta;

                    colDelta[i] = delta;
                }
            }

			foreach ( ItemLayoutInfo ii in _info.Values )
			{
				ii.Column -= colDelta[ii.Column];
				ii.Row -= rowDelta[ii.Row];
			}
		}

		#endregion // PackLayout

		#region RemoveDeletedFields

		private void RemoveDeletedFields( )
		{
			HashSet fieldLayoutFields = new HashSet( );
			fieldLayoutFields.AddItems( GridUtilities.GetFields( _fieldLayout ) );

			List<Field> fieldsToRemove = new List<Field>( );

			foreach ( Field field in this.Keys )
			{
				if ( ! fieldLayoutFields.Exists( field ) )
					fieldsToRemove.Add( field );
			}

			foreach ( Field field in fieldsToRemove )
				_info.Remove( field );
		}

		#endregion // RemoveDeletedFields

        // AS 1/28/09 NA 2009 Vol 1 - Fixed Fields
        #region ShiftItems
        /// <summary>
        /// Helper method to offset the Row or Column of items in the layout.
        /// </summary>
        /// <param name="insertColumns">True if the Column is to be adjusted, otherwise false to adjust the Row</param>
        /// <param name="startingOrigin">The lowest Column/Row for which an item should be adjusted</param>
        /// <param name="delta">The offset amount</param>
        internal void ShiftItems(bool insertColumns, int startingOrigin, int delta)
        {
            if (delta != 0)
            {
                foreach (KeyValuePair<Field, ItemLayoutInfo> ii in _info)
                {
                    ItemLayoutInfo iiInfo = ii.Value;

                    if (insertColumns && iiInfo.Column >= startingOrigin)
                        iiInfo.Column += delta;
                    else if (!insertColumns && iiInfo.Row >= startingOrigin)
                        iiInfo.Row += delta;
                }
            }
        } 
        #endregion //ShiftItems

		#region SynchronizeFieldVisibility

		// SSP 6/29/09 - NAS9.2 Field Chooser
		// 
		/// <summary>
		/// Synchronizes fields' Visibility property with the _isCollapsed value of the associated 
		/// item layout infos.
		/// </summary>
		internal void SynchronizeFieldVisibility( )
		{
			foreach ( KeyValuePair<Field, ItemLayoutInfo> ii in _info )
			{
				Visibility currentVisibility = ii.Key.VisibilityInCellArea;
				Visibility newVisibility = ii.Value.Visibility;

				if ( currentVisibility != newVisibility )
					ii.Key.Visibility = newVisibility;
			}
		}

		#endregion // SynchronizeFieldVisibility

		// AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region UpdateFixedLocation
        internal void UpdateFixedLocation(FixedFieldLocation oldLocation, FixedFieldLocation newLocation, IList<Field> fields)
        {
            Debug.Assert(fields.Count > 0);

            if (fields.Count == 0)
                return;

            bool byRow = _fieldLayout.IsHorizontal;
            FixedLocationComparer comparer = new FixedLocationComparer(byRow);
            int otherSpan = byRow ? this.GetLogicalColumnCount() : this.GetLogicalRowCount();

            #region Setup ItemsToChange

            // build a list of the itemlayoutinfos for the items being changed
            List<ItemLayoutInfo> itemsToChange = new List<ItemLayoutInfo>();
            Dictionary<ItemLayoutInfo, Field> itemToFieldMap = new Dictionary<ItemLayoutInfo, Field>();
            int itemsToChangeStart = 0;
            int itemsToChangeEnd = 0;

            // iterate the fields backward. if there is no layout item for a given 
            // field then we will ignore it
            for (int first = fields.Count - 1, i = first; i >= 0; i--)
            {
                Field field = fields[i];
                ItemLayoutInfo newItem = this[field];

                Debug.Assert(null != newItem);

                if (null == newItem)
                {
                    fields.RemoveAt(i);
                    continue;
                }

                itemsToChange.Add(newItem);

                // maintain a map so we can get back to the field
                itemToFieldMap[newItem] = field;

                // remove the item from this layout info while we are processing it
                // so we can collapse gaps, etc.
                _info.Remove(field);

                // find out the origin and extent (based on the orientation) of the new items
                int tempStart = byRow ? newItem.Row : newItem.Column;
                int tempEnd = tempStart + (byRow ? newItem.RowSpan : newItem.ColumnSpan);

                if (i == first)
                {
                    itemsToChangeStart = tempStart;
                    itemsToChangeEnd = tempEnd;
                }
                else
                {
                    itemsToChangeStart = Math.Min(itemsToChangeStart, tempStart);
                    itemsToChangeEnd = Math.Max(itemsToChangeEnd, tempEnd);
                }
            }

            // since we may have removed items above we need to reverify the field count
            if (itemsToChange.Count == 0)
                return;

            itemsToChange.Sort(comparer); 

            #endregion //Setup ItemsToChange

            #region Setup ItemsToChangeCols
            // build a list of logical of column slots where the items being moved exist
            List<bool[]> itemsToChangeCols = new List<bool[]>();
            int rowExtent = otherSpan;

            for (int c = (itemsToChangeEnd - itemsToChangeStart) - 1; c >= 0; c--)
            {
                bool[] rows = new bool[rowExtent];
                bool hasItem = false;

                for (int r = 0; r < rowExtent; r++)
                {
                    // iterate the items and find out which (if any exist at this slot)
                    for (int i = 0; i < itemsToChange.Count; i++)
                    {
                        ItemLayoutInfo newItem = itemsToChange[i];

                        int row = !byRow ? r : c + itemsToChangeStart;
                        int col = !byRow ? c + itemsToChangeStart : r;

                        if (newItem.Contains(col, row))
                        {
                            rows[r] = true;
                            hasItem = true;
                            break;
                        }
                    }
                }

                // verify that something exists in this logical column. if not then don't add it
                // (i.e. pack the itemsToChange)
                if (hasItem)
                {
                    itemsToChangeCols.Insert(0, rows);
                }
                else
                {
                    // shift over the new items whose origin is after this point
                    for (int i = 0; i < itemsToChange.Count; i++)
                    {
                        ItemLayoutInfo newItem = itemsToChange[i];

                        if (!byRow && newItem.Column > c + itemsToChangeStart)
                            newItem.Column--;
                        else if (byRow && newItem.Row > c + itemsToChangeStart)
                            newItem.Row--;
                    }
                }
            } 
            #endregion //Setup ItemsToChangeCols

            // update the end count since we may have packed the layout
            itemsToChangeEnd = itemsToChangeCols.Count + itemsToChangeStart;

            int groupSpan = itemsToChangeEnd - itemsToChangeStart;

            Debug.Assert(groupSpan == itemsToChangeCols.Count);

            // remove any logical columns that were used by the items being moved
            this.PackLayout(!byRow, byRow);

            // get the remaining items sorted by fixed area and position
            List<ItemLayoutInfo> items = new List<ItemLayoutInfo>(this.Values);
            items.Sort(comparer);

            #region Calculate the Scrollable Area Origin

            // find the origin of the "split"
            int nearEnd = 0;
            int farStart = 0;
            int maxEnd = 0;

            for (int i = 0, count = items.Count; i < count; i++)
            {
                ItemLayoutInfo item = items[i];

                if (item.IsCollapsed)
                    continue;

                int start = byRow ? item.Row : item.Column;
                int end = start + (byRow ? item.RowSpan : item.ColumnSpan);

                maxEnd = Math.Max(end, maxEnd);

                switch (item._fixedLocation)
                {
                    case FixedFieldLocation.FixedToNearEdge:
                        // the scrollable area has to be after the near
                        nearEnd = Math.Max(nearEnd, end);
                        farStart = Math.Max(farStart, end);
                        break;
                    case FixedFieldLocation.Scrollable:
                        nearEnd = Math.Min(nearEnd, start);
                        farStart = Math.Max(farStart, end);
                        break;
                }
            }
            #endregion //Calculate the Scrollable Area Origin

            #region Calculate Splitter Origin

            int splitterOrigin = 0;
            bool shiftNearToFar;

            switch (newLocation)
            {
                case FixedFieldLocation.FixedToNearEdge:
                    splitterOrigin = nearEnd;

                    // start to the left of the splitter and move right
                    shiftNearToFar = true;
                    break;
                case FixedFieldLocation.FixedToFarEdge:
                    splitterOrigin = farStart;

                    // start to the right of the splitter and move left
                    shiftNearToFar = false;
                    break;
                default:
                case FixedFieldLocation.Scrollable:
                    if (oldLocation == FixedFieldLocation.FixedToFarEdge)
                    {
                        splitterOrigin = farStart;

                        // start to the left of the splitter and move right
                        shiftNearToFar = true;
                    }
                    else
                    {
                        splitterOrigin = nearEnd;

                        // start to the right of the splitter and move left
                        shiftNearToFar = false;
                    }
                    break;
            }
            #endregion //Calculate Splitter Origin

            // F E | E U U
            // E F | U E U
            //

            // we have to find the innermost slot that can fit the items being fixed/unfixed
            // i in this loop will be both the offset from the splitter as well as the # of
            // logical columns we need to compare for overlaps
            for (int i = groupSpan; i >= 0; i--)
            {
                bool canUseSlot = true;

                #region Calculate canUseSlot
                // check the slots to see if there is overlap
                for (int c = 0; c < itemsToChangeCols.Count; c++)
                {
                    // depending on i, we will skip 1 or more columns
                    // in our check to see if there is overlap

                    // if we're shifting from left to right, we will
                    // ignore the far (i.e. higher) columns as we 
                    // iterate the outer loop
                    if (shiftNearToFar && c >= i)
                        continue;

                    // if we're shifting from right to left, then we
                    // want to ignore the lower columns as we interate
                    // the outerloop
                    if (!shiftNearToFar && c < groupSpan - i)
                        continue;

                    // if we're comparing the slots then get the bit flag
                    // that represents the row items in the column
                    bool[] rowFlags = itemsToChangeCols[c];

                    for (int r = 0; r < rowFlags.Length; r++)
                    {
                        // skip slots where we don't have a new item
                        if (!rowFlags[r])
                            continue;

                        // one of our new items exists at this slot
                        // now we need to see if anything within the 
                        // layout exists at that slot
                        int col = splitterOrigin;

                        if (shiftNearToFar)
                            col += -i + c;
                        else if (!shiftNearToFar)
                            col += c - (groupSpan - i);

                        int row = r;

                        if (byRow)
                            GridUtilities.SwapValues(ref col, ref row);

                        // AS 1/28/09 TFS12910
                        // This check was being done inside the loop so if there 
                        // were no items this check wasn't done. Really there is 
                        // no need to check this state during the loop - we should
                        // just check it before starting it.
                        //
                        if (col < 0 || row < 0)
                        {
                            canUseSlot = false;
                            break;
                        }

                        foreach (KeyValuePair<Field, ItemLayoutInfo> ii in _info)
                        {
                            ItemLayoutInfo iiInfo = ii.Value;

                            if (iiInfo.IsCollapsed)
                                continue;

                            if (iiInfo.Contains(col, row))
                            {
                                canUseSlot = false;
                                break;
                            }
                        }

                        if (!canUseSlot)
                            break;
                    }

                    if (!canUseSlot)
                        break;
                } 
                #endregion //Calculate canUseSlot

                if (canUseSlot)
                {
                    // first shift over everyone at the splitter origin
                    int delta = groupSpan - i;
                    int startShiftAt = splitterOrigin;

                    // shift any items to account for the insertion
                    // of the group of items
                    ShiftItems(!byRow, startShiftAt, delta);

                    #region Position Moved Items
                    for (int j = 0; j < itemsToChange.Count; j++)
                    {
                        ItemLayoutInfo newItem = itemsToChange[j];

                        int col = splitterOrigin;

                        if (shiftNearToFar)
                            col += -i + ((!byRow ? newItem.Column : newItem.Row)- itemsToChangeStart);
                        else if (!shiftNearToFar)
                            col += ((!byRow ? newItem.Column : newItem.Row) - itemsToChangeStart);

                        int row = !byRow ? newItem.Row : newItem.Column;

                        if (byRow)
                            GridUtilities.SwapValues(ref col, ref row);

                        Debug.Assert(col >= 0 && row >= 0);

                        newItem.Column = col;
                        newItem.Row = row;
                        newItem._fixedLocation = newLocation;

                        // put the item back into the dictionary
                        _info[itemToFieldMap[newItem]] = newItem;
                    } 
                    #endregion //Position Moved Items
                    break;
                }
            }

            this.EnsureFixedLocationsDontOverlap();
            this.PackLayout();

            this.DebugFixedLocations();
        }
        #endregion //UpdateFixedLocation

		#region VerifyTemplateGeneratorLayout
		internal void VerifyTemplateGeneratorLayout()
		{
			// AS 3/15/11 TFS65358
			// if the position of the items are different than the stored 
			// grid position (which would indicate that the developer 
			// changed the layout) and we don't have any overlapping 
			// items or overlapping fixed areas then we don't need to 
			// manipulate their layout. instead we'll just update the 
			// cached fixed locations to represent the new locations
			//
			if (this.HasPositionsChanged() &&
				!this.HasOverlappingItems() &&
				!this.HasOverlappingFixedAreas())
			{
				foreach (var pair in _info)
					pair.Value._fixedLocation = pair.Key.FixedLocation;

				return;
			}

			// the view orientation may have changed so we have 
			// to do a verification of the layout
			this.EnsureFixedLocationsDontOverlap();

			// the FixedLocation of one or more fields may have been set so 
			// we need to do a pass and process them as needed.
			this.EnsureFixedLocationsInSync();
		}
		#endregion //VerifyTemplateGeneratorLayout

        #endregion // Private/Internal Methods

        #region Public Methods

        #region Add

        public void Add(Field field, ItemLayoutInfo itemLayoutInfo)
		{
			_info.Add( field, itemLayoutInfo );
		}

		#endregion // Add

		#region ContainsKey

		public bool ContainsKey( Field field )
		{
			return _info.ContainsKey( field );
		}

		#endregion // ContainsKey

		#region Indexer

		public ItemLayoutInfo this[Field field]
		{
			get
			{
				ItemLayoutInfo ret;
				if ( _info.TryGetValue( field, out ret ) )
					return ret;

				return null;
			}
			set
			{
				_info[field] = value;
			}
		}

		#endregion // Indexer

		#region TryGetValue

		public bool TryGetValue( Field key, out ItemLayoutInfo value )
		{
			return _info.TryGetValue( key, out value );
		}

		#endregion // TryGetValue

		#endregion // Public Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		public int Count
		{
			get
			{
				return _info.Count;
			}
		}

		#endregion // Count

		#region Keys

		public IEnumerable<Field> Keys
		{
			get
			{
				return _info.Keys;
			}
		}

		#endregion // Keys

		#region Values

		public IEnumerable<ItemLayoutInfo> Values
		{
			get
			{
				return _info.Values;
			}
		}

		#endregion // Values

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // LayoutInfo

	// JM 04-17-09 - CrossBand grouping feature
	#region GroupByAreaMultiDropType

	internal enum GroupByAreaMultiDropType
	{
		None,
		Grouping,
		Ungrouping,
		Regrouping
	}

	#endregion //GroupByAreaMultiDropType

	#region FieldChooserDropType enum

	// SSP 6/26/09 - NAS9.2 Field Chooser
	// 
	/// <summary>
	/// Represents a drop where the field being dragged is either made visible or hidden.
	/// </summary>
	internal enum FieldChooserDropType
	{
		/// <summary>
		/// None. This drop type is not being used.
		/// </summary>
		None,

		/// <summary>
		/// Used in the case where the user the user drags a field from the data presenter and
		/// drops it outside of the data presenter or over a field chooser. Doing so hides the field.
		/// </summary>
		HideField,

		/// <summary>
		/// Used in the case where the user drags a field from the field chooser and drops it over
		/// the data presenter. Doing so shows the field in the data presenter. NOTE: When the
		/// field is dropped over a field-layout area (for example in relation to another field
		/// or over an empty logical cell of the field-layout), then this drop type will not be
		/// used. It will use the relative drop-location drop-type.
		/// </summary>
		ShowField
	}

	#endregion // FieldChooserDropType enum
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