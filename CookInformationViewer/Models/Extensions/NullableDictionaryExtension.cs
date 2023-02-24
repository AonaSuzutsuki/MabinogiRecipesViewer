using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookInformationViewer.Models.Extensions
{
    public static class NullableDictionaryExtension
    {
        public static TV Get<TK, TV>(this Dictionary<TK, TV> dict, TK key, TV defaultValue) where TK : notnull
            => dict.ContainsKey(key) ? dict[key] : defaultValue;
    }
}
