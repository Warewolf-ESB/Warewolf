using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 2/19/12 - 12.1 - Table Support
	// http://msdn.microsoft.com/en-us/library/dd910252(v=office.12).aspx
	internal class LIST12Record : Biff8RecordBase
	{
		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[typeof(Worksheet)];
			if (worksheet == null)
			{
				Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			manager.CurrentRecordStream.ReadFrtHeader();
			LIST12DataType lsd = (LIST12DataType)manager.CurrentRecordStream.ReadUInt16();
			uint idList = manager.CurrentRecordStream.ReadUInt32();

			WorksheetTable table = worksheet.Tables.GetTableById(idList);
			if (table == null)
			{
				Utilities.DebugFail("Cannot find the table with the specified id.");
				return;
			}

			switch (lsd)
			{
				case LIST12DataType.BlockLevelFormatting:
					manager.CurrentRecordStream.ReadList12BlockLevel(table);
					break;

				case LIST12DataType.StyleInfo:
					manager.CurrentRecordStream.ReadList12TableStyleClientInfo(table);
					break;

				case LIST12DataType.DisplayName:
					manager.CurrentRecordStream.ReadList12DisplayName(table);
					break;

				default:
					Utilities.DebugFail("Unknown lsd value.");
					break;
			}
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			WorksheetTable table = (WorksheetTable)manager.ContextStack[typeof(WorksheetTable)];
			LIST12DataType lsd = (LIST12DataType)manager.ContextStack[typeof(LIST12DataType)];
			if (table == null)
			{
				Utilities.DebugFail("There is no WorksheetTable in the context stack.");
				return;
			}

			manager.CurrentRecordStream.WriteFrtHeader();
			manager.CurrentRecordStream.Write((ushort)lsd);
			manager.CurrentRecordStream.Write((uint)table.Id); // idList

			switch (lsd)
			{
				case LIST12DataType.BlockLevelFormatting:
					manager.CurrentRecordStream.WriteList12BlockLevel(table);
					break;

				case LIST12DataType.StyleInfo:
					manager.CurrentRecordStream.WriteList12TableStyleClientInfo(table);
					break;

				case LIST12DataType.DisplayName:
					manager.CurrentRecordStream.WriteList12DisplayName(table);
					break;

				default:
					Utilities.DebugFail("Unknown lsd value.");
					break;
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.LIST12; }
		}

		public enum LIST12DataType
		{
			BlockLevelFormatting = 0,
			StyleInfo = 1,
			DisplayName
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