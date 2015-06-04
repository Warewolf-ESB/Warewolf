using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class SharepointListTo
    {
        public string FullName { get; set; }
        public List<ISharepointFieldTo> Fields { get; set; }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return FullName;
        }

        #endregion
    }

    public class SharepointFieldTo : ISharepointFieldTo
    {
        public string Name { get; set; }
        public string InternalName { get; set; }
    }
}