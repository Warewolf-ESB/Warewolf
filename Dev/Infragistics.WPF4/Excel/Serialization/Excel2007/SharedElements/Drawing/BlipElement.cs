using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Net;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{



	internal class BlipElement : XmlElementBase, IConsumedElementValueProvider
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_Blip">
		// <sequence>
		// <choice minOccurs="0" maxOccurs="unbounded">
		// <element name="alphaBiLevel" type="CT_AlphaBiLevelEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="alphaCeiling" type="CT_AlphaCeilingEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="alphaFloor" type="CT_AlphaFloorEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="alphaInv" type="CT_AlphaInverseEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="alphaMod" type="CT_AlphaModulateEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="alphaModFix" type="CT_AlphaModulateFixedEffect" minOccurs="1"
		// maxOccurs="1"/>
		// <element name="alphaRepl" type="CT_AlphaReplaceEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="biLevel" type="CT_BiLevelEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="blur" type="CT_BlurEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="clrChange" type="CT_ColorChangeEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="clrRepl" type="CT_ColorReplaceEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="duotone" type="CT_DuotoneEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="fillOverlay" type="CT_FillOverlayEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="grayscl" type="CT_GrayscaleEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="hsl" type="CT_HSLEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="lum" type="CT_LuminanceEffect" minOccurs="1" maxOccurs="1"/>
		// <element name="tint" type="CT_TintEffect" minOccurs="1" maxOccurs="1"/>
		// </choice>
		// <element name="extLst" type="CT_OfficeArtExtensionList" minOccurs="0" maxOccurs="1"/>
		// </sequence>
		// <attributeGroup ref="AG_Blob"/>
		// <attribute name="cstate" type="ST_BlipCompression" use="optional" default="none"/>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>blip</summary>
		public const string LocalName = "blip";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/blip</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			BlipElement.LocalName;

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
		//private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
		//private const string RelationshipsNamespacePrefix = "r";
		//
		//internal const string EmbedAttributeName = BlipElement.RelationshipsNamespace + "/embed";
		internal const string EmbedAttributeName = XmlElementBase.RelationshipsNamespace + "/embed";

		// MD 10/24/11 - TFS91531
		internal const string LinkName = XmlElementBase.RelationshipsNamespace + "/link";

        #endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return BlipElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            if ( this.consumedValues != null )
                this.consumedValues.Clear();

			// MD 10/30/11 - TFS90733
			// Other shapes (such as controls) can also store an image.
            //WorksheetImage image = manager.ContextStack[typeof(WorksheetImage)] as WorksheetImage;
			IWorksheetImage image = (IWorksheetImage)manager.ContextStack[typeof(IWorksheetImage)];

			object attributeValue = null;
            string rId = string.Empty;

			

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{
					#region embed
					// Name = 'embed', Type = ST_RelationshipId, Default = none
					case BlipElement.EmbedAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_RelationshipID, string.Empty );
                        rId = attributeValue as string;
                        this.consumedValues.Add( attributeName, HandledAttributeIdentifier.BlipElement_Embed );
					}
					break;
					#endregion embed

					// MD 10/24/11 - TFS91531
					#region link
					case BlipElement.LinkName:
					{
						attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_RelationshipID, string.Empty);
						rId = attributeValue as string;
					}
					break; 
					#endregion  // link
				}
			}

			// MD 10/24/11 - TFS91531
			// We should check to make sure we have an id so an exception is not thrown below.
			if (String.IsNullOrEmpty(rId))
			{
				Utilities.DebugFail("Could not find the relationship ID.");
				return;
			}

            //  Get the image out of the holder and assign it to the
            //  WorksheetImage's Image property
			// MD 10/24/11 - TFS91531
			// The data won't be an ImageHolder instance if it is an externally linked image.
            //ImageHolder imageHolder = manager.GetRelationshipDataFromActivePart(rId) as ImageHolder;
			object relationshipData = manager.GetRelationshipDataFromActivePart(rId);
			ImageHolder imageHolder = relationshipData as ImageHolder;

			if (imageHolder == null)
			{

				ExternalLinkPartInfo externalLink = relationshipData as ExternalLinkPartInfo;
				if (externalLink != null)
				{
					try
					{
						using (Stream imageStream = externalLink.GetStream())
							imageHolder = ImageBasePart.GetImageHolder(imageStream);
					}
					catch (Exception ex)
					{
						Utilities.DebugFail("Error getting image:\n" + ex.ToString());
					}
				}


				if (imageHolder == null)
				{
					Utilities.DebugFail("Could not find the ImageHolder instance.");
					return;
				}
			}

            image.Image = imageHolder.Image;

		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
		}

			#endregion Save

		#endregion Base class overrides

        //  BF 8/25/08
        #region IConsumedElementValueProvider implementation

		private Dictionary<string, HandledAttributeIdentifier> consumedValues = new Dictionary<string, HandledAttributeIdentifier>( 1 );

        /// <summary>
        /// cNvPr.id = Infragistics.Documents.Excel.WorksheetShape.ShapeId
        /// </summary>
        Dictionary<string, HandledAttributeIdentifier> IConsumedElementValueProvider.ConsumedValues
        {
            get{ return this.consumedValues; }
        }

        #endregion IConsumedElementValueProvider implementation

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