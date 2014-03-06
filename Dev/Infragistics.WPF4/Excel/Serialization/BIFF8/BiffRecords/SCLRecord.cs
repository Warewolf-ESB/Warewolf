using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd922321(v=office.12)
	internal class SCLRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort numerator = manager.CurrentRecordStream.ReadUInt16();
			ushort denominator = manager.CurrentRecordStream.ReadUInt16();

			int magnification = (int)( ( numerator / (double)denominator ) * 100 );

			// MD 11/11/09 - TFS24618
			// Make sure setting the magnification will not throw an exception.
			Utilities.EnsureMagnificationIsValid( ref magnification );

			if ( worksheet.DisplayOptions.View == WorksheetView.Normal )
			{
				Debug.Assert( worksheet.DisplayOptions.MagnificationInNormalView == magnification );
				worksheet.DisplayOptions.MagnificationInNormalView = magnification;
			}
			else if ( worksheet.DisplayOptions.View == WorksheetView.PageBreakPreview )
			{
				Debug.Assert( worksheet.DisplayOptions.MagnificationInPageBreakView == magnification );
				worksheet.DisplayOptions.MagnificationInPageBreakView = magnification;
			}
			else
			{
				Debug.Assert( worksheet.DisplayOptions.MagnificationInPageLayoutView == magnification );
				worksheet.DisplayOptions.MagnificationInPageLayoutView = magnification;
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

			int magnification;

			if ( worksheet.DisplayOptions.View == WorksheetView.Normal )
				magnification = worksheet.DisplayOptions.MagnificationInNormalView;
			else if ( worksheet.DisplayOptions.View == WorksheetView.PageBreakPreview )
				magnification = worksheet.DisplayOptions.MagnificationInPageBreakView;
			else
				magnification = worksheet.DisplayOptions.MagnificationInPageLayoutView;

			int numerator = magnification;
			int denominator = 100;

			Utilities.ReduceFraction( ref numerator, ref denominator );

			manager.CurrentRecordStream.Write( (ushort)numerator );
			manager.CurrentRecordStream.Write( (ushort)denominator );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.SCL; }
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