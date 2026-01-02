using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using acquleo.Base.Class;

namespace acquleo.Base.Reflection
{
    /// <summary>
    /// Enumerates the constants of the type T
    /// </summary>
    public static class ConstantsEnumerator
    {
        static string GetDescription(FieldInfo fieldInfo)
        {
            string description = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault()?
                .As<DescriptionAttribute>()?
                .Description?.ToString()??string.Empty;
            return description;
        }

        /// <summary>
        /// Enumerates the constants of the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<ConstantValue> Enum<T>()
        {
            var roottype = typeof(T);
            return roottype.GetFields(BindingFlags.Public | BindingFlags.Static |
                   BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                    .Select(c => 
                    new ConstantValue { 
                        Name = c.Name, 
                        Value = c.GetValue(c),
                        Description= GetDescription(c)
                    }).ToList();
        }

        /// <summary>
        /// check duplicate constant values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        public static void CheckDuplicateConstantValues(this List<ConstantValue> codes)
        {
            var query = codes.GroupBy(x => x.Value)
                        .Where(g => g.Count() > 1)
                        .Select(y => y.Key)
                        .ToList();

            if (query.Count > 0)
            {
                throw new InvalidOperationException($@"Duplicate codes found {string.Join("-", query.Select(d => d?.ToString() ?? string.Empty).ToList())}");
            }
        }

        /// <summary>
        /// check duplicate constant values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        public static void CheckDuplicateConstantValues<T>()
        {
            var codes = Enum<T>();

            var query = codes.GroupBy(x => x.Value)
                        .Where(g => g.Count() > 1)
                        .Select(y => y.Key)
                        .ToList();

            if (query.Count > 0)
            {
                throw new InvalidOperationException($@"Duplicate codes found {string.Join("-", query.Select(d=>d?.ToString()??string.Empty).ToList())}");
            }
        }

        /// <summary>
        /// return the name of the constant value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Tval"></typeparam>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool NameOf<T, Tval>(Tval value, out string name)
        {
            name = string.Empty;

            EqualityComparer<Tval> comparer = EqualityComparer<Tval>.Default;

            foreach (FieldInfo field in typeof(T).GetFields
                     (BindingFlags.Static | BindingFlags.Public))
            {
                if (field.FieldType == value.GetType() &&
                    comparer.Equals(value, (Tval)field.GetValue(null)))
                {
                    name = field.Name;
                    return true; // There could be others, of course...
                }
            }
            return false; // Or throw an exception
        }
    }

    /// <summary>
    /// Represent an enumerated constant value
    /// </summary>
    public class ConstantValue
    {
        /// <summary>
        /// name of the constant
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// value of the constant
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// description of the constant
        /// </summary>
        public string Description { get; set; }
    }
}
