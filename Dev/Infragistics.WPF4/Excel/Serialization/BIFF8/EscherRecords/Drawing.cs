using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords

{
	internal class Drawing : EscherRecordBase
	{
		private uint numberOfShapesInDrawing;
		private uint lastSPIDGivenToAShape;

		public Drawing( Worksheet worksheet )
			// MD 9/9/08 - Excel 2007 Format
			// Added a SheetID proeprty so we don't have to keep adding one to the index.
			//: base( 0x00, (ushort)( worksheet.Index + 1 ), 8 )
			: base( 0x00, (ushort)( worksheet.SheetId ), 8 )
		{
			this.numberOfShapesInDrawing = worksheet.NumberOfShapes;
			this.lastSPIDGivenToAShape = worksheet.MaxAssignedShapeId;
		}

		public Drawing( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength ) 
		{
			Debug.Assert( version == 0x00 );
			Debug.Assert( instance > 0 );
			Debug.Assert( recordLength == 8 );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			this.numberOfShapesInDrawing = manager.CurrentRecordStream.ReadUInt32();
			this.lastSPIDGivenToAShape = manager.CurrentRecordStream.ReadUInt32();
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			manager.CurrentRecordStream.Write( this.numberOfShapesInDrawing );
			manager.CurrentRecordStream.Write( this.lastSPIDGivenToAShape );
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( "------------------------------" );
			sb.Append( "\n" );

			sb.Append( base.ToString() );
			sb.Append( "\n\n" );

			sb.Append( "Drawing ID: " + this.Instance );
			sb.Append( "\n" );

			sb.Append( "Number of Shapes in this Drawing: " + this.numberOfShapesInDrawing );
			sb.Append( "\n" );

			sb.Append( "Last SPID Given to a Shape: " + this.lastSPIDGivenToAShape );
			sb.Append( "\n" );

			sb.Append( "------------------------------" );

			return sb.ToString();
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.Drawing; }
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