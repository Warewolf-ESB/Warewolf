using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class USERSVIEWBEGINRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Guid customViewId = new Guid( manager.CurrentRecordStream.ReadBytes( 16 ) );

			CustomView customView = manager.Workbook.CustomViews[ customViewId ];

			if ( customView == null )
			{
                Utilities.DebugFail("There is no custom view with that id.");
				return;
			}

			int tabId = manager.CurrentRecordStream.ReadInt32();

			// MD 6/25/08 - BR34324
			// We were interpretting the tab id incorrectly. I thought it indicated the worksheet to apply the custom view settings to.
			// But it actually indicates what the tab id of the current worksheet should be when the custom view is applied. Since we 
			// basically ignore the tab id anyway, and the custom views don't seem to be able to re-order the tabs, we should ignore
			// this and just use the current worksheet to apply the settings to.
			//if ( manager.WorksheetIndices.ContainsKey( tabId ) == false )
			//	return;
			//
			//int worksheetIndex = manager.WorksheetIndices[ tabId ];
			//Worksheet worksheet = manager.Workbook.Worksheets[ worksheetIndex ];
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			int windowZoom = manager.CurrentRecordStream.ReadInt32(); 
			int gridLineColorIndex = manager.CurrentRecordStream.ReadInt32();

			// MD 5/24/07
			// Property removed: uncomment when property is re-added (remove read call below comment)
			//PaneLocation activePane = (PaneLocation)manager.CurrentRecordStream.ReadUInt32();
			manager.CurrentRecordStream.ReadUInt32();

			uint optionFlags = manager.CurrentRecordStream.ReadUInt32();

			//bool pageBreaksAreDisplayed =							( optionFlags & 0x00000001 ) == 0x00000001;
			bool showFormulasInCells =								( optionFlags & 0x00000002 ) == 0x00000002;
			bool showGridlines =									( optionFlags & 0x00000004 ) == 0x00000004;
			bool showRowAndColumnHeaders =							( optionFlags & 0x00000008 ) == 0x00000008;
			bool showOutlineSymbols =								( optionFlags & 0x00000010 ) == 0x00000010;
			bool showZeroValues =									( optionFlags & 0x00000020 ) == 0x00000020;
			bool centeredHorizontally =								( optionFlags & 0x00000040 ) == 0x00000040;
			bool centeredVertically =								( optionFlags & 0x00000080 ) == 0x00000080;
			bool printRowAndColumnHeaders =							( optionFlags & 0x00000100 ) == 0x00000100;
			bool printGridlines =									( optionFlags & 0x00000200 ) == 0x00000200;
			bool fitToPages =										( optionFlags & 0x00000400 ) == 0x00000400;
			//bool sheetContainsPrintArea =							( optionFlags & 0x00000800 ) == 0x00000800;
			//bool onlyOnePrintArea =								( optionFlags & 0x00001000 ) == 0x00001000;
			//bool listIsFiltered =									( optionFlags & 0x00002000 ) == 0x00002000;
			//bool autoFilterActive =								( optionFlags & 0x00004000 ) == 0x00004000;
			bool panesAreFrozen =									( optionFlags & 0x00018000 ) == 0x00018000; // I think both bits must be true
			bool windowHasVerticalSplit =							( optionFlags & 0x00020000 ) == 0x00020000;
			bool windowHasHorizontalSplit =							( optionFlags & 0x00040000 ) == 0x00040000;
			//bool hasHiddenRows =									( optionFlags & 0x00080000 ) == 0x00080000;
			//bool hasHiddenColumns =								( optionFlags & 0x00200000 ) == 0x00200000;
			WorksheetVisibility visibility = (WorksheetVisibility)( ( optionFlags & 0x00C00000 ) >> 22 );
			//bool chartIsSizedWithWindow =							( optionFlags & 0x01000000 ) == 0x01000000;
			//bool viewContainsFilteredList =						( optionFlags & 0x02000000 ) == 0x02000000;
			bool pageBreakPreview =									( optionFlags & 0x04000000 ) == 0x04000000;
			bool pageLayout =										( optionFlags & 0x08000000 ) == 0x08000000;
			bool showRulerInPageLayout =							( optionFlags & 0x20000000 ) == 0x20000000;

			ushort firstVisibleRow = manager.CurrentRecordStream.ReadUInt16();
			manager.CurrentRecordStream.ReadUInt16(); // last visible row
			ushort firstVisibleColumn = manager.CurrentRecordStream.ReadUInt16();
			manager.CurrentRecordStream.ReadUInt16(); // last visible column

			double verticalPaneSplit = manager.CurrentRecordStream.ReadDouble();
			double horizontalPaneSplit = manager.CurrentRecordStream.ReadDouble();
			ushort firstVisibleColumnOfRightPane = manager.CurrentRecordStream.ReadUInt16(); // FFFF implies no split
			ushort firstVisibleRowOfBottomPane = manager.CurrentRecordStream.ReadUInt16(); // FFFF implies no split

			CustomViewDisplayOptions displayOptions = customView.GetDisplayOptions( worksheet );
			Debug.Assert( displayOptions != null, "Couldn't get the display options." );

			if ( displayOptions != null )
			{
				#region Set pane settings

				displayOptions.PanesAreFrozen = panesAreFrozen;

				if ( panesAreFrozen )
				{
					#region Frozen Rows

					if ( windowHasHorizontalSplit )
					{
						Debug.Assert( firstVisibleRow == 0 );

						// MD 6/25/08 - BR34340
						// I think I made a bad assumption about this value. Because the field indicates an intergral number of frozen rows,
						// I assumed it had to be stored as an integral value, but it appears it can contain a fractional part. The value is
						// rounded to the nearest integer when obtaining the value. 
						//Debug.Assert( horizontalPaneSplit % 1 == 0 );
						//
						//displayOptions.FrozenPaneSettings.FrozenRows = (int)horizontalPaneSplit;
						// MD 3/16/12 - TFS105094
						// MidpointRoundingAwayFromZero now returns a double.
						//displayOptions.FrozenPaneSettings.FrozenRows = Utilities.MidpointRoundingAwayFromZero( horizontalPaneSplit);
						displayOptions.FrozenPaneSettings.FrozenRows = (int)MathUtilities.MidpointRoundingAwayFromZero(horizontalPaneSplit);

						displayOptions.FrozenPaneSettings.FirstRowInBottomPane = firstVisibleRowOfBottomPane;
					}
					else
					{
						Debug.Assert( firstVisibleRowOfBottomPane == 0xFFFF );
						displayOptions.FrozenPaneSettings.FirstRowInBottomPane = firstVisibleRow;
					}

					#endregion Frozen Rows

					#region Frozen Columns

					if ( windowHasVerticalSplit )
					{
						Debug.Assert( firstVisibleColumn == 0 );

						// MD 6/25/08 - BR34340
						// I think I made a bad assumption about this value. Because the field indicates an intergral number of frozen columns,
						// I assumed it had to be stored as an integral value, but it appears it can contain a fractional part. The value is
						// rounded to the nearest integer when obtaining the value. The odd part about the vertical split is when the fractional
						// part is .5, it always rounds down. I thought is might be using a MidpointRounding value of ToEven, but it also rounds
						// down when the value is 3.5. Both MidpointRounding values would have rounded this up to 4, so it looks like a bug in
						// Excel which I will not duplicate here.
						//Debug.Assert( verticalPaneSplit % 1 == 0 );
						//
						//displayOptions.FrozenPaneSettings.FrozenColumns = (int)verticalPaneSplit;
						// MD 3/16/12 - TFS105094
						// MidpointRoundingAwayFromZero now returns a double.
                        //displayOptions.FrozenPaneSettings.FrozenColumns = Utilities.MidpointRoundingAwayFromZero(verticalPaneSplit);
						displayOptions.FrozenPaneSettings.FrozenColumns = (int)MathUtilities.MidpointRoundingAwayFromZero(verticalPaneSplit);

						displayOptions.FrozenPaneSettings.FirstColumnInRightPane = firstVisibleColumnOfRightPane;
					}
					else
					{
						Debug.Assert( firstVisibleColumnOfRightPane == 0xFFFF );
						displayOptions.FrozenPaneSettings.FirstColumnInRightPane = firstVisibleColumn;
					}

					#endregion Frozen Columns

					// MD 5/24/07
					// Property removed: uncomment when property is re-added
					//displayOptions.FrozenPaneSettings.PaneWithActiveCell = activePane;
				}
				else
				{
					#region Unfrozen Rows

					if ( windowHasHorizontalSplit )
					{
						displayOptions.UnfrozenPaneSettings.FirstRowInTopPane = firstVisibleRow;
						displayOptions.UnfrozenPaneSettings.FirstRowInBottomPane = firstVisibleRowOfBottomPane;

						displayOptions.UnfrozenPaneSettings.TopPaneHeight = 0;
						for ( int rowIndex = firstVisibleRow; horizontalPaneSplit > 0; rowIndex++ )
						{
							int rowHeight = worksheet.GetRowHeightInTwips( rowIndex );
							double factor = Math.Min( 1, horizontalPaneSplit );

							displayOptions.UnfrozenPaneSettings.TopPaneHeight += (int)( factor * rowHeight );
							horizontalPaneSplit -= 1;
						}
					}
					else
					{
						// MD 5/25/11 - Data Validations / Page Breaks
						// The original assumption seemed to be incorrect.
						//Debug.Assert( firstVisibleRowOfBottomPane == 0xFFFF );
						Debug.Assert(firstVisibleRowOfBottomPane == 0xFFFF || firstVisibleRowOfBottomPane == 0);

						displayOptions.UnfrozenPaneSettings.FirstRowInTopPane = firstVisibleRow;
					}

					#endregion Unfrozen Rows

					#region Unfrozen Columns

					if ( windowHasVerticalSplit )
					{
						displayOptions.UnfrozenPaneSettings.FirstColumnInLeftPane = firstVisibleColumn;
						displayOptions.UnfrozenPaneSettings.FirstColumnInRightPane = firstVisibleColumnOfRightPane;

						displayOptions.UnfrozenPaneSettings.LeftPaneWidth = 0;
						for ( int columnIndex = firstVisibleColumn; verticalPaneSplit > 0; columnIndex++ )
						{
							int columnWidth = worksheet.GetColumnWidthInTwips( columnIndex );
							double factor = Math.Min( 1, verticalPaneSplit );

							displayOptions.UnfrozenPaneSettings.LeftPaneWidth += (int)( factor * columnWidth );
							verticalPaneSplit -= 1;
						}
					}
					else
					{
						// MD 5/25/11 - Data Validations / Page Breaks
						// The original assumption seemed to be incorrect.
						//Debug.Assert( firstVisibleColumnOfRightPane == 0xFFFF );
						Debug.Assert(firstVisibleColumnOfRightPane == 0xFFFF || firstVisibleColumnOfRightPane == 0);

						displayOptions.UnfrozenPaneSettings.FirstColumnInLeftPane = firstVisibleColumn;
					}

					#endregion Unfrozen Columns

					// MD 5/24/07
					// Property removed: uncomment when property is re-added
					//displayOptions.UnfrozenPaneSettings.PaneWithActiveCell = activePane;
				}

				#endregion Set pane settings

				// MD 1/16/12 - 12.1 - Cell Format Updates
				//if ( gridLineColorIndex != WorkbookColorCollection.AutomaticColor )
				if (gridLineColorIndex != WorkbookColorPalette.AutomaticColor)
					displayOptions.UseAutomaticGridlineColor = false;

				displayOptions.GridlineColorIndex = gridLineColorIndex;

				if ( pageBreakPreview )
					displayOptions.View = WorksheetView.PageBreakPreview;
				else if ( pageLayout )
					displayOptions.View = WorksheetView.PageLayout;
				else
					displayOptions.View = WorksheetView.Normal;
				
				displayOptions.MagnificationInCurrentView = windowZoom;
				displayOptions.ShowFormulasInCells = showFormulasInCells;
				displayOptions.ShowGridlines = showGridlines;
				displayOptions.ShowOutlineSymbols = showOutlineSymbols;
				displayOptions.ShowRowAndColumnHeaders = showRowAndColumnHeaders;
				displayOptions.ShowRulerInPageLayoutView = showRulerInPageLayout;
				displayOptions.ShowZeroValues = showZeroValues;
				displayOptions.Visibility = visibility;
			}

			if ( customView.SavePrintOptions )
			{
				PrintOptions printOptions = customView.GetPrintOptions( worksheet );
				Debug.Assert( printOptions != null, "Couldn't get the print options." );
				manager.ContextStack.Push( printOptions );

				if ( printOptions != null )
				{
					printOptions.CenterHorizontally = centeredHorizontally;
					printOptions.CenterVertically = centeredVertically;
					printOptions.PrintGridlines = printGridlines;
					printOptions.PrintRowAndColumnHeaders = printRowAndColumnHeaders;
					
					if ( fitToPages )
						printOptions.ScalingType = ScalingType.FitToPages;
					else
						printOptions.ScalingType = ScalingType.UseScalingFactor;
				}
			}

			manager.ContextStack.Push( displayOptions );
			manager.ContextStack.Push( customView );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			CustomView customView = (CustomView)manager.ContextStack[ typeof( CustomView ) ];
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( customView == null )
			{
                Utilities.DebugFail("There is no custom view in the context stack.");
				return;
			}

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			// Initialize defaults
			int windowZoom =					100;
			int gridLineColorIndex =			64;

			// MD 5/24/07
			// Property removed: uncomment when property is re-added
			//PaneLocation activePane =			PaneLocation.TopLeft;

			bool pageBreaksAreDisplayed =		false;
			bool showFormulasInCells =			false;
			bool showGridlines =				true;
			bool showRowAndColumnHeaders =		true;
			bool showOutlineSymbols =			true;
			bool showZeroValues =				true;
			bool centeredHorizontally =			false;
			bool centeredVertically =			false;
			bool printRowAndColumnHeaders =		false;
			bool printGridlines =				false;
			bool fitToPages =					false;
			bool sheetContainsPrintArea =		false;
			bool onlyOnePrintArea =				false;
			bool listIsFiltered =				false;
			bool autoFilterActive =				false;
			bool panesAreFrozen =				false;
			bool windowHasVerticalSplit =		false;
			bool windowHasHorizontalSplit =		false;
			bool hasHiddenRows =				false;
			bool hasHiddenColumns =				false;
			WorksheetVisibility visibility =	WorksheetVisibility.Visible;
			bool chartIsSizedWithWindow =		false;
			bool viewContainsFilteredList =		false;
			bool pageBreakPreview =				false;
			bool pageLayout =					false;
			bool showRulerInPageLayout =		true;

			int firstVisibleRow =				0;
			int firstVisibleColumn =			0;

			double verticalPaneSplit =			0;
			double horizontalPaneSplit =		0;
			int firstVisibleColumnOfRightPane = 0xFFFF;
			int firstVisibleRowOfBottomPane =	0xFFFF;

			CustomViewDisplayOptions displayOptions = customView.GetDisplayOptions( worksheet );

			if ( displayOptions != null )
			{
				#region Set pane Settings

				panesAreFrozen = displayOptions.PanesAreFrozen;

				if ( displayOptions.PanesAreFrozen )
				{
					#region Frozen Rows

					if ( displayOptions.FrozenPaneSettings.FrozenRows > 0 )
					{
						windowHasHorizontalSplit = true;
						firstVisibleRow = 0;
						horizontalPaneSplit = displayOptions.FrozenPaneSettings.FrozenRows;
						firstVisibleRowOfBottomPane = displayOptions.FrozenPaneSettings.FirstRowInBottomPane;
					}
					else
					{
						windowHasHorizontalSplit = false;
						firstVisibleRow = displayOptions.FrozenPaneSettings.FirstRowInBottomPane;
						horizontalPaneSplit = 0;
						firstVisibleRowOfBottomPane = 0xFFFF;
					}

					#endregion Frozen Rows

					#region Frozen Columns

					if ( displayOptions.FrozenPaneSettings.FrozenColumns > 0 )
					{
						windowHasVerticalSplit = true;
						firstVisibleColumn = 0;
						verticalPaneSplit = displayOptions.FrozenPaneSettings.FrozenColumns;
						firstVisibleColumnOfRightPane = displayOptions.FrozenPaneSettings.FirstColumnInRightPane;
					}
					else
					{
						windowHasVerticalSplit = false;
						firstVisibleColumn = displayOptions.FrozenPaneSettings.FirstColumnInRightPane;
						verticalPaneSplit = 0;
						firstVisibleColumnOfRightPane = 0xFFFF;
					}

					#endregion Frozen Columns

					// MD 5/24/07
					// Property removed: uncomment when property is re-added
					//activePane = displayOptions.FrozenPaneSettings.PaneWithActiveCell;
				}
				else
				{
					#region Unfrozen Rows

					if ( displayOptions.UnfrozenPaneSettings.TopPaneHeight > 0 )
					{
						windowHasHorizontalSplit = true;
						firstVisibleRow = displayOptions.UnfrozenPaneSettings.FirstRowInTopPane;
						firstVisibleRowOfBottomPane = displayOptions.UnfrozenPaneSettings.FirstRowInBottomPane;

						int paneHeight = displayOptions.UnfrozenPaneSettings.TopPaneHeight;

						horizontalPaneSplit = 0;
						for ( int rowIndex = firstVisibleRow; ; rowIndex++ )
						{
							int rowHeight = worksheet.GetRowHeightInTwips( rowIndex );

							if ( rowHeight < paneHeight )
							{
								horizontalPaneSplit += 1;
								paneHeight -= rowHeight;
							}
							else
							{
								horizontalPaneSplit += (double)paneHeight / rowHeight;
								break;
							}
						}
					}
					else
					{
						windowHasHorizontalSplit = false;
						firstVisibleRow = displayOptions.UnfrozenPaneSettings.FirstRowInTopPane;
						firstVisibleRowOfBottomPane = 0xFFFF;
					}

					#endregion Unfrozen Rows

					#region Unfrozen Columns

					if ( displayOptions.UnfrozenPaneSettings.LeftPaneWidth > 0 )
					{
						windowHasHorizontalSplit = true;
						firstVisibleColumn = displayOptions.UnfrozenPaneSettings.FirstColumnInLeftPane;
						firstVisibleColumnOfRightPane = displayOptions.UnfrozenPaneSettings.FirstColumnInRightPane;

						int paneWidth = displayOptions.UnfrozenPaneSettings.LeftPaneWidth;

						verticalPaneSplit = 0;
						for ( int columnIndex = firstVisibleColumn; ; columnIndex++ )
						{
							int columnWidth = worksheet.GetColumnWidthInTwips( columnIndex );

							if ( columnWidth < paneWidth )
							{
								verticalPaneSplit += 1;
								paneWidth -= columnWidth;
							}
							else
							{
								verticalPaneSplit += (double)paneWidth / columnWidth;
								break;
							}
						}
					}
					else
					{
						windowHasVerticalSplit = false;
						firstVisibleColumn = displayOptions.UnfrozenPaneSettings.FirstColumnInLeftPane;
						firstVisibleColumnOfRightPane = 0xFFFF;
					}

					#endregion Unfrozen Columns

					// MD 5/24/07
					// Property removed: uncomment when property is re-added
					//activePane = displayOptions.UnfrozenPaneSettings.PaneWithActiveCell;
				}

				#endregion Set pane Settings

				gridLineColorIndex = displayOptions.GridlineColorIndex;

				if ( displayOptions.View == WorksheetView.Normal )
				{
					pageBreakPreview = false;
					pageLayout = false;
				}
				else if ( displayOptions.View == WorksheetView.PageBreakPreview )
				{
					pageBreakPreview = true;
					pageLayout = false;
				}
				else
				{
					pageBreakPreview = false;
					pageLayout = true;
				}

				windowZoom = displayOptions.MagnificationInCurrentView;
				showFormulasInCells = displayOptions.ShowFormulasInCells;
				showGridlines = displayOptions.ShowGridlines;
				showOutlineSymbols = displayOptions.ShowOutlineSymbols;
				showRowAndColumnHeaders = displayOptions.ShowRowAndColumnHeaders;
				showRulerInPageLayout = displayOptions.ShowRulerInPageLayoutView;
				showZeroValues = displayOptions.ShowZeroValues;
				visibility = displayOptions.Visibility;
			}

			PrintOptions printOptions = customView.GetPrintOptions( worksheet );

			if ( printOptions != null )
			{
				centeredHorizontally = printOptions.CenterHorizontally;
				centeredVertically = printOptions.CenterVertically;
				printGridlines = printOptions.PrintGridlines;
				printRowAndColumnHeaders = printOptions.PrintRowAndColumnHeaders;
				fitToPages = printOptions.ScalingType == ScalingType.FitToPages;
			}

			HiddenColumnCollection hiddenColumns = customView.GetHiddenColumns( worksheet );

			if ( hiddenColumns != null && hiddenColumns.Count > 0 )
				hasHiddenColumns = true;

			HiddenRowCollection hiddenRows = customView.GetHiddenRows( worksheet );

			if ( hiddenRows != null && hiddenRows.Count > 0 )
				hasHiddenRows = true;

			uint optionFlags = 0;

			if ( pageBreaksAreDisplayed )
				optionFlags |= 0x00000001;

			if ( showFormulasInCells )
				optionFlags |= 0x00000002;

			if ( showGridlines )
				optionFlags |= 0x00000004;

			if ( showRowAndColumnHeaders )
				optionFlags |= 0x00000008;

			if ( showOutlineSymbols )
				optionFlags |= 0x00000010;

			if ( showZeroValues )
				optionFlags |= 0x00000020;

			if ( centeredHorizontally )
				optionFlags |= 0x00000040;

			if ( centeredVertically )
				optionFlags |= 0x00000080;

			if ( printRowAndColumnHeaders )
				optionFlags |= 0x00000100;

			if ( printGridlines )
				optionFlags |= 0x00000200;

			if ( fitToPages )
				optionFlags |= 0x00000400;

			if ( sheetContainsPrintArea )
				optionFlags |= 0x00000800;

			if ( onlyOnePrintArea )
				optionFlags |= 0x00001000;

			if ( listIsFiltered )
				optionFlags |= 0x00002000;

			if ( autoFilterActive )
				optionFlags |= 0x00004000;

			if ( panesAreFrozen )
				optionFlags |= 0x00018000;

			if ( windowHasVerticalSplit )
				optionFlags |= 0x00020000;

			if ( windowHasHorizontalSplit )
				optionFlags |= 0x00040000;

			if ( hasHiddenRows )
				optionFlags |= 0x00080000;

			if ( hasHiddenColumns )
				optionFlags |= 0x00200000;

			optionFlags |= ( (uint)visibility << 22 );

			if ( chartIsSizedWithWindow )
				optionFlags |= 0x01000000;

			if ( viewContainsFilteredList )
				optionFlags |= 0x02000000;

			if ( pageBreakPreview )
				optionFlags |= 0x04000000;

			if ( pageLayout )
				optionFlags |= 0x08000000;

			if ( showRulerInPageLayout )
				optionFlags |= 0x20000000;

			manager.CurrentRecordStream.Write( customView.Id.ToByteArray() );

			// MD 9/9/08 - Excel 2007 Format
			// Added a SheetID proeprty so we don't have to keep adding one to the index.
			//manager.CurrentRecordStream.Write( (int)( worksheet.Index + 1 ) );
			manager.CurrentRecordStream.Write( (int)( worksheet.SheetId ) );

			manager.CurrentRecordStream.Write( windowZoom );
			manager.CurrentRecordStream.Write( gridLineColorIndex );

			// MD 5/24/07
			// Property removed: uncomment when property is re-added (remove read call below comment)
			//manager.CurrentRecordStream.Write( (uint)activePane );
			manager.CurrentRecordStream.Write( (uint)3 );

			manager.CurrentRecordStream.Write( optionFlags );
			manager.CurrentRecordStream.Write( (ushort)firstVisibleRow );
			manager.CurrentRecordStream.Write( (ushort)( firstVisibleRow + 1 ) );
			manager.CurrentRecordStream.Write( (ushort)firstVisibleColumn );
			manager.CurrentRecordStream.Write( (ushort)( firstVisibleColumn + 1 ) );
			manager.CurrentRecordStream.Write( verticalPaneSplit );
			manager.CurrentRecordStream.Write( horizontalPaneSplit );
			manager.CurrentRecordStream.Write( (ushort)firstVisibleColumnOfRightPane );
			manager.CurrentRecordStream.Write( (ushort)firstVisibleRowOfBottomPane );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.USERSVIEWBEGIN; }
		}
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