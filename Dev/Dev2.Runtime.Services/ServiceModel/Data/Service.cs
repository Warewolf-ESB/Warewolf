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

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            if(Method != null)
            {
                result.Add(Method.ToXml());
            }

            return result;
        }

        #endregion

        public ServiceMethod Method { get; set; }
        public Recordset Recordset { get; set; }
    }
}
