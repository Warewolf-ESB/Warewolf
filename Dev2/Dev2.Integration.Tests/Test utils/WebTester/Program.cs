using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections.ObjectModel;

namespace Dev2.Integration.Tests.MEF.WebTester
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        static void Main(string[] args)
        {
            //GetArgs(args);
            Start();
        }

        //private EventHandler DoSomething(EventArgs args)
        //{
            
        //}

        public static void GetArgs(string[] args)
        {

            string sPattern = @"\w[a-zA-Z]";
            Regex regex = new Regex(sPattern);
            foreach (string arg in args)
            {
                if (arg == "-f")
                {
                    
                }
            }
            
            return;
        }


        static void Start()
        {
            //LoadTestWorker worker = new LoadTestWorker();
            //worker.DoWork();
            //AsynchronousRequest async = new AsynchronousRequest();
            //ReadData readData = new ReadData();
            //readData.OpenDoc();
            //DataTable functionData = readData.GetDataTable();
            //GetPerformableActions(functionData);
            //Console.ReadLine();
        }

        public static void GetPerformableActions(DataTable data)
        {
            GetWorker getWorker = null;
            PostWorker postWorker = null;
            foreach (DataRow row in data.Rows)
                switch (row.ItemArray[1].ToString())
                {
                    case "POST":
                        //Do Nothing
                        string postURL = row.ItemArray[2].ToString();
                        postWorker = new PostWorker(postURL);
                        postWorker.DoWork();
                        break;
                    case "GET":
                        //Requires my implementation
                        string getUrl = row.ItemArray[2].ToString();
                        getWorker = new GetWorker(getUrl);
                        getWorker.DoWork();
                        Console.WriteLine(getWorker.GetResponseData());
                        Console.WriteLine("---------------------------------------------------------------------------------");
                        Console.WriteLine();
                        break;
                    case "Load":
                        // get the filepath
                        string path = row.ItemArray[2].ToString();
                        LoadTestWorker loadtest = new LoadTestWorker(path);
                        loadtest.DoWork();

                        break;
                    case "Compare":
                        // Do Nothing
                        string compareData = row.ItemArray[2].ToString();
                        getWorker.Compare(compareData);
                        break;
                    case "AutomationFramework":
                        StartAutomationWorker(row.ItemArray[2].ToString(), row.ItemArray[3].ToString(), row.ItemArray[4].ToString());
                        break;     
                }
        }

        private static void StartAutomationWorker(string ControlType, string AutomationId, string function)
        {
            //AutomationWorker automationCall = null;
            //if (function == "Click" || function == "click")
            //{
            //    if (ControlType == "Button" || ControlType == "Button")
            //        automationCall = new AutomationWorker(AutomationFramework.AutomationWorkerType.ButtonManipulator, AutomationId);
            //    else if (ControlType == "TextBox" || ControlType == "textBox")
            //        automationCall = new AutomationWorker(AutomationFramework.AutomationWorkerType.TextReader, AutomationId);
            //}
            //automationCall.DoWork();

        }


        //private static void GetAssemblyMetadata()
        //{
        //    string path = @"..\plugins\";
        //    string LibName = "ExcelInteraction.dll";
        //    string action = "Action";

        //    Assembly myAssembly = null;
        //    string fullpath = Path.GetFullPath(path);

        //    try 
        //    {
        //        myAssembly = Assembly.LoadFile(fullpath + LibName);
        //    }
        //    catch (Exception fex)
        //    {
        //        throw fex;
        //    }

        //    Type[] types = myAssembly.GetTypes();
        //    foreach (Type t in types)
        //    {
        //        Console.WriteLine(t.GetMethods());
        //        MethodInfo[] methods = t.GetMethods();
        //        foreach (MethodInfo method in methods)
        //            Console.WriteLine("Method Name: {0}, Method Vars: {1}", method.Name, method.GetParameters());
        //    }

            
        //}
    }  


}
