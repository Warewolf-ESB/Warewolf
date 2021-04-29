/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


#pragma warning disable
using System;
using System.Collections.Generic;
using Dev2.Interfaces;
using System.Activities.Statements;
using Dev2.Common.State;
using Warewolf;

namespace Dev2
{
    public interface IDev2Activity
    {
        /// <summary>
        /// UniqueID is the InstanceID and MUST be a guid.
        /// </summary>
        string UniqueID { get; set; }

        enFindMissingType GetFindMissingType();
        IDev2Activity Execute(IDSFDataObject data, int update);
        IEnumerable<IDev2Activity> NextNodes { get; set; }
        Guid ActivityId { get; set; }

        IEnumerable<string> GetOutputs();
        IEnumerable<IDev2Activity> GetChildrenNodes();
        FlowNode GetFlowNode();
        string GetDisplayName();
        IEnumerable<StateVariable> GetState();
        IEnumerable<IDev2Activity> GetNextNodes();
        List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)> ArmConnectors();
        T As<T>() where T : class, IDev2Activity;
    }
}