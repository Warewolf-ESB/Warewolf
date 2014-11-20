
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
using System.ComponentModel;
using System.Linq.Expressions;

namespace Dev2.Tests
{
    public class TestUtils
    {
        public static bool PropertyChangedTester<T,TU>(T objectBeingTested, Expression<Func<TU>> propertyName, Action setter)
            where T : INotifyPropertyChanged
        {
            var wasCalled = false;
            var body = CheckMemberExpression(propertyName);
            var name = body.Member.Name;
            objectBeingTested.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == name)
                {
                    wasCalled = true;
                }
            };
            setter();
            return wasCalled;
        }

        static MemberExpression CheckMemberExpression<TU>(Expression<Func<TU>> propertyName)
        {
            if(propertyName.NodeType != ExpressionType.Lambda)
            {
                throw new ArgumentException(@"Value must be a lamda expression", "propertyName");
            }

            var body = propertyName.Body as MemberExpression;

            if(body == null)
            {
                throw new ArgumentException("Must have body");
            }
            return body;
        }
    }
}
