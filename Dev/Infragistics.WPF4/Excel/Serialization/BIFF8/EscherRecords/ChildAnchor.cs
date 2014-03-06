using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal class ChildAnchor : EscherRecordBase
	{
		private Rectangle rect;

		public ChildAnchor( WorksheetShape shape )
			: base( 0x00, 0x0000, 16 )
		{
			this.rect = shape.GetBoundsInTwips();
		}

		public ChildAnchor( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x00 );
			Debug.Assert( instance == 0x0000 );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Rectangle rect = Utilities.ReadEMURect( manager.CurrentRecordStream );

			WorksheetShape shape = (WorksheetShape)manager.ContextStack[ typeof( WorksheetShape ) ];

			if ( shape == null )
			{
				Utilities.DebugFail( "There is no shape in the context stack." );
				return;
			}

			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
				Utilities.DebugFail( "There is worksheet in the context stack." );
				return;
			}

			// MD 7/2/12 - TFS115692
			GroupInfo groupInfo = manager.ContextStack.Get<GroupInfo>();
			if (groupInfo == null)
			{
				Utilities.DebugFail("There is no GroupInfo on the context stack.");
				return;
			}

			// MD 5/16/07 - BR23017
			// If the shape is a group, getting the first shape group in the context stack will get the shape again and 
			// not the group that we want to be the parent of the shape, so go up one more level to get the parent group.
			//WorksheetShapeGroup group = (WorksheetShapeGroup)manager.ContextStack[ typeof( WorksheetShapeGroup ) ];
			WorksheetShapeGroup group;

			if ( shape is WorksheetShapeGroup )
				group = (WorksheetShapeGroup)manager.ContextStack[ typeof( WorksheetShapeGroup ), 1 ];
			else
				group = (WorksheetShapeGroup)manager.ContextStack[ typeof( WorksheetShapeGroup ) ];

			if ( group == null )
			{
				Utilities.DebugFail( "There is no group in the context stack." );
				return;
			}

			Debug.Assert( group != null );
			if ( group != null )
			{
				// MD 7/2/12 - TFS115692
				// These calculations are incorrect.
				//Rectangle groupBounds = group.GetBoundsInTwips();
				//
				//rect.Offset( groupBounds.X, groupBounds.Y );
				//rect.Offset( -group.LoadedLocation.X, -group.LoadedLocation.Y );
				// MD 7/24/12 - TFS115693
				// This conversion method also needs the workbook format and shape rotation.
				//rect = Utilities.GetAbsoluteShapeBounds(rect, groupInfo.GroupBounds, group.GetBoundsInTwips());
				rect = Utilities.GetAbsoluteShapeBounds(
					rect, groupInfo.GroupBounds, group.GetBoundsInTwips(), 
					manager.Workbook.CurrentFormat, shape.Rotation);
			}

			shape.SetBoundsInTwips( worksheet, rect );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			Utilities.WriteEMURect( manager.CurrentRecordStream, this.rect );
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( "------------------------------" );
			sb.Append( "\n" );

			sb.Append( base.ToString() );
			sb.Append( "\n\n" );

			sb.Append( "Shape Bounds (Twips): " + this.rect );
			sb.Append( "\n" );

			sb.Append( "------------------------------" );

			return sb.ToString();
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.ChildAnchor; }
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