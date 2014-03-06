using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes
{
    internal class DrawingsPart : XmlContentTypeBase
	{
		#region Constants

		public const string BasePartName = "/xl/drawings/drawing.xml";
		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.drawing+xml";
		private const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing";
        public const string DefaultNamespace = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing";
        public const string DefaultNamespacePrefix = "xdr";

        /// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing</summary>
        public const string SpreadsheetDrawingNamespace = DefaultNamespace;

        /// <summary>xdr</summary>
        public const string SpreadsheetDrawingNamespacePrefix = DefaultNamespacePrefix;

        /// <summary>http://schemas.openxmlformats.org/drawingml/2006/main</summary>
        public const string MainNamespace = ThemePart.DefaultNamespace;
        
        /// <summary>a</summary>
        public const string MainNamespacePrefix = ThemePart.DefaultNamespacePrefix;

		#endregion Constants

		#region Base Class Overrides

		#region ContentType

		public override string ContentType
		{
			get { return DrawingsPart.ContentTypeValue; }
		}

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { return DrawingsPart.RelationshipTypeValue; }
		}

		#endregion RelationshipType

		#region Save

		public override void Save( Excel2007WorkbookSerializationManager manager, Stream contentTypeStream )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
				Utilities.DebugFail( "Cannot find the worksheet on the context stack." );
				base.Save( manager, contentTypeStream );
				return;
			}

            Dictionary<Image, RelationshipIdHolder> imageRelationshipIds = new Dictionary<Image, RelationshipIdHolder>();

            //  BF 8/28/08
            //  This logic has to be recursive so we can grab the images in nested shapes
            #region Refactored
            //foreach ( WorksheetShape shape in worksheet.Shapes )
            //{
            //    WorksheetImage imageShape = shape as WorksheetImage;

            //    if ( imageShape == null )
            //        continue;

            //    RelationshipIdHolder relationshipId;
            //    Image image = imageShape.Image;

            //    Uri imagePartPath;
            //    if ( manager.ImagesSavedInPackage.TryGetValue( image, out imagePartPath ) )
            //    {
            //        // If the image has already been saved in the package, make sure this drawings part has a relationship to it.
            //        if ( imageRelationshipIds.TryGetValue( image, out relationshipId ) == false )
            //            relationshipId = new RelationshipIdHolder(manager.CreateRelationshipInPackage( imagePartPath, ImageBasePart.RelationshipTypeValue ));
            //    }
            //    else
            //    {
            //        // If the image hasn't been added to the package, add it
            //        string rId;
            //        imagePartPath = ImageBasePart.AddImageToPackage(manager, image, null, out rId);
            //        manager.ImagesSavedInPackage.Add( image, imagePartPath );
            //        relationshipId = new RelationshipIdHolder(rId);
            //    }

            //    imageRelationshipIds[ image ] = relationshipId;
            //}
            #endregion Refactored
            DrawingsPart.PopulateImageRelationshipIdHolderDictionary( manager, worksheet.Shapes, imageRelationshipIds );

			manager.ContextStack.Push( imageRelationshipIds );
			base.Save( manager, contentTypeStream );
			manager.ContextStack.Pop(); // imageRelationshipIds
		} 

		#endregion Save

        //  BF 8/28/08
        #region PopulateImageRelationshipIdHolderDictionary

        private static void PopulateImageRelationshipIdHolderDictionary(
            Excel2007WorkbookSerializationManager manager,
            WorksheetShapeCollection shapes,
            Dictionary<Image, RelationshipIdHolder> imageRelationshipIds )

        {
			foreach ( WorksheetShape shape in shapes )
			{
                //  If the shape is a group, call this method recursively
                //  to process any WorksheetImages that exist in the group's
                //  Shapes collection.
                WorksheetShapeGroup group = shape as WorksheetShapeGroup;
                if ( group != null )
				{
                    DrawingsPart.PopulateImageRelationshipIdHolderDictionary( manager, group.Shapes, imageRelationshipIds );

					// MD 4/28/11
					// Found while fixing TFS62775
					// This is just a slight performance improvement but we are only checking for images below and a group 
					// can never be an image, so just skip it.
					continue;
				}

				// MD 10/30/11 - TFS90733
				// Other shapes (such as controls) can also store an image.
				//WorksheetImage imageShape = shape as WorksheetImage;
				IWorksheetImage imageShape = shape as IWorksheetImage;

				if ( imageShape == null )
					continue;

				RelationshipIdHolder relationshipId;
				Image image = imageShape.Image;

				// MD 10/30/11 - TFS90733
				// Not all IWorksheetImage instances need to have images.
				if (image == null)
				{
					Debug.Assert((imageShape is WorksheetImage) == false, "The WorksheetImage instances must have images.");
					continue;
				}

				Uri imagePartPath;
				if ( manager.ImagesSavedInPackage.TryGetValue( image, out imagePartPath ) )
				{
					// If the image has already been saved in the package, make sure this drawings part has a relationship to it.
					if ( imageRelationshipIds.TryGetValue( image, out relationshipId ) == false )
						relationshipId = new RelationshipIdHolder(manager.CreateRelationshipInPackage( imagePartPath, ImageBasePart.RelationshipTypeValue ));
				}
				else
				{
					// If the image hasn't been added to the package, add it
                    string rId;
                    imagePartPath = ImageBasePart.AddImageToPackage(manager, image, null, out rId);
					manager.ImagesSavedInPackage.Add( image, imagePartPath );
                    relationshipId = new RelationshipIdHolder(rId);
				}

				imageRelationshipIds[ image ] = relationshipId;
			}
        }

        #endregion PopulateImageRelationshipIdHolderDictionary

        #region SaveElements

		public override void SaveElements( Excel2007WorkbookSerializationManager manager, ExcelXmlDocument document )
		{
			ExcelXmlElement drawingElement = document.CreateElement(
                DrawingsPart.SpreadsheetDrawingNamespacePrefix,
                WsDrElement.LocalName,
                DrawingsPart.SpreadsheetDrawingNamespace );

            document.AppendChild(drawingElement);
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