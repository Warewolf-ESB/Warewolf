
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
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
        public ShowDependenciesMessage(IContextualResourceModel resourceModel, bool showDependentOnMe = false) 
            : base(resourceModel)
        {
            ShowDependentOnMe = showDependentOnMe;
        }
    }
}
