
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;

// ReSharper disable CheckNamespace
namespace Dev2.TO {
// ReSharper restore CheckNamespace
// ReSharper disable InconsistentNaming
    public class DataListMergeOpsTO 
// ReSharper restore InconsistentNaming
    {
        public DataListMergeOpsTO()
        {

        }

        public DataListMergeOpsTO(Guid datalistMergeId)
        {
            DatalistOutMergeID = datalistMergeId;
            DatalistInMergeID = datalistMergeId;

            DataListOutMergeDepth = enTranslationDepth.Data_With_Blank_OverWrite;
            DatalistOutMergeFrequency = DataListMergeFrequency.OnCompletion;
            DatalistOutMergeType = enDataListMergeTypes.Union;

            DataListInMergeDepth = enTranslationDepth.Data_With_Blank_OverWrite;
            DatalistInMergeType = enDataListMergeTypes.Union;
        }

        /// <summary>
        /// Gets or sets the datalist merge ID.
        /// </summary>
// ReSharper disable InconsistentNaming
        public Guid DatalistOutMergeID { get; set; }
// ReSharper restore InconsistentNaming

        /// <summary>
        /// Gets or sets the type of the datalist merge.
        /// </summary>
        public enDataListMergeTypes DatalistOutMergeType { get; set; }

        /// <summary>
        /// Gets or sets the data list merge depth.
        /// </summary>        
        public enTranslationDepth DataListOutMergeDepth { get; set; }

        /// <summary>
        /// Gets or sets the datalist merge frequency.
        /// </summary>
        public DataListMergeFrequency DatalistOutMergeFrequency { get; set; }

        /// <summary>
        /// Gets or sets the datalist merge ID.
        /// </summary>
// ReSharper disable InconsistentNaming
        public Guid DatalistInMergeID { get; set; }
// ReSharper restore InconsistentNaming

        /// <summary>
        /// Gets or sets the type of the datalist merge.
        /// </summary>
        public enDataListMergeTypes DatalistInMergeType { get; set; }

        /// <summary>
        /// Gets or sets the data list merge depth.
        /// </summary>        
        public enTranslationDepth DataListInMergeDepth { get; set; }
    }
}
