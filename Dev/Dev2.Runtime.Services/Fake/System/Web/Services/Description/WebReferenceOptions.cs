using System.Collections.Generic;
using System.Xml.Serialization;

namespace System.Web.Services.Description
{
    internal class WebReferenceOptions
    {
        public WebReferenceOptions()
        {
        }

        public CodeGenerationOptions CodeGenerationOptions { get; internal set; }
        public List<object> SchemaImporterExtensions { get; internal set; }
    }
}