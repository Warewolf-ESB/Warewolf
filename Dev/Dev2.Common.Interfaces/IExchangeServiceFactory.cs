using Microsoft.Exchange.WebServices.Data;

namespace Dev2.Common.Interfaces
{
    public interface IExchangeServiceFactory
    {
        ExchangeService Create();
    }
}
