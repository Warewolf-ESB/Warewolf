using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements
{



	internal class GraphicFrameElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_GraphicalObjectFrame">
		// <sequence>
		// <element name="nvGraphicFramePr" type="CT_GraphicalObjectFrameNonVisual" minOccurs="1"
		// maxOccurs="1"/>
		// <element name="xfrm" type="a:CT_Transform2D" minOccurs="1" maxOccurs="1"/>
		// <element ref="a:graphic" minOccurs="1" maxOccurs="1"/>
		// </sequence>
		// <attribute name="macro" type="xsd:string" use="optional"/>
		// <attribute name="fPublished" type="xsd:boolean" use="optional" default="false"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>graphicFrame</summary>
		public const string LocalName = "graphicFrame";

		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/graphicFrame</summary>
		public const string QualifiedName =
			DrawingsPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			GraphicFrameElement.LocalName;

		private const string MacroAttributeName = "macro";
		private const string FPublishedAttributeName = "fPublished";

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return GraphicFrameElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			// MD 10/12/10 - TFS49853
			// Implemented this method.

			object attributeValue = null;
			ShapeAttributes shapeAttributes = new ShapeAttributes();

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					#region macro
					// Name = 'macro', Type = String, Default = 
					case GraphicFrameElement.MacroAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.String, string.Empty);
							shapeAttributes.macro = attributeValue as string;
						}
						break;
					#endregion macro

					#region fPublished
					// Name = 'fPublished', Type = Boolean, Default = False
					case GraphicFrameElement.FPublishedAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, ShapeAttributes.Default_fPublished);
							shapeAttributes.fPublished = (bool)attributeValue;
						}
						break;
					#endregion fPublished
				}
			}

			//  Create a new UnknownShape, and call the OnShapeCreated method, which handles
			//  the various things that we need to do when we get a new shape element.
			WorksheetChart shape = new WorksheetChart();
			WorksheetShapeSerializationManager.OnShapeCreated(shape, manager.ContextStack, shapeAttributes);
			WorksheetShapeSerializationManager.LoadChildElements(manager, element, ref isReaderOnNextNode);
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

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			// MD 10/12/10 - TFS49853
			// Implemented this method.

			//  Get the image and its serialization manager off the context stack.
			WorksheetChart shape = WorksheetShapeSerializationManager.ConsumeShape(manager.ContextStack, typeof(WorksheetChart)) as WorksheetChart;

			string relationshipId;
			Uri chartPartPath = manager.CreatePartInPackage(
				ChartPart.ContentTypeValue, 
				manager.GetNumberedPartName(ChartPart.BasePartName), 
				shape, 
				out relationshipId);
			RelationshipIdHolder relationshipIdHolder = new RelationshipIdHolder(relationshipId);
			manager.ContextStack.Push(relationshipIdHolder);

			WorksheetShapeSerializationManager serializationManager = shape.Excel2007ShapeSerializationManager;

			string attributeValue = string.Empty;
			ShapeAttributes shapeAttributes = serializationManager.ShapeAttributes;

			//  Break out the attribute values
			string macro = shapeAttributes != null ? shapeAttributes.macro : string.Empty;
			bool fPublished = shapeAttributes != null ? shapeAttributes.fPublished : ShapeAttributes.Default_fPublished;


			#region Macro
			// Name = 'macro', Type = String, Default = 
			//  Note that Excel seems to always write this attribute out, so we will too.
			attributeValue = macro;
			XmlElementBase.AddAttribute(element, GraphicFrameElement.MacroAttributeName, attributeValue);

			#endregion Macro

			#region FPublished
			// Name = 'fPublished', Type = Boolean, Default = False

			if (fPublished != ShapeAttributes.Default_fPublished)
			{
				attributeValue = XmlElementBase.GetXmlString(fPublished, DataType.Boolean);
				XmlElementBase.AddAttribute(element, GraphicFrameElement.FPublishedAttributeName, attributeValue);
			}

			#endregion FPublished

			//  Add the child elements
			WorksheetShapeSerializationManager.SaveWorksheetShape(manager, shape as WorksheetChart, element);

			manager.ContextStack.Pop(); // relationshipIdHolder
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