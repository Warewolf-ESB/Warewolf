
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2WorkflowHotFixer
{
    class Program
    {
        static void Main(string[] args)
        {

            string magicStart = "&lt;FlowDecision x:Name=";
            string magicEnd = "sap:VirtualizedContainerService.HintSize=\"70,87\"&gt;";

            string conditionStart = "Condition=";
            string conditionEnd = "]\"";

            string baseWF = @"C:\Development\Dev\Dev2.Server\bin\Debug\Services\Decision Testing.xml";
            string newWF = @"C:\Development\Dev\Dev2.Server\bin\Debug\Services\Decision Testing New.xml";

            string[] data = File.ReadAllLines(baseWF);


            foreach (string line in data)
            {
                // we have a match ;)
                if (line.IndexOf(magicStart, StringComparison.Ordinal) >= 0 && line.IndexOf(magicEnd, StringComparison.Ordinal) >= 0)
                {
                    // now find the condition ;)
                    int start = line.IndexOf(conditionStart);
                    if (start >= 0)
                    {
                        int end = line.IndexOf(conditionEnd, start);
                        if (end >= 0)
                        {
                            string condition = line.Substring(start, end)
                        }
                    }
                }
            }


        }
    }
}
