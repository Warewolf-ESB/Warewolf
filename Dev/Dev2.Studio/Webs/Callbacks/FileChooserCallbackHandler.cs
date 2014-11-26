
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Common.Utils;
using Dev2.Communication;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Webs.Callbacks
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

        public override void Save(string value, bool closeBrowserWindow = true)
        {
            if(string.IsNullOrEmpty(value))
            {
                _fileChooserMessage.SelectedFiles = null;
            }
            else
            {
                var scrubbedValue = JSONUtils.ScrubJSON(value).Replace(@"\\", @"\");

                var result = new Dev2JsonSerializer().Deserialize<FileChooserResult>(scrubbedValue);
                _fileChooserMessage.SelectedFiles = result.FilePaths;
            }
            if(closeBrowserWindow)
            {
                Close();
            }
        }

        public override void Save(string value, IEnvironmentModel environmentModel, bool closeBrowserWindow = true)
        {
            throw new NotImplementedException();
        }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            throw new NotImplementedException();
        }

        public class FileChooserResult
        {
            public IEnumerable<string> FilePaths { get; set; }
        }
    }

}
