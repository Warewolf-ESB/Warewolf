using Dev2;
using Dev2.Data.Util;

namespace Unlimited.Framework
{

    #region Using Directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using Dev2.Common;


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
                var errorElement = xmlData.Descendants("Error");
                var result = false;

                errorElement
                    .ToList()
                    .ForEach(elm =>
                    {
                        if(elm.Value != null && elm.Value.Length > 0)
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
                var serviceElement = xmlData.Descendants("Service").Where(c => !c.HasAttributes);
                if(serviceElement.Count() > 1)
                {
                    return true;
                }
                return false;
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
                if(_parent == null)
                {
                    var parent = xmlData.Ancestors().FirstOrDefault();

                    if(parent != null)
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

                if(!IsMultipleRequests)
                {
                    return null;
                }

                var unlimitedReq = new List<UnlimitedObject>();

                var requests = xmlData.Descendants("Service").Where(c => !c.HasAttributes);

                var xElements = requests as XElement[] ?? requests.ToArray();
                if(!xElements.Any())
                {
                    return unlimitedReq;
                }

                var svc = new List<XElement>();
                var data = new List<string>();


                //Iterate through all messages that contain a Service tag
                //add them to a collection
                foreach(XElement d in xElements)
                {
                    XElement req = d.Ancestors().FirstOrDefault();

                    svc = new List<XElement>(req.Descendants("Service").Where(c => !c.HasAttributes));

                    if(req.Descendants("Service").Where(c => !c.HasAttributes).Count() > 1)
                    {
                        XElement requestData = new XElement(req);


                        foreach(XElement dsvc in req.Descendants("Service").Where(c => !c.HasAttributes))
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
                catch(Exception ex)
                {
                    this.LogError(ex);
                    throw;
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
                if(xmlData.FirstNode != null)
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
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            xmlData = xml;
            this.ObjectState = enObjectState.UNCHANGED;
        }

        /// <summary>
        /// Initializes a new UnlimitedObject class
        /// </summary>
        /// <param name="xml">Creates the root node named as the this string.</param>
        public UnlimitedObject(string xml)
        {
            if(string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }
            //Travis said so;)
            // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
            if(!xml.StartsWith("<"))
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
            VerifyArgument.IsNotNull("dataObject", dataObject);

            var element = dataObject as XElement;
            if(element != null)
            {
                xmlData = element;
                return;
            }

            xmlData = XElement.Parse(GetXmlDataFromObject(dataObject));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Inners this instance.
        /// </summary>
        /// <returns></returns>
        public string Inner()
        {
            using(var reader = xmlData.CreateReader())
            {
                reader.MoveToContent();
                string data = reader.ReadInnerXml();

                if(DataListUtil.IsXml(data))
                {
                    return XElement.Parse(data).ToString();
                }
                return data;
            }
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

            if(!IsValidElementOrAttributeName(elementName))
            {
                return false;
            }


            var results = xmlData.DescendantsAndSelf(elementName);
            return results.Count() > 0;
        }

        /// <summary>
        /// Gets the name of the root.
        /// </summary>
        /// <value>
        /// The name of the root.
        /// </value>
        public string RootName
        {
            get
            {
                return xmlData.Name.LocalName;
            }
        }


        /// <summary>
        /// Determines whether [is valid element or attribute name] [the specified name].
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if [is valid element or attribute name] [the specified name]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidElementOrAttributeName(string name)
        {
            double value = 0;
            if(double.TryParse(name, out value))
            {
                return false;
            }
            try
            {
                XName.Get(name);
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Get the value of node or attribute in the current XmlData propery
        /// </summary>
        /// <param name="name">The name of the attribute or element of which we are trying to get the value</param>
        /// <returns>string representation of the value</returns>
        ///  <exception cref="System.ArgumentException"></exception>
        ///  <exception cref="System.ArgumentNullException"></exception>
        public string GetValue(string name)
        {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("Name", name);

            if(!IsValidElementOrAttributeName(name))
            {
                return string.Empty;
            }

            var returnVal = string.Empty;

            var currentXMLData = xmlData;
            string s;
            if(HasAttributeValue(name, currentXMLData, out s))
            {
                return s;
            }
            var xElements = ExcludeDatalistFromNodes(name, currentXMLData);
            if(xElements.Count > 1)
            {
                var returnData = new UnlimitedObject();
                xElements.ToList().ForEach(mtch => returnData.Add(new UnlimitedObject(mtch)));

                returnVal = returnData.XmlString;
            }
            else
            {
                var xm = xElements.FirstOrDefault();
                //This is a complex string not just a value
                if(xm != null)
                {
                    if(xm.HasElements)
                    {
                        returnVal = xm.ToString();
                    }
                    else
                    {
                        return currentXMLData.Attribute(XName.Get(name)) != null ? currentXMLData.Attribute(XName.Get(name)).Value : xm.Value;
                        //This is a singleton value that we can return
                    }
                }
                else
                {
                    var attrib = currentXMLData.DescendantsAndSelf().Where(c => c.Attribute(name) != null);
                    var elements = attrib as IList<XElement> ?? attrib.ToList();
                    if(elements.Any())
                    {
                        var firstOrDefault = elements.FirstOrDefault();
                        if(firstOrDefault != null)
                        {
                            returnVal = firstOrDefault.Attribute(name).Value;
                        }
                    }
                }
            }
            return returnVal;
        }

        static IList<XElement> ExcludeDatalistFromNodes(string name, XElement currentXMLData)
        {
            var matches = currentXMLData.DescendantsAndSelf(name);
            var xElements = matches as IList<XElement> ?? matches.Where(element => (element.Parent != null && element.Parent.Name != "DataList") || element.Parent == null).ToList();
            return xElements;
        }

        static bool HasAttributeValue(string name, XElement currentXMLData, out string s)
        {
            s = null;
            var attributeOfProperty = currentXMLData.Attribute(name);
            if(attributeOfProperty == null)
            {
                return false;
            }
            s = attributeOfProperty.Value;
            return true;
        }

        /// <summary>
        /// Retrieves the specified element from the current xml data. If it does not exist, a new one is created with the name provided and returned
        /// </summary>
        /// <param name="Name">The Name of the Element to search for</param>
        /// <returns>UnlimitedObject containing the element found or a new element created</returns>
        public dynamic GetElement(string Name)
        {

            if(!IsValidElementOrAttributeName(Name))
            {
                return new UnlimitedObject("data");
            }

            XElement xm = xmlData.DescendantsAndSelf(Name).FirstOrDefault();

            var data = new UnlimitedObject();

            if(xm == null)
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

        /// <summary>
        /// Creates the element.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public dynamic CreateElement(string name)
        {
            if(!IsValidElementOrAttributeName(name))
            {

                return new UnlimitedObject("data");
            }

            XElement newelement = new XElement(name);
            xmlData.Add(newelement);
            return new UnlimitedObject(newelement);
        }


        /// <summary>
        /// Gets all elements.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public List<UnlimitedObject> GetAllElements(string name)
        {
            var dataList = new List<UnlimitedObject>();

            if(!IsValidElementOrAttributeName(name))
            {
                dataList.Add(new UnlimitedObject("data"));
                return dataList;
            }

            IEnumerable<XElement> xm = xmlData.Descendants(name);

            var data = new UnlimitedObject();

            if(xm == null)
            {
                XElement newelement = new XElement(name);
                xmlData.Add(newelement);
                dataList.Add(new UnlimitedObject(newelement));

                return dataList;
            }
            else
            {
                foreach(XElement x in xm)
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

            if(xmlData != null)
            {

                XElement valueXML = null;

                if(!dataValue.Contains("<") && !dataValue.Contains(">"))
                {
                    xmlData.SetValue(dataValue);
                    return returnValue;
                }

                Regex r = new Regex(@"<([^>]+)>[^<]*</(\1)>");
                if(!r.IsMatch(dataValue))
                {
                    xmlData.SetValue(dataValue);
                    return returnValue;
                }

                if(DataListUtil.IsXml(value))
                {
                    valueXML = XElement.Parse(value);
                    xmlData.ReplaceNodes(valueXML);
                }
                else
                {
                    dynamic delineateResult = DelineateXMLString(value);

                    if(delineateResult is List<XElement>)
                    {
                        var newNodes = delineateResult as List<XElement>;
                        if(newNodes != null)
                        {
                            xmlData.ReplaceAll(newNodes);
                        }
                    }
                    else
                    {
                        xmlData.SetValue(value ?? string.Empty);
                    }
                }


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
                using(var reader = XmlReader.Create(new StringReader(xml), settings))
                {
                    reader.Read();
                    while(!reader.EOF)
                    {

                        var test = string.Empty;

                        test = XNode.ReadFrom(reader).ToString();

                        if(!test.Equals("\r\n") && !test.EndsWith("\r\n"))
                        {
                            try
                            {
                                XElement.Parse(test);
                                xmlconformance.Add(new Tuple<string, bool>(test, true));
                            }
                            catch(XmlException ex)
                            {
                                ServerLogger.LogError("UnlimitedObject", ex);
                                xmlconformance.Add(new Tuple<string, bool>(test, false));
                            }
                        }
                        else
                        {
                            return xml;
                        }
                    }

                    if(xmlconformance.Where(c => c.Item2 == false).Count() > 0)
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
            catch(XmlException ex)
            {
                ServerLogger.LogError("UnlimitedObject", ex);
                return xml;
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
            catch(Exception e)
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
            if(xmlData.Element(tagName) != null)
            {
                xmlData.Element(tagName).Remove();
            }
        }

        public void RemoveElementsByTagName(string tagName)
        {

            if(IsValidElementOrAttributeName(tagName))
            {

                IEnumerable<XElement> nodes = xmlData.DescendantsAndSelf(tagName);

                if((nodes != null) && nodes.Count() > 0)
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
            catch(Exception ex)
            {
                xmlData = new XElement("XmlData", new XElement("Error", ex.Message));
            }
        }

        public void Load(XElement data)
        {
            xmlData = data;
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
                    if(objectInst is ICollection)
                    {
                        foreach(object item in (objectInst as IEnumerable))
                        {
                            if(item.GetType().IsValueType || item is string)
                            {
                                if(xmlData.Descendants(e.Name).Count() == 0)
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
                catch(Exception ex)
                {
                    xmlData.Add(new XElement("ExceptionSourceProperty", e.Name));
                    xmlData.Add(XElement.Parse(GetXmlDataFromObject(ex)));
                }
            });

            return xmlData.ToString();
        }

        public bool IsSuccessResponse()
        {
            string result;
            try
            {
                result = GetValue("DataList");
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                return false;
            }
            return result == "Success";
        }

        #endregion

        #region Overridden methods from the DynamicObject Class

        #region TryGetMember

        /// <summary>
        /// This method is what facilitates the dynamic ability of this object. It gets called whenever you refer to a property or method.
        /// The method returns a value to be bound against the property requested. If you want the binding to fail at runtime then 
        /// set the return value to false.
        /// </summary>
        /// <param name="binder">The object that contains the metadata of the dynamic propery being referenced.</param>
        /// <param name="tmp">The value to bind the property to</param>
        /// <param name="result">The resulting vallue for the property</param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var returnVal = false;
            result = null;

            //Check if there is a match in the xmldocument either node or attribute with the name of the property the developer is requesting
            var name = binder.Name;
            var currentXMLData = xmlData;
            string s;
            if(HasAttributeValue(name, currentXMLData, out s))
            {
                result = s;
                return true;
            }
            var xElements = ExcludeDatalistFromNodes(name, currentXMLData);
            if(xElements.Any())
            {
                //If there is a single match that has children or attributes then return a generic list containing a single UnlimitedObject
                //We do this to enable iteration.
                if(xElements.Count() == 1)
                {
                    if(xElements.First().Descendants().Any() || xElements.First().HasAttributes)
                    {
                        //If the match has attributes return a generic list as this makes
                        //iterations simple using a foreach
                        if(xElements.First().HasAttributes)
                        {
                            result = new List<UnlimitedObject> { new UnlimitedObject(xElements.FirstOrDefault()) };
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
                            result = new UnlimitedObject(xElements.FirstOrDefault());

                        }
                    }
                    else
                    {
                        //There is only 1 element match - we should return the value of that match
                        var xElement = xElements.FirstOrDefault();
                        if(xElement != null)
                        {
                            result = xElement.Value;
                        }
                    }
                    returnVal = true;
                }

                //There are multiple element matches - we should create a new UnlimitedDynamic object per match
                if(xElements.Count() > 1)
                {
                    List<UnlimitedObject> matches = new List<UnlimitedObject>();
                    foreach(XElement subelement in xElements)
                    {
                        matches.Add(new UnlimitedObject(subelement));
                    }
                    result = matches;
                    returnVal = true;
                }

            }
            else
            {
                if(xmlData.Attribute(XName.Get(binder.Name)) != null)
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
            if(match != null)
            {
                if(value != null)
                {
                    if(value is UnlimitedObject)
                    {
                        match.Add(new XElement(binder.Name));
                        return returnVal;
                    }
                    try
                    {
                        XElement data = XElement.Parse(value.ToString());
                        match.ReplaceNodes(data);
                    }
                    catch(XmlException ex)
                    {
                        this.LogError(ex);
                        match.SetValue(value.ToString());
                    }
                }
                returnVal = true;
                StateChanged = true;
            }
            else
            {
                XAttribute attMatch = xmlData.Attribute(XName.Get(binder.Name));
                if(attMatch != null)
                {
                    attMatch.SetValue(value.ToString());
                    returnVal = true;
                    StateChanged = true;
                }
                else
                {

                    if(value is UnlimitedObject)
                    {
                        xmlData.Add(new XElement(binder.Name, (value as UnlimitedObject).xmlData));
                        returnVal = true;
                    }
                    else
                    {
                        if(value != null)
                        {
                            xmlData.Add(new XElement(binder.Name, value.ToString()));
                        }
                        returnVal = true;
                        StateChanged = true;
                    }
                }
            }

            if(StateChanged && this.ObjectState != enObjectState.NEW)
            {
                this.ObjectState = enObjectState.CHANGED;
            }

            return returnVal;
        }
        #endregion TrySetMember

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            switch(binder.Operation)
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
        /// <param name="dataTags">An UnlimitedObject wrapping the csv data as xml</param>
        /// <param name="ambientDataList">A List of UnlimitedObjects to search for parameter values</param>
        /// <param name="parentRequest"></param>
        /// <returns>String xml data containing a request that can be executed in the DSF</returns>
        public string GenerateServiceRequest(string serviceName, dynamic dataTags, List<string> ambientDataList, IDSFDataObject parentRequest)
        {
            dynamic input = new UnlimitedObject("Dev2ServiceInput");

            ambientDataList.ForEach(data => input.Add(GetStringXmlDataAsUnlimitedObject(data)));
            Sanitize(input);
            input.Service = serviceName;

            //
            // Set all the inherited fields from the parent on the new request
            //
            if(parentRequest != null)
            {
                input.BookmarkExecutionCallbackID = parentRequest.BookmarkExecutionCallbackID;
                input.WorkspaceID = parentRequest.WorkspaceID;
                input.IsDataListScoped = parentRequest.IsDataListScoped;
                input.ParentServiceName = parentRequest.ParentServiceName;
                input.ParentInstanceID = parentRequest.ParentInstanceID;
                input.ParentWorkflowInstanceId = parentRequest.ParentWorkflowInstanceId;
                input.IsDebug = parentRequest.IsDebug;
                input.OriginalResourceID = parentRequest.ResourceID;
            }

            return input.XmlString;
        }


        /// <summary>
        /// Sanitizes the specified data to sanitize.
        /// </summary>
        /// <param name="dataToSanitize">The data to sanitize.</param>
        public void Sanitize(UnlimitedObject dataToSanitize)
        {
            dataToSanitize.RemoveElementsByTagName("Service");
        }

        /// <summary>
        /// Returns the value of an element in the xml that an UnlimitedObject is wrapping
        /// </summary>
        /// <param name="tagName">The name of the element to return the value of</param>
        /// <param name="unlimitedObjectSource">The UnlimitedObject that contains the element to to return the value of</param>
        /// <returns>A string that represents the value of element that we searched for</returns>
        public string GetDataValueFromUnlimitedObject(string tagName, dynamic unlimitedObjectSource)
        {
            dynamic dataValue = string.Empty;

            if(string.IsNullOrEmpty(tagName))
            {
                return string.Empty;
            }

            if(unlimitedObjectSource != null)
            {

                if(unlimitedObjectSource is UnlimitedObject)
                {
                    dataValue = unlimitedObjectSource.GetValue(tagName);
                }

                if(unlimitedObjectSource is List<string>)
                {

                    foreach(string ambientDataItem in (unlimitedObjectSource as List<string>))
                    {
                        if(!string.IsNullOrEmpty(ambientDataItem))
                        {

                            dataValue = GetStringXmlDataAsUnlimitedObject(ambientDataItem).GetValue(tagName);
                            if(!string.IsNullOrEmpty(dataValue))
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
        public string GetDataValueFromUnlimitedObject(string tagName, List<dynamic> searchUnlimitedObjects)
        {
            bool gotValue = false;

            string dataValue = string.Empty;
            if(searchUnlimitedObjects != null)
            {
                foreach(dynamic dataObject in searchUnlimitedObjects)
                {
                    if(dataObject != null)
                    {
                        dataValue = GetDataValueFromUnlimitedObject(tagName, dataObject);
                        if(!string.IsNullOrEmpty(dataValue))
                        {
                            gotValue = true;
                            break;
                        }
                    }
                }
            }

            if(!gotValue)
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
        public dynamic GetStringXmlDataAsUnlimitedObject(string xmlData)
        {
            if(string.IsNullOrEmpty(xmlData))
            {
                return new UnlimitedObject("Empty");
            }

            UnlimitedObject dataObject;

            try
            {
                var xElement = XElement.Parse(xmlData);
                dataObject = new UnlimitedObject(xElement);
            }
            catch(XmlException ex)
            {
                try
                {
                    dataObject = new UnlimitedObject(XElement.Parse("<DataList>" + xmlData + "</DataList>"));
                }
                catch(XmlException)
                {
                    dataObject = new UnlimitedObject(XElement.Parse("<XmlData><Error>" + ex.Message + "</Error></XmlData>"));
                }
            }

            return dataObject;
        }

        #endregion
    }
}
