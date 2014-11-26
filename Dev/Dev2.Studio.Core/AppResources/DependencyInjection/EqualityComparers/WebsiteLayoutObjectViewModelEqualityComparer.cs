
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
using Dev2.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers
{
    public class WebsiteLayoutObjectViewModelEqualityComparer : IEqualityComparer<ILayoutObjectViewModel>
    {
        public bool Equals(ILayoutObjectViewModel x, ILayoutObjectViewModel y)
        {
            return x.WebpartServiceDisplayName.Equals(y.WebpartServiceDisplayName, StringComparison.InvariantCultureIgnoreCase)
                && x.WebpartServiceName.Equals(y.WebpartServiceName, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(ILayoutObjectViewModel obj)
        {
            return obj.WebpartServiceDisplayName.ToLower().GetHashCode() + obj.WebpartServiceName.ToLower().GetHashCode();
        }
    }
}
