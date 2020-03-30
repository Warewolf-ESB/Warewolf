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

namespace Warewolf.Esb
{
    public interface IClusterDispatcher
    {
        int Count { get; }
        void Remove(Guid workspaceId);
        void Add(Guid workspaceId, IClusterNotificationWriter writer);
        void Shutdown();
        void Write<T>(T info) where T : class;
    }
}
