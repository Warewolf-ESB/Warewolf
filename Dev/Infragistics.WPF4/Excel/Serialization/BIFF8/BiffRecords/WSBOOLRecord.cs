using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords

{
	internal class WSBOOLRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();

			//bool showAutomaticPageBreaks =			( optionFlags & 0x0001 ) == 0x0001;
			//bool isDialogSheet =						( optionFlags & 0x0010 ) == 0x0010;
			//bool applyAutomaticStylesToOutline =		( optionFlags & 0x0020 ) == 0x0020;	
			// MD 6/19/07 - BR23998
			worksheet.ShowExpansionIndicatorBelowGroup =( optionFlags & 0x0040 ) == 0x0040;

			// MD 6/4/10 - ChildRecordsDisplayOrder feature
			//bool outlineButtonsRightOfOutlneGroup =		( optionFlags & 0x0080 ) == 0x0080;
			worksheet.DisplayOptions.ShowExpansionIndicatorBelowGroupedRows = worksheet.ShowExpansionIndicatorBelowGroup ?
															ExcelDefaultableBoolean.True :
															ExcelDefaultableBoolean.False;
			worksheet.DisplayOptions.ShowExpansionIndicatorToRightOfGroupedColumns =
														(optionFlags & 0x0080) == 0x0080 ?
															ExcelDefaultableBoolean.True :
															ExcelDefaultableBoolean.False;

			bool fitPrintoutToNumberOfPages =			( optionFlags & 0x0100 ) == 0x0100;
			//bool showRowOutlineSymbols =				( optionFlags & 0x0400 ) == 0x0400;
			//bool showColumnOutlineSymbols =			( optionFlags & 0x0800 ) == 0x0800;
			//bool alternativeExpressionEvaluation =	( optionFlags & 0x4000 ) == 0x4000;
			//bool alternativeFormulaEntries =			( optionFlags & 0x8000 ) == 0x8000;

			if ( fitPrintoutToNumberOfPages )
				worksheet.PrintOptions.ScalingType = ScalingType.FitToPages;
			else
				worksheet.PrintOptions.ScalingType = ScalingType.UseScalingFactor;
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			// MD 6/19/07 - BR23998
			// Don;t hard code 0x0040 in the options anymore (ShowExpansionIndicatorBelowGroup), it may be added in later
			//ushort optionFlags = 0x04C1;
			// MD 6/4/10 - ChildRecordsDisplayOrder feature
			// Don't hard code 0x0080 in the options anymore (ShowExpansionIndicatorToRightOfGroupedColumns) because it is not customizable.
			//ushort optionFlags = 0x0481;
			ushort optionFlags = 0x0401;

			// MD 6/19/07 - BR23998
			// MD 6/4/10 - ChildRecordsDisplayOrder feature
			// Use the public resolved property now.
			//if ( worksheet.ShowExpansionIndicatorBelowGroup )
			if (worksheet.DisplayOptions.ShowExpansionIndicatorBelowGroupedRowsResolved)
				optionFlags |= 0x0040;

			// MD 6/4/10 - ChildRecordsDisplayOrder feature
			if (worksheet.DisplayOptions.ShowExpansionIndicatorToRightOfGroupedColumnsResolved)
				optionFlags |= 0x0080;

			if ( worksheet.PrintOptions.ScalingType == ScalingType.FitToPages )
				optionFlags |= 0x0100;

			// If we give the option to customize where the expansion indicators are, change the
			// implementations of HasCollapsedIndicator on row and column
			manager.CurrentRecordStream.Write( optionFlags );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.WSBOOL; }
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