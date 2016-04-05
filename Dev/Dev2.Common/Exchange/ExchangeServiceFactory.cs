using Dev2.Common.Interfaces;
using Microsoft.Exchange.WebServices.Data;

namespace Dev2.Common.Exchange
{
    public class ExchangeServiceFactory : IExchangeServiceFactory
    {
        public ExchangeService Create()
        {
            return new ExchangeService(ExchangeVersion.Exchange2007_SP1);
        }
    }
}
