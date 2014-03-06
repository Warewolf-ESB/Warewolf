using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.LegacyDrawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Variants;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;





using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
	internal abstract class XmlElementBase
	{

		#region Delegates

		public delegate void AfterLoadElementCallback( XmlElementBase elementHandler );
		public delegate void BeforeLoadElementCallback( XmlElementBase elementHandler, ElementDataCache parentElementCache, ref List<ElementDataCache> parentElementCacheCollection ); 

		#endregion Delegates

		#region Constants

		protected const string NamespaceSeparator = "/";

		private const string NamespaceDeclarationPrefix = "xmlns";
		private const string NamespaceDeclarationNamespace = "http://www.w3.org/2000/xmlns/";
        private const string GuidRegexPattern = "{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}}";

        /// <summary>http://schemas.openxmlformats.org/drawingml/2006/main</summary>
        public const string DrawingMLNamespace = "http://schemas.openxmlformats.org/drawingml/2006/main";

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
		protected const string RelationshipIdAttributeName =
			XmlElementBase.RelationshipsNamespace +
			XmlElementBase.NamespaceSeparator +
			XmlElementBase.IdAttributeName;
		protected const string IdAttributeName = "id";
		protected const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
		protected const string RelationshipsNamespacePrefix = "r";

		#endregion Constants

		#region Static Variables

		[ThreadStatic]
		private static Dictionary<string, XmlElementBase> elements;

		// MD 10/20/10 - TFS36617
		// Cached the most commonly used element handlers separately so we don't have to do a lookup in the 
		// main dictionary for each element we encounter of these types.
		#region Common Element Handlers

		[ThreadStatic]
		private static CellElement cellElement;

		[ThreadStatic]
		private static CellValueElement cellValueElement;

		[ThreadStatic]
		private static RowElement rowElement;

		// MD 11/3/10 - TFS49093
		// Added some more common element types.
		[ThreadStatic]
		private static StringItemElement stringItemElement;

		[ThreadStatic]
		private static TextElement textElement;

		#endregion // Common Element Handlers

		#endregion Static Variables

		#region Methods

		#region Abstract/Virtual Methods



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// MD 9/28/09 - TFS21642
		// Added a parameter to indicate whether the reader is already on the next node we want to process.
		//protected abstract void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value );
		protected abstract void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode );

        //  BF 10/7/10  NA 2011.1 - Infragistics.Word
        //  Made this virtual (was abstract) with an assertion since now some
        //  of the classes that derive from this one will have to support Word
        //  as well.


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		protected virtual void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
        {
			Utilities.DebugFail("(Excel) The base class method should never be called.");
        }

		#endregion Abstract/Virtual Methods

		#region Public Methods

		#region AddValueNode

		public static void AddValueNode( ExcelXmlElement element, string value )
		{
			Debug.Assert( element.ChildNodes.Count == 0, "An element cannot have a value and child nodes." );

			ExcelXmlNode valueNode = null;

            if (value.Trim().Length == 0)
            {
                // MBS 8/20/09 - TFS21034
                // The CreateSignificantWhitespace method only accepts a few different characters,
                // otherwise an exception will be thrown
                //
                //valueNode = element.OwnerDocument.CreateSignificantWhitespace(value);
                //
                bool isValidWhitespace = true;
                char[] charArray = value.ToCharArray();
                for (int i = 0; i < charArray.Length; i++)
                {
                    int charCode = (int)charArray[i];
                    if (charCode != 20 && charCode != 10 && charCode != 13 && charCode != 9)
                    {
                        isValidWhitespace = false;
                        break;
                    }
                }           
                // If any of the characters in the value are not valid for the CreateSignificantWhitspace
                // method, just add the value as a regular text node.
                if(isValidWhitespace)
                    valueNode = element.OwnerDocument.CreateSignificantWhitespace(value);
                else
                    valueNode = element.OwnerDocument.CreateTextNode(value);
            }
            else
                valueNode = element.OwnerDocument.CreateTextNode(value);

			element.AppendChild( valueNode );
		} 

		#endregion AddValueNode

		#region GetElementValue

		public static string GetElementValue( ExcelXmlElement element )
		{
			if ( element.ChildNodes.Count != 1 )
				return null;

			ExcelXmlNode childNode = element.ChildNodes[ 0 ];

			if ( childNode is ExcelXmlText || childNode is ExcelXmlSignificantWhitespace )
				return childNode.Value;

			return null;
		}

		#endregion GetElementValue 

		#region LoadChildElements

		// MD 9/28/09 - TFS21642
		// Added a parameter to indicate whether the reader is already on the next node we want to process.
		//public static void LoadChildElements( Excel2007WorkbookSerializationManager manager, ExcelXmlNode node )
		public static void LoadChildElements( Excel2007WorkbookSerializationManager manager, ExcelXmlNode node, ref bool isReaderOnNextNode )
		{
			XmlElementBase.LoadChildElements( manager, node, null, null, ref isReaderOnNextNode );
		}

		public static void LoadChildElements( 
			Excel2007WorkbookSerializationManager manager,
			ExcelXmlNode node, 
			BeforeLoadElementCallback beforeLoadElementHandler,
			AfterLoadElementCallback afterLoadElementHandler,
			// MD 9/28/09 - TFS21642
			// Added a parameter to indicate whether the reader is already on the next node we want to process.
			ref bool isReaderOnNextNode )
		{
			// MD 9/28/09 - TFS21642
			// We need to pass in the document and reader now.
			//XmlElementBase.LoadChildElements( manager, node, null, null, beforeLoadElementHandler, afterLoadElementHandler, ref isReaderOnNextNode );
			ExcelXmlDocument document = node.OwnerDocument;
			XmlReader reader = document.Reader;
			XmlElementBase.LoadChildElements( manager, node, document, reader, null, null, beforeLoadElementHandler, afterLoadElementHandler, ref isReaderOnNextNode );
		}

		private static void LoadChildElements(
			Excel2007WorkbookSerializationManager manager,
			ExcelXmlNode node,

			// MD 9/28/09 - TFS21642
			// Added a few parameters so we don't have to get these from the node on each recursive call.
			ExcelXmlDocument document,
			XmlReader reader,

			ElementDataCache elementCache,
			List<ElementDataCache> elementCacheCollection,
			BeforeLoadElementCallback beforeLoadElementHandler,
			AfterLoadElementCallback afterLoadElementHandler,

			// MD 9/28/09 - TFS21642
			// Added a parameter to indicate whether the reader is already on the next node we want to process.
			ref bool isReaderOnNextNode )
		{
			// Process all child nodes
			// MD 9/28/09 - TFS21642
			// The child nodes collection is not populated because we are reading the nodes as we process them, so we just have to 
			// continue iterating until we finish processing the node.
			//foreach ( ExcelXmlNode childNode in node.ChildNodes )
			while ( true )
			{
				// MD 9/28/09 - TFS21642
				// If we have reached the end of the xml document, exit the loop.
				if ( isReaderOnNextNode == false && reader.Read() == false )
					break;

				// MD 9/28/09 - TFS21642
				// Read the next node from the document.
				bool isEmptyElement;
				ExcelXmlNode childNode = XmlElementBase.CreateNextNode( document, reader, out isReaderOnNextNode, out isEmptyElement );

				// MD 9/28/09 - TFS21642
				// If no node was read, break out of the loop, because we hit the EndElement of the parent node.
				if ( childNode == null )
					break;

				// MD 9/28/09 - TFS21642
				// Add the child node to the parent.
				node.AppendChild( childNode );

				ExcelXmlElement childElement = childNode as ExcelXmlElement;

				if ( childElement == null )
					continue;

				// Determine the full name of the element and find a class to handle the loading of the element
				string qualifiedName = XmlElementBase.GetQualifiedElementName( childElement );
				XmlElementBase elementHandler = XmlElementBase.GetElement( qualifiedName, false );

				// If a BeforeLoadElementCallback has been specified, call into it now, before we check if the element handler is valid.
				if ( beforeLoadElementHandler != null )
					beforeLoadElementHandler( elementHandler, elementCache, ref elementCacheCollection );

				// If there is no handler for the element and we have no place to cache the general data, skip the element, there is 
				// nothing we can do.
				if ( elementHandler == null && elementCacheCollection == null )
				{





					// MD 9/28/09 - TFS21642
					// If the element is not an empty element, we need to skip past all the children.
					if ( isEmptyElement == false )
					{
						if ( isReaderOnNextNode )
						{
							// If the reader is already on the first child node, skip all children and their descendants.
							// Once we hit the EndElement node, we are at the end of the  current node. Then set 
							// isReaderOnNextNode to False so we perform the read on the next iteration of the loop.
							while ( reader.NodeType != XmlNodeType.EndElement )
								reader.Skip();

							isReaderOnNextNode = false;
						}
						else
						{
							// If we are not on the next node, just skip the current node and its descendents.
							reader.Skip();
						}
					}

					continue;
				}

				ElementDataCache childWrapper = null;
				List<ElementDataCache> childElementCacheCollection = null;

				// If the collection into which the element general data should be cached has been specified, cache the general data.
				// MD 7/15/11 - Shape support
				// Suspend the normal element caching logic for everything under the xfrm element, because we will write out everything under that in the normal way.
				
				//if ( elementCacheCollection != null )
				//{
				//    childWrapper = XmlElementBase.CacheElementValues( elementHandler, childElement );
				//    childElementCacheCollection = childWrapper.Elements;
				//
				//    elementCacheCollection.Add( childWrapper );
				//}
				if (elementCacheCollection != null)
				{
					if (qualifiedName != XfrmElement.QualifiedName)
					{
						childWrapper = XmlElementBase.CacheElementValues(elementHandler, childElement);
						childElementCacheCollection = childWrapper.Elements;

						elementCacheCollection.Add(childWrapper);
					}
					else
					{
						beforeLoadElementHandler = null;
						afterLoadElementHandler = null;
					}
				}

				// Keep track of the current context stack size, so we can clear off all items added by this element's loading when the 
				// child elements have been loaded.
				int oldContextStackCount = manager.ContextStack.Count;

				string value = null;
				bool processChildNodes = true;

				// If there is a handler for this element, let it load the element.
				if ( elementHandler != null )
				{
					// If the element does not want the default processing of its children, don't process the child element later.
					processChildNodes = elementHandler.LoadChildNodes;
					value = XmlElementBase.GetElementValue( childElement );

					if ( value != null )
					{
						Debug.Assert( childElement.ChildNodes.Count == 1, "The node should only have one child if it has a value." );
						processChildNodes = false;
					}

					// Let the handler load the element.
					elementHandler.Load( manager, childElement, value, ref isReaderOnNextNode );
				}

				// If an AfterLoadElementCallback has been specified, call into it now
				if ( afterLoadElementHandler != null )
					afterLoadElementHandler( elementHandler );

				// MD 9/28/09 - TFS21642
				// Is isEmptyElement is True, there are definitely no children, so don't even call LoadChildElements. It also causes a 
				// problem if we do that.
				//if ( processChildNodes )
				// MD 7/15/11 - Shape support
				// We can't just skip this logic when processChildNodes is False. We need to also skip passed any descendant nodes recursively until we hit the end element of this node.
				//if ( isEmptyElement == false && processChildNodes )
				//{
				//    // Process all child nodes
				//    XmlElementBase.LoadChildElements( manager, childNode, document, reader, childWrapper, childElementCacheCollection, 
				//        beforeLoadElementHandler, afterLoadElementHandler, ref isReaderOnNextNode );
				//}
				if (isEmptyElement == false)
				{
					if (processChildNodes)
					{
						// Process all child nodes
						XmlElementBase.LoadChildElements(manager, childNode, document, reader, childWrapper, childElementCacheCollection,
							beforeLoadElementHandler, afterLoadElementHandler, ref isReaderOnNextNode);
					}
					else
					{
						// If the reader is already on the first child node, skip all children and their descendants.
						// Once we hit the EndElement node, we are at the end of the  current node. Then set 
						// isReaderOnNextNode to False so we perform the read on the next iteration of the loop.
						while (reader.NodeType != XmlNodeType.EndElement)
							reader.Skip();

						isReaderOnNextNode = false;
					}
				}

				// If there is a handler for this element, let it perform any post-processing needed after loading the child elements.
				if ( elementHandler != null )
				{
					// MD 7/15/11 - Shape support
					// The element cache passed in is the parent element cache. We need to pass along our element cache.
					//elementHandler.OnAfterLoadChildElements( manager, elementCache );
					elementHandler.OnAfterLoadChildElements(manager, childWrapper);
				}

				// Pop off all contexts which were added to the stack when processing this element
				manager.ContextStack.ClearToCount( oldContextStackCount );

				// MD 9/28/09 - TFS21642
				// If the node wasn't empty, it has an 
				if ( isEmptyElement == false && reader.NodeType == XmlNodeType.EndElement )
				{
					Debug.Assert( 
						childElement.LocalName == reader.LocalName, 
						"The EndElement we are reading past does not belong to the current child node being processed." );

					isReaderOnNextNode = true;
					if ( reader.Read() == false )
						break;
				}
			}

			// MD 9/25/09 - TFS21642
			// Since we are writing nodes as we go, we can clear the child collection so we don't keep these items 
			// in memory unnecessarily.
			node.ChildNodes.Clear();
		}

		#endregion LoadChildElements

		#region SaveChildElements

        //  BF 10/7/10  NA 2011.1 - Infragistics.Word
        //  Refactored everything that was in this region
        #region Refactored
        //public static void SaveChildElements( Excel2007WorkbookSerializationManager manager, ExcelXmlNode node )
        //{
        //    XmlElementBase.SaveChildElements( manager, node, null );
        //}

        //public static void SaveChildElements( Excel2007WorkbookSerializationManager manager, ExcelXmlNode node, ElementDataCache elementCache )
        //{
        //    foreach ( ExcelXmlNode childNode in node.ChildNodes )
        //    {
        //        ExcelXmlElement childElement = childNode as ExcelXmlElement;

        //        if ( childElement == null )
        //        {
        //            // MD 9/25/09 - TFS21642
        //            // Write out any nodes which aren't elements.
        //            childNode.WriteNode();

        //            continue;
        //        }

        //        // The shape serialization manager might have already populate the nodes. If that's the case, just skip this element.
        //        if ( childElement.Attributes.Count != 0 || childElement.ChildNodes.Count != 0 )
        //        {
        //            // MD 9/25/09 - TFS21642
        //            // Write out any nodes which are already populated
        //            XmlElementBase.WriteNodeRecursive( childElement );

        //            continue;
        //        }

        //        string qualifiedName = XmlElementBase.GetQualifiedElementName( childElement );
        //        XmlElementBase elementHandler = XmlElementBase.GetElement( qualifiedName );

        //        if ( elementHandler == null )
        //            continue;

        //        int oldContextStackCount = manager.ContextStack.Count;

        //        string value = null;
        //        elementHandler.Save( manager, childElement, ref value );

        //        if ( value != null )
        //        {
        //            XmlElementBase.AddValueNode( childElement, value );

        //            // MD 9/25/09 - TFS21642
        //            // We are now going to write out elements as we go so we don't have to store them in memory.
        //            XmlElementBase.WriteNodeRecursive( childElement );
        //        }
        //        else
        //        {
        //            // MD 9/25/09 - TFS21642
        //            // We are now going to write out elements as we go so we don't have to store them in memory.
        //            childElement.WriteStart();

        //            // MD 9/25/09
        //            // Found while fixing TFS21642
        //            // Recursively call the new overload.
        //            //XmlElementBase.SaveChildElements( manager, childElement );
        //            XmlElementBase.SaveChildElements( manager, childElement, elementCache );

        //            // MD 9/25/09 - TFS21642
        //            // We are now going to write out elements as we go so we don't have to store them in memory.
        //            childElement.WriteEnd();
        //        }

        //        // Pop off all contexts which were added to the stack when processing this node
        //        manager.ContextStack.ClearToCount( oldContextStackCount );
        //    }

        //    // MD 9/25/09 - TFS21642
        //    // Since we are writing nodes as we go, we can clear the child collection so we don't keep these items 
        //    // in memory unnecessarily.
        //    node.ChildNodes.Clear();
        //}
        #endregion Refactored

        #region Save handlers
        
        //  These handlers are used by the SaveChildElements method.
        //  They were necessary in order to cleanly refactor the existing
        //  serialization code so that it could be used by Word as well. 
        
        private delegate void SaveHandler<T>(
            XmlElementBase elementHandler,
            T manager,
            ExcelXmlElement element,
            ref string s);

        private static void ExcelElementSaveHandler(
            XmlElementBase elementHandler,
            Excel2007WorkbookSerializationManager manager,
            ExcelXmlElement element,
            ref string value)
        {
            elementHandler.Save(manager, element, ref value);
        }

        #endregion Save handlers

		public static void SaveChildElements( Excel2007WorkbookSerializationManager manager, ExcelXmlNode node )
		{
			XmlElementBase.SaveChildElementsHelper( manager, node, null, ExcelElementSaveHandler );
		}

		private static void SaveChildElementsHelper<T>(
            T manager,
            ExcelXmlNode node,
            ElementDataCache elementCache,
            SaveHandler<T> saveCallback)
            where T: Excel2007WorkbookSerializationManager
		{
			foreach ( ExcelXmlNode childNode in node.ChildNodes )
			{
				ExcelXmlElement childElement = childNode as ExcelXmlElement;

				if ( childElement == null )
				{
					// MD 9/25/09 - TFS21642
					// Write out any nodes which aren't elements.
					childNode.WriteNode();

					continue;
				}

				// The shape serialization manager might have already populate the nodes. If that's the case, just skip this element.
				// MD 7/15/11 - Shape support
				// This was a poor test to see if the child element was pre-populated, because some pre-populated elements could also have no child nodes or 
				// attributes. So instead, check a flag that explicitly states whether it was pre-populated.
				//if ( childElement.Attributes.Count != 0 || childElement.ChildNodes.Count != 0 )
				if (childElement.IsPrePopulated)
				{
					// MD 9/25/09 - TFS21642
					// Write out any nodes which are already populated
					// MD 7/15/11 - Shape support
					// Write node recursive does not let us get back into the normal saving logic for non pre-populated descendants of a pre-populated parent,
					// so call WriteNode instead, which does.
                    //XmlElementBase.WriteNodeRecursive( childElement );
					XmlElementBase.WriteNode(manager, elementCache, saveCallback, childElement);

					continue;
				}

				string qualifiedName = XmlElementBase.GetQualifiedElementName( childElement );
				XmlElementBase elementHandler = XmlElementBase.GetElement( qualifiedName );

				if ( elementHandler == null )
					continue;

				// MD 11/5/10 - TFS49093
				// Added a way to save elements directly instead of with the normal recursive infrastructure.
				// If the element is saved directly, skip the rest of the logic for this element.
				if (elementHandler.SaveDirect(manager, node.OwnerDocument))
					continue;

				int oldContextStackCount = manager.ContextStack.Count;

				string value = null;

                //  BF 10/7/10  NA 2011.1 - Infragistics.Word
                //  Does the same thing, but through a generic handler so
                //  that Word can use the same logic.
				//elementHandler.Save( manager, childElement, ref value );
                saveCallback(elementHandler, manager, childElement, ref value);

				if ( value != null )
				{
					XmlElementBase.AddValueNode( childElement, value );

					// MD 9/25/09 - TFS21642
					// We are now going to write out elements as we go so we don't have to store them in memory.

					// MD 7/15/11 - Shape support
					// WriteNodeRecursive was insufficient for other purposes, so it was replaced with WriteNode.
					//XmlElementBase.WriteNodeRecursive( childElement );
					XmlElementBase.WriteNode(manager, elementCache, saveCallback, childElement);
				}
				else
				{
					// MD 7/15/11 - Shape support
					// Moved this code to WriteNode to be used in other places.
					#region Moved

					//// MD 9/25/09 - TFS21642
					//// We are now going to write out elements as we go so we don't have to store them in memory.
					//childElement.WriteStart();
					//
					//// MD 9/25/09
					//// Found while fixing TFS21642
					//// Recursively call the new overload.
					//
					////  BF 10/7/10  NA 2011.1 - Infragistics.Word
					////  Does the same thing, but through a generic handler so
					////  that Word can use the same logic.
					////XmlElementBase.SaveChildElements( manager, childElement );
					//XmlElementBase.SaveChildElementsHelper( manager, childElement, elementCache, saveCallback );
					//
					//// MD 9/25/09 - TFS21642
					//// We are now going to write out elements as we go so we don't have to store them in memory.
					//childElement.WriteEnd();

					#endregion // Moved
					XmlElementBase.WriteNode(manager, elementCache, saveCallback, childElement);
				}

				// Pop off all contexts which were added to the stack when processing this node
				manager.ContextStack.ClearToCount( oldContextStackCount );
			}

			// MD 9/25/09 - TFS21642
			// Since we are writing nodes as we go, we can clear the child collection so we don't keep these items 
			// in memory unnecessarily.
			node.ChildNodes.Clear();
		}

        #endregion SaveChildElements

		#endregion Public Methods

		#region Protected Methods

		#region AddAttribute



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		internal protected static void AddAttribute( ExcelXmlElement element, string localOrQualifiedName, string value )
		{
			string namespaceName;
			string localName;
			string prefix;
			XmlElementBase.ParseQualifiedName( element, localOrQualifiedName, false,
				out prefix, out localName, out namespaceName );

			XmlElementBase.AddAttribute(
				element,
				prefix,
				localName,
				namespaceName,
				value );
		}

		#endregion AddAttribute

		#region AddElement



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal protected static ExcelXmlElement AddElement( ExcelXmlElement element, string qualifiedName )
		{
			string namespaceName;
			string localName;
			string prefix;
			XmlElementBase.ParseQualifiedName( element, qualifiedName, true,
				out prefix, out localName, out namespaceName );

			// MD 11/1/10 - TFS56976
			// Moved this logic to another overload of AddElement
			//ExcelXmlElement childElement = element.OwnerDocument.CreateElement( prefix, localName, namespaceName );
			//return element.AppendChild( childElement ) as ExcelXmlElement;
			return XmlElementBase.AddElement(element, namespaceName, localName, prefix);
		}

		// MD 11/1/10 - TFS56976
		// Added an overload so we don't have to parse the fully qualified name if we know what the local name and prefix should already be.
		internal protected static ExcelXmlElement AddElement(ExcelXmlElement element, string namespaceName, string localName, string prefix)
		{
			ExcelXmlElement childElement = element.OwnerDocument.CreateElement(prefix, localName, namespaceName);
			return element.AppendChild(childElement) as ExcelXmlElement;
		}

		#endregion AddElement

		#region AddElements



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected static void AddElements( ExcelXmlElement element, string qualifiedName, int elementCount )
		{
			// MD 11/3/10 - TFS49093
			// There's no need to parse through the qualifiedName each time here. Parse it once and call the new overload which takes the component parts.
			//for ( int i = 0; i < elementCount; i++ )
			//    XmlElementBase.AddElement( element, qualifiedName );
			string namespaceName;
			string localName;
			string prefix;
			XmlElementBase.ParseQualifiedName(element, qualifiedName, true,
				out prefix, out localName, out namespaceName);

			XmlElementBase.AddElements(element, namespaceName, localName, prefix, elementCount);
		}

		// MD 11/3/10 - TFS49093
		// Added a new overload which takes the component parts.
		protected static void AddElements(ExcelXmlElement element, string namespaceName, string localName, string prefix, int elementCount)
		{
			for (int i = 0; i < elementCount; i++)
				XmlElementBase.AddElement(element, namespaceName, localName, prefix);
		}

		#endregion AddElements

		#region AddNamespaceDeclaration



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected static void AddNamespaceDeclaration( ExcelXmlElement element, string namespacePrefix, string namespaceValue )
		{
			XmlElementBase.AddAttribute(
				element,
				XmlElementBase.NamespaceDeclarationPrefix,
				namespacePrefix,
				XmlElementBase.NamespaceDeclarationNamespace,
				namespaceValue );
		}

		#endregion AddNamespaceDeclaration

		// MD 10/12/10
		// Found while fixing TFS49853
		// This code was defined twice so it has been moved to a common location.
		#region AddRootShapeElement

		protected static ExcelXmlElement AddRootShapeElement(ExcelXmlElement element, WorksheetShape shape)
		{
			//  Add the element appropriate for the type of the current shape
			if (shape is WorksheetShapeGroup)
				return XmlElementBase.AddElement(element, GrpSpElement.QualifiedName);

			if (shape is WorksheetImage)
				return XmlElementBase.AddElement(element, PicElement.QualifiedName);

			// MD 10/12/10 - TFS49853
			if (shape is WorksheetChart)
				return XmlElementBase.AddElement(element, GraphicFrameElement.QualifiedName);

			// MD 7/15/11 - Shape support
			// We now support other shape types.
			////  UnknownShape
			//Debug.Assert(shape is UnknownShape, "Unhandled shape type: " + shape.GetType().Name);
			// MD 10/10/11 - TFS81451
			//if (shape.IsConnector)
			if (shape.UseCxnShapePropertiesElement)
				return XmlElementBase.AddElement(element, CxnSpElement.QualifiedName);

			return XmlElementBase.AddElement(element, SpElement.QualifiedName);
		}

		#endregion // AddRootShapeElement

		#region CacheElementValues

		protected static ElementDataCache CacheElementValues( XmlElementBase elementHandler, ExcelXmlElement element )
		{
			//  Get the list of element/attribute values that are consumed by the element
			//  handler, i.e., element/attribute values which correspond to publicly exposed
			//  properties of our object model.
			IConsumedElementValueProvider provider = elementHandler as IConsumedElementValueProvider;
			Dictionary<string, HandledAttributeIdentifier> consumedValues = provider != null ? provider.ConsumedValues : null;

			string elementValue = XmlElementBase.GetElementValue( element );

			// MD 8/31/11 - TFS85353
			// We will need to have an explicit prefix for the element if it's namespace and prefix are defined in one of the 
			// element's own attributes. So search for that sort of thing first.
			//string qualifiedName = XmlElementBase.GetQualifiedElementName( element );
			//ElementDataCache wrapper = new ElementDataCache( qualifiedName, elementValue );
			ElementDataCache wrapper = null;

			if (String.IsNullOrEmpty(element.Prefix) == false &&
				String.IsNullOrEmpty(element.NamespaceURI) == false)
			{
				string namespaceURI = element.NamespaceURI;
				ExcelXmlDocument document = element.OwnerDocument;
				string prefix = null;

				foreach (ExcelXmlAttribute attribute in element.Attributes)
				{
					if (attribute.Prefix.Length == 0)
					{
						if (attribute.LocalName == document.strXmlns && attribute.Value == namespaceURI)
						{
							prefix = string.Empty;
							break;
						}
					}
					else if (attribute.Prefix == document.strXmlns)
					{
						if (attribute.Value == namespaceURI)
						{
							prefix = attribute.LocalName;
							break;
						}
					}
				}

				if (prefix != null)
					wrapper = new ElementDataCache(element.NamespaceURI, element.LocalName, prefix, elementValue);
			}
			
			if (wrapper == null)
			{
				string qualifiedName = XmlElementBase.GetQualifiedElementName(element);
				wrapper = new ElementDataCache(qualifiedName, elementValue);
			}

			//  Add its attribute values.
			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				//  Normalize the name by replacing the prefix with the namespace
				//  URI, if there is a prefix.
				string attributeName = attribute.Name;
				if ( string.IsNullOrEmpty( attribute.Prefix ) == false )
				{
					attributeName = attributeName.Replace( ':', '/' );
					attributeName = attributeName.Replace( attribute.Prefix, attribute.NamespaceURI );
				}

				wrapper.AttributeValues.Add( attributeName, attribute.Value );
			}

			//  Add its consumedValues, i.e., the attributes which provide values
			//  that have relevance in our public object model.
			if ( consumedValues != null )
				wrapper.ConsumedValues = consumedValues;

			//  Return the newly created wrapper.
			return wrapper;
		}

		#endregion CacheElementValues

		#region GetAttributeValue



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		protected static object GetAttributeValue( ExcelXmlAttribute attribute, DataType dataType, object defaultValue )
		{
			// MD 12/30/11 - 12.1 - Table Support
			////  For the time being, assert if defaultValue is null; this might have to change.
			//if ( attribute == null || defaultValue == null )
			if (attribute == null && defaultValue == null)
			{
				Debug.Assert( false, "The value and default value passed to XmlElementBase.GetAttributeValue cannot both be null." );
				return null;
			}

			// MD 12/30/11 - 12.1 - Table Support
			if (defaultValue == null)
				defaultValue = string.Empty;

			return XmlElementBase.GetValue( attribute.Value, dataType, defaultValue );
		}

		#endregion GetAttributeValue

		#region GetQualifiedAttributeName






		protected static string GetQualifiedAttributeName( ExcelXmlAttribute attribute )
		{
			return XmlElementBase.CombineNamespaceAndName( attribute.NamespaceURI, attribute.LocalName );
		}

		#endregion GetQualifiedAttributeName

		#region GetQualifiedElementName






		public static string GetQualifiedElementName( ExcelXmlElement element )
		{
			return XmlElementBase.CombineNamespaceAndName( element.NamespaceURI, element.LocalName );
		}

		#endregion GetQualifiedElementName

		#region GetValue



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		protected static object GetValue( string value, DataType dataType, object defaultValue )
		{
			//  For the time being, assert if defaultValue is null; this might have to change.
			if ( defaultValue == null )
			{
				Debug.Assert( false, "The defaultValue is null in GetValue" );
				return null;
			}

			//  Return defaultValue if the attribute value is null or empty.
			if ( string.IsNullOrEmpty( value ) )
				return defaultValue;

			object retVal = null;

			// MD 1/9/09 - TFS12270
			// If we leave this at null, the current culture will be used as the format provider. This is bad because the culture specific decimal separator
			// will be used for numbers, not to mention other culture specific text stuff being used. Excel expects everything to be in the same format, no 
			// matter what machine the workbook is saved or loaded on. I believe that format is locale 1033 and this is now stored as a static variable on 
			// the Workbook.
			//IFormatProvider formatProvider = null;
			IFormatProvider formatProvider = Workbook.InvariantFormatProvider;

			try
			{
				switch ( dataType )
				{
					//  string
					case DataType.String:
						{
							retVal = value;
							break;
						}

					//  int
					case DataType.Integer:
						{
							retVal = int.Parse( value, formatProvider );
							break;
						}

					//  bool
					case DataType.Boolean:
						{
							// Attribute values could use "0" and "1" instead of "true" and "false", so don't assume the parse will work.
							bool parseResult;
							if ( bool.TryParse( value, out parseResult ) )
								retVal = parseResult;
							else
								retVal = Convert.ToBoolean( XmlElementBase.GetValue( value, DataType.Int32, Convert.ToInt32( defaultValue ) ) );


							break;
						}

					//  DateTime
					case DataType.DateTime:
						{
							retVal = DateTime.Parse( value, formatProvider );
							break;
						}

					//  float
					case DataType.Float:
						{
							retVal = float.Parse( value, formatProvider );
							break;
						}

					//  double
					case DataType.Double:
						{
							retVal = double.Parse( value, formatProvider );
							break;
						}

					//  long
					case DataType.Long:
						{
							retVal = long.Parse( value, formatProvider );
							break;
						}

					//  short
					case DataType.Short:
						{
							retVal = short.Parse( value, formatProvider );
							break;
						}

					//  ushort
					case DataType.UShort:
						{
							retVal = ushort.Parse( value, formatProvider );
							break;
						}

					//  uint
					case DataType.UInt:
						{
							retVal = uint.Parse( value, formatProvider );
							break;
						}

					//  ulong
					case DataType.ULong:
						{
							retVal = ulong.Parse( value, formatProvider );
							break;
						}

					//  byte
					case DataType.Byte:
						{
							retVal = byte.Parse( value, formatProvider );
							break;
						}

					//  sbyte
					case DataType.SByte:
						{
							retVal = sbyte.Parse( value, formatProvider );
							break;
						}

					//  decimal
					case DataType.Decimal:
						{
							retVal = decimal.Parse( value, formatProvider );
							break;
						}

					//  object
					case DataType.Object:
						{
							Utilities.DebugFail( "Don't know how to convert string to Object" );
							break;
						}

					//  ST_VectorBaseType
					case DataType.ST_VectorBaseType:
						{
							if ( value == "bool" )
								value = "_bool";

							retVal = Enum.Parse( typeof( ST_VectorBaseType ), value, false );
							break;
						}

					//  ST_Visibility
					case DataType.ST_Visibility:
						{
                            retVal = (WorksheetVisibility)Enum.Parse(typeof(ST_Visibility), value, false);
							break;
						}

					//  ST_CalcMode
					case DataType.ST_CalcMode:
						{
                            retVal = (CalculationMode)Enum.Parse(typeof(ST_CalcMode), value, false);
							break;
						}

					//  ST_CalcMode
					case DataType.ST_RefMode:
						{
                            retVal = (CellReferenceMode)Enum.Parse(typeof(ST_RefMode), value, false);
							break;
						}

                    //  ST_FontScheme
                    case DataType.ST_FontScheme:
                        {
                            retVal = Enum.Parse(typeof(ST_FontScheme), value, false);
                            break;
                        }

                    //  ST_UnderlineValues
                    case DataType.ST_UnderlineValues:
                        {
                            if (value == "double")
                                value = "_double";
                            else if (value == "single")
                                value = "_single";

                            retVal = (FontUnderlineStyle)Enum.Parse(typeof(ST_UnderlineValues), value, false);
                            break;
                        }

                    //  ST_VerticalAlignment
                    case DataType.ST_VerticalAlignRun:
                        {
                            retVal = (FontSuperscriptSubscriptStyle)Enum.Parse(typeof(ST_VerticalAlignRun), value, false);
                            break;
                        }

                    //  ST_BorderStyle
                    case DataType.ST_BorderStyle:
                        {
                            if (value == "double")
                                value = "_double";

                            retVal = (CellBorderLineStyle)Enum.Parse(typeof(ST_BorderStyle), value, true);
                            break;
                        }
                    case DataType.ST_SheetViewType:
                        {
                            retVal = (WorksheetView)Enum.Parse(typeof(ST_SheetViewType), value, false);
                            break;
                        }
                    //  ST_GradientType
                    case DataType.ST_GradientType:
                        {
                            retVal = Enum.Parse(typeof(ST_GradientType), value, false);
                            break;
                        }
                    //  ST_PatternType
                    case DataType.ST_PatternType:
                        {
                            retVal = (FillPatternStyle)Enum.Parse(typeof(ST_PatternType), value, false);
                            break;
                        }

                    case DataType.ST_Objects:
                        {
                            retVal = (ObjectDisplayStyle)Enum.Parse(typeof(ST_Objects), value, false);
                            break;
                        }

                    case DataType.ST_Links:
                        {
                            retVal = (ST_Links)Enum.Parse(typeof(ST_Links), value, true);
                            break;
                        }

                    case DataType.ST_HorizontalAlignment:
                        {
                            retVal = (HorizontalCellAlignment)Enum.Parse(typeof(ST_HorizontalAlignment), value, true);
                            break;
                        }

                    case DataType.ST_VerticalAlignment:
                        {
                            retVal = (VerticalCellAlignment)Enum.Parse(typeof(ST_VerticalAlignment), value, true);
                            break;
                        }
                    case DataType.ST_CellType:
                        {
                            retVal = (ST_CellType)Enum.Parse(typeof(ST_CellType), value, false);
                            break;
                        }

                    case DataType.ST_Orientation:
                        {
                            if (value == "default")
                                value = "_default";

                            retVal = (Orientation)Enum.Parse(typeof(ST_Orientation), value, false);
                            break;
                        }

                    case DataType.ST_PageOrder:
                        {
                            retVal = (PageOrder)Enum.Parse(typeof(ST_PageOrder), value, false);
                            break;
                        }

                    case DataType.ST_CellComments:
                        {
                            retVal = (PrintNotes)Enum.Parse(typeof(ST_CellComments), value, false);
                            break;
                        }

                    case DataType.ST_PrintError:
                        {
                            retVal = (PrintErrors)Enum.Parse(typeof(ST_PrintError), value, false);
                            break;
                        }

                    case DataType.ST_PaneState:
                        {
                            retVal = (ST_PaneState)Enum.Parse(typeof(ST_PaneState), value, false);
                            break;
                        }

                    case DataType.ST_Guid:
                        {
                            string pattern = XmlElementBase.GuidRegexPattern;
                            if ( System.Text.RegularExpressions.Regex.IsMatch(value, pattern) == false )
                            {
                                Debug.Assert(false, "The ST_Guid value does not conform to the standard ECMA regex pattern for GUIDs; Guid.Empty will be returned." );
                                retVal = Guid.Empty;
                            }
                            else
                                retVal = new Guid(value);

                            break;
                        }

                    case DataType.ST_Comments:
                        {
                            retVal = (CommentDisplayStyle)Enum.Parse(typeof(ST_Comments), value, false);
                            break;
                        }

                    case DataType.ST_SystemColorVal:
                        {
                            if (value == "3dDarkShadow")
                                value = "_3dDarkShadow";
                            else if (value == "3dLight")
                                value = "_3dLight";

                            retVal = (ST_SystemColorVal)Enum.Parse(typeof(ST_SystemColorVal), value, false);
                            break;
                        }

                    case DataType.ST_CellFormulaType:
                        {
                            retVal = (ST_CellFormulaType)Enum.Parse(typeof(ST_CellFormulaType), value, false);
                            break;
                        }

                    //  BF 8/14/08
                    case DataType.ST_UnsignedIntHex:
                        {
                            retVal = Utilities.FromUnsignedIntHex( value );
                            break;
                        }

                    //  BF 8/18/08
					case DataType.ST_EditAs:
						{
                            retVal = (ShapePositioningMode)Enum.Parse(typeof(ST_EditAs), value, false);
							break;
						}

                    //  BF 8/19/08
					case DataType.ST_DrawingElementId:
						{
							retVal = uint.Parse( value );
							break;
						}

                    //  BF 8/20/08
					case DataType.ST_Coordinate:
					case DataType.ST_PositiveCoordinate:
						{
							retVal = long.Parse( value );
							break;
						}

					// MD 2/1/11 - Data Validation support
					case DataType.ST_DataValidationErrorStyle:
						{
							retVal = (ST_DataValidationErrorStyle)Enum.Parse(typeof(ST_DataValidationErrorStyle), value, false);
							break;
						}

					// MD 2/1/11 - Data Validation support
					case DataType.ST_DataValidationOperator:
						{
							retVal = (ST_DataValidationOperator)Enum.Parse(typeof(ST_DataValidationOperator), value, false);
							break;
						}

					// MD 2/1/11 - Data Validation support
					case DataType.ST_DataValidationType:
						{
							if (value == "decimal")
								value = "_decimal";

							retVal = (ST_DataValidationType)Enum.Parse(typeof(ST_DataValidationType), value, false);
							break;
						}

					// MD 7/15/11 - Shape support
					case DataType.ST_ShapeType:
						{
							retVal = (ST_ShapeType)Enum.Parse(typeof(ST_ShapeType), value, false);
							break;
						}

					// MD 8/23/11 - TFS84306
					case DataType.ST_HexBinary3:
						{
							retVal = Int32.Parse(value, NumberStyles.HexNumber);
							break;
						}

					// MD 11/8/11 - TFS85193
					case DataType.ST_Percentage:
						{
							int thousanthsOfAPercent = Int32.Parse(value);
							retVal = thousanthsOfAPercent / 100000d;
							break;
						}

					// MD 11/8/11 - TFS85193
					case DataType.ST_TextAlignType:
						{
							ST_TextAlignType val;
							if (Utilities.EnumTryParse<ST_TextAlignType>(value, out val) == false)
							{
								Utilities.DebugFail("Unknown ST_TextAlignType value: " + val);
								val = ST_TextAlignType.l;
							}
							retVal = val;
							break;
						}

					// MD 11/8/11 - TFS85193
					case DataType.ST_TextAnchoringType:
						{
							ST_TextAnchoringType val;
							if (Utilities.EnumTryParse<ST_TextAnchoringType>(value, out val) == false)
							{
								Utilities.DebugFail("Unknown ST_TextAnchoringType value: " + val);
								val = ST_TextAnchoringType.t;
							}
							retVal = val;
							break;
						}

					// MD 12/6/11 - 12.1 - Table Support
					case DataType.ST_TableType:
						retVal = (ST_TableType)Enum.Parse(typeof(ST_TableType), value, false);
						break;

					// MD 12/6/11 - 12.1 - Table Support
					case DataType.ST_TotalsRowFunction:
						retVal = (ST_TotalsRowFunction)Enum.Parse(typeof(ST_TotalsRowFunction), value, false);
						break;

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_SortMethod:
						retVal = (ST_SortMethod)Enum.Parse(typeof(ST_SortMethod), value, false);
						break;

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_SortBy:
						retVal = (ST_SortBy)Enum.Parse(typeof(ST_SortBy), value, false);
						break;

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_IconSetType:
						retVal = (ST_IconSetType)Enum.Parse(typeof(ST_IconSetType), "_" + value, false);
						break;

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_CalendarType:
						retVal = (ST_CalendarType)Enum.Parse(typeof(ST_CalendarType), value, false);
						break;

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_DateTimeGrouping:
						retVal = (ST_DateTimeGrouping)Enum.Parse(typeof(ST_DateTimeGrouping), value, false);
						break;

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_FilterOperator:
						retVal = (ST_FilterOperator)Enum.Parse(typeof(ST_FilterOperator), value, false);
						break;

					// MD 12/11/11 - 12.1 - Table Support
					case DataType.ST_TableStyleType:
						retVal = (ST_TableStyleType)Enum.Parse(typeof(ST_TableStyleType), value, false);
						break;

					// MD 12/11/11 - 12.1 - Table Support
					case DataType.ST_DynamicFilterType:
						{
							if (value == "null")
								value = "_null";

							retVal = Enum.Parse(typeof(ST_DynamicFilterType), value, false);
							break;
						}

					// MD 1/17/12 - 12.1 - Cell Format Updates
					case DataType.ST_SchemeColorVal:
						retVal = (ST_SchemeColorVal)Enum.Parse(typeof(ST_SchemeColorVal), value, false);
						break;

					// MD 7/3/12 - TFS115689
					// Added round trip support for line end properties.
					case DataType.ST_LineEndLength:
						retVal = (ST_LineEndLength)Enum.Parse(typeof(ST_LineEndLength), value, false);
						break;

					case DataType.ST_LineEndType:
						retVal = (ST_LineEndType)Enum.Parse(typeof(ST_LineEndType), value, false);
						break;

					case DataType.ST_LineEndWidth:
						retVal = (ST_LineEndWidth)Enum.Parse(typeof(ST_LineEndWidth), value, false);
						break;

					default:
						{
							Debug.Assert( false, string.Format( "Unhandled data type '{0}' specified in XmlElementBase.GetAttributeValue.", dataType ) );
							retVal = defaultValue;
						}
						break;
				}
			}
			catch ( Exception ex )
			{
				Debug.Assert( false, string.Format( "Exception thrown trying to parse value '{0}' in XmlElementBase.GetAttributeValue ({1})", value, ex.Message ) );
				retVal = defaultValue;
			}

			return retVal;

		}

		#endregion GetValue

		#region GetXmlString



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal protected static string GetXmlString( object value, DataType dataType )
		{
			string returnedValue = XmlElementBase.GetXmlString( value, dataType, null, true );

			// MD 12/30/11 - 12.1 - Table Support
			// This is slightly faster.
			//return returnedValue != null ? returnedValue : string.Empty;
			return returnedValue ?? string.Empty;
		}



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		protected static string GetXmlString( object value, DataType dataType, object defaultValue, bool valueIsRequired )
		{

			//  if the value is the defaultValue or the value is not provided and the value is not required, nothing should be written to the xml
			if ( valueIsRequired == false &&
				( value == null || Object.Equals( value, defaultValue ) ) )
				return null;

			// if no value was provided, use the default value
			if ( value == null )
			{
				if ( defaultValue == null )
				{
					Utilities.DebugFail( "If the value is required, the value and the default value cannot be null." );
					return null;
				}

				value = defaultValue;
			}

			// MD 1/28/09 - TFS12701
			Debug.Assert( defaultValue == null || value.GetType() == defaultValue.GetType(), "The default value is not of the same type as the value." );

			// MD 1/9/09 - TFS12270
			// If we leave this at null, the current culture will be used as the format provider. This is bad because the culture specific decimal separator
			// will be used for numbers, not to mention other culture specific text stuff being used. Excel expects everthing to be in the same format, no 
			// matter what machine the workbook is saved or loaded on. I believe that format is locale 1033 and this is now stored as a static veriable on 
			// the Workboook.
			//IFormatProvider formatProvider = null;
			IFormatProvider formatProvider = Workbook.InvariantFormatProvider;

            string retVal = String.Empty;

			try
			{
				switch ( dataType )
				{
					//  string
					case DataType.String:
						return (string)value;

					//  int
					case DataType.Integer:
						return ( (int)value ).ToString( formatProvider );
                    
					//  bool
					case DataType.Boolean:
                        // Excel stores booleans as a '0' or '1' and not 'false' or 'true'
                        bool bValue = (bool)value;
                        return bValue ? "1" : "0";

					//  DateTime
					case DataType.DateTime:
						
						return ( (DateTime)value ).ToString( formatProvider );

					//  float
					case DataType.Float:
						return ( (float)value ).ToString( formatProvider );

					//  double
					case DataType.Double:
						// MD 3/7/12 - 12.1 - Table Support
						// We don't want to lose any precision, so use the "R" format string.
						//return ( (double)value ).ToString( formatProvider );
						return ((double)value).ToString("R", formatProvider);

					//  long
					case DataType.Long:
						return ( (long)value ).ToString( formatProvider );

					//  short
					case DataType.Short:
						return ( (short)value ).ToString( formatProvider );

					//  ushort
					case DataType.UShort:
						return ( (ushort)value ).ToString( formatProvider );

					//  uint
					case DataType.UInt:
                        return (Convert.ToUInt32(value, formatProvider).ToString(formatProvider));

					//  ulong
					case DataType.ULong:
						return ( (ulong)value ).ToString( formatProvider );

					//  byte
					case DataType.Byte:
						return ( (byte)value ).ToString( formatProvider );

					//  sbyte
					case DataType.SByte:
						return ( (sbyte)value ).ToString( formatProvider );

					//  decimal
					case DataType.Decimal:
						return ( (decimal)value ).ToString( formatProvider );

					//  object
					case DataType.Object:
						return value.ToString();

					//  ST_VectorBaseType
					case DataType.ST_VectorBaseType:
						{
							Debug.Assert( Enum.IsDefined( typeof( ST_VectorBaseType ), (ST_VectorBaseType)value ), "The value is not defined." );
							string retValue = ( (ST_VectorBaseType)value ).ToString();

							if ( retValue.StartsWith( "_" ) )
								retValue = retValue.Substring( 1 );

							return retValue;
						}

					//  ST_Visibility/ST_SheetState
					case DataType.ST_Visibility:
						Debug.Assert( Enum.IsDefined( typeof( ST_Visibility ), (ST_Visibility)(WorksheetVisibility)value ), "The value is not defined." );
						return ( (ST_Visibility)(WorksheetVisibility)value ).ToString();

					//  ST_CalcMode
					case DataType.ST_CalcMode:
						Debug.Assert( Enum.IsDefined( typeof( ST_CalcMode ), (ST_CalcMode)(CalculationMode)value ), "The value is not defined." );
						return ( (ST_CalcMode)(CalculationMode)value ).ToString();

					//  ST_CalcMode
					case DataType.ST_RefMode:
						Debug.Assert( Enum.IsDefined( typeof( ST_RefMode ), (ST_RefMode)(CellReferenceMode)value ), "The value is not defined." );
						return ( (ST_RefMode)(CellReferenceMode)value ).ToString();

                    //  ST_FontScheme
                    case DataType.ST_FontScheme:
						Debug.Assert( Enum.IsDefined( typeof( ST_FontScheme ), (ST_FontScheme)value ), "The value is not defined." );
                        return ((ST_FontScheme)value).ToString();

                    //  ST_UnderlineValues
                    case DataType.ST_UnderlineValues:
                        FontUnderlineStyle style = (FontUnderlineStyle)value;

                        // There is no concept of "Default" in Excel, so we should
                        // resolve our default
                        if (style == FontUnderlineStyle.Default)
                            style = FontUnderlineStyle.None;

						Debug.Assert( Enum.IsDefined( typeof( ST_UnderlineValues ), (ST_UnderlineValues)value ), "The value is not defined." );

                        retVal = ((ST_UnderlineValues)style).ToString();
                        if (retVal.StartsWith("_"))
                            retVal = retVal.Substring(1);

                        return retVal;

                    //  ST_VerticalAlignment
                    case DataType.ST_VerticalAlignRun:
						Debug.Assert( Enum.IsDefined( typeof( ST_VerticalAlignRun ), (ST_VerticalAlignRun)(FontSuperscriptSubscriptStyle)value ), "The value is not defined." );
                        return ((ST_VerticalAlignRun)(FontSuperscriptSubscriptStyle)value).ToString();

                    //  ST_BorderStyle
                    case DataType.ST_BorderStyle:
                        CellBorderLineStyle borderStyle = (CellBorderLineStyle)value;

                        // There is no concept of "Default" in Excel, so we should
                        // use Excel's default
                        if (borderStyle == CellBorderLineStyle.Default)
                            borderStyle = CellBorderLineStyle.None;

						Debug.Assert( Enum.IsDefined( typeof( ST_BorderStyle ), (ST_BorderStyle)value ), "The value is not defined." );

                        retVal = ((ST_BorderStyle)borderStyle).ToString();
                        if (retVal.StartsWith("_"))
                            retVal = retVal.Substring(1);

                        return retVal;

                    case DataType.ST_SheetViewType:
						Debug.Assert( Enum.IsDefined( typeof( ST_SheetViewType ), (ST_SheetViewType)(WorksheetView)value ), "The value is not defined." );
                        return ((ST_SheetViewType)(WorksheetView)value).ToString();

                    case DataType.ST_Pane:
						Debug.Assert( Enum.IsDefined( typeof( ST_Pane ), (ST_Pane)value ), "The value is not defined." );
                        return ((ST_Pane)value).ToString();
                    //  ST_GradientType
                    case DataType.ST_GradientType:
						Debug.Assert( Enum.IsDefined( typeof( ST_GradientType ), (ST_GradientType)value ), "The value is not defined." );
                        return ((ST_GradientType)value).ToString();
                    //  ST_BorderStyle
                    case DataType.ST_PatternType:
						Debug.Assert( Enum.IsDefined( typeof( ST_PatternType ), (ST_PatternType)(FillPatternStyle)value ), "The value is not defined." );
                        return ((ST_PatternType)(FillPatternStyle)value).ToString();

                    case DataType.ST_Objects:
						Debug.Assert( Enum.IsDefined( typeof( ST_Objects ), (ST_Objects)(ObjectDisplayStyle)value ), "The value is not defined." );
                        return ((ST_Objects)(ObjectDisplayStyle)value).ToString();

                    case DataType.ST_Links:
						Debug.Assert( Enum.IsDefined( typeof( ST_Links ), (ST_Links)value ), "The value is not defined." );
                        return ((ST_Links)value).ToString();
                    case DataType.ST_HorizontalAlignment:
						Debug.Assert( Enum.IsDefined( typeof( ST_HorizontalAlignment ), (ST_HorizontalAlignment)(HorizontalCellAlignment)value ), "The value is not defined." );
                        return ((ST_HorizontalAlignment)(HorizontalCellAlignment)value).ToString();
                    case DataType.ST_VerticalAlignment:
						Debug.Assert( Enum.IsDefined( typeof( ST_VerticalAlignment ), (ST_VerticalAlignment)(VerticalCellAlignment)value ), "The value is not defined." );
                        return ((ST_VerticalAlignment)(VerticalCellAlignment)value).ToString();

                    case DataType.ST_CellType:
						// MD 10/21/10 - TFS34398
						// Since this is a common case, implement it manually. It seems to be faster than the ToString() call.
						//Debug.Assert( Enum.IsDefined( typeof( ST_CellType ), (ST_CellType)value ), "The value is not defined." );
						//return ((ST_CellType)value).ToString();
						switch ((ST_CellType)value)
						{
							case ST_CellType.b:
								return "b";
							case ST_CellType.e:
								return "e";
							case ST_CellType.inlineStr:
								return "inlineStr";
							case ST_CellType.n:
								return "n";
							case ST_CellType.s:
								return "s";
							case ST_CellType.str:
								return "str";
							default:
								Utilities.DebugFail("The value is not defined.");
								return ((ST_CellType)value).ToString();
						}

                    case DataType.ST_Orientation:
						Debug.Assert( Enum.IsDefined( typeof( ST_Orientation ), (ST_Orientation)value ), "The value is not defined." );
                        retVal = ((ST_Orientation)value).ToString();
                        if (retVal.StartsWith("_"))
                            retVal = retVal.Substring(1);

                        return retVal;

                    case DataType.ST_PageOrder:
						Debug.Assert( Enum.IsDefined( typeof( ST_PageOrder ), (ST_PageOrder)(PageOrder)value ), "The value is not defined." );
                        return ((ST_PageOrder)(PageOrder)value).ToString();

                    case DataType.ST_CellComments:
						Debug.Assert( Enum.IsDefined( typeof( ST_CellComments ), (ST_CellComments)(PrintNotes)value ), "The value is not defined." );
                        return ((ST_CellComments)(PrintNotes)value).ToString();

                    case DataType.ST_PrintError:
						Debug.Assert( Enum.IsDefined( typeof( ST_PrintError ), (ST_PrintError)(PrintErrors)value ), "The value is not defined." );
                        return ((ST_PrintError)(PrintErrors)value).ToString();

                    case DataType.ST_PaneState:
						Debug.Assert( Enum.IsDefined( typeof( ST_PaneState ), (ST_PaneState)value ), "The value is not defined." );
                        return ((ST_PaneState)value).ToString();

					case DataType.ST_Comments:
						Debug.Assert( Enum.IsDefined( typeof( ST_Comments ), (ST_Comments)(CommentDisplayStyle)value ), "The value is not defined." );
						return ( (ST_Comments)(CommentDisplayStyle)value ).ToString();

                    case DataType.ST_CellFormulaType:
						Debug.Assert( Enum.IsDefined( typeof( ST_CellFormulaType ), (ST_CellFormulaType)value ), "The value is not defined." );
                        return ((ST_CellFormulaType)value).ToString();

					case DataType.ST_Guid:
                        //  BF 8/12/08
                        //  Note that Excel will choke on the GUID if it isn't enclosed
                        //  with curly braces, which is what the "B" format does. It also
                        //  wants it in uppercase.
                        Guid guid = (Guid)value;
						return guid.ToString("B").ToUpper();

                    //  BF 8/14/08
					case DataType.ST_UnsignedIntHex:
                        if ( (value is int) == false &&
                             (value is Color) == false )
                        {
                            Utilities.DebugFail( "Could not convert the specified type to ST_UnsignedIntHex." );
                            return Utilities.ToUnsignedIntHex(0);
                        }    

                        int unsignedIntHexValue = Utilities.ToInteger( value );
						return Utilities.ToUnsignedIntHex( unsignedIntHexValue );

                    case DataType.ST_SystemColorVal:
						Debug.Assert( Enum.IsDefined( typeof( ST_SystemColorVal ), (ST_SystemColorVal)value ), "The value is not defined." );
                        retVal = ((ST_SystemColorVal)value).ToString();
                        if (retVal.StartsWith("_"))
                            retVal = retVal.Substring(1);

                        return retVal;

                    //  BF 8/18/08
					case DataType.ST_EditAs:
						Debug.Assert( Enum.IsDefined( typeof( ST_EditAs ), (ST_EditAs)(ShapePositioningMode)value ), "The value is not defined." );
						return ( (ST_EditAs)(ShapePositioningMode)value ).ToString();

                    //  BF 8/19/08
					case DataType.ST_DrawingElementId:
						return value.ToString();

                    //  BF 8/20/08
					case DataType.ST_Coordinate:
					case DataType.ST_PositiveCoordinate:
                        return value.ToString();

					// MD 2/1/11 - Data Validation support
					case DataType.ST_DataValidationErrorStyle:
						Debug.Assert(Enum.IsDefined(typeof(ST_DataValidationErrorStyle), (ST_DataValidationErrorStyle)(DataValidationErrorStyle)value), "The value is not defined.");
						return ((ST_DataValidationErrorStyle)(DataValidationErrorStyle)value).ToString();

					// MD 2/1/11 - Data Validation support
					case DataType.ST_DataValidationOperator:
						Debug.Assert(Enum.IsDefined(typeof(ST_DataValidationOperator), (ST_DataValidationOperator)(DataValidationOperatorType)value), "The value is not defined.");
						return ((ST_DataValidationOperator)(DataValidationOperatorType)value).ToString();

					// MD 2/1/11 - Data Validation support
					case DataType.ST_DataValidationType:
						Debug.Assert(Enum.IsDefined(typeof(ST_DataValidationType), (ST_DataValidationType)(DataValidationType)value), "The value is not defined.");
						retVal = ((ST_DataValidationType)(DataValidationType)value).ToString();

                        if (retVal.StartsWith("_"))
                            retVal = retVal.Substring(1);

						return retVal;

					// MD 7/15/11 - Shape support
					case DataType.ST_ShapeType:
						Debug.Assert(Enum.IsDefined(typeof(ST_ShapeType), value), "The value is not defined.");
						return value.ToString();

					// MD 8/23/11 - TFS84306
					case DataType.ST_HexBinary3:
						{
							if (value is int)
							{
								int intValue = (int)value;
								string hexValue = String.Format("{0:X8}", intValue);
								return hexValue.Substring(Math.Max(0, hexValue.Length - 6));
							}

							Utilities.DebugFail("Could not convert the specified type to ST_HexBinary3.");
						}
						return null;

					// MD 11/8/11 - TFS85193
					case DataType.ST_Percentage:
						{
							int thousanthsOfAPercent = (int)Math.Round((double)value * 100000);
							return thousanthsOfAPercent.ToString();
						}

					// MD 11/8/11 - TFS85193
					case DataType.ST_TextAlignType:
						if (Enum.IsDefined(typeof(ST_TextAlignType), (ST_TextAlignType)(HorizontalTextAlignment)value) == false)
						{
							Utilities.DebugFail("The value is not defined.");
							return ST_TextAlignType.l.ToString();
						}
						return ((ST_TextAlignType)(HorizontalTextAlignment)value).ToString();

						// MD 11/8/11 - TFS85193
					case DataType.ST_TextAnchoringType:
						if (Enum.IsDefined(typeof(ST_TextAnchoringType), (ST_TextAnchoringType)(VerticalTextAlignment)value) == false)
						{
							Utilities.DebugFail("The value is not defined.");
							return ST_TextAnchoringType.t.ToString();
						}
						return ((ST_TextAnchoringType)(VerticalTextAlignment)value).ToString();

					// MD 12/6/11 - 12.1 - Table Support
					case DataType.ST_TableType:
						Debug.Assert(Enum.IsDefined(typeof(ST_TableType), value), "The value is not defined.");
						return value.ToString();

					// MD 12/6/11 - 12.1 - Table Support
					case DataType.ST_TotalsRowFunction:
						Debug.Assert(Enum.IsDefined(typeof(ST_TotalsRowFunction), value), "The value is not defined.");
						return value.ToString();

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_SortMethod:
						Debug.Assert(Enum.IsDefined(typeof(ST_SortMethod), value), "The value is not defined.");
						return value.ToString();

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_SortBy:
						Debug.Assert(Enum.IsDefined(typeof(ST_SortBy), value), "The value is not defined.");
						return value.ToString();

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_IconSetType:
						Debug.Assert(Enum.IsDefined(typeof(ST_SortBy), value), "The value is not defined.");
						retVal = ((ST_SortBy)value).ToString();
                        if (retVal.StartsWith("_"))
                            retVal = retVal.Substring(1);
						return retVal;

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_CalendarType:
						Debug.Assert(Enum.IsDefined(typeof(ST_CalendarType), value), "The value is not defined.");
						return value.ToString();

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_DateTimeGrouping:
						Debug.Assert(Enum.IsDefined(typeof(ST_DateTimeGrouping), value), "The value is not defined.");
						return value.ToString();

					// MD 12/9/11 - 12.1 - Table Support
					case DataType.ST_FilterOperator:
						Debug.Assert(Enum.IsDefined(typeof(ST_FilterOperator), value), "The value is not defined.");
						return value.ToString();

					// MD 12/11/11 - 12.1 - Table Support
					case DataType.ST_TableStyleType:
						Debug.Assert(Enum.IsDefined(typeof(ST_TableStyleType), value), "The value is not defined.");
						return value.ToString();

					// MD 12/11/11 - 12.1 - Table Support
					case DataType.ST_DynamicFilterType:
						{
							Debug.Assert(Enum.IsDefined(typeof(ST_DynamicFilterType), (ST_DynamicFilterType)value), "The value is not defined.");
							string retValue = ((ST_DynamicFilterType)value).ToString();

							if (retValue.StartsWith("_"))
								retValue = retValue.Substring(1);

							return retValue;
						}

					// MD 1/17/12 - 12.1 - Cell Format Updates
					case DataType.ST_SchemeColorVal:
						Debug.Assert(Enum.IsDefined(typeof(ST_SchemeColorVal), value), "The value is not defined.");
						return value.ToString();

					// MD 7/3/12 - TFS115689
					// Added round trip support for line end properties.
					case DataType.ST_LineEndLength:
						Debug.Assert(Enum.IsDefined(typeof(ST_LineEndLength), value), "The value is not defined.");
						return value.ToString();

					case DataType.ST_LineEndType:
						Debug.Assert(Enum.IsDefined(typeof(ST_LineEndType), value), "The value is not defined.");
						return value.ToString();

					case DataType.ST_LineEndWidth:
						Debug.Assert(Enum.IsDefined(typeof(ST_LineEndWidth), value), "The value is not defined.");
						return value.ToString();


					default:
						Utilities.DebugFail( string.Format( "Unhandled data type '{0}' specified in XmlElementBase.GetXmlString.", dataType ) );
						return null;
				}
			}
			catch ( Exception ex )
			{
				Utilities.DebugFail( string.Format( "Exception thrown trying to parse value '{0}' in XmlElementBase.GetXmlString ({1})", value, ex.Message ) );
				return null;
			}
		}

		#endregion GetXmlString

		#region OnAfterLoadChildElements

		protected virtual void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache ) { }

		#endregion OnAfterLoadChildElements

		// MD 11/5/10 - TFS49093
		// Added a way to save elements directly instead of with the normal recursive infrastructure.
		#region SaveDirect

		protected virtual bool SaveDirect(Excel2007WorkbookSerializationManager manager, ExcelXmlDocument document)
		{
			return false;
		} 

		#endregion // SaveDirect

		// MD 11/5/10 - TFS49093
		#region WriteString

		protected static void WriteString(XmlWriter writer, string value)
		{
			if (value.Trim().Length != value.Length)
				writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");

			writer.WriteString(value);
		} 

		#endregion // WriteString

		#endregion Protected Methods

		#region Private Methods

		#region AddAttribute

		private static void AddAttribute( ExcelXmlElement element, string prefix, string localName, string namespaceName, string value )
		{
			// MD 2/1/11 - Data Validation support
			// We should automatically bail here if the value is null. This way, the caller doesn't need to check 
			// the returned value form GetXmlString. They can just pass it directly to this method.
			if (value == null)
				return;

			ExcelXmlAttribute attribute = element.OwnerDocument.CreateAttribute( prefix, localName, namespaceName );
			attribute.Value = value;
			element.Attributes.Add( attribute );
		} 

		#endregion AddAttribute

		#region CombineNamespaceAndName

		// MD 8/31/11 - TFS85353
		// Made this internal so it could be used in other classes.
		//private static string CombineNamespaceAndName( string namespaceName, string name )
		internal static string CombineNamespaceAndName(string namespaceName, string name)
		{
			if ( String.IsNullOrEmpty( namespaceName ) )
				return name;

			if ( namespaceName.EndsWith( XmlElementBase.NamespaceSeparator ) == false )
				namespaceName += XmlElementBase.NamespaceSeparator;

			return namespaceName + name;
		}

		#endregion CombineNamespaceAndName

		// MD 9/25/09 - TFS21642
		#region CreateNextNode

		// MD 12/21/11 - 12.1 - Table Support
		// Made internal so this could be used to load standalone XML files.
		//private static ExcelXmlNode CreateNextNode( ExcelXmlDocument document, XmlReader reader, out bool isReaderOnNextNode, out bool isEmptyElement )
		internal static ExcelXmlNode CreateNextNode(ExcelXmlDocument document, XmlReader reader, out bool isReaderOnNextNode, out bool isEmptyElement)
		{
			isReaderOnNextNode = false;
			isEmptyElement = false;

			switch ( reader.NodeType )
			{
				case XmlNodeType.Attribute:
					return XmlElementBase.CreateNextAttribute( document, reader );

				case XmlNodeType.Element:
					{
						isEmptyElement = reader.IsEmptyElement;

						ExcelXmlElement element = document.CreateElement( reader.Prefix, reader.LocalName, reader.NamespaceURI );

						if ( reader.MoveToFirstAttribute() )
						{
							ExcelXmlAttributeCollection attributes = element.Attributes;

							do
							{
								ExcelXmlAttribute attribute = XmlElementBase.CreateNextAttribute( document, reader );
								attributes.Add( attribute );
							}
							while ( reader.MoveToNextAttribute() );

							reader.MoveToElement();
						}

						if ( isEmptyElement == false && reader.Read() )
						{
							while ( reader.NodeType == XmlNodeType.Whitespace )
							{
								if (element.NamespaceURI == DrawingsPart.MainNamespace &&
									element.LocalName == TElement.LocalName)
								{
									// MD 2/16/12 - 12.1 - Table Support
									// Use the AppendChild method, which sets the parent node reference.
									//element.ChildNodes.Add(document.CreateSignificantWhitespace(reader.Value));
									element.AppendChild(document.CreateSignificantWhitespace(reader.Value));

									isReaderOnNextNode = true;
									reader.Read();
									return element;
								}
								
								reader.Read();
							}

							if ( reader.NodeType == XmlNodeType.SignificantWhitespace ||
								reader.NodeType == XmlNodeType.Text )
							{
								bool ignore;

								// MD 2/16/12 - 12.1 - Table Support
								// Use the AppendChild method, which sets the parent node reference.
								//element.ChildNodes.Add( XmlElementBase.CreateNextNode( document, reader, out isReaderOnNextNode, out ignore ) );
								element.AppendChild(XmlElementBase.CreateNextNode(document, reader, out isReaderOnNextNode, out ignore));

								if ( isReaderOnNextNode == false )
								{
									isReaderOnNextNode = true;
									reader.Read();
								}
							}
							else
							{
								isReaderOnNextNode = true;
							}
						}

						return element;
					}

				case XmlNodeType.EndElement:
					return null;

				case XmlNodeType.SignificantWhitespace:
					return document.CreateSignificantWhitespace( reader.Value );

				case XmlNodeType.Text:
					return document.CreateTextNode( reader.Value );

				case XmlNodeType.Whitespace:
					reader.Read();
					return XmlElementBase.CreateNextNode( document, reader, out isReaderOnNextNode, out isEmptyElement );

				case XmlNodeType.XmlDeclaration:
					{
						string version = null;
						string encoding = null;
						string standalone = null;

						while ( reader.MoveToNextAttribute() )
						{
							string name = reader.Name;

							if ( name == null )
								continue;

							if ( name == "version" )
								version = reader.Value;
							else if ( name == "encoding" )
								encoding = reader.Value;
							else if ( name == "standalone" )
								standalone = reader.Value;
						}

						if ( version == null )
						{

							using ( XmlTextReader subReader = new XmlTextReader( reader.Value, XmlNodeType.XmlDeclaration, null ) )



							{
								subReader.Read();

								if ( subReader.MoveToAttribute( "version" ) )
									version = subReader.Value;

								if ( subReader.MoveToAttribute( "encoding" ) )
									encoding = subReader.Value;

								if ( subReader.MoveToAttribute( "standalone" ) )
									standalone = subReader.Value;
							}
						}

						return document.CreateXmlDeclaration( version, encoding, standalone );
					}

				// MD 12/21/11 - 12.1 - Table Support
				case XmlNodeType.None:
					return null;

				default:
					Utilities.DebugFail( "Unknown node type: " + reader.NodeType );
					break;
			}

			return null;
		}

		#endregion CreateNextNode
		
		// MD 9/25/09 - TFS21642
		#region CreateNextAttribute

		private static ExcelXmlAttribute CreateNextAttribute( ExcelXmlDocument document, XmlReader reader )
		{
			ExcelXmlAttribute attribute = document.CreateAttribute( reader.Prefix, reader.LocalName, reader.NamespaceURI );

			while ( reader.ReadAttributeValue() )
			{
				switch ( reader.NodeType )
				{
					case XmlNodeType.Text:
						Debug.Assert( attribute.Value == null, "It is expected that the attribute will only have one attribute value." );
						attribute.Value = reader.Value;
						break;

					default:
						Utilities.DebugFail( "Unknown attribute node type: " + reader.NodeType );
						break;
				}
			}

			return attribute;
		}

		#endregion CreateNextAttribute

		#region ParseQualifiedName

		private static void ParseQualifiedName( ExcelXmlElement element, string qualifiedName, bool requiresNamespace,
			out string prefix, out string localName, out string namespaceName )
		{
			int lastSeparatorIndex = qualifiedName.LastIndexOf( XmlElementBase.NamespaceSeparator );

			if ( lastSeparatorIndex < 0 )
			{
				Debug.Assert( requiresNamespace == false, "A fully qualified name was not specified: " + qualifiedName );

				localName = qualifiedName;
				namespaceName = "";

				// MD 7/2/09 - TFS18634
				// If there is no namespace, there is no use in trying to find the prefix, so just return.
				prefix = string.Empty;
				return;
			}
			else
			{
				localName = qualifiedName.Substring( lastSeparatorIndex + 1 );
				namespaceName = qualifiedName.Substring( 0, lastSeparatorIndex );
			}

			prefix = element.GetPrefixOfNamespace( namespaceName );

			// For some reason, Excel terminates some XML namespaces with a '/' and others not,
			// so if we couldn't find an existing namespace prefix, we should try to see
			// if one was created with a '/' at the end of the namespace provided to us.
			if ( prefix == null || prefix.Length == 0 )
			{
				string tempNamespace = namespaceName + XmlElementBase.NamespaceSeparator;
				prefix = element.GetPrefixOfNamespace( tempNamespace );
				if ( prefix != null && prefix.Length > 0 )
					namespaceName = tempNamespace;
			}
		}

		#endregion ParseQualifiedName

		// MD 7/15/11 - Shape support
		#region WriteNode

		private static void WriteNode<T>(T manager, ElementDataCache elementCache, SaveHandler<T> saveCallback, ExcelXmlElement childElement)
			where T : Excel2007WorkbookSerializationManager
		{
			childElement.WriteStart();
			XmlElementBase.SaveChildElementsHelper(manager, childElement, elementCache, saveCallback);
			childElement.WriteEnd();
		}

		#endregion // WriteNode

		// MD 7/15/11 - Shape support
		// Removed because it was insufficient for some callers. It was replaced by WriteNode.
		#region Removed

		//#region WriteNodeRecursive
		//
		//private static void WriteNodeRecursive( ExcelXmlNode node )
		//{
		//    node.WriteStart();
		//
		//    foreach ( ExcelXmlNode childNode in node.ChildNodes )
		//    {
		//        WriteNodeRecursive( childNode );
		//    }
		//
		//    node.WriteEnd();
		//}
		//
		//#endregion WriteNodeRecursive

		#endregion // Removed

		#endregion Private Methods

		#endregion Methods

		#region Properties






		public abstract string ElementName { get; }

		protected virtual bool LoadChildNodes
		{
			get { return true; }
		}

		#endregion Properties

		#region Elements

		private static Dictionary<string, XmlElementBase> Elements
		{
			get
			{
				if ( XmlElementBase.elements == null )
					XmlElementBase.elements = new Dictionary<string, XmlElementBase>( StringComparer.InvariantCultureIgnoreCase );

				return XmlElementBase.elements;
			}
		}

		#endregion Elements

		#region GetElement

		public static XmlElementBase GetElement( string elementName )
        {
            return XmlElementBase.GetElement( elementName, true );
        }

		public static XmlElementBase GetElement( string elementName, bool assertIfNotHandled )
		{
			XmlElementBase element;

			// MD 10/20/10 - TFS36617
			// Cached the most commonly used element handlers separately so we don't have to do a lookup in the 
			// main dictionary for each element we encounter of these types.
			switch (elementName)
			{
				case CellElement.QualifiedName:
					if (XmlElementBase.cellElement == null)
						XmlElementBase.cellElement = new CellElement();

					return XmlElementBase.cellElement;

				case CellValueElement.QualifiedName:
					if (XmlElementBase.cellValueElement == null)
						XmlElementBase.cellValueElement = new CellValueElement();

					return XmlElementBase.cellValueElement;

				case RowElement.QualifiedName:
					if (XmlElementBase.rowElement == null)
						XmlElementBase.rowElement = new RowElement();

					return XmlElementBase.rowElement;

				// MD 11/3/10 - TFS49093
				// Added some more common element types.
				case StringItemElement.QualifiedName:
					if(XmlElementBase.stringItemElement == null)
						XmlElementBase.stringItemElement = new StringItemElement();

					return XmlElementBase.stringItemElement;

				case TextElement.QualifiedName:
					if (XmlElementBase.textElement == null)
						XmlElementBase.textElement = new TextElement();

					return XmlElementBase.textElement;
			}

			if ( XmlElementBase.Elements.TryGetValue( elementName, out element ) )
				return element;

			element = XmlElementBase.CreateElement( elementName, assertIfNotHandled );

            if ( element != null )
            {
			    Debug.Assert( 
				    String.Equals( element.ElementName, elementName, StringComparison.InvariantCultureIgnoreCase ),
				    "An incorrect element handler was created for the following element: " + elementName );
            }

			XmlElementBase.Elements.Add( elementName, element );

			return element;
		}

		#endregion GetElement

		#region CreateElement

		private static XmlElementBase CreateElement( string elementName, bool assertIfNotHandled )
		{
			switch ( elementName )
			{
                case Accent1Element.QualifiedName:                          return new Accent1Element();
                case Accent2Element.QualifiedName:                          return new Accent2Element();
                case Accent3Element.QualifiedName:                          return new Accent3Element();
                case Accent4Element.QualifiedName:                          return new Accent4Element();
                case Accent5Element.QualifiedName:                          return new Accent5Element();
                case Accent6Element.QualifiedName:                          return new Accent6Element();
                case AlignmentElement.QualifiedName:                        return new AlignmentElement();
                case AlphaElement.QualifiedName:                            return new AlphaElement();
                case AlphaOffElement.QualifiedName:                         return new AlphaOffElement();
                case AlphaModElement.QualifiedName:                         return new AlphaModElement();
				case AnchorLegacyElement.QualifiedName:						return new AnchorLegacyElement();
                case AppVersionElement.QualifiedName:						return new AppVersionElement();
				case ApplicationElement.QualifiedName:						return new ApplicationElement();
				case AuthorElement.QualifiedName:							return new AuthorElement();
				case AuthorsElement.QualifiedName:							return new AuthorsElement();
                case BgColorElement.QualifiedName:                          return new BgColorElement();
                case BlueElement.QualifiedName:                             return new BlueElement();
                case BlueModElement.QualifiedName:                          return new BlueModElement();
                case BlueOffElement.QualifiedName:                          return new BlueOffElement();
                case BoldElement.QualifiedName:                             return new BoldElement();
				case BookViewsElement.QualifiedName:                        return new BookViewsElement();
				case BoolElement.QualifiedName:								return new BoolElement();
                case BorderElement.QualifiedName:                           return new BorderElement();
                case BordersElement.QualifiedName:                          return new BordersElement();
                case BottomElement.QualifiedName:                           return new BottomElement();
				case BrkElement.QualifiedName:								return new BrkElement();							// MD 2/1/11 - Page Break support
				case BstrElement.QualifiedName:								return new BstrElement();
				case CalculationPropertiesElement.QualifiedName:            return new CalculationPropertiesElement();
				case CategoryElement.QualifiedName:							return new CategoryElement();
                case CompElement.QualifiedName:                             return new CompElement();
                case CellStyleElement.QualifiedName:                        return new CellStyleElement();
                case CellStylesElement.QualifiedName:                       return new CellStylesElement();
                case CellStyleXfsElement.QualifiedName:                     return new CellStyleXfsElement();
                case CellXfsElement.QualifiedName:                          return new CellXfsElement();
				case ClientDataLegacyElement.QualifiedName:					return new ClientDataLegacyElement();
                case ClrSchemeElement.QualifiedName:                        return new ClrSchemeElement();
				case ClsidElement.QualifiedName:							return new ClsidElement();
				case CfElement.QualifiedName:								return new CfElement();
				case ColBreaksElement.QualifiedName:						return new ColBreaksElement();						// MD 2/1/11 - Page Break support
                case ColorElement.QualifiedName:                            return new ColorElement();
                case ColorsElement.QualifiedName:                           return new ColorsElement();
				case CommentElement.QualifiedName:							return new CommentElement();
				case ColHiddenLegacyElement.QualifiedName:					return new ColHiddenLegacyElement();
				case ColumnLegacyElement.QualifiedName:						return new ColumnLegacyElement();
				case CommentListElement.QualifiedName:						return new CommentListElement();
				case CommentsElement.QualifiedName:							return new CommentsElement();
				case CommentTextElement.QualifiedName:						return new CommentTextElement();
				case CompanyElement.QualifiedName:							return new CompanyElement();
				case ContentStatusElement.QualifiedName:					return new ContentStatusElement();
				case CorePropertiesElement.QualifiedName:					return new CorePropertiesElement();
				case CreatedElement.QualifiedName:							return new CreatedElement();
                case CreatorElement.QualifiedName:							return new CreatorElement();
                case CustClrElement.QualifiedName:                          return new CustClrElement();
                case CustClrLstElement.QualifiedName:                       return new CustClrLstElement();
				case CyElement.QualifiedName:								return new CyElement();
				case DataValidationElement.QualifiedName:					return new DataValidationElement();					// MD 2/1/11 - Data Validation support
				case DataValidationsElement.QualifiedName:					return new DataValidationsElement();
				case DateElement.QualifiedName:								return new DateElement();
				case DescriptionElement.QualifiedName:						return new DescriptionElement();
                case DiagonalElement.QualifiedName:                         return new DiagonalElement();
                case Dk1Element.QualifiedName:                              return new Dk1Element();
                case Dk2Element.QualifiedName:                              return new Dk2Element();
				case DocSecurityElement.QualifiedName:						return new DocSecurityElement();
                case DxfElement.QualifiedName:                              return new DxfElement();
                case DxfsElement.QualifiedName:                             return new DxfsElement();
				case ErrorElement.QualifiedName:							return new ErrorElement();
                case ExtraClrSchemeElement.QualifiedName:                   return new ExtraClrSchemeElement();
                case ExtraClrSchemeLstElement.QualifiedName:                return new ExtraClrSchemeLstElement();
                case FamilyElement.QualifiedName:                           return new FamilyElement();
                case FgColorElement.QualifiedName:                          return new FgColorElement();
				case FiletimeElement.QualifiedName:							return new FiletimeElement();
                case FillElement.QualifiedName:                             return new FillElement();
                case FillsElement.QualifiedName:                            return new FillsElement();
                case FolHlinkElement.QualifiedName:                         return new FolHlinkElement();
                case FontElement.QualifiedName:                             return new FontElement();
                case FontsElement.QualifiedName:                            return new FontsElement();
                case FontSizeElement.QualifiedName:                         return new FontSizeElement();
				case Formula1Element.QualifiedName:							return new Formula1Element();						// MD 2/1/11 - Data Validation support
				case Formula2Element.QualifiedName:							return new Formula2Element();						// MD 2/1/11 - Data Validation support
                case GradientFillElement.QualifiedName:                     return new GradientFillElement();
				case GraphicFrameElement.QualifiedName:						return new GraphicFrameElement();					// MD 10/12/10 - TFS49853
                case GrayElement.QualifiedName:                             return new GrayElement();
                case GreenElement.QualifiedName:                            return new GreenElement();
                case GreenModElement.QualifiedName:                         return new GreenModElement();
                case GreenOffElement.QualifiedName:                         return new GreenOffElement();
				case HeadingPairsElement.QualifiedName:						return new HeadingPairsElement();
                case HlinkElement.QualifiedName:                            return new HlinkElement();
                case HorizontalElement.QualifiedName:                       return new HorizontalElement();
                case HueElement.QualifiedName:                              return new HueElement();
                case HueModElement.QualifiedName:                           return new HueModElement();
                case HueOffElement.QualifiedName:                           return new HueOffElement();
				case HyperlinksChangedElement.QualifiedName:				return new HyperlinksChangedElement();
				case I1Element.QualifiedName:								return new I1Element();
				case I2Element.QualifiedName:								return new I2Element();
				case I4Element.QualifiedName:								return new I4Element();
				case I8Element.QualifiedName:								return new I8Element();
                case IndexedColorsElement.QualifiedName:                    return new IndexedColorsElement();
                case InvElement.QualifiedName:                              return new InvElement();
                case ItalicElement.QualifiedName:                           return new ItalicElement();
				case KeywordsElement.QualifiedName:							return new KeywordsElement();
				case LastModifiedByElement.QualifiedName:					return new LastModifiedByElement();
                case LeftElement.QualifiedName:                             return new LeftElement();
				case LegacyDrawingElement.QualifiedName:					return new LegacyDrawingElement();
				case LinksUpToDateElement.QualifiedName:					return new LinksUpToDateElement();
				case LpstrElement.QualifiedName:							return new LpstrElement();
				case LpwstrElement.QualifiedName:							return new LpwstrElement();
                case Lt1Element.QualifiedName:                              return new Lt1Element();
                case Lt2Element.QualifiedName:                              return new Lt2Element();
                case LumElement.QualifiedName:                              return new LumElement();
                case LumModElement.QualifiedName:                           return new LumModElement();
                case LumOffElement.QualifiedName:                           return new LumOffElement();
				case ModifiedElement.QualifiedName:							return new ModifiedElement();
                case NameElement.QualifiedName:                             return new NameElement();
                case NumFmtElement.QualifiedName:                           return new NumFmtElement();
                case NumFmtsElement.QualifiedName:                          return new NumFmtsElement();
				case PathLegacyElement.QualifiedName:						return new PathLegacyElement();
                case PatternFillElement.QualifiedName:                      return new PatternFillElement();
				case PropertiesElement.QualifiedName:						return new PropertiesElement();
                case ProtectionElement.QualifiedName:                       return new ProtectionElement();
				case R4Element.QualifiedName:								return new R4Element();
				case R8Element.QualifiedName:								return new R8Element();
                case RedElement.QualifiedName:                              return new RedElement();
                case RedModElement.QualifiedName:                           return new RedModElement();
                case RedOffElement.QualifiedName:                           return new RedOffElement();
                case RgbColorElement.QualifiedName:                         return new RgbColorElement();
                case RightElement.QualifiedName:                            return new RightElement();
				case RowBreaksElement.QualifiedName:						return new RowBreaksElement();						// MD 2/1/11 - Page Break support
				case RowHiddenLegacyElement.QualifiedName:					return new RowHiddenLegacyElement();
				case RowLegacyElement.QualifiedName:						return new RowLegacyElement();
                case SatElement.QualifiedName:                              return new SatElement();
                case SatModElement.QualifiedName:                           return new SatModElement();
                case SatOffElement.QualifiedName:                           return new SatOffElement();
				case ScaleCropElement.QualifiedName:						return new ScaleCropElement();
                case SchemeElement.QualifiedName:                           return new SchemeElement();
                case ShadeElement.QualifiedName:                            return new ShadeElement();
				case ShapeLegacyElement.QualifiedName:						return new ShapeLegacyElement();
				case ShapeTypeLegacyElement.QualifiedName:					return new ShapeTypeLegacyElement();
				case SharedDocElement.QualifiedName:						return new SharedDocElement();
				case SheetProtectionElement.QualifiedName:					return new SheetProtectionElement();				// MD 3/22/11 - TFS66776
                case SrgbClrElement.QualifiedName:                          return new SrgbClrElement();
                case StopElement.QualifiedName:                             return new StopElement();
                case StrikeThroughElement.QualifiedName:                    return new StrikeThroughElement();
				case StrokeLegacyElement.QualifiedName:						return new StrokeLegacyElement();
                case StyleSheetElement.QualifiedName:                       return new StyleSheetElement();
				case SubjectElement.QualifiedName:							return new SubjectElement();
                case SysClrElement.QualifiedName:                           return new SysClrElement();
                case ThemeElement.QualifiedName:                            return new ThemeElement();
                case ThemeElementsElement.QualifiedName:                    return new ThemeElementsElement();
                case TintElement.QualifiedName:                             return new TintElement();
                case TitleElement.QualifiedName:							return new TitleElement();
				case TitlesOfPartsElement.QualifiedName:					return new TitlesOfPartsElement();
                case TopElement.QualifiedName:                              return new TopElement();
				case Ui1Element.QualifiedName:								return new Ui1Element();
				case Ui2Element.QualifiedName:								return new Ui2Element();
				case Ui4Element.QualifiedName:								return new Ui4Element();
				case Ui8Element.QualifiedName:								return new Ui8Element();
				case VariantElement.QualifiedName:							return new VariantElement();
				case VectorElement.QualifiedName:							return new VectorElement();
                case VertAlignElement.QualifiedName:                        return new VertAlignElement();
                case VerticalElement.QualifiedName:                         return new VerticalElement();
				case VisibleLegacyElement.QualifiedName:					return new VisibleLegacyElement();
				case WorkbookElement.QualifiedName:                         return new WorkbookElement();
				case WorkbookViewElement.QualifiedName:                     return new WorkbookViewElement();
                case WorksheetElement.QualifiedName:                        return new WorksheetElement();
                case XfElement.QualifiedName:                               return new XfElement();
				case XmlLegacyElement.QualifiedName:						return new XmlLegacyElement();
                case SheetsElement.QualifiedName:                           return new SheetsElement();
                case SheetElement.QualifiedName:                            return new SheetElement();
                case SheetViewElement.QualifiedName:                        return new SheetViewElement();
                case SheetViewsElement.QualifiedName:                       return new SheetViewsElement();
                case PageMarginsElement.QualifiedName:                      return new PageMarginsElement();
                case PrintOptionsElement.QualifiedName:                     return new PrintOptionsElement();                
                case DimensionElement.QualifiedName:                        return new DimensionElement();
                case SheetFormatPrElement.QualifiedName:                    return new SheetFormatPrElement();
                case WorkbookPrElement.QualifiedName:                       return new WorkbookPrElement();
                case SheetDataElement.QualifiedName:                        return new SheetDataElement();
                case RowElement.QualifiedName:                              return new RowElement();
                case CellElement.QualifiedName:                             return new CellElement();
                case CellValueElement.QualifiedName:                        return new CellValueElement();
                case SharedStringTableElement.QualifiedName:                return new SharedStringTableElement();
                case StringItemElement.QualifiedName:                       return new StringItemElement();
                case TextElement.QualifiedName:                             return new TextElement();
                case RichTextRunElement.QualifiedName:                      return new RichTextRunElement();
                case RichTextRunPropertiesElement.QualifiedName:            return new RichTextRunPropertiesElement();
                case UnderlineElement.QualifiedName:                        return new UnderlineElement();
                case FontNameElement.QualifiedName:                         return new FontNameElement();
                case PageSetupElement.QualifiedName:                        return new PageSetupElement();
                case ColumnElement.QualifiedName:                           return new ColumnElement();
                case ColumnsElement.QualifiedName:                          return new ColumnsElement();
                case PaneElement.QualifiedName:                             return new PaneElement();
                case HeaderFooterElement.QualifiedName:                     return new HeaderFooterElement();
                case OddHeaderElement.QualifiedName:                        return new OddHeaderElement();
                case OddFooterElement.QualifiedName:                        return new OddFooterElement();
                case SheetPrElement.QualifiedName:                          return new SheetPrElement();
                case OutlinePrElement.QualifiedName:                        return new OutlinePrElement();
                case PageSetUpPrElement.QualifiedName:                      return new PageSetUpPrElement();
                case TabColorElement.QualifiedName:                         return new TabColorElement();
                case MergeCellElement.QualifiedName:                        return new MergeCellElement();
                case MergeCellsElement.QualifiedName:                       return new MergeCellsElement();
                case PictureElement.QualifiedName:                          return new PictureElement();
                case SheetCalcPrElement.QualifiedName:                      return new SheetCalcPrElement();
                case FormulaElement.QualifiedName:                          return new FormulaElement();            
                case CustomWorkbookViewsElement.QualifiedName:              return new CustomWorkbookViewsElement();
                case CustomWorkbookViewElement.QualifiedName:               return new CustomWorkbookViewElement();
                case CustomSheetViewsElement.QualifiedName:                 return new CustomSheetViewsElement();
                case CustomSheetViewElement.QualifiedName:                  return new CustomSheetViewElement();
                case DefinedNameElement.QualifiedName:                      return new DefinedNameElement();
                case DefinedNamesElement.QualifiedName:                     return new DefinedNamesElement();
                case ExternalReferenceElement.QualifiedName:                return new ExternalReferenceElement();
                case ExternalReferencesElement.QualifiedName:               return new ExternalReferencesElement();
                case ExternalLinkElement.QualifiedName:                     return new ExternalLinkElement();
                case ExternalBookElement.QualifiedName:                     return new ExternalBookElement();
                case SheetNameElement.QualifiedName:                        return new SheetNameElement();
                case SheetNamesElement.QualifiedName:                       return new SheetNamesElement();
                case SheetDataSetElement.QualifiedName:                     return new SheetDataSetElement();
                case ExternalCellElement.QualifiedName:                     return new ExternalCellElement();
                case ManagerElement.QualifiedName:                          return new ManagerElement();
				case WorkbookProtectionElement.QualifiedName:				return new WorkbookProtectionElement();				// MD 11/22/11 - Found while writing Interop tests

				// MD 12/6/11 - 12.1 - Table Support
				case AutoFilterElement.QualifiedName:						return new AutoFilterElement();
				case CalculatedColumnFormulaElement.QualifiedName:			return new CalculatedColumnFormulaElement();
				case ColorFilterElement.QualifiedName:						return new ColorFilterElement();
				case CustomFiltersElement.QualifiedName:					return new CustomFiltersElement();
				case CustomFilterElement.QualifiedName:						return new CustomFilterElement();
				case DateGroupItemElement.QualifiedName:					return new DateGroupItemElement();
				case DynamicFilterElement.QualifiedName:					return new DynamicFilterElement();
				case FilterColumnElement.QualifiedName:						return new FilterColumnElement();
				case FilterElement.QualifiedName:							return new FilterElement();
				case FiltersElement.QualifiedName:							return new FiltersElement();
				case IconFilterElement.QualifiedName:						return new IconFilterElement();
				case SchemeClrElement.QualifiedName:						return new SchemeClrElement();
				case SortConditionElement.QualifiedName:					return new SortConditionElement();
				case SortStateElement.QualifiedName:						return new SortStateElement();
				case TableColumnElement.QualifiedName:						return new TableColumnElement();
				case TableColumnsElement.QualifiedName:						return new TableColumnsElement();
				case TableElement.QualifiedName:							return new TableElement();
				case TablePartElement.QualifiedName:						return new TablePartElement();
				case TablePartsElement.QualifiedName:						return new TablePartsElement();
				case TableStyleElement.QualifiedName:						return new TableStyleElement();
				case TableStyleElementElement.QualifiedName:				return new TableStyleElementElement();
				case TableStyleInfoElement.QualifiedName:					return new TableStyleInfoElement();
				case TableStylesElement.QualifiedName:						return new TableStylesElement();
				case Top10Element.QualifiedName:							return new Top10Element();
				case TotalsRowFormulaElement.QualifiedName:					return new TotalsRowFormulaElement();

				// MD 2/9/12 - TFS89375
				case FontSchemeElement.QualifiedName:						return new FontSchemeElement();
				case MajorFontElement.QualifiedName:						return new MajorFontElement();
				case MinorFontElement.QualifiedName:						return new MinorFontElement();
				case SupplementalFontElement.QualifiedName:					return new SupplementalFontElement();

                #region Drawing elements
                
                case AbsoluteAnchorElement.QualifiedName:                   return new AbsoluteAnchorElement();
                case BlipElement.QualifiedName:                             return new BlipElement();
                case BlipFillElement.QualifiedName:                         return new BlipFillElement();
				case ChartElement.QualifiedName:							return new ChartElement();							// MD 10/12/10 - TFS49853
                case ClientDataElement.QualifiedName:                       return new ClientDataElement();
                case ChOffElement.QualifiedName:                            return new ChOffElement();
                case ChExtElement.QualifiedName:                            return new ChExtElement();
                case CNvCxnSpPrElement.QualifiedName:                       return new CNvCxnSpPrElement();
                case CNvGrpSpPrElement.QualifiedName:                       return new CNvGrpSpPrElement();
                case CNvPicPrElement.QualifiedName:                         return new CNvPicPrElement();
                case CNvPrElement.QualifiedName:                            return new CNvPrElement();
                case ColElement.QualifiedName:                              return new ColElement();
                case ColOffElement.QualifiedName:                           return new ColOffElement();
				case CxnSpElement.QualifiedName:							return new CxnSpElement();							// MD 7/14/11 - Shape support
                case DrawingElement.QualifiedName:                          return new DrawingElement();
                case ExtElement.QualifiedName:                              return new ExtElement();
				case ExtSPElement.QualifiedName:                            return new ExtSPElement();							// MD 2/15/11 - TFS66316
                case FromElement.QualifiedName:                             return new FromElement();
                case GrpSpElement.QualifiedName:                            return new GrpSpElement();
                case GrpSpPrElement.QualifiedName:                          return new GrpSpPrElement();
                case NvGrpSpPrElement.QualifiedName:                        return new NvGrpSpPrElement();
                case NvPicPrElement.QualifiedName:                          return new NvPicPrElement();
                case OffElement.QualifiedName:                              return new OffElement();
                case OneCellAnchorElement.QualifiedName:                    return new OneCellAnchorElement();
                case PicElement.QualifiedName:                              return new PicElement();
                case PicLocksElement.QualifiedName:                         return new PicLocksElement();
                case PrstGeomElement.QualifiedName:                         return new PrstGeomElement();
                case DrawingMLRowElement.QualifiedName:                     return new DrawingMLRowElement();
                case RElement.QualifiedName:                                return new RElement();
                case RowOffElement.QualifiedName:                           return new RowOffElement();
                case RPrElement.QualifiedName:                              return new RPrElement();
                case SolidFillElement.QualifiedName:                        return new SolidFillElement();
                case SpElement.QualifiedName:                               return new SpElement();
                case SpPrElement.QualifiedName:                             return new SpPrElement();
                case TElement.QualifiedName:                                return new TElement();
                case ToElement.QualifiedName:                               return new ToElement();
                case TwoCellAnchorElement.QualifiedName:                    return new TwoCellAnchorElement();
                case TxBodyElement.QualifiedName:                           return new TxBodyElement();
                case WsDrElement.QualifiedName:                             return new WsDrElement();
                case XfrmElement.QualifiedName:                             return new XfrmElement();
                case ShadowLegacyElement.QualifiedName:                     return new ShadowLegacyElement();
				case LnElement.QualifiedName:								return new LnElement();								// MD 8/23/11 - TFS84306
				case NoFillElement.QualifiedName:							return new NoFillElement();							// MD 8/23/11 - TFS84306
				case BodyPrElement.QualifiedName:							return new BodyPrElement();							// MD 11/8/11 - TFS85193
				case PElement.QualifiedName:								return new PElement();								// MD 11/8/11 - TFS85193
				case PPrElement.QualifiedName:								return new PPrElement();							// MD 11/8/11 - TFS85193
				case LatinElement.QualifiedName:							return new LatinElement();							// MD 11/8/11 - TFS85193

				// MD 3/12/12 - TFS102234
				case LnRefElement.QualifiedName:							return new LnRefElement();

				case HeadEndElement.QualifiedName:							return new HeadEndElement();
				case TailEndElement.QualifiedName:							return new TailEndElement();

				// MD 7/5/12 - TFS115688
				case ExtLstElement.QualifiedName:							return new ExtLstElement();

                #endregion Drawing elements
                
                //Unused Elements
                // return null to avoid the following Utilities.DebugFail.
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/charset":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/condense":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/extend":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/outline":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/shadow":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/fileVersion":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/fileSharing":                
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/webPublishing":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/webPublishObjects": 
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/smartTagPr":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/smartTagTypes":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/extLst":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/fileRecoveryPr":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/pivotCaches":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/oleSize":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/smartTags":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/cellSmartTags":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/cellSmartTag":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/cellSmartTagPr":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/cellWatches":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/conditionalFormatting":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/controls":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/customProperties":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/dataConsolidate":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/hyperlinks":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/ignoredErrors":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/oleObjects":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/phoneticPr":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/scenarios":          
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/webPublishItems":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/pivotSelection":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/rPh":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/selection":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/evenFooter":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/evenHeader":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/firstFooter":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/firstHeader":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/protectedRanges":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/mruColors":
                case "http://schemas.openxmlformats.org/drawingml/2006/main/objectDefaults":
                case "http://schemas.openxmlformats.org/drawingml/2006/main/fmtScheme":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/calcChain":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/functionGroups":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/ddeLink":
                case "http://schemas.openxmlformats.org/spreadsheetml/2006/main/oleLink":
				case "http://schemas.openxmlformats.org/drawingml/2006/main/cs":
				case "http://schemas.openxmlformats.org/drawingml/2006/main/ea":
                case "http://schemas.openxmlformats.org/package/2006/metadata/core-properties/lastPrinted":
				case "urn:schemas-microsoft-com:office:office/shapelayout":
				case "urn:schemas-microsoft-com:vml/fill":
				case "urn:schemas-microsoft-com:vml/textbox":
				case "urn:schemas-microsoft-com:office:excel/MoveWithCells":
				case "urn:schemas-microsoft-com:office:excel/SizeWithCells":
				case "urn:schemas-microsoft-com:office:excel/AutoFill":
                    return null;
			}

            if ( assertIfNotHandled )
			    Utilities.DebugFail( "Unhandled element : " + elementName );

			return null;
		}

		#endregion CreateElement

        //  BF 10/8/10  NA 2011.1 - Infragistics.Word
        //  This helps with debugging, remove it if it's annoying
        #region ToString
        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        public override string ToString()
        {



            return base.ToString();

        }
        #endregion ToString
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