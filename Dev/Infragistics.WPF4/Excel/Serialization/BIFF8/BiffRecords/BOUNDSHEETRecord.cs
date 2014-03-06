using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class BOUNDSHEETRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			manager.CurrentRecordStream.ReadUInt32(); // stream position of sheet

			WorksheetVisibility visibility = (WorksheetVisibility)manager.CurrentRecordStream.ReadByte();
			SheetType type = (SheetType)manager.CurrentRecordStream.ReadByte();

			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//FormattedString formattedName = manager.CurrentRecordStream.ReadFormattedString( LengthType.EightBit );
			StringElement formattedName = manager.CurrentRecordStream.ReadFormattedString(LengthType.EightBit);

			// MD 5/4/09 - TFS17197
			// Each VB module has an associated worksheet that must be added. Otherwise, the worksheet index values will be off.
			//Debug.Assert( type == SheetType.Worksheet );
			//
			//if ( type == SheetType.Worksheet )
			{
				Worksheet worksheet = manager.Workbook.Worksheets.Add( formattedName.UnformattedString );

				// MD 3/21/12 - TFS104630
				// When loading a BIFF8 file, initialize this flag to false so we don't save out a STANDARDWIDTH record if one 
				// wasn't present and we are round-tripping the file.
				worksheet.ShouldSaveDefaultColumnWidths256th = false;

				// MD 5/4/09 - TFS17197
				// The worksheets will not keep track of their sheet type because we may have to create temporary worksheets for VB modules.
				worksheet.Type = type;

				worksheet.DisplayOptions.Visibility = visibility;

				// MD 6/19/07 - BR23998
				// For existing worksheets that are loaded in, default the ShowExpansionIndicatorBelowGroup to true,
				// the default for Excel.
				worksheet.ShowExpansionIndicatorBelowGroup = true;

				// MD 10/7/10 - TFS36582
				// It is possible that the EXTERNSHEET record occurs before this record. If that is the case and worksheet references are 
				// needed in the current workbook, we will have generate the references using temporary worksheet names. Now that the worksheet 
				// is created, we need to update the references with the new names.
				manager.UpdateWorksheetReferenceInCurrentWorkbook(worksheet);
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There was no worksheet in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write( (uint)0 ); // stream position of sheet (we will come back later and wrtie the real value)
			manager.CurrentRecordStream.Write( (byte)worksheet.DisplayOptions.Visibility );
			manager.CurrentRecordStream.Write( (byte)SheetType.Worksheet );
			manager.CurrentRecordStream.Write( worksheet.Name, LengthType.EightBit );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.BOUNDSHEET; }
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