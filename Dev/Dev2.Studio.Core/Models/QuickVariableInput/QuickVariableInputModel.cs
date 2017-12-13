/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Interfaces;


namespace Dev2.Studio.Core.Models.QuickVariableInput
{
    public class QuickVariableInputModel
    {

        readonly ICollectionActivity _activity;
        readonly ModelItem _modelItem;

        public QuickVariableInputModel(ModelItem modelItem, ICollectionActivity activity)
        {
            _modelItem = modelItem;
            _activity = activity;
        }

        public int GetCollectionCount()
        {
            return _activity.GetCollectionCount();
        }

        public void AddListToCollection(IList<string> listToAdd, bool overwrite)
        {
            _activity.AddListToCollection(listToAdd, overwrite, _modelItem);
        }
    }
}
