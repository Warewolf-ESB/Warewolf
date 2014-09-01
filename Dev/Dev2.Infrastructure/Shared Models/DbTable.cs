using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbTable
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string FullName
        {
            get
            {
                if(string.IsNullOrEmpty(Schema))
                {
                    return TableName;
                }
                if(string.IsNullOrEmpty(TableName))
                {
                    return "";
                }
                return string.Format("{0}.{1}", Schema, TableName);
            }
        }
        public List<IDbColumn> Columns { get; set; }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return FullName;
        }

        #endregion
    }
}