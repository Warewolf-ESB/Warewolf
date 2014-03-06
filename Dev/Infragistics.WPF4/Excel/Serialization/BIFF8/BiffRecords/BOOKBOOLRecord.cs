using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd905265(v=office.12).aspx
	internal class BOOKBOOLRecord : Biff8RecordBase
	{
		private const ushort FNoSaveSupBit = 0x0001;

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// MD 11/22/11
			// Found while writing Interop tests
			// This was being loaded incorrectly.
			//manager.Workbook.SaveExternalLinkedValues = Convert.ToBoolean( manager.CurrentRecordStream.ReadUInt16() );
			ushort bookBook = manager.CurrentRecordStream.ReadUInt16();
			bool fNoSaveSup = (bookBook & FNoSaveSupBit) == FNoSaveSupBit;

			manager.Workbook.SaveExternalLinkedValues = (fNoSaveSup == false);

		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 11/22/11
			// Found while writing Interop tests
			// This was being saved incorrectly.
			//manager.CurrentRecordStream.Write( Convert.ToUInt16( manager.Workbook.SaveExternalLinkedValues ) );
			ushort bookBool = 0;
			if (manager.Workbook.SaveExternalLinkedValues == false)
				bookBool |= FNoSaveSupBit;

			manager.CurrentRecordStream.Write(bookBool);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.BOOKBOOL; }
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