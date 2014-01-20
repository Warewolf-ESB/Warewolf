// ReSharper disable PossibleNullReferenceException
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace TRXMerge
{
    class Program
    {

        static int Main(string[] args)
        {
            const string Message = "\nUsage: TRXMerge <output XML file> <first trx file> <second trx file> <third trx file> ... etc\n\nInfo: Changes from one file will be overwritten on the next if there are matching test cases";
            if(args.Length < 3)
            {
                Console.WriteLine(Message);
                return 1;
            }
            if(!File.Exists(args[1])) { Console.WriteLine(Message); return 1; }
            System.Xml.XmlDocument oDocFirst = new XmlDocument();
            oDocFirst.Load(MakeCompatXML(args[1]));

            int count = 2;

            while(count <= args.Length - 1)
            {
                if(!File.Exists(args[count])) { Console.WriteLine(Message); return 1; }
                var oDocSecond = new XmlDocument();
                oDocSecond.Load(MakeCompatXML(args[count]));

                ////locate sections in first and append data from next...
                var oNodeWhereInsert = oDocFirst.SelectSingleNode("//TestDefinitions");

                var i = 0;
                while(oDocSecond.SelectSingleNode("//TestDefinitions").ChildNodes.Count != i)
                {
                    ////insert test only if it is not already present
                    if(!IfTestExists(oDocFirst, oDocSecond.SelectSingleNode("//TestDefinitions").ChildNodes[i].Attributes["name"].Value))
                    {
                        oNodeWhereInsert.AppendChild(oDocFirst.ImportNode(oDocSecond.SelectSingleNode("//TestDefinitions").ChildNodes[i], true));
                    }
                    i++;
                }

                ////insert new results and update existing if present
                var oNodeWhereInsertResult = oDocFirst.SelectSingleNode("//Results");
                i = 0;
                while(oDocSecond.SelectSingleNode("//Results").ChildNodes.Count != i)
                {
                    XmlNode oldNode;

                    if(IfResultExists(oDocFirst, oDocSecond.SelectSingleNode("//Results").ChildNodes[i].Attributes["testName"].Value, out oldNode))
                    {
                        oNodeWhereInsertResult.RemoveChild(oldNode);
                        oNodeWhereInsertResult.AppendChild(oDocFirst.ImportNode(oDocSecond.SelectSingleNode("//Results").ChildNodes[i], true));
                    }
                    else
                    {
                        oNodeWhereInsertResult.AppendChild(oDocFirst.ImportNode(oDocSecond.SelectSingleNode("//Results").ChildNodes[i], true));
                    }
                    i++;
                }

                count++;
            }

            if(File.Exists(args[0]))
            {
                File.Delete(args[0]);
            }
            oDocFirst.Save(args[0]);

            SetSummary(args[0]);
            return 0;
        }

        public static summary GetSummary(XmlDocument doc)
        {
            summary s = new summary { total = -1, executed = -1, passed = -1 };

            XmlNode nTotal = doc.SelectNodes("//Counters/@total").Item(0);
            s.total = Convert.ToInt32(nTotal.InnerText);
            XmlNode nPass = doc.SelectNodes("//Counters/@passed").Item(0);
            s.passed = Convert.ToInt32(nPass.InnerText);
            XmlNode nExecuted = doc.SelectNodes("//Counters/@executed").Item(0);
            s.executed = Convert.ToInt32(nExecuted.InnerText);
            XmlNode nStart = doc.SelectNodes("//Times/@start ").Item(0);
            DateTime start = Convert.ToDateTime(nStart.InnerText);
            XmlNode nEnd = doc.SelectNodes("//Times/@finish").Item(0);
            DateTime end0 = Convert.ToDateTime(nEnd.InnerText);
            s.time = ((end0 - start).TotalSeconds);

            return s;
        }

        public static string MakeCompatXML(string input)
        {
            //change the first tag to have no attributes - VSTS2008
            StringBuilder newFile = new StringBuilder();
            string[] file = File.ReadAllLines(input);

            foreach(string line in file)
            {
                if(line.Contains("<TestRun id"))
                {
                    string nStr = line.Substring(0, 8) + ">";
                    string temp = line.Replace(line, nStr);
                    newFile.Append(temp + "\r");
                    continue;
                }

                newFile.Append(line + "\r");
            }

            File.WriteAllText(input, newFile.ToString());

            return input;
        }

        public static bool IfTestExists(XmlDocument doc, string testName)
        {
            int i = 0;
            while(doc.SelectSingleNode("//TestDefinitions").ChildNodes.Count != i)
            {
                if(doc.SelectSingleNode("//TestDefinitions").ChildNodes[i].Attributes["name"].Value == testName)
                {
                    return true;
                }
                i++;
            }
            return false;
        }

        public static bool IfResultExists(XmlDocument doc, string testName, out XmlNode oldNode)
        {
            int i = 0;
            while(doc.SelectSingleNode("//Results").ChildNodes.Count != i)
            {
                if(doc.SelectSingleNode("//Results").ChildNodes[i].Attributes["testName"].Value == testName)
                {
                    oldNode = doc.SelectSingleNode("//Results").ChildNodes[i];
                    return true;
                }
                i++;
            }
            oldNode = null;
            return false;
        }

        public static void SetSummary(string fileName)
        {
            XmlDocument oDoc = new XmlDocument();
            oDoc.Load(fileName);

            XmlNode master = oDoc.SelectSingleNode("//ResultSummary/Counters");

            summary oSummary;
            oSummary.passed = 0;

            //count the number of test cases for total
            oSummary.total = oDoc.SelectSingleNode("//TestDefinitions").ChildNodes.Count;

            //count the number of test cases executed from count of test results
            oSummary.executed = oDoc.SelectSingleNode("//Results").ChildNodes.Count;

            //count the number of passed test cases from results
            int i = 0;
            while(oDoc.SelectSingleNode("//Results").ChildNodes.Count != i)
            {
                if(oDoc.SelectSingleNode("//Results").ChildNodes[i].Attributes["outcome"].Value == "Passed")
                {
                    oSummary.passed++;
                }
                i++;
            }

            ////update summary with new numbers
            master.Attributes["total"].Value = oSummary.total.ToString();
            master.Attributes["executed"].Value = oSummary.executed.ToString();
            master.Attributes["passed"].Value = oSummary.passed.ToString();

            ////locate and update times
            XmlNode oTimes = oDoc.SelectSingleNode("//Times");
            //add a new attribute elapsed time, original trx would not have that
            XmlAttribute elapsed = oDoc.CreateAttribute("elapsedtime");
            elapsed.Value = CalculateSummaryTime(oDoc).ToString();
            oTimes.Attributes.Append(elapsed);

            ////append ID
            XmlNode oRoot = oDoc.DocumentElement;
            var id = oDoc.CreateAttribute("id");
            id.Value = Guid.NewGuid().ToString();
            oRoot.Attributes.Append(id);

            ////append XML namespace
            var xmlns = oDoc.CreateAttribute("xmlns");
            id.Value = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
            oRoot.Attributes.Append(xmlns);

            oDoc.Save(fileName);
        }

        public static double CalculateSummaryTime(XmlDocument doc)
        {
            double sDiff = 0;

            int i = 0;

            while(doc.SelectSingleNode("//Results").ChildNodes.Count != i)
            {
                DateTime sTime = Convert.ToDateTime(doc.SelectSingleNode("//Results").ChildNodes[i].Attributes["startTime"].Value);
                DateTime eTime = Convert.ToDateTime(doc.SelectSingleNode("//Results").ChildNodes[i].Attributes["endTime"].Value);
                //// calculate timespan for each test case and add them to calculate total time
                sDiff += (eTime - sTime).TotalSeconds;
                i++;
            }
            return sDiff;
        }
    }

    struct summary
    {
        public int total;
        public int executed;
        public int passed;
        public double time;
    }
}
