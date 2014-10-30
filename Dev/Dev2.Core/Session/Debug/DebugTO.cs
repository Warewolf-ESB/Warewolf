/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.DataList.Contract;
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
            get { return _xmlData ?? (_xmlData = DataList); }
            set { _xmlData = value; }
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
            var that = new SaveDebugTO
            {
                DataList = DataList,
                ServiceName = ServiceName,
                IsDebugMode = IsDebugMode,
                RememberInputs = RememberInputs,
                XmlData = XmlData,
                WorkflowID = WorkflowID
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
            RememberInputs = that.RememberInputs;
            DataListHash = that.DataListHash;
        }


        public virtual void CleanUp()
        {
            CleanUpCalled = true;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            if (BinaryDataList != null)
            {
                compiler.ForceDeleteDataListByID(BinaryDataList.UID);
                BinaryDataList.Dispose();
            }
        }

        public bool CleanUpCalled    { get; set; }

        #endregion Methods
    }
}