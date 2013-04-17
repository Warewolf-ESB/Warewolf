using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dev2 {
    public interface IFrameworkRepository<T>  {
        ICollection<T> All();
        ICollection<T> Find(Expression<Func<T, bool>> expression);
        T FindSingle(Expression<Func<T, bool>> expression);
        void Save(T instanceObj);
        void Save(ICollection<T> instanceObjs);
        event EventHandler ItemAdded;
        void Load();
        void Remove(T instanceObj);
        void Remove(ICollection<T> instanceObjs);

    }

}
