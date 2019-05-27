#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Messages;
using Dev2.Studio.Interfaces;
using Dev2.Utils;

namespace Dev2.Webs.Callbacks
{
    public class SaveNewWorkflowCallbackHandler
       : WebsiteCallbackHandler
    {
        #region Fields

        readonly IContextualResourceModel _resourceModel;
        public bool AddToTabManager { private set; get; }

        #endregion
        public SaveNewWorkflowCallbackHandler(IEventAggregator eventPublisher, IServerRepository currentServerRepository, IContextualResourceModel resourceModel, bool addToTabManager)
            : base(eventPublisher, currentServerRepository)
        {
            AddToTabManager = addToTabManager;
            _resourceModel = resourceModel;
        }


        #region Overrides of WebsiteCallbackHandler

        protected override void Save(IServer server, dynamic jsonArgs)
        {
            try
            {
                string resName = jsonArgs.resourceName;
                bool loadingFromServer = jsonArgs.resourceLoadingFromServer;
                string originalPath = jsonArgs.OriginalPath;
                var resCat = HelperUtils.SanitizePath((string)jsonArgs.resourcePath, resName);
                if (_resourceModel != null)
                {
                    EventPublisher.Publish(new SaveUnsavedWorkflowMessage(_resourceModel, resName, resCat, AddToTabManager, loadingFromServer, originalPath));
                }

                Close();
            }
            catch (Exception e)
            {
                var e1 = new Exception("There was a problem saving. Please try again.", e);

                Dev2Logger.Info(e.Message + Environment.NewLine + " Stacktrace : " + e.StackTrace + Environment.NewLine + " jsonObj: " + jsonArgs.ToString(), "Warewolf Info");

                throw e1;
            }
        }
        #endregion
    }
}
