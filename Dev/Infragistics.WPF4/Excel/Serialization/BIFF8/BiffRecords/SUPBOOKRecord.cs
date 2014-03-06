using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd922667(v=office.12)
	internal class SUPBOOKRecord : Biff8RecordBase
	{
		// MD 10/8/07 - BR27172
		// Moved some hard coded values for special name lengths into constants
		private const int CurrentWorkbookID = 0x0401;
		private const int AddInFunctionListID = 0x3A01;

		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			ushort numberOfTabsInWorkbook = manager.CurrentRecordStream.ReadUInt16();
			ushort nameLength = manager.CurrentRecordStream.ReadUInt16();

			WorkbookReferenceBase workbookReference;
			if (nameLength == SUPBOOKRecord.CurrentWorkbookID)
			{
				workbookReference = manager.Workbook.CurrentWorkbookReference;
			}
			else if (nameLength == SUPBOOKRecord.AddInFunctionListID)
			{
				Debug.Assert(numberOfTabsInWorkbook == 1);
				workbookReference = manager.Workbook.AddInFunctionsWorkbookReference;
			}
			else
			{
				string fileNameOfWorkbook = manager.CurrentRecordStream.ReadFormattedString(nameLength).UnformattedString;
				fileNameOfWorkbook = Utilities.DecodeURL(manager.FilePath, fileNameOfWorkbook);

				string[] worksheetNames = new string[numberOfTabsInWorkbook];

				for (int i = 0; i < numberOfTabsInWorkbook; i++)
					worksheetNames[i] = manager.CurrentRecordStream.ReadFormattedString(LengthType.SixteenBit).UnformattedString;

				workbookReference = manager.Workbook.GetWorkbookReference(fileNameOfWorkbook);

				ExternalWorkbookReference externalWorkbookReference = workbookReference as ExternalWorkbookReference;
				if (externalWorkbookReference != null)
				{
					Debug.Assert(externalWorkbookReference.WorksheetNames.Count == 0, "This is unexpected");
					externalWorkbookReference.WorksheetNames.AddRange(worksheetNames);
				}
			}

			manager.WorkbookReferences.Add(workbookReference);
			manager.PrepareForDataFromExternalWorkbook(workbookReference as ExternalWorkbookReference);
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			WorkbookReferenceBase reference = (WorkbookReferenceBase)manager.ContextStack[ typeof( WorkbookReferenceBase ) ];

			if ( reference == null )
			{
                Utilities.DebugFail("There is no workbook reference in the context stack.");
				return;
			}

			if ( reference is CurrentWorkbookReference )
			{
				manager.CurrentRecordStream.Write( (ushort)manager.Workbook.Worksheets.Count );

				// MD 10/8/07 - BR27172
				// Moved this value into a constant
				//manager.CurrentRecordStream.Write( (ushort)1025 );
				manager.CurrentRecordStream.Write( (ushort)SUPBOOKRecord.CurrentWorkbookID );
			}
			// MD 10/8/07 - BR27172
			// If the refernce is an add-in function pseudo-workbook, so special processing
			else if ( reference is AddInFunctionsWorkbookReference )
			{
				manager.CurrentRecordStream.Write( (ushort)1 );
				manager.CurrentRecordStream.Write( (ushort)SUPBOOKRecord.AddInFunctionListID );
			}
			else
			{
				ExternalWorkbookReference externalReference = reference as ExternalWorkbookReference;

				manager.CurrentRecordStream.Write( (ushort)externalReference.WorksheetNames.Count );

				// MD 10/8/07 - BR27172
				// WorkbookFileName was replced with the FileName property
				//manager.CurrentRecordStream.Write( Utilities.EncodeURL( externalReference.WorkbookFileName ), LengthType.SixteenBit );
				manager.CurrentRecordStream.Write( Utilities.EncodeURL( externalReference.FileName ), LengthType.SixteenBit );

				foreach ( string worksheetName in externalReference.WorksheetNames )
					manager.CurrentRecordStream.Write( worksheetName, LengthType.SixteenBit );
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.SUPBOOK; }
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