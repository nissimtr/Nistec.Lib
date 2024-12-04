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
using Nistec.Runtime;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
#pragma warning disable  CS1591
namespace Nistec.Serialization
{
    public delegate string SerializeFunc(object data);
    public delegate object DeserializeFunc(string data);

    #region internal

    internal struct JsonFields
    {
        public string Name;
        public ActivatorUtil.GenericGetterDeligate Field;
        public bool IsRawJson;
    }

    internal struct TypeInfo
    {
        public Type propertyType;
        public Type elementType;
        public Type changeType;
        public ActivatorUtil.GenericSetterDeligate setterField;
        public ActivatorUtil.GenericGetterDeligate getterField;
        public Type[] GenericTypes;
        public string Name;
        public SerialJsonType serialType;
        public bool CanWrite;

        public bool IsClass;
        public bool IsValueType;
        public bool IsGenericType;
        public bool IsStruct;
    }

    internal enum SerialJsonType
    {
        Int,
        Long,
        String,
        Bool,
        DateTime,
        Enum,
        Guid,
        Array,
        ByteArray,
        Dictionary,
        StringKeyDictionary,
        NameValue,
        StringDictionary,
        Hashtable,
        DataSet,
        DataTable,
        Custom,
        Unknown,
    }
    #endregion

    internal class JsonActivator
    {

        private static readonly JsonActivator instance = new JsonActivator();

        static JsonActivator()
        {
        }
        private JsonActivator()
        {
        }
        public static JsonActivator Get { get { return instance; } }


        private ConcurrentDictionary<Type, string> _typeNameCache = new ConcurrentDictionary<Type, string>();
        private ConcurrentDictionary<string, Type> _typeCache = new ConcurrentDictionary<string, Type>();
        private ConcurrentDictionary<Type, JsonFields[]> _jsonFieldsCache = new ConcurrentDictionary<Type, JsonFields[]>();
        private ConcurrentDictionary<Type, Type[]> _genericTypesCache = new ConcurrentDictionary<Type, Type[]>();
        private ConcurrentDictionary<Type, Type> _genericTypeDefCache = new ConcurrentDictionary<Type, Type>();
        private ConcurrentDictionary<string, Dictionary<string, TypeInfo>> _typesInfoCache = new ConcurrentDictionary<string, Dictionary<string, TypeInfo>>();

        public Type GetGenericTypeDefinition(Type type)
        {
            Type result = null;
            if (_genericTypeDefCache.TryGetValue(type, out result))
                return result;
            else
            {
                result = type.GetGenericTypeDefinition();
                _genericTypeDefCache.TryAdd(type, result);
                return result;
            }
        }

        public Type[] GetGenericArguments(Type type)
        {

            Type[] result = null;
            if (_genericTypesCache.TryGetValue(type, out result))
                return result;
            else
            {
                result = type.GetGenericArguments();
                _genericTypesCache.TryAdd(type, result);
                return result;
            }
        }

        #region json custom types

        // Serializer custom
        internal ConcurrentDictionary<Type, SerializeFunc> _customSerializer = new ConcurrentDictionary<Type, SerializeFunc>();
        internal ConcurrentDictionary<Type, DeserializeFunc> _customDeserializer = new ConcurrentDictionary<Type, DeserializeFunc>();

        internal object CreateCustom(string v, Type type)
        {
            DeserializeFunc func;
            _customDeserializer.TryGetValue(type, out func);
            return func(v);
        }

        internal void RegisterCustomType(Type type, SerializeFunc serializer, DeserializeFunc deserializer)
        {
            if (type != null && serializer != null && deserializer != null)
            {
                _customSerializer.TryAdd(type, serializer);
                _customDeserializer.TryAdd(type, deserializer);
            }
        }

        internal bool IsTypeRegistered(Type type)
        {
            if (_customSerializer.Count == 0)
                return false;
            SerializeFunc func;
            return _customSerializer.TryGetValue(type, out func);
        }
        #endregion


        #region Property get/set

        public string GetTypeAssemblyName(Type type)
        {
            string val = "";
            if (_typeNameCache.TryGetValue(type, out val))
                return val;
            else
            {
                string s = type.AssemblyQualifiedName;
                _typeNameCache.TryAdd(type, s);
                return s;
            }
        }

        public Type GetTypeFromName(string typename)
        {
            Type val = null;
            if (_typeCache.TryGetValue(typename, out val))
                return val;
            else
            {
                Type type = Type.GetType(typename);
                _typeCache.TryAdd(typename, type);
                return type;
            }
        }

        internal Dictionary<string, TypeInfo> GetTypesInfo(Type type, string typename, bool ignoreCase, bool customType)
        {
            Dictionary<string, TypeInfo> td = null;
            if (_typesInfoCache.TryGetValue(typename, out td))
            {
                return td;
            }
            else
            {
                td = new Dictionary<string, TypeInfo>();
                PropertyInfo[] pr = type.GetProperties(BindingFlags.Public | BindingFlags.Instance,false);
                foreach (PropertyInfo p in pr)
                {
                    TypeInfo ti = GetTypeInfo(p.PropertyType, p.Name, p.CanWrite, customType);
                    ti.setterField = ActivatorUtil.CreateSetMethod(type, p);
                    if (ti.setterField != null)
                        ti.CanWrite = true;
                    ti.getterField = ActivatorUtil.CreateGetMethod(type, p);
                    if (ignoreCase)
                        td.Add(p.Name.ToLower(), ti);
                    else
                        td.Add(p.Name, ti);
                }
                FieldInfo[] fi = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo f in fi)
                {
                    TypeInfo ti = GetTypeInfo(f.FieldType, f.Name, !f.IsInitOnly, customType);
                    //ti.CanWrite = f.IsInitOnly==false;
                    ti.setterField = ActivatorUtil.CreateSetField(type, f);
                    ti.getterField = ActivatorUtil.CreateGetField(type, f);
                    if (ignoreCase)
                        td.Add(f.Name.ToLower(), ti);
                    else
                        td.Add(f.Name, ti);
                }

                _typesInfoCache.TryAdd(typename, td);
                return td;
            }
        }


        private TypeInfo GetTypeInfo(Type type, string name,bool canWrite, bool customType)
        {
            TypeInfo ti = new TypeInfo();
            SerialJsonType serialType = SerialJsonType.Unknown;
            
            if (type == typeof(int) || type == typeof(int?)) serialType = SerialJsonType.Int;
            else if (type == typeof(long) || type == typeof(long?)) serialType = SerialJsonType.Long;
            else if (type == typeof(string)) serialType = SerialJsonType.String;
            else if (type == typeof(bool) || type == typeof(bool?)) serialType = SerialJsonType.Bool;
            else if (type == typeof(DateTime) || type == typeof(DateTime?)) serialType = SerialJsonType.DateTime;
            else if (type.IsEnum) serialType = SerialJsonType.Enum;
            else if (type == typeof(Guid) || type == typeof(Guid?)) serialType = SerialJsonType.Guid;
            else if (type == typeof(StringDictionary)) serialType = SerialJsonType.StringDictionary;
            else if (type == typeof(NameValueCollection)) serialType = SerialJsonType.NameValue;
            else if (type.IsArray)
            {
                ti.elementType = type.GetElementType();
                if (type == typeof(byte[]))
                    serialType = SerialJsonType.ByteArray;
                else
                    serialType = SerialJsonType.Array;
            }
            else if (type.Name.Contains("Dictionary"))
            {
                ti.GenericTypes = JsonActivator.Get.GetGenericArguments(type);// type.GetGenericArguments();
                if (ti.GenericTypes.Length > 0 && ti.GenericTypes[0] == typeof(string))
                    serialType = SerialJsonType.StringKeyDictionary;
                else
                    serialType = SerialJsonType.Dictionary;
            }

            else if (type == typeof(Hashtable)) serialType = SerialJsonType.Hashtable;
            else if (type == typeof(DataSet)) serialType = SerialJsonType.DataSet;
            else if (type == typeof(DataTable)) serialType = SerialJsonType.DataTable;
            else if (customType)
                serialType = SerialJsonType.Custom;

            if (type.IsValueType && !type.IsPrimitive && !type.IsEnum && type != typeof(decimal))
                ti.IsStruct = true;

            ti.IsClass = type.IsClass;
            ti.IsValueType = type.IsValueType;
            if (type.IsGenericType)
            {
                ti.IsGenericType = true;
                ti.elementType = type.GetGenericArguments()[0];
            }

            ti.propertyType = type;
            ti.Name = name;
            ti.changeType = ChangeType(type);
            ti.serialType = serialType;
            ti.CanWrite = canWrite;

            return ti;
        }
 
        public Type ChangeType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                return JsonActivator.Get.GetGenericArguments(type)[0];// type.GetGenericArguments()[0];

            return type;
        }

       
        internal JsonFields[] GetFieldsGetter(Type type, bool showReadOnlyProperties = false, List<Type> ignoreAttributes = null)
        {
            JsonFields[] val = null;

            if (_jsonFieldsCache.TryGetValue(type, out val))
                return val;

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance,false);
            List<JsonFields> fieldsGetter = new List<JsonFields>();
            foreach (PropertyInfo p in props)
            {
                if (!p.CanWrite && showReadOnlyProperties == false)
                    continue;
                if (SerializeTools.HasNoSerializeAttribute(p))
                    continue;


                if (ignoreAttributes != null)
                {
                    bool found = false;
                    foreach (var ignoreAttr in ignoreAttributes)
                    {
                        if (p.IsDefined(ignoreAttr, false))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        continue;
                }
                ActivatorUtil.GenericGetterDeligate g = ActivatorUtil.CreateGetMethod(type, p);
                if (g != null)
                    fieldsGetter.Add(new JsonFields { Field = g, Name = p.Name, IsRawJson = SerializeTools.HasRawJsonAttribute(p)});
            }

            FieldInfo[] fi = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var f in fi)
            {
                if (SerializeTools.HasNoSerializeAttribute(f))
                    continue;
                if (ignoreAttributes != null)
                {
                    bool found = false;
                    foreach (var ignoreAttr in ignoreAttributes)
                    {
                        if (f.IsDefined(ignoreAttr, false))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        continue;
                }

                ActivatorUtil.GenericGetterDeligate g = ActivatorUtil.CreateGetField(type, f);
                if (g != null)
                    fieldsGetter.Add(new JsonFields { Field = g, Name = f.Name, IsRawJson = SerializeTools.HasRawJsonAttribute(f) });
            }
            val = fieldsGetter.ToArray();
            _jsonFieldsCache.TryAdd(type, val);
            return val;
        }

        #endregion

        public void ResetPropertyCache()
        {
            _typesInfoCache = new ConcurrentDictionary<string, Dictionary<string, TypeInfo>>();
        }

        public void ClearCache()
        {
            _typeNameCache = new ConcurrentDictionary<Type, string>();
            _typeCache = new ConcurrentDictionary<string, Type>();
            _jsonFieldsCache = new ConcurrentDictionary<Type, JsonFields[]>();
            _genericTypesCache = new ConcurrentDictionary<Type, Type[]>();
            _genericTypeDefCache = new ConcurrentDictionary<Type, Type>();
            _typesInfoCache = new ConcurrentDictionary<string, Dictionary<string, TypeInfo>>();

        }
    }

}
