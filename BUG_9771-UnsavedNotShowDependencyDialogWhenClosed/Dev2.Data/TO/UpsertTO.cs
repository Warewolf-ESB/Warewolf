
namespace Dev2.Data.TO
{
    public class UpsertTO
    {
        #region Properties

        public string Expression { get; set; }
        public string Payload { get; set; }

        #endregion

        #region Ctor

        public UpsertTO(string expression, string payload)
        {
            Expression = expression;
            Payload = payload;
        }

        #endregion
    }
}
