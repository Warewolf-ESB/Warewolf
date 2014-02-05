using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Common.Utils;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Webs.Callbacks;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs.Callbacks
{
    public class FileChooserCallbackHandler : WebsiteCallbackHandler
    {
        readonly FileChooserMessage _fileChooserMessage;

        public FileChooserCallbackHandler(FileChooserMessage fileChooserMessage)
            : this(fileChooserMessage, EventPublishers.Aggregator, EnvironmentRepository.Instance)
        {
        }

        public FileChooserCallbackHandler(FileChooserMessage fileChooserMessage, IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository)
            : base(eventPublisher, currentEnvironmentRepository)
        {
            _fileChooserMessage = fileChooserMessage;
            VerifyArgument.IsNotNull("fileChooserMessage", fileChooserMessage);
        }

        //public override void Save(string value, bool closeBrowserWindow = true)
        //{
        //    Save(value, EnvironmentRepository.Instance.Source, closeBrowserWindow);
        //    if(closeBrowserWindow)
        //    {
        //        Close();
        //    }

        //    if(string.IsNullOrEmpty(value))
        //    {
        //        throw new ArgumentNullException("value");
        //    }
        //    value = JSONUtils.ScrubJSON(value);

        //    dynamic jsonObj = jso
        //}

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            var result = jsonObj as FileChooserResult;
            _fileChooserMessage.SelectedFiles = result.FilePaths;
        }

        public override void Cancel()
        {
            Close();
        }

        public class FileChooserResult
        {
            public IEnumerable<string> FilePaths { get; set; }
        }
    }
}
