
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Activities.Designers2.Core
{
    public class IsItemDragged
    {

        #region Fields
        private static IsItemDragged _instance;
        bool _isDragged;

        #endregion

        #region Ctor

        public IsItemDragged()
        {
            IsDragged = false;
        }

        #endregion

        #region Properties

        public static IsItemDragged Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new IsItemDragged();
                }
                return _instance;
            }
        }

        public bool IsDragged
        {
            get
            {
                return _isDragged;
            }
            set
            {
                _isDragged = value;
            }
        }

        #endregion
    }
}
