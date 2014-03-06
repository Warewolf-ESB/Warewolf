using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class SolidFillElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_SolidColorFillProperties">
		// <sequence>
		// <group ref="EG_ColorChoice" minOccurs="0" maxOccurs="1"/>
		// </sequence>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>solidFill</summary>
		public const string LocalName = "solidFill";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/solidFill</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			SolidFillElement.LocalName;


		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return SolidFillElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			// MD 6/20/12 - TFS115074
			// If there is a FormattedTextParagraph on the context stack, this element applies to the paragraph, not the shape itself,
			// so skip loading this element since we don't currently support this.
			FormattedTextParagraph paragraph = manager.ContextStack.Get<FormattedTextParagraph>();
			if (paragraph != null)
			{
				
				return;
			}

			// MD 8/23/11 - TFS84306
			// Implemented the loading of this element.
			// If there is no ISolidColorItem on the stack, this solidFill element is at the root of the shape properties, 
			// meaning it is the background color, so push on a background instance.
			if (manager.ContextStack[typeof(ISolidColorItem)] == null)
			{
				WorksheetShape shape = (WorksheetShape)manager.ContextStack[typeof(WorksheetShape)];
				if (shape == null)
				{
					Utilities.DebugFail("Something is wrong.");
					return;
				}

				ShapeFillSolid background = new ShapeFillSolid();
				shape.Fill = background;
				manager.ContextStack.Push(background);
			}

			// There may already be a ChildDataItem on the conext stack to hold the color. If so, don't add one below.
			ChildDataItem existingItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
			if (existingItem != null && existingItem.Data == null)
				return;

			// Push on a ChildDataItem to capture the color.
			manager.ContextStack.Push(new ChildDataItem());
		}

			#endregion Load

			#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			// MD 8/23/11 - TFS84306
			// Implemented the loading of this element.
			ChildDataItem childDataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
			ISolidColorItem item = (ISolidColorItem)manager.ContextStack[typeof(ISolidColorItem)];

			if (item == null || childDataItem == null)
			{
				// MD 6/20/12 - TFS115074
				// This is a valid situation now.
				//Utilities.DebugFail("Something went wrong.");
				return;
			}

			
			if ((childDataItem.Data is Color) == false)
			{
				// MD 3/12/12 - TFS102234
				// Moved the clearing logic for fills and outlines to WorksheetShapeSerializationManager.OnAfterShapeLoaded because we may need 
				// them after the main element for the fill or outline is loaded.
				//WorksheetShape shape = (WorksheetShape)manager.ContextStack[typeof(WorksheetShape)];
				//if (shape != null)
				//{
				//    if (item is ShapeOutline)
				//        shape.Outline = null;
				//    else if (item is ShapeFill)
				//        shape.Fill = null;
				//}

				return;
			}

			item.Color = (Color)childDataItem.Data;
		}

			#endregion  // OnAfterLoadChildElements

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			// MD 8/23/11 - TFS84306
			// Started implementing this method, but it is not actually used yet, so I left it commented out.
			//WorksheetShape shape = (WorksheetShape)manager.ContextStack[typeof(WorksheetShape)];
			//if (shape == null)
			//{
			//    Utilities.DebugFail("Something went wrong.");
			//    return;
			//}

			//SolidShapeBackground background = shape.Background as SolidShapeBackground;
			//if (background == null)
			//{
			//    Utilities.DebugFail("Something went wrong.");
			//    return;
			//}

			//ChildDataItem childDataItem = new ChildDataItem();
			//childDataItem.Data = background.Color;
			//manager.ContextStack.Push(childDataItem);

			//XmlElementBase.AddElement(element, SrgbClrElement.QualifiedName);
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