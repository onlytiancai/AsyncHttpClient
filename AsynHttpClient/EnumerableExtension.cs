using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsynHttpClient
{
    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> col, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            foreach (var item in col)
            {
                action(item);
            }
        }
    }
}
