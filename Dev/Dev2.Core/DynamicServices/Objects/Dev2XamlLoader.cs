#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
#if (WINDOWS || NETFRAMEWORK)
using System.Activities.Presentation.View;
#endif
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xaml;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Util;
using System.Xml;

namespace Dev2.DynamicServices.Objects
{
    /// <summary>
    ///     Created to break memory leak in ServiceAction ;)
    /// </summary>
    public class Dev2XamlLoader
    {
        /// <summary>
        ///     Loads the specified xaml definition.
        /// </summary>
        /// <param name="xamlDefinition">The xaml definition.</param>
        /// <param name="xamlStream">The xaml stream.</param>
        /// <param name="workflowPool">The workflow pool.</param>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <exception cref="System.ArgumentNullException">xamlDefinition</exception>
        public void Load(StringBuilder xamlDefinition, ref Stream xamlStream,
            ref Queue<PooledServiceActivity> workflowPool, ref Activity workflowActivity)
        {
            if (xamlDefinition == null || xamlDefinition.Length == 0)
            {
                throw new ArgumentNullException("xamlDefinition");
            }
            // Travis.Frisinger : 13.11.2012 - Remove bad namespaces

            if (GlobalConstants.RuntimeNamespaceClean)
                
            {
                xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
            }
			// End Mods

#if !(WINDOWS || NETFRAMEWORK)
            RemoveWindowsElements(ref xamlDefinition);
#endif

			var generation = 0;

            using (xamlStream = xamlDefinition.EncodeForXmlDocument())
            {
                var settings = new XamlXmlReaderSettings
#if (WINDOWS || NETFRAMEWORK)
				{
					LocalAssembly = System.Reflection.Assembly.GetAssembly(typeof(VirtualizedContainerService))
				};
#else
				();
#endif
				using (var reader = new XamlXmlReader(xamlStream, settings))
                {
                    workflowActivity = ActivityXamlServices.Load(reader);
                }

                xamlStream.Seek(0, SeekOrigin.Begin);
                workflowPool.Clear();

                generation++;

                for (int i = 0; i < GlobalConstants._xamlPoolSize; i++)
                {
                    var activity = ActivityXamlServices.Load(xamlStream);
                    xamlStream.Seek(0, SeekOrigin.Begin);
                    workflowPool.Enqueue(new PooledServiceActivity(generation, activity));
                }
            }
		}

		public static void RemoveWindowsElements(ref StringBuilder xamlBuilder)
		{
			// Load XAML content into an XmlDocument
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xamlBuilder.ToString());

			// Define namespaces and elements to remove
			string vbNamespaceUri = "clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities";
			string hintSizeNamespaceUri = "http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation";
			string vbElementName = "VisualBasic.Settings";
			string hintSizeElementName = "VirtualizedContainerService.HintSize";

			// Remove VisualBasic.Settings elements
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("mva", vbNamespaceUri);
			XmlNodeList vbNodesToRemove = doc.SelectNodes($"//mva:{vbElementName}", nsmgr);
			foreach (XmlNode node in vbNodesToRemove)
			{
				node.ParentNode.RemoveChild(node);
			}

			// Remove ALL child elements in the sap: presentation namespace — this covers
			// VirtualizedContainerService.HintSize, WorkflowViewStateService.ViewState, and
			// any other designer-only attached-property elements CoreWF cannot handle.
			nsmgr.AddNamespace("sap", hintSizeNamespaceUri);
			XmlNodeList sapNodesToRemove = doc.SelectNodes("//sap:*", nsmgr);
			// Collect into a plain list first — modifying the DOM invalidates a live XmlNodeList.
			var sapNodes = new System.Collections.Generic.List<XmlNode>();
			foreach (XmlNode node in sapNodesToRemove)
				sapNodes.Add(node);
			foreach (var node in sapNodes)
				node.ParentNode?.RemoveChild(node);

			// Remove sap: attached-property attributes (e.g. sap:VirtualizedContainerService.HintSize="…")
			XmlNodeList elementsWithSapAttrs = doc.SelectNodes("//*", nsmgr);
			foreach (XmlNode node in elementsWithSapAttrs)
			{
				if (node.Attributes == null) continue;
				var toRemove = new System.Collections.Generic.List<XmlAttribute>();
				foreach (XmlAttribute attr in node.Attributes)
					if (attr.NamespaceURI == hintSizeNamespaceUri)
						toRemove.Add(attr);
				foreach (var attr in toRemove)
					node.Attributes.Remove(attr);
			}

			// Update the StringBuilder with the modified XML
			xamlBuilder.Clear();
			xamlBuilder.Append(doc.OuterXml);
		}
	}
}