using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



    internal class GrpSpElement : XmlElementBase
    {
        #region Constants

        public const string LocalName = "grpSp";

        /// <summary>
        /// http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/col
        /// </summary>
        public const string QualifiedName =
            DrawingsPart.DefaultNamespace +
            XmlElementBase.NamespaceSeparator +
            GrpSpElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region ElementName

        public override string ElementName
        {
            get { return GrpSpElement.QualifiedName; }
        }

        #endregion ElementName

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {            
            //  Create a WorksheetShapeGroup instance, and push it onto the context stack.
            WorksheetShapeGroup group = new WorksheetShapeGroup( true );
            WorksheetShapeSerializationManager.OnShapeCreated( group, manager.ContextStack );
			WorksheetShapeSerializationManager.LoadChildElements( manager, element, ref isReaderOnNextNode );
        }

        #endregion Load

		#region LoadChildNodes

		protected override bool LoadChildNodes
		{
			get { return false; }
		}

		#endregion LoadChildNodes

		// MD 7/15/11 - Shape support
		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			WorksheetShapeSerializationManager.OnAfterShapeLoaded(manager.ContextStack);
		}

		#endregion // OnAfterLoadChildElements

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            //  Get the group and its serialization manager off the context stack.
            WorksheetShapeGroup group = WorksheetShapeSerializationManager.ConsumeShape( manager.ContextStack, typeof(WorksheetShapeGroup) ) as WorksheetShapeGroup;
            WorksheetShapeSerializationManager serializationManager = group.Excel2007ShapeSerializationManager;

            //  Add the child elements
            WorksheetShapeSerializationManager.SaveWorksheetShape( manager, group, element );

            //  Push a consumable shape list onto the stack
            List<WorksheetShape> shapeList = new List<WorksheetShape>(group.Shapes.Count);
            manager.ContextStack.Push( shapeList );

			// MD 4/28/11 - TFS62775
			// Use PrepareShapeForSerialization to skip invalid shapes.
			#region Old Code

			////  First populate the shape list
			//foreach ( WorksheetShape shape in group.Shapes )
			//{
			//    shapeList.Add( shape );
			//}
			//
			//foreach ( WorksheetShape shape in group.Shapes )
			//{
			//    //  Initialize the WorksheetShapeSerializationManager object for
			//    //  the shape, so that the conversions and such are already done,
			//    //  and the element can easily write the info out to XML.
			//    WorksheetShapeSerializationManager.OnBeforeShapeSaved( shape, false );
			//
			//    // MD 10/12/10
			//    // Found while fixing TFS49853
			//    // Moved this code to a common location because it was defined twice.
			//    ////  Add the element appropriate for the type of the current shape
			//    //if ( shape is WorksheetShapeGroup )
			//    //    XmlElementBase.AddElement(element, GrpSpElement.QualifiedName);            
			//    //else
			//    //if ( shape is WorksheetImage )
			//    //    XmlElementBase.AddElement(element, PicElement.QualifiedName);
			//    //else
			//    //    //  UnknownShape
			//    //    XmlElementBase.AddElement(element, SpElement.QualifiedName);                
			//    XmlElementBase.AddRootShapeElement(element, shape);
			//} 

			#endregion  // Old Code
			//  First populate the shape list
			for (int i = 0; i < group.Shapes.Count; i++)
			{
				WorksheetShape shape = group.Shapes[i];
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
				WorksheetShapeSerializationManager.OnBeforeShapeSaved(shape, false);

				XmlElementBase.AddRootShapeElement(element, shape);
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