
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

namespace Dev2.Data.TO
{
    public class StringSegments
    {
        const int MAX_SIZE_FOR_STRING = 1 << 12; // = 4K

        public List<string> Segments { get; private set; }

        public int BoundrySize { get; private set; }

        public StringSegments()
        {
            Segments = new List<string>();
            BoundrySize = MAX_SIZE_FOR_STRING;
        }

        public bool IsEmpty()
        {
            return (Length() == 0);
        }

        public int Length()
        {
            int result = 0;

            Segments.ForEach(s=> result += s.Length);

            return result;
        }
        
        public void AddSegment(string segment)
        {
            
        }
    }
}
