using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WikiUtil
{
    internal static class Util
    {
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }

        public static Vector2 Round(this Vector2 vector)
        {
            return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
        }

        public static string SafeString(string str) => Regex.Replace(str, "[^\\w\\d\\-]", "_"); // guaranteed to be able to make into a file name
    }
}
