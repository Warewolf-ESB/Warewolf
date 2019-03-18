#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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


namespace Dev2.Studio.Core.Utils
{
    /// <summary>
    /// Used for storing all the used workflow names and getting the next available name
    /// </summary>
    public class NewWorkflowNames
    {
        #region Fields

        static NewWorkflowNames _instance;
        readonly HashSet<string> _workflowNamesHashSet = new HashSet<string>();

        #endregion

        #region Ctor

        public NewWorkflowNames()
        {
            _workflowNamesHashSet = new HashSet<string>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Remove a name from WorkflowNamesHashSet so i can be reused
        /// </summary>
        /// <param name="nameToRemove"></param>
        public bool Remove(string nameToRemove)
        {
            var result = false;
            if (Contains(nameToRemove))
            {
                _workflowNamesHashSet.Remove(nameToRemove);
                result = true;
            }
            return result;
        }

        public bool RemoveAll(string nameToRemove)
        {
            _workflowNamesHashSet.Remove(nameToRemove);
            return true;
        }

        /// <summary>
        /// Gets the next available name to use for createing new workflow
        /// </summary>
        /// <returns>The next available workflow name</returns>
        public string GetNext()
        {
            var newWorkflowBaseName = StringResources.NewWorkflowBaseName;

            var counter = 1;
            var fullName = StringResources.NewWorkflowBaseName + " " + counter;

            while (Contains(fullName))
            {
                counter++;
                fullName = newWorkflowBaseName + " " + counter;
            }

            Add(fullName);

            return fullName;
        }

        /// <summary>
        /// Check if the HashSet contains the specified name
        /// </summary>
        /// <param name="nameToCheck"></param>
        /// <returns>Does the HashSet contain the string</returns>
        public bool Contains(string nameToCheck) => _workflowNamesHashSet.Contains(nameToCheck);

        /// <summary>
        /// Add a used name to the WorkflowNamesHashSet
        /// </summary>
        /// <param name="newWorkflowName"></param>
        public bool Add(string newWorkflowName)
        {
            // only add the one's that matter ;)
            if(newWorkflowName.IndexOf(StringResources.NewWorkflowBaseName, StringComparison.Ordinal) == 0)
            {
                _workflowNamesHashSet.Add(newWorkflowName);

                return true;
            }

            return false;
        }

        #endregion

        #region Properties

        public static NewWorkflowNames Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new NewWorkflowNames();
                }
                return _instance;
            }
        }

        #endregion
    }
}
