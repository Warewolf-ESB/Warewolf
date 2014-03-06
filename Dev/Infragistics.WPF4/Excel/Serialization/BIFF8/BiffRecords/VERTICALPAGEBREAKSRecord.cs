using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 5/13/11 - Data Validations / Page Breaks
	internal class VERTICALPAGEBREAKSRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			if ( manager.CurrentRecordStream.Length == 0 )
				return;

			PrintOptions printOptions = (PrintOptions)manager.ContextStack[typeof(PrintOptions)];

			if (printOptions == null)
			{
				Utilities.DebugFail("There is no PrintOptions in the context stack.");
				return;
			}

			ushort numberOfBreaks = manager.CurrentRecordStream.ReadUInt16();
			for (int i = 0; i < numberOfBreaks; i++)
			{
				ushort id = manager.CurrentRecordStream.ReadUInt16();
				ushort minValue = manager.CurrentRecordStream.ReadUInt16();
				ushort maxValue = manager.CurrentRecordStream.ReadUInt16();

				ushort? min = minValue;
				if (minValue == 0)
					min = null;

				ushort? max = maxValue;
				if (maxValue == manager.Workbook.MaxRowCount - 1)
					max = null;

				printOptions.VerticalPageBreaks.Add(
					new VerticalPageBreak(id, printOptions.PrintAreas.GetPrintArea(id, minValue, maxValue, true)));
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			PrintOptions printOptions = (PrintOptions)manager.ContextStack[typeof(PrintOptions)];

			if (printOptions == null)
			{
				Utilities.DebugFail("There is no PrintOptions in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write((ushort)printOptions.VerticalPageBreaks.Count);
			for (int i = 0; i < printOptions.VerticalPageBreaks.Count; i++)
			{
				PageBreak pageBreak = printOptions.VerticalPageBreaks[i];

				manager.CurrentRecordStream.Write((ushort)pageBreak.Id);
				manager.CurrentRecordStream.Write((ushort)pageBreak.MinResolved);
				int maxResolved = Math.Min(pageBreak.MaxResolved, manager.Workbook.MaxRowCount - 1);
				manager.CurrentRecordStream.Write((ushort)maxResolved);
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.VERTICALPAGEBREAKS; }
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