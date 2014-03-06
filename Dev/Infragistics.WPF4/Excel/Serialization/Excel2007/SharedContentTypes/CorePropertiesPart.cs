using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;






using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes

{
    internal class CorePropertiesPart : XmlContentTypeBase
	{
		#region Constants

		public const string ContentTypeValue = "application/vnd.openxmlformats-package.core-properties+xml";
		public const string DefaultPartName = "/docProps/core.xml";
		private const string RelationshipTypeValue = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties";

		public const string CorePropertiesNamespace = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
		public const string DcNamespace = "http://purl.org/dc/elements/1.1/";
		public const string DctermsNamespace = "http://purl.org/dc/terms/";
		public const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

		public const string CorePropertiesNamespacePrefix = "cp";
		public const string DcNamespacePrefix = "dc";
		public const string DctermsNamespacePrefix = "dcterms";
		public const string XsiNamespacePrefix = "xsi";

		#endregion Constants

		#region Base Class Overrides

		#region ContentType

		public override string ContentType
		{
			get { return CorePropertiesPart.ContentTypeValue; }
		}

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { return CorePropertiesPart.RelationshipTypeValue; }
		}

		#endregion RelationshipType

		#region SaveElements

		public override void SaveElements( Excel2007WorkbookSerializationManager manager, ExcelXmlDocument document )
		{
            //  BF 10/8/10  NA 2011.1 - Infragistics.Word
            //  Moved
            this.SaveElementsHelper( document );            
		}

        //  BF 10/8/10  NA 2011.1 - Infragistics.Word
        private void SaveElementsHelper( ExcelXmlDocument document )
        {
			ExcelXmlElement corePropertiesElement = document.CreateElement(
				CorePropertiesPart.CorePropertiesNamespacePrefix,
				CorePropertiesElement.LocalName,
				CorePropertiesPart.CorePropertiesNamespace );

			document.AppendChild( corePropertiesElement );
        }

		#endregion SaveElements 

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