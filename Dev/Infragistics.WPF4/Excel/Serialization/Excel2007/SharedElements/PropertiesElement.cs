using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;







using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Variants;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements

{
    internal class PropertiesElement : XmlElementBase
	{
		#region Constants

		public const string LocalName = "Properties";

		public const string QualifiedName =
			ExtendedPropertiesPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			PropertiesElement.LocalName; 

		#endregion Constants

		#region Base Class Overrides

		#region ElementName

		public override string ElementName
		{
			get { return PropertiesElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			manager.ContextStack.Push( manager.Workbook );
			manager.ContextStack.Push( manager.Workbook.DocumentProperties );
		}

		#endregion Load

		#region Save

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			manager.ContextStack.Push( manager.Workbook );
			manager.ContextStack.Push( manager.Workbook.DocumentProperties );

            XmlElementBase.AddNamespaceDeclaration( 
                element, 
                ExtendedPropertiesPart.VariantTypesNamespacePrefix, 
                VariantElement.VariantTypesNamespace );

            XmlElementBase.AddElement( element, ApplicationElement.QualifiedName );
            XmlElementBase.AddElement( element, DocSecurityElement.QualifiedName );
            XmlElementBase.AddElement( element, ScaleCropElement.QualifiedName );
            XmlElementBase.AddElement( element, HeadingPairsElement.QualifiedName );
            XmlElementBase.AddElement( element, TitlesOfPartsElement.QualifiedName );

            if (String.IsNullOrEmpty(manager.Workbook.DocumentProperties.Manager) == false)
                XmlElementBase.AddElement(element, ManagerElement.QualifiedName);

            if ( String.IsNullOrEmpty( manager.Workbook.DocumentProperties.Company ) == false )
                XmlElementBase.AddElement( element, CompanyElement.QualifiedName );

            XmlElementBase.AddElement( element, LinksUpToDateElement.QualifiedName );
            XmlElementBase.AddElement( element, SharedDocElement.QualifiedName );
            XmlElementBase.AddElement( element, HyperlinksChangedElement.QualifiedName );
            XmlElementBase.AddElement( element, AppVersionElement.QualifiedName );
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