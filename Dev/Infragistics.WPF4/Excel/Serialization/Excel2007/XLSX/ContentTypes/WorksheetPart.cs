using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.ContentTypes
{
    internal class WorksheetPart : XLSXContentTypeBase
	{
		#region Constants

		public const string BasePartName = "/xl/worksheets/sheet.xml";
		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml";
		public const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet";

		#endregion Constants

		#region Base Class Overrides

		#region ContentType

		public override string ContentType
		{
			get { return WorksheetPart.ContentTypeValue; }
		} 

		#endregion ContentType

		// MD 3/30/10 - TFS30253
		#region Load

		public override object Load(Excel2007WorkbookSerializationManager manager, Stream contentTypeStream)
		{
			// MD 4/6/12 - TFS102169
			// The worksheet part data should have already been created when loading the workbook part, 
			// so get it here and push it onto the context stack.
			Worksheet worksheet = manager.GetActivePartData() as Worksheet;
			manager.ContextStack.Push(worksheet);

			object retValue = base.Load(manager, contentTypeStream);

			// Shared formulas can only be shared in the worksheet, so after loading the current worksheet, clear the shared formulas.
			manager.SharedFormulas.Clear();

			// MD 4/6/12 - TFS102169
			Debug.Assert(worksheet == retValue, "Something is wrong here.");

			return retValue;
		}

		#endregion // Load

		#region RelationshipType

		public override string RelationshipType
		{
			get { return WorksheetPart.RelationshipTypeValue; }
		}

		#endregion RelationshipType

		#region RootElementType

		public override XLSXElementType RootElementType
		{
			get { return XLSXElementType.worksheet; }
		} 

		#endregion RootElementType

		#region Save

		public override void Save( Excel2007WorkbookSerializationManager manager, Stream contentTypeStream )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
				Utilities.DebugFail( "Cannot find the worksheet on the context stack." );
				return;
			}


			if ( worksheet.ImageBackground != null )
				ImageBasePart.AddImageToPackage( manager, worksheet.ImageBackground, worksheet.PreferredImageBackgroundFormat, out worksheet.imageBackgroundId );


			// MD 4/28/11 - TFS62775
			// Some shapes could be invalid in the collection. Instead, check CurrentWorksheetHasShapes.
			//if ( worksheet.Shapes.Count != 0 )
			if (manager.CurrentWorksheetHasShapes)
				manager.CreatePartInPackage( DrawingsPart.ContentTypeValue, manager.GetNumberedPartName( DrawingsPart.BasePartName ), null, out worksheet.drawingRelationshipId );

			if ( worksheet.CommentShapes.Count != 0 )
			{
				manager.CreatePartInPackage( CommentsPart.ContentTypeValue, manager.GetNumberedPartName( CommentsPart.BasePartName ), null );
				manager.CreatePartInPackage( LegacyDrawingsPart.ContentTypeValue, manager.GetNumberedPartName( LegacyDrawingsPart.BasePartName ), null, out worksheet.legacyDrawingRelationshipId );
			}

			// MD 12/30/11 - 12.1 - Table Support
			for (int i = 0; i < worksheet.Tables.Count; i++)
				manager.CreatePartInPackage(TablePart.ContentTypeValue, manager.GetNumberedPartName(TablePart.BasePartName), worksheet.Tables[i]);

			base.Save( manager, contentTypeStream );
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