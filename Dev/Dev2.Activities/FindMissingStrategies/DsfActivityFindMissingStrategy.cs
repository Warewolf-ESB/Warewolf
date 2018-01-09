/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.FindMissingStrategies
{
    /// <summary>
    /// Responsible for the find missing logic that applys to the DsfActivity
    /// </summary>
 //This is loaded based on SpookyAction implementing IFindMissingStrategy
    class DsfActivityFindMissingStrategy : IFindMissingStrategy
    {
        #region Implementation of ISpookyLoadable<Enum>

        public Enum HandlesType()
        {
            return enFindMissingType.DsfActivity;
        }

        /// <summary>
        /// Gets all the fields for a specific activity
        /// </summary>
        /// <param name="activity">The activity that the fields will be retrieved from</param>
        /// <returns>Returns all the fields in a list of strings</returns>

        public List<string> GetActivityFields(object activity)
        {
            var results = new List<string>();

            if (activity is DsfActivity act)
            {
                if (!string.IsNullOrEmpty(act.ServiceName))
                {
                    results.Add(act.ServiceName);
                }

                if (!string.IsNullOrEmpty(act.InputMapping))
                {
                    var inputMappingElement = XElement.Parse(act.InputMapping);
                    const string InputElement = "Input";
                    var inputs = inputMappingElement.DescendantsAndSelf().Where(c => c.Name.ToString().Equals(InputElement, StringComparison.InvariantCultureIgnoreCase));

                    results.AddRange(inputs.Select(element => element.Attribute("Source").Value).Where(val => !string.IsNullOrEmpty(val)));
                }

                if (!string.IsNullOrEmpty(act.OutputMapping))
                {
                    var outputMappingElement = XElement.Parse(act.OutputMapping);
                    const string OutputElement = "Output";
                    var inputs = outputMappingElement.DescendantsAndSelf().Where(c => c.Name.ToString().Equals(OutputElement, StringComparison.InvariantCultureIgnoreCase));

                    results.AddRange(inputs.Select(element => element.Attribute("Value").Value).Where(val => !string.IsNullOrEmpty(val)));
                }

                if (!string.IsNullOrEmpty(act.OnErrorVariable))
                {
                    results.Add(act.OnErrorVariable);
                }

                if (!string.IsNullOrEmpty(act.OnErrorWorkflow))
                {
                    results.Add(act.OnErrorWorkflow);
                }

            }
            return results;
        }

        #endregion
    }
}
