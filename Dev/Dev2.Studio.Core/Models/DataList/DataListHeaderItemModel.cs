
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models.DataList
{
    public class DataListHeaderItemModel : BaseDataListItemModel
    {
        #region Ctor

        internal DataListHeaderItemModel(string displayName)
        {
            DisplayName = displayName;
        }

        #endregion Ctor

        #region Override Methods

        public override string ValidateName(string name)
        {
            return name;
        }

        #endregion Override Methods

        #region Overrides of Object

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return DisplayName;
        }

        #endregion
    }
}
