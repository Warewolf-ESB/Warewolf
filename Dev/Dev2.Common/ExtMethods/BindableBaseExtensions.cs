using Prism.Mvvm;
using System;

namespace Dev2.Common
{
    //public static class BindableBaseExtensions
    //{
    //    public static void OnPropertyChanged<T>(this BindableBase bindable, System.Linq.Expressions.Expression<Func<T>> propertyExpression)
    //    {
    //        string propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
    //        bindable.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    //    }
    //}


    public class BindableBase2 : Prism.Mvvm.BindableBase
    {
        protected void OnPropertyChanged<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression)
        {
            var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            base.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="T:System.Runtime.CompilerServices.CallerMemberNameAttribute" />.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

}
