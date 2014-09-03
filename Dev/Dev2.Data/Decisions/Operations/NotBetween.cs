using System;
using System.IO;
using Dev2.Common;

namespace Dev2.Data.Decisions.Operations
{
    /// <summary>
    /// Is Between Operator
    /// </summary>
    public class NotBetween : IDecisionOperation
    {

        public Enum HandlesType()
        {
            return enDecisionType.NotBetween;
        }

        public bool Invoke(string[] cols)
        {

            double[] dVal = new double[3];
            DateTime[] dtVal = new DateTime[3];

            int pos = 0;

            foreach(string c in cols)
            {
                if(!double.TryParse(c, out dVal[pos]))
                {
                    try
                    {
                        DateTime dt;
                        if(DateTime.TryParse(c, out dt))
                        {
                            dtVal[pos] = dt;
                        }
                    }
                    catch(Exception ex)
                    {
                        Dev2Logger.Log.Error(ex);
                        // Best effort ;)
                    }
                }

                pos++;
            }


            double left;
            double right;

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

            return (!(left > 0 && right < 0));
        }
    }
}
