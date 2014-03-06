using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal sealed class GroupContainer : EscherRecordContainerBase
	{
		public GroupContainer( BIFF8WorkbookSerializationManager manager, Worksheet worksheet )
			: base( 0x0F, 0x0000, 0 )
		{
			this.AddChildRecord( new ShapeContainer( manager, new Shape.PatriarchShape( worksheet ) ) );
			this.AddChildShapes( manager, worksheet.Shapes );

			// MD 7/20/2007 - BR25039
			// Comment shapes are always added at the end of the normal shapes in a worksheet
			if ( worksheet.HasCommentShapes )
			{
				// MD 9/2/08 - Cell Comments
				//foreach ( WorksheetCellCommentShape comment in worksheet.CommentShapes )
				foreach ( WorksheetCellComment comment in worksheet.CommentShapes )
					this.AddChildRecord( new ShapeContainer( manager, comment ) );
			}
		}

		public GroupContainer( BIFF8WorkbookSerializationManager manager, WorksheetShapeGroup group )
			: base( 0x0F, 0x0000, 0 )
		{
			this.AddChildRecord( new ShapeContainer( manager, group ) );
			this.AddChildShapes( manager, group.Shapes );
		}

		public GroupContainer( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( instance == 0x0000 );
		}

		private void AddChildShapes( BIFF8WorkbookSerializationManager manager, WorksheetShapeCollection shapes )
		{
			// MD 4/28/11 - TFS62775
			// This can now be done in a simpler way by using PrepareShapeForSerialization.
			#region Refactored

			//foreach ( WorksheetShape shape in shapes )
			//{
			//    WorksheetShapeGroup group = shape as WorksheetShapeGroup;
			//	
			//    if ( group != null )
			//    {
			//        if ( group.Shapes.Count == 1 )
			//            this.AddChildRecord( new ShapeContainer( manager, group.Shapes[ 0 ] ) );
			//        else if ( group.Shapes.Count > 1 )
			//            this.AddChildRecord( new GroupContainer( manager, group ) );
			//    }
			//    else
			//        this.AddChildRecord( new ShapeContainer( manager, shape ) );
			//}

			#endregion  // Refactored
			for (int i = 0; i < shapes.Count; i++)
			{
				WorksheetShape shape = shapes[i];
				manager.PrepareShapeForSerialization(ref shape);
				if (shape == null)
					continue;

				WorksheetShapeGroup group = shape as WorksheetShapeGroup;

				if (group != null)
					this.AddChildRecord(new GroupContainer(manager, group));
				else
					this.AddChildRecord(new ShapeContainer(manager, shape));
			}
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// MD 7/2/12 - TFS115692
			// Add a class to store information about the group being loaded.
			GroupInfo groupInfo = new GroupInfo();
			manager.ContextStack.Push(groupInfo);

			base.Load( manager );
			
			Debug.Assert( manager.ContextStack.Current is IWorksheetShapeOwner );
			WorksheetShapeGroup shapeGroup = manager.ContextStack.Pop() as WorksheetShapeGroup;
			
			if ( shapeGroup != null )
				shapeGroup.OnLoadComplete();

			// MD 7/2/12 - TFS115692
			Debug.Assert(manager.ContextStack.Current == groupInfo, "This is unexpected.");
			manager.ContextStack.Pop(); // groupInfo
		}

		protected override bool ValidateChildRecord( EscherRecordBase childRecord )
		{
			switch ( childRecord.Type )
			{
				case EscherRecordType.GroupContainer:
				case EscherRecordType.ShapeContainer:
					return true;
			}

			return false;
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.GroupContainer; }
		}
	}

	// MD 7/2/12 - TFS115692
	internal class GroupInfo
	{
		public Rectangle GroupBounds;
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