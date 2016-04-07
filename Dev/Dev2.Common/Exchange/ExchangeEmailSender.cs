using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Microsoft.Exchange.WebServices.Data;

namespace Dev2.Common.Exchange
{
    public class ExchangeEmailSender : IExchangeEmailSender
    {
        private readonly IExchangeSource _source;

        public ExchangeEmailSender() { }
        public ExchangeEmailSender(IExchangeSource source)
        {
            _source = source;
        }

        private void Initialize(ExchangeService service)
        {
            service.Credentials = new WebCredentials(_source.UserName, _source.Password);
            service.UseDefaultCredentials = false;
            service.TraceEnabled = false;
            service.TraceFlags = TraceFlags.None;
           
            if (!string.IsNullOrEmpty(_source.AutoDiscoverUrl))
            {
                service.Url = new Uri(_source.AutoDiscoverUrl);
            }
            else
            {
                service.AutodiscoverUrl(_source.UserName, RedirectionUrlValidationCallback);
            } 
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            var result = false;

            var redirectionUri = new Uri(redirectionUrl);

            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }

        public void Send(ExchangeService service, EmailMessage message)
        {
            Initialize(service);

            message.Send();
        }
    }
}
