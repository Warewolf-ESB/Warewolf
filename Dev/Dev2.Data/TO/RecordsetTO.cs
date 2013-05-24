using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using Dev2.Common;

namespace Dev2.DataList.Contract
{
    public class RecordsetTO : IRecordsetTO {
        private string _recordsetName;
        private int _recordsetCount;
        private int _currentIndex;
        private bool _isEmpty = true;
        private bool _initailEmpty = false;
        private XmlDocument xDoc;
        private readonly IList<IRecordSetDefinition> _cols;

        private readonly string rootName = "temp";
        private readonly string recsetRootName = "Dev2Recset";


        public IList<IRecordSetDefinition> Def {
            get {
                return _cols;
            }
        }

        internal RecordsetTO(string recordsetString, IEnumerable<IRecordSetDefinition> cols, int currentIndex) {

            if (cols == null) {
                throw new Exception("Null columns for recordset");
            }
            _cols = cols.ToList();
            try {
                xDoc = new XmlDocument();
                xDoc.LoadXml(recordsetString);
                if (xDoc.DocumentElement.HasChildNodes) {
                    _recordsetCount = xDoc.DocumentElement.ChildNodes.Count;
                    _recordsetName = xDoc.DocumentElement.FirstChild.Name;
                }
                else {
                    _recordsetCount = 0;
                    _recordsetName = string.Empty;
                }
                _currentIndex = currentIndex;
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;
            }
        }

        public string RecordsetName {
            get {
                _recordsetCount = xDoc.DocumentElement.ChildNodes.Count;
                return _recordsetName;
            }
            private set {
                _recordsetName = value;
            }
        }

        public bool InitailEmpty {
            get {
                return _initailEmpty;
            }
            set {
                _initailEmpty = value;
            }
        }

        public int RecordCount {
            get {
                return xDoc.DocumentElement.ChildNodes.Count;
            }
            private set {
                _recordsetCount = value;
            }
        }

        public int CurrentIndex {
            get {
                return _currentIndex;
            }
            set {
                _currentIndex = value;
            }
        }

        public bool IsEmpty {
            get {
                //Checks to see if the is any data in the recordset
                if (xDoc.DocumentElement.ChildNodes.Count == 1) {
                    foreach (XmlNode item in xDoc.DocumentElement.ChildNodes[0].ChildNodes) {
                        if (!string.IsNullOrEmpty(item.InnerXml)) {
                            _isEmpty = false;
                        }
                    }
                }
                else {
                    _isEmpty = false;
                }
                return _isEmpty;
            }
            private set {
                _isEmpty = value;
            }
        }

        public string RecordsetAsString {
            get {
                string _recsetString = xDoc.OuterXml.Replace(string.Concat("<", recsetRootName, ">"), "").Replace(string.Concat("</", recsetRootName, ">"), "");
                return _recsetString;
            }
            private set {
                xDoc.LoadXml(value);
            }
        }

        public void InsertBodyAtIndex(int IndexNumber, string PayLoad) {
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(string.Concat("<", rootName, ">", PayLoad, "</", rootName, ">"));
                int tempIndex = IndexNumber - 1;
                XmlNode candidateNode;
                List<XmlNode> appendedNodeList = new List<XmlNode>();
                //Checks to see if the index number passed in is higher then the number of records in the recordset
                if (tempIndex > xDoc.DocumentElement.ChildNodes.Count) {
                    int childCount = xDoc.DocumentElement.ChildNodes.Count;
                    //create blank record templates for the number of records needed to add to the recordset
                    while (tempIndex >= childCount) {                       
                        appendedNodeList.Add(CreateTemplateRecord(xDoc.DocumentElement.FirstChild));
                        childCount++;

                    }
                    candidateNode =appendedNodeList[0] ;
                }
                else {
                    //if the index number that is passed in does exist in the record set
                    candidateNode = xDoc.DocumentElement.ChildNodes[tempIndex];
                }
               // XmlNode candidateNode = xDoc.DocumentElement.ChildNodes[tempIndex];
                int count = 0;
                StringBuilder tmpRegion = new StringBuilder();
                IDev2Definition[] tmpDef = _cols.First().Columns.ToArray();
                XmlNode tmpNode = null;
                /*
                 * Check that we have the correct shape in the Payload
                 * Then select the matching nodes for inclusion in the insert operation
                 */
                int childrenCount = candidateNode.ChildNodes.Count;
                while (count < childrenCount
                    && (tmpNode = xmlDoc.SelectSingleNode(string.Concat("//", tmpDef[count].Name))) != xmlDoc.DocumentElement) {
                    if (tmpNode != null) {
                        tmpRegion.Append(tmpNode.OuterXml);
                    }
                    count++;
                }

                // only add matched regions if we had the correct shape in the payload
                if (count == candidateNode.ChildNodes.Count) {
                    //if the index number did exist then it just write in at that index
                    if (appendedNodeList.Count == 0) {
                        candidateNode.InnerXml = tmpRegion.ToString();
                    }
                    else {
                        //otherwise it will inject the info at all the new records that were created and add them to the record set
                        foreach (XmlNode item in appendedNodeList) {
                            item.InnerXml = tmpRegion.ToString();
                            xDoc.DocumentElement.AppendChild(item);
                        }

                    }
                }

            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;
            }
        }

        /// <summary>
        /// Fetch a field at a specfic index
        /// </summary>
        /// <param name="IndexNumber"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public string FetchFieldAtIndex(int IndexNumber, string FieldName) {
            string result = string.Empty;

            if (IndexNumber <= xDoc.DocumentElement.ChildNodes.Count) {
                // result.Append("[[xpath('//" + recordsetName + "[" + idx + "]" + "/" + fieldName); 
                XmlNode n = xDoc.SelectSingleNode(string.Concat("//", _recordsetName, "[", IndexNumber.ToString(), "]/", FieldName));

                if (n != null) {
                    result = n.InnerXml;
                }
            }

            return result;
        }

        /// <summary>
        /// Inserts a value for a field at a specified index 
        /// </summary>
        /// <param name="IndexNumber"></param>
        /// <param name="FieldName"></param>
        /// <param name="PayLoad"></param>
        public void InsertFieldAtIndex(int IndexNumber, string FieldName, string PayLoad) {



            int tempIndex = IndexNumber - 1;
            //Checks to see if the index number passed in is higher then the number of records in the recordset
            if(tempIndex >= xDoc.DocumentElement.ChildNodes.Count) {
                int childCount = xDoc.DocumentElement.ChildNodes.Count;
                //create blank record templates for the number of records needed to add to the recordset
                while(tempIndex >= childCount) {
                    // what if FirstChild is null?
                    if(xDoc.DocumentElement.FirstChild != null) {
                        xDoc.DocumentElement.AppendChild(CreateTemplateRecord(xDoc.DocumentElement.FirstChild));
                        xDoc.DocumentElement.LastChild.SelectSingleNode(string.Concat("./", FieldName)).InnerText = PayLoad;
                        childCount++;
                    }
                    else {
                        throw new Exception("Null recodset.");
                    }
                }
                //candidateRecNode = appendedNodeList[0];
            }
            else {
                //if the index number that is passed in does exist in the record set
                xDoc.DocumentElement.ChildNodes[tempIndex].SelectSingleNode(string.Concat("./", FieldName)).InnerText = PayLoad;
            }
        }

        public string GetRecordAtIndex(int IndexNumber) {
            try {
                //returns the record at the index that is specified
                return xDoc.DocumentElement.ChildNodes[IndexNumber - 1].OuterXml;               
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;

            }
        }

        public string GetRecordAtCurrentIndex() {
            try {
                //returns the record at tht current index
                return xDoc.DocumentElement.ChildNodes[CurrentIndex -1].OuterXml;
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;

            }
        }

        public void InsertFieldAtCurrentIndex( string FieldName, string PayLoad) {
            try {
                //inserts the field payload at the current index
                InsertFieldAtIndex(CurrentIndex, FieldName, PayLoad);
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;

            }
        }

        public void InsertBodyAtCurrentIndex(string PayLoad) {
            try {
                //inserts the body payload at the current index
                InsertBodyAtIndex(CurrentIndex, PayLoad);
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;

            }
        }

        public void InsertWholeRecordset(string PayLoad) {
            try {
                //inserts a whole Recordset
                RecordsetAsString = string.Concat("<", recsetRootName, ">", PayLoad, "</", recsetRootName, ">");
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;

            }
        }

        private XmlNode CreateTemplateRecord(XmlNode RecordToMakeTemplateOf) {
            //creates a blank template of the record passed in
            XmlNode newNode = null;
            try {
                newNode = xDoc.CreateElement(RecordToMakeTemplateOf.Name);
                foreach (XmlNode item in RecordToMakeTemplateOf.ChildNodes) {
                    newNode.AppendChild(xDoc.CreateElement(item.Name));
                }
            }
            catch (Exception ex) {
                ServerLogger.LogError(ex);
                throw;
            }
            return newNode;
        }
    }
}
