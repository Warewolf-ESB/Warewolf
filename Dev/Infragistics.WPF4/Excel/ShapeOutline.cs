using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing;




using System.Drawing;


namespace Infragistics.Documents.Excel
{
	// MD 8/23/11 - TFS84306
	#region ShapeOutline class

	/// <summary>
	/// Abstract base class for the outline of a shape.
	/// </summary>
	/// <seealso cref="WorksheetShape.Outline"/>
	/// <seealso cref="ShapeOutlineSolid"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 
	public

		 abstract class ShapeOutline
	{
		// MD 7/3/12 - TFS115689
		// Added round trip support for line end properties.
		private LineEndProperties headEndProperties;
		private LineEndProperties tailEndProperties;

		// MD 3/12/12 - TFS102234
		private int widthInternal;

		/// <summary>
		/// Creates an instance to describe a solid color outline.
		/// </summary>
		/// <param name="solidColor">The color of the outline to create.</param>
		/// <returns>A <see cref="ShapeOutlineSolid"/> instance with the specified color.</returns>
		public static ShapeOutline FromColor(Color solidColor)
		{
			return new ShapeOutlineSolid(solidColor);
		}

		internal static uint GetDefaultLineStyleNoLineDrawDash(WorksheetShape shape)
		{
			uint lineStyleNoLineDrawDash = WorksheetShape.LineStyleNoLineDrawDash_Default;

			if (shape.IsConnector)
				lineStyleNoLineDrawDash |= WorksheetShape.LineStyleNoLineDrawDash_ConnectorFlags;
			return lineStyleNoLineDrawDash;
		}

		internal abstract void PopulateDrawingProperties2003(WorksheetShape shape);
		internal abstract void PopulateDrawingProperties2007(ElementDataCache shapePropertiesElement);

		// MD 7/3/12 - TFS115689
		// Added round trip support for line end properties.
		internal void PopulateLineEndProperties2007(ElementDataCache parentElement)
		{
			if (this.headEndProperties != null)
			{
				ElementDataCache headElement = new ElementDataCache(HeadEndElement.QualifiedName);
				WorksheetShapeSerializationManager.SerializeLineEndProperties(headElement, this.headEndProperties);
				parentElement.Elements.Add(headElement);
			}

			if (this.tailEndProperties != null)
			{
				ElementDataCache tailElement = new ElementDataCache(TailEndElement.QualifiedName);
				WorksheetShapeSerializationManager.SerializeLineEndProperties(tailElement, this.tailEndProperties);
				parentElement.Elements.Add(tailElement);
			}
		}

		// MD 7/3/12 - TFS115689
		// Added round trip support for line end properties.
		internal LineEndProperties HeadEndProperties
		{
			get { return this.headEndProperties; }
			set { this.headEndProperties = value; }
		}

		// MD 7/3/12 - TFS115689
		// Added round trip support for line end properties.
		internal LineEndProperties TailEndProperties
		{
			get { return this.tailEndProperties; }
			set { this.tailEndProperties = value; }
		}

		// MD 3/12/12 - TFS102234
		internal int WidthInternal
		{
			get { return this.widthInternal; }
			set { this.widthInternal = value; }
		}
	}

	#endregion  // ShapeOutline class

	// MD 8/23/11 - TFS84306
	#region ShapeOutlineNoOutline class

	internal sealed class ShapeOutlineNoOutline : ShapeOutline
	{
		internal static readonly ShapeOutlineNoOutline Instance = new ShapeOutlineNoOutline();

		private ShapeOutlineNoOutline() { }

		internal override void PopulateDrawingProperties2003(WorksheetShape shape)
		{
			uint lineStyleNoLineDrawDash = ShapeOutline.GetDefaultLineStyleNoLineDrawDash(shape);
			shape.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.LineStyleNoLineDrawDash, lineStyleNoLineDrawDash));
		}

		internal override void PopulateDrawingProperties2007(ElementDataCache shapePropertiesElement)
		{
			//  Add a 'spPr/ln' element
			ElementDataCache ln_Element = new ElementDataCache(LnElement.QualifiedName);
			shapePropertiesElement.Elements.Add(ln_Element);

			WorksheetShapeSerializationManager.SerializeNoFill(ln_Element);

			// MD 7/3/12 - TFS115689
			// Added round trip support for line end properties.
			this.PopulateLineEndProperties2007(ln_Element);
		}
	}

	#endregion  // ShapeOutlineNoOutline class

	// MD 8/23/11 - TFS84306
	#region ShapeOutlineSolid class

	/// <summary>
	/// Represents a shape outline with a solid color.
	/// </summary>
	/// <seealso cref="WorksheetShape.Outline"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 
	public

		 class ShapeOutlineSolid : ShapeOutline, ISolidColorItem
	{
		#region Member Variables

		private Color color;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="ShapeOutlineSolid"/> instance.
		/// </summary>
		public ShapeOutlineSolid() { }

		/// <summary>
		/// Creates a new <see cref="ShapeOutlineSolid"/> instance.
		/// </summary>
		/// <param name="color">The color of the outline.</param>
		public ShapeOutlineSolid(Color color)
		{
			this.color = color;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		internal override void PopulateDrawingProperties2003(WorksheetShape shape)
		{
			uint lineStyleNoLineDrawDash =
				ShapeOutline.GetDefaultLineStyleNoLineDrawDash(shape) |
				WorksheetShape.LineStyleNoLineDrawDash_HasOutline;

			shape.PopulateColorProperties(this.Color, PropertyType.LineStyleColor, PropertyType.LineStyleOpacity);
			shape.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.LineStyleNoLineDrawDash, lineStyleNoLineDrawDash));
		}

		internal override void PopulateDrawingProperties2007(ElementDataCache shapePropertiesElement)
		{
			//  Add a 'spPr/ln' element
			ElementDataCache ln_Element = new ElementDataCache(LnElement.QualifiedName);
			shapePropertiesElement.Elements.Add(ln_Element);

			// MD 3/12/12 - TFS102234
			// Save out the line width if it was loaded.
			if (this.WidthInternal != 0)
				ln_Element.AttributeValues.Add(LnElement.WAttributeName, this.WidthInternal.ToString());

			WorksheetShapeSerializationManager.SerializeSolidFill(ln_Element, this.Color);

			// MD 7/3/12 - TFS115689
			// Added round trip support for line end properties.
			this.PopulateLineEndProperties2007(ln_Element);
		}

		#endregion  // Base Class Overrides

		#region Properties

		/// <summary>
		/// Gets or sets the color of the outline.
		/// </summary>
		public Color Color
		{
			get { return this.color; }
			set { this.color = value; }
		}

		#endregion  // Properties
	}

	#endregion  // ShapeOutlineSolid class
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