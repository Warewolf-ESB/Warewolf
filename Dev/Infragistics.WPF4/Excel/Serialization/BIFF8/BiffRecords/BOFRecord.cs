using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using Infragistics.Shared;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords

{
	internal class BOFRecord : Biff8RecordBase
	{
		// MD 5/22/07 - BR23134
		// Keep track of all excel versions so we can tell them what version it is in when its invalid.
		//private const int Biff8Version = 0x0600;
		private enum BiffVersion
		{
			Biff2_1 = 0x0000,
			Biff2_2 = 0x0007,
			Biff2_3 = 0x0200,
			Biff3	= 0x0300,
			Biff4	= 0x0400,
			Biff5	= 0x0500,
			Biff7	= Biff5,
			Biff8	= 0x0600,
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			ushort version = manager.CurrentRecordStream.ReadUInt16();

			// MD 5/22/07 - BR23134
			// We will now try to tell them whats wrong with the file when we get an incorrect version
			//Debug.Assert( version == BOFRecord.Biff8Version, "This is not a valid BIFF8 file." );
			string invalidVersionName = null;
			switch ( (BiffVersion)version )
			{
				case BiffVersion.Biff2_1:
				case BiffVersion.Biff2_2:
				case BiffVersion.Biff2_3:
					invalidVersionName = "BIFF2";
					break;

				case BiffVersion.Biff3:
					invalidVersionName = "BIFF3";
					break;

				case BiffVersion.Biff4:
					invalidVersionName = "BIFF4";
					break;

				case BiffVersion.Biff5:
					invalidVersionName = "BIFF5/BIFF7";
					break;

				case BiffVersion.Biff8:
					break;

				default:
					throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_UnknownBIFFFormat" ) );
			}

			if ( invalidVersionName != null )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_UnsupportedBIFFFormat", invalidVersionName ) );

			SubstreamType substreamType = (SubstreamType)manager.CurrentRecordStream.ReadUInt16();

			// Build Id
			manager.CurrentRecordStream.ReadUInt16();

			// Build Year
			manager.CurrentRecordStream.ReadUInt16();

			switch ( substreamType )
			{
				case SubstreamType.WorkbookGlobals:
					{
						manager.ContextStack.Push( manager.Workbook );
					}
					break;

				case SubstreamType.Worksheet:
					{
						Worksheet worksheet = manager.Workbook.Worksheets[ manager.NextWorksheetIndex ];

						manager.ContextStack.Push( worksheet.DisplayOptions );
						manager.ContextStack.Push( worksheet.PrintOptions );
						manager.ContextStack.Push( worksheet );
					}
					break;

				default:
					{
                        Utilities.DebugFail("Unknown substream type");

						// MD 5/4/09 - TFS17197
						// We have to move to the end of the current record stream before trying to bypass other streams. 
						// This was a huge oversight.
						manager.CurrentRecordStream.Position = manager.CurrentRecordStream.Length;

						while ( true )
						{
							Biff8RecordStream stream = new Biff8RecordStream( manager );







							manager.CurrentRecordStream.AddSubStream( stream );
							stream.Close();

							if ( stream.RecordType == BIFF8RecordType.EOF || manager.WorkbookStream.Position == manager.WorkbookStream.Length )
								break;
						}

						// MD 5/4/09 - TFS17197
						// Once we have moved passed this "worksheet", increment the index.
						manager.NextWorksheetIndex++;
					}
					break;
			}

			// MD 9/5/07 - BR26240
			// Some workbooks written with older versions of Excel (Excel 2000 in this case) 
			// may only contain 8 bytes in the BOF record.
			if ( manager.CurrentRecordStream.Position == manager.CurrentRecordStream.Length )
				return;

			// File history flags
			manager.CurrentRecordStream.ReadUInt32();

			// The lowest biff version which can read the file contents
			manager.CurrentRecordStream.ReadUInt32();
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 5/22/07 - BR23134
			//manager.CurrentRecordStream.Write( (ushort)BOFRecord.Biff8Version );
			manager.CurrentRecordStream.Write( (ushort)BiffVersion.Biff8 );

			object currentStream = manager.ContextStack.Current;
			Worksheet worksheet = currentStream as Worksheet;
			if ( worksheet != null )
				manager.CurrentRecordStream.Write( (ushort)SubstreamType.Worksheet );
			else
				manager.CurrentRecordStream.Write( (ushort)SubstreamType.WorkbookGlobals );

			manager.CurrentRecordStream.Write( (ushort)0x0DBB );
			manager.CurrentRecordStream.Write( (ushort)0x07CC );
			manager.CurrentRecordStream.Write( (uint)0 );

			// MD 5/22/07 - BR23134
			//manager.CurrentRecordStream.Write( (uint)BOFRecord.Biff8Version );
			manager.CurrentRecordStream.Write( (uint)BiffVersion.Biff8 );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.BOF; }
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