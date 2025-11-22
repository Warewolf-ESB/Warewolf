using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Dev2.WorkflowConverters
{
    public class ActivityPropertiesReaderHelper
    {
        private static readonly ConcurrentDictionary<Type, List<Func<Activity, IEnumerable<Activity>>>> _childExtractorsCache = new();

        public static void GetChildActivities(Activity activity, List<Activity> children)
        {
            children.Clear();

            var extractors = _childExtractorsCache.GetOrAdd(activity.GetType(), BuildExtractors);

            foreach (var extractor in extractors)
            {
                foreach (var child in extractor(activity))
                {
                    children.Add(child);
                }
            }
        }

        private static List<Func<Activity, IEnumerable<Activity>>> BuildExtractors(Type activityType)
        {
            var extractors = new List<Func<Activity, IEnumerable<Activity>>>();

            foreach (var prop in activityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (typeof(Activity).IsAssignableFrom(prop.PropertyType))
                {
                    var getter = BuildGetter(prop);
                    extractors.Add(a =>
                    {
                        var child = getter(a) as Activity;
                        return child != null ? new[] { child } : Array.Empty<Activity>();
                    });
                }
                else if (typeof(ICollection<Activity>).IsAssignableFrom(prop.PropertyType))
                {
                    var getter = BuildGetter(prop);
                    extractors.Add(a => (getter(a) as ICollection<Activity>) ?? Array.Empty<Activity>());
                }
            }

            return extractors;
        }

        private static Func<Activity, object?> BuildGetter(PropertyInfo prop)
        {
            var instance = Expression.Parameter(typeof(Activity), "instance");
            var convert = Expression.Convert(instance, prop.DeclaringType!);
            var propertyAccess = Expression.Property(convert, prop);
            var castResult = Expression.Convert(propertyAccess, typeof(object));
            return Expression.Lambda<Func<Activity, object?>>(castResult, instance).Compile();
        }

    }

}
