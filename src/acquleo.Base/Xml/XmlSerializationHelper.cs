using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace acquleo.Base.Xml
{
    /// <summary>
    /// xmlserializer extensions
    /// </summary>
    public static class XmlSerializationHelper
    {
        static Dictionary<string, XmlSerializer> map = new Dictionary<string, XmlSerializer>();
        static readonly object sync = new object();
        /// <summary>
        /// Clear the serializer cache
        /// </summary>
        public static void ClearSerializerCache()
        {
            map.Clear();
        }

        /// <summary>
        /// returns a serializer for the type T
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <returns></returns>
        public static XmlSerializer GetSerializer<T>() where T : class
        {
            Type t = typeof(T);
            return GetSerializer(t);
        }

        /// <summary>
        /// returns a serializer for the type t
        /// </summary>
        /// <param name="t">input type</param>
        /// <returns></returns>
        public static XmlSerializer GetSerializer(Type t)
        {
            lock (sync)
            {
                if (!map.ContainsKey(t.FullName))
                {
                    map.Add(t.FullName, new XmlSerializer(t));
                }
                return map[t.FullName];
            }
        }

        /// <summary>
        /// deserialize an object from a string
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="data">deserialized object</param>
        /// <returns></returns>
        public static T XmlDeserializeFromString<T>(this string data) where T : class
        {
            return Deserialize<T>(data);
        }


        /// <summary>
        /// deserialize an object from a string
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="data">deserialized object</param>
        /// <returns></returns>
        public static T Deserialize<T>(string data) where T : class
        {
            Type t = typeof(T);
            return Deserialize(t, data) as T;
        }

        /// <summary>
        /// deserialize an object from a string
        /// </summary>
        /// <param name="type">object type</param>
        /// <param name="data">deserialized object</param>
        /// <returns></returns>
        public static object Deserialize(Type type, string data)
        {
            
            XmlSerializer serializer = GetSerializer(type);
            using (StringReader reader = new StringReader(data))
            {
                return serializer.Deserialize(reader);
            }
        }

       /// <summary>
       /// deserialize an object from a file
       /// </summary>
       /// <typeparam name="T">object type</typeparam>
       /// <param name="path">deserialized object</param>
       /// <returns></returns>
        public static T DeserializeFromFile<T>(string path) where T : class
        {
            Type t = typeof(T);
            return DeserializeFromFile(t,path) as T;
        }

        /// <summary>
        /// deserialize an object from a file
        /// </summary>
        /// <param name="type">object type</param>
        /// <param name="path">deserialized object</param>
        /// <returns></returns>
        public static object DeserializeFromFile(Type type, string path)
        {
            XmlSerializer serializer = GetSerializer(type);
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    return serializer.Deserialize(reader);
                }
            }
        }
        

       /// <summary>
       /// serialize an object to a string
       /// </summary>
       /// <typeparam name="T">object type</typeparam>
       /// <param name="data">object to be serialized</param>
       /// <returns>xml string</returns>
        public static string XmlSerializeToString<T>(this T data) where T : class
        {
            return Serialize<T>(data);
        }
        
        /// <summary>
        /// serialize an object to a string
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="data">object to be serialized</param>
        /// <returns>xml string</returns>
        public static string Serialize<T>(T data) where T : class
        {
            Type t = typeof(T);
            return Serialize(t, data);
        }

        /// <summary>
        /// serialize an object to a string
        /// </summary>
        /// <param name="type">object type</param>
        /// <param name="data">object to be serialized</param>
        /// <returns>xml string</returns>
        public static string Serialize(Type type, object data)
        {
            return Serialize(type, data, null);
        }

        /// <summary>
        /// serlialize an object to a string using the specified Encoding
        /// </summary>
        /// <param name="type">object type</param>
        /// <param name="data">object to be serialized</param>
        /// <param name="encoding">text encoding</param>
        /// <returns></returns>
        public static string Serialize(Type type, object data, Encoding encoding)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            if(encoding!=null) settings.Encoding = encoding;
            XmlSerializerNamespaces names = new XmlSerializerNamespaces();
            names.Add(string.Empty, string.Empty);
            XmlSerializer serializer = GetSerializer(type);
            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                serializer.Serialize(writer, data, names);
                return builder.ToString();
            }
        }

        /// <summary>
        /// serializza un oggetto su file xml
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="data">object to be serialized</param>
        /// <param name="path">file path</param>
        public static void SerializeToFile<T>(T data,string path) where T : class
        {            
            SerializeToFile<T>(data, path,true);
        }
        /// <summary>
        /// serializza un oggetto su file xml
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="data">object to be serialized</param>
        /// <param name="path">file path</param>
        /// <param name="excludeNamespace">exclude xml namespaces</param>
        public static void SerializeToFile<T>(T data, string path, bool excludeNamespace) where T : class
        {
            Type t = typeof(T);
            SerializeToFile(t, data, path, excludeNamespace);
        }

        /// <summary>
        /// serializza un oggetto su file xml
        /// </summary>
        /// <param name="type">object type</param>
        /// <param name="data">object to be serialized</param>
        /// <param name="path">file path</param>
        /// <param name="excludeNamespace">exclude xml namespaces</param>
        public static void SerializeToFile(Type type, object data, string path,bool excludeNamespace)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            if (excludeNamespace)
            {
                settings.OmitXmlDeclaration = true;
            }
            settings.Indent = true;
            
            XmlSerializerNamespaces names = new XmlSerializerNamespaces();
            names.Add(string.Empty, string.Empty);
            XmlSerializer serializer = GetSerializer(type);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sWriter = new StreamWriter(fs))
                {
                    using (XmlWriter writer = XmlWriter.Create(sWriter, settings))
                    {
                        if (excludeNamespace)
                            serializer.Serialize(writer, data, names);
                        else
                            serializer.Serialize(writer, data);
                    }
                }
            }
        }
    }
}
