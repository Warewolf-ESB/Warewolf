using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 2/22/12 - 12.1 - Table Support
	// http://msdn.microsoft.com/en-us/library/dd922683(v=office.12).aspx
	internal class TABLESTYLERecord : Biff8RecordBase
	{
		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			manager.CurrentRecordStream.ReadFrtHeader();

			ushort temp16 = manager.CurrentRecordStream.ReadUInt16();
			bool reserved1 = Utilities.TestBit(temp16, 0);
			Debug.Assert(reserved1 == false, "The reserved1 value is incorrect.");
			bool fIsPivot = Utilities.TestBit(temp16, 1);
			bool fIsTable = Utilities.TestBit(temp16, 2);
			int reserved2 = Utilities.GetBits(temp16, 3, 15);
			Debug.Assert(reserved2 == 0, "The reserved2 value is incorrect.");

			uint ctse = manager.CurrentRecordStream.ReadUInt32();
			Debug.Assert(ctse <= 28, "The ctse value is incorrect.");
			ushort cchName = manager.CurrentRecordStream.ReadUInt16();
			string rgchName = manager.CurrentRecordStream.ReadUnicodeString(cchName);
			Debug.Assert(rgchName.Length != 0, "The table style has no name.");

			if (fIsTable && rgchName.Length > 0)
				manager.Workbook.CustomTableStyles.Add(new WorksheetTableStyle(rgchName));
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			WorksheetTableStyle style = (WorksheetTableStyle)manager.ContextStack[typeof(WorksheetTableStyle)];
			if (style == null)
			{
				Utilities.DebugFail("There is no WorksheetTableStyle in the context stack.");
				return;
			}

			manager.CurrentRecordStream.WriteFrtHeader();

			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, true, 2); // fIsTable
			manager.CurrentRecordStream.Write(temp16);

			manager.CurrentRecordStream.Write((uint)style.DxfIdsByAreaDuringSave.Count); // ctse 
			manager.CurrentRecordStream.Write((ushort)style.Name.Length); // cchName 
			manager.CurrentRecordStream.WriteUnicodeString(style.Name);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.TABLESTYLE; }
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