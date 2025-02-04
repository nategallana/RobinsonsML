using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RobinsonMallLinkingWBOX.Helpers
{
    internal static class ObjectFormatter
    {
        internal static string GetTableName<T>(this T entity)
        {
            if (entity != null)
            {
                return entity.GetType().Name.RemoveNonAlphaNumeric().ToLower();
            }
            return string.Empty;
        }
        internal static string RemoveUnwantedChars(this string value)
        {
            Regex regex = new Regex(@"[^a-zA-Z0-9\-_. ]");
            if (!(string.IsNullOrEmpty(value)))
            {
                return regex.Replace(value, string.Empty);
            }
            return string.Empty;
        }

        internal static IEnumerable<List<T>> SplitList<T>(this List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

    }
}
