#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
#if WINDOWS || NETFRAMEWORK
using System.Activities.Presentation.Model;
#endif



namespace Dev2.Studio.Core.Messages
{
    public class EditActivityMessage : IMessage
	{
#if WINDOWS || NETFRAMEWORK
        public ModelItem ModelItem { get; private set; }
#endif
        public Guid ParentEnvironmentID { get; private set; }

#if WINDOWS || NETFRAMEWORK
        public EditActivityMessage(ModelItem modelItem, Guid parentEnvironmentID)
        {
            ModelItem = modelItem;
            ParentEnvironmentID = parentEnvironmentID;
        }
#endif
    }
}
