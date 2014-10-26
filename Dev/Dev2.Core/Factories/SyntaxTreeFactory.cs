/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Parsing.Intellisense;

namespace Dev2
{
    public static class SyntaxTreeFactory
    {
        public static SyntaxTreeBuilder CreateDatalistTreeBuilder()
        {
            var builder = new SyntaxTreeBuilder();

            builder.RegisterGrammer(new DatalistGrammer());

            return builder;
        }

        public static SyntaxTreeBuilder CreateInfrigistsTreeBuilder()
        {
            var builder = new SyntaxTreeBuilder();

            builder.RegisterGrammer(new StringLiteralGrammer());
            builder.RegisterGrammer(new InfrigisticNumericLiteralGrammer());
            builder.RegisterGrammer(new BooleanLiteralGrammer());
            builder.RegisterGrammer(new InfrigisticsGrammer());

            return builder;
        }

        public static SyntaxTreeBuilder CreateInfrigistsAndDatalistTreeBuilder()
        {
            SyntaxTreeBuilder builder = CreateInfrigistsTreeBuilder();

            builder.RegisterGrammer(new DatalistGrammer());

            return builder;
        }


        public static SyntaxTreeBuilder CreateActivityDataItemTreeBuilder()
        {
            return CreateInfrigistsAndDatalistTreeBuilder();
        }
    }
}