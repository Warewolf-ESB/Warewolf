/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dev2.Activities.Designers2.Core
{
    public static class CEventHelper
    {
        static readonly Dictionary<Type, List<FieldInfo>> DicEventFieldInfos = new Dictionary<Type, List<FieldInfo>>();

        static BindingFlags AllBindings => BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        
        static IEnumerable<FieldInfo> GetTypeEventFields(Type t)
        {
            if (DicEventFieldInfos.ContainsKey(t))
            {
                return DicEventFieldInfos[t];
            }

            var lst = new List<FieldInfo>();
            BuildEventFields(t, lst);
            DicEventFieldInfos.Add(t, lst);
            return lst;
        }
        
        static void BuildEventFields(Type t, List<FieldInfo> lst)
        {
            // Type.GetEvent(s) gets all Events for the type AND it's ancestors
            // Type.GetField(s) gets only Fields for the exact type.
            //  (BindingFlags.FlattenHierarchy only works on PROTECTED & PUBLIC
            //   doesn't work because Fieds are PRIVATE)

            // NEW version of this routine uses .GetEvents and then uses .DeclaringType
            // to get the correct ancestor type so that we can get the FieldInfo.
            lst.AddRange(from ei in t.GetEvents(AllBindings)
                         let dt = ei.DeclaringType
                         where dt != null
                         select dt.GetField(ei.Name, AllBindings)
                         into fi where fi != null select fi);
        }

        public static void RemoveAllEventHandlers(object obj) => TryRemoveEventHandler(obj, "");

        public static void TryRemoveEventHandler(object obj, string eventName)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }
                RemoveEventHandler(obj, eventName);
            }
            catch (Exception e)
            {
                Dev2Logger.Warn(e.Message, "Warewolf Warn");
            }
        }

        static void RemoveEventHandler(object obj, string eventName)
        {
            var t = obj.GetType();
            var eventFields = GetTypeEventFields(t);

            foreach (var fi in eventFields.Where(fi => eventName == "" || String.Compare(eventName, fi.Name, StringComparison.OrdinalIgnoreCase) == 0))
            {
                RemoveEventField(obj, t, fi);
            }
        }

        static void RemoveEventField(object obj, Type t, FieldInfo fi)
        {
            if (!fi.IsStatic)
            {
                RemoveNonStaticEventField(obj, t, fi);
            }
        }

        static void RemoveNonStaticEventField(object obj, Type t, FieldInfo fi)
        {
            var ei = t.GetEvent(fi.Name, AllBindings);
            if (ei != null)
            {
                var val = fi.GetValue(obj);
                if (val is Delegate mdel)
                {
                    RemoveAllDelegates(obj, ei, mdel);
                }
            }
        }

        static void RemoveAllDelegates(object obj, EventInfo ei, Delegate mdel)
        {
            foreach (Delegate del in mdel.GetInvocationList())
            {
                ei.RemoveEventHandler(obj, del);
            }
        }
    }
}
