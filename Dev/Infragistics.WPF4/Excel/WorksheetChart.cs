using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Excel.Serialization.Excel2007;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 10/12/10 - TFS49853
	/// <summary>
	/// Represents a chart shape. This class does not allow for the editing of chart data. 
	/// It is used for round-tripping chart data between loading and saving a workbook.
	/// </summary>



	public

		 class WorksheetChart : WorksheetShape
	{
		#region Member Variables

		// MD 4/28/11 - TFS62775
		// Separated this into two data blocks so we can maintain round trip data when the format is changed and changed back.
		//private byte[] data;
		private byte[] excel2003RoundTripData;
		private byte[] excel2007RoundTripData;

		// MD 4/28/11 - TFS62775
		// This is only used for the 2003 format.
		private uint shapeRecordOptionFlags;

		#endregion // Member Variables

		#region Constructor

		// MD 4/28/11 - TFS62775
		internal WorksheetChart() { }

		// MD 4/28/11 - TFS62775
		// Added a copy constructor to initialize a new WorksheetChart from an existing UnknownShape.
		internal WorksheetChart(UnknownShape unknownShape)
			: base(unknownShape)
		{
			this.shapeRecordOptionFlags = unknownShape.ShapeRecordOptionFlags;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		// MD 10/10/11 - TFS90805
		#region Removed

		//#region Type

		//internal override ShapeType Type
		//{
		//    get { return ShapeType.HostControl; }
		//}

		//#endregion // Type

		#endregion  // Removed 

		// MD 10/10/11 - TFS90805
		#region Type2003

		internal override ShapeType? Type2003
		{
			get { return ShapeType.HostControl; }
		}

		#endregion  // Type2003

		// MD 10/10/11 - TFS90805
		#region Type2007

		internal override ST_ShapeType? Type2007
		{
			get { return null; }
		}

		#endregion  // Type2007

		// MD 7/14/11 - Shape support
		#region VerifyOrientationChange

		internal override void VerifyOrientationChange()
		{
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_ShapeCantChangeOrientation"));
		}

		#endregion // VerifyOrientationChange

		#endregion // Base Class Overrides

		#region Properties

		// MD 4/28/11 - TFS62775
		// Separated this into two data blocks so we can maintain round trip data when the format is changed and changed back.
		#region Removed

		//#region Data
		//
		//internal byte[] Data
		//{
		//    get { return this.data; }
		//    set { this.data = value; }
		//}
		//
		//#endregion // Data 

		#endregion  // Removed

		#region Excel2003RoundTripData

		internal byte[] Excel2003RoundTripData
		{
			get { return this.excel2003RoundTripData; }
			set { this.excel2003RoundTripData = value; }
		}

		#endregion // Excel2003RoundTripData

		#region Excel2007RoundTripData

		internal byte[] Excel2007RoundTripData
		{
			get { return this.excel2007RoundTripData; }
			set { this.excel2007RoundTripData = value; }
		}

		#endregion // Excel2007RoundTripData

		// MD 4/28/11 - TFS62775
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