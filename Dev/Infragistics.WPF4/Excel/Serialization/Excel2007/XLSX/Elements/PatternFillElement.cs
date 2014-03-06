using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class PatternFillElement : XLSXElementBase
    {
		#region XML Schema Fragment

		//<complexType name="CT_PatternFill">
		//    <sequence>
		//        <element name="fgColor" type="CT_Color" minOccurs="0" maxOccurs="1"/>
		//        <element name="bgColor" type="CT_Color" minOccurs="0" maxOccurs="1"/>
		//    </sequence>
		//    <attribute name="patternType" type="ST_PatternType" use="optional"/>
		//</complexType>

		#endregion // XML Schema Fragment

        #region Constants

        public const string LocalName = "patternFill";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            PatternFillElement.LocalName;

        public const string PatternTypeAttributeName = "patternType";

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.patternFill; }
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
            PatternFillInfo patternFillInfo = new PatternFillInfo();
            fillInfo.PatternFill = patternFillInfo;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XLSXElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case PatternFillElement.PatternTypeAttributeName:
						// MD 1/19/12 - 12.1 - Cell Format Updates
						// None is the default value, and Default is never used anymore.
                        //patternFillInfo.PatternStyle = (FillPatternStyle)XLSXElementBase.GetAttributeValue(attribute, DataType.ST_PatternType, FillPatternStyle.Default);
						patternFillInfo.PatternStyle = (FillPatternStyle)XLSXElementBase.GetAttributeValue(attribute, DataType.ST_PatternType, FillPatternStyle.None);
                        break;
                    default:
                        Utilities.DebugFail("Unknown attribute type in the patternFill element: " + attributeName);
                        break;
                }
            }
            manager.ContextStack.Push(patternFillInfo);
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            PatternFillInfo patternFillInfo = (PatternFillInfo)manager.ContextStack[typeof(PatternFillInfo)];
            if (patternFillInfo == null)
            {
                Utilities.DebugFail("PatternFillInfo object not located on the stack.");
                return;
            }

			// MD 1/19/12 - 12.1 - Cell Format Updates
			// Default is never used anymore.
            //if (patternFillInfo.PatternStyle != FillPatternStyle.Default)
			XLSXElementBase.AddAttribute(element, PatternFillElement.PatternTypeAttributeName, XLSXElementBase.GetXmlString(patternFillInfo.PatternStyle, DataType.ST_PatternType));

            //  BF 8/14/08
            //  I moved this up from down below because the order in which
            //  the child elements are added matters; in this case, fgColor
            //  has to appear before bgColor.
            if (patternFillInfo.ForegroundColor != null &&
                !patternFillInfo.ForegroundColor.IsDefault)
            {
                // 09/17/08 - CDS - FgColorElement will look for the PatternFillInfo object instead of the ColorInfo now.
                //manager.ContextStack.Push(patternFillInfo.ForegroundColor);
                XLSXElementBase.AddElement(element, FgColorElement.QualifiedName);
            }

            if (patternFillInfo.BackgroundColor != null &&
                !patternFillInfo.BackgroundColor.IsDefault)
            {
                // 09/17/08 - CDS - BgColorElement will look for the PatternFillInfo object instead of the ColorInfo now.
                //manager.ContextStack.Push(patternFillInfo.BackgroundColor);
                XLSXElementBase.AddElement(element, BgColorElement.QualifiedName);
            }

            //  BF 8/14/08  (see above)
            #region Obsolete code
            //if (patternFillInfo.ForegroundColor != null &&
            //    !patternFillInfo.ForegroundColor.IsDefault)
            //{
            //    manager.ContextStack.Push(patternFillInfo.ForegroundColor);
            //    XLSXElementBase.AddElement(element, FgColorElement.QualifiedName);
            //}
            #endregion Obsolete code
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