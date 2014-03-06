using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;






using System.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;
namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing

{



	internal class CNvCxnSpPrElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_NonVisualConnectorProperties">
		// <sequence>
		// <element name="cxnSpLocks" type="CT_ConnectorLocking" minOccurs="0" maxOccurs="1"/>
		// <element name="stCxn" type="CT_Connection" minOccurs="0" maxOccurs="1"/>
		// <element name="endCxn" type="CT_Connection" minOccurs="0" maxOccurs="1"/>
		// <element name="extLst" type="CT_OfficeArtExtensionList" minOccurs="0" maxOccurs="1"/>
		// </sequence>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>cNvCxnSpPr</summary>
		public const string LocalName = "cNvCxnSpPr";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/cNvCxnSpPr</summary>
		public const string QualifiedName =
			DrawingsPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			CNvCxnSpPrElement.LocalName;


		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return CNvCxnSpPrElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
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