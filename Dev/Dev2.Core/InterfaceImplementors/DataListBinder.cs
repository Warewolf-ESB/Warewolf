//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Diagnostics.CodeAnalysis;
//using System.IO;
//using System.Linq;
//using System.ServiceModel;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Xml;
//using System.Xml.Linq;
//using Dev2.Common;
//using Dev2.DataList.Contract;
//using Unlimited.Framework;

//namespace Dev2
//{
//    public class DataListBinder : IDataListBinder
//    {


//        #region Public Methods

//        public string ParseHTML(string html, string bindingData, IEsbChannel dsfChannel, IDSFDataObject parentRequest)
//        {
//            //System.Diagnostics.Debugger.Break();
//            if(dsfChannel == null)
//            {
//                throw new ArgumentNullException("dsfChannel");
//            }

//            if(string.IsNullOrEmpty(html))
//            {
//                throw new ArgumentNullException("html");
//            }

//            UnlimitedObject bindingContext = null;
//            if(!string.IsNullOrEmpty(bindingData))
//            {
//                bindingContext = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(bindingData);
//            }

//            string replacedHtml = html;

//            UnlimitedObject tags = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(html);

//            replacedHtml = tags.XmlString;

//            var tagItems = tags.GetAllElements("dev2html");

//            foreach(dynamic tag in tagItems)
//            {
//                string name = null;
//                string type = null;

//                if(tag.name is string)
//                {
//                    name = tag.name;
//                }

//                if(tag.type is string)
//                {
//                    type = tag.type;
//                }

//                List<string> exclusions = new List<string>() { "form", "meta", "pagetitle" };

//                if(!string.IsNullOrEmpty(type))
//                {
//                    if(!exclusions.Contains(type, StringComparer.OrdinalIgnoreCase))
//                    {
//                        if((bindingContext != null) && !string.IsNullOrEmpty(name))
//                        {
//                            if(!string.IsNullOrEmpty(type))
//                            {
//                                string instruction = UnlimitedObject.GenerateServiceRequest(
//                                        type,
//                                        string.Empty,
//                                        new List<string> { bindingContext.GetElement(name).XmlString },
//                                        parentRequest
//                                );

//                                UnlimitedObject result = null; // UnlimitedObject.GetStringXmlDataAsUnlimitedObject(dsfChannel.ExecuteCommand(instruction));
//                                if(!result.HasError)
//                                {
//                                    replacedHtml = replacedHtml.Replace(tag.XmlString, result.XPath("//Fragment/*").Inner());

//                                    //replacedHtml = replacedHtml.Replace(tag.XmlString, tmp.XPath("//Fragment/text()").Inner());
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            return replacedHtml;
//        }

//        public Guid InvokeDsfService(string requestXml, string uri, Guid dataListID)
//        {
//            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("requestXml", requestXml);
//            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("uri", uri);

//            EndpointAddress dsfAddress = new EndpointAddress(uri);
//            BasicHttpBinding dsfBinding = new BasicHttpBinding();

//            IEsbChannel dsf = ChannelFactory<IEsbChannel>.CreateChannel(dsfBinding, dsfAddress);

//            Guid result = GlobalConstants.NullDataListID;
//            var workspaceID = Guid.Empty; // TODO: Fix hard-coded workspaceID
//            string r = dsf.ExecuteCommand(requestXml, workspaceID, dataListID);
//            Guid.TryParse(r, out result);

//            return result;
//        }

//        public IEnumerable<string> EnumTextStreamLines(StreamReader stream)
//        {
//            while(!stream.EndOfStream)
//            {
//                var lineData = stream.ReadLine();
//                yield return lineData + "\r\n";
//            }

//            stream.Close();
//        }

//        public string RecursiveDescentScanner(string dataSource, string transform)
//        {
//            string replacexml = transform.Replace("[[", "<x>").Replace("]]", "</x>");

//            var xel = new XElement("a");
//            try
//            {
//                xel = XElement.Parse("<a>" + replacexml + "</a>");
//            }
//            catch
//            {
//                return "Syntax Error";
//            }

//            var res = xel.DescendantsAndSelf("x");

//            List<string> tokens = new List<string>();

//            res.Reverse().ToList().ForEach(c => tokens.Add(c.ToString().Replace(" ", string.Empty).Replace("\r\n", string.Empty).Replace("<x>", "[[").Replace("</x>", "]]")));

//            string prevData = string.Empty;
//            string prevToken = string.Empty;

//            foreach(var data in tokens)
//            {
//                if(!string.IsNullOrEmpty(prevData))
//                {
//                    prevData = data.Replace(prevToken, prevData);
//                }
//                prevToken = data;
//                prevData = RegionParser(dataSource, string.IsNullOrEmpty(prevData) ? data : prevData);
//            }

//            return prevData;
//        }

//        public string RegionParser(string dataSource, string transform)
//        {
//            if(transform.Equals("[[]]"))
//            {
//                return string.Empty;
//            }

//            string xPathMatch = "xpath";

//            // Inject XPath recordsets, but avoid xpath expressions
//            if(transform.Contains("(") && !transform.ToLower().Contains(xPathMatch))
//            {
//                transform = ConvertDotNotationToXPath(transform, dataSource);
//            }

//            dynamic data = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(dataSource);

//            string pattern = @"(?<tagonly>(\[\[)(?<tag>[0-9a-zA-z]+)(\]\]))|(?<xpathonly>(\[\[)xpath\((?<xpathexpression>.*)\)(\]\]))";

//            MatchCollection matches = Regex.Matches(transform, pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

//            foreach(Match patternMatch in matches)
//            {
//                var tag = patternMatch.Groups["tag"].Value;
//                var xPathExpression = patternMatch.Groups["xpathexpression"].Value.Replace("'", "");
//                ;
//                var xPathOnlyMatch = patternMatch.Groups["xpathonly"].Value;
//                var tagWithXPathMatch = patternMatch.Groups["tagwithxpath"].Value;
//                var tagOnlyMatch = patternMatch.Groups["tagonly"].Value;
//                var databindingExpression = patternMatch.Groups["databindingExpression"].Value;
//                var dataExpression = patternMatch.Groups["dataExpression"].Value;

//                string result = string.Empty;

//                Stopwatch s = new Stopwatch();
//                string msg = string.Empty;
//                //This is an xpathonly expression
//                if(string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(xPathExpression))
//                {
//                    s.Start();
//                    if(!xPathExpression.Contains("[["))
//                    {
//                        result = data.XPath(xPathExpression).Inner();
//                    }
//                    s.Stop();
//                    msg = string.Format("{0} XPath {1}", s.ElapsedMilliseconds, xPathExpression);
//                    //TraceWriter.WriteTrace(msg);
//                    s.Reset();

//                    transform = transform.Replace(xPathOnlyMatch, result);
//                }

//                //this is a tagwithxpath match
//                if(!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(xPathExpression))
//                {
//                    result = data.XPath(string.Format("//{0}", tag)).XPath(xPathExpression).Inner();
//                    transform = transform.Replace(tagWithXPathMatch, result);
//                }

//                //This is a standard data region
//                if(!string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(xPathExpression))
//                {
//                    //tmp = data.XPath(string.Format("//{0}", tag)).Inner();
//                    s.Start();
//                    result = data.GetValue(tag);
//                    s.Stop();
//                    msg = string.Format("{0} Get Value for [[{1}]]", s.ElapsedMilliseconds, tag);
//                    //TraceWriter.WriteTrace(msg);
//                    s.Reset();
//                    transform = transform.Replace(tagOnlyMatch, result);

//                }

//            }
//            return transform;
//        }

//        public string TextAndJScriptRegionEvaluator(List<string> ambientDataList, string transform, string currentValue = "", bool dataBindRecursive = false, string rootService = "")
//        {
//            if(string.IsNullOrEmpty(transform))
//            {
//                return string.Empty;
//            }

//            if(!transform.Contains("[[") && !transform.Contains("{{"))
//            {
//                return BindEnvironmentVariables(transform, rootService);
//            }

//            Stopwatch s = new Stopwatch();
//            s.Start();
//            var data = DataListToUnlimitedObject(ambientDataList);
//            s.Stop();

//            string message = string.Format("{0}ms: DataListToUnlimitedObject", s.ElapsedMilliseconds.ToString());
//            s.Reset();

//            //TraceWriter.WriteTrace(message);

//            s.Start();
//            List<DataRegion> regions = RecursiveDescentParser(transform);
//            s.Stop();


//            message = string.Format("{0}ms: End Region Parsing", s.ElapsedMilliseconds.ToString());
//            //TraceWriter.WriteTrace(message);
//            s.Reset();

//            s.Start();
//            regions
//                .ForEach(c =>
//                {
//                    var dat = data.XmlString;
//                    RecursiveDescentEvaluator(c, dat);
//                });
//            s.Stop();
//            message = string.Format("{0}ms: End Region Evaluation", s.ElapsedMilliseconds.ToString());
//            //TraceWriter.WriteTrace(message);
//            s.Reset();

//            s.Start();
//            //Get all root level items and then to the substitution;
//            var roots = regions.Where(c => c.IsRootLevel);
//            if(roots.Count() > 0)
//            {
//                roots
//                    .OrderByDescending(a => a.RegionData.ToString().Length)
//                    .ToList()
//                    .ForEach(d =>
//                    {
//                        transform = transform.Replace(d.RegionData.ToString(), d.Value);
//                    });
//            }
//            s.Stop();
//            message = string.Format("{0}ms: End Region Substitution", s.ElapsedMilliseconds.ToString());
//            //TraceWriter.WriteTrace(message);
//            s.Reset();


//            if(transform.Contains("{{"))
//            {
//                string codepattern = @"(?<code>\{\{.*\}\})";
//                string jScriptCode = string.Empty;
//                Match match = Regex.Match(transform, codepattern, RegexOptions.Singleline);
//                if(match.Success)
//                {
//                    jScriptCode = match.Value;

//                    if(!string.IsNullOrEmpty(jScriptCode))
//                    {
//                        s.Start();
//                        string codeToExecute = jScriptCode.Replace("\r\n", string.Empty).Replace("\n", string.Empty);
//                        string returnVal = JScriptEvaluator.EvaluateToString(codeToExecute);
//                        s.Stop();

//                        message = string.Format("{0}ms: End JScriptEval", s.ElapsedMilliseconds.ToString());
//                        //TraceWriter.WriteTrace(message);

//                        transform = transform.Replace(match.Value, returnVal).Trim();
//                    }
//                }
//            }

//            if(transform.Equals(currentValue))
//            {
//                return transform;
//            }

//            if(dataBindRecursive)
//            {
//                if(transform.Contains("[[") || transform.Contains("{{"))
//                {
//                    transform = TextAndJScriptRegionEvaluator(ambientDataList, transform, transform, true, rootService);
//                }
//            }

//            return BindEnvironmentVariables(transform, rootService);
//        }

//        //string EvaluateTextAndJScriptRegions(IList<string> dataList, string transformation, string currentValue, bool recursiveBinding, string rootService);

//        public string BindEnvironmentVariables(string transform, string rootServiceName = "")
//        {

//            if(string.IsNullOrEmpty(transform))
//            {
//                return transform;
//            }

//            transform = transform.Replace("@AppPath", Environment.CurrentDirectory);
//            transform = transform.Replace("@ServiceName", rootServiceName);
//            transform = transform.Replace("@OSVersion", Environment.OSVersion.VersionString);

//            return transform;
//        }

//        public string ParseDataRegionTokens(IList<string> ambientDataList, string transform)
//        {
//            dynamic data = DataListToUnlimitedObject(ambientDataList);

//            if(string.IsNullOrEmpty(transform))
//            {
//                return string.Empty;
//            }

//            string pattern = @"^(\[\[)(.*?)([^/]+\]\])$";
//            //Extract all data region tokens at their highest level
//            //We will be recursively descending into their children if applicable 
//            //at a later stage
//            MatchCollection matches = Regex.Matches(transform, pattern, RegexOptions.Multiline);
//            foreach(Match patternMatch in matches)
//            {
//                //Pass data region token to recursive descent parser with data to be used
//                string result = RecursiveDescentScanner(data.XmlString, patternMatch.Value);

//                transform = transform.Replace(patternMatch.Value, result);
//            }

//            return transform;
//        }

//        public string CleanStringForXmlName(string activityString)
//        {
//            return activityString.Replace(" ", string.Empty).Replace("_", string.Empty);
//        }

//        public string NormalizeFieldValue(string value)
//        {
//            return value.Replace("\r", "").Replace("\r\n", "").Replace("\n", "").Replace("\t", " ");
//        }

//        public IList<UnlimitedObject> FindDataObjectByTagName(IList<string> dataList, string tagName)
//        {
//            return DataListToUnlimitedObject(dataList).GetAllElements(tagName);
//        }

//        public UnlimitedObject DataListToUnlimitedObject(IList<string> dataList)
//        {
//            UnlimitedObject data = new UnlimitedObject();
//            List<string> internalDataList = dataList.ToList();

//            if(internalDataList == null)
//            {
//                return data;
//            }
//            internalDataList.ForEach(c => data.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c)));

//            return data;

//        }

//        public bool TagsExist(string DataTags, IList<string> dataList, out UnlimitedObject dataObject)
//        {

//            dataObject = new UnlimitedObject();

//            if(string.IsNullOrEmpty(DataTags))
//            {
//                return false;
//            }

//            List<string> tags = DataTags.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
//                .ToList();


//            dataObject = DataListToUnlimitedObject(dataList);

//            foreach(string tag in tags)
//            {

//                if(!dataObject.ElementOrAttributeExists(tag))
//                {
//                    dynamic missingTag = new UnlimitedObject();
//                    missingTag.MissingTag = tag;

//                    dataObject.AddResponse(missingTag);


//                }
//            }

//            if(!dataObject.ElementOrAttributeExists("MissingTag"))
//            {

//                return true;
//            }
//            else
//            {
//                return false;
//            }


//        }

//        //public string InvokeDsfService(string requestXml, string uri) {
//        //    Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("requestXml", requestXml);
//        //    Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("uri", uri);

//        //    EndpointAddress dsfAddress = new EndpointAddress(uri);
//        //    BasicHttpBinding dsfBinding = new BasicHttpBinding();

//        //    IEsbChannel dsf = ChannelFactory<IEsbChannel>.CreateChannel(dsfBinding, dsfAddress);

//        //    return dsf.ExecuteCommand(requestXml);
//        //}

//        public bool ResultValidation(string result, string resultValidationRequiredTags, string resultValidationExpression)
//        {
//            bool isValid = false;

//            //We cannot validate an empty tmp;
//            if(string.IsNullOrEmpty(result))
//            {
//                throw new ArgumentException("Cannot validate an empty tmp");
//            }

//            //If we do not have tags to validate against then the tmp is automatically set to valid
//            if(string.IsNullOrEmpty(resultValidationRequiredTags))
//            {
//                return true;
//            }

//            //Make the tmp data an UnlimitedObject
//            dynamic resultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);
//            if(resultObj != null)
//            {
//                //Extract the tag csv as an UnlimitedObject
//                dynamic resultTagsObj = UnlimitedObject.GetCsvAsUnlimitedObject(resultValidationRequiredTags);

//                //Get the required tag(s)
//                //This could be a single tag or multiple tags
//                dynamic tags = resultTagsObj.dt;

//                //We will store all applicable tags in a list
//                //as the UnlimitedObject could return either
//                //a string or an UnlimitedObject or a List of UnlimitedObjects
//                List<string> requiredTags = new List<string>();

//                if(tags is string)
//                {
//                    requiredTags.Add(tags);
//                }

//                if(tags is List<UnlimitedObject>)
//                {
//                    foreach(dynamic tag in tags)
//                    {
//                        requiredTags.Add(tag.dt);
//                    }
//                }

//                int ordinal = 0;
//                foreach(string tag in requiredTags)
//                {

//                    //If a single required tag does not exist 
//                    //in the tmp data then immediately exit
//                    //with a false return value indicating
//                    //that the tmp data is invalid
//                    if(!resultObj.ElementOrAttributeExists(tag))
//                    {
//                        return false;
//                    }
//                    else
//                    {
//                        isValid = true;
//                    }

//                    //Build the tmp validation expression
//                    //that will be executed later
//                    if(!string.IsNullOrEmpty(resultValidationExpression))
//                    {
//                        resultValidationExpression = resultValidationExpression.Replace("{" + ordinal.ToString() + "}", UnlimitedObject.GetDataValueFromUnlimitedObject(tag, resultObj));

//                    }
//                    ordinal++;
//                }

//                //Run the JScript expression and evaluate the tmp.
//                //We are only looking for boolean return values
//                //anything else is ignored.
//                if(!string.IsNullOrEmpty(resultValidationExpression))
//                {
//                    dynamic returnData = null;

//#pragma warning disable
//                    returnData = Microsoft.JScript.Eval.JScriptEvaluate(resultValidationExpression, Microsoft.JScript.Vsa.VsaEngine.CreateEngine());
//#pragma warning enable



//                    if (returnData != null)
//                    {
//                        if(returnData is bool)
//                        {
//                            isValid = returnData;
//                        }
//                    }
//                }

//            }

//            return isValid;
//        }

//        //public string ParseHTML(string html, string bindingData, IEsbChannel dsfChannel, IDSFDataObject parentRequest) {
//        //    //System.Diagnostics.Debugger.Break();
//        //    if (dsfChannel == null) {
//        //        throw new ArgumentNullException("dsfChannel");
//        //    }

//        //    if (string.IsNullOrEmpty(html)) {
//        //        throw new ArgumentNullException("html");
//        //    }

//        //    UnlimitedObject bindingContext = null;
//        //    if (!string.IsNullOrEmpty(bindingData)) {
//        //        bindingContext = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(bindingData);
//        //    }

//        //    string replacedHtml = html;

//        //    UnlimitedObject tags = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(html);

//        //    replacedHtml = tags.XmlString;

//        //    var tagItems = tags.GetAllElements("dev2html");

//        //    foreach (dynamic tag in tagItems) {
//        //        string name = null;
//        //        string type = null;

//        //        if (tag.name is string) {
//        //            name = tag.name;
//        //        }

//        //        if (tag.type is string) {
//        //            type = tag.type;
//        //        }

//        //        List<string> exclusions = new List<string>() { "form", "meta", "pagetitle" };

//        //        if (!string.IsNullOrEmpty(type)) {
//        //            if (!exclusions.Contains(type, StringComparer.OrdinalIgnoreCase)) {
//        //                if ((bindingContext != null) && !string.IsNullOrEmpty(name)) {
//        //                    if (!string.IsNullOrEmpty(type)) {
//        //                        string instruction = UnlimitedObject.GenerateServiceRequest(
//        //                                type,
//        //                                string.Empty,
//        //                                new List<string> { bindingContext.GetElement(name).XmlString },
//        //                                parentRequest
//        //                        );

//        //                        UnlimitedObject result = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(dsfChannel.ExecuteCommand(instruction));
//        //                        if (!result.HasError) {
//        //                            replacedHtml = replacedHtml.Replace(tag.XmlString, result.XPath("//Fragment/*").Inner());

//        //                            //replacedHtml = replacedHtml.Replace(tag.XmlString, tmp.XPath("//Fragment/text()").Inner());
//        //                        }
//        //                    }
//        //                }
//        //            }
//        //        }
//        //    }

//        //    return replacedHtml;
//        //}

//        #endregion

//        #region Private Methods

//        private List<DataRegion> RecursiveDescentParser(string transform)
//        {
//            StringReader s = new StringReader(transform);

//            List<DataRegion> regions = new List<DataRegion>();
//            StringBuilder regionData = new StringBuilder();
//            DataRegion lastDataRegion = null;

//            while(s.Peek() != -1)
//            {


//                var lastRegionList = regions.Where(c => c.IsOpen);
//                if(lastRegionList.Count() > 0)
//                {
//                    lastDataRegion = lastRegionList.Last();
//                }
//                else
//                {
//                    lastDataRegion = null;
//                }

//                char ch = (char)s.Peek();

//                char nxtch = '\0';

//                //if (char.IsWhiteSpace(ch)) {
//                //    s.Read();
//                //}

//                if(ch == '[')
//                {
//                    s.Read();

//                    nxtch = (char)s.Peek();

//                    if(nxtch == '[')
//                    {
//                        //Region Starting
//                        DataRegion d = new DataRegion { IsOpen = true };
//                        if(lastDataRegion != null)
//                        {
//                            d.Parent = lastDataRegion;
//                            d.Parent.Child = d;
//                            //lastDataRegion.RegionData.Append("$DEV2$");
//                        }

//                        regions.Add(d);
//                        d.RegionData.Append(new char[] { ch, ch });
//                        s.Read();
//                    }
//                    else
//                    {
//                        if(lastDataRegion != null)
//                        {
//                            lastDataRegion.RegionData.Append(ch);
//                        }
//                    }

//                }
//                else
//                {
//                    if(ch == ']')
//                    {
//                        s.Read();

//                        nxtch = (char)s.Peek();

//                        if(nxtch == ']')
//                        {


//                            if(lastDataRegion != null)
//                            {
//                                //Region Closing
//                                lastDataRegion.RegionData.Append(new char[] { ch, ch });

//                                lastDataRegion.IsOpen = false;

//                                string regionString = lastDataRegion.RegionData.ToString();

//                                if(lastDataRegion.Parent != null)
//                                {
//                                    lastDataRegion.Parent.RegionData.Append(regionString);
//                                }
//                            }

//                            s.Read();
//                        }
//                        else
//                        {
//                            if(lastDataRegion != null)
//                            {
//                                lastDataRegion.RegionData.Append(ch);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        //This data needs to be appended to the last open region
//                        if(lastDataRegion != null)
//                        {
//                            lastDataRegion.RegionData.Append(ch);
//                        }
//                        s.Read();
//                    }
//                }
//            }
//            return regions;
//        }

//        private string ConvertDotNotationToXPath(string transformation, string dataList)
//        {

//            /*
//             * Recordset()
//             * Recordset().FieldName
//             * Recordset(x).FieldName
//             */

//            transformation = transformation.Replace("[[", "").Replace("]]", "");

//            StringBuilder result = new StringBuilder();

//            int pos = transformation.IndexOf("()");
//            // check for () aka AllRows()
//            // 09.07.2012 - Travis.Frisinger ~ This notation now becomes last row
//            if(pos > 0 && (pos == (transformation.Length - 2) || (pos == (transformation.Substring(0, transformation.IndexOf(".")).Length - 2))))
//            {
//                string recordsetName = transformation.Substring(0, pos);
//                pos = SecondDotIndex(transformation);
//                if(pos > 0)
//                {
//                    // Recordset().FieldName
//                    string fieldName = ExtractFieldName(transformation, pos);
//                    result.Append("[[xpath('//" + recordsetName + "[last()]/" + fieldName);
//                }
//                else
//                {
//                    // Recordset.AllRows()

//                    result.Append("[[xpath('//" + recordsetName + "[last()]");
//                }
//                result.Append("')]]");
//            }
//            else
//            {
//                // check for index
//                pos = transformation.IndexOf("(");
//                string recordsetName = transformation.Substring(0, pos);
//                string index = ExtractRowIndex(transformation);
//                int idx = -1;
//                pos = SecondDotIndex(transformation);
//                string fieldName = ExtractFieldName(transformation, pos);

//                if(!int.TryParse(index, out idx))
//                {
//                    idx = -1;
//                    // try evaluation
//                    if(index != "*")
//                    {
//                        throw new NotImplementedException("Evaluated index values are not currently implemented");
//                    }
//                }

//                if(pos > 0)
//                {
//                    // Recordset(x).FieldName                        
//                    if(idx >= 0)
//                    {
//                        result.Append("[[xpath('//" + recordsetName + "[" + idx + "]" + "/" + fieldName);

//                        //tmp.Append("[[xpath('//" + recordsetName + "/" + fieldName + "[" + idx + "]");
//                    }
//                    else if(index == "*")
//                    {
//                        // new AllRows() notation
//                        result.Append("[[xpath('//" + recordsetName + "/" + fieldName);
//                    }
//                }
//                else
//                {
//                    // Recordset(x) 
//                    if(idx >= 0)
//                    {
//                        result.Append("[[xpath('//" + recordsetName + "[" + idx + "]");
//                    }
//                    else if(index == "*")
//                    {
//                        result.Append("[[xpath('//" + recordsetName);
//                    }
//                }
//                //}
//                result.Append("')]]");
//            }



//            return result.ToString();
//        }

//        private int SecondDotIndex(string transformation)
//        {
//            return transformation.IndexOf(".");
//        }

//        private string ExtractFieldName(string transformation, int pos)
//        {
//            return transformation.Substring((pos + 1), (transformation.Length - (pos + 1)));
//        }

//        private string ExtractRowIndex(string transformation)
//        {
//            string result = string.Empty;

//            int start = transformation.IndexOf("(");
//            int end = transformation.IndexOf(")");

//            if(start < end && start > 0)
//            {
//                result = transformation.Substring((start + 1), (end - (start + 1)));
//            }

//            return result;
//        }

//        private int CountTags(string dataList, string recordset)
//        {
//            return CountTags(dataList, recordset, null);
//        }

//        private int CountTags(string dataList, string recordset, string field)
//        {
//            int result = 0;

//            XmlDocument xDoc = new XmlDocument();
//            xDoc.LoadXml(dataList);

//            XmlNodeList rsNl = xDoc.GetElementsByTagName(recordset);

//            for(int i = 0; i < rsNl.Count; i++)
//            {

//                // is there a field to count
//                if(field != null && field != string.Empty)
//                {
//                    XmlNodeList childNl = rsNl[i].ChildNodes;

//                    for(int q = 0; q < childNl.Count; q++)
//                    {
//                        if(childNl[q].Name == field)
//                        {
//                            result++;
//                        }

//                    }
//                }
//                else
//                {
//                    result = rsNl.Count;
//                }
//            }

//            return result;
//        }

//        private void RecursiveDescentEvaluator(DataRegion region, string dataSource)
//        {

//            if(region.IsTokenGenerated)
//            {
//                return;
//            }

//            var s = Stopwatch.StartNew();
//            var current = region;

//            //This is a standard dataregion with no embedded data regions

//            if(!current.HasChild && !current.HasParent)
//            {
//                current.RootLevelToken = current.RegionData.ToString();
//                current.Value = RegionParser(dataSource, current.RootLevelToken.Replace("\r\n", string.Empty).Replace("\t", string.Empty));
//                //current.IsTokenGenerated = true;
//                current.IsRootLevel = true;
//                return;
//            }
//            s.Stop();
//            string message = string.Format("{0}: Set value of singleton Top Level node", s.ElapsedMilliseconds.ToString());
//            //TraceWriter.WriteTrace(message);
//            s.Reset();

//            //We are traversing the object graph down to the last level
//            //so we can work from the innermost child up for tree
//            s = Stopwatch.StartNew();
//            while(true)
//            {
//                if(current.Child == null)
//                    break;
//                current = current.Child;
//            }
//            s.Stop();
//            message = string.Format("{0}: Recursive Descend to innermost child", s.ElapsedMilliseconds.ToString());
//            //TraceWriter.WriteTrace(message);
//            s.Reset();

//            s.Start();
//            if(!current.HasChild)
//            {
//                current.RootLevelToken = current.RegionData.ToString();
//                current.Value = RegionParser(dataSource, current.RootLevelToken);
//                //current.IsTokenGenerated = true;
//            }
//            s.Stop();
//            message = string.Format("{0}: Set Current Value For Parent Binding", s.ElapsedMilliseconds.ToString());
//            //TraceWriter.WriteTrace(message);
//            s.Reset();

//            s.Start();
//            while(current.Parent != null)
//            {
//                string parentData = string.IsNullOrEmpty(current.Parent.RootLevelToken) ? current.Parent.RegionData.ToString() : current.Parent.RootLevelToken;
//                string myData = current.RootLevelToken;
//                string myDataValue = current.Value;

//                current.Parent.RootLevelToken = parentData.Replace(myData, myDataValue);//.Replace("$DEV2$", myData);
//                current.Parent.Value = parentData.Replace(myData, myDataValue);
//                //current.Parent.IsTokenGenerated = true;
//                current.Parent.Value = RegionParser(dataSource, current.Parent.Value);

//                current = current.Parent;
//                if(current.Parent == null)
//                {
//                    current.IsRootLevel = true;
//                }


//            }
//            s.Stop();
//            message = string.Format("{0}: Ascend to toplevel parent", s.ElapsedMilliseconds.ToString());
//            //TraceWriter.WriteTrace(message);
//            s.Reset();
//            ;


//        }
//        #endregion
//    }
//}
