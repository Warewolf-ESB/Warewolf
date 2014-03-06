using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class PicElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_Picture">
		// <sequence>
		// <element name="nvPicPr" type="CT_PictureNonVisual" minOccurs="1" maxOccurs="1"/>
		// <element name="blipFill" type="a:CT_BlipFillProperties" minOccurs="1" maxOccurs="1"/>
		// <element name="spPr" type="a:CT_ShapeProperties" minOccurs="1" maxOccurs="1"/>
		// <element name="style" type="a:CT_ShapeStyle" minOccurs="0" maxOccurs="1"/>
		// </sequence>
		// <attribute name="macro" type="xsd:string" use="optional" default=""/>
		// <attribute name="fPublished" type="xsd:boolean" use="optional" default="false"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>pic</summary>
		public const string LocalName = "pic";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/pic</summary>
		public const string QualifiedName =
			DrawingsPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			PicElement.LocalName;

		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return PicElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            //  Create a WorksheetImage and push it onto the context stack.
			// MD 1/6/12 - TFS92740
			// Don't initialize default properties when we are loading.
            //WorksheetImage image = new WorksheetImage();
			WorksheetImage image = new WorksheetImage(false);

            WorksheetShapeSerializationManager.OnShapeCreated( image, manager.ContextStack );
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

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
            //  Get the image and its serialization manager off the context stack.
            WorksheetImage image = WorksheetShapeSerializationManager.ConsumeShape( manager.ContextStack, typeof(WorksheetImage) ) as WorksheetImage;
            WorksheetShapeSerializationManager serializationManager = image.Excel2007ShapeSerializationManager;

            //  Add the child elements
            WorksheetShapeSerializationManager.SaveWorksheetShape( manager, image, element );
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