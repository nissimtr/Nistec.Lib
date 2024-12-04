//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Linq;
using System.Data;
using System.Xml;
using Nistec.Generic;
using Nistec.IO;
using System.Runtime.Serialization.Formatters;
using System.Collections.Specialized;
using Nistec.Runtime;
using System.Collections.Concurrent;
using System.Diagnostics;
#pragma warning disable  CS1591

namespace Nistec.Serialization
{

    /// <summary>
    ///   Some help functions for the serializing framework. As these functions are complexer
    ///   they can be converted to single classes.
    /// </summary>
    public static class SerializeTools
    {

        #region PropertyInfo

        
        //public static PropertyInfo[] GetValidProperties(Type type, bool isPublic)
        //{
        //    return type.GetProperties(isPublic, false, SerializeTools.GetExcludeAttribute());
        //}
        //public static Type[] GetExcludeAttribute()
        //{
        //    return new Type[] { typeof(NoSerializeAttribute) };//, typeof(EntitySerializeAttribute) };
        //}


        /*
        public static PropertyInfo[] GetValidProperties(Type type, bool isPublic)
        {
           //&& p.PropertyType.IsSerializable
            if (isPublic)
                return type.GetProperties().Where(p => ((p.PropertyType.IsPublic && !p.IsDefined(typeof(NoSerializeAttribute), false)) || (p.IsDefined(typeof(EntitySerializeAttribute), false)))  && p.CanWrite && p.CanRead).ToArray();
            return type.GetProperties().Where(p => p.CanWrite && p.CanRead).ToArray();
        }

        public static PropertyInfo[] GetObjectProperties(object instance, bool isPublic)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            var type = instance.GetType();

        //&& p.PropertyType.IsSerializable

            if (isPublic)
                return type.GetProperties().Where(p => ((p.PropertyType.IsPublic && !p.IsDefined(typeof(NoSerializeAttribute), false)) || (p.IsDefined(typeof(EntitySerializeAttribute), false)))  && p.CanWrite && p.CanRead).ToArray();

            return type.GetProperties().Where(p => p.CanWrite && p.CanRead).ToArray();
        }

        static int GetObjectFieldsCount(object instance, bool isPublic)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            var type = instance.GetType();
            // && p.PropertyType.IsSerializable
            if (isPublic)
                return type.GetProperties().Where(p => ((p.PropertyType.IsPublic && !p.IsDefined(typeof(NoSerializeAttribute), false)) || (p.IsDefined(typeof(EntitySerializeAttribute), false))) && p.CanWrite && p.CanRead).Count();

            return type.GetProperties().Where(p => p.CanWrite && p.CanRead).Count();
        }

        public static object[] GetObjectPropertiesValues(object instance, bool isPublic)
        {
            var properties = GetObjectProperties(instance, isPublic);
            List<object> values = new List<object>();

            foreach (var property in properties)
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                object fieldValue = property.GetValue(instance, null);

                values.Add(fieldValue);
            }
            return values.ToArray();
        }

        public static GenericNameValue GetObjectPropertiesNameValues(object instance, bool isPublic)
        {
            GenericNameValue values = new GenericNameValue();
            var properties = GetObjectProperties(instance, isPublic);

            foreach (var property in properties)
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                object fieldValue = property.GetValue(instance, null);

                values.Add(property.Name, fieldValue);
            }
            return values;
        }

        public static string GetObjectPropertiesNameValuesString(object instance, bool isPublic)
        {
            StringBuilder values = new StringBuilder();
            var properties = GetObjectProperties(instance, isPublic);

            foreach (var property in properties)
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                object fieldValue = property.GetValue(instance, null);

                values.AppendFormat("{0}:{1}|", property.Name, fieldValue);
            }
            if (values.Length == 0)
                return null;
            return values.ToString().TrimEnd('|');
        }
        public static T CreateInstance<T>(object[] values)
        {
            T instance = System.Activator.CreateInstance<T>();

            var properties = GetObjectProperties(instance, true);

            if (values.Length != properties.Length)
            {
                throw new Exception("Object properties not match to record values");
            }
            int i = 0;
            foreach (var property in properties)
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                object fieldValue = values[i];

                object value = GenericTypes.ConvertProperty(fieldValue, property);

                property.SetValue(instance, value, null);

                i++;
            }
            return instance;
        }
        */

        #endregion

        public static bool IsEntityClass(Type type)
        {
            if (IsPrimitive(type))
                return false;
            if (IsEnumerable(type))
                return false;
            if (IsCollection(type))
                return false;
            if (IsAssignableFromDictionary(type))
                return false;
            if (IsArray(type))
                return false;
            if (IsType(type))
                return false;
            return type.IsClass;
        }

        public static bool IsEntityClassOrStruct(Type type)
        {
             return IsEntityClass(type) || type.IsValueType;
        }

        public static bool IsEntityClassOrStructSerialize(Type type)
        {
            if (HasNoSerializeAttribute(type))
                return false;
            return IsEntityClassOrStruct(type);
        }
        public static bool HasNoSerializeAttribute(Type type)
        {
            if (type.IsDefined(typeof(NoSerializeAttribute), false))
            {
                return true;
            }
            return false;
        }
        
        public static bool HasNoSerializeAttribute(FieldInfo p)
        {
            if (p.IsDefined(typeof(NoSerializeAttribute), false))
            {
                return true;
            }
            return false;
        }
        public static bool HasNoSerializeAttribute(PropertyInfo p)
        {
            if (p.IsDefined(typeof(NoSerializeAttribute),false))
            {
                return true;
            }
            return false;
        }
       
        public static bool HasSerializeAttribute(FieldInfo p)
        {
            if (p.IsDefined(typeof(SerializeAttribute), false))
            {
                return true;
            }
            return false;
        }
        public static bool HasRawJsonAttribute(PropertyInfo p)
        {
            if (p.IsDefined(typeof(RawJsonAttribute), false))
            {
                return true;
            }
            return false;
        }
        public static bool HasRawJsonAttribute(FieldInfo p)
        {
            if (p.IsDefined(typeof(RawJsonAttribute), false))
            {
                return true;
            }
            return false;
        }
        public static bool HasSerializeAttribute(PropertyInfo p)
        {
            if (p.IsDefined(typeof(SerializeAttribute), false))
            {
                return true;
            }
            return false;
        }
        public static bool HasSerializeAttribute(Type type)
        {
            if (type.IsDefined(typeof(SerializeAttribute), false))
            {
                return true;
            }
            return false;
        }
        public static bool HasSerializableAttribute(Type type)
        {
            if (type.IsDefined(typeof(SerializableAttribute), false))
            {
                return true;
            }
            return false;
        }
        public static bool IsISerializable(Type type)
        {
            if (typeof(ISerializable).IsAssignableFrom(type))
            {
                return true;
            }
            return false;
        }

        //public static bool IsEntitySerialize(Type type)
        //{
        //    if (type.IsDefined(typeof(EntitySerializeAttribute), false))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //public static bool IsSerialize(PropertyInfo p)
        //{
        //    if (p.IsDefined(typeof(SerializeAttribute), false))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        public static bool EnableEntitySerialize(Type type, bool enableAny)
        {
            //if (type.IsDefined(typeof(EntitySerializeAttribute), false))
            //    return true;
            //if (p.IsDefined(typeof(SerializeAttribute), false))
            //    return true;
            if (HasSerializeAttribute(type))
                return true;
            if (HasNoSerializeAttribute(type))
                return false;
            if (!HasSerializableAttribute(type))//?????
                return true;
            if (enableAny)
            {
                return IsEntityClassOrStruct(type);
            }

            return false;
        }

        public static bool IsPrimitiveOrString(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }

            return IsPrimitive(type);
        }

        /// <summary>
        ///   Is the simple type (string, DateTime, TimeSpan, Decimal, Enumeration or other primitive type)
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsPrimitive(Type type)
        {
            if (type == typeof(DateTime))
            {
                return true;
            }
            if (type == typeof(TimeSpan))
            {
                return true;
            }
            if (type == typeof(Decimal))
            {
                // new since the version 2
                return true;
            }
            if (type == typeof(Guid))
            {
                // new since the version 2.8
                return true;
            }
 

            return type.IsPrimitive;
        }

        /// <summary>
        ///   Is the simple type (string, DateTime, TimeSpan, Decimal, Enumeration or other primitive type)
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsSimple(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }
            if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
            {
                // new since v.2.11
                return true;
            }
            if (type.IsEnum)
            {
                return true;
            }
            if (type == typeof(byte[]))
            {
                // since v.2.16 is byte[] a simple type
                return true;
            }

            return IsPrimitive(type);
        }

        /// <summary>
        ///   Is type an IEnumerable
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is type an Generic IEnumerable
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsGenericEnumerable(Type type)
        {
            return type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is type ICollection
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsCollection(Type type)
        {
            return typeof(ICollection).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is type IDictionary
        /// </summary>
        /// <param name = "type"></param>
        /// <param name = "includeGeneric"></param>
        /// <returns></returns>
        public static bool IsAssignableFromDictionary(Type type, bool includeGeneric=false)
        {
            if (includeGeneric)
                return (type.IsGenericType || (type.BaseType != null && type.BaseType.IsGenericType)) && typeof(IDictionary).IsAssignableFrom(type);
            return typeof(IDictionary).IsAssignableFrom(type);
        }

        public static bool IsHashtable(Type type)
        {
            return type== typeof(Hashtable);
        }

        /// <summary>
        ///   Is type Is Generic Dictionary{string,object}
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsStringObjectDictionary(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<string,object>) || type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<string, object>));
        }

        /// <summary>
        ///   Is type Is Generic Dictionary
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsGenericDictionary(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>));
        }
       
        public static bool IsGenericKeyStringDictionary(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>)) && type.GetGenericArguments()[0] == typeof(string);
            //return type.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(type) && type.GetGenericArguments()[0] == typeof(string);
        }

        public static bool IsKeyValuePair(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() != null ? type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>) : false;
            }
            return false;
        }
        public static bool IsKeyValuePaire<TKey, TValue>(Type type)
        {
            return type == typeof(KeyValuePair<TKey, TValue>);
        }
       

        /// <summary>
        ///   Is type Is Generic List
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsGenericList(Type type)
        {

            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)));

            //return type.IsGenericType && typeof(IList<>).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is type Is Generic Hashset
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsGenericSet(Type type)
        {
            return type.IsGenericType && typeof(ISet<>).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is it array? It does not matter if singledimensional or multidimensional
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsArray(Type type)
        {
            return type.IsArray;
        }

        public static bool IsType(Type type)
        {
            return type == typeof(Type) || type.IsSubclassOf(typeof(Type));
        }
        public static bool IsDataTable(Type type)
        {
            return type == typeof(DataTable);
        }
        public static bool IsDataSet(Type type)
        {
            return type == typeof(DataSet);
        }
         public static bool IsStream(Type type)
        {
            return typeof(Stream).IsAssignableFrom(type);
        }
         public static bool IsXmlDocument(Type type)
         {
             return typeof(XmlDocument)==type;
         }
         public static bool IsXmlNode(Type type)
        {
            return typeof(XmlNode).IsAssignableFrom(type);
        }
         public static bool IsListKeyValue<TKey,TValue>(Type type)
        {
            return typeof(IKeyValue).IsAssignableFrom(type)
                || type == typeof(List<KeyValuePair<TKey, TValue>>);
        }
        

        //System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089

        //public static bool IsGenericNameValue(Type type)
        //{
        //    return typeof(IKeyValue).IsAssignableFrom(type) 
        //        || type == typeof(List<KeyValuePair<string, string>>);
        //}
        public static bool IsISerialEntity(Type type)
        {
            return typeof(ISerialEntity).IsAssignableFrom(type);
        }
        public static bool IsISerialContext(Type type)
        {
            return typeof(ISerialContext).IsAssignableFrom(type);
        }
         public static bool IsISerialJson(Type type)
         {
             return typeof(ISerialJson).IsAssignableFrom(type);
         }

        public static void LoadReferencedAssembly(Type type)
        {
            LoadReferencedAssembly(type.Assembly);
        }
        //public static void LoadReferencedAssembly()
        //{
        //    var w = Stopwatch.StartNew();
        //    try
        //    {
        //        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("System") == false).ToList();
        //        foreach (Assembly a in loadedAssemblies)
        //        {
        //            LoadReferencedAssembly(a);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string err = ex.Message;
        //    }
        //    w.Stop();
        //    Console.WriteLine("LoadReferencedAssemblies :{0}", w.ElapsedMilliseconds);
        //}

        public static void LoadReferencedAssembly(Assembly assembly)
        {
           
            try
            {
                foreach (AssemblyName name in assembly.GetReferencedAssemblies().Where(a => a.FullName.StartsWith("System") == false))
                {
                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == name.FullName && a.FullName.StartsWith("System")))
                    {
                        LoadReferencedAssembly(Assembly.Load(name));
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
        }

        public static void LoadReferencedAssemblies()
        {
            //var w = Stopwatch.StartNew();
            try
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("System") == false).ToList();

                loadedAssemblies
                    .SelectMany(x => x.GetReferencedAssemblies().Where(a => a.FullName.StartsWith("System") == false))
                    .Distinct()
                    .Where(y => loadedAssemblies.Any((a) => a.FullName == y.FullName) == false)
                    .ToList()
                    .ForEach(x => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(x)));
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
            //w.Stop();
            //Console.WriteLine("LoadReferencedAssemblies :{0}", w.ElapsedMilliseconds);
        }

        public static void ResolveLoadAssemblies()
        {
            //var w= Stopwatch.StartNew();
            try
            {
                //var loadedAssemblies1 = AppDomain.CurrentDomain.GetAssemblies().ToList();
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a=> a.FullName.StartsWith("System")==false).ToList();
                var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

                var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
                var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
                toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
            //w.Stop();
            //Console.WriteLine("ResolveLoadAssemblies :{0}", w.ElapsedMilliseconds);
        }

        public static Type GetQualifiedType(string fullTypeName, bool enableException = false)
         {
             if (string.IsNullOrEmpty(fullTypeName))
                 return null;
             try
             {
                //var type1 = Type.GetType(fullTypeName);

                //var type2 = 
                //        AppDomain.CurrentDomain.GetAssemblies()
                //                 .Select(a => a.GetType(fullTypeName))
                //                 .FirstOrDefault(t => t != null);

                //var type3 =
                //         AppDomain.CurrentDomain.GetAssemblies()
                //                  .Select(a => a.GetType(fullTypeName))
                //                  .FirstOrDefault();

                Type type = Type.GetType(fullTypeName) ??
                        AppDomain.CurrentDomain.GetAssemblies()
                                 .Select(a => a.GetType(fullTypeName))
                                 .FirstOrDefault(t => t != null);

                if(type==null)
                {
                    LoadReferencedAssemblies();
                    type = Type.GetType(fullTypeName) ??
                        AppDomain.CurrentDomain.GetAssemblies()
                                 .Select(a => a.GetType(fullTypeName))
                                 .FirstOrDefault(t => t != null);
                }

                return type;
                //System.Web.Compilation.BuildManager.GetReferencedAssemblies();


             }
             catch
             {
                 if (enableException)
                     throw new SerializationException("Could not Get Qualified Type for type " + fullTypeName);
                 return null;
             }
         }

         public static Type GetAssemblyQualifiedType(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return null;
            Type type = Type.GetType(fullName);
            if (type == null)
            {
                string targetAssembly = fullName;
                while (type == null && targetAssembly.Length > 0)
                {
                    try
                    {
                        int dotInd = targetAssembly.LastIndexOf('.');
                        targetAssembly = dotInd >= 0 ? targetAssembly.Substring(0, dotInd) : "";
                        if (targetAssembly.Length > 0)
                            type = Type.GetType(fullName + ", " + targetAssembly);
                    }
                    catch { }
                }
            }
            return type;
        }

        public static Type GetGenericBaseType(Type type)
        {
            if (type==null)
                return null;
            Type baseType = type;
            do
            {
                if (baseType.IsGenericType)
                    return baseType;
                if (baseType.BaseType == null)
                    return baseType;
                baseType = baseType.BaseType;

            }
            while (baseType!=null);

            return baseType;
        }
        public static bool IsGenericBaseType(Type type)
        {
            if (type == null)
                return false;
            if (type.IsGenericType)
                return true;
            if (type.BaseType != null)
                return type.BaseType.IsGenericType;

            return false;
        }

        public static bool IsKnownGenericType(Type type)
        {
            return !type.Equals(typeof(object));
        }

        internal static SerialType GetSerialType<T>(Type baseType)
        {
           return GetSerialType(typeof(T), baseType);
        }

        internal static SerialType GetSerialType(Type type, Type baseType)
        {

            if (type == null)
                return SerialType.nullType;

            switch (type.FullName)
            {

                case "System.Boolean": return SerialType.boolType;
                case "System.Byte": return SerialType.byteType;
                case "System.UInt16": return SerialType.uint16Type;
                case "System.UInt32": return SerialType.uint32Type;
                case "System.UInt64": return SerialType.uint64Type;
                case "System.SByte": return SerialType.sbyteType;
                case "System.Int16": return SerialType.int16Type;
                case "System.Int32": return SerialType.int32Type;
                case "System.Int64": return SerialType.int64Type;
                case "System.Char": return SerialType.charType;
                case "System.String": return SerialType.stringType;
                case "System.Single": return SerialType.singleType;
                case "System.Double": return SerialType.doubleType;
                case "System.Decimal": return SerialType.decimalType;
                case "System.DateTime": return SerialType.dateTimeType;
                case "System.TimeSpan": return SerialType.timeSpanType;
                case "System.Byte[]": return SerialType.byteArrayType;
                case "System.Char[]": return SerialType.charArrayType;
                case "System.Guid": return SerialType.guidType;
                case "System.Int16[]": return SerialType.int16ArrayType;
                case "System.Int32[]": return SerialType.int32ArrayType;
                case "System.Int64[]": return SerialType.int64ArrayType;
                case "System.String[]": return SerialType.stringArrayType;
                case "System.Object[]": return SerialType.objectArrayType;
                case "System.Data.DataSet": return SerialType.dataSetType;
                case "System.Data.DataTable": return SerialType.dataTableType;
                case "Nistec.IO.NetStream": return SerialType.netStreamType;
                case "System.Collections.Specialized.StringDictionary": return SerialType.stringDictionary;
                case "System.Collections.Specialized.NameValueCollection": return SerialType.nameValueCollection;
                case "System.Xml.XmlDocument": return SerialType.xmlDocumentType;
                    
                default:
                    if (type.IsEnum)
                        return SerialType.enumType;
                    else if (SerializeTools.IsISerialEntity(type))
                        return SerialType.serialEntityType;
                    else if (SerializeTools.IsISerialContext(type) && baseType == null)
                        return SerialType.serialContextType;
                    else if (SerializeTools.IsStringObjectDictionary(type))
                        return SerialType.dictionaryEntityType;
                    else if (SerializeTools.IsGenericDictionary(type))
                        return SerialType.dictionaryGenericType;
                    else if (SerializeTools.IsGenericList(type))
                        return SerialType.listGenericType;
                    else if (SerializeTools.IsHashtable(type))
                        return SerialType.hashtableType;
                    else if (SerializeTools.IsAssignableFromDictionary(type))
                        return SerialType.dictionaryAssignType;
                    else if (SerializeTools.IsStream(type))
                        return SerialType.streamType;
                    else if (SerializeTools.IsXmlNode(type))
                        return SerialType.xmlNodeType;
                    else if (SerializeTools.IsListKeyValue<string, object>(type))
                        return SerialType.genericKeyValueType;
                    //else if (SerializeTools.IsListKeyValue<string,string>(type))
                    //    return SerialType.genericNameValueType;
                    else if (SerializeTools.IsType(type))
                        return SerialType.typeType;
                    else
                    {
                        //if (SerializeTools.EnableEntitySerialize(type, useStreamerForUnknownType))
                        return SerialType.anyClassType;
                    }
            } // switch

        }
 
        internal static SerialType GetSerializePrimitiveType(Type type)
        {

            if (type == null)
                return SerialType.nullType;

            switch (type.Name)
            {

                case "Boolean": return SerialType.boolType;
                case "Byte": return SerialType.byteType;
                case "UInt16": return SerialType.uint16Type;
                case "UInt32": return SerialType.uint32Type;
                case "UInt64": return SerialType.uint64Type;
                case "SByte": return SerialType.sbyteType;
                case "Int16": return SerialType.int16Type;
                case "Int32": return SerialType.int32Type;
                case "Int64": return SerialType.int64Type;
                case "Char": return SerialType.charType;
                case "String": return SerialType.stringType;
                case "Single": return SerialType.singleType;
                case "Double": return SerialType.doubleType;
                case "Decimal": return SerialType.decimalType;
                case "DateTime": return SerialType.dateTimeType;
                case "TimeSpan": return SerialType.timeSpanType;
                case "Byte[]": return SerialType.byteArrayType;
                case "Char[]": return SerialType.charArrayType;
                case "Guid": return SerialType.guidType;
                case "Int16[]": return SerialType.int16ArrayType;
                case "Int32[]": return SerialType.int32ArrayType;
                case "Int64[]": return SerialType.int64ArrayType;
                case "String[]": return SerialType.stringArrayType;
                case "Object[]": return SerialType.objectArrayType;
                default:
                    if (type.IsEnum)
                        return SerialType.enumType;
                    else if (SerializeTools.IsType(type))
                        return SerialType.typeType;
                    else
                    {
                        return SerialType.otherType;
                    }
            } // switch

        }

        internal static bool IsSimpleType(SerialType t)
        {
            switch (t)
            {
                case SerialType.boolType: 
                case SerialType.byteType: 
                case SerialType.uint16Type: 
                case SerialType.uint32Type: 
                case SerialType.uint64Type: 
                case SerialType.sbyteType: 
                case SerialType.int16Type: 
                case SerialType.int32Type: 
                case SerialType.int64Type: 
                case SerialType.charType: 
                case SerialType.stringType: 
                case SerialType.singleType: 
                case SerialType.doubleType: 
                case SerialType.decimalType: 
                case SerialType.dateTimeType:
                    return true;
                
                default: return false;
            }
        }

        public static IKeyValue CreateGenericIKeyValue(Type genericType, Type keyType, Type valueType)
        {
            Type[] typeArgs = { keyType, valueType };
            Type constructed = genericType.MakeGenericType(typeArgs);
            object o = ActivatorUtil.CreateInstance(constructed);

            return (IKeyValue)o;
        }

        public static IDictionary CreateGenericDictionary(Type keyType, Type valueType)
        {
            Type d1 = typeof(Dictionary<,>);
            Type[] typeArgs = { keyType, valueType };
            Type constructed = d1.MakeGenericType(typeArgs);
            object o = ActivatorUtil.CreateInstance(constructed);

            return (IDictionary)o;
        }

        public static IList CreateGenericList(Type type)
        {
            Type d1 = typeof(List<>);
            Type[] typeArgs = { type };
            Type constructed = d1.MakeGenericType(typeArgs);
            object o = ActivatorUtil.CreateInstance(constructed);
            return (IList)o;
        }

        public static byte[] StreamToBytes(Stream stream)
        {
            
            if (stream is MemoryStream)
            {
                stream.Position = 0;
                return ((MemoryStream)stream).ToArray();
            }
            if (stream is NetStream)
            {
                stream.Position = 0;
                return ((NetStream)stream).ToArray();
            }
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static Stream BytesToStream(byte[] bytes, Type type)
        {
            if (type == typeof(MemoryStream))
            {
                return new MemoryStream(bytes);
            }
            if (type == typeof(NetStream))
            {
                return new NetStream(bytes);
            }

            object stream=ActivatorUtil.CreateInstance(type);

            ((Stream)stream).Write(bytes, 0, bytes.Length);

            return stream as Stream;
        }

        public static void ChangeContextType(NetStream stream, SerialContextType type)
        {
            if (stream == null)
                return;
             stream.Replace((byte)type, 0);
            
            stream.Position = 0;
        }

        public static object[] GetPropertyValue(PropertyInfo p, object o)
        {
            //List<object> output = new List<object>();
            Type t = p.PropertyType;

            int count = -1;

            if (t.GetProperty("Count") != null &&
                t.GetProperty("Count").PropertyType == typeof(System.Int32))
            {
                count = (int)t.GetProperty("Count").GetValue(o, null);
            }
            else
            {
                return null;
            }
            if (count > 0)
            {
                object[] index = new object[count];
                for (int i = 0; i < count; i++)
                {
                    object val = p.GetValue(o, new object[] { i });
                    index[i] = val;
                }
                return index;
            }

            return null;// output.ToArray();
        }


        public static object[] GetPropertyValue(object o)
        {
            List<object> output = new List<object>();
            Type t = o.GetType();
            if (t.GetProperty("Item") != null)
            {
                System.Reflection.PropertyInfo p = t.GetProperty("Item");
                int count = -1;
                if (t.GetProperty("Count") != null &&
                    t.GetProperty("Count").PropertyType == typeof(System.Int32))
                {
                    count = (int)t.GetProperty("Count").GetValue(o, null);
                }
                if (count > 0)
                {
                    object[] index = new object[count];
                    for (int i = 0; i < count; i++)
                    {
                        object val = p.GetValue(o, new object[] { i });
                        output.Add(val);
                    }
                }
            }
            return output.ToArray();
        }

 
        #region Property Info

        public static bool IsVirtual(this PropertyInfo propertyInfo)
        {
            Validation.ArgumentNull(propertyInfo, "propertyInfo");

            MethodInfo m = propertyInfo.GetGetMethod();
            if (m != null && m.IsVirtual)
                return true;

            m = propertyInfo.GetSetMethod();
            if (m != null && m.IsVirtual)
                return true;

            return false;
        }

        public static MethodInfo GetBaseDefinition(this PropertyInfo propertyInfo)
        {
            Validation.ArgumentNull(propertyInfo, "propertyInfo");

            MethodInfo m = propertyInfo.GetGetMethod();
            if (m != null)
                return m.GetBaseDefinition();

            m = propertyInfo.GetSetMethod();
            if (m != null)
                return m.GetBaseDefinition();

            return null;
        }

        public static bool IsPublic(PropertyInfo property)
        {
            if (property.GetGetMethod() != null && property.GetGetMethod().IsPublic)
                return true;
            if (property.GetSetMethod() != null && property.GetSetMethod().IsPublic)
                return true;

            return false;
        }

        public static bool IsIndexedProperty(PropertyInfo property)
        {
            return (property.GetIndexParameters().Length > 0);
        }
        #endregion

        #region Type name

        public static Type GetObjectType(object v)
        {
            return (v != null) ? v.GetType() : null;
        }

        public static string GetTypeName(Type t, FormatterAssemblyStyle assemblyFormat, SerializationBinder binder)
        {
            string fullyQualifiedTypeName;
            if (binder != null)
            {
                string assemblyName, typeName;
                binder.BindToName(t, out assemblyName, out typeName);
                fullyQualifiedTypeName = typeName + (assemblyName == null ? "" : ", " + assemblyName);
            }
            else
            {
                fullyQualifiedTypeName = t.AssemblyQualifiedName;
            }

            switch (assemblyFormat)
            {
                case FormatterAssemblyStyle.Simple:
                    return RemoveAssemblyDetails(fullyQualifiedTypeName);
                case FormatterAssemblyStyle.Full:
                    return fullyQualifiedTypeName;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetTypeName(Type t, bool fullyQualifiedTypeName=true)
        {
            if (fullyQualifiedTypeName)
                return t.AssemblyQualifiedName;
            return t.FullName;
        }
        public static Type GetType(string fullyQualifiedTypeName)
        {
            if (fullyQualifiedTypeName == null)
                return null;
            return Type.GetType(fullyQualifiedTypeName);
        }
        #endregion

        #region Constructor Info
        public static bool HasDefaultConstructor(Type t, bool nonPublic)
        {
            Validation.ArgumentNull(t, "t");

            if (t.IsValueType)
                return true;

            return (GetDefaultConstructor(t, nonPublic) != null);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t)
        {
            return GetDefaultConstructor(t, false);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (nonPublic)
                bindingFlags = bindingFlags | BindingFlags.NonPublic;

            return t.GetConstructors(bindingFlags).SingleOrDefault(c => !c.GetParameters().Any());
        }
        #endregion

        #region Collection
        public static Type GetCollectionItemType(Type type)
        {
            Validation.ArgumentNull(type, "type");
            Type genericListType;

            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (IsGenericDefinition(type, typeof(IEnumerable<>), out genericListType))
            {
                if (genericListType.IsGenericTypeDefinition)
                    throw new Exception(string.Format("Type {0} is not a collection.", type));

                return genericListType.GetGenericArguments()[0];
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return null;
            }

            throw new Exception(string.Format("Type {0} is not a collection.", type));
        }

        public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type keyType, out Type valueType)
        {
            Validation.ArgumentNull(dictionaryType, "type");

            Type genericDictionaryType;
            if (IsGenericDefinition(dictionaryType, typeof(IDictionary<,>), out genericDictionaryType))
            {
                if (genericDictionaryType.IsGenericTypeDefinition)
                    throw new Exception(string.Format("Type {0} is not a dictionary.", dictionaryType));

                Type[] dictionaryGenericArguments = genericDictionaryType.GetGenericArguments();

                keyType = dictionaryGenericArguments[0];
                valueType = dictionaryGenericArguments[1];
                return;
            }
            if (typeof(IDictionary).IsAssignableFrom(dictionaryType))
            {
                keyType = null;
                valueType = null;
                return;
            }

            throw new Exception(string.Format("Type {0} is not a dictionary.", dictionaryType));
        }
        #endregion

        #region Generic Definition

        public static bool IsGenericDefinition(Type type, Type genericInterfaceDefinition)
        {
            if (!type.IsGenericType)
                return false;

            Type t = type.GetGenericTypeDefinition();
            return (t == genericInterfaceDefinition);
        }
 
        public static bool IsGenericDefinition(Type type, Type genericInterfaceDefinition, out Type implementingType)
        {
            Validation.ArgumentNull(type, "type");
            Validation.ArgumentNull(genericInterfaceDefinition, "genericInterfaceDefinition");

            if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
                throw new ArgumentNullException(string.Format("'{0}' is not a generic interface definition.", genericInterfaceDefinition));

            if (type.IsInterface)
            {
                if (type.IsGenericType)
                {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = null;
            return false;
        }

        public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition)
        {
            Type implementingType;
            return InheritsGenericDefinition(type, genericClassDefinition, out implementingType);
        }

        public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition, out Type implementingType)
        {
            Validation.ArgumentNull(type, "type");
            Validation.ArgumentNull(genericClassDefinition, "genericClassDefinition");

            if (!genericClassDefinition.IsClass || !genericClassDefinition.IsGenericTypeDefinition)
                throw new ArgumentNullException(string.Format("'{0}' is not a generic class definition.", genericClassDefinition));

            return InheritsGenericDefinitionInternal(type, genericClassDefinition, out implementingType);
        }

        private static bool InheritsGenericDefinitionInternal(Type currentType, Type genericClassDefinition, out Type implementingType)
        {
            if (currentType.IsGenericType)
            {
                Type currentGenericClassDefinition = currentType.GetGenericTypeDefinition();

                if (genericClassDefinition == currentGenericClassDefinition)
                {
                    implementingType = currentType;
                    return true;
                }
            }

            if (currentType.BaseType == null)
            {
                implementingType = null;
                return false;
            }

            return InheritsGenericDefinitionInternal(currentType.BaseType, genericClassDefinition, out implementingType);
        }
        #endregion
 
        #region Attributes
        public static T GetAttribute<T>(object attributeProvider) where T : Attribute
        {
            return GetAttribute<T>(attributeProvider, true);
        }

        public static T GetAttribute<T>(object attributeProvider, bool inherit) where T : Attribute
        {
            T[] attributes = GetAttributes<T>(attributeProvider, inherit);

            return (attributes != null) ? attributes.SingleOrDefault() : null;
        }

        public static T[] GetAttributes<T>(object attributeProvider, bool inherit) where T : Attribute
        {
            return (T[])GetAttributes(attributeProvider, typeof(T), inherit);
        }

        public static Attribute[] GetAttributes(object attributeProvider, Type attributeType, bool inherit)
        {
            Validation.ArgumentNull(attributeProvider, "attributeProvider");

            object provider = attributeProvider;


            if (provider is Type)
                return (Attribute[])((Type)provider).GetCustomAttributes(attributeType, inherit);

            if (provider is Assembly)
                return Attribute.GetCustomAttributes((Assembly)provider, attributeType);

            if (provider is MemberInfo)
                return Attribute.GetCustomAttributes((MemberInfo)provider, attributeType, inherit);

            if (provider is Module)
                return Attribute.GetCustomAttributes((Module)provider, attributeType, inherit);

            if (provider is ParameterInfo)
                return Attribute.GetCustomAttributes((ParameterInfo)provider, attributeType, inherit);

            return (Attribute[])((ICustomAttributeProvider)attributeProvider).GetCustomAttributes(attributeType, inherit);

            //throw new Exception("Cannot get attributes from '{0}'.".FormatWith(CultureInfo.InvariantCulture, provider));
        }
        #endregion

        #region Assembly

        private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
        {
            StringBuilder sb = new StringBuilder();

            // loop through the type name and filter out qualified assembly details from nested type names
            bool writingAssemblyName = false;
            bool skippingAssemblyDetails = false;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
            {
                char current = fullyQualifiedTypeName[i];
                switch (current)
                {
                    case '[':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        sb.Append(current);
                        break;
                    case ']':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        sb.Append(current);
                        break;
                    case ',':
                        if (!writingAssemblyName)
                        {
                            writingAssemblyName = true;
                            sb.Append(current);
                        }
                        else
                        {
                            skippingAssemblyDetails = true;
                        }
                        break;
                    default:
                        if (!skippingAssemblyDetails)
                            sb.Append(current);
                        break;
                }
            }

            return sb.ToString();
        }

        public static void SplitFullyQualifiedTypeName(string fullyQualifiedTypeName, out string typeName, out string assemblyName)
        {
            int? assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);

            if (assemblyDelimiterIndex != null)
            {
                typeName = fullyQualifiedTypeName.Substring(0, assemblyDelimiterIndex.Value).Trim();
                assemblyName = fullyQualifiedTypeName.Substring(assemblyDelimiterIndex.Value + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.Value - 1).Trim();
            }
            else
            {
                typeName = fullyQualifiedTypeName;
                assemblyName = null;
            }
        }

        private static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
        {
            // we need to get the first comma following all surrounded in brackets because of generic types
            // e.g. System.Collections.Generic.Dictionary`2[[System.String, mscorlib,Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            int scope = 0;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
            {
                char current = fullyQualifiedTypeName[i];
                switch (current)
                {
                    case '[':
                        scope++;
                        break;
                    case ']':
                        scope--;
                        break;
                    case ',':
                        if (scope == 0)
                            return i;
                        break;
                }
            }

            return null;
        }

        #endregion

        #region MemberInfo

        public static bool IsMethodOverridden(Type currentType, Type methodDeclaringType, string method)
        {
            bool isMethodOverriden = currentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(info =>
                    info.Name == method &&
                        // check that the method overrides the original on DynamicObjectProxy
                    info.DeclaringType != methodDeclaringType
                    && info.GetBaseDefinition().DeclaringType == methodDeclaringType
                );

            return isMethodOverriden;
        }

        private static bool IsOverridenGenericMember(MemberInfo memberInfo, BindingFlags bindingAttr)
        {
            if (memberInfo.MemberType != MemberTypes.Property)
                return false;

            PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
            if (!IsVirtual(propertyInfo))
                return false;

            Type declaringType = propertyInfo.DeclaringType;
            if (!declaringType.IsGenericType)
                return false;
            Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
            if (genericTypeDefinition == null)
                return false;
            MemberInfo[] members = genericTypeDefinition.GetMember(propertyInfo.Name, bindingAttr);
            if (members.Length == 0)
                return false;
            Type memberUnderlyingType = GetMemberUnderlyingType(members[0]);
            if (!memberUnderlyingType.IsGenericParameter)
                return false;

            return true;
        }

        public static Type GetMemberUnderlyingType(MemberInfo member)
        {
            Validation.ArgumentNull(member, "member");

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                default:
                    throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo, EventInfo or MethodInfo", "member");
            }
        }
        public static MethodInfo GetMethodInfo(string typeClassName, string method)
        {
            if (typeClassName == null || method == null)
            {
                throw new ArgumentNullException("GetMethodInfo.typeClassName|method");
            }
            Type type = Type.GetType(typeClassName);
            MethodInfo methodinfo = type.GetMethod(method);
            return methodinfo;
        }

        public static Type GetMethodReturnType(string typeClassName, string method)
        {
            MethodInfo methodinfo = GetMethodInfo(typeClassName, method);
            return methodinfo.ReturnType;
        }

        public static Type GetElementType(Type type)
        {
            if(type==null)
            {
                throw new ArgumentNullException("GetElementType.type");
            }
            //Type generic = type.GetGenericTypeDefinition();

            if (type.IsGenericType)
            {
                var types = type.GetGenericArguments();
                if (types == null)
                    return type;
                if (types.Length > 1)//dictionary<string,object>
                    return types[1];
                if (types.Length > 0)
                    return types[0];
                else
                    return type;
            }
            else if (type.IsArray)
            {
                return type.GetElementType();
            }
            else
                return type;
        }
        public static bool IsIndexedProperty(MemberInfo member)
        {
            Validation.ArgumentNull(member, "member");

            PropertyInfo propertyInfo = member as PropertyInfo;

            if (propertyInfo != null)
                return IsIndexedProperty(propertyInfo);
            else
                return false;
        }

        public static object GetMemberValue(MemberInfo member, object target)
        {
            Validation.ArgumentNull(member, "member");
            Validation.ArgumentNull(target, "target");

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(target);
                case MemberTypes.Property:
                    try
                    {
                        return ((PropertyInfo)member).GetValue(target, null);
                    }
                    catch (TargetParameterCountException e)
                    {
                        throw new ArgumentException(string.Format("MemberInfo '{0}' has index parameters", member.Name), e);
                    }
                default:
                    throw new ArgumentException(string.Format("MemberInfo '{0}' is not of type FieldInfo or PropertyInfo", member.Name), "member");
            }
        }

        public static void SetMemberValue(MemberInfo member, object target, object value)
        {
            Validation.ArgumentNull(member, "member");
            Validation.ArgumentNull(target, "target");

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)member).SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)member).SetValue(target, value, null);
                    break;
                default:
                    throw new ArgumentException(string.Format("MemberInfo '{0}' must be of type FieldInfo or PropertyInfo", member.Name), "member");
            }
        }

        public static bool CanReadMemberValue(MemberInfo member, bool nonPublic)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)member;

                    if (nonPublic)
                        return true;
                    else if (fieldInfo.IsPublic)
                        return true;
                    return false;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanRead)
                        return false;
                    if (nonPublic)
                        return true;
                    return (propertyInfo.GetGetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }

        public static bool CanSetMemberValue(MemberInfo member, bool nonPublic, bool canSetReadOnly)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)member;

                    if (fieldInfo.IsInitOnly && !canSetReadOnly)
                        return false;
                    if (nonPublic)
                        return true;
                    else if (fieldInfo.IsPublic)
                        return true;
                    return false;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanWrite)
                        return false;
                    if (nonPublic)
                        return true;
                    return (propertyInfo.GetSetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }
         
        public static MemberInfo GetMemberInfoFromType(Type targetType, MemberInfo memberInfo)
        {
            const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)memberInfo;

                    Type[] types = propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray();

                    return targetType.GetProperty(propertyInfo.Name, bindingAttr, null, propertyInfo.PropertyType, types, null);
                default:
                    return targetType.GetMember(memberInfo.Name, memberInfo.MemberType, bindingAttr).SingleOrDefault();
            }
        }

        public static IEnumerable<FieldInfo> GetFields(Type targetType, BindingFlags bindingAttr)
        {
            Validation.ArgumentNull(targetType, "targetType");

            List<MemberInfo> fieldInfos = new List<MemberInfo>(targetType.GetFields(bindingAttr));

            // Type.GetFields doesn't return inherited private fields
            // manually find private fields from base class
            //GetChildPrivateFields(fieldInfos, targetType, bindingAttr);

            if ((bindingAttr & BindingFlags.NonPublic) != 0)
            {
                // modify flags to not search for public fields
                BindingFlags nonPublicBindingAttr = bindingAttr.RemoveFlag(BindingFlags.Public);

                while ((targetType = targetType.BaseType) != null)
                {
                    // filter out protected fields
                    IEnumerable<MemberInfo> childPrivateFields =
                        targetType.GetFields(nonPublicBindingAttr).Where(f => f.IsPrivate).Cast<MemberInfo>();
                    foreach (var mi in childPrivateFields)
                    {
                        fieldInfos.Add(mi);
                    }

                    //initialFields.AddRange(childPrivateFields);
                }
            }

            return fieldInfos.Cast<FieldInfo>();
        }

        public static BindingFlags RemoveFlag(this BindingFlags bindingAttr, BindingFlags flag)
        {
            return ((bindingAttr & flag) == flag)
                ? bindingAttr ^ flag
                : bindingAttr;
        }

        #endregion
        public static void MapToDictionary(
    Dictionary<string, object> dictionary, object source, string name, bool allowReadOnly = false)
        {
            Type sourceType = source.GetType();
            if (sourceType.IsEnum)
            {
                dictionary[sourceType.Name] = (int)Convert.ToInt32(source);
                return;
            }
            else if (SerializeTools.IsSimple(sourceType))
            {
                dictionary[sourceType.Name] = source;
                return;
            }
            else if (SerializeTools.IsStream(sourceType))
            {
                dictionary[sourceType.Name] = source;
                return;
            }
            else if (SerializeTools.IsType(sourceType))
            {
                dictionary[sourceType.Name] = sourceType.FullName;
                return;
            }

            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance, allowReadOnly);
            foreach (var p in properties)
            {

                if (!allowReadOnly && !p.CanWrite) { continue; }

                if (p.CanRead)
                {
                    var key = !string.IsNullOrWhiteSpace(name) ? name + "." + p.Name : p.Name;
                    object value = p.GetValue(source, null);
                    if (value == null)
                        continue;
                    Type valueType = value.GetType();

                    if (valueType.IsEnum)
                    {
                        dictionary[key] = (int)Convert.ToInt32(value);
                    }
                    else if (SerializeTools.IsSimple(valueType))
                    {
                        dictionary[key] = value;
                    }
                    else if (SerializeTools.IsStream(valueType))
                    {
                        dictionary[key] = value;
                    }
                    else if (SerializeTools.IsType(valueType))
                    {
                        dictionary[key] = valueType.FullName;
                    }
                    else if (SerializeTools.IsDataTable(valueType) || SerializeTools.IsDataSet(valueType))
                    {
                        dictionary[key] = value;
                    }
                    else if (typeof(IKeyValue).IsAssignableFrom(valueType))
                    {
                        dictionary[key] = ((IKeyValue)value).Dictionary();
                    }
                    else if (typeof(IKeyValue<object>).IsAssignableFrom(valueType))
                    {
                        dictionary[key] = ((IKeyValue<object>)value).ToDictionary();
                    }
                    else if (SerializeTools.IsAssignableFromDictionary(valueType, true))
                    {
                        dictionary[key] = value;
                    }
                    else if (SerializeTools.IsEnumerable(valueType))
                    {
                        var i = 0;
                        var subdictionary = new Dictionary<string, object>();
                        foreach (object o in (IEnumerable)value)
                        {
                            MapToDictionary(subdictionary, o, key + "[" + i + "]");
                            i++;
                        }
                        dictionary[key] = subdictionary;
                    }
                    else
                    {
                        var subdictionary = new Dictionary<string, object>();
                        MapToDictionary(subdictionary, value, key, allowReadOnly);
                        dictionary[key] = subdictionary;
                    }
                }
            }
        }

    }

} // namespace
