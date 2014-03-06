using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Sorting;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 2/21/12 - 12.1 - Table Support
	// http://msdn.microsoft.com/en-us/library/dd953303(v=office.12).aspx
	internal class DXFRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			manager.CurrentRecordStream.ReadFrtHeader();

			ushort temp16 = manager.CurrentRecordStream.ReadUInt16();
			bool fUIFill = Utilities.TestBit(temp16, 0);
			Debug.Assert(fUIFill, "Excel seems to have a problem opening files when fUIFill is False. If Excel opens this file correctly, find out when we should use that value.");
			bool fNewBorder = Utilities.TestBit(temp16, 1);
			int reserved = Utilities.GetBits(temp16, 3, 15);
			Debug.Assert(reserved == 0, "The reserved value is incorrect.");

			WorksheetCellFormatData dxf = manager.CurrentRecordStream.ReadXFProps(fUIFill);
			manager.Dxfs.Add(dxf);
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			WorksheetCellFormatData format = (WorksheetCellFormatData)manager.ContextStack[typeof(WorksheetCellFormatData)];
			if (format == null)
			{
				Utilities.DebugFail("There is no WorksheetCellFormatData in the context stack.");
				return;
			}

			manager.CurrentRecordStream.WriteFrtHeader();

			bool fUIFill = true;

			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, fUIFill, 0);
			Utilities.SetBit(ref temp16, true, 1); // fNewBorder
			manager.CurrentRecordStream.Write(temp16);

			manager.CurrentRecordStream.WriteXFProps(format);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.DXF; }
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