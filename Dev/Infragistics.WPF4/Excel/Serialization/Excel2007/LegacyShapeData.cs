using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
	internal class LegacyShapeData
	{
		#region Member Variables

		private int[] anchorData;
		private bool colHidden;

		// MD 4/12/11 - TFS67084
		// Use short instead of int so we don't have to cast.
		//private int columnIndex;
		private short columnIndex;

		private bool rowHidden;
		private int rowIndex;
		private int shapeId;
		private bool visible;

		// MD 1/18/12 - 12.1 - Cell Format Updates
		private WorksheetShape shape;

		#endregion Member Variables

		#region Constructor

		public LegacyShapeData()
		{
			this.anchorData = new int[ 8 ];
		} 

		#endregion Constructor

		#region InitializeAnchorData

		public void InitializeAnchorData( WorksheetShape shape )
		{
			// MD 1/18/12 - 12.1 - Cell Format Updates
			this.shape = shape;

			Worksheet worksheet = shape.Worksheet;

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (shape.TopLeftCornerCell.Worksheet == null || shape.BottomRightCornerCell.Worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			this.anchorData[ 0 ] = shape.TopLeftCornerCell.ColumnIndex;
			this.anchorData[ 2 ] = shape.TopLeftCornerCell.RowIndex;
			this.anchorData[ 4 ] = shape.BottomRightCornerCell.ColumnIndex;
			this.anchorData[ 6 ] = shape.BottomRightCornerCell.RowIndex;

			double leftColumnWidthInPixels = worksheet.GetColumnWidthInPixels( shape.TopLeftCornerCell.ColumnIndex );
			double leftOffsetPixels = ( ( leftColumnWidthInPixels * shape.TopLeftCornerPosition.X ) / 100 );

			// MD 3/16/12 - TFS105094
			// MidpointRoundingAwayFromZero now returns a double.
		    //this.anchorData[1] = Utilities.MidpointRoundingAwayFromZero(leftOffsetPixels);
			this.anchorData[1] = (int)MathUtilities.MidpointRoundingAwayFromZero(leftOffsetPixels);

			double topRowHeightInPixels = worksheet.GetRowHeightInPixels( shape.TopLeftCornerCell.RowIndex );
			double topOffsetPixels = ( ( topRowHeightInPixels * shape.TopLeftCornerPosition.Y ) / 100 );

			// MD 3/16/12 - TFS105094
			// MidpointRoundingAwayFromZero now returns a double.
            //this.anchorData[3] = Utilities.MidpointRoundingAwayFromZero(topOffsetPixels);
			this.anchorData[3] = (int)MathUtilities.MidpointRoundingAwayFromZero(topOffsetPixels);

			double rightColumnWidthInPixels = worksheet.GetColumnWidthInPixels( shape.BottomRightCornerCell.ColumnIndex );
			double rightOffsetPixels = ( ( rightColumnWidthInPixels * shape.BottomRightCornerPosition.X ) / 100 );

			// MD 3/16/12 - TFS105094
			// MidpointRoundingAwayFromZero now returns a double.
		    //this.anchorData[5] = Utilities.MidpointRoundingAwayFromZero(rightOffsetPixels);
			this.anchorData[5] = (int)MathUtilities.MidpointRoundingAwayFromZero(rightOffsetPixels);

			double bottomRowHeightInPixels = worksheet.GetRowHeightInPixels( shape.BottomRightCornerCell.RowIndex );
			double bottomOffsetPixels = ( ( bottomRowHeightInPixels * shape.BottomRightCornerPosition.Y ) / 100 );

			// MD 3/16/12 - TFS105094
			// MidpointRoundingAwayFromZero now returns a double.
		    //this.anchorData[7] = Utilities.MidpointRoundingAwayFromZero(bottomOffsetPixels);
			this.anchorData[7] = (int)MathUtilities.MidpointRoundingAwayFromZero(bottomOffsetPixels);
		} 

		#endregion InitializeAnchorData

		#region Properties

		public int[] AnchorData
		{
			get { return this.anchorData; }
		}

		public bool ColHidden
		{
			get { return this.colHidden; }
			set { this.colHidden = value; }
		}

		// MD 4/12/11 - TFS67084
		// Use short instead of int so we don't have to cast.
		//public int ColumnIndex
		public short ColumnIndex
		{
			get { return this.columnIndex; }
			set { this.columnIndex = value; }
		}

		public bool RowHidden
		{
			get { return this.rowHidden; }
			set { this.rowHidden = value; }
		}

		public int RowIndex
		{
			get { return this.rowIndex; }
			set { this.rowIndex = value; }
		}

		// MD 1/18/12 - 12.1 - Cell Format Updates
		public WorksheetShape Shape
		{
			get { return this.shape; }
		}

		public int ShapeId
		{
			get { return this.shapeId; }
			set { this.shapeId = value; }
		}

		public bool Visible
		{
			get { return this.visible; }
			set { this.visible = value; }
		}

		public bool VisibleResolved
		{
			get
			{
				return
					this.visible &&
					this.rowHidden == false &&
					this.colHidden == false;
			}
		}

		#endregion Properties
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