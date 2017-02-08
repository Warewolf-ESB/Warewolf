/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using System;

namespace Dev2.Common.Exchange
{
    public class ExchangeEmailSender : IExchangeEmailSender
    {
        private readonly IExchange _source;

        public ExchangeEmailSender()
        {
        }

        public ExchangeEmailSender(IExchange source)
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