
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <New>
    /// Activity for finding records accoring to a search criteria that the user specifies
    /// </New>
    public class DsfFindRecordsActivity : DsfActivityAbstract<string>, IRecsetSearch
    {
        #region Fields

        private string _searchType;

        #endregion


        #region Properties

        /// <summary>
        /// Property for holding a string the user enters into the "In Fields" box
        /// </summary>
        [Inputs("FieldsToSearch")]
        [FindMissing]
        public string FieldsToSearch { get; set; }

        /// <summary>
        /// Property for holding a string the user selects in the "Where" drop down box
        /// </summary>
        [Inputs("SearchType")]
        public string SearchType
        {
            get
            {
                return _searchType;
            }
            set
            {
                _searchType = FindRecordsDisplayUtil.ConvertForDisplay(value);
            }
        }

        public string From { get; set; }
        public string To { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Match" box
        /// </summary>
        [Inputs("SearchCriteria")]
        [FindMissing]
        public string SearchCriteria { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Start Index" box
        /// </summary>
        [Inputs("StartIndex")]
        [FindMissing]
        public string StartIndex { get; set; }

        /// <summary>
        /// Property for holding a bool the user chooses with the "MatchCase" Checkbox
        /// </summary>
        [Inputs("MatchCase")]
        public bool MatchCase { get; set; }

        public bool RequireAllFieldsToMatch { get; set; }

        #endregion Properties

        #region Ctor

        public DsfFindRecordsActivity()
            : base("Find Record Index")
        {
            // Initialise all the properties here
            FieldsToSearch = string.Empty;
            SearchType = "<";
            SearchCriteria = string.Empty;
            Result = string.Empty;
            StartIndex = string.Empty;
            RequireAllFieldsToMatch = false;
        }

        #endregion Ctor

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

        /// <summary>
        /// Executes the logic of the activity and calls the backend code to do the work
        /// Also responsible for adding the results to the data list
        /// </summary>
        /// <param name="context"></param>
        protected override void OnExecute(NativeActivityContext context)
        {            
        }
        #region Private Methods

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {

            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
        {

            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        ///  <summary>
        /// Not covered as this Activity has been deprecated and replaced with the <see cref="DsfFindRecordsMultipleCriteriaActivity"/>
        /// It is here purely for backward compatibility
        ///  </summary>
        ///  <param name="updates"></param>
        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates.Count == 1)
            {
                FieldsToSearch = updates[0].Item2;
            }
        }

        ///  <summary>
        /// Not covered as this Activity has been deprecated and replaced with the <see cref="DsfFindRecordsMultipleCriteriaActivity"/>
        /// It is here purely for backward compatibility
        ///  </summary>
        ///  <param name="updates"></param>
        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }


        #region GetForEachInputs/Outputs
        /// <summary>
        ///Not covered as this Activity has been deprecated and replaced with the <see cref="DsfFindRecordsMultipleCriteriaActivity"/>
        ///It is here purely for backward compatibility
        /// </summary>
        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(FieldsToSearch);
        }
        /// <summary>
        ///Not covered as this Activity has been deprecated and replaced with the <see cref="DsfFindRecordsMultipleCriteriaActivity"/>
        ///It is here purely for backward compatibility
        /// </summary>
        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {
        }

        #endregion
    }
}
