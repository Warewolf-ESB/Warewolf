﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Exchange;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using Warewolf.Resource.Errors;

namespace Dev2.Common
{
    public class Dev2EmailSender:IDev2EmailSender
    {
       

        #region Implementation of IDev2EmailSender

        public Dev2EmailSender()
        {
        }
        public Dev2EmailSender(ExchangeService exchangeService, IExchangeEmailSender emailSender)
        {
            ExchangeService = exchangeService;
            EmailSender = emailSender;
           
        }
        private IExchangeServiceFactory _exchangeServiceFactory;
        private void InitializeService()
        {
            _exchangeServiceFactory = new ExchangeServiceFactory();
            ExchangeService = _exchangeServiceFactory.Create();
        }

        public IExchangeEmailSender EmailSender { get; set; }
        public ExchangeService ExchangeService { get; set; }

    
        public string SendEmail(IExchangeSource runtimeSource, IWarewolfListIterator colItr, IWarewolfIterator toItr, IWarewolfIterator ccItr, IWarewolfIterator bccItr, IWarewolfIterator subjectItr, IWarewolfIterator bodyItr, IWarewolfIterator attachmentsItr, out ErrorResultTO errors)
        // ReSharper restore TooManyArguments
        {
            InitializeService();
             errors = new ErrorResultTO();
            var toValue = colItr.FetchNextValue(toItr);
            var ccValue = colItr.FetchNextValue(ccItr);
            var bccValue = colItr.FetchNextValue(bccItr);
            var subjectValue = colItr.FetchNextValue(subjectItr);
            var bodyValue = colItr.FetchNextValue(bodyItr);
            var attachmentsValue = colItr.FetchNextValue(attachmentsItr);
            var mailMessage = new EmailMessage(ExchangeService) { Subject = subjectValue };

            AddToAddresses(toValue, mailMessage);

            mailMessage.Body = bodyValue;

            if (!string.IsNullOrEmpty(ccValue))
            {
                AddCcAddresses(ccValue, mailMessage);
            }
            if (!string.IsNullOrEmpty(bccValue))
            {
                AddBccAddresses(bccValue, mailMessage);
            }
            if (!string.IsNullOrEmpty(attachmentsValue))
            {
                AddAttachmentsValue(attachmentsValue, mailMessage);
            }
            string result;
            try
            {
                EmailSender = new ExchangeEmailSender(runtimeSource);

                EmailSender.Send(ExchangeService, mailMessage);

                result = "Success";
            }
            catch (Exception e)
            {
                result = "Failure";
                errors.AddError(e.Message);
            }

            return result;
        }

        void AddToAddresses(string toValue, EmailMessage mailMessage)
        {
            try
            {
                var toAddresses = GetSplitValues(toValue, new[] { ',', ';' });
                toAddresses.ForEach(s => mailMessage.ToRecipients.Add(s));
            }
            catch (FormatException exception)
            {
                throw new Exception(string.Format(ErrorResource.ToAddressInvalidFormat, toValue), exception);
            }
        }

        void AddCcAddresses(string toValue, EmailMessage mailMessage)
        {
            try
            {
                var ccAddresses = GetSplitValues(toValue, new[] { ',', ';' });
                ccAddresses.ForEach(s => mailMessage.CcRecipients.Add(s));
            }
            catch (FormatException exception)
            {
                throw new Exception(string.Format(ErrorResource.CCAddressInvalidFormat, toValue), exception);
            }
        }

        void AddBccAddresses(string toValue, EmailMessage mailMessage)
        {
            try
            {
                var bccAddresses = GetSplitValues(toValue, new[] { ',', ';' });
                bccAddresses.ForEach(s => mailMessage.BccRecipients.Add(s));
            }
            catch (FormatException exception)
            {
                throw new Exception(string.Format(ErrorResource.BCCAddressInvalidFormat, toValue), exception);
            }
        }

        void AddAttachmentsValue(string attachmentsValue, EmailMessage mailMessage)
        {
            try
            {
                var attachements = GetSplitValues(attachmentsValue, new[] { ',', ';' });
                attachements.ForEach(s => mailMessage.Attachments.AddFileAttachment(s));
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format(ErrorResource.AttachmentInvalidFormat, attachmentsValue), exception);
            }
        }

        private List<string> GetSplitValues(string stringToSplit, char[] splitOn)
        {
            return stringToSplit.Split(splitOn, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        #endregion
    }


}
