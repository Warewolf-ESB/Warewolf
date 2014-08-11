using System;
using Dev2.DataList.Contract.Binary_Objects;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Session
{

    [Serializable]
    public class DebugTO
    {
        #region Fields

        private string _xmlData;

        #endregion Fields

        #region Properties

        public string WorkflowXaml { get; set; }

        public string DataList { get; set; }

        public string ServiceName { get; set; }

        public bool IsDebugMode { get; set; }

        public bool RememberInputs { get; set; }

        public string BaseSaveDirectory { get; set; }

        public string Error { get; set; }

        public int DataListHash { get; set; }

        public string XmlData
        {
            get
            {
                return _xmlData ?? (_xmlData = DataList);
            }
            set
            {
                _xmlData = value;
            }
        }

        public IBinaryDataList BinaryDataList { get; set; }

        public string WorkflowID { get; set; }

        public Guid ResourceID { get; set; }

        public Guid ServerID { get; set; }
        public Guid SessionID { get; set; }

        #endregion Properties

        #region Methods

        public SaveDebugTO CopyToSaveDebugTO()
        {
            SaveDebugTO that = new SaveDebugTO { DataList = DataList, ServiceName = ServiceName, IsDebugMode = IsDebugMode, RememberInputs = RememberInputs, XmlData = XmlData, WorkflowID = WorkflowID };

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
            RememberInputs = that.RememberInputs;
            DataListHash = that.DataListHash;
        }


        #endregion Methods

    }
}
