using System;
using System.Linq;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Exchange.WebServices.Data;

namespace Dev2.Activities.Exchange
{
    public class ExchangeEmailSender : IExchangeEmailSender
    {
        private readonly IExchangeSource _source;

        public ExchangeEmailSender(IExchangeSource source)
        {
            this._source = source;
        }

        private void Initialize(ExchangeService service)
        {
            service.Credentials = new WebCredentials(_source.UserName, _source.Password);
            service.UseDefaultCredentials = false;
            service.TraceEnabled = false;
            service.TraceFlags = TraceFlags.None;
            
            service.AutodiscoverUrl(_source.UserName, RedirectionUrlValidationCallback);
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
