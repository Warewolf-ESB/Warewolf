using System;

namespace Dev2.Session {
    [Serializable]
    public class SaveDebugTO {
        #region Properties

        public string WorkflowXaml { get; set; }

        public string DataList { get; set; }

        public string ServiceName { get; set; }

        public bool IsDebugMode { get; set; }

        public bool RememberInputs { get; set; }

        public string XmlData { get; set; }

        public string WorkflowID { get; set; }

        public int DataListHash { get; set; }

        #endregion Properties
    }
}
