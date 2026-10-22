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
            var xamlDefinition = action?.Element("XamlDefinition")?.Value;
            if (string.IsNullOrWhiteSpace(xamlDefinition))
            {
                throw new BiteFormatException("Resource has no <XamlDefinition>", service);
            }
            return ReadXaml(xamlDefinition, (string?)service.Attribute("ID") ?? Hash(xamlDefinition));
        }

        /// <summary>Reads a bare XAML definition; <paramref name="resourceId"/> seeds stable ids for activities saved without one.</summary>
        public IDev2Activity? ReadXaml(string xamlDefinition, string resourceId)
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
            var chart = new XamlObjectReader(_resolver, resourceId).ReadWorkflow(root);
            return FlowGraphBuilder.Build(chart);
        }

        static string Hash(string text)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
            }
        }
    }
}
