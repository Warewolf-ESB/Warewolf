using System;
using Infragistics.Documents.Excel.Serialization.Excel2007;

// MD 7/14/11 - Shape support
namespace Infragistics.Documents.Excel.PredefinedShapes
{
	/// <summary>
	/// Represents the diamond shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class DiamondShape : WorksheetShapeWithText
	{
		/// <summary>
		/// Creates a new <see cref="DiamondShape"/> instance.
		/// </summary>
		public DiamondShape() { }

		internal DiamondShape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override ShapeType? Type2003
		{
			get { return ShapeType.Diamond; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.diamond; }
		}
	}

	/// <summary>
	/// Represents the ellipse shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class EllipseShape : WorksheetShapeWithText
	{
		/// <summary>
		/// Creates a new <see cref="EllipseShape"/> instance.
		/// </summary>
		public EllipseShape() { }

		internal EllipseShape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override ShapeType? Type2003
		{
			get { return ShapeType.Ellipse; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.ellipse; }
		}
	}

	/// <summary>
	/// Represents the heart shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class HeartShape : WorksheetShapeWithText
	{
		/// <summary>
		/// Creates a new <see cref="HeartShape"/> instance.
		/// </summary>
		public HeartShape() { }

		internal HeartShape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override ShapeType? Type2003
		{
			get { return ShapeType.Heart; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.heart; }
		}
	}

	/// <summary>
	/// Represents the irregular seal 1 shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class IrregularSeal1Shape : WorksheetShapeWithText
	{
		/// <summary>
		/// Creates a new <see cref="IrregularSeal1Shape"/> instance.
		/// </summary>
		public IrregularSeal1Shape() { }

		internal IrregularSeal1Shape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override ShapeType? Type2003
		{
			get { return ShapeType.IrregularSeal1; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.irregularSeal1; }
		}
	}

	/// <summary>
	/// Represents the irregular seal 2 shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class IrregularSeal2Shape : WorksheetShapeWithText
	{
		/// <summary>
		/// Creates a new <see cref="IrregularSeal2Shape"/> instance.
		/// </summary>
		public IrregularSeal2Shape() { }

		internal IrregularSeal2Shape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override ShapeType? Type2003
		{
			get { return ShapeType.IrregularSeal2; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.irregularSeal2; }
		}
	}

	/// <summary>
	/// Represents the lightning bolt shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class LightningBoltShape : WorksheetShapeWithText
	{
		/// <summary>
		/// Creates a new <see cref="LightningBoltShape"/> instance.
		/// </summary>
		public LightningBoltShape() { }

		internal LightningBoltShape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override ShapeType? Type2003
		{
			get { return ShapeType.LightningBolt; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.lightningBolt; }
		}
	}

	/// <summary>
	/// Represents the line shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class LineShape : WorksheetShape
	{
		/// <summary>
		/// Creates a new <see cref="LineShape"/> instance.
		/// </summary>
		public LineShape() { }

		internal LineShape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override bool IsConnector
		{
			get { return true; }
		}

		internal override ShapeType? Type2003
		{
			get { return ShapeType.Line; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.line; }
		}
	}

	/// <summary>
	/// Represents the pentagon shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class PentagonShape : WorksheetShapeWithText
	{
		/// <summary>
		/// Creates a new <see cref="PentagonShape"/> instance.
		/// </summary>
		public PentagonShape() { }

		internal PentagonShape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override ShapeType? Type2003
		{
			get { return ShapeType.Pentagon; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.pentagon; }
		}
	}

	/// <summary>
	/// Represents the rectangle shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class RectangleShape : WorksheetShapeWithText
	{
		/// <summary>
		/// Creates a new <see cref="RectangleShape"/> instance.
		/// </summary>
		public RectangleShape() { }

		internal RectangleShape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override ShapeType? Type2003
		{
			get { return ShapeType.Rectangle; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.rect; }
		}
	}

	/// <summary>
	/// Represents the right triangle shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class RightTriangleShape : WorksheetShapeWithText
	{
		/// <summary>
		/// Creates a new <see cref="RightTriangleShape"/> instance.
		/// </summary>
		public RightTriangleShape() { }

		internal RightTriangleShape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override ShapeType? Type2003
		{
			get { return ShapeType.RightTriangle; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.rtTriangle; }
		}
	}

	/// <summary>
	/// Represents the straight connector 1 shape.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)]
	public

		 class StraightConnector1Shape : WorksheetShape
	{
		/// <summary>
		/// Creates a new <see cref="StraightConnector1Shape"/> instance.
		/// </summary>
		public StraightConnector1Shape() { }

		internal StraightConnector1Shape(bool initializeDefaults)
			: base(initializeDefaults) { }

		internal override bool IsConnector
		{
			get { return true; }
		}

		internal override ShapeType? Type2003
		{
			get { return ShapeType.StraightConnector1; }
		}

		internal override ST_ShapeType? Type2007
		{
			get { return ST_ShapeType.straightConnector1; }
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