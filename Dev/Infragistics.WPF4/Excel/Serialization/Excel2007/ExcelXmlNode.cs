using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
	// MD 9/25/09 - TFS21642
	internal class ExcelXmlNode
	{
		private List<ExcelXmlNode> childNodes = new List<ExcelXmlNode>();

		// MD 2/16/12 - 12.1 - Table Support
		private int index = -1;

		private string name;
		private ExcelXmlDocument ownerDocument;
		private ExcelXmlNode parentNode;

		public ExcelXmlNode( ExcelXmlDocument ownerDocument )
		{
			this.ownerDocument = ownerDocument;
		}

		#region Methods

		public ExcelXmlNode AppendChild( ExcelXmlNode node )
		{
			// MD 2/16/12 - 12.1 - Table Support
			node.index = this.childNodes.Count;

			this.childNodes.Add( node );
			node.parentNode = this;
			return node;
		}

		public string GetPrefixOfNamespace( string namespaceURI )
		{
			string prefixOfNamespaceStrict = this.GetPrefixOfNamespaceStrict( namespaceURI );

			if ( prefixOfNamespaceStrict == null )
				return string.Empty;

			return prefixOfNamespaceStrict;
		}

		private string GetPrefixOfNamespaceStrict( string namespaceURI )
		{
			ExcelXmlDocument document = this.OwnerDocument;

			if ( document == null )
				return null;

			namespaceURI = document.NameTable.Add( namespaceURI );
			ExcelXmlNode parentNode = this;

			while ( parentNode != null )
			{
				if ( parentNode.NodeType == XmlNodeType.Element )
				{
					ExcelXmlElement element = (ExcelXmlElement)parentNode;
					List<ExcelXmlAttribute> attributes = element.Attributes;

					for ( int i = 0; i < attributes.Count; i++ )
					{
						ExcelXmlAttribute attribute = attributes[ i ];

						if ( attribute.Prefix.Length == 0 )
						{
							if ( attribute.LocalName == document.strXmlns && attribute.Value == namespaceURI )
								return string.Empty;
						}
						else if ( attribute.Prefix == document.strXmlns )
						{
							if ( attribute.Value == namespaceURI )
								return attribute.LocalName;
						}
						else if ( attribute.NamespaceURI == namespaceURI )
						{
							return attribute.Prefix;
						}
					}

					if ( parentNode.NamespaceURI == namespaceURI )
						return parentNode.Prefix;

					parentNode = parentNode.ParentNode;
				}
				else if ( parentNode.NodeType == XmlNodeType.Attribute )
				{
					parentNode = ( (ExcelXmlAttribute)parentNode ).OwnerElement;
				}
				else
				{
					parentNode = parentNode.ParentNode;
				}
			}

			if ( object.Equals( document.strReservedXml, namespaceURI ) )
				return document.strXml;

			if ( object.Equals( document.strReservedXmlns, namespaceURI ) )
				return document.strXmlns;

			return null;
		}

		public virtual void WriteEnd() { }

		public void WriteNode()
		{
			this.WriteStart();
			this.WriteEnd();
		}

		public virtual void WriteStart() { }

		#endregion Methods

		#region Properties

		public List<ExcelXmlNode> ChildNodes
		{
			get { return this.childNodes; }
		}

		// MD 2/16/12 - 12.1 - Table Support
		public int Index
		{
			get { return this.index; }
		}

		public virtual string LocalName { get { return null; } }

		public string Name
		{
			get
			{
				if ( this.name == null )
				{
					string prefix = this.Prefix;
					string localName = this.LocalName;
					ExcelXmlDocument ownerDocument = this.OwnerDocument;

					if ( prefix.Length > 0 )
					{
						if ( localName.Length > 0 )
							this.name = ownerDocument.NameTable.Add( prefix + ":" + localName );
						else
							this.name = prefix;
					}
					else
					{
						this.name = localName;
					}
				}

				return this.name;
			}
		}

		public virtual string NamespaceURI
		{
			get { return string.Empty; }
		}

		public virtual XmlNodeType NodeType { get { return XmlNodeType.None;} }

		public ExcelXmlDocument OwnerDocument
		{
			get
			{
				if ( this.NodeType == XmlNodeType.Document )
					return (ExcelXmlDocument)this;

				return this.ownerDocument;
			}
		}

		public ExcelXmlNode ParentNode
		{
			get { return this.parentNode; }
		}

		public virtual string Prefix
		{
			get { return string.Empty; }
		}

		public virtual string Value
		{
			get { return null; }
			set { Utilities.DebugFail( "The Value can't be set on the base node." ); }
		}

		#endregion Properties
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