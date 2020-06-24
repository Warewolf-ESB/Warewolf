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

namespace Warewolf.Triggers
{
    public delegate void TriggerChangeEvent(Guid guid);
    public interface ITriggersCatalog
    {
        string PathFromResourceId(string triggerId);
        IList<ITriggerQueue> Queues { get; }
        void SaveTriggerQueue(ITriggerQueue triggerQueue);
        void DeleteTriggerQueue(ITriggerQueue triggerQueue);
        ITriggerQueue LoadQueueTriggerFromFile(string filename);
        event TriggerChangeEvent OnChanged;
        event TriggerChangeEvent OnDeleted;
        event TriggerChangeEvent OnCreated;
    }
}
