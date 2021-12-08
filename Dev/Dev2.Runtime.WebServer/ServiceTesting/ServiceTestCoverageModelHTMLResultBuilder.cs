/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
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
using System;
using Dev2.Data;

namespace Dev2.Runtime.WebServer
{
    public static class ServiceTestCoverageModelHTMLResultBuilder
    {
        public static void SetupNavBarHtml(this HtmlTextWriter writer, double totalReportsCoverage)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "10px 10px 20px 10px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "5px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "Roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "28px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "500");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "nav-bar-row");

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write("Coverage Summary:");
            writer.RenderEndTag();

            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "5px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "Roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "28px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "500");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "nav-bar-row");

            writer.AddColorCoding(totalReportsCoverage);

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write(Math.Round(totalReportsCoverage, 0) +" %");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Br);
        }

        private static void AddColorCoding(this HtmlTextWriter writer, double totalReportsCoverage)
        {
            if (IsCoverageRed(totalReportsCoverage))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
            }
            if (IsCoverageYellow(totalReportsCoverage))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "orange");
            }
            if (IsCoverageGreen(totalReportsCoverage))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "green");
            }
        }

        private static bool IsCoverageGreen(double totalReportsCoverage)
        {
            return totalReportsCoverage >= 85 && totalReportsCoverage <= 100;
        }

        private static bool IsCoverageYellow(double totalReportsCoverage)
        {
            return totalReportsCoverage >= 65 && totalReportsCoverage <= 85;
        }

        private static bool IsCoverageRed(double totalReportsCoverage)
        {
            return totalReportsCoverage >= 0 && totalReportsCoverage <= 65;
        }

        public static void SetupWorkflowRowHtml(this HtmlTextWriter writer, string resourcePath, ICoverageDataObject coverageData, IWorkflowCoverageReportsTO coverageReports)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "SetupWorkflowPathHtml");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "#333");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "20%");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "8px 16px 5px 8px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write(resourcePath + "\\" + coverageData.ReportName?.Replace("*", ""));
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

            writer.SetupWorkflowReportsHtml(coverageReports.TotalCoverage, nameof(SetupWorkflowReportsHtml));
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "500");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0 0 0 35px");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "workflow-nodes-row");

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            SetupCoverageCountSummaryHtml(writer, coverageReports);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            coverageReports.WorkflowNodes.ToList()
                .ForEach(node => node.SetupWorkflowNodeHtml(writer, coverageReports.CoveredWorkflowNodes));

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Br);
        }

        public static void SetupWorkflowReportsHtml(this HtmlTextWriter writer, double CoveragePercentage , string classValue)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, classValue);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            double testFailing = 1 - CoveragePercentage;
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
            else if(IsNodeMocked(coveredNodes, workflowNode))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "#9ACD32");
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

            if (workflowNode.ChildNodes.ToList()?.Count > 0)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "12px");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "next-nodes");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                workflowNode.ChildNodes
                    .ToList()
                    .ForEach(node => SetupNextNodeHTML(writer, coveredNodes, node)); //Should add SetupChildNodeHTML
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

        internal static void SetupLinesCountSummaryHtml(this HtmlTextWriter writer, WarewolfWorkflowReports workflowReports)
        {
            var totalNodes = workflowReports.TotalWorkflowNodesCount;
            var coveredNodes = workflowReports.TotalWorkflowNodesCoveredCount;
            var notCoveredNodes = totalNodes - coveredNodes;

            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "10px 10px 5px 10px");
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
            writer.Write("Total Nodes: " + totalNodes);
            writer.RenderEndTag();

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "green");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-green");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Covered Nodes: " + coveredNodes);
            writer.RenderEndTag();

             
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-red");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Not Covered Nodes: " + notCoveredNodes);
            writer.RenderEndTag();

            var coveragePer = workflowReports.TotalWorkflowNodesCoveredPercentage;

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");

            if (coveragePer <= 65)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
            }
            else if ((coveragePer > 65) && (coveragePer <= 85))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "orange");
            }           
            else if(coveragePer > 85)  
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "green");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-red");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Coverage : " + coveragePer + " %");
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
             
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        internal static void SetupCountSummaryHtml(this HtmlTextWriter writer, List<IServiceTestModelTO> allTests, ICoverageDataObject coverageData)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "10px 10px 0px 10px");
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
            if ((coverageData.IsMultipleWorkflowReport) || (coverageData.ReportName == "*"))
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

        internal static void SetupCoverageCountSummaryHtml(this HtmlTextWriter writer, IWorkflowCoverageReportsTO coverageReports)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "10px 10px 5px 10px");
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
            writer.Write("Total Nodes Count: " + coverageReports.WorkflowNodes.Count());
            writer.RenderEndTag();

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "green");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-green");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            var coveredNodesCount = coverageReports.CoveredWorkflowNodesIds.Count();
            var assertCount = coverageReports.CoveredWorkflowNodesNotMockedIds.Count();
            var mockedCount = coverageReports.CoveredWorkflowNodesMockedIds.Count();

            writer.Write("Covered Nodes: " + coveredNodesCount + "<br> (Assert : " + assertCount + " / <font color='#9ACD32'> Mocked : " + mockedCount + "</font>)");
            writer.RenderEndTag();

            var notCoveredNodesCount = coverageReports.NotCoveredNodesCount;
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            if (notCoveredNodesCount > 0)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "black");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-td-red");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Not Covered Nodes: " + notCoveredNodesCount);
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();

        }

        private static bool IsNodeCovered(IWorkflowNode[] coveredNodes, IWorkflowNode node)
        {
            return coveredNodes.Any(o => o.ActivityID == node.UniqueID && o.MockSelected is false);
        }

        private static bool IsNodeMocked(IWorkflowNode[] coveredNodes, IWorkflowNode node)
        {
            return coveredNodes.Any(o => o.ActivityID == node.UniqueID && o.MockSelected is true);
        }
    }
}
