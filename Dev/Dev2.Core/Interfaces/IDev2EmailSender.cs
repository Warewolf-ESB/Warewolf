using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Microsoft.Exchange.WebServices.Data;

namespace Dev2.Interfaces
{
    public interface IDev2EmailSender
    {
        IExchangeEmailSender EmailSender { get; set; }
        ExchangeService ExchangeService { get; set; }
        string SendEmail(IExchange runtimeSource, IWarewolfListIterator colItr, IWarewolfIterator toItr, IWarewolfIterator ccItr, IWarewolfIterator bccItr, IWarewolfIterator subjectItr, IWarewolfIterator bodyItr, IWarewolfIterator attachmentsItr, out ErrorResultTO errors);
    }
}
