using System;
using System.Collections.Generic;
using System.Data;

namespace Dev2.Runtime.ServiceModel.Data
{
    public abstract class DataBroker
    {
        #region Abstract Methods

        public abstract void GetStoredProcedures(Func<IDbCommand, IList<IDataParameter>, bool> resultProcessor, bool continueOnProcessorException = false);

        #endregion

        #region Methods

        public void Execute(IDbCommand command, Func<IDataReader, bool> rowProcessor, bool continueOnProcessorException = false)
        {
            using(var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
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
