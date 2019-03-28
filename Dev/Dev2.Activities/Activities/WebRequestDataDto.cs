#pragma warning disable
using System.Activities;
using Dev2.Common.Interfaces;

namespace Dev2.Activities
{
    public interface IWebRequestData
    {
        WebRequestMethod WebRequestMethod { get; set; }
        string DisplayName { get; set; }
        InArgument<string> Type { set; get; }
    }
    public class WebRequestDataDto : IWebRequestData
    {
        WebRequestDataDto()
        {

        }
        public static WebRequestDataDto CreateRequestDataDto(WebRequestMethod requestMethod, InArgument<string> type, string displayName) => new WebRequestDataDto()
        {
            WebRequestMethod = requestMethod,
            DisplayName = displayName,
            Type = type
        };

        public WebRequestMethod WebRequestMethod { get; set; }
        public string DisplayName { get; set; }
        public InArgument<string> Type { get; set; }
    }
    
}