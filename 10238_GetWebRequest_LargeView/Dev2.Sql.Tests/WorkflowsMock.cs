using System.Xml.Linq;
using Warewolf.Sql;

namespace Dev2.Sql.Tests
{
    public class WorkflowsMock : Workflows
    {
        public string ReturnXml { get; set; }

        public override XElement RunWorkflow(string requestUri)
        {
            return string.IsNullOrEmpty(ReturnXml) ? null : XElement.Parse(ReturnXml);
        }
    }
}
