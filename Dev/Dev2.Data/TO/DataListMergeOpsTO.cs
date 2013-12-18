using System;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;

namespace Dev2.TO {
    public class DataListMergeOpsTO 
    {
        public DataListMergeOpsTO()
        {

        }

        public DataListMergeOpsTO(Guid datalistMergeID)
        {
            DatalistOutMergeID = datalistMergeID;
            DatalistInMergeID = datalistMergeID;

            DataListOutMergeDepth = enTranslationDepth.Data_With_Blank_OverWrite;
            DatalistOutMergeFrequency = DataListMergeFrequency.OnCompletion;
            DatalistOutMergeType = enDataListMergeTypes.Union;

            DataListInMergeDepth = enTranslationDepth.Data_With_Blank_OverWrite;
            DatalistInMergeType = enDataListMergeTypes.Union;
        }

        /// <summary>
        /// Gets or sets the datalist merge ID.
        /// </summary>
        public Guid DatalistOutMergeID { get; set; }

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
        public Guid DatalistInMergeID { get; set; }

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
