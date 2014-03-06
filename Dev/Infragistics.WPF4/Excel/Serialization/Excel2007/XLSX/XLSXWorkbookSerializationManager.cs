using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.BIFF8;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.ContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX
{
    internal class XLSXWorkbookSerializationManager : Excel2007WorkbookSerializationManager
	{
		#region Constructor

        public XLSXWorkbookSerializationManager(IPackage zipPackage, Workbook workbook, string loadingPath)
			: this( zipPackage, workbook, null, true ) { }

        public XLSXWorkbookSerializationManager(IPackage zipPackage, Workbook workbook, string loadingPath, bool verifyExcel2007Xml)
			: base( zipPackage, workbook, loadingPath, verifyExcel2007Xml )
		{
		}

		public XLSXWorkbookSerializationManager(IPackage zipPackage, BIFF8WorkbookSerializationManager parentManager, WorksheetShape shapeBeingLoaded)
			: base(zipPackage, parentManager.Workbook, parentManager.FilePath, parentManager.VerifyExcel2007Xml)
		{
			this.ShapeBeingLoaded = shapeBeingLoaded;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region WriteWorkbookGlobalData

		protected override void WriteWorkbookGlobalData( bool hasShapes )
		{
			// MD 10/1/08 - TFS8471
			// If this is a macro-enabled workbook, we need to write out a different content type
			//this.CreatePartInPackage( XLSXContentType.Workbook, WorkbookPart.DefaultPartName, this.Workbook );
			string contentType;
			switch ( this.Workbook.CurrentFormat )
			{
				case WorkbookFormat.Excel2007:
					contentType = WorkbookPart.ContentTypeValue;
					break;

				case WorkbookFormat.Excel2007MacroEnabled:
					contentType = MacroEnabledWorkbookPart.ContentTypeValue;
					break;

				// MD 5/7/10 - 10.2 - Excel Templates
				case WorkbookFormat.Excel2007MacroEnabledTemplate:
					contentType = MacroEnabledTemplatePart.ContentTypeValue;
					break;

				// MD 5/7/10 - 10.2 - Excel Templates
				case WorkbookFormat.Excel2007Template:
					contentType = TemplatePart.ContentTypeValue;
					break;

				default:
					Utilities.DebugFail( "This format is not a valid XLSX format." );
					return;
			}

			this.CreatePartInPackage( contentType, WorkbookPart.DefaultPartName, this.Workbook );
		}

		#endregion WriteWorkbookGlobalData

		protected override void WriteWorksheet( Worksheet worksheet, bool hasShapes )
		{
			// MD 10/1/08
			// Found while fixing TFS8453
			// XLSXContentType was no longer needed
			//this.CreatePartInPackage( XLSXContentType.Worksheet, this.GetNumberedPartName( WorksheetPart.BasePartName ), worksheet);
			this.CreatePartInPackage( WorksheetPart.ContentTypeValue, this.GetNumberedPartName( WorksheetPart.BasePartName ), worksheet );
		}

		#endregion Base Class Overrides

		// MD 10/1/08
		// Found while fixing TFS8453
		// XLSXContentType was no longer needed
		//#region CreatePartInPackage
		//
		//internal Uri CreatePartInPackage( XLSXContentType contentType, string partPath, object context )
		//{
		//    string contentTypeValue = XLSXContentTypeBase.GetContentTypeValue( contentType );
		//
		//    if ( contentTypeValue == null )
		//        return null;
		//
		//    return this.CreatePartInPackage( contentTypeValue, partPath, context );
		//} 
		//
		//#endregion CreatePartInPackage
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