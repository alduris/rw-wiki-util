using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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

        public static T GetUninitialized<T>() => (T)FormatterServices.GetUninitializedObject(typeof(T));

        public static HashSet<TEnum> GetExtEnumFieldsFromClass<TEnum, TClass>()
        {
            return [.. typeof(TClass).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.FieldType == typeof(TEnum) && x.GetValue(null) != null)
                .Select(x => x.GetValue(null))
                .Cast<TEnum>()];
        }
    }
}
