/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class EditActivityMessage : IMessage
    {
        public ModelItem ModelItem { get; private set; }
        public Guid ParentEnvironmentID { get; private set; }

        public EditActivityMessage(ModelItem modelItem, Guid parentEnvironmentID)
        {
            ModelItem = modelItem;
            ParentEnvironmentID = parentEnvironmentID;
        }
    }
}
