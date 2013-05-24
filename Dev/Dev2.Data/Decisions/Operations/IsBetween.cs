using System;
using System.IO;
using Dev2.Common;

namespace Dev2.Data.Decisions.Operations
{
    public class IsBetween : IDecisionOperation
    {

        public Enum HandlesType()
        {
            return enDecisionType.IsBetween;
        }

        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 3 || cols.Length > 3)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            //int left = string.CompareOrdinal(cols[1], cols[0]);
            //int right = string.CompareOrdinal(cols[2], cols[0]);

            
            double[] dVal = new double[3];
            DateTime[] dtVal = new DateTime[3];

            int pos = 0;

            foreach(string c in cols)
            {
                if(!double.TryParse(c, out dVal[pos]))
                {
                    try
                    {
                        DateTime dt = new DateTime();
                        if (DateTime.TryParse(c, out dt))
                        {
                            dtVal[pos] = dt;
                        }
                    }
                    catch(Exception ex)
                    {
                        ServerLogger.LogError(ex);
                        // Best effort ;)
                    }

                    //throw new InvalidDataException("Data is not numeric");
                }

                pos++;
            }


            double left = 0.0;
            double right = 0.0;

            if(dVal.Length == 3)
            {

                left = dVal[0] - dVal[1];
                right = dVal[0] - dVal[2];

                
            }
            else if(dtVal.Length == 3)
            {
                left = dtVal[0].Ticks - dtVal[1].Ticks;
                right = dtVal[0].Ticks - dtVal[2].Ticks;

            }
            else
            {
                throw new InvalidDataException("IsBetween Numeric and DateTime mis-match");
            }

            return (left > 0 && right < 0);
        }
    }
}
