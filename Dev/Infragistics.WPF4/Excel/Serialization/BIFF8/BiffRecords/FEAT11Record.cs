using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 1/26/12 - 12.1 - Cell Format Updates
	// http://msdn.microsoft.com/en-us/library/dd907085(v=office.12).aspx
	internal class FEAT11Record : Biff8RecordBase
	{
		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			Worksheet worksheet = manager.ContextStack.Get<Worksheet>();
			if (worksheet == null)
			{
				Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			WorksheetRegion region = manager.CurrentRecordStream.ReadFrtHeaderU(worksheet);

			SharedFeatureType isf = (SharedFeatureType)manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(isf == SharedFeatureType.List, "The isf value is incorrect.");

			byte reserved1 = (byte)manager.CurrentRecordStream.ReadByte();
			Debug.Assert(reserved1 == 0, "The reserved1 value is incorrect.");

			uint reserved2 = manager.CurrentRecordStream.ReadUInt32();
			Debug.Assert(reserved2 == 0, "The reserved2 value is incorrect.");

			ushort cref2 = manager.CurrentRecordStream.ReadUInt16();
			uint cbFeatData = manager.CurrentRecordStream.ReadUInt32();

			uint rgbFeat = cbFeatData;
			if (rgbFeat == 0)
				rgbFeat = (uint)(manager.CurrentRecordStream.Length - (8 * cref2) - 27);

			ushort reserved3 = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(reserved3 == 0, "The reserved3 value is incorrect.");

			WorksheetRegion[] refs2 = new WorksheetRegion[cref2];
			for (int i = 0; i < cref2; i++)
				refs2[i] = manager.CurrentRecordStream.ReadRef8U(worksheet);

			Debug.Assert(refs2.Length == 1 && refs2[0] == region, "This is unexpected.");

			WorksheetRegion tableRegion = region;
			if (refs2.Length == 1)
				tableRegion = refs2[0];

			WorksheetTable table = manager.CurrentRecordStream.ReadTableFeatureType(worksheet, tableRegion);

			if (table != null)
				worksheet.Tables.InternalAdd(table);
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			WorksheetTable table = manager.ContextStack.Get<WorksheetTable>();
			SortedList<int, TableColumnFilterData> columnsFilterData = manager.ContextStack.Get<SortedList<int, TableColumnFilterData>>();
			if (table == null || columnsFilterData == null)
			{
				Utilities.DebugFail("There is no WorksheetTable in the context stack.");
				return;
			}

			WorksheetRegion tableRegion = table.WholeTableRegion;

			manager.CurrentRecordStream.WriteFrtHeaderU(tableRegion);
			manager.CurrentRecordStream.Write((ushort)SharedFeatureType.List); // isf
			manager.CurrentRecordStream.WriteByte(0); // reserved1
			manager.CurrentRecordStream.Write((uint)0); // reserved2
			manager.CurrentRecordStream.Write((ushort)1); // cref2
			manager.CurrentRecordStream.Write((uint)0); // cbFeatData
			manager.CurrentRecordStream.Write((ushort)0); // reserved3
			manager.CurrentRecordStream.WriteRef8U(tableRegion); // refs2
			manager.CurrentRecordStream.WriteTableFeatureType(table, columnsFilterData); // rgbFeat
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.FEAT11; }
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