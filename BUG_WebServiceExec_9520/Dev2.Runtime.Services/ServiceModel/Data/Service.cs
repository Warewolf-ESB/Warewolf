using System.Xml.Linq;

namespace Dev2.Runtime.ServiceModel.Data
{
    // DO NOT override ToXml() here!
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

        public ServiceMethod Method { get; set; }
    }
}
