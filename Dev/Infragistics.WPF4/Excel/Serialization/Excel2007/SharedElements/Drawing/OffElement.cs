using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{
    #region OffElement



	internal class OffElement : OffsetElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_Point2D">
		// <attribute name="x" type="ST_Coordinate" use="required"/>
		// <attribute name="y" type="ST_Coordinate" use="required"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>off</summary>
		public const string LocalName = "off";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/off</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			OffElement.LocalName;

		#endregion Constants

		#region Base class overrides

            #region ElementName

		public override string ElementName
		{
			get { return OffElement.QualifiedName; }
		}

			#endregion ElementName

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			// MD 7/15/11 - Shape support
			// Added support for saving this element.

			Transform transform = (Transform)manager.ContextStack[typeof(Transform)];
			if (transform == null)
			{
				Utilities.DebugFail("Could not find the Transform in the context stack.");
				return;
			}

			string attributeValue;
			attributeValue = XmlElementBase.GetXmlString(transform.Off.x, DataType.ST_Coordinate);
			XmlElementBase.AddAttribute(element, OffsetElementBase.XAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(transform.Off.y, DataType.ST_Coordinate);
			XmlElementBase.AddAttribute(element, OffsetElementBase.YAttributeName, attributeValue);
		}

			#endregion Save

		#endregion Base class overrides

	}
    #endregion OffElement

    #region OffsetElementBase



	// MD 7/18/11 - Shape support
	// We now fully support this element, so we don't need to use the consumed value logic.
	//internal class OffsetElementBase : XmlElementBase, IConsumedElementValueProvider
	internal class OffsetElementBase : XmlElementBase
	{
		#region Constants

		// MD 7/15/11 - Shape support
		// Made these protected
		//private const string XAttributeName = "x";
		//private const string YAttributeName = "y";
        protected const string XAttributeName = "x";
		protected const string YAttributeName = "y";
		
        #endregion Constants

        #region ElementName

		public override string ElementName
		{
			get { throw new NotSupportedException("This should not be getting called."); }
		}

        #endregion ElementName

        #region GetOffset
        protected virtual void GetOffset( ContextStack contextStack, ref Transform.Offset offset )
        {
            WorksheetShape shape = contextStack[typeof(WorksheetShape)] as WorksheetShape;
            WorksheetShapeSerializationManager cache = shape != null ? shape.Excel2007ShapeSerializationManager : null;
            Transform xfrm = cache != null ? cache.Transform : null;

            if ( xfrm != null )
                offset = xfrm.Off;
        }
        #endregion GetOffset

		// MD 7/18/11 - Shape support
		// We now fully support this element, so we don't need to use the consumed value logic.
		#region Removed

		//#region AttributeIdentifierX
		//protected virtual HandledAttributeIdentifier AttributeIdentifierX
		//{
		//    get { return HandledAttributeIdentifier.OffElement_X; }
		//}
		//#endregion AttributeIdentifierX

		//#region AttributeIdentifierY
		//protected virtual HandledAttributeIdentifier AttributeIdentifierY
		//{
		//    get { return HandledAttributeIdentifier.OffElement_Y; }
		//}
		//#endregion AttributeIdentifierY

		#endregion  // Removed

        #region Load

        /// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			// MD 7/18/11 - Shape support
			// We now fully support this element, so we don't need to use the consumed value logic.
			//if ( this.consumedValues != null )
			//    this.consumedValues.Clear();

            Transform.Offset offset = new Transform.Offset();
            this.GetOffset( manager.ContextStack, ref offset );
			object attributeValue = null;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{
					#region x
					// ***REQUIRED***
					// Name = 'x', Type = ST_Coordinate, Default = 
					case OffElement.XAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Coordinate, 0 );
						offset.x = (long)attributeValue;

						// MD 7/18/11 - Shape support
						// We now fully support this element, so we don't need to use the consumed value logic.
                        //this.consumedValues.Add( OffElement.XAttributeName, this.AttributeIdentifierX );
					}
					break;
					#endregion x

					#region y
					// ***REQUIRED***
					// Name = 'y', Type = ST_Coordinate, Default = 
					case OffElement.YAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Coordinate, 0 );
						offset.y = (long)attributeValue;

						// MD 7/18/11 - Shape support
						// We now fully support this element, so we don't need to use the consumed value logic.
                        //this.consumedValues.Add( OffElement.YAttributeName, this.AttributeIdentifierY );
					}
					break;
					#endregion y
				}
			}
		}

        #endregion Load

		#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
		}

        #endregion Save

		// MD 7/18/11 - Shape support
		// We now fully support this element, so we don't need to use the consumed value logic.
		#region Removed

		////  BF 8/25/08
		//#region IConsumedElementValueProvider implementation

		//private Dictionary<string, HandledAttributeIdentifier> consumedValues = new Dictionary<string, HandledAttributeIdentifier>(2);

		///// <summary>
		///// off.x, off.y = Infragistics.Documents.Excel.WorksheetShape.GetBoundsInTwips
		///// </summary>
		//Dictionary<string, HandledAttributeIdentifier> IConsumedElementValueProvider.ConsumedValues
		//{
		//    get { return this.consumedValues; }
		//}

		//#endregion IConsumedElementValueProvider implementation 

		#endregion  // Removed

    }
    #endregion OffsetElementBase

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