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
using Dev2.Interfaces;
using Warewolf.Data;
using Dev2.Common.Interfaces.Runtime.WebServer;
using System;
using Dev2.Data;
using Warewolf.Common.Interfaces.NetStandard20.Wrappers;

namespace Dev2.Runtime.WebServer
{
    public static class ServiceTestCoverageModelHTMLResultBuilder
    {
        public static void SetupNavBarHtml(this IHtmlTextWriterWrapper writer, double totalReportsCoverage)
        {
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Padding, "10px 10px 20px 10px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Margin, "5px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "Roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "28px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "500");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "nav-bar-row");

            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "Roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "28px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "500");
            writer.Write("Coverage Summary:");
            writer.RenderEndTag();

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Margin, "5px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "Roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "28px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "500");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "nav-bar-row");

            writer.AddColorCoding(totalReportsCoverage);

            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
            writer.Write(Math.Round(totalReportsCoverage, 0) +" %");
            writer.RenderEndTag();
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Br.ToString());
        }

        private static void AddColorCoding(this IHtmlTextWriterWrapper writer, double totalReportsCoverage)
        {
            if (IsCoverageRed(totalReportsCoverage))
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "red");
            }
            if (IsCoverageYellow(totalReportsCoverage))
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "orange");
            }
            if (IsCoverageGreen(totalReportsCoverage))
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "green");
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

        public static void SetupWorkflowRowHtml(this IHtmlTextWriterWrapper writer, string resourcePath, ICoverageDataObject coverageData, IWorkflowCoverageReportsTO coverageReports)
        {
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "SetupWorkflowPathHtml");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "#333");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "16px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "20%");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Padding, "8px 16px 5px 8px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Display, "inline-block");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
            writer.Write(resourcePath + "\\" + coverageData.ReportName?.Replace("*", ""));
            writer.RenderEndTag();

            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "SetupWorkflowPathHtml-link");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "100px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "12px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Display, "inline-block");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Target.ToString(), "_new");
            var testUrl = coverageData.GetTestUrl(resourcePath);
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Href.ToString(), testUrl);
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.A.ToString());
            writer.Write("Run Tests");
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.SetupWorkflowReportsHtml(coverageReports.TotalCoverage, nameof(SetupWorkflowReportsHtml));
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "16px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "500");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Margin, "0 0 0 35px");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "workflow-nodes-row");

            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
            SetupCoverageCountSummaryHtml(writer, coverageReports);
            writer.RenderEndTag();

            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());

            coverageReports.WorkflowNodes.ToList()
                .ForEach(node => node.SetupWorkflowNodeHtml(writer, coverageReports.CoveredWorkflowNodes));

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Br.ToString());
        }

        public static void SetupWorkflowReportsHtml(this IHtmlTextWriterWrapper writer, double CoveragePercentage , string classValue)
        {
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), classValue);
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());

            double testFailing = 1 - CoveragePercentage;
            double testPassed = CoveragePercentage;

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Display, "inline-block");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "progress-bar-width");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "80px");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());

            if (testPassed > 0)
            {
                writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "progress-bar-passed");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.BackgroundColor, "green");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Display, "inline-block");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "10px");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "white");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.TextAlign, "center");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Padding, "5px 0px");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Height, "12px");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, testPassed.ToString("0%"));
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
                writer.Write(testPassed.ToString("0%"));
                writer.RenderEndTag();
            }

            if (testFailing > 0)
            {
                writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "progress-bar-failed");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, testFailing.ToString("0%") == "100%" ? "white" : "red");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.BackgroundColor, "red");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Display, "inline-block");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.TextAlign, "center");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "10px");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Padding, "5px 0px");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Height, "12px");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, testFailing.ToString("0%"));
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
                writer.Write(testFailing.ToString("0%") == "100%" ? "0%" : "-");
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
            writer.RenderEndTag();
        }


        public static void SetupWorkflowNodeHtml(this IWorkflowNode workflowNode, IHtmlTextWriterWrapper writer, IWorkflowNode[] coveredNodes)
        {
            if (IsNodeCovered(coveredNodes, workflowNode))
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "green");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "12px");
                writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "step-description-green");
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
                writer.Write(workflowNode.StepDescription);
                writer.RenderEndTag();
            }
            else if(IsNodeMocked(coveredNodes, workflowNode))
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "#9ACD32");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "12px");
                writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "step-description-green");
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
                writer.Write(workflowNode.StepDescription);
                writer.RenderEndTag();
            }
            else
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "red");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "12px");
                writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "step-description-red");
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());
                writer.Write(workflowNode.StepDescription);
                writer.RenderEndTag();
            }

            if (workflowNode.NextNodes?.Count > 0)
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "12px");
                writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "next-nodes");
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());

                workflowNode.NextNodes.ForEach(node => SetupNextNodeHTML(writer, coveredNodes, node));
                writer.RenderEndTag();
            }

            if (workflowNode.ChildNodes.ToList()?.Count > 0)
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "12px");
                writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "next-nodes");
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());

                workflowNode.ChildNodes
                    .ToList()
                    .ForEach(node => SetupNextNodeHTML(writer, coveredNodes, node)); //Should add SetupChildNodeHTML
                writer.RenderEndTag();
            }
        }

        public static void SetupNextNodeHTML(IHtmlTextWriterWrapper writer, IWorkflowNode[] coveredNodes, IWorkflowNode node)
        {
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "next-node");

            if (IsNodeCovered(coveredNodes, node))
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "green");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "12px");
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Li.ToString());
            }
            else
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "red");
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "12px");
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Li.ToString());
            }

            writer.Write(node.StepDescription);
            writer.RenderEndTag();
        }

        internal static void SetupLinesCountSummaryHtml(this IHtmlTextWriterWrapper writer, WarewolfWorkflowReports workflowReports)
        {
            var totalNodes = workflowReports.TotalWorkflowNodesCount;
            var coveredNodes = workflowReports.TotalWorkflowNodesCoveredCount;
            var notCoveredNodes = totalNodes - coveredNodes;

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Padding, "10px 10px 5px 10px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Margin, "5px");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "count-summary row");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Margin, "0 -15px 0 -15px");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Table.ToString());
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Tr.ToString());

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "black");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-black");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Total Nodes: " + totalNodes);
            writer.RenderEndTag();

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "green");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-green");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Covered Nodes: " + coveredNodes);
            writer.RenderEndTag();

             
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "red");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-red");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Not Covered Nodes: " + notCoveredNodes);
            writer.RenderEndTag();

            var coveragePer = workflowReports.TotalWorkflowNodesCoveredPercentage;

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto san.ToString()s-serif");

            if (coveragePer <= 65)
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "red");
            }
            else if ((coveragePer > 65) && (coveragePer <= 85))
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "orange");
            }           
            else if(coveragePer > 85)  
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "green");
            }
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-red");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Coverage : " + coveragePer + " %");
            writer.RenderEndTag();

            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
             
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        internal static void SetupCountSummaryHtml(this IHtmlTextWriterWrapper writer, List<IServiceTestModelTO> allTests, ICoverageDataObject coverageData)
        {
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Padding, "10px 10px 0px 10px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Margin, "5px");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "count-summary row");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Margin, "0 -15px 0 -15px");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Table.ToString());
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Tr.ToString());

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "black");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-black");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Total Test Count: " + allTests.Count);
            writer.RenderEndTag();

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "green");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-green");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Tests Passed: " + allTests.Count(o => o.TestPassed));
            writer.RenderEndTag();

            var failedCount = allTests.Count(o => o.TestFailing);
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            if (failedCount > 0)
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "red");
            }
            else
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "black");
            }
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-red");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Tests Failed: " + failedCount);
            writer.RenderEndTag();

            var invalidCount = allTests.Count(o => o.TestInvalid);
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            if (invalidCount > 0)
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "orange");
            }
            else
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "black");
            }
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-red");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Tests Invalid: " + invalidCount);
            writer.RenderEndTag();

            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            if ((coverageData.IsMultipleWorkflowReport) || (coverageData.ReportName == "*"))
            {
                writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Target.ToString(), "_new");
                writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Href.ToString(), coverageData.GetAllTestsUrl());
                writer.RenderBeginTag(WarewolfHtmlTextWriterTag.A.ToString());
                writer.Write("Run All Tests");
                writer.RenderEndTag();
            }
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        internal static void SetupCoverageCountSummaryHtml(this IHtmlTextWriterWrapper writer, IWorkflowCoverageReportsTO coverageReports)
        {
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Padding, "10px 10px 5px 10px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Margin, "5px");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "count-summary row");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Div.ToString());

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Margin, "0 -15px 0 -15px");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Table.ToString());
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Tr.ToString());

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "black");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-black");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Total Nodes Count: " + coverageReports.WorkflowNodes.Count());
            writer.RenderEndTag();

            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "green");
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-green");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());

            var coveredNodesCount = coverageReports.CoveredWorkflowNodesIds.Count();
            var assertCount = coverageReports.CoveredWorkflowNodesNotMockedIds.Count();
            var mockedCount = coverageReports.CoveredWorkflowNodesMockedIds.Count();

            writer.Write("Covered Nodes: " + coveredNodesCount + "<br> (Assert : " + assertCount + " / <font color='#9ACD32'> Mocked : " + mockedCount + "</font>)");
            writer.RenderEndTag();

            var notCoveredNodesCount = coverageReports.NotCoveredNodesCount;
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Width, "200px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontWeight, "bold");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontSize, "14px");
            writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.FontFamily, "roboto sans-serif");
            if (notCoveredNodesCount > 0)
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "red");
            }
            else
            {
                writer.AddStyleAttribute(WarewolfHtmlTextWriterStyle.Color, "black");
            }
            writer.AddAttribute(WarewolfHtmlTextWriterAttribute.Class.ToString(), "table-td-red");
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
            writer.Write("Not Covered Nodes: " + notCoveredNodesCount);
            writer.RenderEndTag();
            writer.RenderBeginTag(WarewolfHtmlTextWriterTag.Td.ToString());
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
