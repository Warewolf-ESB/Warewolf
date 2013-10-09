using System;
using System.Activities;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dev2.TO;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Activities
{
    public class DsfSqlBulkInsertActivity : DsfActivityAbstract<string>
    {
        ISqlBulkInserter _sqlBulkInserter;

        public DsfSqlBulkInsertActivity()
            : base("SQL Bulk Insert")
        {
            InputMappings = new List<DataColumnMapping>();
        }

        public IList<DataColumnMapping> InputMappings
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

        public bool CheckConstraints { get; set; }

        public bool FireTriggers { get; set; }

        public bool UserInternalTransaction { get; set; }

        public bool UseDefaultValues { get; set; }

        public bool KeepIdentity { get; set; }

        public bool KeepLock { get; set; }
        
        public ISqlBulkInserter SqlBulkInserter
        {
            get
            {
                return _sqlBulkInserter ?? (_sqlBulkInserter = new SqlBulkInserter());
            }
            set
            {
                _sqlBulkInserter = value;
            }
        }
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

    public interface ISqlBulkInserter
    {
        void Insert(SqlBulkCopy sqlBulkCopy);
    }

    public class SqlBulkInserter : ISqlBulkInserter
    {
        #region Implementation of ISqlBulkInserter

        public void Insert(SqlBulkCopy sqlBulkCopy)
        {
        }

        #endregion
    }
}
