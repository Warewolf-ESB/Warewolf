using System;

namespace Dev2.Data.SystemTemplates
{
    public class SystemTemplate : ISystemTemplate
    {
        public string TemplateData { get; private set; }

        internal SystemTemplate(string data)
        {
            TemplateData = data;
        }
    }
}
