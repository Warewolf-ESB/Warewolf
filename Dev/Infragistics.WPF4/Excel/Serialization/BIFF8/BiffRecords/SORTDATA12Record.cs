using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Sorting;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 2/21/12 - 12.1 - Table Support
	// http://msdn.microsoft.com/en-us/library/dd943357(v=office.12).aspx
	internal class SORTDATA12Record : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[typeof(Worksheet)];
			if (worksheet == null)
			{
				Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			manager.CurrentRecordStream.ReadFrtHeader();

			ushort temp16 = manager.CurrentRecordStream.ReadUInt16();
			bool fCol = Utilities.TestBit(temp16, 0);
			bool fCaseSensitive = Utilities.TestBit(temp16, 1);
			bool fAltMethod = Utilities.TestBit(temp16, 2);
			int sfp = Utilities.GetBits(temp16, 3, 5);

			if (sfp != 0x01)
			{
				Utilities.DebugFail("Sort conditions are only supported on tables currently.");
				return;
			}

			WorksheetRegion rfx = manager.CurrentRecordStream.ReadRFX(worksheet);

			uint cconditions = manager.CurrentRecordStream.ReadUInt32();
			uint idParent = manager.CurrentRecordStream.ReadUInt32();

			WorksheetTable table = worksheet.Tables.GetTableById(idParent);
			if (table == null)
			{
				Utilities.DebugFail("Cannot find the table.");
				return;
			}

			Debug.Assert(table.SortAndHeadersRegion.Equals(rfx), "Something is wrong here.");

			table.SortSettings.CaseSensitive = fCaseSensitive;
			// MD 4/9/12 - TFS101506
			//table.SortSettings.SortMethod = (fAltMethod || CultureInfo.CurrentCulture.Name == "zh-TW") ? SortMethod.Stroke : SortMethod.PinYin;
			table.SortSettings.SortMethod = (fAltMethod || manager.Workbook.CultureResolved.Name == "zh-TW") ? SortMethod.Stroke : SortMethod.PinYin;

			for (int i = 0; i < cconditions; i++)
				manager.CurrentRecordStream.ReadSortCond12(table);
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			WorksheetTable table = (WorksheetTable)manager.ContextStack[typeof(WorksheetTable)];
			if (table == null)
			{
				Utilities.DebugFail("There is no WorksheetTable in the context stack.");
				return;
			}

			SortSettings<WorksheetTableColumn> sortSettings = table.SortSettings;

			manager.CurrentRecordStream.WriteFrtHeader();

			int sfp = 1;
			bool fAltMethod = table.SortSettings.HasAlternateSortMethod;

			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, false, 0); // fCol
			Utilities.SetBit(ref temp16, sortSettings.CaseSensitive, 1); // fCaseSensitive
			Utilities.SetBit(ref temp16, fAltMethod, 2);
			Utilities.AddBits(ref temp16, sfp, 3, 5);
			manager.CurrentRecordStream.Write(temp16);

			manager.CurrentRecordStream.WriteRFX(table.SortAndHeadersRegion); // rfx
			manager.CurrentRecordStream.Write((uint)sortSettings.SortConditions.Count); // cconditions 
			manager.CurrentRecordStream.Write(table.Id); // idParent 

			foreach (KeyValuePair<WorksheetTableColumn, SortCondition> pair in sortSettings.SortConditions)
			{
				manager.CurrentRecordStream.CapCurrentBlock();
				manager.CurrentRecordStream.WriteSortCond12(pair.Key, pair.Value);
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.SORTDATA12; }
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