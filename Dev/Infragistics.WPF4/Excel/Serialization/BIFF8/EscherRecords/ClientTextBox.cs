using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;






using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords

{
	internal class ClientTextBox : EscherRecordBase
	{
		private WorksheetShape shape;

		public ClientTextBox( WorksheetShape shape )
			: base( 0x00, 0x0000, 0 )
		{
			this.shape = shape;
		}

		public ClientTextBox( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength ) 
		{
			Debug.Assert( version == 0x00 );
			Debug.Assert( instance == 0x0000 );
			Debug.Assert( recordLength == 0 );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Biff8RecordStream txoStream = new Biff8RecordStream( manager );
			Debug.Assert( txoStream.RecordType == BIFF8RecordType.TXO );

			manager.CurrentRecordStream.AddSubStream( txoStream );

			// MD 7/20/2007 - BR25039
			// Moved below, we don't want to do this in all situations now
			//byte[] data = txoStream.ReadBytes( (int)txoStream.Length );
			//
			//txoStream.Close();
			//
			//manager.CurrentRecordStream.AppendNextRecordIfType( BIFFType.MSODRAWING );

			WorksheetShape shape = (WorksheetShape)manager.ContextStack[ typeof( WorksheetShape ) ];

			if ( shape == null )
			{
				Utilities.DebugFail( "There is no shape in the context stack." );
				return;
			}

			// MD 9/2/08 - Cell Comments
			// All shapes support TXO data now.
			#region Old Code

			//// MD 7/20/2007 - BR25039
			//// This will only be done in certain cases now
			////shape.TxoData = data;
			//
			//// MD 7/20/2007 - BR25039
			//// If the shape to which this ClientTextBox record applies is a TextBox, do special processing for it
			//if ( shape.Type == ShapeType.TextBox )
			//{
			//    manager.LoadRecord( txoStream );
			//}
			//else
			//{
			//    // Otherwise, just store the data as a block on the shape
			//    byte[] data = txoStream.ReadBytes( (int)txoStream.Length );
			//    txoStream.Close();
			//
			//    shape.TxoData = data;
			//} 

			#endregion Old Code
			manager.LoadRecord( txoStream );
			manager.CurrentRecordStream.AppendNextRecordIfType( (int)BIFF8RecordType.MSODRAWING );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 7/20/2007 - BR25039
			// Moved from below so we always call it
			base.Save( manager );

			// MD 9/2/08 - Cell Comments
			// All shapes support TXO data now.
			#region Old Code

			//// MD 7/20/2007 - BR25039
			//// If the shape type is TextBox, we want the TXORecord class to Save the data
			//if ( this.shape.Type == ShapeType.TextBox )
			//{
			//    manager.ContextStack.Push( this.shape );
			//    manager.WriteRecord( BIFF8RecordType.TXO );
			//    manager.CurrentRecordStream.NextBlockType = BIFF8RecordType.MSODRAWING;
			//    manager.ContextStack.Pop(); // this.shape
			//
			//    return;
			//}
			//
			//if ( this.shape.TxoData == null )
			//{
			//    Utilities.DebugFail( "Missing Txo Data" );
			//    return;
			//}
			//
			//// MD 7/20/2007 - BR25039
			//// Moved the cell to the base up because we always want it to occur
			////base.Save( manager );
			//
			//Biff8RecordStream txoStream = new Biff8RecordStream( manager, manager.WorkbookStream, BIFF8RecordType.TXO );
			//manager.CurrentRecordStream.AddSubStream( txoStream );
			//
			//// MD 7/20/2007 - BR25039
			//// The TXO data need to be separated in a certain way, use the new helper method to write it.
			////txoStream.Write( this.shape.TxoData );
			//txoStream.WriteEntireTXORecord( this.shape.TxoData );
			//
			//txoStream.Close();
			//
			//manager.CurrentRecordStream.NextBlockType = BIFF8RecordType.MSODRAWING; 

			#endregion Old Code
			manager.ContextStack.Push( this.shape );
			manager.WriteRecord( BIFF8RecordType.TXO );
			manager.CurrentRecordStream.NextBlockType = BIFF8RecordType.MSODRAWING;
			manager.ContextStack.Pop(); // this.shape
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.ClientTextbox; }
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