namespace Dev2.Runtime.WebServer.TransferObjects
{
    public class WebRequestTO
    {
        public string ServiceName { get; set; }
        public string InstanceID { get; set; }
        public string Bookmark { get; set; }
        public string WebServerUrl { get; set; }
        public string Dev2WebServer { get; set; }
        public string RawRequestPayload { get; set; }
    }
}
