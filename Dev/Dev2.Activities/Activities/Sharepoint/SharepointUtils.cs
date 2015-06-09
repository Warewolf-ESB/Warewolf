using System.Collections.Generic;
using System.Linq;
using Dev2.TO;

namespace Dev2.Activities.Sharepoint
{
    public class SharepointUtils
    {
        
        public IEnumerable<SharepointReadListTo> GetValidReadListItems(IList<SharepointReadListTo> sharepointReadListTos)
        {
            if(sharepointReadListTos == null)
            {
                return new List<SharepointReadListTo>();
            }
            return sharepointReadListTos.Where(to => !string.IsNullOrEmpty(to.VariableName));
        }
    }
}