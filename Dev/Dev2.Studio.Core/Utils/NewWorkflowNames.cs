using System.Collections.Generic;

namespace Dev2.Studio.Core.Utils
{
    /// <summary>
    /// Used for storing all the used workflow names and getting the next available name
    /// </summary>
    public class NewWorkflowNames
    {
        #region Fields

        private static NewWorkflowNames _instance;
        private HashSet<string> _workflowNamesHashSet = new HashSet<string>();

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
            bool result = false;
            if (Contains(nameToRemove))
            {
                _workflowNamesHashSet.Remove(nameToRemove);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Gets the next available name to use for createing new workflow
        /// </summary>
        /// <returns>The next available workflow name</returns>
        public string GetNext()
        {
            string newWorkflowBaseName = StringResources.NewWorkflowBaseName;

            int counter = 1;
            string fullName = StringResources.NewWorkflowBaseName + " " + counter;
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
        public bool Contains(string nameToCheck)
        {
            return _workflowNamesHashSet.Contains(nameToCheck);
        }

        /// <summary>
        /// Add a used name to the WorkflowNamesHashSet
        /// </summary>
        /// <param name="newWorkflowName"></param>
        public bool Add(string newWorkflowName)
        {
            bool result = false;
            if (!Contains(newWorkflowName))
            {
                result = true;
                _workflowNamesHashSet.Add(newWorkflowName);
            }
            return result;
        }

        #endregion

        #region Properties

        public static NewWorkflowNames Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NewWorkflowNames();
                }
                return _instance;
            }
        }

        #endregion
    }
}
