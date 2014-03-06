using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class PrstGeomElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_PresetGeometry2D">
		// <sequence>
		// <element name="avLst" type="CT_GeomGuideList" minOccurs="0" maxOccurs="1"/>
		// </sequence>
		// <attribute name="prst" type="ST_ShapeType" use="required"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>prstGeom</summary>
		public const string LocalName = "prstGeom";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/prstGeom</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			PrstGeomElement.LocalName;

		private const string PrstAttributeName = "prst";

		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return PrstGeomElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            //  Get the  off the context stack
			UnknownShape shape = manager.ContextStack[typeof(UnknownShape)] as UnknownShape;

            //  This element can also belong to a 'pic' element, in which case we
            //  don't need to continue.
            if ( shape == null )
                return;

			// MD 7/15/11 - Shape support
			// We should be using the ST_ShapeType enum.
			////  Get the value of the 'prst' attribute
			//ExcelXmlAttribute attribute = element.Attributes[PrstGeomElement.PrstAttributeName];
			//object attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.String, string.Empty );
			//
			////  Convert the value of the 'prst' attribute to a ShapeType constant
			//ShapeType shapeType = WorksheetShapeSerializationManager.ShapeTypeFromName( attributeValue as string );
			//  Get the value of the 'prst' attribute
			ExcelXmlAttribute attribute = element.Attributes[PrstGeomElement.PrstAttributeName];
			ST_ShapeType attributeValue = (ST_ShapeType)XmlElementBase.GetAttributeValue(attribute, DataType.ST_ShapeType, ST_ShapeType.rect);

			//  Convert the value of the 'prst' attribute to a ShapeType constant
			// MD 10/10/11 - TFS90805
			// This may be null.
			//ShapeType shapeType = WorksheetShape.ConvertShapeType(attributeValue);
			ShapeType? shapeType = WorksheetShape.ConvertShapeType(attributeValue);

			// MD 10/10/11 - TFS90805
			// Wrapped in an if statement because the conversion may fail.
			if (shapeType.HasValue)
			{
			// MD 7/15/11 - Shape support
			// If we can create the shape type, create it and replace the shape on the context stack.
			// MD 10/10/11 - TFS90805
			//PredefinedShapeType predefinedShapeType = (PredefinedShapeType)shapeType;
			PredefinedShapeType predefinedShapeType = (PredefinedShapeType)shapeType.Value;
			if (Enum.IsDefined(typeof(PredefinedShapeType), predefinedShapeType))
			{
				// MD 8/23/11 - TFS84306
				// Use the new overload and pass off False so we don't initialize the property defaults. 
				// We will load them from the file.
				//WorksheetShape replacementShape = WorksheetShape.CreatePredefinedShape(predefinedShapeType);
				WorksheetShape replacementShape = WorksheetShape.CreatePredefinedShape(predefinedShapeType, false);

				replacementShape.InitializeFrom(shape);
				manager.ContextStack.ReplaceItem(shape, replacementShape);
				return;
			}
			}

            //  Assign the ShapeType value to the UnknownShape.
			// MD 10/10/11 - TFS90805
            //shape.SetType( shapeType );
			shape.SetType(shapeType, attributeValue);
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
		}

			#endregion Save

		#endregion Base class overrides

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