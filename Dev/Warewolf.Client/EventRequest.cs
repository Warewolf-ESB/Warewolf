/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Communication;
using Warewolf.Esb;

namespace Warewolf.Client
{
    public class EventRequest<T> : ICatalogSubscribeRequest
    {
        private readonly Guid _workspaceId;

        public EventRequest(Guid workspaceId)
        {
            _workspaceId = workspaceId;
        }

        public IEsbRequest Build()
        {
            return new EsbExecuteRequest();
        }
    }
}
