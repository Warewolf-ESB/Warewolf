using System;
using System.IO;
using System.Xml;

namespace Warewolf.Launcher.TestResultsMergers
{
    class TRXMerger : ITestResultsMerger
    {
        public void MergeRetryResults(string originalResults, string retryResults)
        {
            var trxContent = new XmlDocument();
            trxContent.Load(retryResults);
            var newNamespaceManager = new XmlNamespaceManager(trxContent.NameTable);
            newNamespaceManager.AddNamespace("a", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", newNamespaceManager).Count > 0)
            {
                var originalTrxContent = new XmlDocument();
                originalTrxContent.Load(originalResults);
                var originalNamespaceManager = new XmlNamespaceManager(originalTrxContent.NameTable);
                originalNamespaceManager.AddNamespace("a", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
                foreach (XmlNode TestResult in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", newNamespaceManager))
                {
                    if (TestResult.Attributes["outcome"] == null || TestResult.Attributes["outcome"].InnerText == "Failed")
                    {
                        foreach (XmlNode OriginalTestResult in originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", originalNamespaceManager))
                        {
                            if (OriginalTestResult.Attributes["testName"] != null && TestResult.Attributes["testName"] != null && OriginalTestResult.Attributes["testName"].InnerXml == TestResult.Attributes["testName"].InnerXml)
                            {
                                XmlNode originalOutputNode = OriginalTestResult.SelectSingleNode("//a:Output", originalNamespaceManager);
                                XmlNode newOutputNode = TestResult.SelectSingleNode("//a:Output", newNamespaceManager);
                                if (newOutputNode != null)
                                {
                                    if (originalOutputNode != null)
                                    {
                                        XmlNode originalStdErrNode = originalOutputNode.SelectSingleNode("//a:StdErr", originalNamespaceManager);
                                        XmlNode newStdErrNode = newOutputNode.SelectSingleNode("//a:StdErr", newNamespaceManager);
                                        if (newStdErrNode != null)
                                        {
                                            if (originalStdErrNode != null)
                                            {
                                                originalStdErrNode.InnerText += "\n" + newStdErrNode.InnerText;
                                            }
                                            else
                                            {
                                                originalOutputNode.AppendChild(originalOutputNode.OwnerDocument.ImportNode(newStdErrNode, true));
                                            }
                                        }
                                        XmlNode originalStdOutNode = originalOutputNode.SelectSingleNode("//a:StdOut", originalNamespaceManager);
                                        XmlNode newStdOutNode = newOutputNode.SelectSingleNode("//a:StdOut", newNamespaceManager);
                                        if (newStdOutNode != null)
                                        {
                                            if (originalStdOutNode != null)
                                            {
                                                originalStdOutNode.InnerText += "\n" + newStdOutNode.InnerText;
                                            }
                                            else
                                            {
                                                originalOutputNode.AppendChild(originalOutputNode.OwnerDocument.ImportNode(newStdOutNode, true));
                                            }
                                        }
                                        XmlNode originalErrorInfoNode = originalOutputNode.SelectSingleNode("//a:ErrorInfo", originalNamespaceManager);
                                        XmlNode newErrorInfoNode = newOutputNode.SelectSingleNode("//a:ErrorInfo", newNamespaceManager);
                                        if (newErrorInfoNode != null)
                                        {
                                            if (originalErrorInfoNode != null)
                                            {
                                                XmlNode originalMessageNode = originalErrorInfoNode.SelectSingleNode("//a:Message", originalNamespaceManager);
                                                XmlNode newMessageNode = newErrorInfoNode.SelectSingleNode("//a:Message", newNamespaceManager);
                                                if (newErrorInfoNode != null)
                                                {
                                                    if (originalMessageNode != null)
                                                    {
                                                        originalMessageNode.InnerText += "\n" + newErrorInfoNode.InnerText;
                                                    }
                                                    else
                                                    {
                                                        originalMessageNode.AppendChild(originalMessageNode.OwnerDocument.ImportNode(newErrorInfoNode, true));
                                                    }
                                                }
                                                XmlNode originalStackTraceNode = originalErrorInfoNode.SelectSingleNode("//a:StackTrace", originalNamespaceManager);
                                                XmlNode newStackTraceNode = newErrorInfoNode.SelectSingleNode("//a:StackTrace", newNamespaceManager);
                                                if (newStackTraceNode != null)
                                                {
                                                    if (originalStackTraceNode != null)
                                                    {
                                                        originalStackTraceNode.InnerText += "\n" + newStackTraceNode.InnerText;
                                                    }
                                                    else
                                                    {
                                                        originalStackTraceNode.AppendChild(originalStackTraceNode.OwnerDocument.ImportNode(newStackTraceNode, true));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                originalOutputNode.OwnerDocument.ImportNode(newErrorInfoNode, true);
                                                originalOutputNode.AppendChild(newErrorInfoNode);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        OriginalTestResult.OwnerDocument.ImportNode(newOutputNode, true);
                                        OriginalTestResult.AppendChild(newOutputNode);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode OriginalTestResult in originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", originalNamespaceManager))
                        {
                            if (OriginalTestResult.Attributes["testName"] != null && TestResult.Attributes["testName"] != null && OriginalTestResult.Attributes["testName"].InnerXml == TestResult.Attributes["testName"].InnerXml)
                            {
                                if (OriginalTestResult.Attributes["outcome"] == null)
                                {
                                    var newOutcomeAttribute = originalTrxContent.CreateAttribute("outcome");
                                    newOutcomeAttribute.Value = "Passed";
                                    OriginalTestResult.Attributes.Append(newOutcomeAttribute);
                                }
                                else
                                {
                                    OriginalTestResult.Attributes["outcome"].InnerText = "Passed";
                                }
                                XmlNode originalOutputNode = OriginalTestResult.SelectSingleNode("//a:Output", originalNamespaceManager);
                                XmlNode newOutputNode = TestResult.SelectSingleNode("//a:Output", newNamespaceManager);
                                if (newOutputNode != null)
                                {
                                    if (originalOutputNode != null)
                                    {
                                        XmlNode originalStdErrNode = originalOutputNode.SelectSingleNode("//a:StdErr", originalNamespaceManager);
                                        if (originalStdErrNode != null)
                                        {
                                            try
                                            {
                                                originalOutputNode.RemoveChild(originalStdErrNode);
                                            }
                                            catch (ArgumentException) { }
                                        }
                                        XmlNode originalStdOutNode = originalOutputNode.SelectSingleNode("//a:StdOut", originalNamespaceManager);
                                        XmlNode newStdOutNode = newOutputNode.SelectSingleNode("//a:StdOut", newNamespaceManager);
                                        if (newStdOutNode != null)
                                        {
                                            if (originalStdOutNode != null)
                                            {
                                                originalStdOutNode.InnerText += "\n" + newStdOutNode.InnerText;
                                            }
                                            else
                                            {
                                                originalOutputNode.AppendChild(originalOutputNode.OwnerDocument.ImportNode(newStdOutNode, true));
                                            }
                                        }
                                        XmlNode originalErrorInfoNode = originalOutputNode.SelectSingleNode("//a:ErrorInfo", originalNamespaceManager);
                                        if (originalErrorInfoNode != null)
                                        {
                                            try
                                            {
                                                originalOutputNode.RemoveChild(originalErrorInfoNode);
                                            }
                                            catch (ArgumentException) { }
                                        }
                                    }
                                    else
                                    {
                                        OriginalTestResult.AppendChild(OriginalTestResult.OwnerDocument.ImportNode(newOutputNode, true));
                                    }
                                }
                            }
                        }
                        var countersNodes = originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:ResultSummary/a:Counters", originalNamespaceManager);
                        if (countersNodes.Count > 0)
                        {
                            var countersNode = countersNodes.Item(0);
                            var failuresBefore = int.Parse(countersNode.Attributes["failed"].InnerText);
                            var passesBefore = int.Parse(countersNode.Attributes["passed"].InnerText);
                            if (--failuresBefore <= 0)
                            {
                                var resultsSummaryNodes = originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:ResultSummary", originalNamespaceManager);
                                if (resultsSummaryNodes.Count > 0)
                                {
                                    var resultsSummaryNode = resultsSummaryNodes.Item(0);
                                    resultsSummaryNode.Attributes["outcome"].InnerText = "Completed";
                                }
                            }
                            countersNode.Attributes["failed"].InnerText = failuresBefore.ToString();
                            countersNode.Attributes["passed"].InnerText = (++passesBefore).ToString();
                        }
                    }
                }
                originalTrxContent.Save(originalResults);
                File.Delete(retryResults);
            }
            else
            {
                Console.WriteLine("Error parsing /TestRun/TestDefinitions/UnitTest/TestMethod from trx file at " + retryResults);
            }
        }
    }
}
