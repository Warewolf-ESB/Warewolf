#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Runtime.Serialization;
using Dev2.Data;




namespace Dev2.Session
{
    [DataContract]
    public class DebugTO
    {
        #region Fields

        string _xmlData;
        string _jsonData;

        #endregion Fields

        #region Properties

        [DataMember]
        public string WorkflowXaml { get; set; }

        [DataMember]
        public string DataList { get; set; }

        [DataMember]
        public string ServiceName { get; set; }

        [DataMember]
        public bool IsDebugMode { get; set; }

        [DataMember]
        public bool RememberInputs { get; set; }

        [DataMember]
        public string BaseSaveDirectory { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public int DataListHash { get; set; }

        [DataMember]
        public string XmlData
        {
            get { return _xmlData ?? (_xmlData = DataList); }
            set { _xmlData = value; }
        }

        [DataMember]
        public string JsonData
        {
            get => _jsonData;
            set => _jsonData = value;
        }

        [DataMember]
        public IDataListModel BinaryDataList { get; set; }

        [DataMember]
        public string WorkflowID { get; set; }

        [DataMember]
        public Guid ResourceID { get; set; }

        [DataMember]
        public Guid ServerID { get; set; }

        [DataMember]
        public Guid SessionID { get; set; }

        #endregion Properties

        #region Methods

        public SaveDebugTO CopyToSaveDebugTO()
        {
            var that = new SaveDebugTO
            {
                DataList = DataList,
                ServiceName = ServiceName,
                IsDebugMode = IsDebugMode,
                RememberInputs = RememberInputs,
                XmlData = XmlData,
                WorkflowID = WorkflowID,
                JsonData = JsonData
            };

            that.RememberInputs = RememberInputs;
            that.DataListHash = DataListHash;

            return that;
        }

        public void CopyFromSaveDebugTO(SaveDebugTO that)
        {
            DataList = that.DataList;
            ServiceName = that.ServiceName;
            IsDebugMode = that.IsDebugMode;
            RememberInputs = that.RememberInputs;
            XmlData = that.XmlData;
            WorkflowID = that.WorkflowID;
            DataListHash = that.DataListHash;
            JsonData = that.JsonData;
        }


        public virtual void CleanUp()
        {
            CleanUpCalled = true;
        }

        [DataMember]
        public bool CleanUpCalled    { get; set; }

        #endregion Methods
    }
}