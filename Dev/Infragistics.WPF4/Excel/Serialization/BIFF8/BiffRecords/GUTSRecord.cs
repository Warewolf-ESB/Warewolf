using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords

{
	internal class GUTSRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			ushort widthOfRowOutlineArea = manager.CurrentRecordStream.ReadUInt16();
			ushort heightOfColumnOutlineArea = manager.CurrentRecordStream.ReadUInt16();

			ushort visibleRowOutlineLevels = manager.CurrentRecordStream.ReadUInt16();
			ushort visibleColumnOutlineLevels = manager.CurrentRecordStream.ReadUInt16();

			if ( visibleColumnOutlineLevels != 0 )
			{
				visibleColumnOutlineLevels--;

				Debug.Assert( heightOfColumnOutlineArea == ( 12 * visibleColumnOutlineLevels ) + 17 );
			}

			if ( visibleRowOutlineLevels != 0 )
			{
				visibleRowOutlineLevels--;

				Debug.Assert( widthOfRowOutlineArea == ( 12 * visibleRowOutlineLevels ) + 17 );
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			int maxColumnOutlineLevel = worksheet.MaxColumnOutlineLevel;
			int maxRowOutlineLevel = worksheet.MaxRowOutlineLevel;

			if ( maxRowOutlineLevel > 0 )
			{
				manager.CurrentRecordStream.Write( (ushort)( ( 12 * maxRowOutlineLevel ) + 17 ) );
				maxRowOutlineLevel++;
			}
			else
			{
				manager.CurrentRecordStream.Write( (ushort)0 );
			}

			if ( maxColumnOutlineLevel > 0 )
			{
				manager.CurrentRecordStream.Write( (ushort)( ( 12 * maxColumnOutlineLevel ) + 17 ) );
				maxColumnOutlineLevel++;
			}
			else
			{
				manager.CurrentRecordStream.Write( (ushort)0 );
			}

			manager.CurrentRecordStream.Write( (ushort)maxRowOutlineLevel );
			manager.CurrentRecordStream.Write( (ushort)maxColumnOutlineLevel );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.GUTS; }
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