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

        private readonly IContextualResourceModel _resourceModel;
        public bool AddToTabManager { private set; get; }

        #endregion
        public SaveNewWorkflowCallbackHandler(IEventAggregator eventPublisher, IServerRepository currentServerRepository, IContextualResourceModel resourceModel, bool addToTabManager)
            : base(eventPublisher, currentServerRepository)
        {
            AddToTabManager = addToTabManager;
            _resourceModel = resourceModel;
        }


        #region Overrides of WebsiteCallbackHandler

        protected override void Save(IServer server, dynamic jsonObj)
        {
            try
            {
                string resName = jsonObj.resourceName;
                bool loadingFromServer = jsonObj.resourceLoadingFromServer;
                string originalPath = jsonObj.OriginalPath;
                string resCat = HelperUtils.SanitizePath((string)jsonObj.resourcePath, resName);
                if (_resourceModel != null)
                {
                    EventPublisher.Publish(new SaveUnsavedWorkflowMessage(_resourceModel, resName, resCat, AddToTabManager, loadingFromServer, originalPath));
                }

                Close();
            }
            catch (Exception e)
            {
                Exception e1 = new Exception("There was a problem saving. Please try again.", e);

                Dev2Logger.Info(e.Message + Environment.NewLine + " Stacktrace : " + e.StackTrace + Environment.NewLine + " jsonObj: " + jsonObj.ToString(), "Warewolf Info");

                throw e1;
            }
        }
        #endregion
    }
}
