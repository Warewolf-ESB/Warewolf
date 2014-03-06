using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd944974(v=office.12).aspx
	internal class DEFCOLWIDTHRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			// MD 1/11/08 - BR29105
			// We don't need to do calculations anymore. Now we store the value in the format it exists in the excel file.
			//worksheet.DefaultColumnWidth = manager.CurrentRecordStream.ReadUInt16() * 256;
			// MD 2/10/12 - TFS97827
			// Use the SetDefaultColumnWidth method and pass True for the last parameter so we round up to the nearest multiple of 8 pixels
			//worksheet.DefaultMinimumCharsPerColumn = manager.CurrentRecordStream.ReadUInt16();
			ushort temp = manager.CurrentRecordStream.ReadUInt16();
			worksheet.SetDefaultColumnWidth(temp, WorksheetColumnWidthUnit.CharacterPaddingExcluded, true);

			// MD 10/24/11 - TFS89375
			worksheet.hasExplicitlyLoadedDefaultColumnWidth = true;
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			// MD 1/11/08 - BR29105
			// We don't need to do calculations anymore. Now we store the value in the format it exists in the excel file.
			//manager.CurrentRecordStream.Write( (ushort)( worksheet.DefaultColumnWidth / 256 ) );
			// MD 2/10/12 - TFS97827
			// Use the truncated value of the CharacterPaddingExcluded units. We will write out a more accurate default column width in the 
			// STANDARDWIDTH record.
			//manager.CurrentRecordStream.Write( (ushort)worksheet.DefaultMinimumCharsPerColumn );
			int displayCharacters = (int)MathUtilities.Truncate(worksheet.GetDefaultColumnWidth(WorksheetColumnWidthUnit.CharacterPaddingExcluded));
			manager.CurrentRecordStream.Write((ushort)displayCharacters);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.DEFCOLWIDTH; }
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