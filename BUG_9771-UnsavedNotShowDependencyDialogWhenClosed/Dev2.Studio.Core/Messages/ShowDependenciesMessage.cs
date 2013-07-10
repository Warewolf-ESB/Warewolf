using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class ShowDependenciesMessage : AbstractResourceMessage
    {
        /// <summary>
        /// Gets or sets a value indicating whether to show workflows dependent on me
        /// </summary>
        /// <value>
        /// <c>true</c> if the dependenies on me should be shown; otherwise, <c>false</c>, will show what I depend on.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/06/26</date>
        public bool ShowDependentOnMe { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowDependenciesMessage"/> class.
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        /// <param name="showDependentOnMe">if set to <c>true</c> [show dependent on me].</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/06/26</date>
        public ShowDependenciesMessage(IResourceModel resourceModel, bool showDependentOnMe = false) 
            : base(resourceModel)
        {
            ShowDependentOnMe = showDependentOnMe;
        }
    }
}
