using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warewolf.ResourceManagement
{
    public class ResourceActivityParseWithCacheParameters
    {
        public ResourceActivityParseWithCacheParameters(DynamicActivity activity, Guid workspaceID, Guid resourceID, bool faileOnError)
        {
            Activity = activity;
            WorkspaceID = workspaceID;
            ResourceIdGuid = resourceID;
            FailOnError = faileOnError;
        }

        public DynamicActivity Activity { get; set; }
        public Guid WorkspaceID { get; set; }
        public Guid ResourceIdGuid { get; set; }
        public bool FailOnError { get; set; }

        
    }
}
