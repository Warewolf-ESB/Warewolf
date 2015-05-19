
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
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Factories
{
    public static class DataMappingListFactory
    {

        public static IList<IDev2Definition> CreateListOutputMapping(string xmlServiceDefintion)
        {
            IList<IDev2Definition> outputDef = DataListFactory.CreateOutputParser().ParseAndAllowBlanks(xmlServiceDefintion);
            return outputDef;
        }

        public static IList<IDev2Definition> CreateListInputMapping(string xmlServiceDefintion)
        {
            IList<IDev2Definition> inputDef = DataListFactory.CreateInputParser().ParseAndAllowBlanks(xmlServiceDefintion);
            return inputDef;
        }

        public static IRecordSetCollection CreateListOutputRecordSetMapping(string xmlServiceDefintion)
        {
            IList<IDev2Definition> outputDef = DataListFactory.CreateOutputParser().Parse(xmlServiceDefintion);
            IRecordSetCollection outputRecSet = DataListFactory.CreateRecordSetCollection(outputDef, true);
            return outputRecSet;
        }

        public static IRecordSetCollection CreateListInputRecordSetMapping(string xmlServiceDefintion)
        {
            IList<IDev2Definition> inputDef = DataListFactory.CreateInputParser().ParseAndAllowBlanks(xmlServiceDefintion);
            IRecordSetCollection inputRecSet = DataListFactory.CreateRecordSetCollection(inputDef, false);
            return inputRecSet;
        }

        public static string GenerateMapping(IList<IDev2Definition> defs, enDev2ArgumentType typeOf)
        {
            return DataListFactory.GenerateMapping(defs, typeOf);
        }

    }
}
