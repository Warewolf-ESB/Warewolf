using System;
using System.Xml.Linq;
using Dev2.DynamicServices;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbService : Service
    {
        #region CTOR

        public DbService()
        {
        }

        public DbService(XElement xml)
            : base(xml)
        {
            Source = new DbSource(xml.Element("Source"));
        }

        #endregion

        public DbSource Source { get; set; }

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.Add(Source.ToXml());

            return result;
        }

        #endregion

        #region CreateEmpty

        public static DbService Create()
        {
            return new DbService
            {
                ResourceID = Guid.Empty,
                ResourceType = enSourceType.SqlDatabase,
                Source = new DbSource { ResourceID = Guid.Empty, ResourceType = enSourceType.SqlDatabase }
            };
        }

        #endregion


    }
}