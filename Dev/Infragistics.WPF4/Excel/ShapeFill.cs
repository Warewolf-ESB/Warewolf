using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;
using Infragistics.Documents.Excel.Serialization.Excel2007;




using System.Drawing;


namespace Infragistics.Documents.Excel
{
	// MD 8/23/11 - TFS84306
	#region ShapeFill class

	/// <summary>
	/// Abstract base class for the fill of a shape.
	/// </summary>
	/// <seealso cref="WorksheetShape.Fill"/>
	/// <seealso cref="ShapeFillSolid"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 
	public

		 abstract class ShapeFill
	{
		/// <summary>
		/// Creates an instance to describe a solid fill outline.
		/// </summary>
		/// <param name="solidColor">The color of the fill to create.</param>
		/// <returns>A <see cref="ShapeFillSolid"/> instance with the specified color.</returns>
		public static ShapeFill FromColor(Color solidColor)
		{
			return new ShapeFillSolid(solidColor);
		}

		internal abstract void PopulateDrawingProperties2003(WorksheetShape shape);
		internal abstract void PopulateDrawingProperties2007(ElementDataCache shapePropertiesElement);
	}

	#endregion  // ShapeFill class

	// MD 8/23/11 - TFS84306
	#region ShapeFillNoFill class

	internal sealed class ShapeFillNoFill : ShapeFill
	{
		internal static readonly ShapeFillNoFill Instance = new ShapeFillNoFill();

		private ShapeFillNoFill() { }

		internal override void PopulateDrawingProperties2003(WorksheetShape shape)
		{
			uint fillStyleNoFillHitTest = WorksheetShape.FillStyleNoFillHitTest_Default;
			shape.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.FillStyleNoFillHitTest, fillStyleNoFillHitTest));
		}

		internal override void PopulateDrawingProperties2007(ElementDataCache shapePropertiesElement)
		{
			WorksheetShapeSerializationManager.SerializeNoFill(shapePropertiesElement);
		}
	}

	#endregion  // ShapeFillNoFill class

	// MD 8/23/11 - TFS84306
	#region ShapeFillSolid class

	/// <summary>
	/// Represents a shape fill with a solid color.
	/// </summary>
	/// <seealso cref="WorksheetShape.Fill"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 
	public

		 class ShapeFillSolid : ShapeFill, ISolidColorItem
	{
		#region Member Variables

		private Color color;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="ShapeFillSolid"/> instance.
		/// </summary>
		public ShapeFillSolid() { }

		/// <summary>
		/// Creates a new <see cref="ShapeFillSolid"/> instance.
		/// </summary>
		/// <param name="color">The color of the fill.</param>
		public ShapeFillSolid(Color color)
		{
			this.color = color;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		internal override void PopulateDrawingProperties2003(WorksheetShape shape)
		{
			shape.PopulateColorProperties(this.Color, PropertyType.FillStyleColor, PropertyType.FillStyleOpacity);

			uint fillStyleNoFillHitTest =
				WorksheetShape.FillStyleNoFillHitTest_Default |
				WorksheetShape.FillStyleNoFillHitTest_HasFill;

			shape.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.FillStyleNoFillHitTest, fillStyleNoFillHitTest));
		}

		internal override void PopulateDrawingProperties2007(ElementDataCache shapePropertiesElement)
		{
			WorksheetShapeSerializationManager.SerializeSolidFill(shapePropertiesElement, this.Color);
		}

		#endregion  // Base Class Overrides

		#region Properties

		/// <summary>
		/// Gets or sets the color of the fill.
		/// </summary>
		public Color Color
		{
			get { return this.color; }
			set { this.color = value; }
		}

		#endregion  // Properties
	}

	#endregion  // ShapeFillSolid class
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