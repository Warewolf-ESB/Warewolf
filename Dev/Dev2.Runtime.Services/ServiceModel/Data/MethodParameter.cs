
using System.Xml.Linq;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class MethodParameter
    {
        #region Constructor

        public MethodParameter() 
            : this("", false, false, "", "")
        {
        }

        public MethodParameter(string name, bool emptyToNull, bool isRequired, string value, string defaultValue)
        {
            Name = name;
            EmptyToNull = emptyToNull;
            IsRequired = isRequired;
            Value = value;
            DefaultValue = defaultValue;
        }

        #endregion Constructor

        #region Properties

        public string Name { get; set; }
        public bool EmptyToNull { get; set; }
        public bool IsRequired { get; set; }
        public string Value { get; set; }
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
