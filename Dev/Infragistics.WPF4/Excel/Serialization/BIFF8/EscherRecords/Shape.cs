using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;





using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal class Shape : EscherRecordBase
	{
		// MD 7/20/2007 - BR25039
		// These constant are not needed anymore, the ShapeType enum has been created to replace them
		//private const int NotPrimitive = 0;
		//private const int PictureFrame = 75;

		private uint shapeId;
		private WorksheetShape shape;

		public Shape( WorksheetShape shape )
			// MD 7/20/2007 - BR25039
			// ResolveShapeType has been removed, use the Type property of WorksheetShape instead
			//: base( 0x02, ResolveShapeType( shape ), 8 )
			// MD 10/10/11 - TFS90805
			//: base( 0x02, (ushort)shape.Type, 8 )
			: base(0x02, (ushort)shape.Type2003.Value, 8)
		{
			this.shape = shape;
			this.shapeId = shape.ShapeId;
		}

		// MD 7/20/2007 - BR25039
		// Note needed anymore, the Type property has been added to WorksheetShape to get the shape type
		//private static ushort ResolveShapeType( WorksheetShape shape )
		//{
		//    if ( shape is WorksheetImage )
		//        return Shape.PictureFrame;
		//    else if ( shape is WorksheetShapeGroup )
		//        return Shape.NotPrimitive;
		//    else
		//        return ( (UnknownShape)shape ).ShapeType;
		//}

		public Shape( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x02 );
			Debug.Assert( recordLength == 8 );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			this.shapeId = manager.CurrentRecordStream.ReadUInt32();

			uint optionFlags = manager.CurrentRecordStream.ReadUInt32();

			bool isGroupShape =				( optionFlags & 0x00000001 ) == 0x00000001;
			//bool isChild =					( optionFlags & 0x00000002 ) == 0x00000002;
			bool isPatriarch =				( optionFlags & 0x00000004 ) == 0x00000004;
			bool isDeleted =				( optionFlags & 0x00000008 ) == 0x00000008;
			bool isOleShape =				( optionFlags & 0x00000010 ) == 0x00000010;
			bool hasMaster =				( optionFlags & 0x00000020 ) == 0x00000020;
			bool flippedHorizontally =		( optionFlags & 0x00000040 ) == 0x00000040;
			bool flippedVertically =		( optionFlags & 0x00000080 ) == 0x00000080;
			bool isConnector =				( optionFlags & 0x00000100 ) == 0x00000100;
			//bool hasAnchor =				( optionFlags & 0x00000200 ) == 0x00000200;
			bool isBackground =				( optionFlags & 0x00000400 ) == 0x00000400;
			//bool hasShapeTypeProperty =		( optionFlags & 0x00000800 ) == 0x00000800;

			if ( isDeleted )
				return;

			// MD 10/30/11 - TFS90733
			// We now support ole shapes.
			//Debug.Assert( isOleShape == false );

			Debug.Assert( hasMaster == false );
			// MD 8/1/07 - BR25039
			// These options may be true
			//Debug.Assert( flippedHorizontally == false );
			//Debug.Assert( flippedVertically == false );
			//Debug.Assert( isConnector == false );
			Debug.Assert( isBackground == false );

			// MD 7/2/12 - TFS115692
			GroupInfo groupInfo = manager.ContextStack.Get<GroupInfo>();

			if ( isPatriarch )
			{
				// MD 7/2/12 - TFS115692
				// The Rectangle is no longer placed directly on the ContextStack.
				//Debug.Assert( manager.ContextStack.Current is Rectangle );
				//if ( manager.ContextStack.Current is Rectangle )
				//    manager.ContextStack.Pop();

				Debug.Assert( manager.ContextStack[ typeof( WorksheetShapeCollection ) ] == null, "There should be no shape collection in the context stack" );

				// MD 7/2/12 - TFS115692
				// The Worksheet is not the first item on the stack anymore.
				//Debug.Assert( manager.ContextStack.Current is Worksheet );

				// Push the worksheet back on the stack
				// MD 7/2/12 - TFS115692
				//manager.ContextStack.Push( manager.ContextStack.Current );
				manager.ContextStack.Push(manager.ContextStack.Get<Worksheet>());
				return;
			}

			WorksheetShape shape;

			if ( isGroupShape )
			{
				// MD 7/20/2007 - BR25039
				// The shape stype constants has been replaced by the ShapeType enum
				//Debug.Assert( this.Instance == Shape.NotPrimitive );
				Debug.Assert( this.Instance == (ushort)ShapeType.NotPrimitive );

				// MD 7/2/12 - TFS115692
				// This is no longer necessary. Now the GroupItem item stores the loaded group bounds.
				//Rectangle groupBounds = Rectangle.Empty;
				//
				//Debug.Assert( manager.ContextStack.Current is Rectangle, "The rectangle had no group bounds." );
				//if ( manager.ContextStack.Current is Rectangle )
				//    groupBounds = (Rectangle)manager.ContextStack.Pop();

				WorksheetShapeGroup group = new WorksheetShapeGroup( true );

				// MD 7/2/12 - TFS115692
				//group.LoadedLocation = new Point(groupBounds.X, groupBounds.Y);

				shape = group;
			}
			// MD 7/20/2007 - BR25039
			// USe a switch statement instead, its faster that else-ifs
			//else if ( this.Instance == Shape.PictureFrame )
			//{
			//    shape = new WorksheetImage();
			//}
			//else
			//{
			//    shape = new UnknownShape( this.Instance );
			//}
			else
			{
				ShapeType type = (ShapeType)this.Instance;

				switch ( type )
				{
					case ShapeType.NotPrimitive:
						// MD 11/1/11
						// Found while fixing TFS90733
						// This is not true. Polygons are not primitives or groups. We should still store them as unknown shapes.
						//Utilities.DebugFail( "This should have had isGroupShape set to true." );
						goto default;

					case ShapeType.PictureFrame:
						// MD 1/6/12 - TFS92740
						// Don't initialize default properties when we are loading.
						//shape = new WorksheetImage();
						shape = new WorksheetImage(false);
						break;

					case ShapeType.TextBox:
						// MD 5/4/09 - TFS17197
						// Not all TextBox shapes are cell comments. Some are actually text boxes.
						//// MD 9/2/08 - Cell Comments
						////shape = new WorksheetCellCommentShape();
						//shape = new WorksheetCellComment();
                        shape = new UnknownShape( type, optionFlags );
						break;

					default:
						// MD 8/1/07 - BR25039
						// The unknown shape constructor now takes another parameter
						//shape = new UnknownShape( type );
						// MD 7/18/11 - Shape support
						//shape = new UnknownShape( type, optionFlags );
						PredefinedShapeType predefinedShapeType = (PredefinedShapeType)type;
						if (Enum.IsDefined(typeof(PredefinedShapeType), predefinedShapeType))
						{
							// MD 8/23/11 - TFS84306
							// Use the new overload and pass off False so we don't initialize the property defaults. 
							// We will load them from the file.
							//shape = WorksheetShape.CreatePredefinedShape(predefinedShapeType);
							shape = WorksheetShape.CreatePredefinedShape(predefinedShapeType, false);
						}
						else
							shape = new UnknownShape(type, optionFlags);

						break;
				}
			}

			// MD 10/30/11 - TFS90733
			// Store the isOleShape flag.
			UnknownShape unknownShape = shape as UnknownShape;
			if (unknownShape != null)
				unknownShape.IsOleShape = isOleShape;
			else
				Debug.Assert(isOleShape == false || shape is WorksheetImage, "We are not expecting isOleShape to be true on any current 'known' shapes other than images.");

			// MD 6/14/07 - BR23880
			// Store the deserialized shape id on the shape
			shape.ShapeId = this.shapeId;

			shape.FlippedHorizontally = flippedHorizontally;
			shape.FlippedVertically = flippedVertically;

			manager.ContextStack.Push( shape );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			uint optionFlags = 0;

			// MD 8/1/07 - BR25039
			// Merge in all unknown option flags from the unknown shape
			UnknownShape unknownShape = this.shape as UnknownShape;
			if ( unknownShape != null )
			{
				optionFlags = (uint)( unknownShape.ShapeRecordOptionFlags & ~0x00000A07 );
			}
			// MD 4/28/11 - TFS62775
			else
			{
				WorksheetChart chartShape = this.shape as WorksheetChart;
				if (chartShape != null)
					optionFlags = (uint)(chartShape.ShapeRecordOptionFlags & ~0x00000A07);
			}

			// isGroupShape
			if ( this.shape is WorksheetShapeGroup )
				optionFlags |= 0x00000001;

			// isChild
			if ( this.shape.IsTopMost == false )
				optionFlags |= 0x00000002;

			// isPatriarch
			if ( this.shape is PatriarchShape )
				optionFlags |= 0x00000004;

			// isDeleted is always false

			// MD 10/30/11 - TFS90733
			// isOleShape
			if (unknownShape != null && unknownShape.IsOleShape)
				optionFlags |= 0x00000010;

			// MD 7/18/11 - Shape support
			// --------------------------------------
			// flippedHorizontally
			if (this.shape.FlippedHorizontally)
				optionFlags |= 0x00000040;

			// flippedVertically
			if (this.shape.FlippedVertically)
				optionFlags |= 0x00000080;

			// isConnector
			if (this.shape.IsConnector)
				optionFlags |= 0x00000100;
			// ------------ Shape support -----------

			// hasAnchor
			if ( ( this.shape is PatriarchShape ) == false )
				optionFlags |= 0x00000200;

			// hasShapeTypeProperty
			if ( this.Instance != 0 )
				optionFlags |= 0x00000800;

			manager.CurrentRecordStream.Write( this.shapeId );
			manager.CurrentRecordStream.Write( optionFlags );
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( "------------------------------" );
			sb.Append( "\n" );

			sb.Append( base.ToString() );
			sb.Append( "\n\n" );

			sb.Append( "Shape Type: " + this.Instance );
			sb.Append( "\n" );

			sb.Append( "Shape ID: " + this.shapeId );
			sb.Append( "\n" );

			sb.Append( "------------------------------" );

			return sb.ToString();
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.Shape; }
		}

		public class PatriarchShape : WorksheetShapeGroup
		{
			private Worksheet worksheet;

			public PatriarchShape( Worksheet worksheet )
			{
				this.worksheet = worksheet;
			}

			// MD 3/24/10 - TFS28374
			// This virtual method has a new parameter added to it.
			//public override Rectangle GetBoundsInTwips()
			public override Rectangle GetBoundsInTwips(PositioningOptions options)
			{
				return Rectangle.Empty;
			}

			internal override bool IsTopMost
			{
				get { return true; }
			}

			internal override uint ShapeId
			{
				get { return this.worksheet.PatriarchShapeId; }
				set { Utilities.DebugFail( "This shape id cannot be set" ); }
			}
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