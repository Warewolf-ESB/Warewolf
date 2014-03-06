using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{
	// MD 8/23/11 - TFS84306
	internal class LnElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		//<complexType name="CT_LineProperties">
		//<sequence>
		//<group ref="EG_LineFillProperties" minOccurs="0" maxOccurs="1"/>
		//<group ref="EG_LineDashProperties" minOccurs="0" maxOccurs="1"/>
		//<group ref="EG_LineJoinProperties" minOccurs="0" maxOccurs="1"/>
		//<element name="headEnd" type="CT_LineEndProperties" minOccurs="0" maxOccurs="1"/>
		//<element name="tailEnd" type="CT_LineEndProperties" minOccurs="0" maxOccurs="1"/>
		//<element name="extLst" type="CT_OfficeArtExtensionList" minOccurs="0" maxOccurs="1"/>
		//</sequence>
		//<attribute name="w" type="ST_LineWidth" use="optional"/>
		//<attribute name="cap" type="ST_LineCap" use="optional"/>
		//<attribute name="cmpd" type="ST_CompoundLine" use="optional"/>
		//<attribute name="algn" type="ST_PenAlignment" use="optional"/>
		//</complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>spPr</summary>
		public const string LocalName = "ln";

		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/ln</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			LnElement.LocalName;

		// MD 3/12/12 - TFS102234
		public const string WAttributeName = "w";

		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return LnElement.QualifiedName; }
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

			WorksheetShape shape = (WorksheetShape)manager.ContextStack[typeof(WorksheetShape)];
			if (shape == null)
			{
				Utilities.DebugFail("Something is wrong.");
				return;
			}

			ShapeOutlineSolid outline = new ShapeOutlineSolid();
			shape.Outline = outline;
			manager.ContextStack.Push(outline);

			// MD 3/12/12 - TFS102234
			object attributeValue;
			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
				switch (attributeName)
				{
					case LnElement.WAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Int32, 0);
							outline.WidthInternal = (int)attributeValue;
						}
						break;
				}
			}

			// Push on a ChildDataItem to capture the color.
			manager.ContextStack.Push(new ChildDataItem());
		}

			#endregion Load

			#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			// MD 3/12/12 - TFS102234
			// Moved the clearing logic for fills and outlines to WorksheetShapeSerializationManager.OnAfterShapeLoaded because we may need 
			// them after the main element for the fill or outline is loaded.
			//ChildDataItem childDataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
			//WorksheetShape shape = (WorksheetShape)manager.ContextStack[typeof(WorksheetShape)];
			//
			//if (shape == null || childDataItem == null)
			//{
			//    Utilities.DebugFail("Something went wrong.");
			//    return;
			//}
			//
			//// If the child was a noFill element, there will be no color, so we should clear the outline.
			//if ((childDataItem.Data is Color) == false)
			//    shape.Outline = null;
		}

			#endregion  // OnAfterLoadChildElements

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