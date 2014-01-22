using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Newtonsoft.Json.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.Framework.Converters.Graph.String.Json;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Unlimited.Framework.Converters.Graph.String
{
    public class StringInterrogator : IInterrogator
    {
        #region Methods

        public IMapper CreateMapper(object data)
        {
            IMapper mapper;

            if(IsXml(data.ToString()))
            {
                mapper = new XmlMapper();
            }
            else if(IsJson(data.ToString()))
            {
                mapper = new JsonMapper();
            }
            else
            {
                mapper = data.GetType().IsPrimitive ? new PocoMapper() : null;
            }
            return mapper;
        }

        public INavigator CreateNavigator(object data, Type pathType)
        {
            if(!pathType.GetInterfaces().Contains(typeof(IPath)))
            {
                throw new Exception("'" + pathType.ToString() + "' doesn't implement '" + typeof(IPath).ToString() + "'");
            }

            INavigator navigator;

            if(pathType == typeof(XmlPath))
            {
                navigator = new XmlNavigator(data);
            }
            else if(pathType == typeof(JsonPath))
            {
                navigator = new JsonNavigator(data);
            }
            else if(pathType == typeof(PocoPath))
            {
                navigator = new PocoNavigator(data);
            }
            else
            {
                navigator = null;
            }

            return navigator;
        }

        #endregion Methods

        #region Private Methods

        private bool IsXml(string data)
        {
            bool result = true;

            try
            {
                XDocument.Parse(data);
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                result = false;
            }

            return result;
        }

        private bool IsJson(string data)
        {
            bool result = true;

            try
            {
                JToken.Parse(data);
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                result = false;
            }

            return result;
        }

        #endregion Private Methods
    }
}
