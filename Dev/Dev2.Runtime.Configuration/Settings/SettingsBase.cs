using System;
using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public abstract class SettingsBase
    {
        public string Name { get; private set; }

        #region CTOR

        protected SettingsBase(string name)
        {
            Name = name;
        }

        protected SettingsBase(XElement xml)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            Name = xml.AttributeSafe("Name");

        }

        #endregion

        #region ToXml

        public virtual XElement ToXml()
        {
            var result = new XElement("Setting",
                new XAttribute("Name", Name ?? string.Empty)
                );
            return result;
        }

        #endregion
    }
}