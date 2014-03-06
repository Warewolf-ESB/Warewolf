using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;







using System.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes

{
	// MD 10/8/10
	// Found while fixing TFS44359
	// Added support to round-trip custom Xml parts.
	internal class CustomXmlPropertiesPart : ContentTypeBase
	{
		#region Constants

		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.customXmlProperties+xml";
		public const string BasePartName = "/customXml/itemProps.xml";
		public const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/customXmlProps";
		public const string DsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/customXml";

		public const string DsNamespacePrefix = "ds";

		#endregion Constants

		#region Base Class Overrides

		#region ContentType

		public override string ContentType
		{
			get { return CustomXmlPropertiesPart.ContentTypeValue; }
		}

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { return CustomXmlPropertiesPart.RelationshipTypeValue; }
		}

		#endregion RelationshipType

		public override object Load(Excel2007WorkbookSerializationManager manager, Stream contentTypeStream)
		{
			byte[] data = new byte[contentTypeStream.Length];
			contentTypeStream.Read(data, 0, data.Length);
			manager.Workbook.AddCustomXmlPropertiesPart(data);
			return data;
		}

		public override void Save(Excel2007WorkbookSerializationManager manager, Stream contentTypeStream)
		{
			ListContext<byte[]> customXmlPropertiesPartsContext = (ListContext<byte[]>)manager.ContextStack[typeof(ListContext<byte[]>)];

			if (customXmlPropertiesPartsContext == null)
			{
				Utilities.DebugFail("Could not get the custom Xml properties parts context from the context stack");
				return;
			}

			byte[] data = (byte[])customXmlPropertiesPartsContext.ConsumeCurrentItem();
			contentTypeStream.Write(data, 0, data.Length);
		}

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