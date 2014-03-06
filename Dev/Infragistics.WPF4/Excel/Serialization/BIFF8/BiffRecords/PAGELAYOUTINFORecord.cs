using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords

{
	internal class PAGELAYOUTINFORecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			BIFF8RecordType repeatedRecordType = (BIFF8RecordType)manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert( repeatedRecordType == this.Type );

			manager.CurrentRecordStream.ReadUInt16(); // FRT flags, not used currently
			manager.CurrentRecordStream.ReadBytes( 8 ); // Not used

			// MD 11/11/09 - TFS24618
			// The method using this value requires an int parameter.
			//ushort magnificationInPageLayoutView = manager.CurrentRecordStream.ReadUInt16();
			int magnificationInPageLayoutView = manager.CurrentRecordStream.ReadUInt16();

			if ( magnificationInPageLayoutView == 0 )
				magnificationInPageLayoutView = DisplayOptions.DefaultMagnificationInPageLayoutView;

			// MD 11/11/09 - TFS24618
			// Make sure setting the magnification will not throw an exception.
			Utilities.EnsureMagnificationIsValid( ref magnificationInPageLayoutView );

			worksheet.DisplayOptions.MagnificationInPageLayoutView = magnificationInPageLayoutView;

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();

			bool displayInPageLayout =									( optionFlags & 0x0001 ) == 0x0001;
			worksheet.DisplayOptions.ShowRulerInPageLayoutView =		( optionFlags & 0x0002 ) == 0x0002;
			worksheet.DisplayOptions.ShowWhitespaceInPageLayoutView =	( optionFlags & 0x0004 ) != 0x0004;

			if ( displayInPageLayout )
				worksheet.DisplayOptions.View = WorksheetView.PageLayout;
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write( (ushort)this.Type );
			manager.CurrentRecordStream.Write( (ushort)0 );
			manager.CurrentRecordStream.Write( new byte[ 8 ] );
			manager.CurrentRecordStream.Write( (ushort)worksheet.DisplayOptions.MagnificationInPageLayoutView );

			ushort optionFlags = 0;

			if ( worksheet.DisplayOptions.View == WorksheetView.PageLayout )
				optionFlags |= 0x0001;

			if ( worksheet.DisplayOptions.ShowRulerInPageLayoutView )
				optionFlags |= 0x0002;

			if ( worksheet.DisplayOptions.ShowWhitespaceInPageLayoutView == false )
				optionFlags |= 0x0004;

			manager.CurrentRecordStream.Write( optionFlags );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.PAGELAYOUTINFO; }
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