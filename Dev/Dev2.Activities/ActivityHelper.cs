using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Dev2;
using System.ServiceModel;
using Microsoft.JScript;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    /// <summary>
    /// Helper static class for the activity execution inside a workflow
    /// </summary>
    public static class ActivityHelper {

        #region Old Helper Methods

        /*public static IEnumerable<string> EnumTextStreamLines(StreamReader stream) {

            while (!stream.EndOfStream) {
                var lineData = stream.ReadLine();
                yield return lineData + "\r\n";
            }

            stream.Close();
        }

        public static string RecursiveDescentScanner(string dataSource, string transform) {
            string replacexml = transform.Replace("[[", "<x>").Replace("]]", "</x>");

            var xel = new XElement("a");
            try {
                xel = XElement.Parse("<a>" + replacexml + "</a>");
            }
            catch {
                return "Syntax Error";
            }

            var resShape = xel.DescendantsAndSelf("x");

            List<string> tokens = new List<string>();

            resShape.Reverse().ToList().ForEach(c => tokens.Add(c.ToString().Replace(" ", string.Empty).Replace("\r\n", string.Empty).Replace("<x>", "[[").Replace("</x>", "]]")));

            string prevData = string.Empty;
            string prevToken = string.Empty;

            foreach (var data in tokens) {
                if (!string.IsNullOrEmpty(prevData)) {
                    prevData = data.Replace(prevToken, prevData);
                }
                prevToken = data;
                prevData = RegionParser(dataSource, string.IsNullOrEmpty(prevData) ? data : prevData);
            }

            return prevData;
        }

        public static string RegionParser(string dataSource, string transform) {
            if (transform.Equals("[[]]")) {
                return string.Empty;
            }


            dynamic data = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(dataSource);

            string pattern = @"(?<tagonly>(\[\[)(?<tag>[0-9a-zA-z]+)(\]\]))|(?<xpathonly>(\[\[)xpath\((?<xpathexpression>.*)\)(\]\]))";

            MatchCollection matches = Regex.Matches(transform, pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

            foreach (Match patternMatch in matches) {
                var tag = patternMatch.Groups["tag"].Value;
                var xPathExpression = patternMatch.Groups["xpathexpression"].Value.Replace("'", ""); ;
                var xPathOnlyMatch = patternMatch.Groups["xpathonly"].Value;
                var tagWithXPathMatch = patternMatch.Groups["tagwithxpath"].Value;
                var tagOnlyMatch = patternMatch.Groups["tagonly"].Value;
                var databindingExpression = patternMatch.Groups["databindingExpression"].Value;
                var dataExpression = patternMatch.Groups["dataExpression"].Value;

                string result = string.Empty;

                Stopwatch s = new Stopwatch();
                string msg = string.Empty;
                //This is an xpathonly expression
                if (string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(xPathExpression)) {
                    s.Start();
                    if (!xPathExpression.Contains("[[")) {
                        result = data.XPath(xPathExpression).Inner();
                    }
                    s.Stop();
                    msg = string.Format("{0} XPath {1}", s.ElapsedMilliseconds, xPathExpression);
                    TraceWriter.WriteTrace(msg);
                    s.Reset();
                    
                    transform = transform.Replace(xPathOnlyMatch, result);
                } 

                //this is a tagwithxpath match
                if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(xPathExpression)) {
                    result = data.XPath(string.Format("//{0}", tag)).XPath(xPathExpression).Inner();
                    transform = transform.Replace(tagWithXPathMatch, result);
                }

                //This is a standard data region
                if (!string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(xPathExpression)) {
                    //result = data.XPath(string.Format("//{0}", tag)).Inner();
                    s.Start();
                    result = data.GetValue(tag);
                    s.Stop();
                    msg = string.Format("{0} Get Value for [[{1}]]", s.ElapsedMilliseconds, tag);
                    TraceWriter.WriteTrace(msg);
                    s.Reset();
                    transform = transform.Replace(tagOnlyMatch, result);
                    
                }

            }
            return transform;
        }

        public static string TextAndJScriptRegionEvaluator(List<string> ambientDataList, string transform, string currentValue = "", bool dataBindRecursive = false, string rootService = "") {

            if (string.IsNullOrEmpty(transform)) {
                return string.Empty;
            }

            if (!transform.Contains("[[") && !transform.Contains("{{")) {
                return BindEnvironmentVariables(transform, rootService);
            }

            Stopwatch s = new Stopwatch();
            s.Start();
            var data = DataListToUnlimitedObject(ambientDataList);
            s.Stop();

            string message = string.Format("{0}ms: DataListToUnlimitedObject", s.ElapsedMilliseconds.ToString());
            s.Reset();

            TraceWriter.WriteTrace(message);

            s.Start();
            List<DataRegion> regions = RecursiveDescentParser(transform);
            s.Stop();


            message = string.Format("{0}ms: End Region Parsing", s.ElapsedMilliseconds.ToString());
            TraceWriter.WriteTrace(message);
            s.Reset();

            s.Start();
            regions
                .ForEach(c => {
                    var dat = data.XmlString;
                    RecursiveDescentEvaluator(c, dat);
                });
            s.Stop();
            message = string.Format("{0}ms: End Region Evaluation", s.ElapsedMilliseconds.ToString());
            TraceWriter.WriteTrace(message);
            s.Reset();

            s.Start();
            //Get all root level items and then to the substitution;
            var roots = regions.Where(c => c.IsRootLevel);
            if (roots.Count() > 0) {
                roots
                    .OrderByDescending(a => a.RegionData.ToString().Length)
                    .ToList()
                    .ForEach(d => {
                        transform = transform.Replace(d.RegionData.ToString(), d.Value);
                    });
            }
            s.Stop();
            message = string.Format("{0}ms: End Region Substitution", s.ElapsedMilliseconds.ToString());
            TraceWriter.WriteTrace(message);
            s.Reset();


            if (transform.Contains("{{")) {
                string codepattern = @"(?<code>\{\{.*\}\})";
                string jScriptCode = string.Empty;
                Match match = Regex.Match(transform, codepattern, RegexOptions.Singleline);
                if (match.Success) {
                    jScriptCode = match.Value;

                    if (!string.IsNullOrEmpty(jScriptCode)) {
                        s.Start();
                        string returnVal = JScriptEvaluator.EvaluateToString(jScriptCode.Replace("\r\n", string.Empty).Replace("\n", string.Empty));
                        s.Stop();

                        message = string.Format("{0}ms: End JScriptEval", s.ElapsedMilliseconds.ToString());
                        TraceWriter.WriteTrace(message);

                        transform = transform.Replace(match.Value, returnVal).Trim();
                    }
                }
            }

            if (transform.Equals(currentValue)) {
                return transform;
            }

            if (dataBindRecursive) {
                if (transform.Contains("[[") || transform.Contains("{{")) {
                    transform = TextAndJScriptRegionEvaluator(ambientDataList, transform, transform, true, rootService);
                }
            }

            return BindEnvironmentVariables(transform, rootService);
        }


        public static string BindEnvironmentVariables(string transform, string rootServiceName="") {
            if (string.IsNullOrEmpty(transform)) {
                return transform;
            }

            transform = transform.Replace("@AppPath", Environment.CurrentDirectory);
            transform = transform.Replace("@ServiceName", rootServiceName);
            transform = transform.Replace("@OSVersion", Environment.OSVersion.VersionString);

            return transform;
        }


        private static List<DataRegion> RecursiveDescentParser(string transform) {
            StringReader s = new StringReader(transform);

            List<DataRegion> regions = new List<DataRegion>();
            StringBuilder regionData = new StringBuilder();
            DataRegion lastDataRegion = null;

            while (s.Peek() != -1) {


                var lastRegionList = regions.Where(c => c.IsOpen);
                if (lastRegionList.Count() > 0) {
                    lastDataRegion = lastRegionList.Last();
                }
                else {
                    lastDataRegion = null;
                }

                char ch = (char)s.Peek();

                char nxtch = '\0';

                //if (char.IsWhiteSpace(ch)) {
                //    s.Read();
                //}

                if (ch == '[') {
                    s.Read();

                    nxtch = (char)s.Peek();

                    if (nxtch == '[') {
                        //Region Starting
                        DataRegion d = new DataRegion { IsOpen = true };
                        if (lastDataRegion != null) {
                            d.Parent = lastDataRegion;
                            d.Parent.Child = d;
                            //lastDataRegion.RegionData.Append("$DEV2$");
                        }

                        regions.Add(d);
                        d.RegionData.Append(new char[] { ch, ch });
                        s.Read();
                    }
                    else {
                        if (lastDataRegion != null) {
                            lastDataRegion.RegionData.Append(ch);
                        }
                    }

                }
                else {
                    if (ch == ']') {
                        s.Read();

                        nxtch = (char)s.Peek();

                        if (nxtch == ']') {


                            if (lastDataRegion != null) {
                                //Region Closing
                                lastDataRegion.RegionData.Append(new char[] { ch, ch });

                                lastDataRegion.IsOpen = false;

                                string regionString = lastDataRegion.RegionData.ToString();

                                if (lastDataRegion.Parent != null) {
                                    lastDataRegion.Parent.RegionData.Append(regionString);
                                }
                            }

                            s.Read();
                        }
                        else {
                            if (lastDataRegion != null) {
                                lastDataRegion.RegionData.Append(ch);
                            }
                        }
                    }
                    else {
                        //This data needs to be appended to the last open region
                        if (lastDataRegion != null) {
                            lastDataRegion.RegionData.Append(ch);
                        }
                        s.Read();
                    }
                }
            }
            return regions;
        }

        public static void RecursiveDescentEvaluator(DataRegion region, string dataSource) {

            if (region.IsTokenGenerated) {
                return;
            }

            var s = Stopwatch.StartNew();
            var current = region;

            //This is a standard dataregion with no embedded data regions
            
            if (!current.HasChild && !current.HasParent) {
                current.RootLevelToken = current.RegionData.ToString();
                current.Value = ActivityHelper.RegionParser(dataSource, current.RootLevelToken.Replace("\r\n", string.Empty).Replace("\t", string.Empty));
                //current.IsTokenGenerated = true;
                current.IsRootLevel = true;
                return;
            }
            s.Stop();
            string message = string.Format("{0}: Set value of singleton Top Level node", s.ElapsedMilliseconds.ToString());
            TraceWriter.WriteTrace(message);
            s.Reset(); 

            //We are traversing the object graph down to the last level
            //so we can work from the innermost child up for tree
            s = Stopwatch.StartNew();
            while (true) {
                if (current.Child == null) break;
                current = current.Child;
            }
            s.Stop();
            message = string.Format("{0}: Recursive Descend to innermost child", s.ElapsedMilliseconds.ToString());
            TraceWriter.WriteTrace(message);
            s.Reset(); 

            s.Start();
            if (!current.HasChild) {
                current.RootLevelToken = current.RegionData.ToString();
                current.Value = ActivityHelper.RegionParser(dataSource, current.RootLevelToken);
                //current.IsTokenGenerated = true;
            }
            s.Stop();
            message = string.Format("{0}: Set Current Value For Parent Binding", s.ElapsedMilliseconds.ToString());
            TraceWriter.WriteTrace(message);
            s.Reset(); 

            s.Start();
            while (current.Parent != null) {
                string parentData = string.IsNullOrEmpty(current.Parent.RootLevelToken) ? current.Parent.RegionData.ToString() : current.Parent.RootLevelToken;
                string myData = current.RootLevelToken;
                string myDataValue = current.Value;

                current.Parent.RootLevelToken = parentData.Replace(myData, myDataValue);//.Replace("$DEV2$", myData);
                current.Parent.Value = parentData.Replace(myData, myDataValue);
                //current.Parent.IsTokenGenerated = true;
                current.Parent.Value = ActivityHelper.RegionParser(dataSource, current.Parent.Value);

                current = current.Parent;
                if (current.Parent == null) {
                    current.IsRootLevel = true;
                }


            }
            s.Stop();
            message = string.Format("{0}: Ascend to toplevel parent", s.ElapsedMilliseconds.ToString());
            TraceWriter.WriteTrace(message);
            s.Reset(); ;


        }

        public static string ParseDataRegionTokens(List<string> ambientDataList, string transform) {
            dynamic data = DataListToUnlimitedObject(ambientDataList);

            if (string.IsNullOrEmpty(transform)) {
                return string.Empty;
            }

            string pattern = @"^(\[\[)(.*?)([^/]+\]\])$";
            //Extract all data region tokens at their highest level
            //We will be recursively descending into their children if applicable 
            //at a later stage
            MatchCollection matches = Regex.Matches(transform, pattern, RegexOptions.Multiline);
            foreach (Match patternMatch in matches) {
                //Pass data region token to recursive descent parser with data to be used
                string result = RecursiveDescentScanner(data.XmlString, patternMatch.Value);

                transform = transform.Replace(patternMatch.Value, result);
            }

            return transform;
        }

        public static string CleanStringForXmlName(string activityString) {

            return activityString.Replace(" ", string.Empty).Replace("_", string.Empty);
        }


        public static string NormalizeFieldValue(string value) {
            return value.Replace("\r", "").Replace("\r\n", "").Replace("\n", "").Replace("\t", " ");

        }

        public static List<UnlimitedObject> FindDataObjectByTagName(List<string> dataList, string tagName) {
            return DataListToUnlimitedObject(dataList).GetAllElements(tagName);
        }

        public static UnlimitedObject DataListToUnlimitedObject(List<string> dataList) {
            UnlimitedObject data = new UnlimitedObject();

            if (dataList == null) {
                return data;
            }
            dataList.ForEach(c => data.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c)));

            return data;
        }

        public static bool TagsExist(string DataTags, List<string> dataList, out UnlimitedObject dataObject) {

            dataObject = new UnlimitedObject();

            if (string.IsNullOrEmpty(DataTags)) {
                return false;
            }

            List<string> tags = DataTags.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();


            dataObject = DataListToUnlimitedObject(dataList);

            foreach (string tag in tags) {

                if (!dataObject.ElementOrAttributeExists(tag)) {
                    dynamic missingTag = new UnlimitedObject();
                    missingTag.MissingTag = tag;

                    dataObject.AddResponse(missingTag);


                }
            }

            if (!dataObject.ElementOrAttributeExists("MissingTag")) {

                return true;
            }
            else {
                return false;
            }



        }


        /// <summary>
        /// Executes a service request at the dsf
        /// </summary>
        /// <param name="requestXml">The service request xml string</param>
        /// <returns>A String containing the result from the dsf</returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static string InvokeDsfService(string requestXml, string uri) {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("requestXml", requestXml);
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("uri", uri);

            EndpointAddress dsfAddress = new EndpointAddress(uri);
            BasicHttpBinding dsfBinding = new BasicHttpBinding();

            IEsbChannel dsf = ChannelFactory<IEsbChannel>.CreateChannel(dsfBinding, dsfAddress);

            return dsf.ExecuteCommand(requestXml);
        }

        /// <summary>
        /// Validates that an xml is valid according to the existence of specific tags and optionally
        /// a jscript expression that must evaluate to true [boolean and not string] in order for the string to be interpreted as valid.
        /// This behaviour is required in order to make workflow branching (decisions/if statements) function.
        /// It is important to note that there is a direct correlation between each tagname 
        /// and the ordinal number in the expression that musth start at 0 and be wrapped with curved braces 
        /// as with the string.format function e.g. if resultValidationRequiredTags = GovID,Surname
        /// then resultValidationExpression of '{0}'=='800421' && '{1}'== 'test
        /// will replace {0} and {1} with GovID and Surname respectively 
        /// </summary>
        /// <param name="result">The string to validate</param>
        /// <param name="resultValidationRequiredTags">The xml tags that must appear in the string for it to be valid</param>
        /// <param name="resultValidationExpression">The expression to use to validate the data in the tags</param>
        /// <returns>Boolean value indicating success</returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static bool ResultValidation(string result, string resultValidationRequiredTags, string resultValidationExpression) {
            bool isValid = false;

            //We cannot validate an empty result;
            if (string.IsNullOrEmpty(result)) {
                throw new ArgumentException("Cannot validate an empty result");
            }

            //If we do not have tags to validate against then the result is automatically set to valid
            if (string.IsNullOrEmpty(resultValidationRequiredTags)) {
                return true;
            }

            //Make the result data an UnlimitedObject
            dynamic resultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);
            if (resultObj != null) {
                //Extract the tag csv as an UnlimitedObject
                dynamic resultTagsObj = UnlimitedObject.GetCsvAsUnlimitedObject(resultValidationRequiredTags);

                //Get the required tag(s)
                //This could be a single tag or multiple tags
                dynamic tags = resultTagsObj.dt;

                //We will store all applicable tags in a list
                //as the UnlimitedObject could return either
                //a string or an UnlimitedObject or a List of UnlimitedObjects
                List<string> requiredTags = new List<string>();

                if (tags is string) {
                    requiredTags.Add(tags);
                }

                if (tags is List<UnlimitedObject>) {
                    foreach (dynamic tag in tags) {
                        requiredTags.Add(tag.dt);
                    }
                }

                int ordinal = 0;
                foreach (string tag in requiredTags) {

                    //If a single required tag does not exist 
                    //in the result data then immediately exit
                    //with a false return value indicating
                    //that the result data is invalid
                    if (!resultObj.ElementOrAttributeExists(tag)) {
                        return false;
                    }
                    else {
                        isValid = true;
                    }

                    //Build the result validation expression
                    //that will be executed later
                    if (!string.IsNullOrEmpty(resultValidationExpression)) {
                        resultValidationExpression = resultValidationExpression.Replace("{" + ordinal.ToString() + "}", UnlimitedObject.GetDataValueFromUnlimitedObject(tag, resultObj));

                    }
                    ordinal++;
                }

                //Run the JScript expression and evaluate the result.
                //We are only looking for boolean return values
                //anything else is ignored.
                if (!string.IsNullOrEmpty(resultValidationExpression)) {
                    dynamic returnData = null;

                    returnData = Microsoft.JScript.Eval.JScriptEvaluate(resultValidationExpression, Microsoft.JScript.Vsa.VsaEngine.CreateEngine());



                    if (returnData != null) {
                        if (returnData is bool) {
                            isValid = returnData;
                        }
                    }
                }

            }

            return isValid;
        }*/

        #endregion Old ActivityHelper Methods

        public static string CDATAWrapOnScriptRegion(string evalRegion) {
            string scriptCDATARegion = string.Empty;
            if (evalRegion.Contains("script")) {
                var region = GetScriptTagRegion(evalRegion);
                if (!String.IsNullOrEmpty(region)) {
                    scriptCDATARegion = evalRegion.Replace(region, String.Format("<![CDATA[{0}]]>", region));
                    return scriptCDATARegion;
                }
                return evalRegion;
            }
            else {
                return evalRegion;
            }
        }

        private static string GetScriptTagRegion(string evalRegion) {
            List<KeyValuePair<string, string>> htmlObjects = new List<KeyValuePair<string, string>>();
            htmlObjects.Add(new KeyValuePair<string, string>("<script>", "</script>"));
            if (evalRegion.Contains("<![CDATA")) {
                return string.Empty;
            }
            string reeavluatedEvalRegion = String.Empty;
            string regiontoReplace = string.Empty;

            // { "<script>", "<div", "<span", "<input", "<button" };
            htmlObjects.ForEach(html => {
                var scriptRegion = evalRegion.IndexOf(html.Key);
                var endScriptRegion = evalRegion.IndexOf(html.Value) + html.Value.Length;
                if (scriptRegion != -1 && endScriptRegion != -1) {
                    reeavluatedEvalRegion += evalRegion.Substring(scriptRegion, endScriptRegion - scriptRegion);
                }
            });
            return reeavluatedEvalRegion;
        }
    }
}
