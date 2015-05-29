using System;
using System.Collections.Generic;
using System.Linq;

namespace Warewolf.Core
{
    public static class DescendantsExtension
    {
        public static IEnumerable<T> Descendants<T>(this T root,Func<T,IEnumerable<T>> childrenFunc )
        {
            var nodes = new Stack<T>(new[] { root });
            while (nodes.Any())
            {
                T node = nodes.Pop();
                yield return node;
                foreach (var n in childrenFunc(root)) nodes.Push(n);
            }
        }

    }
}