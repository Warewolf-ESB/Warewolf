using System;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Session {
    [Serializable]
    public class DebugTO {
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

        public string XmlData {
            get {
                if (_xmlData == null) {
                    _xmlData = DataList;
                }
                return _xmlData;
            }
            set {
                _xmlData = value;
            }
        }

        public IBinaryDataList BinaryDataList { get; set; }

        public string WorkflowID { get; set; }

        #endregion Properties

        #region Ctor

        public DebugTO() {

        }

        #endregion Ctor

        #region Methods

        public SaveDebugTO CopyToSaveDebugTO() {
            SaveDebugTO that = new SaveDebugTO();

            that.DataList = this.DataList;
            that.ServiceName = this.ServiceName;
            that.IsDebugMode = this.IsDebugMode;
            that.RememberInputs = this.RememberInputs;
            that.XmlData = this.XmlData;
            that.WorkflowID = this.WorkflowID;
            that.RememberInputs = this.RememberInputs;
            that.DataListHash = this.DataListHash;

            return that;
        }

        public void CopyFromSaveDebugTO(SaveDebugTO that) {

            this.DataList = that.DataList;
            this.ServiceName = that.ServiceName;
            this.IsDebugMode = that.IsDebugMode;
            this.RememberInputs = that.RememberInputs;
            this.XmlData = that.XmlData;
            this.WorkflowID = that.WorkflowID;
            this.RememberInputs = that.RememberInputs;
            this.DataListHash = that.DataListHash;
        }


        #endregion Methods

    }
}
