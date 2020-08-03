/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Studio.Interfaces;

namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A deploy DTO.
    /// </summary>
    public class DeployDto : IDeployDto
    {
        /// <summary>
        /// Gets or sets the resource models.
        /// </summary>
        public IList<IResourceModel> ResourceModels
        {
            get;
            set;
        }
        public bool DeployTests { get; set; }
        public bool DeployTriggers { get; set; }
    }
}
