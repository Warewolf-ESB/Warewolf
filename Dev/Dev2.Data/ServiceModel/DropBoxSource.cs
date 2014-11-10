using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Data.ServiceModel
{
    // ReSharper disable once UnusedMember.Global
    public class DropBoxSource : Resource
    {
        #region Properties

        // ReSharper disable once MemberCanBePrivate.Global
        public string Secret { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public string Key { get; set; }


        #endregion

        #region CTOR

        public DropBoxSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = Common.Interfaces.Data.ResourceType.DropBoxSource;
        }

        public DropBoxSource(XElement xml)
            : base(xml)
        {
            ResourceType = Common.Interfaces.Data.ResourceType.DropBoxSource;


            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Secret", string.Empty },
                { "Key", string.Empty }
            };

            ParseProperties(xml.AttributeSafe("ConnectionString"), properties);

            Secret = properties["Secret"];
            Key = properties["Key"];
   

        }

       
        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                string.Format("Secret={0}", Secret),
                string.Format("Key={0}", Key)
                );

            result.Add(
                new XAttribute("ConnectionString", connectionString),
                new XAttribute("Type", ResourceType),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        #endregion

    }
}