using Dev2;

namespace Unlimited.Framework
{

    #region Using Directives
    using Dev2.Common;
    using Dev2.DataList.Contract;
    using Dev2.DataList.Contract.Binary_Objects;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    #endregion

    /// <summary>
    /// Author:         Sameer Chunilall
    /// Date:           2009-12-16
    /// Description:    The UnlimitedObject type inherits from the DynamicObject class and allows us to dynamically create read and interpret
    ///                 xml data. 
    ///                 The interesting aspect of this class is that any property or method that you define or invoke will only be evaluated or invoked 
    ///                 at runtime. The compiler purposefully ignores all types that exist within dynamic types so the code compiles perfectly at design time.
    ///                 This does not mean that it will work at runtime.
    ///                 The primary reason for creating this class is to facilitate a simple universal xml communication object that can
    ///                 be shared by both service layer and presentation layer code.
    /// 
    /// </summary>
    public class UnlimitedObject : DynamicObject
    {

        #region Attributes
        private static IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        #endregion

        #region Properties
        /// <summary>
        /// The actual xml data that we will be manipulating or reading from
        /// </summary>
        public XElement xmlData { get; set; }
        /// <summary>
        /// The state of the current object
        /// </summary>
        public enObjectState ObjectState { get; set; }
        /// <summary>
        /// Indicates whether the current xml data is in an error state which is represented by
        /// the presence of an Error tag in the data
        /// </summary>
        public bool HasError
        {
            get
            {
                IEnumerable<XElement> ErrorElement = xmlData.Descendants("Error");
                bool result = false;

                ErrorElement
                    .ToList()
                    .ForEach(elm =>
                    {
                        if (elm.Value != null && elm.Value.Length > 0)
                        {
                            result = true;
                        }
                    });


                return result;
            }
        }
        /// <summary>
        /// Indicates whether there are multiple service requests in the xml data
        /// This is useful typically but not exclusively to the dsf service endpoint
        /// </summary>
        public bool IsMultipleRequests
        {
            get
            {
                IEnumerable<XElement> serviceElement = xmlData.Descendants("Service").Where(c => !c.HasAttributes);
                if (serviceElement.Count() > 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private UnlimitedObject _parent;
        /// <summary>
        /// Contains the first parent node of the current xml data
        /// </summary>
        public UnlimitedObject Parent
        {
            get
            {
                if (_parent == null)
                {
                    XElement parent = xmlData.Ancestors().FirstOrDefault();

                    if (parent != null)
                    {
                        _parent = new UnlimitedObject(parent);
                    }
                }
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }
        /// <summary>
        /// Contains a collection of executable service requests if applicable.
        /// This is typically useful to the dsf service endpoint
        /// </summary>
        public IEnumerable<UnlimitedObject> Requests
        {
            get
            {

                if (!IsMultipleRequests)
                {
                    return null;
                }

                List<UnlimitedObject> unlimitedReq = new List<UnlimitedObject>();

                IEnumerable<XElement> requests = xmlData.Descendants("Service").Where(c => !c.HasAttributes);

                if (requests.Count() == 0)
                {
                    return unlimitedReq;
                }

                List<XElement> svc = new List<XElement>();
                List<string> data = new List<string>();


                //Iterate through all messages that contain a Service tag
                //add them to a collection
                foreach (XElement d in requests)
                {
                    XElement req = d.Ancestors().FirstOrDefault();

                    svc = new List<XElement>(req.Descendants("Service").Where(c => !c.HasAttributes));

                    if (req.Descendants("Service").Where(c => !c.HasAttributes).Count() > 1)
                    {
                        XElement requestData = new XElement(req);


                        foreach (XElement dsvc in req.Descendants("Service").Where(c => !c.HasAttributes))
                        {
                            requestData.Descendants("Service").Remove();

                            requestData.AddFirst(dsvc);

                            data.Add(new XElement(requestData).ToString());
                        }
                    }
                    else
                    {
                        data.Add(req.ToString());
                    }
                }

                data.Distinct().ToList().ForEach(c => { unlimitedReq.Add(new UnlimitedObject(XElement.Parse(c))); });

                return unlimitedReq.Distinct();
            }

        }

        /// <summary>
        /// The string representation of the xml data in this object
        /// </summary>
        public string XmlString
        {
            get
            {
                string val = string.Empty;
                try
                {
                    val = xmlData.ToString();
                }
                catch (Exception e)
                {
                    throw e;
                }

                return val;
            }
        }
        /// <summary>
        /// The string representation of the xml data from the first child downward
        /// </summary>
        public string InnerXmlString
        {
            get
            {
                if (xmlData.FirstNode != null)
                {
                    return xmlData.FirstNode.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new UnlimitedObject class
        /// </summary>
        public UnlimitedObject()
        {
            xmlData = new XElement("XmlData");
            this.ObjectState = enObjectState.NEW;

        }

        /// <summary>
        /// Initializes a new UnlimitedObject class
        /// </summary>
        /// <param name="xml">The xml elements that we will be manipulating or reading from</param>
        public UnlimitedObject(XElement xml)
        {
            xmlData = xml;
            this.ObjectState = enObjectState.UNCHANGED;
        }

        /// <summary>
        /// Initializes a new UnlimitedObject class
        /// </summary>
        /// <param name="xml">Creates the root node named as the this string.</param>
        public UnlimitedObject(string xml)
        {
            //Travis said so;)
            // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
            if (!xml.StartsWith("<"))
            // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
            {
                xmlData = new XElement(xml);
            }
            else
            {
                xmlData = XElement.Parse(xml);
            }

            this.ObjectState = enObjectState.UNCHANGED;
        }

        /// <summary>
        /// Serializes any object to xml and returns an UnlimitedObject that wraps the xml
        /// </summary>
        /// <param name="dataObject"></param>
        public UnlimitedObject(object dataObject)
        {

            if (dataObject is XElement)
            {
                xmlData = (XElement)dataObject;
                return;
            }

            xmlData = XElement.Parse(GetXmlDataFromObject(dataObject));
        }

        #endregion

        #region Public Methods

        public string Inner()
        {
            var reader = xmlData.CreateReader();
            reader.MoveToContent();
            string data = reader.ReadInnerXml();

            //Regex r = new Regex(@"<([^>]+)>[^<]*</(\1)>");
            //if (!r.IsMatch(data))
            //{
            //    return data;
            //}


            if (DataListUtil.IsXml(data))
            {
                return XElement.Parse(data).ToString();
            }
            else
            {
                return data;
            }
            //try
            //{
            //    return XElement.Parse(data).ToString();
            //}
            //catch (XmlException)
            //{
            //    return data;
            //}

        }


        /// <summary>
        /// Indicates whether the current xml data contains an element
        /// </summary>
        /// <param name="elementName">The element to search for</param>
        /// <returns>Boolean value true means that the element exists</returns>
        /// <exception cref="System.ArgumentException"></exception>
        ///  <exception cref="System.ArgumentNullException"></exception>
        public bool ElementExists(string elementName)
        {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("elementName", elementName);

            if (!IsValidElementOrAttributeName(elementName))
            {
                return false;
            }


            var results = xmlData.DescendantsAndSelf(elementName);
            return results.Count() > 0;
        }

        public void AddAttrib(string attribName, string attribValue)
        {
            xmlData.Add(new XAttribute(attribName, attribValue));

        }

        public string RootName
        {
            get
            {
                return xmlData.Name.LocalName;
            }
        }

        public bool ElementOrAttributeExists(string name)
        {


            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("name", name);

            if (!IsValidElementOrAttributeName(name))
            {
                return false;
            }

            bool exists = false;

            if (!ElementExists(name))
            {
                var attrib = xmlData.DescendantsAndSelf().Where(c => c.Attribute(name) != null);
                if (attrib.Count() > 0)
                {
                    exists = true;
                }
            }
            else
            {
                exists = true;
            }

            return exists;

        }

        /// <summary>
        /// Facilitates an XPath expression that can be run against the current xml data
        /// </summary>
        /// <param name="xPathExpression">An XPath expression</param>
        /// <returns>A List of UnlimitedObjects that match the XPath expression</returns>
        /// <exception cref="System.ArgumentException"></exception>
        ///  <exception cref="System.ArgumentNullException"></exception>
        public UnlimitedObject XPath(string xPathExpression)
        {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("xPathExpression", xPathExpression);

            IEnumerable<XObject> results = null;
            UnlimitedObject unlimitedReq;

            try
            {

                IEnumerable res = xmlData.XPathEvaluate(xPathExpression) as IEnumerable;

                if (res != null)
                {
                    results = res.Cast<XObject>();
                }
            }
            catch (XPathException xPathEx)
            {
                return new UnlimitedObject(xPathEx);
            }
            catch (InvalidOperationException invalidEx)
            {
                return new UnlimitedObject(invalidEx);
            }

            unlimitedReq = new UnlimitedObject("QueryResult");


            StringBuilder sb = new StringBuilder();

            foreach (var result in results)
            {

                switch (result.NodeType)
                {

                    case XmlNodeType.Attribute:
                        XAttribute att = result as XAttribute;
                        if (att != null)
                        {
                            unlimitedReq.CreateElement(att.Name.ToString()).SetValue(att.Value);
                        }
                        break;

                    case XmlNodeType.Text:
                        sb.Append(result.ToString());
                        break;

                    default:
                        unlimitedReq.Add(new UnlimitedObject(result));
                        break;
                }

            }

            if (sb.Length > 0)
            {
                unlimitedReq.GetElement("QueryResult").SetValue(sb.ToString());
            }


            return unlimitedReq;


        }
        /// <summary>
        /// Indicates whether the current xml data is a descendant of a particular element
        /// </summary>
        /// <param name="elementName">The ancestor element that we are searching for</param>
        /// <returns>Boolean where true means that the current xml data is a descendant of the provided ancestor</returns>
        ///  <exception cref="System.ArgumentException"></exception>
        ///  <exception cref="System.ArgumentNullException"></exception>
        public bool IsDescendantOf(string elementName)
        {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("elementName", elementName);
            if (!IsValidElementOrAttributeName(elementName))
            {
                return false;
            }

            bool isDescendant = xmlData.Ancestors(elementName).Count() >= 1;

            if (isDescendant)
            {
                XElement parent = xmlData.Ancestors(elementName).FirstOrDefault();

                if (parent != null)
                {
                    Parent = new UnlimitedObject(parent);
                }
            }

            return isDescendant;
        }

        public bool IsValidElementOrAttributeName(string name)
        {
            double value = 0;
            if (double.TryParse(name, out value))
            {
                return false;
            }
            try
            {
                XName.Get(name);
            }
            catch
            {
                return false;
            }

            return true;
            //return (CodeIdentifier.MakeValid(name) == name);
        }
        /// <summary>
        /// Replaces all occurrences of a tag's name with another tag name
        /// </summary>
        /// <param name="sourceTagName">The tag name to replace</param>
        /// <param name="targetTagName">The new tag name to use to replace the old one</param>
        /// <returns>String representation of the new xml data</returns>
        ///  <exception cref="System.ArgumentException"></exception>
        ///  <exception cref="System.ArgumentNullException"></exception>
        public string ReplaceTagName(string sourceTagName, string targetTagName)
        {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("sourceTagName", sourceTagName);
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("targetTagName", targetTagName);

            if (!IsValidElementOrAttributeName(targetTagName))
            {
                return xmlData.ToString();
            }

            IEnumerable<XElement> nodes = xmlData.DescendantsAndSelf(sourceTagName);

            if (nodes != null)
            {
                foreach (XElement xn in nodes)
                {
                    xn.Name = targetTagName;
                }
            }
            return xmlData.ToString();
        }

        /// <summary>
        /// Get the value of node or attribute in the current XmlData propery
        /// </summary>
        /// <param name="Name">The name of the attribute or element of which we are trying to get the value</param>
        /// <returns>string representation of the value</returns>
        ///  <exception cref="System.ArgumentException"></exception>
        ///  <exception cref="System.ArgumentNullException"></exception>
        public string GetValue(string Name)
        {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("Name", Name);

            if (!IsValidElementOrAttributeName(Name))
            {
                return string.Empty;
            }

            string returnVal = string.Empty;


            var matches = xmlData.DescendantsAndSelf(Name);

            if (matches.Count() > 1)
            {
                var returnData = new UnlimitedObject();
                matches.ToList().ForEach(mtch => returnData.Add(new UnlimitedObject(mtch)));

                //matches.ToList().ForEach(c => {
                //    ////Check if the node we are trying to retrieve
                //    ////contains XML value. Make the value XML and append to 
                //    ////return tmp if the value can be parsed as XML
                //    //XElement nodeValueXML = null;
                //    //try {
                //    //    nodeValueXML = XElement.Parse(c.Value);

                //    //}
                //    //catch { }

                //    //if (nodeValueXML != null) {
                //    //    UnlimitedObject data = new UnlimitedObject(Name);
                //    //    data.AddResponse(new UnlimitedObject(nodeValueXML));
                //    //    returnData.AddResponse(data);
                //    //}
                //    //else {
                //    //    //If the node data is not xml then just add the node as is to the tmp
                //    //    returnData.AddResponse(new UnlimitedObject(c));
                //    //}
                //});

                returnVal = returnData.XmlString;
            }
            else
            {
                var xm = matches.FirstOrDefault();
                //This is a complex string not just a value
                if (xm != null)
                {
                    if (xm.HasElements)
                    {
                        returnVal = xm.ToString();
                    }
                    else
                    {

                        if (xmlData.Attribute(XName.Get(Name)) != null)
                        {
                            //This is a singleton attribute value that we can return.
                            return xmlData.Attribute(XName.Get(Name)).Value;
                        }
                        else
                        {
                            //This is a singleton value that we can return
                            return returnVal = xm.Value;
                        }

                    }
                }
                else
                {
                    var attrib = xmlData.DescendantsAndSelf().Where(c => c.Attribute(Name) != null);
                    if (attrib.Count() > 0)
                    {
                        returnVal = attrib.FirstOrDefault().Attribute(Name).Value;
                    }
                }
            }


            return returnVal;
        }
        /// <summary>
        /// Retrieves the specified element from the current xml data. If it does not exist, a new one is created with the name provided and returned
        /// </summary>
        /// <param name="Name">The Name of the Element to search for</param>
        /// <returns>UnlimitedObject containing the element found or a new element created</returns>
        public dynamic GetElement(string Name)
        {

            if (!IsValidElementOrAttributeName(Name))
            {
                return new UnlimitedObject("data");
            }

            XElement xm = xmlData.DescendantsAndSelf(Name).FirstOrDefault();

            var data = new UnlimitedObject();

            if (xm == null)
            {
                XElement newelement = new XElement(Name);
                xmlData.Add(newelement);
                return new UnlimitedObject(newelement);
            }
            else
            {

                return new UnlimitedObject(xm);
            }

        }

        public dynamic CreateElement(string name)
        {
            if (!IsValidElementOrAttributeName(name))
            {

                return new UnlimitedObject("data");
            }

            XElement newelement = new XElement(name);
            xmlData.Add(newelement);
            return new UnlimitedObject(newelement);
        }


        public List<UnlimitedObject> GetAllElements(string name)
        {
            var dataList = new List<UnlimitedObject>();

            if (!IsValidElementOrAttributeName(name))
            {
                dataList.Add(new UnlimitedObject("data"));
                return dataList;
            }

            IEnumerable<XElement> xm = xmlData.Descendants(name);

            var data = new UnlimitedObject();

            if (xm == null)
            {
                XElement newelement = new XElement(name);
                xmlData.Add(newelement);
                dataList.Add(new UnlimitedObject(newelement));

                return dataList;
            }
            else
            {
                foreach (XElement x in xm)
                {
                    dataList.Add(new UnlimitedObject(x));

                }
            }

            return dataList;

        }

        /// <summary>
        /// Set the value of the current xml element
        /// </summary>
        /// <param name="value">The value to set for the current xml element</param>
        /// <returns>Boolean where true means that the value was set successfully</returns>
        ///  <exception cref="System.ArgumentException"></exception>
        ///  <exception cref="System.ArgumentNullException"></exception>
        public dynamic SetValue(string value)
        {
            bool returnValue = false;

            string dataValue = value ?? string.Empty;

            if (xmlData != null)
            {

                XElement valueXML = null;

                if (!dataValue.Contains("<") && !dataValue.Contains(">"))
                {
                    xmlData.SetValue(dataValue);
                    return returnValue;
                }

                Regex r = new Regex(@"<([^>]+)>[^<]*</(\1)>");
                if (!r.IsMatch(dataValue))
                {
                    xmlData.SetValue(dataValue);
                    return returnValue;
                }

                if (DataListUtil.IsXml(value))
                {
                    valueXML = XElement.Parse(value);
                    xmlData.ReplaceNodes(valueXML);
                }
                else
                {
                    dynamic delineateResult = DelineateXMLString(value);

                    if (delineateResult is List<XElement>)
                    {
                        var newNodes = delineateResult as List<XElement>;
                        if (newNodes != null)
                        {
                            xmlData.ReplaceAll(newNodes);
                        }
                    }
                    else
                    {
                        xmlData.SetValue(value ?? string.Empty);
                    }
                }

                //try
                //{
                //    valueXML = XElement.Parse(value);
                //}
                //catch { }

                //if (valueXML != null)
                //{
                //    xmlData.ReplaceNodes(valueXML);
                //}
                //else
                //{
                //    dynamic delineateResult = DelineateXMLString(value);

                //    if (delineateResult is List<XElement>)
                //    {
                //        var newNodes = delineateResult as List<XElement>;
                //        if (newNodes != null)
                //        {
                //            xmlData.ReplaceAll(newNodes);
                //        }
                //    }
                //    else
                //    {
                //        xmlData.SetValue(value ?? string.Empty);
                //    }
                //}
            }
            return returnValue;
        }


        public static dynamic DelineateXMLString(string xml)
        {
            List<Tuple<string, bool>> xmlconformance = new List<Tuple<string, bool>>();

            var settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            try
            {
                using (var reader = XmlReader.Create(new StringReader(xml), settings))
                {
                    reader.Read();
                    while (!reader.EOF)
                    {
                        //if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower() == "html" && reader.Depth == 0)
                        //{
                        //    return xml;
                        //}

                        var test = string.Empty;

                        test = XNode.ReadFrom(reader).ToString();

                        if (!test.Equals("\r\n") && !test.EndsWith("\r\n"))
                        {
                            try
                            {
                                XElement.Parse(test);
                                xmlconformance.Add(new Tuple<string, bool>(test, true));
                            }
                            catch (XmlException)
                            {
                                xmlconformance.Add(new Tuple<string, bool>(test, false));
                            }
                        }
                        else
                        {
                            return xml;
                        }
                    }

                    if (xmlconformance.Where(c => c.Item2 == false).Count() > 0)
                    {
                        return xml;
                    }
                    else
                    {
                        List<XElement> xmldataFragments = new List<XElement>();
                        xmlconformance.ForEach(c => xmldataFragments.Add(XElement.Parse(c.Item1)));
                        return xmldataFragments;
                    }

                }
            }
            catch (XmlException)
            {
                return xml;
            }
        }


        public void SetValueOfAll(string name, string value)
        {

            XElement valueXML = null;
            string replaceValue = string.Empty;
            bool isXml = false;

            try
            {
                valueXML = XElement.Parse(value);
                replaceValue = valueXML.ToString();
                isXml = true;
            }
            catch { replaceValue = value; }


            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(replaceValue))
            {

                var matches = xmlData.Descendants(name);

                matches.ToList().ForEach(c =>
                {
                    if (isXml)
                    {
                        c.ReplaceNodes(valueXML);
                    }
                    else
                    {
                        c.SetValue(replaceValue);
                    }
                });

            }


        }
        /// <summary>
        /// Adds an element to the current XmlData property
        /// </summary>
        /// <param name="response">The UnlimitedObject that contains the XElement that we want to embed into the current XmlData propery</param>
        ///  <exception cref="System.ArgumentNullException"></exception>
        public void AddResponse(UnlimitedObject response)
        {
            Exceptions.ThrowArgumentNullExceptionIfObjectIsNull("response", response);
            try
            {
                xmlData.Add(response.xmlData);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Add(UnlimitedObject addXmlObject)
        {
            AddResponse(addXmlObject);
        }

        public void Add(XElement add)
        {
            xmlData.Add(add);
        }
        /// <summary>
        /// Removes the first child object with the specified tag name;
        /// </summary>
        /// <param name="tagName">The name of the element to find</param>
        ///  <exception cref="System.InvalidOperationException"></exception> 
        public void RemoveElementByTagName(string tagName)
        {
            var tag = xmlData.Element(tagName);
            if (xmlData.Element(tagName) != null)
            {
                xmlData.Element(tagName).Remove();
            }
        }

        public void RemoveElementsByTagName(string tagName)
        {
            //Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("tagName", tagName);

            if (IsValidElementOrAttributeName(tagName))
            {

                IEnumerable<XElement> nodes = xmlData.DescendantsAndSelf(tagName);

                if ((nodes != null) && nodes.Count() > 0)
                {
                    nodes.Remove();
                }

            }

        }


        /// <summary>
        /// Parses a string as xml and loads into this instance of the UnlimitedObject
        /// </summary>
        /// <param name="XmlData">The string to parse as xml</param>
        public void Load(string XmlData)
        {
            try
            {
                xmlData = XElement.Parse(XmlData);
            }
            catch (Exception ex)
            {
                xmlData = new XElement("XmlData", new XElement("Error", ex.Message));
            }
        }

        public void Load(XElement data)
        {
            xmlData = data;
        }

        /// <summary>
        /// Get the name of the current element
        /// </summary>
        /// <returns>String</returns>
        public string GetTagName()
        {
            string returnValue = string.Empty;
            if (xmlData != null)
            {
                returnValue = xmlData.Name.LocalName;
            }

            return returnValue;
        }

        /// <summary>
        /// Recursively reflects over an object and returns an xml representation of its members
        /// </summary>
        /// <param name="dataObject">The object that we want to return and xml representation of</param>
        /// <returns>string containing XML representation of the object</returns>
        public string GetXmlDataFromObject(object dataObject)
        {
            //We are going be using Linq to XML to achieve the xml representation of an object
            //This variable will be used to store the xml data retrieved by the reflection
            XElement xmlData = null;

            //Set the root node of the xml document that we are building
            //to the type name of the object.
            //e.g if the object is being passed in is a SqlException type then the
            //root tag will be SqlException
            xmlData = new XElement(dataObject.GetType().Name);

            //Iterate through every property in the object 
            //and process it into an xml representation.
            //If the property if of a type that implements 
            //ICollection in its inheritance/implementation
            //hierarchy e.g string[] or List<object> then 
            //process each item in the collection recursively 
            //by calling this method again to process the complex type
            dataObject.GetType().GetProperties().ToList().ForEach(e =>
            {
                try
                {
                    //Get property itself - not that we are not looking to the property
                    //type, but the property itself
                    object objectInst = e.GetValue(dataObject, null);

                    //Process 
                    if (objectInst is ICollection)
                    {
                        foreach (object item in (objectInst as IEnumerable))
                        {
                            if (item.GetType().IsValueType || item is string)
                            {
                                if (xmlData.Descendants(e.Name).Count() == 0)
                                {
                                    xmlData.Add(new XElement(e.Name));
                                }
                                xmlData.Descendants(e.Name).FirstOrDefault().Add(new XElement(item.GetType().Name, item));
                            }
                            else
                            {
                                xmlData.Add(XElement.Parse(GetXmlDataFromObject(item)));
                            }
                        }
                    }
                    else
                    {
                        xmlData.Add(new XElement(e.Name, objectInst));
                    }
                }
                catch (Exception ex)
                {
                    xmlData.Add(new XElement("ExceptionSourceProperty", e.Name));
                    xmlData.Add(XElement.Parse(GetXmlDataFromObject(ex)));
                }
            });

            return xmlData.ToString();
        }

        #endregion

        #region Overridden methods from the DynamicObject Class

        public override IEnumerable<string> GetDynamicMemberNames()
        {

            return base.GetDynamicMemberNames();
        }

        #region TryGetMember
        /// <summary>
        /// This method is what facilitates the dynamic ability of this object. It gets called whenever you refer to a property or method.
        /// The method returns a value to be bound against the property requested. If you want the binding to fail at runtime then 
        /// set the return value to false.
        /// </summary>
        /// <param name="binder">The object that contains the metadata of the dynamic propery being referenced.</param>
        /// <param name="tmp">The value to bind the property to</param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            bool returnVal = false;
            result = null;
            if (binder == null)
                return true;

            //Check if there is a match in the xmldocument either node or attribute with the name of the property the developer is requesting
            var match = xmlData.DescendantsAndSelf(binder.Name);

            if (match.Count() > 0)
            {
                //If there is a single match that has children or attributes then return a generic list containing a single UnlimitedObject
                //We do this to enable iteration.
                if (match.Count() == 1)
                {
                    if (match.First().Descendants().Count() > 0 || match.First().HasAttributes)
                    {
                        //If the match has attributes return a generic list as this makes
                        //iterations simple using a foreach
                        if (match.First().HasAttributes)
                        {
                            result = new List<UnlimitedObject> { new UnlimitedObject(match.FirstOrDefault()) };
                        }
                        //If the match does not have attributes we 
                        //can safely assume that the application
                        //is creating an xml document and does not require
                        //a generic list of unlimited objects
                        //If we didn't do this then the notation of 
                        //XmlRequest.AccountInfo.AccountNumber will fail 
                        //as AccountInfo will return a generic list and 
                        //assigning Accountnumber to generic list will 
                        //throw an exception.
                        else
                        {
                            result = new UnlimitedObject(match.FirstOrDefault());

                        }
                    }
                    else
                    {
                        //There is only 1 element match - we should return the value of that match
                        result = match.FirstOrDefault().Value;
                    }
                    returnVal = true;
                }

                //There are multiple element matches - we should create a new UnlimitedDynamic object per match
                if (match.Count() > 1)
                {
                    List<UnlimitedObject> matches = new List<UnlimitedObject>();
                    foreach (XElement subelement in match)
                    {
                        matches.Add(new UnlimitedObject(subelement));
                    }
                    result = matches;
                    returnVal = true;
                }

            }
            else
            {
                if (xmlData.Attribute(XName.Get(binder.Name)) != null)
                {
                    result = xmlData.Attribute(XName.Get(binder.Name)).Value;
                    returnVal = true;
                }
                else
                {

                    //There is no matching element in the xml document - we create it.
                    XElement newelement = new XElement(binder.Name);
                    xmlData.Add(newelement);
                    result = new UnlimitedObject(newelement);
                    returnVal = true;
                }
            }


            return returnVal;
        }
        #endregion TryGetMember

        #region TrySetMember
        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            bool returnVal = false;
            bool StateChanged = false;

            XElement match = xmlData.Element(binder.Name);
            if (match != null)
            {
                if (value != null)
                {
                    if (value is UnlimitedObject)
                    {
                        match.Add(new XElement(binder.Name));
                        return returnVal;
                    }
                    try
                    {
                        XElement data = XElement.Parse(value.ToString());
                        match.ReplaceNodes(data);
                    }
                    catch (XmlException)
                    {
                        match.SetValue(value.ToString());
                    }
                }
                returnVal = true;
                StateChanged = true;
            }
            else
            {
                XAttribute attMatch = xmlData.Attribute(XName.Get(binder.Name));
                if (attMatch != null)
                {
                    attMatch.SetValue(value.ToString());
                    returnVal = true;
                    StateChanged = true;
                }
                else
                {

                    if (value is UnlimitedObject)
                    {
                        xmlData.Add(new XElement(binder.Name, (value as UnlimitedObject).xmlData));
                        returnVal = true;
                    }
                    else
                    {
                        if (value != null)
                        {
                            xmlData.Add(new XElement(binder.Name, value.ToString()));
                        }
                        returnVal = true;
                        StateChanged = true;
                    }
                }
            }

            if (StateChanged && this.ObjectState != enObjectState.NEW)
            {
                this.ObjectState = enObjectState.CHANGED;
            }

            return returnVal;
        }
        #endregion TrySetMember

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            //tmp = new UnlimitedObject(xmlData.Elements().ElementAt((int)indexes[0]));
            result = new UnlimitedObject(xmlData.Element(XName.Get((string)indexes[0])));
            return true;
            //return base.TryGetIndex(binder, indexes, out tmp);
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            switch (binder.Operation)
            {
                case ExpressionType.RightShift:


                    break;

                case ExpressionType.Equal:

                    break;

                default:
                    throw new InvalidOperationException("Unsupported operation specified");

            }

            return base.TryBinaryOperation(binder, arg, out result);
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Returns a service request that can be executed at the dsf
        /// </summary>
        /// <param name="serviceName">The name of the dsf service to invoke</param>
        /// <param name="dataTags">The data tags that is a comma separated list of required service parameter names</param>
        /// <param name="workflowInputData">The string xml data passed into the workflow when the it was invoked</param>
        /// <param name="lastResult">The string xml data from the previous activity execution </param>
        /// <param name="resultPipeLine">The string xml data from all previous activity execution</param>
        /// <returns>String xml data containing a request that can be executed in the DSF</returns>
        public static string GenerateServiceRequest(string serviceName, string dataTags, string workflowInputData, string lastResult, string resultPipeLine, IDSFDataObject parentRequest)
        {

            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("serviceName", serviceName);

            //Create unlimited objects from all the string xml data that we need to
            //search to retrieve parameter values to pass to the dsf service
            List<dynamic> dataSources = new List<dynamic>{
                GetStringXmlDataAsUnlimitedObject(workflowInputData),
                GetStringXmlDataAsUnlimitedObject(lastResult),
                GetStringXmlDataAsUnlimitedObject(resultPipeLine)
            };

            //Retrieve the csv of parameternames as an unlimited object
            dynamic dataTagObj = GetCsvAsUnlimitedObject(dataTags);
            //Return a service request string.
            return GenerateServiceRequest(serviceName, dataTagObj, dataSources, parentRequest);
        }

        /// <summary>
        /// Returns a service request that can be executed at the dsf
        /// </summary>
        /// <param name="serviceName">The name of the dsf service to invoke</param>
        /// <param name="dataTags">An UnlimitedObject wrapping the csv data as xml</param>
        /// <param name="dataSources">A List of UnlimitedObjects to search for parameter values</param>
        /// <returns>String xml data containing a request that can be executed in the DSF</returns>
        public static string GenerateServiceRequest(string serviceName, dynamic dataTags, List<dynamic> dataSources, IDSFDataObject parentRequest)
        {
            //Create an UnlimitedObject that will wrap a dsf request
            dynamic serviceRequest = new UnlimitedObject();
            //Set the name of the dsf service to execute
            serviceRequest.Service = serviceName;

            dynamic tags = dataTags.dt;
            string dataValue = string.Empty;

            //Traverse the list of unlimitedObjects for each data tag
            //A data tag in this xml document contains a parameter name
            //that the dsf service is expecting
            if (tags is List<UnlimitedObject>)
            {
                foreach (dynamic dataTag in tags)
                {
                    //Search the datasources for the current parameter's value 
                    dataValue = GetDataValueFromUnlimitedObject(dataTag.dt, dataSources);
                    //Set the dsf requests parameter value
                    serviceRequest.GetElement(dataTag.dt).SetValue(dataValue);
                }
            }

            //There is only a single parameter
            //Search for the parameters value and set it in the dsf request
            if (tags is string)
            {
                dataValue = GetDataValueFromUnlimitedObject(tags, dataSources);
                serviceRequest.GetElement(tags).SetValue(dataValue);
            }

            //
            // Set all the inherited fields from the parent on the new request
            //
            if (parentRequest != null)
            {
                serviceRequest.BookmarkExecutionCallbackID = parentRequest.BookmarkExecutionCallbackID;
                serviceRequest.WorkspaceID = parentRequest.WorkspaceID;
            }

            return serviceRequest.XmlString;
        }

        /// <summary>
        /// Returns a service request that can be executed at the dsf
        /// </summary>
        /// <param name="serviceName">The name of the dsf service to invoke</param>
        /// <param name="dataTags">An UnlimitedObject wrapping the csv data as xml</param>
        /// <param name="dataSources">A List of UnlimitedObjects to search for parameter values</param>
        /// <returns>String xml data containing a request that can be executed in the DSF</returns>
        public static string GenerateServiceRequest(string serviceName, dynamic dataTags, List<string> ambientDataList, IDSFDataObject parentRequest)
        {
            dynamic input = new UnlimitedObject("Dev2ServiceInput");

            ambientDataList.ForEach(data => input.Add(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(data)));
            Sanitize(input);
            input.Service = serviceName;

            //
            // Set all the inherited fields from the parent on the new request
            //
            if (parentRequest != null)
            {
                input.BookmarkExecutionCallbackID = parentRequest.BookmarkExecutionCallbackID;
                input.WorkspaceID = parentRequest.WorkspaceID;
                input.IsDataListScoped = parentRequest.IsDataListScoped;
                input.ParentServiceName = parentRequest.ParentServiceName;
                input.ParentInstanceID = parentRequest.ParentInstanceID;
                input.ParentWorkflowInstanceId = parentRequest.ParentWorkflowInstanceId;
                input.IsDebug = parentRequest.IsDebug;
            }

            return input.XmlString;
        }


        public static void Sanitize(UnlimitedObject dataToSanitize)
        {
            dataToSanitize.RemoveElementsByTagName("Service");
            //dataToSanitize.RemoveElementsByTagName("Data"); // Travis : 03-07-2012 : Removed due to DataList data removal issues, this is now a valid tag!
        }

        /// <summary>
        /// Returns the value of an element in the xml that an UnlimitedObject is wrapping
        /// </summary>
        /// <param name="tagName">The name of the element to return the value of</param>
        /// <param name="unlimitedObjectSource">The UnlimitedObject that contains the element to to return the value of</param>
        /// <returns>A string that represents the value of element that we searched for</returns>
        public static string GetDataValueFromUnlimitedObject(string tagName, dynamic unlimitedObjectSource)
        {
            dynamic dataValue = string.Empty;

            if (string.IsNullOrEmpty(tagName))
            {
                return string.Empty;
            }

            if (unlimitedObjectSource != null)
            {

                if (unlimitedObjectSource is UnlimitedObject)
                {
                    dataValue = unlimitedObjectSource.GetValue(tagName);
                }

                if (unlimitedObjectSource is List<string>)
                {
                    //(unlimitedObjectSource as List<string>).Reverse();

                    foreach (string ambientDataItem in (unlimitedObjectSource as List<string>))
                    {
                        if (!string.IsNullOrEmpty(ambientDataItem))
                        {

                            dataValue = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(ambientDataItem).GetValue(tagName);
                            if (!string.IsNullOrEmpty(dataValue))
                            {
                                break;
                            }
                        }

                    }
                }
            }

            else
            {
                dataValue = null;
            }

            return dataValue;

        }

        /// <summary>
        ///  Returns the value of an element in the xml by searching a list of UnlimitedObject data sources
        /// </summary>
        /// <param name="tagName">The name of the element to return the value of.</param>
        /// <param name="searchUnlimitedObjects">A list of UnlimitedObject data sources to seach</param>
        /// <returns>A string that represents the value of element that we searched for</returns>
        public static string GetDataValueFromUnlimitedObject(string tagName, List<dynamic> searchUnlimitedObjects)
        {
            bool gotValue = false;

            string dataValue = string.Empty;
            if (searchUnlimitedObjects != null)
            {
                foreach (dynamic dataObject in searchUnlimitedObjects)
                {
                    if (dataObject != null)
                    {
                        dataValue = GetDataValueFromUnlimitedObject(tagName, dataObject);
                        if (!string.IsNullOrEmpty(dataValue))
                        {
                            gotValue = true;
                            break;
                        }
                    }
                }
            }

            if (!gotValue)
            {
                throw new ArgumentException("Could not find argument value in any workflow data source.", tagName);
            }

            return dataValue;
        }

        /// <summary>
        /// Returns an UnlimitedObject that wraps a string of xml data
        /// </summary>
        /// <param name="xmlData">The string xml data</param>
        /// <returns>UnlimiteObject that wraps the xml string data</returns>
        public static dynamic GetStringXmlDataAsUnlimitedObject(string xmlData)
        {
            if (string.IsNullOrEmpty(xmlData))
            {
                return new UnlimitedObject("Empty");
            }

            UnlimitedObject dataObject;

            try
            {
                var xElement = XElement.Parse(xmlData); 
                dataObject = new UnlimitedObject(xElement);
            }
            catch (XmlException ex)
            {
                try
                {
                    dataObject = new UnlimitedObject(XElement.Parse("<DataList>" + xmlData + "</DataList>"));
                }
                catch (XmlException)
                {
                    dataObject = new UnlimitedObject(XElement.Parse("<XmlData><Error>" + ex.Message + "</Error></XmlData>"));
                }
            }
            //dataObject.Load(xmlData);

            return dataObject;
        }

        //This method exists only for the business design studio
        //Its short name makes it more understandable in that environment
        //when switches need to be understood

        // PBI : 5376 - Kept this method for decision nodes....
        // Just hi-jacked the functionality so we can passing dataSource as the dataListID ;)
        public static string Get(string tagName, dynamic dataSource)
        {
            // TODO : Hi-jack the DataListID to look up from....

            Guid dataListID;
            IList<string> tmp = dataSource as IList<string>;
            string result = string.Empty;

            if (tmp != null)
            {
                Guid.TryParse((tmp[0] as string), out dataListID);

                // all good, fetch the data for evaluation ;)
                if (dataListID != GlobalConstants.NullDataListID)
                {
                    ErrorResultTO errors = new ErrorResultTO();
                    string evalExp = DataListUtil.AddBracketsToValueIfNotExist(tagName);
                    IBinaryDataListEntry val = _compiler.Evaluate(dataListID, enActionType.User, evalExp, false, out errors);
                    if (errors.HasErrors())
                    {
                        TraceWriter.WriteTrace("Dev2 Expression Fetch Error : " + errors.MakeUserReady());
                    }

                    if (val != null)
                    {
                        if (!val.IsRecordset)
                        {
                            result = val.FetchScalar().TheValue;
                        }
                        else
                        {
                            string error = string.Empty;
                            result = val.TryFetchLastIndexedRecordsetUpsertPayload(out error).TheValue;
                            if (error != string.Empty)
                            {
                                TraceWriter.WriteTrace("Dev2 Expression Fetch Error : " + error);
                            }
                        }
                    }

                }
            }

            return result;

            //return GetDataValueFromUnlimitedObject(tagName, dataSource);
        }

        /// <summary>
        /// Returns UnlimitedObject that wraps and xml interpretation of the csv string
        /// </summary>
        /// <param name="csv">The csv string</param>
        /// <returns>UnlimitedObject that wraps the csv data as xml</returns>
        public static dynamic GetCsvAsUnlimitedObject(string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return new UnlimitedObject();
            }
            csv = "<rt><dt>" + csv.Replace(",", "</dt><dt>") + "</dt></rt>";
            return GetStringXmlDataAsUnlimitedObject(csv);
        }

        public static UnlimitedObject GetUnlimitedObjectFromUnlimitedObjects(IList<UnlimitedObject> unlimitedObjects)
        {
            dynamic dataObj = new UnlimitedObject();

            unlimitedObjects.ToList().ForEach(uobj => dataObj.AddResponse(uobj));

            return dataObj;

        }
        #endregion
    }
}
