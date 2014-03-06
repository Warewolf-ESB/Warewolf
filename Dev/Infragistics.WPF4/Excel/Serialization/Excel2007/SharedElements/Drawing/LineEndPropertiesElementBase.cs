using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{
	// MD 7/3/12 - TFS115689
	// Added round trip support for line end properties.
	internal abstract class LineEndPropertiesElementBase : XmlElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_LineEndProperties">
		//  <attribute name="type" type="ST_LineEndType" use="optional"/>
		//  <attribute name="w" type="ST_LineEndWidth" use="optional"/>
		//  <attribute name="len" type="ST_LineEndLength" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants

		internal const string LenAttributeName = "len";
		internal const string TypeAttributeName = "type";
		internal const string WAttributeName = "w";

		#endregion // Constants

		#region Base Class Overrides

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			ShapeOutline outline = manager.ContextStack.Get<ShapeOutline>();
			if (outline == null)
			{
				Utilities.DebugFail("Cannot find the ShapeOutline on the stack");
				return;
			}

			ST_LineEndLength? len = null;
			ST_LineEndType? type = null;
			ST_LineEndWidth? w = null;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case LineEndPropertiesElementBase.LenAttributeName:
						len = (ST_LineEndLength)XmlElementBase.GetAttributeValue(attribute, DataType.ST_LineEndLength, len);
						break;

					case LineEndPropertiesElementBase.TypeAttributeName:
						type = (ST_LineEndType)XmlElementBase.GetAttributeValue(attribute, DataType.ST_LineEndType, type);
						break;

					case LineEndPropertiesElementBase.WAttributeName:
						w = (ST_LineEndWidth)XmlElementBase.GetAttributeValue(attribute, DataType.ST_LineEndWidth, w);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			LineEndProperties properties = new LineEndProperties();

			if (len != null)
				properties.len = len.Value;

			if (type != null)
				properties.type = type.Value;

			if (w != null)
				properties.w = w.Value;

			this.SetLineEndProperties(outline, properties);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			ShapeOutline outline = manager.ContextStack.Get<ShapeOutline>();
			if (outline == null)
			{
				Utilities.DebugFail("Cannot find the ShapeOutline on the stack");
				return;
			}

			LineEndProperties properties = this.GetLineEndProperties(outline);
			if (properties == null)
				return;

			string attributeValue = String.Empty;

			// Add the 'len' attribute
			attributeValue = XmlElementBase.GetXmlString(properties.len, DataType.ST_LineEndLength, default(ST_LineEndLength), false);
			XmlElementBase.AddAttribute(element, LineEndPropertiesElementBase.LenAttributeName, attributeValue);

			// Add the 'type' attribute
			attributeValue = XmlElementBase.GetXmlString(properties.type, DataType.ST_LineEndType, default(ST_LineEndType), false);
			XmlElementBase.AddAttribute(element, LineEndPropertiesElementBase.TypeAttributeName, attributeValue);

			// Add the 'w' attribute
			attributeValue = XmlElementBase.GetXmlString(properties.w, DataType.ST_LineEndWidth, default(ST_LineEndWidth), false);
			XmlElementBase.AddAttribute(element, LineEndPropertiesElementBase.WAttributeName, attributeValue);
		}

		#endregion // Save

		#endregion // Base Class Overrides

		protected abstract LineEndProperties GetLineEndProperties(ShapeOutline outline);
		protected abstract void SetLineEndProperties(ShapeOutline outline, LineEndProperties properties);
	}

	internal class HeadEndElement : LineEndPropertiesElementBase
	{
		#region Constants






		public const string LocalName = "headEnd";






		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			HeadEndElement.LocalName;

		#endregion // Constants

		#region Base Class Overrides

		#region GetLineEndProperties

		protected override LineEndProperties GetLineEndProperties(ShapeOutline outline)
		{
			return outline.HeadEndProperties;
		}

		#endregion // GetLineEndProperties

		#region ElementName

		public override string ElementName
		{
			get { return HeadEndElement.QualifiedName; }
		}

		#endregion ElementName

		#region SetLineEndProperties

		protected override void SetLineEndProperties(ShapeOutline outline, LineEndProperties properties)
		{
			outline.HeadEndProperties = properties;
		}

		#endregion // SetLineEndProperties

		#endregion // Base Class Overrides
	}

	internal class TailEndElement : LineEndPropertiesElementBase
	{
		#region Constants






		public const string LocalName = "tailEnd";






		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			TailEndElement.LocalName;

		#endregion // Constants

		#region Base Class Overrides

		#region GetLineEndProperties

		protected override LineEndProperties GetLineEndProperties(ShapeOutline outline)
		{
			return outline.TailEndProperties;
		}

		#endregion // GetLineEndProperties

		#region ElementName

		public override string ElementName
		{
			get { return TailEndElement.QualifiedName; }
		}

		#endregion ElementName

		#region SetLineEndProperties

		protected override void SetLineEndProperties(ShapeOutline outline, LineEndProperties properties)
		{
			outline.TailEndProperties = properties;
		}

		#endregion // SetLineEndProperties

		#endregion // Base Class Overrides
	}

	internal class LineEndProperties
	{
		public ST_LineEndLength len = ST_LineEndLength.med;
		public ST_LineEndType type = ST_LineEndType.none;
		public ST_LineEndWidth w = ST_LineEndWidth.med;
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