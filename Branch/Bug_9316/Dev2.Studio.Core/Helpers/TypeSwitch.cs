using System;
using System.Data;
using System.Linq;
using System.ServiceModel;

namespace Dev2.Studio.Core.Helpers
{
    public static class TypeSwitch
    {
        public class CaseInfo
        {
            public bool IsDefault { get; set; }
            public Type Target { get; set; }
            public Action<object> Action { get; set; }
        }

        public static void Do(object source, params CaseInfo[] cases)
        {
            if (source == null)
            {
                if (!cases.ToList().Any(c => c.IsDefault))
                    throw new Exception("Can not do switch on null type argument if no default implementation provided");
                
                foreach (var entry in cases.Where(entry => entry.IsDefault))
                {
                    entry.Action(null);
                    break;
                }
            }
            else
            {
                var type = source.GetType();
                foreach (var entry in cases.Where(entry => entry.IsDefault || entry.Target.IsAssignableFrom(type)))
                {
                    entry.Action(source);
                    break;
                }
            }
        }

        public static CaseInfo Case<T>(Action action)
        {
            return new CaseInfo
                {
                    Action = x => action(),
                    Target = typeof (T)
                };
        }

        public static CaseInfo Case<T>(Action<T> action)
        {
            return new CaseInfo
                {
                    Action = x => action((T) x),
                    Target = typeof (T)
                };
        }

        public static CaseInfo Default(Action action)
        {
            return new CaseInfo
                {
                    Action = x => action(),
                    IsDefault = true
                };
        }
    }
}
