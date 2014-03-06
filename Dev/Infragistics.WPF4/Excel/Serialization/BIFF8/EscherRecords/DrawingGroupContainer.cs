using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords

{
	internal sealed class DrawingGroupContainer : EscherRecordContainerBase
	{
		public DrawingGroupContainer( BIFF8WorkbookSerializationManager manager )
			: base( 0x0F, 0x0000, 0 ) 
		{
			this.AddChildRecord( new DrawingGroup( manager.Workbook.Worksheets ) );

			if ( manager.Images.Count > 0 )
				this.AddChildRecord( new BLIPStoreContainer( manager.Images ) );

			if ( manager.Workbook.DrawingProperties1 != null && manager.Workbook.DrawingProperties1.Count > 0 )
				this.AddChildRecord( new PropertyTable1( manager.Workbook.DrawingProperties1 ) );

			if ( manager.Workbook.DrawingProperties3 != null && manager.Workbook.DrawingProperties3.Count > 0 )
				this.AddChildRecord( new PropertyTable3( manager.Workbook.DrawingProperties3 ) );

			this.AddChildRecord( new SplitMenuColors() );
		}

		public DrawingGroupContainer( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( instance == 0x0000 );
		}

		protected override bool ValidateChildRecord( EscherRecordBase childRecord )
		{
			switch ( childRecord.Type )
			{
				case EscherRecordType.DrawingGroup:
				case EscherRecordType.ClassID:
				case EscherRecordType.PropertyTable1:
				case EscherRecordType.PropertyTable3:
				case EscherRecordType.ColorMRU:
				case EscherRecordType.SplitMenuColors:
				case EscherRecordType.BLIPStoreContainer:
					return true;
			}

			return false;
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.DrawingGroupContainer; }
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