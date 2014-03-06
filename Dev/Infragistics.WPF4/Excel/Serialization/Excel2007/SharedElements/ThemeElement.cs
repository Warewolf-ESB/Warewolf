using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements
{
    internal class ThemeElement : XmlElementBase
    {
        #region Constants

        public const string LocalName = "theme";

        public const string QualifiedName =
            ThemePart.DefaultNamespace +
            XmlElementBase.NamespaceSeparator +
            ThemeElement.LocalName;

        public const string NameAttributeName = "name";

        private const string Namespace = "http://schemas.openxmlformats.org/drawingml/2006/main";
        private const string NamespacePrefix = "a";

        #endregion Constants

        #region Base Class Overrides

        #region ElementName

        public override string ElementName
        {
            get { return ThemeElement.QualifiedName; }
        }

        #endregion ElementName

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 1/16/12 - 12.1 - Cell Format Updates
			// This code is no longer needed.
			#region Removed

			//// CDS - 8/19/08 Theme colors are all stored in a single list for indexing
			//// Therefore ColorSchemeInfo is no longer used.

			//#region Commented out

			////ColorSchemeInfo schemeInfo = new ColorSchemeInfo();

			////foreach (ExcelXmlAttribute attribute in element.Attributes)
			////{
			////    string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

			////    switch (attributeName)
			////    {

			////        case ThemeElement.NameAttributeName:
			////            {
			////                string val = (string)XmlElementBase.GetAttributeValue(attribute, DataType.String, String.Empty);
			////                schemeInfo.ThemeName = val;
			////            }
			////            break;
			////    }
			////}

			////manager.ContextStack.Push(schemeInfo);

			//#endregion Commented out

			//ListContext<Color> listContext = new ListContext<Color>(manager.ThemeColors);
			//manager.ContextStack.Push(listContext);

			#endregion // Removed
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            Utilities.DebugFail("Save() method for theme element not yet implemented.");

            XmlElementBase.AddNamespaceDeclaration(
                element,
                ThemeElement.NamespacePrefix,
                ThemeElement.Namespace);

            //ROUNDTRIP. Lots of data in the theme.xml which needs to be written out again.
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