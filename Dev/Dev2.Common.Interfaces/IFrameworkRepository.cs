using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dev2.Common.Interfaces {

    public interface IFrameworkRepository<T> : IDisposable {
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
