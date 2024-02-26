using System.Collections.Generic;
using System.Linq;

namespace ED.Atlas.Svc.TC.Ice.FE.Common
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitByIndex(this string str, params int[] indexes)
        {
            var previousIndex = 0;
            foreach (var index in indexes.OrderBy(i => i))
            {
                yield return str.Substring(previousIndex, index - previousIndex);
                previousIndex = index;
            }

            yield return str.Substring(previousIndex);
        }
    }
}