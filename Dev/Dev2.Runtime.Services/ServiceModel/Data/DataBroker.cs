using System;
using System.Collections.Generic;
using System.Data;

namespace Dev2.Runtime.ServiceModel.Data
{
    public abstract class DataBroker
    {
        #region Abstract Methods

        public abstract void GetStoredProcedures(IDbConnection connection, Func<IDbCommand, IList<IDataParameter>, string, bool> procedureProcessor, 
            Func<IDbCommand, IList<IDataParameter>, string, bool> functionProcessor, bool continueOnProcessorException = false);

        public abstract DataSet ExecuteSelect(IDbCommand command);

        #endregion

        #region Methods

        public void ExecuteSelect(IDbCommand selectCommand, Func<IDataReader, bool> rowProcessor, bool continueOnProcessorException = false, CommandBehavior commandBehavior = CommandBehavior.CloseConnection)
        {
            using (var reader = selectCommand.ExecuteReader(commandBehavior))
            {
                if (reader != null)
                {
                    bool read = true;
                    while (read && reader.Read())
                    {
                        try
                        {
                            read = rowProcessor(reader);
                        }
                        catch(Exception)
                        {
                            if (!continueOnProcessorException)
                            {
                                throw;
                            }
                        }
                        
                    }
                }
            }
        }

        #endregion

    }
}
