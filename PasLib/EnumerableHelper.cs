using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal static class EnumerableHelper
    {
        public static T ArgMax<T>(this IEnumerable<T> source, Func<T, int> map)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            int? max = null;
            T argMax = default(T);

            foreach (var item in source)
            {
                var metric = map(item);

                if (!max.HasValue || max.Value < metric)
                {
                    max = metric;
                    argMax = item;
                }
            }

            if (!max.HasValue)
            {
                throw new InvalidOperationException($"{nameof(source)} doesn't contain any element");
            }

            return argMax;
        }
    }
}