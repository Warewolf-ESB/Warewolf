using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;




using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.ContentTypes

{
	// MD 10/1/08 - TFS8471
	// This now derived from WorkbookPartBase so another class can share some functionality. 
	// Some of this class's code has been moved to the new base type.
	//internal class WorkbookPart : XLSXContentTypeBase
	internal class WorkbookPart : WorkbookPartBase
	{
		// MD 10/1/08 - TFS8471
		// Moved to base class
        //private Dictionary<string, int> relationshipLoadPriorityDictionary;

		#region Constants

		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml";

		// MD 10/1/08 - TFS8471
		// Moved to base class
		//public const string DefaultPartName = "/xl/workbook.xml";
		//public const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";

		#endregion Constants

		#region Base Class Overrides

		#region ContentType

		public override string ContentType
		{
			get { return WorkbookPart.ContentTypeValue; }
		} 

		#endregion ContentType

		// MD 10/1/08 - TFS8471
		#region Moved to Base

		
#region Infragistics Source Cleanup (Region)








































































#endregion // Infragistics Source Cleanup (Region)


		#endregion Moved to Base

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