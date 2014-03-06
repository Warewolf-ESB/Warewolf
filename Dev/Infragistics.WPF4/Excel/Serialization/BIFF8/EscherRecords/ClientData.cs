using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal class ClientData : EscherRecordBase
	{
		private WorksheetShape shape;

		public ClientData( WorksheetShape shape )
			: base( 0x00, 0x0000, 0 )
		{
			this.shape = shape;
		}

		public ClientData( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x00 );
			Debug.Assert( instance == 0x0000 );
			Debug.Assert( recordLength == 0 );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// MD 9/15/08 - TFS7442
			// Part of this code has been refactored into a method that can be called recursively because there are apparently
			// some situations where multiple charts will be saved to the same ClientData record. There will be one HostControl
			// shape with multiple chart sub-streams in this ClientData record.
			#region Refactored

			
#region Infragistics Source Cleanup (Region)


























































#endregion // Infragistics Source Cleanup (Region)


			#endregion Refactored
			// MD 10/30/11 - TFS90733
			// We will now use an Obj class to hold all the records rather than a collection.
			//List<OBJRecordBase> objRecords = this.LoadObjRecordData( manager );
			Obj obj = this.LoadObjRecordData( manager );

			manager.CurrentRecordStream.AppendNextRecordIfType( (int)BIFF8RecordType.MSODRAWING );

			// MD 10/30/11 - TFS90733
			//if ( objRecords == null )
			if (obj == null)
			{
				Utilities.DebugFail( "We should have parsed OBJ records." );
				return;
			}

			WorksheetShape shape = (WorksheetShape)manager.ContextStack[ typeof( WorksheetShape ) ];

			if ( shape == null )
			{
				Utilities.DebugFail( "There is no shape in the context stack." );
				return;
			}

			// MD 10/30/11 - TFS90733
			//shape.ObjRecords = objRecords;
			shape.Obj = obj;

			// MD 11/8/11 - TFS85193
			// We should convert over to the comment shape type as soon as possible so the TXO record can correctly
			// load FormattedText instead of a FormattedString.
			if (obj != null &&
				obj.Cmo != null &&
				obj.Cmo.Ot == ObjectType.Comment)
			{
				UnknownShape unknownShape = shape as UnknownShape;
				if (unknownShape != null)
				{
					WorksheetCellComment comment = new WorksheetCellComment(unknownShape);
					manager.ContextStack.ReplaceItem(unknownShape, comment);
					manager.LoadedComments.Add(comment);
				}
			}
		}

		// MD 9/15/08 - TFS7442
		// MD 10/30/11 - TFS90733
		// We will now use an Obj class to hold all the records rather than a collection.
		//private List<OBJRecordBase> LoadObjRecordData( BIFF8WorkbookSerializationManager manager )
		private Obj LoadObjRecordData( BIFF8WorkbookSerializationManager manager )
		{
			long currentPosition = manager.WorkbookStream.Position;

			Biff8RecordStream objStream = new Biff8RecordStream( manager );

			if ( objStream.RecordType != BIFF8RecordType.OBJ )
			{
				objStream.Close();
				manager.WorkbookStream.Position = currentPosition;
				return null;
			}




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			manager.CurrentRecordStream.AddSubStream( objStream );

			// MD 10/30/11 - TFS90733
			// The Obj class will load all individual Obj records.
			//List<OBJRecordBase> objRecords = new List<OBJRecordBase>();
			//
			//while ( objStream.Position < objStream.Length )
			//{
			//    objRecords.Add( OBJRecordBase.LoadRecord( objStream ) );
			//}
			Obj obj = new Obj();
			obj.Load(objStream);

			objStream.Close();

			// MD 10/30/11 - TFS90733
			//Debug.Assert( 
			//    objRecords.Count > 0 && 
			//    objRecords[ 0 ] is CommonObjectData && 
			//    objRecords[ objRecords.Count - 1 ] is End );
			Debug.Assert(obj.Cmo != null, "The Obj records were not loaded correctly.");

			// MD 8/1/07 - BR25039
			#region Move Past Chart Data

			// MD 10/30/11 - TFS90733
			//CommonObjectData commonObjectData = objRecords[ 0 ] as CommonObjectData;
			//
			//if ( commonObjectData != null && commonObjectData.ObjectType == ObjectType.Chart )
			if (obj.Cmo != null && obj.Cmo.Ot == ObjectType.Chart)
			{
				// MD 4/28/11 - TFS62775
				// Keep track of where the chart stream starts. We will now cache it for round tripping.
				long startStreamPosition = manager.WorkbookStream.Position;

				// The chart stream is stored in the ClientData record, move past it here

				// Read and move past the BOF record
				Biff8RecordStream bofStream = new Biff8RecordStream( manager );
				Debug.Assert( bofStream.RecordType == BIFF8RecordType.BOF, "The chart shape must have a data stream for the chart data." );

				manager.CurrentRecordStream.AddSubStream( bofStream );

                Utilities.DebugIndent();


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

				bofStream.Close();

				// Read all records up to and including the EOF record
				Biff8RecordStream chartRecord;
				do
				{
					chartRecord = new Biff8RecordStream( manager );

					Debug.Assert( chartRecord.RecordType != BIFF8RecordType.BOF, "A new sub stream should not have been started in the chart data." );			

					manager.CurrentRecordStream.AddSubStream( chartRecord );


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

					chartRecord.Close();
				}
				while ( chartRecord.RecordType != BIFF8RecordType.EOF );

				Utilities.DebugUnindent();

				// MD 4/28/11 - TFS62775
				// Cache the chart stream data and store it on shape.
				long endStreamPosition = manager.WorkbookStream.Position;
				manager.WorkbookStream.Position = startStreamPosition;
				byte[] data = new byte[endStreamPosition - startStreamPosition];
				manager.WorkbookStream.Read(data, 0, data.Length);

				// Take the UnknownShape off the stack. We will replace it with a WorksheetChart.
				// If should be at the peek of the stack.
				UnknownShape unknownShape = manager.ContextStack.Pop() as UnknownShape;

				// Initialize the WorksheetChart from the shape data we already have.
				WorksheetChart chart;
				if (unknownShape == null)
				{
					Utilities.DebugFail("There should be an UnknownShape at the top of the ContextStack.");
					chart = new WorksheetChart();
				}
				else
				{
					chart = new WorksheetChart(unknownShape);
				}

				// Cache the chart stream and store the new shape instance on the stack.
				chart.Excel2003RoundTripData = data;
				manager.ContextStack.Push(chart);
				// -------------------------- TFS62775 ---------------------------
			}

			#endregion Move Past Chart Data

			// MD 10/30/11 - TFS90733
			//this.LoadObjRecordData( manager );
			//return objRecords;
			Obj nextObj = this.LoadObjRecordData(manager);
			Debug.Assert(nextObj == null, "Figure out how to merge these two Obj records.");
			return obj;
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			// MD 10/30/11 - TFS90733
			// This needs to be done when the shapes are initialized for serialization.
			//this.shape.PopulateObjRecords();

			Biff8RecordStream objStream = new Biff8RecordStream( manager, BIFF8RecordType.OBJ );
			manager.CurrentRecordStream.AddSubStream( objStream );

			// MD 10/30/11 - TFS90733
			//Debug.Assert( 
			//    this.shape.ObjRecords.Count > 0 &&
			//    this.shape.ObjRecords[ 0 ] is CommonObjectData &&
			//    this.shape.ObjRecords[ this.shape.ObjRecords.Count - 1 ] is End );
			FtCmo cmo = this.shape.Obj.Cmo;
			Debug.Assert(cmo != null, "The shape Obj was not initialized correctly.");

			// MD 10/30/11 - TFS90733
			// The Obj instance will save each individual record.
			//foreach ( OBJRecordBase record in this.shape.ObjRecords )
			//{
			//    // MD 5/4/09 - TFS17197
			//    // If one of the OBJ records in the Escher stream could not be read correctly, skip it.
			//    if ( record == null )
			//        continue;
			//
			//    record.Save( objStream );
			//}
			this.shape.Obj.Save(objStream);

			objStream.Close();

			// MD 4/28/11 - TFS62775
			// If we are saving a chart object, also save the chart stream after the OBJ records.
			// MD 10/30/11 - TFS90733
			//CommonObjectData commonObjectData = this.shape.ObjRecords[0] as CommonObjectData;
			//if (commonObjectData != null && commonObjectData.ObjectType == ObjectType.Chart)
			if (cmo != null && cmo.Ot == ObjectType.Chart)
			{
				WorksheetChart chart = this.shape as WorksheetChart;
				if (chart != null && chart.Excel2003RoundTripData != null)
				{
					manager.CurrentRecordStream.WriteRawDataAfterRecord(chart.Excel2003RoundTripData);
				}
				else
				{
					Utilities.DebugFail("We should have a chart instance here.");
				}
			}

			manager.CurrentRecordStream.NextBlockType = BIFF8RecordType.MSODRAWING;
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.ClientData; }
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