using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class DbSourcesMock : DbSources
    {
        public int DatabaseValidationHitCount { get; set; }

        protected override DatabaseValidationResult DoDatabaseValidation(DbSource dbSourceDetails)
        {
            //PBI 8720
            DatabaseValidationHitCount++;
            return new DatabaseValidationResult { IsValid = true };
        }
    }
}
