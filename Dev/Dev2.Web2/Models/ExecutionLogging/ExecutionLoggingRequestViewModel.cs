using Dev2.Common;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Dev2.Web2.Models.ExecutionLogging
{
    public class ExecutionLoggingRequestViewModel: LogEntry
    {
        public ExecutionLoggingRequestViewModel()
        {
            Protocols = new List<SelectListItem>
            {
                new SelectListItem{Text = "https",Value="https"}
                ,
                new SelectListItem{Text = "http",Value="http" }
            };
        }
        public List<SelectListItem> Protocols { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string Protocol { get; set; }
        public string SortBy { get; set; }
    }
}