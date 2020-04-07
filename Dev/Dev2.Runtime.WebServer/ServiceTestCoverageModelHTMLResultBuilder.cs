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
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "10px 10px 20px 10px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "5px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "Roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "28px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "500");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, classValue);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write(resourcePath);
            writer.RenderEndTag();
        }

        public static void SetupWorkflowReportsHtml(this IServiceTestCoverageModelTo coverageModelTo, HtmlTextWriter writer, string classValue)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, classValue);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            double testFailing = (100 - (coverageModelTo.CoveragePercentage * 100)) / 100;
            double testPassed = coverageModelTo.CoveragePercentage;

            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "progress-bar-width");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "80px");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            if (testPassed > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "progress-bar-passed");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "green");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "10px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "white");
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "5px 0px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, testPassed.ToString("0%"));
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(testPassed.ToString("0%"));
                writer.RenderEndTag();
            }

            if (testFailing > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "progress-bar-failed");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "red");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "10px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "white");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "5px 0px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, testFailing.ToString("0%"));
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(testFailing.ToString("0%"));
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
            writer.RenderEndTag();
        }

       
        public static void SetupWorkflowNodeHtml(this IWorkflowNode workflowNode, HtmlTextWriter writer, string className, List<IWorkflowNode> coveredNodes)
        {
            if (IsNodeCovered(coveredNodes, workflowNode))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "green");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "12px");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "step-description-green");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(workflowNode.StepDescription);
                writer.RenderEndTag();
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "12px");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "step-description-red");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(workflowNode.StepDescription);
                writer.RenderEndTag();
            }
           
            if (workflowNode.NextNodes?.Count > 0)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "12px");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "next-nodes");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                workflowNode.NextNodes.ForEach(node => SetupNextNodeHTML(writer, coveredNodes, node));
                writer.RenderEndTag();
            }
        }

        public static void SetupNextNodeHTML(HtmlTextWriter writer, List<IWorkflowNode> coveredNodes, IWorkflowNode node)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "next-node");

            if (IsNodeCovered(coveredNodes, node))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "green");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "12px");
                writer.RenderBeginTag(HtmlTextWriterTag.Li);
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "12px");
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
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0 -15px 0 -15px");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "black");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-black");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Total Test Count: " + allTests.Count.ToString());
            writer.RenderEndTag();

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "green");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-green");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Tests Passed: " + allTests.Count(o => o.TestPassed));
            writer.RenderEndTag();

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-red");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Tests Failed: " + allTests.Count(o => o.TestFailing));
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private static bool IsNodeCovered(List<IWorkflowNode> coveredNodes, IWorkflowNode node)
        {
            return coveredNodes.Any(o => o.UniqueID == node.UniqueID);
        }
    }
}