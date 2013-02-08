
using System.Xml.Linq;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class MethodParameter
    {
        #region Properties

        public string Name { get; set; }
        public bool EmptyToNull { get; set; }
        public object Value { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }

        #endregion

        #region Methods

        public XElement ToXml()
        {
            return new XElement("");
        }

        #endregion

    }
}
