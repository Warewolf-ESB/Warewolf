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
using System.Collections.Generic;
// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects
{
    /// <summary>
    /// Used with the ForEach Activity
    /// </summary>
    public class ForEachInnerActivityTO
    {
        public IDev2ActivityIOMapping InnerActivity { get; private set; }
        public string OrigInnerOutputMapping { get; private set; }
        public string OrigInnerInputMapping { get; private set; }

        public IList<Tuple<string, string>> OrigCodedInputs { get; set; }
        public IList<Tuple<string, string>> OrigCodedOutputs { get; set;}

        public IList<Tuple<string, string>> CurCodedInputs { get; set; }
        public IList<Tuple<string, string>> CurCodedOutputs { get; set; }

        public ForEachInnerActivityTO(IDev2ActivityIOMapping act)
        {
            InnerActivity = act;

            if (InnerActivity != null)
            {
                OrigInnerInputMapping = string.IsNullOrEmpty(act.InputMapping) ? null : act.InputMapping;
                OrigInnerOutputMapping = string.IsNullOrEmpty(act.OutputMapping) ? null : act.OutputMapping;
            }
        }

    }
}
