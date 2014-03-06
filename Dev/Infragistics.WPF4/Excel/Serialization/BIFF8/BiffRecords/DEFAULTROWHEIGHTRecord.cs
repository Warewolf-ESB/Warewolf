using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class DEFAULTROWHEIGHTRecord : Biff8RecordBase
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

			// MD 5/19/09 - TFS17774
			// Uncommented: this flag is now needed.
			bool rowHeightAndDefaultFontHeightDontMatch =	( optionFlags & 0x0001 ) == 0x0001;

			worksheet.DefaultRowHidden =					( optionFlags & 0x0002 ) == 0x0002;
			bool hasAdditionalSpaceAboveRow =				( optionFlags & 0x0004 ) == 0x0004;
			bool hasAdditionalSpaceBelowRow =				( optionFlags & 0x0008 ) == 0x0008;

			// This options don't seem to be honored by excel
			Debug.Assert( 
				hasAdditionalSpaceAboveRow == false && 
				hasAdditionalSpaceBelowRow == false );

			// MD 3/21/11
			// Found while fixing TFS65198
			// It appears this assumption is no longer correct. The Microsoft Excel 2007 application now honors the default 
			// row height from xls files even if the first bit is not set in the options flags.
			//// MD 5/19/09 - TFS17774
			//// It looks like the value in this record is ignored if the options flags don't have the 1 bit set.
			////worksheet.DefaultRowHeight = manager.CurrentRecordStream.ReadUInt16();
			//ushort defaultRowHeight = manager.CurrentRecordStream.ReadUInt16();
			//if ( rowHeightAndDefaultFontHeightDontMatch )
			//    worksheet.DefaultRowHeight = defaultRowHeight;
			worksheet.DefaultRowHeight = manager.CurrentRecordStream.ReadUInt16();
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
				Utilities.DebugFail( "There is no worksheet in the context stack." );
				return;
			}

			ushort optionFlags = 0x0000;

			// MD 5/19/09 - TFS17774
			// It looks like the value in this record is ignored if the options flags don't have the 1 bit set,
			// so if the value needs to be honored, set that bit.
			if ( worksheet.DefaultRowHeight != worksheet.DefaultRowHeightResolved )
				optionFlags |= 0x0001;

			if ( worksheet.DefaultRowHidden )
				optionFlags |= 0x0002;

			manager.CurrentRecordStream.Write( optionFlags );
			manager.CurrentRecordStream.Write( (ushort)worksheet.DefaultRowHeight );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.DEFAULTROWHEIGHT; }
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