using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Excel.Serialization.Excel2007;




using Infragistics.Shared;
using System.Drawing;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents an unsupported shape which has been loaded from a workbook file.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// This class is used for round-tripping purposes, so unsupported shapes which have been
	/// loaded can be saved back out with the workbook.  The class allows the unsupported shapes 
	/// to be identified in shape collections so they can be removed. This shape will become 
	/// obsolete in future versions when all shape types are supported.
	/// </p>
	/// </remarks>
	// MD 9/11/08 - Cell Comments
	// These shapes support text so they now derive from the new shape with text base type
	//public class UnknownShape : WorksheetShape



	public

		 class UnknownShape : WorksheetShapeWithText
		// MD 10/30/11 - TFS90733
		// Some unknown shapes (such as controls) may store an associated image.
		, IWorksheetImage
	{
		#region Member Variables

		// MD 7/20/2007 - BR25039
		// Use the new ShapeType enum
		//private ushort shapeType;
		// MD 10/10/11 - TFS90805
		//private ShapeType type;
		private ShapeType? type2003;
		private ST_ShapeType? type2007;

		// MD 8/1/07 - BR25039
		// Store the shape's options flags with the unknwown shape
		private uint shapeRecordOptionFlags;

		// MD 10/30/11 - TFS90733
		private Image image;
		private bool isOleShape;

		#endregion Member Variables

		#region Constructor

		// MD 7/20/2007 - BR25039
		// Use the new ShapeType enum
		//internal UnknownShape( ushort shapeType ) 
		//{
		//    this.shapeType = shapeType;
		//}
		// MD 8/1/07 - BR25039
		// Added a parameter
		//internal UnknownShape( ShapeType type )
		internal UnknownShape( ShapeType type, uint shapeRecordOptionFlags )
		{
			// MD 10/10/11 - TFS90805
			//this.type = type;
			this.type2003 = type;
			this.type2007 = WorksheetShape.ConvertShapeType(type);

			// MD 8/1/07 - BR25039
			// Store the shape's options flags with the unknown shape
			this.shapeRecordOptionFlags = shapeRecordOptionFlags;
		}

        //  BF 8/22/08  Excel2007 Format
        //  Need a parameterless ctor since the shape type is unknown
        //  at the time we serialize in the 'sp' element.
        internal UnknownShape(){}

		#endregion Constructor

		#region Interfaces

		// MD 10/30/11 - TFS90733
		// Some unknown shapes (such as controls) may store an associated image.
		#region IWorksheetImage Members

		Image IWorksheetImage.Image
		{
			get { return this.image; }
			set { this.image = value; }
		}

		#endregion

		#endregion // Interfaces

		#region Base Class Overrides

		// MD 8/1/07 - BR25039
		// Not all shapes can be serialized properly, they must be removed from the shapes collection
		#region CanBeAddedToShapesCollection

		internal override bool CanBeAddedToShapesCollection
		{
			get
			{
				// MD 10/10/11 - TFS90805
				#region Old Code

				// MD 10/10/11 - TFS90805
				//switch ( this.type )
				//{
				//    // MD 7/15/11 - Shape support
				//    // The connector shapes can be added to a shapes collection.
				//    //case ShapeType.StraightConnector1:
				//    //case ShapeType.BentConnector2:
				//    //case ShapeType.BentConnector3:
				//    //case ShapeType.BentConnector4:
				//    //case ShapeType.BentConnector5:
				//    //case ShapeType.CurvedConnector2:
				//    //case ShapeType.CurvedConnector3:
				//    //case ShapeType.CurvedConnector4:
				//    //case ShapeType.CurvedConnector5:
				//    case ShapeType.HostControl:
				//        return false;
				//
				//    default:
				//        return true;
				//}

				#endregion  // Old Code
				// MD 10/30/11 - TFS90733
				// We now support round-tripping controls, so we can allow these in the shapes collection as unknown shapes.
				//if (this.type2003.HasValue && 
				//	this.type2003.Value == ShapeType.HostControl)
				//	return false;

				return true;
			}
		}

		#endregion CanBeAddedToShapesCollection

		#region ClearUnknownData

		/// <summary>
		/// Throws an exception because all data in an unknown shape is unknown, and clearing that data would leave 
		/// no data with the shape.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// This method is called on an <see cref="UnknownShape"/> instance.
		/// </exception>
		public override void ClearUnknownData()
		{
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_ClearUnknownShapeData"));
		}

		#endregion ClearUnknownData

		// MD 3/12/12 - TFS102234
		#region InitializeDefaults

		internal override void InitializeDefaults() { }

		#endregion // InitializeDefaults

		// MD 10/10/11 - TFS90805
		#region Removed

		//// MD 7/20/2007 - BR25039
		//// Override new Type property to return the shape type
		//#region Type

		//internal override ShapeType Type
		//{
		//    get { return this.type; }
		//}

		//#endregion Type 

		#endregion  // Removed

		// MD 10/10/11 - TFS90805
		#region Type2003

		internal override ShapeType? Type2003
		{
			get { return this.type2003; }
		}

		#endregion  // Type2003

		// MD 10/10/11 - TFS90805
		#region Type2007

		internal override ST_ShapeType? Type2007
		{
			get { return this.type2007; }
		}

		#endregion  // Type2007

		#endregion Base Class Overrides

		#region Methods

		//  BF 8/22/08 Excel2007 Format
		#region SetType

		// MD 10/10/11 - TFS90805
		//internal void SetType( ShapeType type ) { this.type = type; }
		internal void SetType(ShapeType? type2003, ST_ShapeType type2007)
		{
			this.type2003 = type2003;
			this.type2007 = type2007;
		}

		#endregion SetType

		#endregion // Methods

		#region Properties

		// MD 10/30/11 - TFS90733
		#region IsOleShape

		internal bool IsOleShape
		{
			get { return this.isOleShape; }
			set { this.isOleShape = value; }
		}

		#endregion // IsOleShape

		// MD 8/1/07 - BR25039
		#region ShapeRecordOptionFlags

		internal uint ShapeRecordOptionFlags
		{
			get { return this.shapeRecordOptionFlags; }
		}

		#endregion ShapeRecordOptionFlags

		#endregion // Properties
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