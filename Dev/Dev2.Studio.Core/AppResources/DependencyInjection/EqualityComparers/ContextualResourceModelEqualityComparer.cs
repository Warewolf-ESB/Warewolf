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
using System.Collections.Generic;
using Dev2.Studio.Interfaces;


namespace Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers
{
    public class ContexttualResourceModelEqualityComparer : IEqualityComparer<IContextualResourceModel>
    {
        private static readonly Lazy<ContexttualResourceModelEqualityComparer> _current
            = new Lazy<ContexttualResourceModelEqualityComparer>(() => new ContexttualResourceModelEqualityComparer());

        private ContexttualResourceModelEqualityComparer()
        {
        }

        public static ContexttualResourceModelEqualityComparer Current => _current.Value;

        public bool Equals(IContextualResourceModel x, IContextualResourceModel y)
        {
            if(ReferenceEquals(x, y))
            {
                return true;
            }            
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }            
            return EnvironmentModelEqualityComparer.Current.Equals(x.Environment, y.Environment) && x.ResourceName == y.ResourceName;
        }

        public int GetHashCode(IContextualResourceModel obj) => throw new NotImplementedException();
    }
}
