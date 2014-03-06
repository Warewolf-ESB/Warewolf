using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class ChExtElement : ExtentElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_PositiveSize2D">
		// <attribute name="cx" type="ST_PositiveCoordinate" use="required"/>
		// <attribute name="cy" type="ST_PositiveCoordinate" use="required"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>chExt</summary>
		public const string LocalName = "chExt";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/chExt</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			ChExtElement.LocalName;

		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return ChExtElement.QualifiedName; }
		}

			#endregion ElementName

            #region GetExtent
        protected override void GetExtent( ContextStack contextStack, ref Transform.Extent extent )
        {
            WorksheetShape shape = contextStack[typeof(WorksheetShape)] as WorksheetShape;
            WorksheetShapeSerializationManager cache = shape != null ? shape.Excel2007ShapeSerializationManager : null;
            GroupTransform xfrm = cache != null ? cache.Transform as GroupTransform : null;

            if ( xfrm != null )
                extent = xfrm.ChExt;
        }
            #endregion GetExtent

		// MD 7/18/11 - Shape support
		// We now fully support this element, so we don't need to use the consumed value logic.
		#region Removed

		//    #region AttributeIdentifierCx
		//protected override HandledAttributeIdentifier AttributeIdentifierCx
		//{
		//    get { return HandledAttributeIdentifier.ChExtElement_Cx; }
		//}
		//    #endregion AttributeIdentifierCx

		//    #region AttributeIdentifierCy
		//protected override HandledAttributeIdentifier AttributeIdentifierCy
		//{
		//    get { return HandledAttributeIdentifier.ChExtElement_Cy; }
		//}
		//    #endregion AttributeIdentifierCy

		#endregion  // Removed

            #region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			// MD 7/15/11 - Shape support
			// Added support for saving this element.

			GroupTransform transform = (GroupTransform)manager.ContextStack[typeof(GroupTransform)];
			if (transform == null)
			{
				Utilities.DebugFail("Could not find the GroupTransform in the context stack.");
				return;
			}

			string attributeValue;
			attributeValue = XmlElementBase.GetXmlString(transform.ChExt.cx, DataType.ST_PositiveCoordinate);
			XmlElementBase.AddAttribute(element, ExtentElementBase.CxAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(transform.ChExt.cy, DataType.ST_PositiveCoordinate);
			XmlElementBase.AddAttribute(element, ExtentElementBase.CyAttributeName, attributeValue);
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