/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Triggers;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Triggers
{
    public delegate void TriggerChangeEvent(string guid);
    public interface ITriggersCatalog
    {
        IList<ITriggerQueue> Queues { get; set; }
        void SaveTriggerQueue(ITriggerQueue triggerQueue);
        void Load();
        void DeleteTriggerQueue(ITriggerQueue triggerQueue);
        ITriggerQueue LoadQueueTriggerFromFile(string filename);
        event TriggerChangeEvent OnChanged;
        event TriggerChangeEvent OnDeleted;
        event TriggerChangeEvent OnCreated;
    }
}
