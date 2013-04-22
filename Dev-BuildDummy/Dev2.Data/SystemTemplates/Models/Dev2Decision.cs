using Dev2.Data.Decisions.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Data.SystemTemplates.Models
{
    public class Dev2Decision : IDev2DataModel
    {
        private const int TotalCols = 3;

        #region Properties

        public string Col1 { get; set; }

        public string Col2 { get; set; }

        public string Col3 { get; set; }

        public int PopulatedColumnCount { 
            get { 
                    int cnt = 0;

                    if (!string.IsNullOrEmpty(Col1))
                    {
                        cnt++;
                    }

                    if (!string.IsNullOrEmpty(Col2))
                    {
                        cnt++;
                    }

                    if (!string.IsNullOrEmpty(Col3))
                    {
                        cnt++;
                    }   

                    return cnt;
            } 
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enDecisionType EvaluationFn { get; set; }

        [JsonIgnore]
        public string Version {  get { return "1.0.0"; } }

        [JsonIgnore]
        public Dev2ModelType ModelName { get { return Dev2ModelType.Dev2Decision; } }

        #endregion

        public string ToWebModel()
        {
            string result = JsonConvert.SerializeObject(this);

            return result;
        }

        public string GenerateUserFriendlyModel()
        {

            string fn = DecisionDisplayHelper.GetDisplayValue(EvaluationFn);



            if(PopulatedColumnCount == 0)
            {
                return "If " + fn + " ";
            }

            if (PopulatedColumnCount == 1)
            {
                return "If " + Col1 + " " + fn + " ";
            }

            if(PopulatedColumnCount == 2)
            {
                return "If " + Col1 + " " + fn + " "+ Col2 + " ";
            }
             
            if(PopulatedColumnCount == 3)
            {
                return "If " + Col1 + " " + fn + " " + Col2 + " and " + Col3;
            }

            return "<< Internal Error Generationg Decision Model >>";
        }

        /// <summary>
        /// Fetches the cols as array.
        /// </summary>
        /// <returns></returns>
        public string[] FetchColsAsArray()
        {
            string[] result = new string[TotalCols];

            if (Col1 == null)
            {
                Col1 = string.Empty;
            }

            if (Col2 == null)
            {
                Col2 = string.Empty;
            }

            if (Col3 == null)
            {
                Col3 = string.Empty;
            }


            result[0] = Col1;
            result[1] = Col2;
            result[2] = Col3;


            return result;

        }

    }
}
