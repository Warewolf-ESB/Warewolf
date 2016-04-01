using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Microsoft.Exchange.WebServices.Data;

namespace Dev2.Activities.Exchange
{
    public interface IExchangeEmailSender
    {
        void Send(ExchangeService service, EmailMessage message);
    }
}
