/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Dev2;
using Warewolf.Bite.Flow;
using Warewolf.Bite.Xaml;

namespace Warewolf.Bite
{
    /// <summary>
    /// The WF-free <see cref="IBiteReader"/>: reads a workflow's <c>&lt;XamlDefinition&gt;</c> with System.Xml and links the
    /// executable graph directly, without <c>ActivityXamlServices.Load</c> or <c>ActivityParser</c>, so nothing is
    /// compiled. Fails with <see cref="BiteFormatException"/> rather than returning a partial graph.
    /// </summary>
    public sealed class BiteReader : IBiteReader
    {
        readonly XamlTypeResolver _resolver = new XamlTypeResolver();

        public IDev2Activity? Read(string bite)
        {
            XElement service;
            try
            {
                service = XElement.Parse(bite, LoadOptions.SetLineInfo);
            }
            catch (XmlException e)
            {
                throw new BiteFormatException($"Resource is not valid XML: {e.Message}", null, e);
            }

            // Same lookup as WorkflowExecutor.ExtractWorkflowParts: <Action> directly or under <Actions>.
            var action = service.Element("Actions")?.Element("Action") ?? service.Element("Action");

            // A versioned JSON body (T1.5) takes precedence over legacy XAML.
            var biteDefinition = action?.Element("BiteDefinition");
            if (biteDefinition != null)
            {
                var version = (string?)biteDefinition.Attribute("FormatVersion");
                if (version != Json.BiteJsonWriter.FormatVersion.ToString(System.Globalization.CultureInfo.InvariantCulture))
                {
                    throw new BiteFormatException($"Unsupported BiteDefinition FormatVersion '{version}'; this reader understands '{Json.BiteJsonWriter.FormatVersion}'", biteDefinition);
                }
                return ReadJson(biteDefinition.Value, SeedFor(service, biteDefinition.Value));
            }

            var xamlDefinition = action?.Element("XamlDefinition")?.Value;
            if (string.IsNullOrWhiteSpace(xamlDefinition))
            {
                throw new BiteFormatException("Resource has no <BiteDefinition> or <XamlDefinition>", service);
            }
            return ReadXaml(xamlDefinition, SeedFor(service, xamlDefinition));
        }

        /// <summary>Reads a bare XAML definition; <paramref name="resourceId"/> seeds stable ids for activities saved without one.</summary>
        public IDev2Activity? ReadXaml(string xamlDefinition, string resourceId) =>
            FlowGraphBuilder.Build(ReadModel(xamlDefinition, resourceId));

        /// <summary>Reads a bare version 1 JSON body.</summary>
        public IDev2Activity? ReadJson(string jsonDefinition, string resourceId) =>
            FlowGraphBuilder.Build(new Json.BiteJsonReader(_resolver, resourceId).Read(jsonDefinition));

        /// <summary>The unlinked definition of a XAML workflow, or null when its implementation is not a Flowchart.</summary>
        public FlowchartModel? ReadModel(string xamlDefinition, string resourceId)
        {
            XElement root;
            try
            {
                root = XElement.Parse(xamlDefinition, LoadOptions.SetLineInfo);
            }
            catch (XmlException e)
            {
                throw new BiteFormatException($"XamlDefinition is not valid XML: {e.Message}", null, e);
            }
            return new XamlObjectReader(_resolver, resourceId).ReadWorkflow(root);
        }

        internal static string SeedFor(XElement service, string body) => (string?)service.Attribute("ID") ?? Hash(body);

        static string Hash(string text)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
            }
        }
    }
}
