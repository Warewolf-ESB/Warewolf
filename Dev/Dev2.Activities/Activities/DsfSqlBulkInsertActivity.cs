using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.TO;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Activities
{
    public class DsfSqlBulkInsertActivity : DsfActivityAbstract<string>
    {
        public DsfSqlBulkInsertActivity()
            : base("SQL Bulk Insert")
        {
            InputMappings = new List<SqlBulkInsertTO>();
        }

        public IList<SqlBulkInsertTO> InputMappings
        {
            get;
            set;
        }

        [Inputs("Database")]
        public string Database { get; set; }

        [Inputs("TableName")]
        public string TableName { get; set; }

        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }


        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        #endregion
    }
}
