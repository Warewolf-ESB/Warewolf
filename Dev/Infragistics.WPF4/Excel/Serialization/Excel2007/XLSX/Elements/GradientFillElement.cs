using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class GradientFillElement : XLSXElementBase
    {
		#region XML Schema Fragment

		//<complexType name="CT_GradientFill">
		//        <sequence>
		//            <element name="stop" type="CT_GradientStop" minOccurs="0" maxOccurs="unbounded"/>
		//        </sequence>
		//    <attribute name="type" type="ST_GradientType" use="optional" default="linear"/>
		//    <attribute name="degree" type="xsd:double" use="optional" default="0"/>
		//    <attribute name="left" type="xsd:double" use="optional" default="0"/>
		//    <attribute name="right" type="xsd:double" use="optional" default="0"/>
		//    <attribute name="top" type="xsd:double" use="optional" default="0"/>
		//    <attribute name="bottom" type="xsd:double" use="optional" default="0"/>
		//</complexType>

		#endregion // XML Schema Fragment

        #region Constants

        public const string LocalName = "gradientFill";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            GradientFillElement.LocalName;

        public const string BottomAttributeName = "bottom";
        public const string TopAttributeName = "top";
        public const string RightAttributeName = "right";
        public const string LeftAttributeName = "left";
        public const string DegreeAttributeName = "degree";
        public const string TypeAttributeName = "type";

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.gradientFill; }
        }
        #endregion Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            FillInfo fillInfo = (FillInfo)manager.ContextStack[typeof(FillInfo)];
            if (fillInfo == null)
            {
                Utilities.DebugFail("FillInfo object not located on the stack.");
                return;
            }
            GradientFillInfo gradientFillInfo = new GradientFillInfo();
            fillInfo.GradientFill = gradientFillInfo;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XLSXElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case GradientFillElement.BottomAttributeName:
                        gradientFillInfo.Bottom = (double)XLSXElementBase.GetAttributeValue(attribute, DataType.Double, -1);
                        break;
                    case GradientFillElement.TopAttributeName:
                        gradientFillInfo.Top = (double)XLSXElementBase.GetAttributeValue(attribute, DataType.Double, -1);
                        break;
                    case GradientFillElement.RightAttributeName:
                        gradientFillInfo.Right = (double)XLSXElementBase.GetAttributeValue(attribute, DataType.Double, -1);
                        break;
                    case GradientFillElement.LeftAttributeName:
                        gradientFillInfo.Left = (double)XLSXElementBase.GetAttributeValue(attribute, DataType.Double, -1);
                        break;
                    case GradientFillElement.DegreeAttributeName:
                        gradientFillInfo.Degree = (double)XLSXElementBase.GetAttributeValue(attribute, DataType.Double, -1);
                        break;
                    case GradientFillElement.TypeAttributeName:
                        gradientFillInfo.FillType = (ST_GradientType)XLSXElementBase.GetAttributeValue(attribute, DataType.ST_GradientType, ST_GradientType.linear);
                        break;
                    default:
                        Utilities.DebugFail("Unknown attribute type in the gradientFill element: " + attributeName);
                        break;
                }
            }

            ListContext<StopInfo> listContext = new ListContext<StopInfo>(gradientFillInfo.Stops);
            manager.ContextStack.Push(listContext);
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            GradientFillInfo gradientFillInfo = (GradientFillInfo)manager.ContextStack[typeof(GradientFillInfo)];
            if (gradientFillInfo == null)
            {
                Utilities.DebugFail("GradientFillInfo object not located on the stack.");
                return;
            }

            if (gradientFillInfo.Bottom != GradientFillInfo.DefaultValueBottom)
                XLSXElementBase.AddAttribute(element, GradientFillElement.BottomAttributeName, XLSXElementBase.GetXmlString(gradientFillInfo.Bottom, DataType.Double));

            if (gradientFillInfo.Top != GradientFillInfo.DefaultValueTop)
                XLSXElementBase.AddAttribute(element, GradientFillElement.TopAttributeName, XLSXElementBase.GetXmlString(gradientFillInfo.Top, DataType.Double));

            if (gradientFillInfo.Left != GradientFillInfo.DefaultValueLeft)
                XLSXElementBase.AddAttribute(element, GradientFillElement.LeftAttributeName, XLSXElementBase.GetXmlString(gradientFillInfo.Left, DataType.Double));

            if (gradientFillInfo.Right != GradientFillInfo.DefaultValueRight)
                XLSXElementBase.AddAttribute(element, GradientFillElement.RightAttributeName, XLSXElementBase.GetXmlString(gradientFillInfo.Right, DataType.Double));

            if (gradientFillInfo.Degree != GradientFillInfo.DefaultValueDegree)
                XLSXElementBase.AddAttribute(element, GradientFillElement.DegreeAttributeName, XLSXElementBase.GetXmlString(gradientFillInfo.Degree, DataType.Double));

            if (gradientFillInfo.FillType != GradientFillInfo.DefaultFillType)
                XLSXElementBase.AddAttribute(element, GradientFillElement.TypeAttributeName, XLSXElementBase.GetXmlString(gradientFillInfo.FillType, DataType.ST_GradientType));

            List<StopInfo> stops = gradientFillInfo.Stops;
            manager.ContextStack.Push(new ListContext<StopInfo>(stops));
            for (int i = 0; i < stops.Count; i++)
            {
                XLSXElementBase.AddElement(element, StopElement.QualifiedName);
            }
        }

        #endregion Save

        #endregion Base Class Overrides
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