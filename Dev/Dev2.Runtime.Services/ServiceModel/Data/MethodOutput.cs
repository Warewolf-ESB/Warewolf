using System;
using System.Xml.Linq;
using Dev2.DataList.Contract;

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class MethodOutput : IDev2Definition
    {
        #region Properties

        public string Name { get; private set; }

        public string MapsTo { get; private set; }

        public string Value { get; private set; }

        public bool IsRecordSet { get; private set; }

        public string RecordSetName { get; private set; }

        public bool IsEvaluated { get; private set; }

        public string DefaultValue { get; private set; }

        public bool IsRequired { get; private set; }

        public string RawValue { get; private set; }

        public bool EmptyToNull { get; private set; }

        #endregion

        #region Methods

        public XElement ToXml()
        {
            return new XElement("");
        }

        #endregion


    }
}
