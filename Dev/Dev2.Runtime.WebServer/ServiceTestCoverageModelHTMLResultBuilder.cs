/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Dev2.Runtime.WebServer
{
    public static class ServiceTestCoverageModelHTMLResultBuilder
    {

        public static void SetupNavBarHtml(this HtmlTextWriter writer, string classValue, string resourcePath)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, classValue);

            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "#F7F7F7");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "65");

            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.RenderBeginTag(HtmlTextWriterTag.H2);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "Roboto sans-serif");
            writer.Write(resourcePath);
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        public static void SetupWorkflowReportsHtml(this IServiceTestCoverageModelTo coverageModelTo, HtmlTextWriter writer, string classValue)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, classValue);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "5px 0 15px 0");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.Write("Report name - " + coverageModelTo.ReportName + "  Coverage: " + coverageModelTo.CoveragePercentage);

            writer.RenderEndTag(); 
        }


        public static void SetupWorkflowNodeHtml(this IWorkflowNode workflowNode, HtmlTextWriter writer, string className, List<IWorkflowNode> coveredNodes)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            if (IsNodeCovered(coveredNodes, workflowNode))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "green");
                writer.Write(workflowNode.StepDescription);
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
                writer.Write(workflowNode.StepDescription);
            }

            if (workflowNode.NextNodes?.Count > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "next-nodes");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                workflowNode.NextNodes.ForEach(node => SetupNextNodeHTML(writer, coveredNodes, node));
            }

            writer.RenderEndTag();
        }

        public static void SetupNextNodeHTML(HtmlTextWriter writer, List<IWorkflowNode> coveredNodes, IWorkflowNode node)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "next-node");

            if (IsNodeCovered(coveredNodes, node))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "green");
                writer.RenderBeginTag(HtmlTextWriterTag.Li);
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
                writer.RenderBeginTag(HtmlTextWriterTag.Li);
            }

            writer.Write(node.StepDescription);
            writer.RenderEndTag();

        }

        public static void SetupCountSummaryHtml(this List<IServiceTestModelTO> allTests, HtmlTextWriter writer, string className)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "10px 10px 20px 10px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "5px");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, className);

            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "row table-borderless");

            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "200px");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "counts");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            writer.Write("Total Test Count: " + allTests.Count.ToString());

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "200px");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "counts");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "oi oi-beaker green");
            writer.RenderEndTag();
            writer.Write("Tests Passed: " + allTests.Count(o => o.TestPassed));

            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "oi oi-beaker green");
            writer.RenderEndTag();
            writer.Write("Tests Failed: " + allTests.Count(o => o.TestFailing));

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        public static void SetupWorkflowPathHtml(this object resourcePath, HtmlTextWriter writer, string classValue)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, classValue);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "#F36F21");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "5px 0 15px 0");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.Write("Summary path - " + resourcePath);

            writer.RenderEndTag();
        }

        private static bool IsNodeCovered(List<IWorkflowNode> coveredNodes, IWorkflowNode node)
        {
            return coveredNodes.Any(o => o.UniqueID == node.UniqueID);
        }
    }
}
