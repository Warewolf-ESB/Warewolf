using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class XfrmElement : XmlElementBase
	{
		// MD 7/15/11 - Shape support
		private const float RotationUnitsPerDegree = 60000;

		#region XML Schema fragment <docs>
		// <complexType name="CT_GroupTransform2D">
		// <sequence>
		// <element name="off" type="CT_Point2D" minOccurs="0" maxOccurs="1"/>
		// <element name="ext" type="CT_PositiveSize2D" minOccurs="0" maxOccurs="1"/>
		// <element name="chOff" type="CT_Point2D" minOccurs="0" maxOccurs="1"/>
		// <element name="chExt" type="CT_PositiveSize2D" minOccurs="0" maxOccurs="1"/>
		// </sequence>
		// <attribute name="rot" type="ST_Angle" use="optional" default="0"/>
		// <attribute name="flipH" type="xsd:boolean" use="optional" default="false"/>
		// <attribute name="flipV" type="xsd:boolean" use="optional" default="false"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>xfrm</summary>
		public const string LocalName = "xfrm";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/xfrm</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			XfrmElement.LocalName;

		// MD 7/15/11 - Shape support
		private const string FlipHAttributeName = "flipH";
		private const string FlipVAttributeName = "flipV";
		private const string RotAttributeName = "rot";

		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return XfrmElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            //  Get the shape off the ContextStack
            WorksheetShape shape = manager.ContextStack[typeof(WorksheetShape)] as WorksheetShape;

			// MD 7/15/11 - Shape support
			// We now load and save the attributes for this element.
			object attributeValue = null;
			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					#region flipH

					case XfrmElement.FlipHAttributeName:
						attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
						shape.FlippedHorizontally = (bool)attributeValue;
						break;

					#endregion flipH

					#region flipV

					case XfrmElement.FlipVAttributeName:
						attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
						shape.FlippedVertically = (bool)attributeValue;
						break;

					#endregion flipV

					#region rot

					case XfrmElement.RotAttributeName:
						attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Int32, false);
						shape.Rotation = (int)attributeValue / XfrmElement.RotationUnitsPerDegree;
						break;

					#endregion rot
				}
			}

            //  Create a Transform object to hold the element and attribute values,
            //  and assign it to the Transform property of the shape's serialization
            //  cache. Note that we can't push it onto the ContextStack, because this element
            //  is a sibling of the ones that represent the shapes, so it will be popped off
            //  the stack before we can make use of it.
            Transform transform = shape is WorksheetShapeGroup ? new GroupTransform() : new Transform();
            shape.Excel2007ShapeSerializationManager.Transform = transform;
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
            WorksheetShape shape = manager.ContextStack[typeof(WorksheetShape)] as WorksheetShape;

			// MD 7/15/11 - Shape support
			// Added support for saving this element.

			string attributeValue;

			attributeValue = XmlElementBase.GetXmlString(shape.FlippedHorizontally, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, XfrmElement.FlipHAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(shape.FlippedVertically, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, XfrmElement.FlipVAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString((int)Math.Round(shape.Rotation * XfrmElement.RotationUnitsPerDegree), DataType.Int32, 0, false);
			XmlElementBase.AddAttribute(element, XfrmElement.RotAttributeName, attributeValue);

			bool isShapeGroup = shape is WorksheetShapeGroup;

			Transform transform = isShapeGroup ? new GroupTransform() : new Transform();
			transform.FromBoundsInTwips(shape.GetBoundsInTwips());
			manager.ContextStack.Push(transform);

			XmlElementBase.AddElement(element, OffElement.QualifiedName);
			XmlElementBase.AddElement(element, ExtElement.QualifiedName);

			if (isShapeGroup)
			{
				XmlElementBase.AddElement(element, ChOffElement.QualifiedName);
				XmlElementBase.AddElement(element, ChExtElement.QualifiedName);
			}
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