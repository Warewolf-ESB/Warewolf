using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;

namespace Dev2.Activities.Exchange
{
    public class ExchangeServiceFactory : IExchangeServiceFactory
    {
        public ExchangeService Create()
        {
            return new ExchangeService(ExchangeVersion.Exchange2007_SP1);
        }
    }
}
