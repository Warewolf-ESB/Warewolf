using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 9/12/08 - TFS6887
	internal class DVALRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort wDviFlags = manager.CurrentRecordStream.ReadUInt16();

			





			int xLeft = manager.CurrentRecordStream.ReadInt32(); // The x coordinate of the input window
			int yLeft = manager.CurrentRecordStream.ReadInt32(); // The y coordinate of the input window
			int idObj = manager.CurrentRecordStream.ReadInt32(); // Obj record id
			int idcMac = manager.CurrentRecordStream.ReadInt32(); // number of DV record

			// MD 2/1/11 - Data Validation support
			// This information doesn't seem to be honored in Excel 2007, so we are not going to round trip it anymore.
			//Debug.Assert( worksheet.DataValidationInfo2003 == null, "The worksheet should not have data validation round-trip data yet." );
			//worksheet.DataValidationInfo2003 = new DataValidationRoundTripInfo();

			//worksheet.DataValidationInfo2003.WDviFlags = wDviFlags;
			//worksheet.DataValidationInfo2003.XLeft = xLeft;
			//worksheet.DataValidationInfo2003.YLeft = yLeft;
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			// MD 2/1/11 - Data Validation support
			//if ( worksheet.DataValidationInfo2003 == null )
			//{
			//    Utilities.DebugFail("The worksheet had no data validation info.");
			//    return;
			//}
			//
			//manager.CurrentRecordStream.Write( worksheet.DataValidationInfo2003.WDviFlags );
			//
			//manager.CurrentRecordStream.Write( worksheet.DataValidationInfo2003.XLeft );
			//manager.CurrentRecordStream.Write( worksheet.DataValidationInfo2003.YLeft );
			//manager.CurrentRecordStream.Write( (int)-1 );
			//manager.CurrentRecordStream.Write( worksheet.DataValidationInfo2003.DataValidations.Count );
			if ( worksheet.HasDataValidationRules == false )
			{
                Utilities.DebugFail("The worksheet had no data validation info.");
				return;
			}

			manager.CurrentRecordStream.Write( (ushort)0 );

			manager.CurrentRecordStream.Write( 0 );
			manager.CurrentRecordStream.Write( 0 );
			manager.CurrentRecordStream.Write( (int)-1 );
			manager.CurrentRecordStream.Write( worksheet.DataValidationRules.Count );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.DVAL; }
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