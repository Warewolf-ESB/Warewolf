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
                ImportTestResults(originalNamespaceManager, newNamespaceManager, originalTrxContent, trxContent);
                originalTrxContent.Save(originalResults);
                File.Delete(retryResults);
            }
            else
            {
                Console.WriteLine("Error parsing /TestRun/TestDefinitions/UnitTest/TestMethod from trx file at " + retryResults);
            }
        }

        void ImportTestResults(XmlNamespaceManager originalNamespaceManager, XmlNamespaceManager newNamespaceManager, XmlDocument originalTrxContent, XmlDocument newTrxContent)
        {
            foreach (XmlNode TestResult in newTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", newNamespaceManager))
            {
                if (TestResult.Attributes["outcome"] == null || TestResult.Attributes["outcome"].InnerText == "Failed")
                {
                    ImportAllFailingNodes(originalNamespaceManager, newNamespaceManager, originalTrxContent, TestResult);
                }
                else
                {
                    ImportAllPassingTestNodes(originalNamespaceManager, newNamespaceManager, originalTrxContent, TestResult);
                }
            }
        }

        void ImportAllPassingTestNodes(XmlNamespaceManager originalNamespaceManager, XmlNamespaceManager newNamespaceManager, XmlDocument originalTrxContent, XmlNode TestResult)
        {
            foreach (XmlNode OriginalTestResult in originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", originalNamespaceManager))
            {
                if (OriginalTestResult.Attributes["testName"] != null && TestResult.Attributes["testName"] != null && OriginalTestResult.Attributes["testName"].InnerXml == TestResult.Attributes["testName"].InnerXml)
                {
                    Add1PassedToCounters(originalNamespaceManager, originalTrxContent, OriginalTestResult.Attributes["outcome"]?.InnerText);
                    SetOutcomePassed(originalNamespaceManager, originalTrxContent, OriginalTestResult);
                    ImportStdOutNode(originalNamespaceManager, newNamespaceManager, OriginalTestResult, TestResult);
                }
            }
        }

        void Add1PassedToCounters(XmlNamespaceManager originalNamespaceManager, XmlDocument originalTrxContent, string OriginalTestResult)
        {
            if (OriginalTestResult == "Failed")
            {
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

        void ImportAllFailingNodes(XmlNamespaceManager originalNamespaceManager, XmlNamespaceManager newNamespaceManager, XmlDocument originalTrxContent, XmlNode TestResult)
        {
            foreach (XmlNode OriginalTestResult in originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", originalNamespaceManager))
            {
                if (OriginalTestResult.Attributes["testName"] != null && TestResult.Attributes["testName"] != null && OriginalTestResult.Attributes["testName"].InnerXml == TestResult.Attributes["testName"].InnerXml)
                {
                    ImportOutputNode(originalNamespaceManager, newNamespaceManager, OriginalTestResult, TestResult);
                }
            }
        }

        void SetOutcomePassed(XmlNamespaceManager namespaceManager, XmlDocument doc, XmlNode node)
        {
            if (node.Attributes["outcome"] == null)
            {
                var newOutcomeAttribute = doc.CreateAttribute("outcome");
                newOutcomeAttribute.Value = "Passed";
                node.Attributes.Append(newOutcomeAttribute);
                Add1ExecutedToCounters(namespaceManager, doc);
            }
            else
            {
                node.Attributes["outcome"].InnerText = "Passed";
            }
        }

        void Add1ExecutedToCounters(XmlNamespaceManager namespaceManager, XmlDocument doc)
        {
            var countersNodes = doc.DocumentElement.SelectNodes("/a:TestRun/a:ResultSummary/a:Counters", namespaceManager);
            if (countersNodes.Count > 0)
            {
                var countersNode = countersNodes.Item(0);
                var executedBefore = int.Parse(countersNode.Attributes["executed"].InnerText);
                var passesBefore = int.Parse(countersNode.Attributes["passed"].InnerText);
                countersNode.Attributes["executed"].InnerText = (++executedBefore).ToString();
                countersNode.Attributes["passed"].InnerText = (++passesBefore).ToString();
            }
        }

        void ImportStdOutNode(XmlNamespaceManager originalNamespaceManager, XmlNamespaceManager newNamespaceManager, XmlNode originalNode, XmlNode newNode)
        {
            XmlNode originalOutputNode = originalNode.SelectSingleNode("//a:Output", originalNamespaceManager);
            XmlNode newOutputNode = newNode.SelectSingleNode("//a:Output", newNamespaceManager);
            if (newOutputNode != null)
            {
                if (originalOutputNode != null)
                {
                    RemoveChildNode(originalNamespaceManager, originalOutputNode, "//a:StdErr");
                    RemoveChildNode(originalNamespaceManager, originalOutputNode, "//a:ErrorInfo");
                    ImportSingleNode(originalNamespaceManager, newNamespaceManager, originalOutputNode, newOutputNode, "//a:StdOut");
                }
                else
                {
                    originalNode.AppendChild(originalNode.OwnerDocument.ImportNode(newOutputNode, true));
                }
            }
        }

        void ImportOutputNode(XmlNamespaceManager originalNamespaceManager, XmlNamespaceManager newNamespaceManager, XmlNode originalNode, XmlNode newNode)
        {
            XmlNode originalOutputNode = originalNode.SelectSingleNode("//a:Output", originalNamespaceManager);
            XmlNode newOutputNode = newNode.SelectSingleNode("//a:Output", newNamespaceManager);
            if (newOutputNode != null)
            {
                if (originalOutputNode != null)
                {
                    ImportSingleNode(originalNamespaceManager, newNamespaceManager, originalOutputNode, newOutputNode, "//a:StdErr");
                    ImportSingleNode(originalNamespaceManager, newNamespaceManager, originalOutputNode, newOutputNode, "//a:StdOut");
                    ImportErrorNode(originalNamespaceManager, newNamespaceManager, originalOutputNode, newOutputNode);
                }
                else
                {
                    originalNode.OwnerDocument.ImportNode(newOutputNode, true);
                    originalNode.AppendChild(newOutputNode);
                }
            }
        }

        void RemoveChildNode(XmlNamespaceManager parentNamespaceManager, XmlNode parentNode, string nodeXpath)
        {
            XmlElement removeNode = (XmlElement)parentNode.SelectSingleNode(nodeXpath, parentNamespaceManager);
            if (removeNode != null)
            {
                removeNode.ParentNode.RemoveChild(removeNode);
            }
        }

        void ImportErrorNode(XmlNamespaceManager originalNamespaceManager, XmlNamespaceManager newNamespaceManager, XmlNode originalNode, XmlNode newNode)
        {
            XmlNode originalErrorInfoNode = originalNode.SelectSingleNode("//a:ErrorInfo", originalNamespaceManager);
            XmlNode newErrorInfoNode = newNode.SelectSingleNode("//a:ErrorInfo", newNamespaceManager);
            if (newErrorInfoNode != null)
            {
                if (originalErrorInfoNode != null)
                {
                    ImportSingleNode(originalNamespaceManager, newNamespaceManager, originalNode, newNode, "//a:Message");
                    ImportSingleNode(originalNamespaceManager, newNamespaceManager, originalNode, newNode, "//a:StackTrace");
                }
                else
                {
                    originalNode.AppendChild(originalNode.OwnerDocument.ImportNode(newErrorInfoNode, true));
                }
            }
        }

        void ImportSingleNode(XmlNamespaceManager originalNamespaceManager, XmlNamespaceManager newNamespaceManager, XmlNode originalNode, XmlNode newNode, string SingleNodeXpath)
        {
            XmlNode originalStdErrNode = originalNode.SelectSingleNode(SingleNodeXpath, originalNamespaceManager);
            XmlNode importNode = newNode.SelectSingleNode(SingleNodeXpath, newNamespaceManager);
            if (importNode != null)
            {
                if (originalStdErrNode != null)
                {
                    originalStdErrNode.InnerText += "\n" + importNode.InnerText;
                }
                else
                {
                    originalNode.AppendChild(originalNode.OwnerDocument.ImportNode(importNode, true));
                }
            }
        }
    }
}
