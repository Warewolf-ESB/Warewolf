using Microsoft.Exchange.WebServices.Data;

namespace Dev2.Activities.Exchange
{
    public interface IExchangeServiceFactory
    {
        ExchangeService Create();
    }
}
