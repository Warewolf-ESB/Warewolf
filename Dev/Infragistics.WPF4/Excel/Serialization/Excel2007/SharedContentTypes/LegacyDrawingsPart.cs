using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.LegacyDrawing;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes
{
    internal class LegacyDrawingsPart : XmlContentTypeBase
	{
		#region Constants

		public const string BasePartName = "/xl/drawings/vmlDrawing.vml";
		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.vmlDrawing";
		private const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/vmlDrawing";

		public const string VmlNamespace = "urn:schemas-microsoft-com:vml";
		public const string VmlNamespacePrefix = "v";

		public const string OfficeNamespace = "urn:schemas-microsoft-com:office:office";
		public const string OfficeNamespacePrefix = "o";

		public const string ExcelNamespace = "urn:schemas-microsoft-com:office:excel";
		public const string ExcelNamespacePrefix = "x";

		#endregion Constants

		#region Base Class Overrides

		#region ContentType

		public override string ContentType
		{
			get { return LegacyDrawingsPart.ContentTypeValue; }
		}

		#endregion ContentType

		#region IncludeXmlDeclaration






		protected override bool IncludeXmlDeclaration
		{
			get { return false; }
		} 

		#endregion IncludeXmlDeclaration

		#region RelationshipType

		public override string RelationshipType
		{
			get { return LegacyDrawingsPart.RelationshipTypeValue; }
		}

		#endregion RelationshipType

		#region SaveElements

		public override void SaveElements( Excel2007WorkbookSerializationManager manager, ExcelXmlDocument document )
		{
			ExcelXmlElement xmlElement = document.CreateElement(
				null,
				XmlLegacyElement.LocalName,
				null );

			document.AppendChild( xmlElement );
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