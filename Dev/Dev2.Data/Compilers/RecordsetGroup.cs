
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
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Compilers
{
    /// <summary>
    /// Used to batch up recordset mapping operations ;)
    /// </summary>
    public class RecordsetGroup
    {
        public RecordsetGroup(IBinaryDataListEntry sourceEntry, IList<IDev2Definition> definitions, Func<IDev2Definition, string> inputExpressionExtractor, Func<IDev2Definition, string> outputExpressionExtractor)
        {
            if(sourceEntry == null)
            {
                throw new ArgumentNullException("sourceEntry");
            }

            if(definitions == null)
            {
                throw new ArgumentNullException("definitions");
            }
            if(inputExpressionExtractor == null)
            {
                throw new ArgumentNullException("inputExpressionExtractor");
            }
            if(outputExpressionExtractor == null)
            {
                throw new ArgumentNullException("outputExpressionExtractor");
            }

            SourceEntry = sourceEntry;
            Definitions = definitions;
            InputExpressionExtractor = inputExpressionExtractor;
            OutputExpressionExtractor = outputExpressionExtractor;
        }

        public IBinaryDataListEntry SourceEntry { get; private set; }
        public IList<IDev2Definition> Definitions { get; private set; }
        public Func<IDev2Definition, string> InputExpressionExtractor { get; private set; }
        public Func<IDev2Definition, string> OutputExpressionExtractor { get; private set; }
    }
}
