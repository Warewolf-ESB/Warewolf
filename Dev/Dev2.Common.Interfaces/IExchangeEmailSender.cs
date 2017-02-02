using Microsoft.Exchange.WebServices.Data;

namespace Dev2.Common.Interfaces
{
    public interface IExchangeEmailSender
    {
        void Send(ExchangeService service, EmailMessage message);
    }
}
