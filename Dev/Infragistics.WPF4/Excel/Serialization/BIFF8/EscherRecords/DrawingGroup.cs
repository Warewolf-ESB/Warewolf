using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords

{
	internal class DrawingGroup : EscherRecordBase
	{
		// MD 9/10/08 - Cell Comments
		// These don't need to be store don this record anymore
		//private uint maxShapeId;
		//private uint numberOfIDClusters;
		//private uint numberOfShapesSaved;
		//private uint numberOfDrawingsSaved;
		//private List<FileIdCluster> fileIdClusters;

		public DrawingGroup( WorksheetCollection worksheets )
			: base( 0x00, 0x0000, 16 + ( 8 * (uint)worksheets.Count ) )
		{
			// MD 9/10/08 - Cell Comments
			// This type of data is now initialized in WorkbookSerializationManager.InitializeShapes()
			//uint nextShapeId = 1024;
			//uint nextDrawingId = 1;
			//
			//this.fileIdClusters = new List<FileIdCluster>();
			//
			//foreach ( Worksheet worksheet in worksheets )
			//{
			//    nextShapeId = (uint)Utilities.RoundUpToMultiple( (int)nextShapeId, 1024 );
			//
			//    worksheet.AssignShapeIds( ref nextShapeId );
			//
			//    FileIdCluster cluster = new FileIdCluster();
			//    cluster.drawingOwningCluster = nextDrawingId++;
			//    cluster.numberOfShapeIdsUsedInCluster = worksheet.NumberOfShapes;
			//    this.fileIdClusters.Add( cluster );
			//
			//    this.numberOfShapesSaved += worksheet.NumberOfShapes;
			//}
			//
			//this.maxShapeId = nextShapeId;
			//
			//this.numberOfDrawingsSaved = (uint)worksheets.Count;
			//this.numberOfIDClusters = (uint)this.fileIdClusters.Count + 1; 
		}

		public DrawingGroup( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x00 );
			Debug.Assert( instance == 0x0000 );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// MD 9/10/08 - Cell Comments
			// We can ignore this data now, so just skip past the data in the stream.
			//this.maxShapeId = manager.CurrentRecordStream.ReadUInt32();
			//this.numberOfIDClusters = manager.CurrentRecordStream.ReadUInt32();
			//this.numberOfShapesSaved = manager.CurrentRecordStream.ReadUInt32();
			//this.numberOfDrawingsSaved = manager.CurrentRecordStream.ReadUInt32();
			//
			//this.fileIdClusters = new List<FileIdCluster>();
			//
			//// Read in each ID cluster
			//for ( int i = 0; i < this.numberOfIDClusters - 1; i++ )
			//{
			//    FileIdCluster cluster = new FileIdCluster();
			//
			//    cluster.drawingOwningCluster = manager.CurrentRecordStream.ReadUInt32();
			//    cluster.numberOfShapeIdsUsedInCluster = manager.CurrentRecordStream.ReadUInt32();
			//
			//    this.fileIdClusters.Add( cluster );
			//}
			manager.CurrentRecordStream.Seek( this.RecordLength, System.IO.SeekOrigin.Current );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			// MD 9/10/08 - Cell Comments
			// This data is now stored on the serialization manager and if it isn't, it can be figured out.
			//manager.CurrentRecordStream.Write( this.maxShapeId );
			//manager.CurrentRecordStream.Write( this.numberOfIDClusters );
			//manager.CurrentRecordStream.Write( this.numberOfShapesSaved );
			//manager.CurrentRecordStream.Write( this.numberOfDrawingsSaved );
			//
			//foreach ( FileIdCluster cluster in this.fileIdClusters )
			//{
			//    manager.CurrentRecordStream.Write( cluster.drawingOwningCluster );
			//    manager.CurrentRecordStream.Write( cluster.numberOfShapeIdsUsedInCluster );
			//}

			uint totalNumberOfShapes = 0;
			foreach ( Worksheet worksheet in manager.Workbook.Worksheets )
				totalNumberOfShapes += worksheet.NumberOfShapes;

			manager.CurrentRecordStream.Write( manager.MaxShapeId );
			manager.CurrentRecordStream.Write( (uint)( manager.Workbook.Worksheets.Count + 1 ) );
			manager.CurrentRecordStream.Write( totalNumberOfShapes );
			manager.CurrentRecordStream.Write( (uint)manager.Workbook.Worksheets.Count );

			foreach ( Worksheet worksheet in manager.Workbook.Worksheets )
			{
				manager.CurrentRecordStream.Write( (uint)worksheet.SheetId );
				manager.CurrentRecordStream.Write( (uint)worksheet.NumberOfShapes );
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( "------------------------------" );
			sb.Append( "\n" );

			sb.Append( base.ToString() );
			sb.Append( "\n\n" );

			// MD 9/10/08 - Cell Comments
			// This data is not stored on this record anymore
			//sb.Append( "Maximum Shape ID Used: " + this.maxShapeId );
			//sb.Append( "\n" );
			//
			//sb.Append( "Number of ID Clusters: " + this.numberOfIDClusters );
			//sb.Append( "\n" );
			//
			//sb.Append( "Number of Shapes Saved: " + this.numberOfShapesSaved );
			//sb.Append( "\n" );
			//
			//sb.Append( "Number of Drawings (worksheets) Saved: " + this.numberOfDrawingsSaved );
			//sb.Append( "\n" );
			//
			//sb.Append( "ID Clusters:\n" );
			//
			//foreach ( FileIdCluster cluster in this.fileIdClusters )
			//{
			//    sb.AppendFormat( "Drawing {0}: {1} Shapes Used", cluster.drawingOwningCluster, cluster.numberOfShapeIdsUsedInCluster );
			//    sb.Append( "\n" );
			//}

			sb.Append( "------------------------------" );

			return sb.ToString();
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.DrawingGroup; }
		}

		// MD 9/10/08 - Cell Comments
		// Not used anymore
		//private class FileIdCluster
		//{
		//    public uint drawingOwningCluster;
		//    public uint numberOfShapeIdsUsedInCluster;
		//}
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