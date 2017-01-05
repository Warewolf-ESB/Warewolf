/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Utils;
using Dev2.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace Dev2.Webs.Callbacks
{
    public abstract class WebsiteCallbackHandler : IPropertyEditorWizard
    {
        protected readonly IEventAggregator EventPublisher;

        protected WebsiteCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository, IShowDependencyProvider showDependencyProvider = null)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("currentEnvironmentRepository", currentEnvironmentRepository);
            EventPublisher = eventPublisher;

            CurrentEnvironmentRepository = currentEnvironmentRepository;
        }


        #region Properties

        public Window Owner { get; set; }

        public IEnvironmentRepository CurrentEnvironmentRepository { get; private set; }

        #endregion

        protected abstract void Save(IEnvironmentModel environmentModel, dynamic jsonArgs);

        #region Navigate

        #endregion

        #region ReloadResource

        #endregion

        #region Implementation of IPropertyEditorWizard

        public ILayoutObjectViewModel SelectedLayoutObject => null;

        public virtual void Save(string value, IEnvironmentModel environmentModel, bool closeBrowserWindow = true)
        {
            if(closeBrowserWindow)
            {
                Close();
            }

            if(string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }
            value = JSONUtils.ScrubJSON(value);

            dynamic jsonObj = JObject.Parse(value);
            Save(environmentModel, jsonObj);
        }

        public virtual void Close()
        {
            Owner?.Close();
        }

        // ReSharper disable once EventNeverSubscribedTo.Global
#pragma warning disable 67
        public event NavigateRequestedEventHandler NavigateRequested;
#pragma warning restore 67

        #endregion

    }
}
