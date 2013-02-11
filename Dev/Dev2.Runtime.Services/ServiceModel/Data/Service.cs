using System.Xml.Linq;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Service : Resource
    {
        #region CTOR

        public Service()
        {
        }

        public Service(XElement xml)
            : base(xml)
        {
        }

        #endregion
    }
}
