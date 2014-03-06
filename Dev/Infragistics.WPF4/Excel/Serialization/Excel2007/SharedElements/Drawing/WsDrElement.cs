using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



    internal class WsDrElement : XmlElementBase
    {
        #region Constants

        public const string LocalName = "wsDr";

        /// <summary>
        /// http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/wsDr
        /// </summary>
        public const string QualifiedName =
            DrawingsPart.DefaultNamespace +
            XmlElementBase.NamespaceSeparator +
            WsDrElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region ElementName

        public override string ElementName
        {
            get { return WsDrElement.QualifiedName; }
        }

        #endregion ElementName

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            IPackagePart activePart = manager.ActivePart;
            if ( activePart == null )
            {
                Utilities.DebugFail( "No active part in WsDrElement.Load - unexpected, no shapes can be serialized in." );
                return;
            }

            string key = activePart.Uri.ToString();
            if ( key != null )
            {
                //  Create a WorksheetShapesHolder instance and add it to
                //  the manager's dictionary, with the active part's name
                //  as the key.
                WorksheetShapesHolder shapes = new WorksheetShapesHolder();
                manager.SerializedShapes.Add( key, shapes );

                //  Push it onto the stack as well for convenience.
				manager.ContextStack.Push( shapes );
            }
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            XmlElementBase.AddNamespaceDeclaration(
                element,
                DrawingsPart.SpreadsheetDrawingNamespacePrefix,
                DrawingsPart.SpreadsheetDrawingNamespace );

            XmlElementBase.AddNamespaceDeclaration(
                element,
                DrawingsPart.MainNamespacePrefix,
                DrawingsPart.MainNamespace );

            //  Get the Worksheet off the ContextStack
            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;

            //  Push a WorksheetShapesSaveHelper onto the ContextStack
            //WorksheetShapesSaveHelper saveHelper = new WorksheetShapesSaveHelper();
            //manager.ContextStack.Push( saveHelper );

            //  Push a consumable list which represents the root Shapes collection
            //  onto the ContextStack so we can get references to WorksheetShapes.
            WorksheetShapeCollection shapes = worksheet.Shapes;
            List<WorksheetShape> shapeList = new List<WorksheetShape>(shapes.Count);
            manager.ContextStack.Push( shapeList );

			// MD 4/28/11 - TFS62775
			// Use PrepareShapeForSerialization to skip invalid shapes.
			#region Old Code

			////  Populate the shape list
			//foreach ( WorksheetShape shape in shapes )
			//{
			//    //  Add the current shape to the consumable list
			//    shapeList.Add( shape );
			//}
			//
			//foreach ( WorksheetShape shape in shapes )
			//{
			//    //  Initialize the WorksheetShapeSerializationManager object for
			//    //  the shape, so that the conversions and such are already done,
			//    //  and the element can easily write the info out to XML.
			//    WorksheetShapeSerializationManager.OnBeforeShapeSaved( shape, true );
			//
			//    //  Add a twoCellAnchor element
			//    XmlElementBase.AddElement(element, TwoCellAnchorElement.QualifiedName);            
			//} 

			#endregion  // Old Code
			//  Populate the shape list
			for (int i = 0; i < shapes.Count; i++)
			{
				WorksheetShape shape = shapes[i];
				manager.PrepareShapeForSerialization(ref shape);

				if (shape != null)
					shapeList.Add(shape);
			}

			for (int i = 0; i < shapeList.Count; i++)
			{
				WorksheetShape shape = shapeList[i];

				//  Initialize the WorksheetShapeSerializationManager object for
				//  the shape, so that the conversions and such are already done,
				//  and the element can easily write the info out to XML.
				WorksheetShapeSerializationManager.OnBeforeShapeSaved(shape, true);

				//  Add a twoCellAnchor element
				XmlElementBase.AddElement(element, TwoCellAnchorElement.QualifiedName);
			}
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