﻿/*
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
using Dev2.Interfaces;
using Warewolf.Data;
using Dev2.Common.Interfaces.Runtime.WebServer;

namespace Dev2.Runtime.WebServer
{
    public static class ServiceTestCoverageModelHTMLResultBuilder
    {
        public static void SetupNavBarHtml(this HtmlTextWriter writer)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "10px 10px 20px 10px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "5px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "Roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "28px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "500");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "nav-bar-row");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write("Coverage Summary");
            writer.RenderEndTag();
        }

        public static void SetupWorkflowRowHtml(this HtmlTextWriter writer, string resourcePath, ICoverageDataObject coverageData, IWorkflowCoverageReports coverageReports)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "SetupWorkflowPathHtml");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "#333");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "20%");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "8px 16px 16px 8px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write(resourcePath);
            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "SetupWorkflowPathHtml-link");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "12px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute(HtmlTextWriterAttribute.Target, "_new");
            var testUrl = coverageData.GetTestUrl(resourcePath);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, testUrl);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write("Run Tests");
            writer.RenderEndTag();
            writer.RenderEndTag();

            (double totalCoverage, List<IWorkflowNode> workflowNodes, IWorkflowNode[] coveredNodes) = coverageReports.GetTotalCoverage();

            writer.SetupWorkflowReportsHtml(totalCoverage, nameof(SetupWorkflowReportsHtml));
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "500");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0 0 0 35px");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "workflow-nodes-row");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            workflowNodes.ForEach(node => node.SetupWorkflowNodeHtml(writer, coveredNodes));

            writer.RenderEndTag();
        }

        public static void SetupWorkflowReportsHtml(this HtmlTextWriter writer, double CoveragePercentage , string classValue)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, classValue);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            double testFailing = (1 - (CoveragePercentage));
            double testPassed = CoveragePercentage;

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
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "12px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, testPassed.ToString("0%"));
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(testPassed.ToString("0%"));
                writer.RenderEndTag();
            }

            if (testFailing > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "progress-bar-failed");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, testFailing.ToString("0%") == "100%" ? "white" : "red");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "red");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
                writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "10px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "5px 0px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "12px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, testFailing.ToString("0%"));
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(testFailing.ToString("0%") == "100%" ? "0%" : "-");
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
            writer.RenderEndTag();
        }


        public static void SetupWorkflowNodeHtml(this IWorkflowNode workflowNode, HtmlTextWriter writer, IWorkflowNode[] coveredNodes)
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

        public static void SetupNextNodeHTML(HtmlTextWriter writer, IWorkflowNode[] coveredNodes, IWorkflowNode node)
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


        internal static void SetupCountSummaryHtml(this List<IServiceTestModelTO> allTests, HtmlTextWriter writer, ICoverageDataObject coverageData)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "10px 10px 20px 10px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "5px");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "count-summary row");
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
            writer.Write("Total Test Count: " + allTests.Count);
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

            var failedCount = allTests.Count(o => o.TestFailing);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            if (failedCount > 0)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "black");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-red");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Tests Failed: " + failedCount);
            writer.RenderEndTag();

            var invalidCount = allTests.Count(o => o.TestInvalid);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            if (invalidCount > 0)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "orange");
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "black");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-red");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Tests Invalid: " + invalidCount);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if (coverageData.IsMultipleWorkflowReport)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Target, "_new");
                writer.AddAttribute(HtmlTextWriterAttribute.Href, coverageData.GetAllTestsUrl());
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write("Run All Tests");
                writer.RenderEndTag();
            }
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private static bool IsNodeCovered(IWorkflowNode[] coveredNodes, IWorkflowNode node)
        {
            return coveredNodes.Any(o => o.ActivityID == node.UniqueID && o.MockSelected is false);
        }
    }
}