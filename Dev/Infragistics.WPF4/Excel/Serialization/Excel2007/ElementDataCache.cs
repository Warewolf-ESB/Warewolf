using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





	internal class ElementDataCache
	{
		#region Delegates

		public delegate string ReplaceConsumedValueCallback( HandledAttributeIdentifier handledAttributeIdentifier );

		#endregion Delegates

		#region Constants

		internal const string ElementValueConsumedValueKey = "<Value>";

		#endregion Constants

		#region Member variables

		// MD 8/31/11 - TFS85353
		// We may need to store the name components separately, and if they are stored that way, the 
		// qualifiedElementName should be null.
		//private string qualifiedElementName = string.Empty;
		private string qualifiedElementName;
		private string namespaceURI;
		private string localName;
		private string prefix;

		private string value = string.Empty;
		private Dictionary<string, string> attributeValues = null;
		private Dictionary<string, HandledAttributeIdentifier> consumedValues = null;
		private List<ElementDataCache> elements = null;

		#endregion Member variables

		#region Constructors
		public ElementDataCache( string qualifiedElementName )
			: this( qualifiedElementName, string.Empty )
		{
		}

		public ElementDataCache( string qualifiedElementName, string value )
		{
			this.qualifiedElementName = qualifiedElementName;
			this.value = value;
		}

		// MD 8/31/11 - TFS85353
		public ElementDataCache(string namespaceURI, string localName, string prefix, string value)
		{
			this.namespaceURI = namespaceURI;
			this.localName = localName;
			this.prefix = prefix;
			this.value = value;
		}

		#endregion Constructors

		#region Base Class Overrides

		#region ToString

		public override string ToString()
		{
			// MD 8/31/11 - TFS85353
			// If the qualifiedElementName is null, we should use the separate name components instead.
			//return this.QualifiedElementName;
			if (this.qualifiedElementName != null)
				return this.qualifiedElementName;

			return XmlElementBase.CombineNamespaceAndName(this.namespaceURI, this.localName);
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Properties

		#region AttributeValues

		public Dictionary<string, string> AttributeValues
		{
			get
			{
				if ( this.attributeValues == null )
					this.attributeValues = new Dictionary<string, string>();

				return this.attributeValues;
			}
		}

		public bool HasAttributeValues { get { return this.attributeValues != null && this.attributeValues.Count > 0; } }

		#endregion AttributeValues

		#region ConsumedValues

		public Dictionary<string, HandledAttributeIdentifier> ConsumedValues
		{
			get { return this.consumedValues; }
			set { this.consumedValues = value; }
		}

		#endregion ConsumedValues

		#region Elements

		public List<ElementDataCache> Elements
		{
			get
			{
				if ( this.elements == null )
					this.elements = new List<ElementDataCache>();

				return this.elements;
			}
		}

		public bool HasElements { get { return this.elements != null && this.elements.Count > 0; } }

		#endregion Elements

		#region QualifiedElementName
		public string QualifiedElementName { get { return this.qualifiedElementName; } }
		#endregion QualifiedElementName

		#region Value
		public string Value { get { return this.value; } }
		#endregion Value

		#endregion Properties

		#region Methods

		#region SaveDataInElement

		// MD 7/15/11 - Shape support
		// This method needs to take the serialization manager.
		//public void SaveDataInElement( ExcelXmlElement element, ReplaceConsumedValueCallback replaceConsumedValueCallback )
		public void SaveDataInElement(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ReplaceConsumedValueCallback replaceConsumedValueCallback)
		{
			bool hasConsumedValues = this.consumedValues != null && this.consumedValues.Count > 0;

			//  Write the attributes out
			if ( this.HasAttributeValues )
			{
				foreach ( KeyValuePair<string, string> attribute in this.AttributeValues )
				{
					string attributeValue = attribute.Value;

					if ( hasConsumedValues )
					{
						//  Get the value of the attribute from the member of our object model to which
						//  it corresponds.
						HandledAttributeIdentifier handledAttributeIdentifier;
						if ( consumedValues.TryGetValue( attribute.Key, out handledAttributeIdentifier ) )
							attributeValue = replaceConsumedValueCallback( handledAttributeIdentifier );
					}

					//  Write out the attribute value
					XmlElementBase.AddAttribute( element, attribute.Key, attributeValue );
				}
			}

			//  Get the element's value, which can be either a deserialized
			//  value or a property value of a member of our public object model.
			string elementValue = this.Value;
			if ( hasConsumedValues )
			{
				//  Get the value of the attribute from the member of our object
				//  model to which it corresponds.
				HandledAttributeIdentifier handledAttributeIdentifier;
				if ( consumedValues.TryGetValue( ElementDataCache.ElementValueConsumedValueKey, out handledAttributeIdentifier ) )
					elementValue = replaceConsumedValueCallback( handledAttributeIdentifier );
			}

			//  If the element has a value, write it out and return,
			//  since an element with a value cannot have attributes
			//  or child elements.
			if ( string.IsNullOrEmpty( elementValue ) == false )
			{
				XmlElementBase.AddValueNode( element, elementValue );
				return;
			}

			// MD 7/15/11 - Shape support
			
			// MD 8/31/11 - TFS85353
			// The QualifiedElementName has been removed.
			//if (this.QualifiedElementName == SpPrElement.QualifiedName || 
			//    this.QualifiedElementName == GrpSpPrElement.QualifiedName)
			string qualifiedElementName = this.ToString();
			if (qualifiedElementName == SpPrElement.QualifiedName ||
				qualifiedElementName == GrpSpPrElement.QualifiedName)
			{
				XmlElementBase.AddElement(element, XfrmElement.QualifiedName);
			}

			foreach ( ElementDataCache childElementCache in this.Elements )
			{
				// MD 7/15/11 - Shape support
				// This method needs to take the serialization manager.
				//childElementCache.SaveDataUnderParentElement( element, replaceConsumedValueCallback );
				childElementCache.SaveDataUnderParentElement(manager, element, replaceConsumedValueCallback);
			}
		}

		#endregion SaveDataInElement

		#region SaveDataUnderParentElement

		// MD 7/15/11 - Shape support
		// This method needs to take the serialization manager.
		//public void SaveDataUnderParentElement( ExcelXmlElement parentElement, ReplaceConsumedValueCallback replaceConsumedValueCallback )
		public void SaveDataUnderParentElement(Excel2007WorkbookSerializationManager manager, ExcelXmlElement parentElement, ReplaceConsumedValueCallback replaceConsumedValueCallback)
		{
			//  Create the actual XmlElement
			// MD 8/31/11 - TFS85353
			// If the qualifiedElementName is null, we should use the separate name components instead.
			//ExcelXmlElement element = XmlElementBase.AddElement( parentElement, this.QualifiedElementName );
			ExcelXmlElement element;
			if (this.qualifiedElementName != null)
				element = XmlElementBase.AddElement(parentElement, this.qualifiedElementName);
			else
				element = XmlElementBase.AddElement(parentElement, this.namespaceURI, this.localName, this.prefix);

			// MD 7/15/11 - Shape support
			// Set the flag indicating that this element has been pre-populated.
			element.IsPrePopulated = true;

			// MD 7/15/11 - Shape support
			// This method needs to take the serialization manager.
			//this.SaveDataInElement( element, replaceConsumedValueCallback );
			this.SaveDataInElement(manager, element, replaceConsumedValueCallback);
		}

		#endregion SaveDataUnderParentElement

		#endregion Methods
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