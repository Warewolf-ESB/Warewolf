using System;
using System.Data;

namespace Dev2.Activities
{
	public interface IAdvancedRecordset : IDisposable
	{
		DataSet ExecuteQuery(string sqlQuery);
		
		int ExecuteScalar(string sqlQuery);

		int ExecuteNonQuery(string sqlQuery);
		
	}
}
