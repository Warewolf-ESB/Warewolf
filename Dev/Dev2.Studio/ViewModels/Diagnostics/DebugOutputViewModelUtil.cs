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
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Studio.Diagnostics;


namespace Dev2.Studio.ViewModels.Diagnostics
{

    class DebugOutputViewModelUtil : IDebugOutputViewModelUtil
    {
        readonly Guid _sessionId;

        public DebugOutputViewModelUtil(Guid sessionId)
        {
            _sessionId = sessionId;
        }

        public bool QueuePending(IDebugState item, List<IDebugState> pendingItems, bool isProcessing)
        {
            if (item.StateType == StateType.Message && isProcessing)
            {
                pendingItems.Add(item);
                return true;
            }
            return false;
        }

        public bool ContenIsNotValid(IDebugState content)
        {
            if (content == null || content.SessionID != _sessionId)
            {
                return true;
            }
            if (content.Name == "EsbServiceInvoker" && content.ExecutionOrigin == ExecutionOrigin.Unknown)
            {
                return true;
            }

            return false;
        }

        public bool IsValidLineItem(IDebugLineItem item)
        {
            if (item != null)
            {
                return false;
            }

            Dev2Logger.Debug("Debug line item is null, did not proceed", "Warewolf Debug");
            return true;
        }
        public bool IsItemMoreLinkValid(IDebugLineItem item) => !string.IsNullOrEmpty(item.MoreLink);
    }
}
