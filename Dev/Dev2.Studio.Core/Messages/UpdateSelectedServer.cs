
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Messages
{
    public class UpdateSelectedServer:IMessage
    {
        public IEnvironmentModel EnvironmentModel { get; set; }
        public bool IsSourceServer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public UpdateSelectedServer(IEnvironmentModel environmentModel,bool isSourceServer)
        {
            if(environmentModel == null)
            {
                throw new ArgumentNullException("environmentModel");
            }
            EnvironmentModel = environmentModel;
            IsSourceServer = isSourceServer;
        }
    }
}
