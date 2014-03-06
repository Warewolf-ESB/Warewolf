using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd923476(v=office.12)
	internal class EXTERNSHEETRecord : Biff8RecordBase
	{
		public const short SheetCannotBeFoundIndex = -1;
		public const short WorkbookLevelReferenceIndex = -2;

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			ushort numberOfREFStructures = manager.CurrentRecordStream.ReadUInt16();

			for ( int i = 0; i < numberOfREFStructures; i++ )
			{
				ushort supbookRecordIndex = manager.CurrentRecordStream.ReadUInt16();
				short firstSheetInReference = manager.CurrentRecordStream.ReadInt16();
				short lastSheetInReference = manager.CurrentRecordStream.ReadInt16();

				WorkbookReferenceBase workbook = manager.WorkbookReferences[ supbookRecordIndex ];

				WorksheetReference reference;
				Debug.Assert(firstSheetInReference != WorkbookLevelReferenceIndex || lastSheetInReference == firstSheetInReference, "This is unexpected.");

				WorksheetReferenceSingle firstReference = workbook.GetWorksheetReference(firstSheetInReference);
				if (lastSheetInReference == firstSheetInReference)
				{
					reference = firstReference;
				}
				else
				{
					WorksheetReferenceSingle lastReference = workbook.GetWorksheetReference(lastSheetInReference);
					reference = workbook.GetMultiSheetReference(firstReference, lastReference);
				}

				manager.WorksheetReferences.Add(reference);
			}
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			manager.CurrentRecordStream.Write((ushort)manager.WorksheetReferences.Count);

			foreach (WorksheetReference reference in manager.WorksheetReferences)
			{
				int index = manager.WorkbookReferences.IndexOf(reference.WorkbookReference);
				Debug.Assert(index >= 0, "This is unexpected.");

				manager.CurrentRecordStream.Write((ushort)index);
				manager.CurrentRecordStream.Write((short)reference.FirstWorksheetIndex);
				manager.CurrentRecordStream.Write((short)reference.LastWorksheetIndex);
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.EXTERNSHEET; }
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