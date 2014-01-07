
namespace Dev2.Studio.Core.Services.Communication
{
    /// <summary>
    /// Message details to be sent through email
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <datetime>2013/01/14-09:12 AM</datetime>
    public class EmailCommMessage : ICommMessage
    {
        public string To { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string AttachmentLocation { get; set; }
    }
}
