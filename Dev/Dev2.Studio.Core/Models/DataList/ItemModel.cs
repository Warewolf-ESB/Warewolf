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

namespace Dev2.Studio.Core.Models.DataList
{
    public class ItemModel
    {
        readonly bool hasError;
        readonly string errorMessage;
        readonly bool isEditable;
        readonly bool isVisable;
        readonly bool isSelected;

        public bool HasError => hasError;

        public string ErrorMessage => errorMessage;

        public bool IsEditable => isEditable;

        public bool IsVisable => isVisable;

        public bool IsSelected => isSelected;
        
        public ItemModel(bool _isEditable)
        {
            hasError = false;
            errorMessage = "";
            isEditable = _isEditable;
            isVisable = true;
            isSelected = false;
        }

        public ItemModel()
        {
            hasError = false;
            errorMessage = "";
            isEditable = true;
            isVisable = true;
            isSelected = false;
        }
    }
}
