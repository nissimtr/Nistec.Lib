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
using Nistec.Generic;
using Nistec.Serialization;
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

namespace Nistec.Runtime
{
   
    public static class ActivatorExtension
    {
        public static object CreateInstance(this Type type)
        {
            return ActivatorUtil.Get.CreateInstanceInternal(type);
        }
        //public static PropertyInfo[] GetProperties(this Type type, bool enableReadOnly, params Type[] attributesExclude)
        //{
        //    return ActivatorUtil.Get.GetProperties(type, enableReadOnly, false, attributesExclude);
        //}


        public static PropertyInfo[] GetProperties(this Type type, bool isPublic)
        {
            return type.GetProperties(isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Instance)
                .Where(p => ((p.PropertyType.IsPublic && isPublic) &&
                (!p.IsDefined(typeof(NoSerializeAttribute), true) && !p.IsDefined(typeof(NonSerializedAttribute), true)) && 
                (p.CanWrite || p.IsDefined(typeof(SerializeAttribute), false)) && 
                p.CanRead)).ToArray();
        }
        public static PropertyInfo[] GetProperties(this Type type, bool isPublic, bool enableReadOnly)
        {
            return type.GetProperties(isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Instance)
                .Where(p => ((p.PropertyType.IsPublic && isPublic) &&
                (!p.IsDefined(typeof(NoSerializeAttribute), true) && !p.IsDefined(typeof(NonSerializedAttribute), true)) &&
                (enableReadOnly || p.CanWrite || p.IsDefined(typeof(SerializeAttribute), false)) &&
                p.CanRead)).ToArray();
        }
        public static PropertyInfo[] GetProperties(this Type type, BindingFlags flags, bool enableReadOnly)
        {

            return type.GetProperties(flags).Where(p => (
                (!p.IsDefined(typeof(NoSerializeAttribute), true) && !p.IsDefined(typeof(NonSerializedAttribute), true)) &&
                (enableReadOnly || p.CanWrite || p.IsDefined(typeof(SerializeAttribute), false)) 
                && p.CanRead)).ToArray();
        }

        //public static PropertyInfo[] GetProperties(this Type type, BindingFlags flags, bool enableReadOnly, params Type[] attributesExclude)
        //{
        //    return ActivatorUtil.Get.GetProperties(type, flags, enableReadOnly, type.FullName, false, attributesExclude);
        //}
        //public static PropertyInfo[] GetProperties(this Type type, bool isPublic, bool enableReadOnly=false, params Type[] attributesExclude)
        //{
        //    return ActivatorUtil.Get.GetProperties(type, isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Instance, enableReadOnly, type.FullName, false, attributesExclude);
        //}

         public static FieldInfo[] GetFields(Type type, bool isPublic)
        {
            return ActivatorUtil.Get.GetFields(type, isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Instance, type.FullName, false);
        }
    }

    public class ActivatorUtil
    {
        #region members
        private static readonly ActivatorUtil instance = new ActivatorUtil();

        // Explicit static constructor to tell C# compiler
        // not to mark type as before field init
        static ActivatorUtil()
        {
        }
        private ActivatorUtil()
        {
        }
        public static ActivatorUtil Get { get { return instance; } }

        internal delegate object GenericSetterDeligate(object target, object value);
        internal delegate object GenericGetterDeligate(object obj);
        private delegate object CreateObjectDeligate();

        private ConcurrentDictionary<Type, CreateObjectDeligate> _ctorCache = new ConcurrentDictionary<Type, CreateObjectDeligate>();
        private ConcurrentDictionary<string, Dictionary<string, FieldInfo>> _fieldCache = new ConcurrentDictionary<string, Dictionary<string, FieldInfo>>();
        private ConcurrentDictionary<string, Dictionary<string, PropertyInfo>> _propertyCache = new ConcurrentDictionary<string, Dictionary<string, PropertyInfo>>();
        #endregion

        public static object CreateInstance(Type objtype)
        {
            return ActivatorUtil.Get.CreateInstanceInternal(objtype);
        }
        public static T CreateInstance<T>()
        {
            return (T)ActivatorUtil.Get.CreateInstanceInternal(typeof(T));
        }

        public static BindingFlags GetFlags(bool isPublic)
        {
            return isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Instance;
        }

        #region properties/fields
        public object CreateInstanceInternal(Type objtype)
        {
            try
            {
                CreateObjectDeligate oDeligate = null;
                if (_ctorCache.TryGetValue(objtype, out oDeligate))
                {
                    return oDeligate();
                }
                else
                {
                    if (objtype.IsClass)
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", objtype, null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
                        ilGen.Emit(OpCodes.Ret);
                        oDeligate = (CreateObjectDeligate)dynMethod.CreateDelegate(typeof(CreateObjectDeligate));
                        _ctorCache.TryAdd(objtype, oDeligate);
                    }
                    else // structs
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_", typeof(object), null);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        var lv = ilGen.DeclareLocal(objtype);
                        ilGen.Emit(OpCodes.Ldloca_S, lv);
                        ilGen.Emit(OpCodes.Initobj, objtype);
                        ilGen.Emit(OpCodes.Ldloc_0);
                        ilGen.Emit(OpCodes.Box, objtype);
                        ilGen.Emit(OpCodes.Ret);
                        oDeligate = (CreateObjectDeligate)dynMethod.CreateDelegate(typeof(CreateObjectDeligate));
                        _ctorCache.TryAdd(objtype, oDeligate);
                    }
                    return oDeligate();
                }
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format("Failed to create instance for type '{0}' from assembly '{1}'",
                    objtype.FullName, objtype.AssemblyQualifiedName), exc);
            }
        }

        //public PropertyInfo[] GetProperties(Type type, bool enableReadOnly=false, bool ignoreCase= false, Type[] attributesExclude=null)
        //{
        //    return GetProperties(type, BindingFlags.Public | BindingFlags.Instance, enableReadOnly, type.FullName, ignoreCase, attributesExclude);
        //}

        //public PropertyInfo[] GetProperties(Type type, BindingFlags flags, bool enableReadOnly, string typename, bool ignoreCase, Type[] attributesExclude)
        //{

        //    Dictionary<string, PropertyInfo> td = null;
        //    if (_propertyCache.TryGetValue(typename, out td))
        //    {
        //        return td.Values.ToArray();
        //    }
        //    else
        //    {
        //        td = new Dictionary<string, PropertyInfo>();
        //        PropertyInfo[] pr = type.GetProperties(flags);// BindingFlags.Public | BindingFlags.Instance);
        //        bool hasAttributes = attributesExclude != null && attributesExclude.Length > 0;

        //        foreach (PropertyInfo p in pr)
        //        {
        //            if(hasAttributes)
        //            {
        //                foreach(var attr in attributesExclude)
        //                {
        //                    if (p.IsDefined(attr, false))
        //                        continue;
        //                }
        //            }
        //            if ((!enableReadOnly && !p.CanWrite) || !p.CanRead)
        //                continue;
        //            if (ignoreCase)
        //                td.Add(p.Name.ToLower(), p);
        //            else
        //                td.Add(p.Name, p);
        //        }
        //        _propertyCache.TryAdd(typename, td);
        //        return td.Values.ToArray();
        //    }
        //}

        public PropertyInfo[] GetProperties(Type type, BindingFlags flags, string typename, bool ignoreCase)
        {

            Dictionary<string, PropertyInfo> td = null;
            if (_propertyCache.TryGetValue(typename, out td))
            {
                return td.Values.ToArray();
            }
            else
            {
                td = new Dictionary<string, PropertyInfo>();
                PropertyInfo[] pr = type.GetProperties(flags);// BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo p in pr)
                {
                    if (ignoreCase)
                        td.Add(p.Name.ToLower(), p);
                    else
                        td.Add(p.Name, p);
                }
                _propertyCache.TryAdd(typename, td);
                return td.Values.ToArray();
            }
        }
        public PropertyInfo[] GetProperties(Type type, bool isPublic, string typename, bool ignoreCase)
        {

            Dictionary<string, PropertyInfo> td = null;
            if (_propertyCache.TryGetValue(typename, out td))
            {
                return td.Values.ToArray();
            }
            else
            {
                td = new Dictionary<string, PropertyInfo>();
                PropertyInfo[] pr = type.GetProperties(isPublic);// BindingFlags.Public | BindingFlags.Instance);
                
                foreach (PropertyInfo p in pr)
                {
                    if (ignoreCase)
                        td.Add(p.Name.ToLower(), p);
                    else
                        td.Add(p.Name, p);
                }
                _propertyCache.TryAdd(typename, td);
                return td.Values.ToArray();
            }
        }

        public FieldInfo[] GetFields(Type type, bool ignoreCase= false)
        {
            return GetFields(type, BindingFlags.Public | BindingFlags.Instance, type.FullName, ignoreCase);
        }
        public FieldInfo[] GetFields(Type type, BindingFlags flags, string typename, bool ignoreCase)
        {
            Dictionary<string, FieldInfo> td = null;
            if (_fieldCache.TryGetValue(typename, out td))
            {
                return td.Values.ToArray();
            }
            else
            {
                td = new Dictionary<string, FieldInfo>();
                FieldInfo[] fi = type.GetFields(flags);// BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo f in fi)
                {
                    if (ignoreCase)
                        td.Add(f.Name.ToLower(), f);
                    else
                        td.Add(f.Name, f);
                }

                _fieldCache.TryAdd(typename, td);
                return td.Values.ToArray();
            }
        }
        #endregion

        #region CreateSet/Get
        internal static GenericSetterDeligate CreateSetField(Type type, FieldInfo fieldInfo)
        {
            Type[] arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            DynamicMethod dynamicSet = new DynamicMethod("_", typeof(object), arguments, type);

            ILGenerator il = dynamicSet.GetILGenerator();

            if (!type.IsClass) // structs
            {
                var lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.Emit(OpCodes.Ldarg_1);
                if (fieldInfo.FieldType.IsClass)
                    il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
                else
                    il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                il.Emit(OpCodes.Stfld, fieldInfo);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Box, type);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                if (fieldInfo.FieldType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                il.Emit(OpCodes.Stfld, fieldInfo);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);
            }
            return (GenericSetterDeligate)dynamicSet.CreateDelegate(typeof(GenericSetterDeligate));
        }

        internal static GenericSetterDeligate CreateSetMethod(Type type, PropertyInfo propertyInfo)
        {
            MethodInfo setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
                return null;

            Type[] arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            DynamicMethod fieldsSetter = new DynamicMethod("_", typeof(object), arguments);
            ILGenerator il = fieldsSetter.GetILGenerator();

            if (!type.IsClass) // structs
            {
                var lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.Emit(OpCodes.Ldarg_1);
                if (propertyInfo.PropertyType.IsClass)
                    il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                else
                    il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                il.EmitCall(OpCodes.Call, setMethod, null);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Box, type);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                il.Emit(OpCodes.Ldarg_1);
                if (propertyInfo.PropertyType.IsClass)
                    il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                else
                    il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                il.EmitCall(OpCodes.Callvirt, setMethod, null);
                il.Emit(OpCodes.Ldarg_0);
            }

            il.Emit(OpCodes.Ret);

            return (GenericSetterDeligate)fieldsSetter.CreateDelegate(typeof(GenericSetterDeligate));
        }

        internal static GenericGetterDeligate CreateGetField(Type type, FieldInfo fieldInfo)
        {
            DynamicMethod dynamicGet = new DynamicMethod("_", typeof(object), new Type[] { typeof(object) }, type);

            ILGenerator il = dynamicGet.GetILGenerator();

            if (!type.IsClass) // structs
            {
                var lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.Emit(OpCodes.Ldfld, fieldInfo);
                if (fieldInfo.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldInfo);
                if (fieldInfo.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }

            il.Emit(OpCodes.Ret);

            return (GenericGetterDeligate)dynamicGet.CreateDelegate(typeof(GenericGetterDeligate));
        }

        internal static GenericGetterDeligate CreateGetMethod(Type type, PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
                return null;

            DynamicMethod fieldsGetter = new DynamicMethod("_", typeof(object), new Type[] { typeof(object) }, type);

            ILGenerator il = fieldsGetter.GetILGenerator();

            if (!type.IsClass) // structs
            {
                var lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.EmitCall(OpCodes.Call, getMethod, null);
                if (propertyInfo.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                il.EmitCall(OpCodes.Callvirt, getMethod, null);
                if (propertyInfo.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            il.Emit(OpCodes.Ret);

            return (GenericGetterDeligate)fieldsGetter.CreateDelegate(typeof(GenericGetterDeligate));
        }
        #endregion

        public void ClearCache()
        {
            _ctorCache = new ConcurrentDictionary<Type, CreateObjectDeligate>();
            _fieldCache = new ConcurrentDictionary<string, Dictionary<string, FieldInfo>>();
            _propertyCache = new ConcurrentDictionary<string, Dictionary<string, PropertyInfo>>();

        }

        #region PropertyInfo

        public static object[] GetObjectPropertiesValues(object instance, bool isPublic, bool enableReadOnly)
        {
            var properties = instance.GetType().GetProperties(isPublic, enableReadOnly);
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

        public static NameValueArgs GetObjectPropertiesNameValuesArgs(object instance, bool isPublic, bool enableReadOnly)
        {
            NameValueArgs values = new NameValueArgs();
            //var properties = GetObjectProperties(instance, isPublic);
            var properties = instance.GetType().GetProperties(isPublic , enableReadOnly);
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

        public static GenericNameValue GetObjectPropertiesNameValues(object instance, bool isPublic, bool enableReadOnly)
        {
            GenericNameValue values = new GenericNameValue();
            //var properties = GetObjectProperties(instance, isPublic);
            var properties = instance.GetType().GetProperties(isPublic , enableReadOnly);

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

        public static string GetObjectPropertiesNameValuesString(object instance, bool isPublic, bool enableReadOnly)
        {
            StringBuilder values = new StringBuilder();
            //var properties = GetObjectProperties(instance, isPublic);
            var properties = instance.GetType().GetProperties(isPublic , enableReadOnly);

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

        public static T CreateInstance<T>(object[] values, bool isPublic, bool enableReadOnly)
        {
            T instance = ActivatorUtil.CreateInstance<T>();

            //var properties = GetObjectProperties(instance, true);
            var properties = instance.GetType().GetProperties(isPublic, enableReadOnly);

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


        #endregion

        #region PropertyInfo
        /*
                public static object[] GetObjectPropertiesValues(object instance, bool isPublic, bool enableReadOnly, Type[] attributesExclude)
                {
                    var properties = instance.GetType().GetProperties(isPublic? BindingFlags.Public| BindingFlags.Instance: BindingFlags.Instance, enableReadOnly,attributesExclude);// GetObjectProperties(instance, isPublic);
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

                public static NameValueArgs GetObjectPropertiesNameValuesArgs(object instance, bool isPublic, bool enableReadOnly, Type[] attributesExclude)
                {
                    NameValueArgs values = new NameValueArgs();
                    //var properties = GetObjectProperties(instance, isPublic);
                    var properties = instance.GetType().GetProperties(isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Instance, enableReadOnly, attributesExclude);
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

                public static GenericNameValue GetObjectPropertiesNameValues(object instance, bool isPublic, bool enableReadOnly, Type[] attributesExclude)
                {
                    GenericNameValue values = new GenericNameValue();
                    //var properties = GetObjectProperties(instance, isPublic);
                    var properties = instance.GetType().GetProperties(isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Instance, enableReadOnly, attributesExclude);

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

                public static string GetObjectPropertiesNameValuesString(object instance, bool isPublic, bool enableReadOnly, Type[] attributesExclude)
                {
                    StringBuilder values = new StringBuilder();
                    //var properties = GetObjectProperties(instance, isPublic);
                    var properties = instance.GetType().GetProperties(isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Instance, enableReadOnly, attributesExclude);

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
                public static T CreateInstance<T>(object[] values, bool isPublic, bool enableReadOnly, Type[] attributesExclude)
                {
                    T instance = ActivatorUtil.CreateInstance<T>();

                    //var properties = GetObjectProperties(instance, true);
                    var properties = instance.GetType().GetProperties(isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.Instance, enableReadOnly, attributesExclude);

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
    }
}