using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime
{
    public interface IDbSources
    {
        DatabaseValidationResult DoDatabaseValidation(DbSource dbSourceDetails);
    }
}
