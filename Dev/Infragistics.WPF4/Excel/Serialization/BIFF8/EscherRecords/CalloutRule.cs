using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords

{
	// MD 6/6/07 - BR23645
	// Added new escher record type
	internal sealed class CalloutRule : EscherRecordBase
	{
		// MD 6/14/07 - BR23880
		// Store the data for the record, it will be stored on the shape and serialized later
		private uint ruleId;
		private uint shapeId;

		public CalloutRule( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x00 );
			Debug.Assert( instance == 0x0000 );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// MD 6/14/07 - BR23880
			// Store the values in the new member variables now
			//uint ruleId = manager.CurrentRecordStream.ReadUInt32();
			//uint shapeId = manager.CurrentRecordStream.ReadUInt32();
			this.ruleId = manager.CurrentRecordStream.ReadUInt32();
			this.shapeId = manager.CurrentRecordStream.ReadUInt32();

			// MD 6/14/07 - BR23880
			// We need to assign the callout rule record to the associated shape
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			// Get the shape with the specified shape id
			WorksheetShape shape = worksheet.GetShapeById( shapeId );

			if ( shape == null )
			{
				Utilities.DebugFail( "There is no shape with the specified shape id." );
				return;
			}

			// Assign the callout rule to the shape
			shape.CalloutRule = this;
		}

		// MD 6/14/07 - BR23880
		// This record will now be serialized
		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			manager.CurrentRecordStream.Write( this.ruleId );
			manager.CurrentRecordStream.Write( this.shapeId );
		}

		// MD 6/14/07 - BR23880
		// The shape id may need to be modified
		public uint ShapeId
		{
			get { return this.shapeId; }
			set { this.shapeId = value; }
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.CalloutRule; }
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