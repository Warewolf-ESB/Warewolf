using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.ContentTypes
{
    // MD 10/1/08 - TFS8471
	// Moved some code from WorkbookPart
	internal abstract class WorkbookPartBase : XLSXContentTypeBase
	{
		#region Constants

		public const string DefaultPartName = "/xl/workbook.xml";
		public const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"; 

		#endregion Constants

		#region Member Variables

		private Dictionary<string, int> relationshipLoadPriorityDictionary; 

		#endregion Member Variables

		#region Base Class Overrides

		// MD 4/6/12 - TFS102169
		#region OnLoadComplete

		public override void OnLoadComplete(Excel2007WorkbookSerializationManager manager)
		{
			base.OnLoadComplete(manager);
			manager.Workbook.OnAfterLoadGlobalSettings(manager);
			manager.OnWorkbookLoaded();
		}

		#endregion // OnLoadComplete

		// MD 4/6/12 - TFS102169
		#region PostLoadRelationshipTypes

		public override string[] PostLoadRelationshipTypes
		{
			get
			{
				return new string[] { WorksheetPart.RelationshipTypeValue };
			}
		}

		#endregion // PostLoadRelationshipTypes

		#region RelationshipLoadPriorityDictionary



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		public override Dictionary<string, int> RelationshipLoadPriorityDictionary
		{
			get
			{
				if ( this.relationshipLoadPriorityDictionary == null )
				{
					this.relationshipLoadPriorityDictionary = new Dictionary<string, int>();
					this.relationshipLoadPriorityDictionary.Add( ThemePart.RelationshipTypeValue, 0 );
					this.relationshipLoadPriorityDictionary.Add( WorkbookStylesPart.RelationshipTypeValue, 1 );
					this.relationshipLoadPriorityDictionary.Add( SharedStringTablePart.RelationshipTypeValue, 2 );
				}
				return this.relationshipLoadPriorityDictionary;
			}
		}
		#endregion //RelationshipLoadPriorityDictionary

		#region RelationshipType

		public override string RelationshipType
		{
			get { return WorkbookPartBase.RelationshipTypeValue; }
		}

		#endregion RelationshipType

		#region RootElementType

		public override XLSXElementType RootElementType
		{
			get { return XLSXElementType.workbook; }
		}

		#endregion RootElementType

		#region Save

		public override void Save( Excel2007WorkbookSerializationManager manager, Stream contentTypeStream )
		{
			foreach ( Worksheet worksheet in manager.Workbook.Worksheets )
			{
				// MD 4/28/11 - TFS62775
				// We area actually using the hasShapes parameter now, so we can't just pass in False.
				// Instead, figure out if we have shapes.
				//manager.WriteWorksheetRecords( worksheet, false );
				bool hasShapes = false;
				for (int i = 0; i < worksheet.Shapes.Count; i++)
				{
					WorksheetShape shape = worksheet.Shapes[i];
					manager.PrepareShapeForSerialization(ref shape);
					if (shape != null)
					{
						hasShapes = true;
						break;
					}
				}

				manager.WriteWorksheetRecords(worksheet, hasShapes);
			}

			manager.CreatePartInPackage( WorkbookStylesPart.ContentTypeValue, WorkbookStylesPart.DefaultPartName, null );
			manager.CreatePartInPackage( ThemePart.ContentTypeValue, manager.GetNumberedPartName( ThemePart.BasePartName ), null );

			// MD 2/1/12 - TFS100573
			// The SharedStringTable collection now only stores additional strings not in the workbook's string table during 
			// a save operation, so use the SharedStringCountDuringSave instead.
			//if ( manager.SharedStringTable.Count != 0 )
			if (manager.SharedStringCountDuringSave != 0)
				manager.CreatePartInPackage( SharedStringTablePart.ContentTypeValue, SharedStringTablePart.DefaultPartName, null );

			foreach ( WorkbookReferenceBase workbookRef in manager.ExternalReferences.Keys )
			{
				ExternalWorkbookReference externalRef = workbookRef as ExternalWorkbookReference;
				if ( externalRef != null )
				{
					manager.ContextStack.Push( externalRef );
					manager.CreatePartInPackage( ExternalWorkbookPart.ContentTypeValue, manager.GetNumberedPartName( ExternalWorkbookPart.BasePartName ), externalRef );
					manager.ContextStack.Pop();
				}
				else
					Utilities.DebugFail( "Encountered a non-external workbook reference in the ExternalReferences collection" );
			}

			if ( manager.Workbook.VBAData2007 != null )
				manager.CreatePartInPackage( VBAProjectPart.ContentTypeValue, VBAProjectPart.BasePartName, null );

			base.Save( manager, contentTypeStream );
		}
		#endregion //Save

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