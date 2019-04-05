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

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    public struct WriteArgs
    {
        public IDebugState debugState;
        public bool isTestExecution;
        public bool isDebugFromWeb;
        public string testName;
        public bool isRemoteInvoke;
        public string remoteInvokerId;
        public string parentInstanceId;
        public IList<IDebugState> remoteDebugItems;
    }

    public interface IDebugDispatcher
    {
        int Count { get; }
        void Remove(Guid workspaceId);
        IDebugWriter Get(Guid workspaceId);
        void Add(Guid workspaceId, IDebugWriter writer);
        void Shutdown();
        void Write(WriteArgs writeArgs);
    }
}