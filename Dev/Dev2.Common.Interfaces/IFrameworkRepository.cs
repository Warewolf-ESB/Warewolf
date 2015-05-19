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
using System.Linq.Expressions;

namespace Dev2.Common.Interfaces
{
    public interface IFrameworkRepository<T> : IDisposable
    {
        ICollection<T> All();
        ICollection<T> Find(Expression<Func<T, bool>> expression);
        T FindSingle(Expression<Func<T, bool>> expression);
        string Save(T instanceObj);
        void Save(ICollection<T> instanceObjs);
        event EventHandler ItemAdded;
        void Load();
        void Remove(T instanceObj);
        void Remove(ICollection<T> instanceObjs);
    }
}