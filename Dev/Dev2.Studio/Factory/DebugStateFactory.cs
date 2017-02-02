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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Factory
{
    public static class DebugStateFactory
    {
        public static IDebugState Create(string message, IContextualResourceModel resourceModel)
        {
            var state = new DebugState
                {
                    Message = message,
                };

            if(resourceModel != null)
            {
                state.ServerID = resourceModel.ServerID;
                state.OriginatingResourceID = resourceModel.ID;
            }
            state.StateType = String.IsNullOrWhiteSpace(message) ? StateType.Clear : StateType.Message;

            return state;
        }
    }
}
