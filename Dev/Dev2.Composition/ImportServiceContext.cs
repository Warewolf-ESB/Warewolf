
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Composition
{
    public class ImportServiceContext
    {
        private readonly object _value;
        private readonly int _hash;

        public ImportServiceContext()
        {
            _value = new object();
            _hash = _value.GetHashCode();
        }

        public override int  GetHashCode()
        {
 	        return _hash;
        }
    }
}
