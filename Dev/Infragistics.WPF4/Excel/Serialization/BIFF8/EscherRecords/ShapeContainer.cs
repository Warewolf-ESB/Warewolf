using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal sealed class ShapeContainer : EscherRecordContainerBase
	{
		public ShapeContainer( BIFF8WorkbookSerializationManager manager, WorksheetShape shape )
			: base( 0x0F, 0x0000, 0 )
		{
			// MD 10/10/11 - TFS90805
			if (shape.Type2003.HasValue == false)
			{
				Utilities.DebugFail("This shape is invalid.");
				return;
			}

			shape.PopuplateDrawingProperties( manager );

			WorksheetShapeGroup group = shape as WorksheetShapeGroup;

			if ( group != null )
			{
				this.AddChildRecord( new GroupShape( group ) );

				if ( group is Shape.PatriarchShape )
				{
					this.AddChildRecord( new Shape( group ) );
					return;
				}
			}

			this.AddChildRecord( new Shape( shape ) );

			if ( shape.HasDrawingProperties1 )
			{
				// MD 9/2/08 - Cell Comments
				shape.DrawingProperties1.Sort();

				this.AddChildRecord( new PropertyTable1( shape ) );
			}

			if ( shape.HasDrawingProperties2 )
			{
				// MD 9/2/08 - Cell Comments
				shape.DrawingProperties2.Sort();

				this.AddChildRecord( new PropertyTable2( shape ) );
			}

			if ( shape.HasDrawingProperties3 )
			{
				// MD 9/2/08 - Cell Comments
				shape.DrawingProperties3.Sort();

				this.AddChildRecord( new PropertyTable3( shape ) );
			}

			this.AddChildRecord( ShapeContainer.GetAnchorRecord( shape ) );
			this.AddChildRecord( new ClientData( shape ) );

			// MD 9/11/08 - Cell Comments
			// Shapes do not store the TXO data anymore. Only shapes with text support TXO.
			//if ( shape.HasTxoData )
			// MD 11/8/11 - TFS85193
			// We need to check for both comments and shapes with text since WorksheetCellComment no longer derives from WorksheetShapeWithText.
			//WorksheetShapeWithText shapeWithText = shape as WorksheetShapeWithText;
			//if ( shapeWithText != null && shapeWithText.HasText )
			//    this.AddChildRecord( new ClientTextBox( shape ) );
			WorksheetShapeWithText shapeWithText = shape as WorksheetShapeWithText;
			WorksheetCellComment comment = shape as WorksheetCellComment;
			if ((shapeWithText != null && shapeWithText.HasText) ||
				(comment != null && comment.HasText))
			{
				this.AddChildRecord(new ClientTextBox(shape));
			}
		}

		public ShapeContainer( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( instance == 0x0000 );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			base.Load( manager );

			// Pop off the shape that was loaded and add it to the collection
			if ( manager.ContextStack.Current is WorksheetShape )
			{
				WorksheetShape shape = (WorksheetShape)manager.ContextStack.Pop();

				IWorksheetShapeOwner shapeOwner = (IWorksheetShapeOwner)manager.ContextStack[ typeof( IWorksheetShapeOwner ) ];

				if ( shapeOwner == null )
				{
					Utilities.DebugFail( "There is no shape owner in the context stack." );
					return;
				}

				// MD 7/20/2007 - BR25039
				// Use the new overload of the Add method to prevent verification of the shape type 
				// (We temporarily want to add comment shapes to the shapes collection even though they 
				// aren't allowed to be added to a shapes collection).
				//shapeOwner.Shapes.Add( shape );
				shapeOwner.Shapes.Add( shape, false );

				if ( shape is WorksheetShapeGroup )
					manager.ContextStack.Push( shape );
			}
		}

		private static EscherRecordBase GetAnchorRecord( WorksheetShape shape )
		{
			if ( shape.IsTopMost )
				return new ClientAnchor( shape );
			else
				return new ChildAnchor( shape );
		}

		protected override bool ValidateChildRecord( EscherRecordBase childRecord )
		{
			switch ( childRecord.Type )
			{
				case EscherRecordType.GroupShape:
				case EscherRecordType.Shape:
				case EscherRecordType.PropertyTable1:
				case EscherRecordType.PropertyTable2:
				case EscherRecordType.PropertyTable3:
				case EscherRecordType.Textbox:
				case EscherRecordType.ClientTextbox:
				case EscherRecordType.Anchor:
				case EscherRecordType.ChildAnchor:
				case EscherRecordType.ClientAnchor:
				case EscherRecordType.ClientData:
				case EscherRecordType.OLEObject:
				case EscherRecordType.DeletedPSPL:
					return true;
			}

			return false;
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.ShapeContainer; }
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