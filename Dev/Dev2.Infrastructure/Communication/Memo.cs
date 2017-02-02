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
using Dev2.Common.Interfaces.Communication;

namespace Dev2.Communication
{
    public class Memo : IMemo
    {
        public Memo()
        {
            Date = DateTime.Now;
        }

        public Guid InstanceID { get; set; }

        public Guid ServerID { get; set; }

        public Guid WorkspaceID { get; set; }

        public DateTime Date { get; set; }

        public string DateString => Date.ToString("yyyy-MM-dd.HH.mm.ss.ffff");

        #region IEquatable

        public bool Equals(IMemo other)
        {
            if(other == null)
            {
                return false;
            }
            return InstanceID == other.InstanceID;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IMemo);
        }

        public override int GetHashCode()
        {
            return InstanceID.GetHashCode();
        }

        #endregion
    }
}
