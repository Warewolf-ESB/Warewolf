
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.Interfaces;
using System;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class AddServerNavigationMessage : IMessage
    {
        public AddServerNavigationMessage(IEnvironmentModel environmentModel, bool forceConnect = false, Action callBackFunction = null)
        {
            EnvironmentModel = environmentModel;
            ForceConnect = forceConnect;
            CallBackFunction = callBackFunction;
        }

        public IEnvironmentModel EnvironmentModel { get; set; }
        public bool ForceConnect { get; set; }
        public Action CallBackFunction { get; set; }
    }
}
